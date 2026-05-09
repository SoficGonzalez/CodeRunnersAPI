# SGA — Sistema de Gestión de Plantillas y Autenticación

API REST monolítica desarrollada en **.NET 8** con **C#** para la gestión de incidencias laborales, plantillas de documentos Word/RTF, usuarios y sedes. Desarrollada por el equipo **Code Runners** como parte del proyecto de investigación de la Universidad Evangélica de El Salvador.

---

## Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 8.0 | Framework principal |
| ASP.NET Core | 8.0 | API REST |
| Entity Framework Core | 8.0.10 | ORM y migraciones |
| SQL Server | Express | Base de datos relacional |
| ASP.NET Core Identity | 8.0.10 | Autenticación y gestión de usuarios |
| JWT Bearer | 8.0.10 | Autorización por tokens |
| DocumentFormat.OpenXml | 3.0.2 | Procesamiento de archivos .docx |
| Swashbuckle (Swagger) | 6.6.2 | Documentación interactiva de la API |
| System.Text.Encoding.CodePages | 8.0.0 | Soporte RTF (Windows-1252) |

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server Express (o superior)
- Visual Studio Insiders

---

## Configuración

### 1. Cadena de conexión

Edita `appsettings.json` con tus credenciales de SQL Server:

```json
{
  "ConnectionStrings": {
    "SgaDb": "Server=localhost\\SQLEXPRESS;Database=SGA_DB;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "clave-super-secreta-sga-minimo-32-caracteres!",
    "Issuer": "sga-api",
    "Audience": "sga-api"
  },
  "Storage": {
    "RutaPlantillas": "C:\\SGA\\Repositorio\\Plantillas",
    "TamanoMaximoMB": 50
  }
}
```

### 2. Migraciones

```bash
# Package Manager Console (Visual Studio)
Add-Migration InitialCreate
Update-Database

# O con CLI
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Correr el proyecto

```bash
dotnet run
```

Al iniciar por primera vez, el sistema crea automáticamente:
- Roles de Identity: `Administrador`, `RRHH`, `Operador`
- Usuario administrador por defecto:
  - **Correo:** `admin@sga.com`
  - **Password:** `Admin1234*`

---

## Endpoints

### Autenticación — `/api/auth`

| Método | Ruta | Descripción | Acceso |
|---|---|---|---|
| POST | `/api/auth/login` | Iniciar sesión, retorna JWT | Público |
| POST | `/api/auth/registrar` | Registrar nuevo usuario | Solo Administrador |

#### Login — Request
```json
{
  "correo": "admin@sga.com",
  "password": "Admin1234*"
}
```

#### Login — Response
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "nombreCompleto": "Administrador Sistema",
  "correo": "admin@sga.com",
  "roles": ["Administrador"]
}
```

#### Registrar — Request
```json
{
  "nombreUsuario": "usuario.rrhh",
  "nombreCompleto": "Nombre de Usuario",
  "correo": "usuario@sga.com",
  "password": "Usuario1234*",
  "rolId": 2,
  "sedeId": 1
}
```

---

### Sedes — `/api/sedes`

| Método | Ruta | Descripción | Acceso |
|---|---|---|---|
| GET | `/api/sedes` | Listar todas las sedes | Público |
| GET | `/api/sedes/{id}` | Obtener sede por ID | Público |
| POST | `/api/sedes` | Crear nueva sede | Solo Administrador |
| PUT | `/api/sedes/{id}` | Actualizar sede | Solo Administrador |

#### POST/PUT — Request
```json
{
  "nombre": "Sede San Salvador",
  "direccion": "Blvd. de los Héroes, Col. Escalón",
  "departamento": "San Salvador",
  "activa": true
}
```

---

### Plantillas — `/api/plantillas`

| Método | Ruta | Descripción | Acceso |
|---|---|---|---|
| POST | `/api/plantillas` | Crear plantilla manual (solo metadata) | Autenticado |
| POST | `/api/plantillas/importar-docx` | Importar plantilla desde archivo .docx o .rtf | Autenticado |
| PUT | `/api/plantillas/{id}` | Actualizar metadata de plantilla | Autenticado |

#### POST Crear — Request
```json
{
  "nombre": "Solicitud de Vacaciones",
  "descripcion": "Formulario de solicitud de vacaciones",
  "estadoPlantillaId": 1,
  "creadoPorUsuarioId": 1
}
```

#### POST Importar — Form Data
| Campo | Tipo | Descripción |
|---|---|---|
| archivo | File | Archivo .docx o .rtf con Content Controls |
| descripcion | Text | Descripción opcional |
| estadoPlantillaId | Text | ID del estado de plantilla |
| creadoPorUsuarioId | Text | ID del usuario creador |

#### PUT Actualizar — Request
```json
{
  "nombre": "Solicitud de Vacaciones v2",
  "descripcion": "Descripción actualizada",
  "estadoPlantillaId": 1,
  "activa": true
}
```

---

## Roles del sistema

| Rol | Descripción | Permisos |
|---|---|---|
| Administrador | Administrador del sistema | Acceso total, registrar usuarios, gestionar sedes |
| RRHH | Recursos Humanos | Gestión de incidencias y documentos |
| Operador | Usuario operativo | Acceso a funciones básicas |

---

## Modelos principales

```
Sede
 └── Usuario (uno a muchos)
      └── Rol (muchos a uno)

EstadoPlantilla
 └── Plantilla (uno a muchos)
      ├── CampoPlantilla (uno a muchos)
      └── DocumentoLlenado (uno a muchos)
           ├── ValorCampo (uno a muchos)
           └── ArchivoEvidencia (uno a muchos)
```

---

## Plantillas Word (.docx)

Para importar una plantilla, el archivo `.docx` debe contener **Content Controls** con Tag y Título definidos:

1. Activar pestaña **Desarrollador** en Word
2. Insertar **Control de contenido de texto** (`Aa`)
3. En **Propiedades** del control definir:
   - `Título`: etiqueta visible en pantalla (ej. `Nombre del Empleado`)
   - `Etiqueta (Tag)`: clave técnica del campo (ej. `nombre_empleado`)

Para archivos `.rtf`, usar marcadores tipo `{{nombre_campo}}` o `<<nombre_campo>>`.

---

## Uso de Swagger

1. Correr el proyecto — Swagger disponible en `https://localhost:{puerto}/swagger`
2. Hacer login en `POST /api/auth/login`
3. Copiar el token de la respuesta
4. Clic en **Authorize** (candado) → pegar el token
5. Todos los endpoints protegidos quedan habilitados

---

## Equipo

**Code Runners** — Ingeniería en Desarrollo de Software y Ciencias de Datos  
Universidad Evangélica de El Salvador

- María José Castellanos Castillo
- Sofía Cristina González González
- David Alexander Umaña Cortez
- Carlos Enrique Sosa Henríquez
- Rodrigo Eduardo Herrera Coto
