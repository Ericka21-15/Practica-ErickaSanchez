FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["Historial_Clinico.Api.csproj", "./"]

RUN dotnet restore "Historial_Clinico.Api.csproj"

COPY . .

RUN dotnet publish "Historial_Clinico.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Historial_Clinico.Api.dll"]
