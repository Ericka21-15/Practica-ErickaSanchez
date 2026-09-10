# Script de despliegue AA Pacientes.
# NO ponga secretos en el repositorio. Defina las variables en su terminal antes de ejecutar.
# Ejemplo: $env:SQL_ADMIN_PASSWORD = "..."
#
# Requisitos: Azure CLI, Docker (solo si no usa az acr build), sesión az login.

param(
    [string]$ResourceGroup = "rg-aa-pacientes",
    [string]$Location = "eastus",
    [Parameter(Mandatory = $true)][string]$AcrName,
    [Parameter(Mandatory = $true)][string]$SqlServerName,
    [Parameter(Mandatory = $true)][string]$SqlAdminUser,
    [Parameter(Mandatory = $true)][string]$SqlAdminPassword,
    [Parameter(Mandatory = $true)][string]$JwtKey,
    [string]$RabbitUser = "admin",
    [Parameter(Mandatory = $true)][string]$RabbitPassword,
    [string]$AuthAdminUser = "admin",
    [Parameter(Mandatory = $true)][string]$AuthAdminPassword,
    [string]$AuthUser = "usuario",
    [Parameter(Mandatory = $true)][string]$AuthUserPassword
)

$ErrorActionPreference = "Stop"

az group create --name $ResourceGroup --location $Location

az acr create --resource-group $ResourceGroup --name $AcrName --sku Basic --admin-enabled true
az acr login --name $AcrName

az acr build --registry $AcrName --image oauthjwt:v1 --file OAuthJWT/OAuthJWT/Dockerfile OAuthJWT/OAuthJWT
az acr build --registry $AcrName --image pacientes:v1 --file Pacientes/Pacientes.Api/Dockerfile Pacientes/Pacientes.Api
az acr build --registry $AcrName --image historialclinico:v1 --file Historial_Clinico/Historial_Clinico.Api/Dockerfile Historial_Clinico/Historial_Clinico.Api
az acr build --registry $AcrName --image apigateway:v1 --file ApiGateway/Dockerfile ApiGateway

az sql server create --name $SqlServerName --resource-group $ResourceGroup --location $Location --admin-user $SqlAdminUser --admin-password $SqlAdminPassword
az sql server firewall-rule create --resource-group $ResourceGroup --server $SqlServerName --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
az sql db create --resource-group $ResourceGroup --server $SqlServerName --name PacientesDB --service-objective Basic --backup-storage-redundancy Local
az sql db create --resource-group $ResourceGroup --server $SqlServerName --name Historial_ClinicoDB --service-objective Basic --backup-storage-redundancy Local

az containerapp env create --name env-aa-pacientes --resource-group $ResourceGroup --location $Location

$acrLoginServer = "$AcrName.azurecr.io"
$acrUser = az acr credential show --name $AcrName --query username -o tsv
$acrPass = az acr credential show --name $AcrName --query "passwords[0].value" -o tsv
$pacientesCs = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=PacientesDB;User ID=$SqlAdminUser;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;"
$historialCs = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=Historial_ClinicoDB;User ID=$SqlAdminUser;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;"

az containerapp create --name rabbitmq --resource-group $ResourceGroup --environment env-aa-pacientes --image rabbitmq:4-management --cpu 0.5 --memory 1Gi --min-replicas 1 --max-replicas 1 --ingress internal --target-port 5672 --exposed-port 5672 --transport tcp --env-vars "RABBITMQ_DEFAULT_USER=$RabbitUser" "RABBITMQ_DEFAULT_PASS=$RabbitPassword"


az containerapp create --name oauthjwt --resource-group $ResourceGroup --environment env-aa-pacientes --image "$acrLoginServer/oauthjwt:v1" --registry-server $acrLoginServer --registry-username $acrUser --registry-password $acrPass --cpu 0.25 --memory 0.5Gi --min-replicas 1 --ingress external --target-port 8080 --env-vars "ASPNETCORE_URLS=http://+:8080" "Jwt__Key=$JwtKey" "Jwt__Issuer=OAuthJWT" "Jwt__Audience=ServiciosPacientes" "Jwt__ExpireMinutes=60" "Auth__AdminUser=$AuthAdminUser" "Auth__AdminPassword=$AuthAdminPassword" "Auth__UserName=$AuthUser" "Auth__UserPassword=$AuthUserPassword"

az containerapp create --name pacientes --resource-group $ResourceGroup --environment env-aa-pacientes --image "$acrLoginServer/pacientes:v1" --registry-server $acrLoginServer --registry-username $acrUser --registry-password $acrPass --cpu 0.25 --memory 0.5Gi --min-replicas 1 --ingress external --target-port 8080 --env-vars "ASPNETCORE_URLS=http://+:8080" "ConnectionStrings__PacientesConnection=$pacientesCs" "RabbitMQ__HostName=rabbitmq" "RabbitMQ__Port=5672" "RabbitMQ__UserName=$RabbitUser" "RabbitMQ__Password=$RabbitPassword" "RabbitMQ__QueueName=paciente_creado" "Jwt__Key=$JwtKey" "Jwt__Issuer=OAuthJWT" "Jwt__Audience=ServiciosPacientes"

az containerapp create --name historialclinico --resource-group $ResourceGroup --environment env-aa-pacientes --image "$acrLoginServer/historialclinico:v1" --registry-server $acrLoginServer --registry-username $acrUser --registry-password $acrPass --cpu 0.25 --memory 0.5Gi --min-replicas 1 --ingress external --target-port 8080 --env-vars "ASPNETCORE_URLS=http://+:8080" "ConnectionStrings__HistorialClinicoConnection=$historialCs" "RabbitMQ__HostName=rabbitmq" "RabbitMQ__Port=5672" "RabbitMQ__UserName=$RabbitUser" "RabbitMQ__Password=$RabbitPassword" "RabbitMQ__QueueName=paciente_creado" "Jwt__Key=$JwtKey" "Jwt__Issuer=OAuthJWT" "Jwt__Audience=ServiciosPacientes"

az containerapp create --name apigateway --resource-group $ResourceGroup --environment env-aa-pacientes --image "$acrLoginServer/apigateway:v1" --registry-server $acrLoginServer --registry-username $acrUser --registry-password $acrPass --cpu 0.25 --memory 0.5Gi --min-replicas 1 --ingress external --target-port 8080 --env-vars "ASPNETCORE_URLS=http://+:8080" "ReverseProxy__Clusters__oauthJwtCluster__Destinations__oauthJwtDestination__Address=http://oauthjwt" "ReverseProxy__Clusters__pacientesCluster__Destinations__pacientesDestination__Address=http://pacientes" "ReverseProxy__Clusters__historialClinicoCluster__Destinations__historialClinicoDestination__Address=http://historialclinico"

Write-Host "Despliegue enviado. Consulte FQDN con:"
Write-Host "az containerapp show -g $ResourceGroup -n apigateway --query properties.configuration.ingress.fqdn -o tsv"
