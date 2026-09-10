-- Azure SQL: ejecutar conectado a Historial_ClinicoDB

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

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_historial_clinico)
BEGIN
    INSERT INTO dbo.tbl_historial_clinico (id_pac, num_historia, diagnostico_pac, tratamiento_pac, fecha_his)
    VALUES 
        (1, 1001, 'Control preventivo general', 'Reposo e hidratacion', GETDATE()),
        (2, 1002, 'Rinitis alergica estacional', 'Antihistaminicos por 7 dias', GETDATE());
END
GO
