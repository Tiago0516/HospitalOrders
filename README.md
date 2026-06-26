# HospitalOrders

API REST para gestión de órdenes médicas hospitalarias, construida con **.NET 8**, **Clean Architecture**, **CQRS** y procesamiento asíncrono mediante un **Worker Service** con cola en memoria.

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server accesible (local, Docker o Azure)
- `dotnet-ef` instalado globalmente:

```bash
dotnet tool install --global dotnet-ef
```

---

## Configuración

### Cadena de conexión

Edita `HospitalOrders.Api/appsettings.json` y `HospitalOrders.Worker/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "TuCONEXION(Server=localhost,1433;Database=HospitalOrdersDb;User Id=sa;Password=TuPassword;Encrypt=True;TrustServerCertificate=True;)"
}
```

### JWT

La autenticación usa JWT Bearer. La configuración está en `appsettings.json`:

```json
"Jwt": {
  "Key": "HospitalOrders-SuperSecretKey-2024-MustBe32CharsOrMore!",
  "Issuer": "HospitalOrders.Api",
  "Audience": "HospitalOrders.Client",
  "TestUser": "admin",
  "TestPassword": "Admin123!"
}
```

> En producción reemplaza estos valores con variables de entorno o un gestor de secretos.

---

## Pasos de ejecución

### 1. Clonar y restaurar paquetes

```bash
git clone <url-del-repositorio>
cd HospitalOrders
dotnet restore
```

### 2. Crear la base de datos y aplicar migraciones

```bash
dotnet ef migrations add InitialCreate \
  --project HospitalOrders.Infrastructure \
  --startup-project HospitalOrders.Api

dotnet ef database update \
  --project HospitalOrders.Infrastructure \
  --startup-project HospitalOrders.Api
```

### 3. Ejecutar la API (incluye el Worker)

```bash
dotnet run --project HospitalOrders.Api
```

La API queda disponible en `http://localhost:5210`.  
Swagger UI accesible en `http://localhost:5210` (ruta raíz).

> El `OrderProcessingWorker` arranca automáticamente junto a la API como un `IHostedService`. Esto es necesario porque la cola usa `System.Threading.Channels` (memoria compartida) — ambos componentes deben correr en el mismo proceso para compartir la misma instancia del canal. El Worker vive en el proyecto `HospitalOrders.Worker` (ensamblado separado) pero se registra en el host de la API. Al iniciar verás en consola: `Worker iniciado. Escuchando la cola de órdenes...`

---

## Endpoints


| Método | Ruta               | Auth | Descripción                        |
| ------ | ------------------ | ---- | ---------------------------------- |
| `POST` | `/api/auth/token`  | No   | Obtiene el token JWT               |
| `POST` | `/api/orders`      | Sí   | Crea una nueva orden médica        |
| `GET`  | `/api/orders`      | Sí   | Lista órdenes (filtros opcionales) |
| `GET`  | `/api/orders/{id}` | Sí   | Detalle de una orden por Id        |


### Obtener token

```http
POST /api/auth/token
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin123!"
}
```

### Crear orden

```http
POST /api/orders
Authorization: Bearer <token>
Content-Type: application/json

{
  "patientId": "12345",
  "patientName": "Juan Perez",
  "serviceCode": "LAB001",
  "serviceDescription": "Hemograma",
  "priority": "Normal"
}
```

Valores válidos para `priority`: `Normal` | `Urgente`

### Filtros en GET /api/orders

```
GET /api/orders?patientId=12345
GET /api/orders?status=Pending
GET /api/orders?patientId=12345&status=Processed
```

Valores válidos para `status`: `Pending` | `Processing` | `Processed` | `Failed`

---

## Estados de una orden


| Estado       | Descripción                                      |
| ------------ | ------------------------------------------------ |
| `Pending`    | Orden creada, esperando en la cola               |
| `Processing` | Worker tomó la orden y está procesando           |
| `Processed`  | Procesamiento completado exitosamente            |
| `Failed`     | Error durante el procesamiento (no se reintenta) |


---

## Autenticación

La API usa **JWT Bearer Token**. Para acceder a los endpoints protegidos:

1. Llama a `POST /api/auth/token` con las credenciales
2. Copia el `token` de la respuesta
3. En Swagger: haz clic en **Authorize** e ingresa `Bearer <token>`
4. En clientes HTTP: agrega el header `Authorization: Bearer <token>`

---

## Logs

Los logs se generan en consola y en archivos con rotación diaria:


| Componente | Ruta del archivo                                 |
| ---------- | ------------------------------------------------ |
| API        | `HospitalOrders.Api/logs/api-YYYYMMDD.log`       |
| Worker     | `HospitalOrders.Worker/logs/worker-YYYYMMDD.log` |


Se registran los siguientes eventos:

- Creación de cada orden (Id, PatientId, ServiceCode, Priority)
- Cambios de estado durante el procesamiento
- Errores y excepciones con stack trace completo
- Intentos de autenticación fallidos

---

## Ejecutar tests

```bash
dotnet test HospitalOrders.Tests/HospitalOrders.Tests.csproj
```

Resultado esperado: **11 tests, 0 fallidos**.

Los tests cubren:

- Creación de orden válida
- Rechazo de orden con PatientId vacío
- Rechazo de orden con ServiceCode vacío
- Rechazo de prioridad inválida
- Transición de estados (Pending → Processing → Processed)
- Marcado como fallida con razón
- Handler: orden válida guarda en BD y encola
- Handler: orden inválida no toca BD ni cola

---

## Script DDL

Las migraciones de EF Core generan el DDL automáticamente. Si necesitas el script SQL puro:

```bash
dotnet ef migrations script \
  --project HospitalOrders.Infrastructure \
  --startup-project HospitalOrders.Api \
  --output ddl.sql
```

Esto genera el archivo `ddl.sql` con todo el SQL necesario para crear la base de datos desde cero.