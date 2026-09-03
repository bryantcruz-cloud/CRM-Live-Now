# CRM Live Now

Sistema CRM/ERP especializado para Live Now, empresa dedicada a la venta y gestión de entradas y paquetes para maratones internacionales.

## Arquitectura

El proyecto sigue una arquitectura en capas con separación clara de responsabilidades:

```
CRM Live Now
├── LiveNow.CRM              → Aplicación Windows WinUI 3 (Frontend)
├── LiveNow.CRM.API          → ASP.NET Core Web API (Backend)
├── LiveNow.CRM.Core         → Entidades, DTOs, Interfaces y Reglas de Negocio
├── LiveNow.CRM.Infrastructure → Entity Framework Core y Acceso a Datos
└── LiveNow.CRM.Tests        → Pruebas Unitarias e Integración
```

## Especificaciones Técnicas

- **Plataforma:** Windows
- **Frontend:** WinUI 3 / Windows App SDK (.NET 8 LTS)
- **Backend:** ASP.NET Core Web API 8.0
- **Base de Datos:**
  - Desarrollo: SQLite
  - Producción: SQL Server o PostgreSQL
- **ORM:** Entity Framework Core 8.0
- **Patrón:** MVVM (en aplicación WinUI)
- **Análisis de código:** Habilitado con `TreatWarningsAsErrors`

## Proyectos

### LiveNow.CRM (Frontend)
Aplicación de escritorio nativa con WinUI 3 que se comunica con la API mediante HTTP/HTTPS.

```
LiveNow.CRM/
├── ViewModels/      → Lógica de presentación (MVVM)
├── Views/           → Páginas XAML
├── Services/        → Servicios de cliente HTTP
├── Helpers/         → Utilidades
├── App.xaml         → Punto de entrada
└── MainWindow.xaml  → Ventana principal
```

### LiveNow.CRM.API (Backend)
Web API que actúa como intermediario entre la aplicación Windows y la base de datos.

```
LiveNow.CRM.API/
├── Controllers/     → Controladores REST
├── Middlewares/     → Middlewares personalizados
├── Extensions/      → Métodos de extensión
├── Program.cs       → Configuración de la aplicación
└── appsettings.json → Configuración
```

### LiveNow.CRM.Core (Dominio)
Contiene las entidades, enumeraciones, DTOs e interfaces independientes de infraestructura.

```
LiveNow.CRM.Core/
├── Common/          → Clases base (BaseEntity)
├── Entities/        → Entidades del dominio
├── Enums/           → Enumeraciones
├── Interfaces/      → Contratos (IRepository, IUnitOfWork)
└── DTOs/            → Objetos de transferencia de datos
```

### LiveNow.CRM.Infrastructure (Infraestructura)
Implementa el acceso a datos con Entity Framework Core.

```
LiveNow.CRM.Infrastructure/
├── Data/            → DbContext y UnitOfWork
├── Configurations/  → Configuraciones de EF Core
├── Repositories/    → Implementaciones de repositorios
└── DependencyInjection.cs → Registro de servicios
```

### LiveNow.CRM.Tests (Pruebas)
Pruebas unitarias e integración con xUnit.

```
LiveNow.CRM.Tests/
├── Unit/            → Pruebas unitarias
└── Integration/     → Pruebas de integración
```

## Configuración de Base de Datos

El sistema soporta múltiples proveedores de base de datos configurables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=livenow.db"
  },
  "DatabaseProvider": "sqlite"  // Opciones: sqlite, sqlserver, postgresql
}
```

## Modelo de Dominio (Fase 2)

### Entidades

| Entidad | Descripción |
|---------|-------------|
| `Customer` | Clientes con información de contacto y estatus. |
| `Race` | Carrera permanente (Chicago Marathon, Berlin Marathon, etc.). Configurable, sin casos especiales codificados. |
| `RaceEdition` | Edición de carrera por año. Restricción única `Race + Year`. |
| `RaceSlot` | Plaza/entrada individual del inventario. Restricción única `InternalCode`. Trazable por su historial (venta, transferencia, cancelación). |
| `Supplier` | Proveedor de entradas, hoteles u otros servicios. |
| `Quote` / `QuoteItem` | Cotización y su detalle (Entrada, Hotel, Transporte, Seguro, Tour, Asistencia, Otros). |
| `Sale` / `SaleItem` | Venta confirmada y su detalle. |
| `Payment` / `PaymentFee` | Pagos del cliente y comisiones/costos del método de pago (porcentual, fijo, porcentual + fijo, sin comisión). |
| `Hotel` / `HotelReservation` | Catálogo de hoteles y reservas vinculadas a cliente/venta. |
| `RunnerRegistration` | Registro operativo del corredor (Pending, InProgress, Completed, Cancelled). |
| `CustomerChecklist` | Checklist operativo por cliente/venta. Elementos configurables. |
| `Cancellation` | Cancelaciones con razón, indicador de lesión y posibilidad de reasignar plaza. |
| `SlotTransfer` | Transferencia/reasignación de plazas entre clientes. |
| `AuditLog` | Auditoría de operaciones (Entity, Action, OldValues, NewValues, Timestamp). |

### Enums principales

| Enum | Valores |
|------|---------|
| `SlotStatusEnum` | Available, Reserved, Sold, Registered, Cancelled, Injured, Transferable, Transferred, Lost |
| `PaymentMethodEnum` | Card, BankTransfer, Cash, Deposit, Other |
| `PaymentFeeTypeEnum` | Percentage, FixedAmount, PercentagePlusFixed, NoFee |
| `QuoteItemTypeEnum` / `SaleItemTypeEnum` | Entry, Hotel, Transport, Insurance, Tour, Assistance, Other |
| `CurrencyEnum` | USD, EUR, MXN, GBP, JPY, CAD |

### Conceptos financieros

El modelo distingue explícitamente (sin mezclarlos):

- **Precio de venta**: `Sale.TotalSalePrice` / `SaleItem.TotalPrice`
- **Costo de adquisición**: `RaceSlot.AcquisitionCost` / `SaleItem.TotalCost`
- **Comisión de pago**: `PaymentFee.CalculatedAmount`
- **Pago recibido**: suma de `Payment.Amount` con `Status == Completed`
- **Saldo pendiente**: `Sale.TotalSalePrice - (suma de pagos válidos)`
- **Ganancia bruta**: `Sale.GrossProfit = TotalSalePrice - TotalCost - TotalPaymentFees`
- **Margen**: `Sale.ProfitMargin = GrossProfit / TotalSalePrice`

Todos los montos usan `decimal` (precisión 18,2 / 18,4 para márgenes). **Nunca se usa `float`/`double` para dinero.**

Los campos de ganancia (`TotalCost`, `TotalPaymentFees`, `GrossProfit`, `ProfitMargin`) son **recalculables** a partir de los items, pagos y fees para mantenerse consistentes si cambian los datos de origen.

### Estructura de EF Core

```
LiveNow.CRM.Infrastructure/
├── Configurations/   → Configuración por entidad (PK, FK, índices, únicos, precisión decimal, delete behaviors)
├── Data/
│   ├── LiveNowDbContext.cs  → DbSets y aplicación de configuraciones + seed
│   ├── SeedData.cs          → Datos seed de desarrollo (Chicago, Berlín y sus ediciones 2026)
│   └── UnitOfWork.cs        → Transacciones
├── Migrations/       → Migraciones de EF Core
└── DependencyInjection.cs   → Registro de servicios (sqlite/sqlserver/postgresql)
```

### Migraciones

```bash
# Crear una migración
cd LiveNow.CRM.Infrastructure
dotnet ef migrations add <Nombre> --startup-project ..\LiveNow.CRM.API

# Aplicar la migración a la base de datos local
dotnet ef database update --startup-project ..\LiveNow.CRM.API

# Revertir la última migración
dotnet ef migrations remove --startup-project ..\LiveNow.CRM.API
```

La primera migración es `InitialCreate` (crea todas las tablas + datos seed de desarrollo).

> ⚠️ La base de datos SQLite generada en desarrollo (`*.db`) está ignorada por Git.

## Módulos Planificados

- Dashboard
- Clientes
- Leads
- Cotizaciones
- Ventas
- Pagos
- Cuentas por cobrar
- Inventario de entradas/plazas
- Carreras (Chicago Marathon, Berlin Marathon, etc.)
- Hoteles
- Registro de corredores
- Checklist operativo
- Lesiones
- Cancelaciones
- Reasignación de plazas
- Proveedores
- Comisiones de tarjetas
- Costos
- Ganancias
- Reportes
- Usuarios
- Roles y permisos
- Auditoría
- Notificaciones
- Exportación PDF / Excel

## Requisitos para Desarrollo

- Visual Studio 2022 o superior
- .NET 8.0 SDK
- Windows App SDK 1.5+
- Windows 10 versión 1809 o superior

## Comandos Útiles

```bash
# Compilar la solución
dotnet build

# Ejecutar pruebas
dotnet test

# Ejecutar la API
dotnet run --project LiveNow.CRM.API

# Ejecutar la aplicación Windows
dotnet run --project LiveNow.CRM
```

## Carreras Soportadas Inicialmente

- Chicago Marathon
- Berlin Marathon

La arquitectura permite agregar posteriormente las demás Abbott World Marathon Majors y cualquier otra carrera internacional sin modificar el código principal.

## Licencia

Propiedad de Live Now. Todos los derechos reservados.
