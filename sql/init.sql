IF DB_ID(N'PacientesDB') IS NULL
BEGIN
    CREATE DATABASE PacientesDB;
END
GO

IF DB_ID(N'Historial_ClinicoDB') IS NULL
BEGIN
    CREATE DATABASE Historial_ClinicoDB;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'usuario_pacientes')
BEGIN
    CREATE LOGIN usuario_pacientes WITH PASSWORD = N'1234', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'usuario_historial')
BEGIN
    CREATE LOGIN usuario_historial WITH PASSWORD = N'1234', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;
END
GO

USE PacientesDB;
GO

IF OBJECT_ID(N'dbo.tbl_pacientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_pacientes(
        id_pac INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        cedula_pac VARCHAR(13) NOT NULL,
        nombre_pac VARCHAR(30) NOT NULL,
        apellido_pac VARCHAR(30) NOT NULL,
        direccion_pac VARCHAR(50) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'usuario_pacientes')
BEGIN
    CREATE USER usuario_pacientes FOR LOGIN usuario_pacientes;
END
GO

ALTER ROLE db_owner ADD MEMBER usuario_pacientes;
GO

USE Historial_ClinicoDB;
GO

IF OBJECT_ID(N'dbo.tbl_historial_clinico', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_historial_clinico(
        id_histcl INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        id_pac INT NOT NULL,
        num_historia INT NOT NULL,
        diagnostico_pac VARCHAR(150) NOT NULL,
        tratamiento_pac VARCHAR(100) NOT NULL,
        fecha_his DATETIME NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'usuario_historial')
BEGIN
    CREATE USER usuario_historial FOR LOGIN usuario_historial;
END
GO

ALTER ROLE db_owner ADD MEMBER usuario_historial;
GO
