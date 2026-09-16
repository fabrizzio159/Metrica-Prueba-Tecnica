# Sistema de Carga Masiva — Microservicios .NET 9

Sistema de microservicios para carga masiva de archivos Excel con procesamiento asíncrono, persistencia en SQL Server y notificaciones por correo electrónico.

## Arquitectura

```
┌─────────────┐     ┌──────────────┐     ┌───────────────────┐
│   Cliente    │────▶│  API Gateway │────▶│   AuthService     │
│  (Postman)   │     │   (YARP)     │     │  JWT + BCrypt     │
└─────────────┘     │  Rate Limit  │     └───────────────────┘
                    │              │     ┌───────────────────┐
                    │              │────▶│  ControlService   │
                    └──────────────┘     │  Upload + Track   │
                                        └────────┬──────────┘
                                                 │ RabbitMQ
                                        ┌────────▼──────────┐
                                        │ CargaMasivaService│
                                        │  Excel Processing │
                                        └────────┬──────────┘
                                                 │ RabbitMQ
                                        ┌────────▼──────────┐
                                        │NotificacionService│
                                        │  Email (MailKit)  │
                                        └───────────────────┘
```

**Infraestructura:** SQL Server 2022 | RabbitMQ 3 | SeaweedFS (almacenamiento distribuido)

## Stack Tecnológico

| Componente | Tecnología |
|---|---|
| Framework | .NET 9 |
| ORM | Entity Framework Core |
| Base de Datos | SQL Server 2022 |
| Cola de Mensajes | RabbitMQ |
| Almacenamiento | SeaweedFS |
| Gateway | YARP Reverse Proxy |
| Autenticación | JWT Bearer + Refresh Tokens |
| Hashing | BCrypt |
| Excel | ClosedXML |
| Email | MailKit |
| Logging | Serilog |
| Contenedores | Docker + Docker Compose |

## Microservicios

### API Gateway (puerto 5000)
- Reverse proxy con YARP
- Validación JWT centralizada
- Rate Limiting (Fixed Window: 100 req/60s por IP)
- CORS habilitado

### AuthService
- Login con email/password
- Generación de JWT (2h expiración) con claims: userId, email, role
- Refresh Tokens (7 días expiración)
- Password hashing con BCrypt
- Usuario seed: `admin@cargamasiva.com` / `Admin123!`

### ControlService
- Upload de archivos Excel (.xlsx, máximo 10MB configurable)
- Almacenamiento en SeaweedFS
- Publicación a cola RabbitMQ `carga_masiva`
- Historial de cargas por usuario
- Requiere autenticación JWT

### CargaMasivaService (Worker)
- Consume cola `carga_masiva`
- Descarga archivo de SeaweedFS
- Procesa Excel con ClosedXML
- Validaciones de negocio:
  - **Periodo duplicado**: rechaza si existe carga Cargado/Finalizado/Notificado
  - **Periodo activo**: bloquea si existe carga Pendiente/EnProceso
  - **CodigoProducto duplicado**: registra en auditoría, no inserta
  - **Filas vacías**: se ignoran
  - **Columnas vacías**: valor por defecto
- Estados: Pendiente → EnProceso → Cargado → Finalizado
- Publica a cola `notificaciones`

### NotificacionService (Worker)
- Consume cola `notificaciones`
- Envía email HTML con resumen de la carga via MailKit
- Actualiza estado a `Notificado`

## Formato del Excel

| Columna | Tipo | Requerido |
|---|---|---|
| Periodo | texto | Sí |
| CodigoProducto | texto | Sí |
| NombreProducto | texto | Sí |
| Precio | decimal | Sí |

## Stored Procedures

| SP | Descripción |
|---|---|
| `sp_ActualizarEstadoCarga` | Actualiza estado y fecha fin |
| `sp_ValidarPeriodo` | Verifica cargas existentes por periodo |
| `sp_InsertarDataProcesada` | Inserta dato validando duplicado por CodigoProducto |
| `sp_ObtenerHistorialCargas` | Lista cargas con paginación |
| `sp_InsertarAuditoriaFallo` | Registra fallo de auditoría |

## Despliegue

### Prerrequisitos
- Docker y Docker Compose

### Levantar todos los servicios

```bash
docker-compose up --build
```

### Configurar SMTP (NotificacionService)

Editar las variables de entorno en `docker-compose.yml`:

```yaml
- SMTP__Host=smtp.gmail.com
- SMTP__Port=587
- SMTP__User=tu_correo@gmail.com
- SMTP__Password=tu_app_password
- SMTP__From=tu_correo@gmail.com
```

### Servicios expuestos

| Servicio | URL |
|---|---|
| API Gateway | http://localhost:5000 |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| SQL Server | localhost:1433 (sa/SqlServer2024!) |
| SeaweedFS Master | http://localhost:9333 |

## Endpoints

### Autenticación
```
POST /api/auth/login        — Login (retorna JWT + Refresh Token)
POST /api/auth/refresh       — Renovar token
```

### Carga de Archivos (requiere JWT)
```
POST /api/carga/upload       — Subir archivo Excel (multipart/form-data)
GET  /api/carga/historial    — Historial de cargas del usuario
GET  /api/carga/{id}         — Detalle de una carga
```

## Pruebas con Postman

1. Importar `postman/PruebaTecnica.postman_collection.json`
2. Ejecutar **Login** — el token se guarda automáticamente en variables de colección
3. Ejecutar **Upload Excel** — seleccionar archivo .xlsx
4. Ejecutar **Historial de Cargas** — verificar el estado de la carga

## Patrón Arquitectónico

```
Controller → Interface → Service → Repository → Model
```

- **Repositorio Genérico**: `IRepository<T>` / `Repository<T>` replicado en cada microservicio
- **Migraciones Automáticas**: cada servicio ejecuta `Database.Migrate()` al iniciar
- **Exception Middleware**: manejo global de excepciones con respuestas estandarizadas
- **Logging Estructurado**: Serilog con output a consola

## Estructura del Proyecto

```
├── docker-compose.yml
├── src/
│   ├── ApiGateway/          — YARP + JWT + Rate Limiting
│   ├── AuthService/         — Autenticación JWT + BCrypt
│   ├── ControlService/      — Upload + SeaweedFS + RabbitMQ Publisher
│   ├── CargaMasivaService/  — Worker: Excel Processing + Validaciones
│   └── NotificacionService/ — Worker: Email con MailKit
└── postman/
    └── PruebaTecnica.postman_collection.json
```

Cada microservicio tiene su propio `.sln` para builds Docker independientes.
