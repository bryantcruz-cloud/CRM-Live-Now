# Producción: Render + Neon

Arquitectura: `LiveNow.CRM` → HTTPS → `LiveNow.CRM.API` en Render → PostgreSQL en Neon. El cliente Windows nunca se conecta directamente a PostgreSQL.

## 1. Crear PostgreSQL en Neon

1. Crear un proyecto en la consola de Neon y una base/branch de producción.
2. En **Connect**, copiar la cadena PostgreSQL para uso exclusivo en el secreto de Render.
3. Confirmar que incluya TLS, como `sslmode=require` (Neon puede recomendar también `channel_binding=require`). Npgsql/EF Core 8 es compatible con esta cadena.
4. No guardar la cadena en Git, en `appsettings.*.json`, en la aplicación Windows ni en logs.

La aplicación selecciona PostgreSQL con `DatabaseProvider=postgresql` y lee `ConnectionStrings__DefaultConnection`. SQLite permanece únicamente para desarrollo.

## 2. Crear el Web Service en Render

Crear un **Web Service** conectado al repositorio y seleccionar despliegue mediante el `Dockerfile` existente. No añadir un comando que ejecute migraciones automáticamente.

Configurar el health check como `GET /api/health` y estas variables en el panel **Environment** de Render:

```text
ASPNETCORE_ENVIRONMENT=Production
DatabaseProvider=postgresql
ConnectionStrings__DefaultConnection=<secreto de Neon>
Jwt__Key=<secreto generado aleatoriamente>
Jwt__Issuer=<identificador estable de la API>
Jwt__Audience=<audiencia estable del cliente>
Jwt__ExpirationMinutes=60
BootstrapAdmin__Name=<nombre inicial>
BootstrapAdmin__Username=<usuario inicial>
BootstrapAdmin__Email=<email inicial>
BootstrapAdmin__Password=<contraseña temporal>
```

`PORT` lo proporciona Render. El Dockerfile publica el servicio en `0.0.0.0:${PORT}`; no fijar un puerto distinto en Render. Swagger solo se habilita en Development.

## 3. Generar la clave JWT

Generar una clave aleatoria fuera del repositorio y copiarla directamente al secreto `Jwt__Key` de Render. Por ejemplo, con OpenSSL en PowerShell, sin mostrarla en consola:

```powershell
openssl rand -base64 48 | Set-Clipboard
```

Pegarla una sola vez en Render y no incluirla en comandos registrados, capturas, commits o documentación.

## 4. Aplicar migraciones explícitamente

No se ejecutan migraciones automáticamente al arrancar la API. Con la cadena real disponible solo como variable de entorno local segura, ejecutar desde la raíz del repositorio:

```powershell
$env:DatabaseProvider = "postgresql"
$env:ConnectionStrings__DefaultConnection = "<cadena segura de Neon>"
dotnet ef database update --project .\LiveNow.CRM.Infrastructure.PostgreSql\LiveNow.CRM.Infrastructure.PostgreSql.csproj --startup-project .\LiveNow.CRM.API\LiveNow.CRM.API.csproj --context LiveNow.CRM.Infrastructure.Data.LiveNowDbContext -- --environment Production
```

El placeholder anterior no debe sustituirse en un archivo ni pegarse en un informe. Antes de una migración, usar el backup/restore disponible en Neon. Aplicar las migraciones existentes en orden hasta `20260905193701_AddUsers`; no borrar la base, recrear el esquema ni modificar migraciones históricas.

En este entorno no se ejecuta la migración real porque no hay una cadena Neon proporcionada de forma segura. El assembly PostgreSQL contiene el baseline `20260906001158_InitialPostgreSql`; SQLite conserva sus migraciones históricas en `LiveNow.CRM.Infrastructure`.

### Base Neon parcialmente inicializada

El primer intento puede haber creado únicamente `__EFMigrationsHistory` antes de fallar. Como la base es nueva y no contiene datos reales, confirmar en el SQL Editor de Neon que no existan otras tablas con datos y ejecutar allí, una sola vez:

```sql
DROP TABLE IF EXISTS "__EFMigrationsHistory";
```

No borrar el esquema completo ni ejecutar este SQL sobre una base con datos. Después aplicar el comando EF anterior; la base debe quedar registrada con `20260906001158_InitialPostgreSql`.

## 5. Bootstrap del primer administrador

1. Aplicar primero `AddUsers`.
2. Configurar temporalmente las variables `BootstrapAdmin__*` en Render y reiniciar.
3. Probar `POST /api/auth/login` con el usuario inicial.
4. Retirar como mínimo `BootstrapAdmin__Password` y, si ya no se necesitan, todas las variables `BootstrapAdmin__*`.
5. Reiniciar y confirmar que el login existente sigue funcionando.

La contraseña se almacena como hash PBKDF2 con salt. No dejar credenciales de bootstrap permanentes si no son necesarias.

## 6. Validación posterior al despliegue

Con la URL pública asignada por Render:

```text
GET  https://<servicio>.onrender.com/api/health
POST https://<servicio>.onrender.com/api/auth/login
```

Comprobar que health devuelve HTTP 200 con la base accesible, que login devuelve HTTP 200 y que un endpoint protegido devuelve HTTP 401 sin Bearer y HTTP 200 con un token válido. No copiar tokens completos a logs o informes.

## 7. Cliente Windows

En cada PC configurar la variable de entorno de usuario:

```text
LIVENOW_API_URL=https://<servicio>.onrender.com/
```

La aplicación ya usa esta variable y conserva `https://localhost:5001` como fallback de desarrollo. Validar login, dashboard, clientes, inventario, cotizaciones y ventas desde cada PC.

La prueba real debe crear un cliente temporal, leerlo de nuevo, cerrar/reabrir la aplicación y confirmar que persiste en Neon. Eliminarlo solo después de verificar que no se trata de un registro útil.

## 8. Backup, restore y rollback básico

Usar las copias de seguridad, retención y puntos de restauración disponibles según el plan de Neon; verificar en la consola qué retención aplica antes de anunciar un RPO. Antes de cada migración relevante:

1. Crear/verificar un punto de restauración o backup de Neon.
2. Registrar la migración que se va a aplicar.
3. Aplicarla explícitamente y comprobar `/api/health` y login.
4. Si falla, detener el despliegue, conservar logs sin secretos y restaurar según el procedimiento del plan de Neon; después volver a la imagen/configuración anterior en Render.

Para una copia externa controlada se puede usar `pg_dump` y para restaurarla `pg_restore`, siempre desde un entorno seguro y sin escribir credenciales en el comando o en el repositorio.

## 9. Actualizaciones futuras

Publicar una nueva imagen, verificar variables, ejecutar manualmente las migraciones pendientes contra Neon y luego reiniciar Render. Mantener `DatabaseProvider=postgresql`, `ConnectionStrings__DefaultConnection`, la configuración JWT y `LIVENOW_API_URL`. No activar migraciones automáticas como sustituto del procedimiento controlado.
