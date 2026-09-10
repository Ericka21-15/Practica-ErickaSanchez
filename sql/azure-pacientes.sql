-- Azure SQL: ejecutar conectado a PacientesDB
-- Placeholders: <SQL_ADMIN_USER>  (solo documentación; este script no crea logins)

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

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_pacientes)
BEGIN
    INSERT INTO dbo.tbl_pacientes (cedula_pac, nombre_pac, apellido_pac, direccion_pac)
    VALUES 
        ('1712345678', 'Juan', 'Perez', 'Av. Amazonas y Colon, Quito'),
        ('1798765432', 'Maria', 'Gomez', 'Av. 10 de Agosto, Quito');
END
GO
