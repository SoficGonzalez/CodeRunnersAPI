---
name: SGA Plantillas Word API
overview: Construir una API ASP.NET Core 8 monolítica (EF Core + SQL Server + OpenXml) para gestión de plantillas Word con dos endpoints (importar .docx detectando Content Controls, y crear plantilla manual), incluyendo script SQL completo, semilla de datos, validaciones, manejo global de errores y Swagger.
todos:
  - id: csproj
    content: Actualizar SGA.csproj con EF Core SqlServer, EF Core Tools, DocumentFormat.OpenXml, Swashbuckle
    status: completed
  - id: models
    content: Crear las 10 entidades en Models/ con anotaciones y navegaciones (EstadoPlantilla, Rol, Modulo, RolModulo, Usuario, Plantilla, CampoPlantilla, DocumentoLlenado, ValorCampo, ArchivoEvidencia)
    status: completed
  - id: dbcontext
    content: Crear Data/SgaDbContext.cs con DbSets, OnModelCreating (PKs, FKs, indices, UNIQUEs, CHECK, datetime2)
    status: completed
  - id: dtos
    content: "Crear DTOs: CrearPlantillaRequest, PlantillaResponse, ImportarPlantillaResponse, CampoDetectadoDto, ErrorResponse"
    status: completed
  - id: options
    content: Crear Configuration/StorageOptions.cs
    status: completed
  - id: parser
    content: Implementar WordTemplateParser (lee .docx, recorre SdtElement, normaliza ClaveCampo, dedupe, mantiene orden)
    status: completed
  - id: storage
    content: Implementar FileStorageService (guardar archivo en disco, renombrar a {plantillaId}.docx, validar tamano)
    status: completed
  - id: service
    content: Implementar PlantillaService con metodos CrearAsync e ImportarDocxAsync (transaccion EF)
    status: completed
  - id: controller
    content: Crear PlantillasController con POST /api/plantillas y POST /api/plantillas/importar-docx
    status: completed
  - id: middleware
    content: Crear ExceptionHandlingMiddleware para respuestas JSON estandarizadas
    status: completed
  - id: program
    content: Reescribir Program.cs (DI, EF, Swagger, IFormFile size limits, middleware, options binding)
    status: completed
  - id: appsettings
    content: Configurar appsettings.json (ConnectionStrings:SgaDb, Storage:RutaPlantillas, Storage:TamanoMaximoMB) y appsettings.Development.json
    status: completed
  - id: sql
    content: Crear Sql/01_schema_sga.sql con CREATE DATABASE comentado, todas las tablas, FKs, indices, UNIQUE, CHECK y semillas (estados, roles, modulos, usuario admin)
    status: completed
  - id: http
    content: Actualizar SGA.http con ejemplos cURL/REST de ambos endpoints
    status: completed
  - id: build
    content: Ejecutar dotnet build para verificar compilacion
    status: completed
  - id: deliverable
    content: Redactar respuesta final en chat con las 10 secciones exigidas (arquitectura, estructura, blueprint, archivo-por-archivo, SQL, appsettings, Program.cs, cURL, pruebas, checklist)
    status: completed
isProject: false
---

## Decisiones de diseño (fijadas)

- **Normalización `ClaveCampo` (fieldKey)**: tomar `SdtProperties.Tag` si existe; si no, `Alias`; si no, `campo_N`. Normalizar: minúsculas, sin acentos, espacios → `_`, eliminar todo lo que no sea `[a-z0-9_]`, colapsar `_` repetidos, máx. 100 chars. Si tras normalizar queda vacío, usar `campo_N`. Se descartan duplicados por `ClaveCampo` dentro de la misma plantilla (se conserva el primero).
- **Etiqueta de pantalla**: se toma de `Alias` (si existe) o `Tag`; si ambos faltan, `"Campo {N}"`.
- **Orden**: orden de aparición en el documento (recorrido secuencial sobre `MainDocumentPart.Document.Body.Descendants<SdtElement>()`).
- **Tipo de dato**: `"texto"` por defecto (SDT plain text).
- **Almacenamiento**: ruta configurable (`Storage:RutaPlantillas`). Nombre físico: `{plantillaId}.docx` (se renombra tras insertar en BD para conocer el ID; se usa transacción + rollback si falla la copia).
- **Límite subida**: configurable (`Storage:TamanoMaximoMB`, default 50 MB), aplicado por Kestrel y `[RequestSizeLimit]`.
- **Si no se detecta ningún Content Control** → `422 Unprocessable Entity`.
- **Versiones de paquetes**: `Microsoft.EntityFrameworkCore.SqlServer` 8.0.x, `DocumentFormat.OpenXml` 3.0.x, `Swashbuckle.AspNetCore` 6.6.x.
- **Base de datos**: `SGA_DB`. Convención: PKs `int IDENTITY`, FKs `int NOT NULL`, `datetime2`, `nvarchar` con tamaños explícitos.

## Estructura de carpetas final

```
SGA/
  Controllers/
    PlantillasController.cs
  Data/
    SgaDbContext.cs
    SeedData.cs
  Models/
    EstadoPlantilla.cs
    Rol.cs
    Modulo.cs
    RolModulo.cs
    Usuario.cs
    Plantilla.cs
    CampoPlantilla.cs
    DocumentoLlenado.cs
    ValorCampo.cs
    ArchivoEvidencia.cs
  DTOs/
    CrearPlantillaRequest.cs
    PlantillaResponse.cs
    ImportarPlantillaResponse.cs
    CampoDetectadoDto.cs
    ErrorResponse.cs
  Services/
    IPlantillaService.cs
    PlantillaService.cs
    IWordTemplateParser.cs
    WordTemplateParser.cs
    IFileStorageService.cs
    FileStorageService.cs
  Middleware/
    ExceptionHandlingMiddleware.cs
  Configuration/
    StorageOptions.cs
  Sql/
    01_schema_sga.sql
  appsettings.json
  appsettings.Development.json
  Program.cs
  SGA.csproj
  SGA.http
```

## Blueprint paso a paso

1. Actualizar `SGA.csproj` con paquetes EF Core SQL Server, EF Core Tools, OpenXml.
2. Crear todas las entidades en `Models/` con propiedades y relaciones del ER.
3. Crear `Data/SgaDbContext.cs` con `DbSet`s, `OnModelCreating` (PKs, FKs, índices, UNIQUEs).
4. Crear DTOs en `DTOs/`.
5. Crear `Configuration/StorageOptions.cs` para bindeo de `Storage` desde appsettings.
6. Implementar `Services/`:
  - `WordTemplateParser`: lee `.docx`, recorre `SdtElement`, normaliza claves, dedupe.
  - `FileStorageService`: guardar/renombrar archivo en `Storage:RutaPlantillas`.
  - `PlantillaService`: orquesta importación (transacción) y creación manual.
7. Crear `Controllers/PlantillasController.cs` con los dos endpoints + validaciones.
8. Crear `Middleware/ExceptionHandlingMiddleware.cs` para errores → JSON estandarizado.
9. Reescribir `Program.cs` (DI, EF, Swagger, opciones, límites Kestrel/Form, middleware).
10. Configurar `appsettings.json` (`ConnectionStrings:SgaDb`, `Storage`).
11. Escribir script SQL `Sql/01_schema_sga.sql` (DDL + índices + UNIQUE + CHECK + semillas).
12. Actualizar `SGA.http` con ejemplos de los dos endpoints.
13. Ejecutar `dotnet build` para verificar compilación.

## Endpoints a implementar

- `POST /api/plantillas` — JSON, crea metadata, devuelve 201.
- `POST /api/plantillas/importar-docx` — `multipart/form-data`, valida extensión `.docx`, parsea Content Controls, inserta `Plantilla` + `CampoPlantilla[]`, copia archivo a `{plantillaId}.docx`, devuelve 201 con `camposDetectados`.

Errores estandarizados: 400 (validación/extensión), 404 (FKs no existen), 422 (sin campos), 500 (errores internos del parser/IO).

## Entrega final

Después de ejecutar el plan, en el chat se entregará TODO en el orden exacto solicitado por el usuario (10 secciones: arquitectura → estructura → blueprint → tabla archivo-por-archivo con código completo → SQL completo → appsettings → Program.cs → cURL → pruebas mínimas → checklist).