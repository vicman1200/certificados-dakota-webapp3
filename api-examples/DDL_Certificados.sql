-- =============================================
-- DDL: Tabla de Certificados
-- Sistema: Bradial - Gestión de Certificados
-- Descripción: Almacena la información de los certificados generados
-- =============================================

-- Crear tabla de Certificados
CREATE TABLE [dbo].[Certificados] (
    -- Identificador único del certificado
    [Id] INT IDENTITY(1,1) NOT NULL,
    
    -- Número de certificado (formato: CERT-XXX)
    [NoCertificado] NVARCHAR(20) NOT NULL,
    
    -- Información del titular
    [Titular] NVARCHAR(200) NOT NULL,
    
    -- Fecha de expedición del certificado
    [FechaExpedicion] DATE NOT NULL,
    
    -- Años de vigencia del certificado (1, 2, 3 o 4)
    [AniosVigencia] TINYINT NOT NULL,
    
    -- Fecha de inicio de vigencia
    [VigenteDesde] DATE NOT NULL,
    
    -- Fecha de fin de vigencia
    [VigenteHasta] DATE NOT NULL,
    
    -- Información del vehículo
    [Marca] NVARCHAR(100) NOT NULL,
    [Submarca] NVARCHAR(100) NOT NULL,
    [Modelo] NVARCHAR(100) NOT NULL,
    [NumeroSerie] NVARCHAR(100) NOT NULL,
    
    -- Estado del certificado: 'Solicitado', 'Vigente', 'Vencido', 'Cancelado'
    [Estado] NVARCHAR(20) NOT NULL DEFAULT 'Solicitado',
    
    -- Información de auditoría
    [Usuario] NVARCHAR(100) NOT NULL,
    [CreadoPor] NVARCHAR(100) NOT NULL,
    [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ModificadoPor] NVARCHAR(100) NULL,
    [FechaModificacion] DATETIME2 NULL,
    
    -- Observaciones o notas adicionales
    [Observaciones] NVARCHAR(500) NULL,
    
    -- Ruta o referencia al archivo PDF del certificado (si se almacena físicamente)
    [RutaArchivoPDF] NVARCHAR(500) NULL,
    
    -- Constraint de clave primaria
    CONSTRAINT [PK_Certificados] PRIMARY KEY CLUSTERED ([Id] ASC),
    
    -- Constraint de unicidad para el número de certificado
    CONSTRAINT [UQ_Certificados_NoCertificado] UNIQUE ([NoCertificado]),
    
    -- Constraint para validar el estado
    CONSTRAINT [CK_Certificados_Estado] CHECK ([Estado] IN ('Solicitado', 'Vigente', 'Vencido', 'Cancelado')),
    
    -- Constraint para validar años de vigencia
    CONSTRAINT [CK_Certificados_AniosVigencia] CHECK ([AniosVigencia] >= 1 AND [AniosVigencia] <= 4),
    
    -- Constraint para validar que la fecha de fin sea mayor a la de inicio
    CONSTRAINT [CK_Certificados_FechasVigencia] CHECK ([VigenteHasta] > [VigenteDesde]),
    
    -- Constraint para validar que la fecha de expedición sea menor o igual a vigente desde
    CONSTRAINT [CK_Certificados_FechaExpedicion] CHECK ([FechaExpedicion] <= [VigenteDesde])
)
ON [PRIMARY];

GO

-- =============================================
-- Índices para mejorar el rendimiento
-- =============================================

-- Índice para búsquedas por número de certificado
CREATE NONCLUSTERED INDEX [IX_Certificados_NoCertificado] 
ON [dbo].[Certificados] ([NoCertificado] ASC)
INCLUDE ([Titular], [Estado], [VigenteDesde], [VigenteHasta]);

GO

-- Índice para búsquedas por titular
CREATE NONCLUSTERED INDEX [IX_Certificados_Titular] 
ON [dbo].[Certificados] ([Titular] ASC)
INCLUDE ([NoCertificado], [Estado], [VigenteHasta]);

GO

-- Índice para búsquedas por estado
CREATE NONCLUSTERED INDEX [IX_Certificados_Estado] 
ON [dbo].[Certificados] ([Estado] ASC)
INCLUDE ([NoCertificado], [Titular], [VigenteHasta]);

GO

-- Índice para búsquedas por fechas de vigencia (útil para certificados próximos a vencer)
CREATE NONCLUSTERED INDEX [IX_Certificados_VigenteHasta] 
ON [dbo].[Certificados] ([VigenteHasta] ASC)
INCLUDE ([NoCertificado], [Titular], [Estado]);

GO

-- Índice para búsquedas por usuario
CREATE NONCLUSTERED INDEX [IX_Certificados_Usuario] 
ON [dbo].[Certificados] ([Usuario] ASC)
INCLUDE ([NoCertificado], [FechaCreacion]);

GO

-- Índice compuesto para búsquedas por marca y modelo
CREATE NONCLUSTERED INDEX [IX_Certificados_MarcaModelo] 
ON [dbo].[Certificados] ([Marca] ASC, [Modelo] ASC)
INCLUDE ([NoCertificado], [Titular], [Estado]);

GO

-- =============================================
-- Triggers para auditoría automática
-- =============================================

-- Trigger para actualizar automáticamente la fecha de modificación
CREATE TRIGGER [TR_Certificados_UpdateFechaModificacion]
ON [dbo].[Certificados]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Certificados]
    SET [FechaModificacion] = GETDATE()
    FROM [dbo].[Certificados] c
    INNER JOIN inserted i ON c.[Id] = i.[Id]
    WHERE c.[FechaModificacion] IS NULL 
       OR c.[FechaModificacion] < i.[FechaModificacion];
END;

GO

-- =============================================
-- Vista para certificados vigentes (opcional)
-- =============================================

CREATE VIEW [dbo].[vw_CertificadosVigentes]
AS
SELECT 
    [Id],
    [NoCertificado],
    [Titular],
    [FechaExpedicion],
    [AniosVigencia],
    [VigenteDesde],
    [VigenteHasta],
    [Marca],
    [Submarca],
    [Modelo],
    [NumeroSerie],
    [Estado],
    [Usuario],
    [CreadoPor],
    [FechaCreacion],
    [ModificadoPor],
    [FechaModificacion],
    [Observaciones],
    [RutaArchivoPDF],
    -- Campo calculado: Días restantes de vigencia
    DATEDIFF(DAY, GETDATE(), [VigenteHasta]) AS [DiasRestantesVigencia]
FROM [dbo].[Certificados]
WHERE [Estado] = 'Vigente'
  AND [VigenteHasta] >= CAST(GETDATE() AS DATE);

GO

-- =============================================
-- Comentarios descriptivos en las columnas
-- =============================================

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Identificador único del certificado (clave primaria)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Certificados', 
    @level2type = N'COLUMN', @level2name = N'Id';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Número único del certificado (formato: CERT-XXX)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Certificados', 
    @level2type = N'COLUMN', @level2name = N'NoCertificado';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Nombre completo del titular del certificado', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Certificados', 
    @level2type = N'COLUMN', @level2name = N'Titular';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Estado del certificado: Solicitado, Vigente, Vencido, Cancelado', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Certificados', 
    @level2type = N'COLUMN', @level2name = N'Estado';

GO

-- =============================================
-- Script completado
-- =============================================

