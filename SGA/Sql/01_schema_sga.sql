/* =============================================================================
   SGA - Esquema de base de datos (DDL + índices + restricciones + semillas)
   Motor: SQL Server 2019+
   Convenciones:
     - PKs INT IDENTITY
     - FechaXxx -> datetime2
     - Texto    -> nvarchar con tamaño explícito
   ============================================================================= */

/* -----------------------------------------------------------------------------
   1) Crear la base de datos (descomenta si la quieres crear desde cero)
   ----------------------------------------------------------------------------- */
-- IF DB_ID('SGA_DB') IS NULL
-- BEGIN
--     CREATE DATABASE SGA_DB;
-- END
-- GO
-- USE SGA_DB;
-- GO

SET NOCOUNT ON;
GO

/* -----------------------------------------------------------------------------
   2) DROP en orden inverso de dependencias (idempotente para re-ejecutar)
   ----------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ArchivoEvidencia',  'U') IS NOT NULL DROP TABLE dbo.ArchivoEvidencia;
IF OBJECT_ID('dbo.ValorCampo',        'U') IS NOT NULL DROP TABLE dbo.ValorCampo;
IF OBJECT_ID('dbo.DocumentoLlenado',  'U') IS NOT NULL DROP TABLE dbo.DocumentoLlenado;
IF OBJECT_ID('dbo.CampoPlantilla',    'U') IS NOT NULL DROP TABLE dbo.CampoPlantilla;
IF OBJECT_ID('dbo.Plantilla',         'U') IS NOT NULL DROP TABLE dbo.Plantilla;
IF OBJECT_ID('dbo.Usuario',           'U') IS NOT NULL DROP TABLE dbo.Usuario;
IF OBJECT_ID('dbo.RolModulo',         'U') IS NOT NULL DROP TABLE dbo.RolModulo;
IF OBJECT_ID('dbo.Modulo',            'U') IS NOT NULL DROP TABLE dbo.Modulo;
IF OBJECT_ID('dbo.Rol',               'U') IS NOT NULL DROP TABLE dbo.Rol;
IF OBJECT_ID('dbo.EstadoPlantilla',   'U') IS NOT NULL DROP TABLE dbo.EstadoPlantilla;
GO

/* -----------------------------------------------------------------------------
   3) Tablas catálogo
   ----------------------------------------------------------------------------- */
CREATE TABLE dbo.EstadoPlantilla (
    EstadoPlantillaId INT IDENTITY(1,1) NOT NULL,
    Codigo            NVARCHAR(20)      NOT NULL,
    Nombre            NVARCHAR(80)      NOT NULL,
    Orden             INT               NOT NULL CONSTRAINT DF_EstadoPlantilla_Orden  DEFAULT (0),
    Activo            BIT               NOT NULL CONSTRAINT DF_EstadoPlantilla_Activo DEFAULT (1),
    CONSTRAINT PK_EstadoPlantilla PRIMARY KEY CLUSTERED (EstadoPlantillaId),
    CONSTRAINT UQ_EstadoPlantilla_Codigo UNIQUE (Codigo)
);
GO

CREATE TABLE dbo.Rol (
    RolId     INT IDENTITY(1,1) NOT NULL,
    NombreRol NVARCHAR(60)      NOT NULL,
    Activo    BIT               NOT NULL CONSTRAINT DF_Rol_Activo DEFAULT (1),
    CONSTRAINT PK_Rol PRIMARY KEY CLUSTERED (RolId),
    CONSTRAINT UQ_Rol_NombreRol UNIQUE (NombreRol)
);
GO

CREATE TABLE dbo.Modulo (
    ModuloId     INT IDENTITY(1,1) NOT NULL,
    CodigoModulo NVARCHAR(40)      NOT NULL,
    NombreModulo NVARCHAR(120)     NOT NULL,
    Orden        INT               NOT NULL CONSTRAINT DF_Modulo_Orden DEFAULT (0),
    CONSTRAINT PK_Modulo PRIMARY KEY CLUSTERED (ModuloId),
    CONSTRAINT UQ_Modulo_CodigoModulo UNIQUE (CodigoModulo)
);
GO

CREATE TABLE dbo.RolModulo (
    RolModuloId INT IDENTITY(1,1) NOT NULL,
    RolId       INT               NOT NULL,
    ModuloId    INT               NOT NULL,
    CONSTRAINT PK_RolModulo PRIMARY KEY CLUSTERED (RolModuloId),
    CONSTRAINT UQ_RolModulo_Rol_Modulo UNIQUE (RolId, ModuloId),
    CONSTRAINT FK_RolModulo_Rol    FOREIGN KEY (RolId)    REFERENCES dbo.Rol(RolId)       ON DELETE CASCADE,
    CONSTRAINT FK_RolModulo_Modulo FOREIGN KEY (ModuloId) REFERENCES dbo.Modulo(ModuloId) ON DELETE CASCADE
);
GO

/* -----------------------------------------------------------------------------
   4) Usuario
   ----------------------------------------------------------------------------- */
CREATE TABLE dbo.Usuario (
    UsuarioId       INT IDENTITY(1,1) NOT NULL,
    RolId           INT               NOT NULL,
    NombreUsuario   NVARCHAR(60)      NOT NULL,
    NombreCompleto  NVARCHAR(150)     NOT NULL,
    Correo          NVARCHAR(150)     NOT NULL,
    Activo          BIT               NOT NULL CONSTRAINT DF_Usuario_Activo        DEFAULT (1),
    FechaCreacion   DATETIME2(0)      NOT NULL CONSTRAINT DF_Usuario_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Usuario PRIMARY KEY CLUSTERED (UsuarioId),
    CONSTRAINT UQ_Usuario_NombreUsuario UNIQUE (NombreUsuario),
    CONSTRAINT UQ_Usuario_Correo        UNIQUE (Correo),
    CONSTRAINT FK_Usuario_Rol           FOREIGN KEY (RolId) REFERENCES dbo.Rol(RolId),
    CONSTRAINT CK_Usuario_Correo        CHECK (Correo LIKE '_%@_%._%')
);
GO

/* -----------------------------------------------------------------------------
   5) Plantilla y CampoPlantilla
   ----------------------------------------------------------------------------- */
CREATE TABLE dbo.Plantilla (
    PlantillaId          INT IDENTITY(1,1) NOT NULL,
    EstadoPlantillaId    INT               NOT NULL,
    CreadoPorUsuarioId   INT               NOT NULL,
    Nombre               NVARCHAR(180)     NOT NULL,
    Descripcion          NVARCHAR(500)     NULL,
    RutaArchivoWord      NVARCHAR(500)     NULL,
    Activa               BIT               NOT NULL CONSTRAINT DF_Plantilla_Activa             DEFAULT (1),
    FechaCreacion        DATETIME2(0)      NOT NULL CONSTRAINT DF_Plantilla_FechaCreacion      DEFAULT (SYSUTCDATETIME()),
    FechaActualizacion   DATETIME2(0)      NOT NULL CONSTRAINT DF_Plantilla_FechaActualizacion DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Plantilla PRIMARY KEY CLUSTERED (PlantillaId),
    CONSTRAINT FK_Plantilla_EstadoPlantilla FOREIGN KEY (EstadoPlantillaId)  REFERENCES dbo.EstadoPlantilla(EstadoPlantillaId),
    CONSTRAINT FK_Plantilla_Usuario         FOREIGN KEY (CreadoPorUsuarioId) REFERENCES dbo.Usuario(UsuarioId)
);
GO

CREATE INDEX IX_Plantilla_Estado  ON dbo.Plantilla (EstadoPlantillaId);
CREATE INDEX IX_Plantilla_Creador ON dbo.Plantilla (CreadoPorUsuarioId);
CREATE INDEX IX_Plantilla_Nombre  ON dbo.Plantilla (Nombre);
GO

CREATE TABLE dbo.CampoPlantilla (
    CampoPlantillaId INT IDENTITY(1,1) NOT NULL,
    PlantillaId      INT               NOT NULL,
    ClaveCampo       NVARCHAR(100)     NOT NULL,
    EtiquetaPantalla NVARCHAR(150)     NOT NULL,
    Orden            INT               NOT NULL CONSTRAINT DF_CampoPlantilla_Orden       DEFAULT (0),
    Obligatorio      BIT               NOT NULL CONSTRAINT DF_CampoPlantilla_Obligatorio DEFAULT (0),
    TipoDato         NVARCHAR(30)      NOT NULL CONSTRAINT DF_CampoPlantilla_TipoDato    DEFAULT ('texto'),
    ValorPorDefecto  NVARCHAR(500)     NULL,
    CONSTRAINT PK_CampoPlantilla PRIMARY KEY CLUSTERED (CampoPlantillaId),
    CONSTRAINT UQ_CampoPlantilla_Plantilla_Clave UNIQUE (PlantillaId, ClaveCampo),
    CONSTRAINT FK_CampoPlantilla_Plantilla FOREIGN KEY (PlantillaId)
        REFERENCES dbo.Plantilla(PlantillaId) ON DELETE CASCADE,
    CONSTRAINT CK_CampoPlantilla_TipoDato CHECK (TipoDato IN ('texto','numero','fecha','booleano','memo'))
);
GO

CREATE INDEX IX_CampoPlantilla_Plantilla_Orden ON dbo.CampoPlantilla (PlantillaId, Orden);
GO

/* -----------------------------------------------------------------------------
   6) DocumentoLlenado y ValorCampo
   ----------------------------------------------------------------------------- */
CREATE TABLE dbo.DocumentoLlenado (
    DocumentoLlenadoId       INT IDENTITY(1,1) NOT NULL,
    PlantillaId              INT               NOT NULL,
    RegistradoPorUsuarioId   INT               NOT NULL,
    Titulo                   NVARCHAR(200)     NOT NULL,
    RutaDocumentoGenerado    NVARCHAR(500)     NULL,
    Notas                    NVARCHAR(1000)    NULL,
    FechaCreacion            DATETIME2(0)      NOT NULL CONSTRAINT DF_DocumentoLlenado_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_DocumentoLlenado PRIMARY KEY CLUSTERED (DocumentoLlenadoId),
    CONSTRAINT FK_DocumentoLlenado_Plantilla FOREIGN KEY (PlantillaId)            REFERENCES dbo.Plantilla(PlantillaId),
    CONSTRAINT FK_DocumentoLlenado_Usuario   FOREIGN KEY (RegistradoPorUsuarioId) REFERENCES dbo.Usuario(UsuarioId)
);
GO

CREATE INDEX IX_DocumentoLlenado_Plantilla ON dbo.DocumentoLlenado (PlantillaId);
CREATE INDEX IX_DocumentoLlenado_Usuario   ON dbo.DocumentoLlenado (RegistradoPorUsuarioId);
GO

CREATE TABLE dbo.ValorCampo (
    ValorCampoId        INT IDENTITY(1,1) NOT NULL,
    DocumentoLlenadoId  INT               NOT NULL,
    CampoPlantillaId    INT               NOT NULL,
    TextoValor          NVARCHAR(MAX)     NULL,
    FechaGuardado       DATETIME2(0)      NOT NULL CONSTRAINT DF_ValorCampo_FechaGuardado DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ValorCampo PRIMARY KEY CLUSTERED (ValorCampoId),
    CONSTRAINT UQ_ValorCampo_Doc_Campo UNIQUE (DocumentoLlenadoId, CampoPlantillaId),
    CONSTRAINT FK_ValorCampo_DocumentoLlenado FOREIGN KEY (DocumentoLlenadoId)
        REFERENCES dbo.DocumentoLlenado(DocumentoLlenadoId) ON DELETE CASCADE,
    CONSTRAINT FK_ValorCampo_CampoPlantilla   FOREIGN KEY (CampoPlantillaId)
        REFERENCES dbo.CampoPlantilla(CampoPlantillaId)
);
GO

/* -----------------------------------------------------------------------------
   7) ArchivoEvidencia
   ----------------------------------------------------------------------------- */
CREATE TABLE dbo.ArchivoEvidencia (
    ArchivoEvidenciaId  INT IDENTITY(1,1) NOT NULL,
    DocumentoLlenadoId  INT               NOT NULL,
    SubidoPorUsuarioId  INT               NOT NULL,
    NombreArchivo       NVARCHAR(255)     NOT NULL,
    RutaEnRepositorio   NVARCHAR(500)     NOT NULL,
    TipoContenido       NVARCHAR(120)     NULL,
    TamanoBytes         BIGINT            NOT NULL,
    FechaSubida         DATETIME2(0)      NOT NULL CONSTRAINT DF_ArchivoEvidencia_FechaSubida DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ArchivoEvidencia PRIMARY KEY CLUSTERED (ArchivoEvidenciaId),
    CONSTRAINT FK_ArchivoEvidencia_DocumentoLlenado FOREIGN KEY (DocumentoLlenadoId)
        REFERENCES dbo.DocumentoLlenado(DocumentoLlenadoId) ON DELETE CASCADE,
    CONSTRAINT FK_ArchivoEvidencia_Usuario           FOREIGN KEY (SubidoPorUsuarioId)
        REFERENCES dbo.Usuario(UsuarioId),
    CONSTRAINT CK_ArchivoEvidencia_Tamano CHECK (TamanoBytes >= 0)
);
GO

CREATE INDEX IX_ArchivoEvidencia_Doc ON dbo.ArchivoEvidencia (DocumentoLlenadoId);
GO

/* =============================================================================
   DATOS SEMILLA
   ============================================================================= */

-- Estados de plantilla
INSERT INTO dbo.EstadoPlantilla (Codigo, Nombre, Orden, Activo) VALUES
 ('BORRADOR',   'Borrador',   1, 1),
 ('PUBLICADA',  'Publicada',  2, 1),
 ('ARCHIVADA',  'Archivada',  3, 1);
GO

-- Roles
INSERT INTO dbo.Rol (NombreRol, Activo) VALUES
 ('Administrador', 1),
 ('Editor',        1),
 ('Lector',        1);
GO

-- Módulos
INSERT INTO dbo.Modulo (CodigoModulo, NombreModulo, Orden) VALUES
 ('PLANTILLAS',  'Gestión de Plantillas',  1),
 ('LLENADOS',    'Llenado de Documentos',  2),
 ('USUARIOS',    'Gestión de Usuarios',    3),
 ('REPORTES',    'Reportes',               4);
GO

-- Permisos por rol
DECLARE @RolAdmin   INT = (SELECT RolId FROM dbo.Rol WHERE NombreRol = 'Administrador');
DECLARE @RolEditor  INT = (SELECT RolId FROM dbo.Rol WHERE NombreRol = 'Editor');
DECLARE @RolLector  INT = (SELECT RolId FROM dbo.Rol WHERE NombreRol = 'Lector');

INSERT INTO dbo.RolModulo (RolId, ModuloId)
SELECT @RolAdmin, ModuloId FROM dbo.Modulo;

INSERT INTO dbo.RolModulo (RolId, ModuloId)
SELECT @RolEditor, ModuloId FROM dbo.Modulo WHERE CodigoModulo IN ('PLANTILLAS','LLENADOS');

INSERT INTO dbo.RolModulo (RolId, ModuloId)
SELECT @RolLector, ModuloId FROM dbo.Modulo WHERE CodigoModulo IN ('LLENADOS','REPORTES');
GO

-- Usuario administrador de ejemplo
INSERT INTO dbo.Usuario (RolId, NombreUsuario, NombreCompleto, Correo, Activo)
SELECT TOP 1 RolId, 'admin', 'Administrador del Sistema', 'admin@sga.local', 1
FROM dbo.Rol WHERE NombreRol = 'Administrador';
GO

-- Verificación rápida
SELECT 'EstadoPlantilla' AS Tabla, COUNT(*) AS Filas FROM dbo.EstadoPlantilla UNION ALL
SELECT 'Rol',                       COUNT(*)         FROM dbo.Rol             UNION ALL
SELECT 'Modulo',                    COUNT(*)         FROM dbo.Modulo          UNION ALL
SELECT 'RolModulo',                 COUNT(*)         FROM dbo.RolModulo       UNION ALL
SELECT 'Usuario',                   COUNT(*)         FROM dbo.Usuario;
GO
