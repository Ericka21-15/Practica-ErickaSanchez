# AA Pacientes y Historial Clínico — arquitectura distribuida segura

Tema asignado: **Pacientes** (microservicio 1) e **Historial clínico** (microservicio 2).  
Asignatura: Aplicaciones Distribuidas.  
Repositorio: https://github.com/Ericka21-15/Practica-ErickaSanchez

## Arquitectura

Cliente → **API Gateway (YARP)** →

- `/api/auth` → **OAuthJWT** (emisión de JWT)
- `/api/pacientes` → **Pacientes.Api** (SQL PacientesDB + publica evento RabbitMQ)
- `/api/historialclinico` → **Historial_Clinico.Api** (SQL Historial_ClinicoDB + consume evento)

**RabbitMQ** cola `paciente_creado`: al crear un paciente, Historial Clínico genera un historial inicial.

```
                 +-------------+
                 |   Cliente   |
                 +------+------+
                        | HTTP
                 +------v------+
                 | API Gateway |
                 |    YARP     |
                 +--+---+---+--+
        /api/auth |   |   | /api/pacientes
                  |   |   +--> Pacientes.Api ----publish----> RabbitMQ
                  |   |                                         |
                  |   +--> Historial_Clinico.Api <---consume----+
                  |
                  +--> OAuthJWT (login + JWT)
```

OAuthJWT es un servicio **independiente**. Los microservicios de negocio **no emiten** tokens; solo los validan (Issuer `OAuthJWT`, Audience `ServiciosPacientes`).

## Servicios

| Servicio | Función |
| --- | --- |
| OAuthJWT | Login y generación de JWT (`Issuer`, `Audience`, `Key`, expiración) |
| Pacientes.Api | CRUD pacientes; publica `paciente_creado` |
| Historial_Clinico.Api | CRUD historial; BackgroundService consumidor RabbitMQ |
| ApiGateway | Reverse proxy YARP |
| RabbitMQ | Eventos entre microservicios |
| SQL Server | PacientesDB e Historial_ClinicoDB |

## Entregables obligatorios

| N.º | Entregable | Ubicación / Archivo | Descripción |
| :---: | :--- | :--- | :--- |
| **1** | **Enlace de GitHub** | [Repositorio en GitHub](https://github.com/Ericka21-15/Practica-ErickaSanchez) | Repositorio completo con los 5 servicios, Docker Compose, SQL y configs. |
| **2** | **README.md** | `README.md` | Arquitectura, Docker Compose, pruebas JWT, URLs de Azure y eliminación. |
| **3** | **Scripts de base de datos** | `sql/init.sql`, `sql/azure-pacientes.sql`, `sql/azure-historial.sql`, `sql/azure-init.sql` | Scripts DDL y datos de prueba para SQL Server local y Azure SQL sin secretos. |
| **4** | **Credenciales de ejemplo** | `CLAVES_AZURE_EJEMPLO.txt` | Estructura de credenciales y placeholders según políticas de seguridad. |
| **5** | **Memoria de comandos de Azure** | `MEMORIA_COMANDOS_AZURE.txt` | Secuencia ordenada de comandos de Azure CLI (Login, RG, ACR, SQL, ACA, pruebas, borrado). |


## Requisitos locales

- Docker Desktop
- (Opcional) .NET 10 SDK si se ejecuta sin contenedores

## Ejecución con Docker Compose

En la raíz del repositorio:

```bash
docker compose up --build
```

Puertos:

- API Gateway: http://localhost:5003
- Pacientes: http://localhost:5001/swagger
- Historial clínico: http://localhost:5002/swagger
- OAuthJWT: http://localhost:5004/swagger
- RabbitMQ Management: http://localhost:15672 (`admin` / `admin123`)
- SQL Server: localhost:1433

Detener:

```bash
docker compose down
```

## Obtener un token JWT y usarlo

Usuarios de demostración (ver también `CLAVES_AZURE_EJEMPLO.txt`):

- Administrador: `admin` / `Admin1234!`
- Usuario: `usuario` / `Usuario1234!`

Login a través del gateway:

```bash
curl -X POST http://localhost:5003/api/Auth/Login ^
  -H "Content-Type: application/json" ^
  -d "{\"usuario\":\"admin\",\"password\":\"Admin1234!\"}"
```

Petición **sin token** (debe devolver 401):

```bash
curl -i http://localhost:5003/api/pacientes
```

Petición **con token** (autorizada):

```bash
curl http://localhost:5003/api/pacientes ^
  -H "Authorization: Bearer PEGAR_TOKEN_AQUI"
```

Crear paciente (dispara RabbitMQ → historial automático):

```bash
curl -X POST http://localhost:5003/api/pacientes ^
  -H "Authorization: Bearer PEGAR_TOKEN_AQUI" ^
  -H "Content-Type: application/json" ^
  -d "{\"cedula_pac\":\"0102030405\",\"nombre_pac\":\"Ana\",\"apellido_pac\":\"Lopez\",\"direccion_pac\":\"Quito\"}"
```

Luego:

```bash
curl http://localhost:5003/api/HistorialClinico ^
  -H "Authorization: Bearer PEGAR_TOKEN_AQUI"
```

PUT/DELETE de pacientes e historial requieren rol **Administrador**.

## Endpoints principales

| Método | Ruta (vía Gateway :5003) | Auth |
| --- | --- | --- |
| POST | `/api/Auth/Login` | No |
| GET | `/health` | No |
| GET/POST | `/api/pacientes` | JWT |
| GET/PUT/DELETE | `/api/pacientes/{id}` | JWT (PUT/DELETE: Administrador) |
| GET | `/api/HistorialClinico` | JWT |
| POST/PUT/DELETE | `/api/HistorialClinico` | JWT (escritura: Administrador) |

Swagger de cada API también acepta Bearer.

## Scripts SQL

- Local / Compose: `sql/init.sql`
- Azure PacientesDB: `sql/azure-pacientes.sql`
- Azure Historial_ClinicoDB: `sql/azure-historial.sql`
- Índice Azure: `sql/azure-init.sql`

## Servicios desplegados en Azure

Los servicios se encuentran desplegados y funcionales en **Microsoft Azure (Azure Container Apps + Azure SQL)**. Permanecerán activos hasta el **13/09/2026**:

- **API Gateway (Punto de entrada principal)**: https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io
- **Health check Gateway**: https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/health
- **Login JWT (vía Gateway)**: `POST https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/Auth/Login`
- **Swagger OAuthJWT (Servicio independiente)**: https://oauthjwt.proudsky-9da8e591.eastus.azurecontainerapps.io/swagger
- **Swagger Pacientes**: https://pacientes.proudsky-9da8e591.eastus.azurecontainerapps.io/swagger
- **Swagger Historial Clínico**: https://historialclinico.proudsky-9da8e591.eastus.azurecontainerapps.io/swagger
- **RabbitMQ (Broker AMQP interno)**: Host `rabbitmq`, Puerto `5672` (cola `paciente_creado`)

Memoria técnica de comandos utilizados: [`MEMORIA_COMANDOS_AZURE.txt`](./MEMORIA_COMANDOS_AZURE.txt).
Guía para grabación del video: [`GUIA_GRABACION_VIDEO.md`](./GUIA_GRABACION_VIDEO.md).


## Eliminar recursos de Azure (después de la revisión)

```powershell
az group delete --name rg-aa-pacientes --yes --no-wait
```

No deje recursos activos: en Pay-As-You-Go Azure puede generar cargos aunque no haya tráfico.
