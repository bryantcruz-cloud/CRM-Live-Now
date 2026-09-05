# Producción: Render + Neon

La arquitectura de producción es `LiveNow.CRM` → HTTPS → `LiveNow.CRM.API` en Render → PostgreSQL en Neon. El cliente Windows nunca se conecta directamente a PostgreSQL.

## Variables de Render

Configurar en el servicio, sin guardarlas en Git:

- `ASPNETCORE_ENVIRONMENT=Production`
- `DatabaseProvider=postgresql`
- `ConnectionStrings__DefaultConnection=<connection string de Neon>`
- `PORT` lo proporciona Render; el Dockerfile escucha en `0.0.0.0` usando ese valor.

`ConnectionStrings__DefaultConnection` debe contener la cadena real únicamente en la configuración segura de Render. No usar la cadena de ejemplo del repositorio.

## Neon

Crear el proyecto y la base PostgreSQL en Neon, copiar su cadena de conexión al secreto `ConnectionStrings__DefaultConnection` de Render y no exponerla en logs ni en la aplicación Windows.

## Migraciones

Las migraciones no se ejecutan automáticamente por request ni al arrancar el contenedor. Para la primera instalación o una actualización, detener/coordinar el servicio y ejecutar explícitamente desde un entorno controlado:

```powershell
$env:DatabaseProvider = "postgresql"
$env:ConnectionStrings__DefaultConnection = "<cadena segura de Neon>"
dotnet ef database update --project LiveNow.CRM.Infrastructure --startup-project LiveNow.CRM.API -- --environment Production
```

Verificar el resultado y hacer backup según la política de Neon antes de aplicar cambios. Nunca borrar o recrear la base de producción.

## Health check y actualizaciones

Render debe comprobar `GET /api/health`. Un estado HTTP 503 indica que la API está viva pero no alcanza la base de datos. Para actualizar: publicar la nueva imagen, aplicar primero cualquier migración explícita compatible y después reiniciar el servicio.

## PCs Windows

Definir `LIVENOW_API_URL` en cada PC con la URL HTTPS del servicio Render, por ejemplo `https://<servicio>.onrender.com/`. El cliente mantiene `https://localhost:5001` como fallback de desarrollo.

## Seguridad pendiente

La API actualmente no registra autenticación ni autorización (`AddAuthentication`, `UseAuthentication` ni políticas `[Authorize]`). Debe añadirse un mecanismo de identidad y autorización antes de exponer endpoints operativos públicamente.
