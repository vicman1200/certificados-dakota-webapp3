-- =============================================
-- DDL: Tabla de Usuarios
-- Sistema: Bradial - Gestión de Certificados
-- Descripción: Almacena la información de usuarios del sistema para autenticación
-- =============================================

-- Crear tabla de Usuarios
CREATE TABLE [dbo].[Usuarios] (
    -- Identificador único del usuario
    [Id] INT IDENTITY(1,1) NOT NULL,
    
    -- Nombre de usuario (único, usado para login)
    [Usuario] NVARCHAR(50) NOT NULL,
    
    -- Contraseña hasheada (usar BCrypt, PBKDF2, o similar)
    -- Longitud recomendada: 256 caracteres para hash BCrypt
    [PasswordHash] NVARCHAR(256) NOT NULL,
    
    -- Nombre completo del usuario
    [Nombre] NVARCHAR(200) NOT NULL,
    
    -- Email del usuario (único)
    [Email] NVARCHAR(200) NOT NULL,
    
    -- Rol del usuario: 'Admin', 'Usuario', 'Supervisor', etc.
    [Rol] NVARCHAR(50) NOT NULL DEFAULT 'Usuario',
    
    -- Indica si el usuario está activo (puede iniciar sesión)
    [Activo] BIT NOT NULL DEFAULT 1,
    
    -- Información de auditoría
    [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [FechaUltimoAcceso] DATETIME2 NULL,
    [IntentosFallidos] INT NOT NULL DEFAULT 0,
    [BloqueadoHasta] DATETIME2 NULL,
    
    -- Token de restablecimiento de contraseña (opcional)
    [TokenResetPassword] NVARCHAR(256) NULL,
    [TokenResetPasswordExpira] DATETIME2 NULL,
    
    -- Observaciones o notas adicionales
    [Observaciones] NVARCHAR(500) NULL,
    
    -- Constraint de clave primaria
    CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED ([Id] ASC),
    
    -- Constraint de unicidad para el nombre de usuario
    CONSTRAINT [UQ_Usuarios_Usuario] UNIQUE ([Usuario]),
    
    -- Constraint de unicidad para el email
    CONSTRAINT [UQ_Usuarios_Email] UNIQUE ([Email]),
    
    -- Check constraint para validar formato de email básico
    CONSTRAINT [CK_Usuarios_Email] CHECK ([Email] LIKE '%@%.%'),
    
    -- Check constraint para validar que el rol sea válido
    CONSTRAINT [CK_Usuarios_Rol] CHECK ([Rol] IN ('Admin', 'Usuario', 'Supervisor', 'Operador'))
);

-- Crear índice para búsquedas por usuario (ya está cubierto por el UNIQUE, pero útil para performance)
CREATE NONCLUSTERED INDEX [IX_Usuarios_Usuario] 
ON [dbo].[Usuarios] ([Usuario] ASC)
INCLUDE ([PasswordHash], [Activo], [BloqueadoHasta]);

-- Crear índice para búsquedas por email
CREATE NONCLUSTERED INDEX [IX_Usuarios_Email] 
ON [dbo].[Usuarios] ([Email] ASC);

-- Crear índice para búsquedas por rol
CREATE NONCLUSTERED INDEX [IX_Usuarios_Rol] 
ON [dbo].[Usuarios] ([Rol] ASC)
WHERE [Activo] = 1;

-- =============================================
-- Datos de ejemplo (OPCIONAL - Solo para desarrollo)
-- =============================================
-- NOTA: Las contraseñas deben estar hasheadas. 
-- Ejemplo usando BCrypt.Net: BCrypt.Net.BCrypt.HashPassword("password123")
-- 
-- Usuario: admin
-- Password: Admin123! (hash: $2a$11$ejemplo_hash_aqui)
-- 
-- Usuario: tester
-- Password: Tester123! (hash: $2a$11$ejemplo_hash_aqui)
-- 
-- INSERT INTO [dbo].[Usuarios] ([Usuario], [PasswordHash], [Nombre], [Email], [Rol], [Activo])
-- VALUES 
--     ('admin', '$2a$11$ejemplo_hash_bcrypt_aqui', 'Administrador', 'admin@bradial.mx', 'Admin', 1),
--     ('tester', '$2a$11$ejemplo_hash_bcrypt_aqui', 'Usuario de Prueba', 'tester@bradial.mx', 'Usuario', 1);

-- =============================================
-- Comentarios y Notas
-- =============================================
-- 
-- SEGURIDAD:
-- 1. NUNCA almacenar contraseñas en texto plano
-- 2. Usar algoritmos de hash seguros: BCrypt, PBKDF2, Argon2
-- 3. Implementar bloqueo de cuenta después de X intentos fallidos
-- 4. Considerar implementar 2FA (autenticación de dos factores)
-- 
-- HASH DE CONTRASEÑAS:
-- En C# usar: BCrypt.Net-Next (NuGet Package)
-- Ejemplo:
--   string passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
--   bool isValid = BCrypt.Net.BCrypt.Verify("password123", passwordHash);
-- 
-- VALIDACIÓN:
-- - Verificar que el usuario existe
-- - Verificar que está activo (Activo = 1)
-- - Verificar que no está bloqueado (BloqueadoHasta < GETDATE() o NULL)
-- - Verificar la contraseña usando el hash almacenado
-- - Actualizar FechaUltimoAcceso y resetear IntentosFallidos en login exitoso
-- - Incrementar IntentosFallidos y bloquear si excede el límite en login fallido

