# Guía y Guion Paso a Paso para la Grabación del Video (Máximo 5 Minutos)

**Actividad Autónoma:** Arquitectura distribuida segura y despliegue en Azure  
**Tema:** Pacientes (Microservicio 1) e Historial Clínico (Microservicio 2)  
**Asignatura:** Aplicaciones Distribuidas  
**Autora:** Ericka Sánchez  
**Duración máxima:** 5 minutos  

---

## 1. Preparación previa antes de grabar (Checklist)

Tener abiertas y listas las siguientes pestañas o ventanas en pantalla:
1. **Azure Portal**:
   - Grupo de recursos `rg-aa-pacientes`.
   - Lista de recursos visibles: Azure Container Registry (`acraapacientes2026`), Azure SQL Server (`sql-pac-central-7821`), Container Apps Environment (`env-aa-pacientes`), y las 5 Container Apps (`apigateway`, `oauthjwt`, `pacientes`, `historialclinico`, `rabbitmq`).
2. **Navegador web**:
   - Pestaña con Swagger de `oauthjwt`: `https://oauthjwt.proudsky-9da8e591.eastus.azurecontainerapps.io/swagger`
   - Pestaña con Swagger de `pacientes`: `https://pacientes.proudsky-9da8e591.eastus.azurecontainerapps.io/swagger`
   - Pestaña con Swagger de `historialclinico`: `https://historialclinico.proudsky-9da8e591.eastus.azurecontainerapps.io/swagger`
   - Pestaña con `/health` del API Gateway: `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/health`
3. **Postman o Terminal (PowerShell / Windows Terminal)**:
   - Comandos preparados o colección de Postman con:
     - 1. Petición GET `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/pacientes` **sin token** (espera `401 Unauthorized`).
     - 2. Petición POST `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/Auth/Login` para **obtener token JWT** (`admin` / `Admin1234!`).
     - 3. Petición GET `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/pacientes` **con token Bearer** (espera `200 OK` con datos).
     - 4. Petición POST `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/pacientes` para **crear paciente** (dispara evento a RabbitMQ).
     - 5. Petición GET `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/HistorialClinico` **con token Bearer** (demuestra que RabbitMQ creó automáticamente el historial).

---

## 2. Guion Minuto a Minuto (5 Minutos)

### Minuto 0:00 – 0:45 | Introducción y Explicación de la Arquitectura
- **Qué mostrar en pantalla:** Diagrama del README.md o vista general del Azure Portal (`rg-aa-pacientes`).
- **Qué decir:**
  > *"Saludos cordiales, soy Ericka Sánchez y presento la Actividad Autónoma de Aplicaciones Distribuidas correspondiente a una Arquitectura Distribuida Segura desplegada en Azure.*  
  > *El tema asignado corresponde a los microservicios de Pacientes e Historial Clínico.*  
  > *La solución consta de 5 componentes desplegados en Azure Container Apps:*  
  > *1. Servicio independiente OAuthJWT para emisión y autenticación de tokens JWT.*  
  > *2. Microservicio de Pacientes conectado a Azure SQL.*  
  > *3. Microservicio de Historial Clínico conectado a su propia base de datos en Azure SQL.*  
  > *4. RabbitMQ como broker de mensajería para comunicación asíncrona basada en eventos.*  
  > *5. API Gateway con YARP como punto único de entrada para el cliente."*

---

### Minuto 0:45 – 1:30 | Demostración de Recursos en Azure
- **Qué mostrar en pantalla:** Azure Portal en el Resource Group `rg-aa-pacientes`.
- **Qué decir y señalar:**
  > *"Aquí en Azure Portal podemos observar el grupo de recursos `rg-aa-pacientes` en la región East US:*  
  > *- El Azure Container Registry donde se compilaron y almacenan las imágenes Docker de cada servicio.*  
  > *- El servidor de Azure SQL con dos bases de datos independientes: `PacientesDB` e `Historial_ClinicoDB`.*  
  > *- El entorno de Container Apps con las 5 aplicaciones en ejecución: `apigateway`, `oauthjwt`, `pacientes`, `historialclinico` y `rabbitmq`.*  
  > *- Verificamos el endpoint `/health` del API Gateway para confirmar que está activo y respondiendo."*

---

### Minuto 1:30 – 2:30 | Prueba de Seguridad JWT (401 Rechazado vs 200 Autorizado)
- **Qué mostrar en pantalla:** Postman o Terminal PowerShell.
- **Qué hacer:**
  1. Ejecutar GET `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/pacientes` **sin cabecera Authorization**.
  2. Mostrar la respuesta: `401 Unauthorized`.
  3. Ejecutar POST `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/Auth/Login` con credenciales de `admin` (`Admin1234!`).
  4. Mostrar la respuesta con el token JWT, expiración (60 min), y claims de rol.
- **Qué decir:**
  > *"Demostramos el servicio OAuthJWT independiente:*  
  > *Al enviar una petición GET a `/api/pacientes` a través del Gateway sin token, el servicio rechaza la petición inmediatamente con un estado 401 Unauthorized, demostrando que los endpoints están protegidos.*  
  > *A continuación, hacemos login en `/api/Auth/Login` enviando usuario y contraseña. El microservicio OAuthJWT valida las credenciales y genera un token JWT firmado con algoritmo HMAC-SHA256, incluyendo el Issuer `OAuthJWT`, Audience `ServiciosPacientes`, tiempo de expiración y el rol Administrador.*  
  > *Copiamos este token y lo configuramos como Bearer Token."*

---

### Minuto 2:30 – 4:00 | Consumo por API Gateway y Comunicación con RabbitMQ
- **Qué mostrar en pantalla:** Postman / Terminal.
- **Qué hacer:**
  1. Ejecutar GET `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/pacientes` con `Authorization: Bearer <TOKEN>`.
     - Mostrar que devuelve `200 OK` con los pacientes.
  2. Ejecutar POST `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/pacientes` con un nuevo paciente de prueba:
     ```json
     {
       "cedula_pac": "1755554444",
       "nombre_pac": "Roberto",
       "apellido_pac": "Gomez",
       "direccion_pac": "Cuenca"
     }
     ```
     - Mostrar que responde `201 Created` con el nuevo `id_pac`.
  3. Ejecutar GET `https://apigateway.proudsky-9da8e591.eastus.azurecontainerapps.io/api/HistorialClinico` con `Authorization: Bearer <TOKEN>`.
     - Mostrar que el historial clínico para ese nuevo paciente fue generado automáticamente con estado inicial.
- **Qué decir:**
  > *"Ahora realizamos la petición GET a `/api/pacientes` adjuntando el Bearer Token a través del API Gateway. Vemos que responde 200 OK con el listado de pacientes.*  
  > *Para demostrar la comunicación basada en eventos mediante RabbitMQ, creamos un nuevo paciente vía POST.*  
  > *Al crearse el paciente en Pacientes.Api, este publica inmediatamente un evento `paciente_creado` en la cola de RabbitMQ.*  
  > *El microservicio Historial Clínico, que tiene un servicio en segundo plano (BackgroundService) consumiendo dicha cola, recibe el evento y crea automáticamente el historial clínico asociado con estado inicial.*  
  > *Consultamos `/api/HistorialClinico` y verificamos que el historial con el ID del paciente recién creado ya existe en la base de datos de Historial Clínico."*

---

### Minuto 4:00 – 4:45 | Verificación de URLs Públicas y Control de Costos
- **Qué mostrar en pantalla:** Navegador con los Swagger o URLs de los servicios y el archivo README.md.
- **Qué decir:**
  > *"Todos los endpoints son públicos y están accesibles a través de Azure Container Apps como se documenta en el archivo README.md del repositorio de GitHub.*  
  > *De acuerdo con los requerimientos, los recursos permanecerán disponibles y ejecutables hasta el domingo 13 de septiembre de 2026 para su respectiva calificación.*  
  > *Posterior a la revisión, se ejecutará el comando `az group delete --name rg-aa-pacientes --yes --no-wait` para eliminar todos los recursos y evitar costos adicionales en Azure."*

---

### Minuto 4:45 – 5:00 | Conclusión y Cierre
- **Qué decir:**
  > *"Con esto queda evidenciado el correcto funcionamiento de la arquitectura distribuida con microservicios independientes, seguridad con OAuth/JWT, mensajería asíncrona con RabbitMQ, enrutamiento mediante API Gateway y despliegue exitoso en Microsoft Azure. Muchas gracias."*
