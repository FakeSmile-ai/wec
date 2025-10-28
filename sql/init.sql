-- Crea base si no existe
IF DB_ID('authbd') IS NULL
BEGIN
    CREATE DATABASE authbd;
END
GO

USE authbd;
GO

-- Crea LOGIN si no existe
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = 'marcador_app')
BEGIN
    CREATE LOGIN marcador_app
    WITH PASSWORD = 'MarcadorAppPy1',
         CHECK_POLICY = ON,
         CHECK_EXPIRATION = OFF;
END
GO

-- Crea USER si no existe
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'marcador_app')
BEGIN
    CREATE USER marcador_app FOR LOGIN marcador_app;
END
GO

-- Permisos: para correr migraciones de EF Core sin problemas
IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members drm
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id AND r.name = 'db_owner'
    JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id AND m.name = 'marcador_app'
)
BEGIN
    EXEC sp_addrolemember 'db_owner','marcador_app';
END
GO
