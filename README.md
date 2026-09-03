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
