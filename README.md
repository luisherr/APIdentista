# AgendaDentista — Aplicación Web para Gestión de Citas Dentales

## Descripción del Proyecto

Sistema completo de gestión de citas dentales con recordatorios automáticos por WhatsApp.

**Backend (COMPLETADO):**
- API REST en .NET 8 con Clean Architecture
- Worker Service que envía recordatorios automáticos cada 10 minutos
- Integración con WhatsApp Cloud API (envío/recepción de mensajes)
- Base de datos SQL Server con Entity Framework Core
- Los pacientes pueden confirmar/cancelar citas respondiendo por WhatsApp

**Frontend (POR CONSTRUIR):**
- Aplicación web en React para que cada dentista gestione su consultorio
- Sistema de login por dentista
- Dashboard con agenda del día
- CRUD de pacientes y citas

---

## Stack del Backend Existente

- .NET 8
- ASP.NET Web API
- Entity Framework Core 8
- SQL Server (Docker)
- WhatsApp Cloud API

**Estructura de la solución:**
```
AgendaDentista.sln
├── AgendaDentista.Dominio          (entidades, enums, interfaces)
├── AgendaDentista.Aplicacion       (DTOs, servicios, lógica de negocio)
├── AgendaDentista.Infraestructura  (EF Core, repositorios, WhatsApp)
├── AgendaDentista.API              (controladores REST)
└── AgendaDentista.WorkerRecordatorios (envío automático de recordatorios)
```

---

## PASO 1: Agregar Autenticación al Backend

El backend actualmente NO tiene autenticación. Se debe implementar antes de la web.

### 1.1 Agregar campo PasswordHash a la entidad Dentista

**Archivo:** `AgendaDentista.Dominio/Entidades/Dentista.cs`

Agregar esta propiedad a la clase Dentista:
```csharp
public string PasswordHash { get; set; } = string.Empty;
```

### 1.2 Crear DTOs de autenticación

**Archivo:** `AgendaDentista.Aplicacion/DTOs/Auth/LoginDto.cs`
```csharp
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

**Archivo:** `AgendaDentista.Aplicacion/DTOs/Auth/RegistroDto.cs`
```csharp
public class RegistroDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

**Archivo:** `AgendaDentista.Aplicacion/DTOs/Auth/AuthResponseDto.cs`
```csharp
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int IdDentista { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

### 1.3 Crear interfaz y servicio de autenticación

**Archivo:** `AgendaDentista.Aplicacion/Interfaces/IAuthServicio.cs`
```csharp
public interface IAuthServicio
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegistroAsync(RegistroDto dto);
}
```

El servicio debe:
- Usar `BCrypt.Net-Next` para hashear passwords
- Generar JWT tokens con `System.IdentityModel.Tokens.Jwt`
- Incluir `IdDentista` y `Email` como claims en el token
- Token expira en 24 horas

### 1.4 Crear AuthController

**Archivo:** `AgendaDentista.API/Controllers/AuthController.cs`

Dos endpoints SIN `[Authorize]`:
- `POST /api/auth/login`
- `POST /api/auth/registro`

### 1.5 Configurar JWT en Program.cs

Instalar paquete: `Microsoft.AspNetCore.Authentication.JwtBearer`

Agregar en `appsettings.json`:
```json
{
  "Jwt": {
    "Key": "UnaClaveSecretaMuyLargaParaElToken2024AgendaDentista!",
    "Issuer": "AgendaDentista",
    "Audience": "AgendaDentistaWeb",
    "ExpireHours": 24
  }
}
```

Agregar en `Program.cs` antes de `builder.Build()`:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* configurar validación del token */ });
```

Agregar en el pipeline después de `UseCors()`:
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### 1.6 Proteger los controladores existentes

Agregar `[Authorize]` a:
- `DentistasController` (excepto crear si se usa registro aparte)
- `PacientesController` (todos los endpoints)
- `CitasController` (todos los endpoints)

NO proteger:
- `AuthController` (login y registro deben ser públicos)
- `WebhookWhatsAppController` (WhatsApp debe poder enviar mensajes)

### 1.7 Crear nueva migración

```bash
dotnet ef migrations add AgregarPasswordDentista --project AgendaDentista.Infraestructura --startup-project AgendaDentista.API
dotnet ef database update --startup-project AgendaDentista.API
```

---

## PASO 2: Documentación Completa de Endpoints

**Base URL:** `http://localhost:5174/api`

Todos los endpoints protegidos requieren header:
```
Authorization: Bearer {token_jwt}
```

---

### Auth (públicos, sin token)

#### POST /api/auth/registro
Registra un nuevo dentista con contraseña.

**Request:**
```json
{
  "nombre": "Dr. García López",
  "telefono": "5512345678",
  "email": "garcia@dental.com",
  "password": "MiPassword123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "idDentista": 1,
  "nombre": "Dr. García López",
  "email": "garcia@dental.com"
}
```

#### POST /api/auth/login
Inicia sesión y obtiene un token JWT.

**Request:**
```json
{
  "email": "garcia@dental.com",
  "password": "MiPassword123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "idDentista": 1,
  "nombre": "Dr. García López",
  "email": "garcia@dental.com"
}
```

**Response 401:**
```json
{
  "error": "Credenciales inválidas."
}
```

---

### Dentistas (protegidos con token)

#### POST /api/dentistas
Crea un nuevo dentista (sin password, usado internamente).

**Request:**
```json
{
  "nombre": "Dra. Martínez",
  "telefono": "5598765432",
  "email": "martinez@dental.com"
}
```

**Response 201:**
```json
{
  "idDentista": 2,
  "nombre": "Dra. Martínez",
  "telefono": "+5215598765432",
  "email": "martinez@dental.com",
  "fechaRegistro": "2026-03-05T04:00:00Z",
  "activo": true
}
```

#### GET /api/dentistas
Obtiene todos los dentistas.

**Response 200:**
```json
[
  {
    "idDentista": 1,
    "nombre": "Dr. García López",
    "telefono": "+5215512345678",
    "email": "garcia@dental.com",
    "fechaRegistro": "2026-03-05T04:00:00Z",
    "activo": true
  }
]
```

#### GET /api/dentistas/{id}
Obtiene un dentista por su ID.

**Response 200:**
```json
{
  "idDentista": 1,
  "nombre": "Dr. García López",
  "telefono": "+5215512345678",
  "email": "garcia@dental.com",
  "fechaRegistro": "2026-03-05T04:00:00Z",
  "activo": true
}
```

**Response 404:** si no existe.

---

### Pacientes (protegidos con token)

#### POST /api/pacientes
Crea un nuevo paciente asociado a un dentista.

**Request:**
```json
{
  "idDentista": 1,
  "nombre": "Juan Pérez Sánchez",
  "telefono": "5511223344",
  "email": "juan@email.com"
}
```

**Response 201:**
```json
{
  "idPaciente": 1,
  "idDentista": 1,
  "nombre": "Juan Pérez Sánchez",
  "telefono": "+5215511223344",
  "email": "juan@email.com",
  "fechaRegistro": "2026-03-05T04:10:00Z",
  "activo": true
}
```

**Response 404:** si el dentista no existe.

#### GET /api/pacientes?idDentista={id}
Obtiene todos los pacientes de un dentista.

**Parámetro query:** `idDentista` (requerido, int)

**Response 200:**
```json
[
  {
    "idPaciente": 1,
    "idDentista": 1,
    "nombre": "Juan Pérez Sánchez",
    "telefono": "+5215511223344",
    "email": "juan@email.com",
    "fechaRegistro": "2026-03-05T04:10:00Z",
    "activo": true
  },
  {
    "idPaciente": 2,
    "idDentista": 1,
    "nombre": "María López",
    "telefono": "+5215599887766",
    "email": null,
    "fechaRegistro": "2026-03-05T04:15:00Z",
    "activo": true
  }
]
```

#### GET /api/pacientes/{id}
Obtiene un paciente por su ID.

**Response 200:**
```json
{
  "idPaciente": 1,
  "idDentista": 1,
  "nombre": "Juan Pérez Sánchez",
  "telefono": "+5215511223344",
  "email": "juan@email.com",
  "fechaRegistro": "2026-03-05T04:10:00Z",
  "activo": true
}
```

---

### Citas (protegidos con token)

#### POST /api/citas
Crea una nueva cita. La fecha debe ser futura.

**Request:**
```json
{
  "idPaciente": 1,
  "idDentista": 1,
  "fechaHora": "2026-03-10T10:00:00",
  "tratamiento": "Limpieza dental profunda"
}
```

**Response 201:**
```json
{
  "idCita": 1,
  "idPaciente": 1,
  "idDentista": 1,
  "nombrePaciente": "Juan Pérez Sánchez",
  "nombreDentista": "Dr. García López",
  "fechaHora": "2026-03-10T10:00:00",
  "tratamiento": "Limpieza dental profunda",
  "estado": 0,
  "confirmado": false,
  "recordatorioEnviado": false,
  "fechaCreacion": "2026-03-05T04:20:00Z",
  "fechaActualizacion": null
}
```

**Response 400:** `"La fecha de la cita debe ser futura."`
**Response 404:** si paciente o dentista no existen.

#### GET /api/citas/{id}
Obtiene una cita por su ID.

**Response 200:**
```json
{
  "idCita": 1,
  "idPaciente": 1,
  "idDentista": 1,
  "nombrePaciente": "Juan Pérez Sánchez",
  "nombreDentista": "Dr. García López",
  "fechaHora": "2026-03-10T10:00:00",
  "tratamiento": "Limpieza dental profunda",
  "estado": 1,
  "confirmado": true,
  "recordatorioEnviado": true,
  "fechaCreacion": "2026-03-05T04:20:00Z",
  "fechaActualizacion": "2026-03-09T14:30:00Z"
}
```

#### GET /api/citas?idDentista={id}&fecha={fecha}
Obtiene la agenda de un dentista. La fecha es opcional.

**Parámetros query:**
- `idDentista` (requerido, int) — ID del dentista
- `fecha` (opcional, string formato `2026-03-10`) — filtra por día específico. Si se omite, trae todas las citas.

**Response 200:**
```json
[
  {
    "idCita": 1,
    "idPaciente": 1,
    "idDentista": 1,
    "nombrePaciente": "Juan Pérez Sánchez",
    "nombreDentista": "Dr. García López",
    "fechaHora": "2026-03-10T10:00:00",
    "tratamiento": "Limpieza dental profunda",
    "estado": 0,
    "confirmado": false,
    "recordatorioEnviado": false,
    "fechaCreacion": "2026-03-05T04:20:00Z",
    "fechaActualizacion": null
  },
  {
    "idCita": 2,
    "idPaciente": 2,
    "idDentista": 1,
    "nombrePaciente": "María López",
    "nombreDentista": "Dr. García López",
    "fechaHora": "2026-03-10T14:30:00",
    "tratamiento": "Extracción de muela",
    "estado": 1,
    "confirmado": true,
    "recordatorioEnviado": true,
    "fechaCreacion": "2026-03-05T04:25:00Z",
    "fechaActualizacion": "2026-03-09T18:00:00Z"
  }
]
```

#### PUT /api/citas/{id}/confirmar
Confirma una cita manualmente. Sin body.

**Response 200:**
```json
{
  "idCita": 1,
  "estado": 1,
  "confirmado": true,
  "fechaActualizacion": "2026-03-05T05:00:00Z"
}
```

#### PUT /api/citas/{id}/cancelar
Cancela una cita manualmente. Sin body.

**Response 200:**
```json
{
  "idCita": 1,
  "estado": 2,
  "confirmado": false,
  "fechaActualizacion": "2026-03-05T05:00:00Z"
}
```

#### PUT /api/citas/{id}/estado
Actualiza el estado de una cita (ej: marcar como Atendida).

**Request:**
```json
{
  "nuevoEstado": 3
}
```

**Response 200:** la cita actualizada completa.

---

### Webhook WhatsApp (públicos, sin token)

#### GET /api/webhook/whatsapp
Verificación del webhook por Meta/WhatsApp.

**Parámetros query:**
- `hub.mode` = "subscribe"
- `hub.verify_token` = el token configurado en appsettings.json
- `hub.challenge` = string de verificación

**Response 200:** retorna el valor de `hub.challenge`
**Response 403:** si el token no coincide

#### POST /api/webhook/whatsapp
Recibe mensajes entrantes de WhatsApp. Retorna 200 inmediatamente y procesa en background.

**Request (enviado por Meta automáticamente):**
```json
{
  "object": "whatsapp_business_account",
  "entry": [
    {
      "id": "WHATSAPP_BUSINESS_ACCOUNT_ID",
      "changes": [
        {
          "value": {
            "messaging_product": "whatsapp",
            "messages": [
              {
                "id": "wamid.xxx",
                "from": "5215512345678",
                "timestamp": "1709600000",
                "type": "text",
                "text": {
                  "body": "1"
                }
              }
            ],
            "contacts": [
              {
                "wa_id": "5215512345678",
                "profile": {
                  "name": "Juan Pérez"
                }
              }
            ]
          }
        }
      ]
    }
  ]
}
```

**Response 200:** siempre retorna 200 (requisito de WhatsApp)

**Lógica interna:** Si el paciente responde "1", "confirmar" o "sí" → la cita se marca como Confirmada. Si responde "2" o "cancelar" → se marca como Cancelada.

---

## Enums y Valores

### EstadoCita
| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | Pendiente | Cita creada, sin confirmar |
| 1 | Confirmada | Paciente confirmó asistencia |
| 2 | Cancelada | Cita cancelada |
| 3 | Atendida | Paciente fue atendido |

### TipoRecordatorio
| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | Horas24 | Recordatorio 24 horas antes |
| 1 | Horas3 | Recordatorio 3 horas antes |

### EstadoEnvio
| Valor | Nombre |
|-------|--------|
| 0 | Pendiente |
| 1 | Enviado |
| 2 | Fallido |
| 3 | Entregado |
| 4 | Leido |

---

## PASO 3: Especificación de la Aplicación Web React

### Stack tecnológico
- **React 18** con Vite
- **TailwindCSS** para estilos
- **React Router v6** para navegación
- **Axios** para llamadas HTTP
- **date-fns** para manejo de fechas
- **react-hot-toast** para notificaciones
- **lucide-react** para iconos

### Diseño visual profesional

**Paleta de colores dental:**
- Primary: `#0EA5E9` (sky-500, azul dental profesional)
- Primary dark: `#0284C7` (sky-600)
- Secondary: `#10B981` (emerald-500, confirmaciones)
- Danger: `#EF4444` (red-500, cancelaciones)
- Warning: `#F59E0B` (amber-500, pendientes)
- Background: `#F8FAFC` (slate-50)
- Sidebar: `#0F172A` (slate-900, dark)
- Cards: `#FFFFFF` con shadow-sm y border border-slate-200

**Tipografía:** Inter (Google Fonts)

**Layout:** Sidebar fija a la izquierda (250px) + contenido principal con header

### Páginas de la aplicación

#### 1. Login (`/login`)
- Formulario centrado en pantalla con logo
- Campos: email y password
- Botón "Iniciar sesión"
- Link "¿No tienes cuenta? Regístrate"
- Fondo con gradiente sutil azul/blanco
- Validación de campos vacíos

#### 2. Registro (`/registro`)
- Similar al login pero con campos adicionales: nombre, teléfono, email, password
- Botón "Crear cuenta"
- Link "¿Ya tienes cuenta? Inicia sesión"

#### 3. Dashboard (`/` o `/dashboard`)
Pantalla principal después del login.

**Contenido:**
- **Header:** "Buenos días, Dr. García" con fecha actual
- **4 cards de estadísticas:**
  - Citas hoy (total)
  - Confirmadas (verde)
  - Pendientes (amarillo)
  - Canceladas (rojo)
- **Agenda del día:** lista de citas de hoy ordenadas por hora
  - Cada cita muestra: hora, nombre paciente, tratamiento, estado (badge de color)
  - Botones de acción: confirmar, cancelar, marcar atendida
- **Próximas citas:** las siguientes 5 citas futuras

#### 4. Pacientes (`/pacientes`)
**Contenido:**
- **Header:** "Pacientes" + botón "Nuevo paciente"
- **Barra de búsqueda** para filtrar por nombre
- **Tabla de pacientes:**
  - Columnas: nombre, teléfono, email, fecha registro, acciones
  - Acciones: ver citas del paciente
- **Modal "Nuevo paciente":**
  - Campos: nombre, teléfono, email (opcional)
  - Botón guardar y cancelar

#### 5. Nueva Cita (`/citas/nueva`)
**Contenido:**
- **Formulario:**
  - Seleccionar paciente (dropdown con búsqueda, cargado de GET /api/pacientes?idDentista={id})
  - Fecha y hora (date picker + time picker)
  - Tratamiento (input de texto)
  - Botón "Agendar cita"
- Validación: fecha futura, campos requeridos

#### 6. Agenda (`/agenda`)
**Contenido:**
- **Selector de fecha** (calendario o navegación por día/semana)
- **Vista de día:** lista de citas del día seleccionado
- Cada cita como card con:
  - Hora prominente
  - Nombre del paciente
  - Tratamiento
  - Badge de estado con color
  - Botones: confirmar | cancelar | atendida
- **Estado vacío:** "No hay citas para este día" con botón "Agendar cita"

### Componentes reutilizables

| Componente | Descripción |
|------------|-------------|
| `Layout` | Sidebar + Header + contenido (children) |
| `Sidebar` | Logo, navegación (Dashboard, Pacientes, Agenda, Nueva Cita), botón cerrar sesión |
| `Header` | Nombre de la página actual + nombre del dentista |
| `EstadoBadge` | Badge de color según estado: verde=confirmada, amarillo=pendiente, rojo=cancelada, azul=atendida |
| `CitaCard` | Card de cita con info + botones de acción |
| `StatsCard` | Card de estadística con icono, número y label |
| `Modal` | Modal reutilizable para formularios |
| `Input` | Input estilizado con label y error |
| `Button` | Botón con variantes: primary, secondary, danger, outline |
| `LoadingSpinner` | Indicador de carga |
| `EmptyState` | Mensaje cuando no hay datos |

### Flujo de autenticación

1. Usuario entra a la app → verificar si hay token en `localStorage`
2. Si NO hay token → redirigir a `/login`
3. Si HAY token → verificar que no haya expirado (decodificar JWT)
4. Login exitoso → guardar `token`, `idDentista`, `nombre` en `localStorage`
5. Crear interceptor Axios que agrega `Authorization: Bearer {token}` a todas las peticiones
6. Si una petición retorna 401 → limpiar `localStorage` y redirigir a `/login`
7. Botón "Cerrar sesión" → limpiar `localStorage` y redirigir a `/login`

### Estructura de archivos React

```
src/
├── main.jsx
├── App.jsx                    (rutas)
├── api/
│   └── axios.js               (instancia axios con interceptor)
├── context/
│   └── AuthContext.jsx         (contexto de autenticación)
├── pages/
│   ├── Login.jsx
│   ├── Registro.jsx
│   ├── Dashboard.jsx
│   ├── Pacientes.jsx
│   ├── NuevaCita.jsx
│   └── Agenda.jsx
├── components/
│   ├── Layout.jsx
│   ├── Sidebar.jsx
│   ├── Header.jsx
│   ├── EstadoBadge.jsx
│   ├── CitaCard.jsx
│   ├── StatsCard.jsx
│   ├── Modal.jsx
│   ├── Input.jsx
│   ├── Button.jsx
│   ├── LoadingSpinner.jsx
│   └── EmptyState.jsx
└── utils/
    └── formatters.js           (formatear fechas, estados, teléfonos)
```

---

## Ejecución del Backend

### Prerrequisitos
- .NET 8 SDK
- Docker Desktop (para SQL Server)
- dotnet-ef tool: `dotnet tool install --global dotnet-ef`

### Levantar SQL Server con Docker
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=TuPassword123!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### Aplicar migraciones
```bash
cd AgendaDentista
dotnet ef database update --startup-project AgendaDentista.API
```

### Ejecutar la API
```bash
dotnet run --project AgendaDentista.API
```

La API estará disponible en `http://localhost:5174` con Swagger en `/swagger`.

### Ejecutar el Worker de recordatorios (en otra terminal)
```bash
dotnet run --project AgendaDentista.WorkerRecordatorios
```

### Cadena de conexión actual
```
Server=localhost,1433;Database=AgendaDentista;User Id=sa;Password=TuPassword123!;TrustServerCertificate=true;
```

---

## Errores comunes de la API

| Código | Significado | Ejemplo |
|--------|-------------|---------|
| 200 | OK | GET exitoso, PUT exitoso |
| 201 | Creado | POST exitoso (incluye header Location) |
| 400 | Validación | "La fecha de la cita debe ser futura." |
| 401 | No autenticado | Token JWT faltante o expirado |
| 404 | No encontrado | "Dentista con ID 99 no fue encontrado." |
| 502 | Error WhatsApp | Error comunicándose con WhatsApp Cloud API |
| 500 | Error interno | Error no manejado del servidor |

Formato de error:
```json
{
  "error": "Mensaje descriptivo del error"
}
```
