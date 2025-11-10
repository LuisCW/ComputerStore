# ?? Solución: Base de Datos No Carga en Azure

## ?? Problema Identificado

La base de datos de Supabase no se está cargando en Azure debido a uno o más de estos problemas:

### Causas Comunes:

1. **Connection String mal configurado** en Azure
2. **Variables de entorno** no se leen correctamente
3. **SSL/TLS** no está configurado adecuadamente
4. **Migraciones** no se han aplicado en Supabase
5. **Firewall** de Azure bloqueando la conexión

---

## ? Solución Paso a Paso

### **PASO 1: Diagnosticar el Problema**

Ejecuta el script de diagnóstico:

```powershell
.\diagnose-supabase.ps1
```

Este script te mostrará:
- ? Si la conexión a Supabase funciona desde tu PC
- ? Si el Connection String está configurado en Azure
- ? Si las tablas existen en Supabase
- ? Estado de la aplicación en Azure

**Revisa los resultados** y anota los ? que veas.

---

### **PASO 2: Corregir la Configuración de Azure**

Ejecuta el script de corrección:

```powershell
.\fix-azure-connection.ps1
```

Este script:
- ?? Configura el Connection String como **PostgreSQL** (no SQL Server)
- ?? Verifica todas las variables de entorno
- ?? Habilita logging detallado
- ?? Reinicia la aplicación

**Al final del script, elige "S"** para ver los logs en tiempo real.

---

### **PASO 3: Re-Desplegar la Aplicación**

La aplicación ahora incluye diagnósticos mejorados. Re-despliégala:

```powershell
.\deploy-app.ps1
```

Esto compilará y desplegará la nueva versión con:
- ?? Diagnósticos de configuración al inicio
- ?? Detección automática de Connection Strings en Azure
- ?? Mensajes de error más claros

---

### **PASO 4: Verificar los Logs**

Espera 2-3 minutos y luego descarga los logs:

```powershell
az webapp log download --name compuhipermegared --resource-group CompuHiperMegaRed --log-file logs.zip
```

Busca en los logs:

#### ? Mensajes de Éxito:
```
? Connection String encontrado en configuración
? Conexión a base de datos exitosa
?? Base de datos inicializada correctamente
?? Aplicación iniciada correctamente
```

#### ? Mensajes de Error:
```
? Connection String 'DefaultConnection' NO encontrado
? No se pudo conectar a la base de datos
? Error durante la inicialización de la base de datos
```

---

## ?? Soluciones Específicas por Error

### **Error: "Connection String not found"**

**Causa**: Azure no está exponiendo el Connection String correctamente.

**Solución**:

```powershell
# Ejecutar fix-azure-connection.ps1 nuevamente
.\fix-azure-connection.ps1

# Verificar que se configuró
az webapp config connection-string list `
    --name compuhipermegared `
    --resource-group CompuHiperMegaRed `
    --query "[?name=='DefaultConnection'].type" -o tsv
```

Debe decir: **PostgreSQL**

Si dice **SQLAzure** o **Custom**, ejecuta:

```powershell
az webapp config connection-string set `
    --resource-group CompuHiperMegaRed `
    --name compuhipermegared `
    --settings DefaultConnection="Host=db.xakfuxhafqbelwankypo.supabase.co;Database=postgres;Username=postgres;Password=CwJ4jyBHT2FBX_x;Port=5432;SSL Mode=Require;Trust Server Certificate=true" `
    --connection-string-type PostgreSQL
```

---

### **Error: "Cannot connect to database"**

**Causa**: Supabase puede estar bloqueando conexiones desde Azure.

**Solución**:

1. Ve a Supabase Dashboard: https://supabase.com/dashboard
2. Ve a tu proyecto ? **Settings** ? **Database**
3. En **Connection Pooling**, asegúrate de que esté habilitado
4. Verifica que **SSL Mode** sea "require"
5. En **Network Restrictions**, asegúrate de que Azure no esté bloqueado

Si Supabase está restringiendo IPs:
- Ve a **Settings** ? **Database** ? **Connection String**
- Usa el **Connection Pooler** en lugar de la conexión directa:

```
Host=aws-0-us-east-1.pooler.supabase.com
Port=6543
```

---

### **Error: "Npgsql.NpgsqlException: SSL connection required"**

**Causa**: Falta configurar SSL Mode en el Connection String.

**Solución**:

El Connection String debe incluir:
```
SSL Mode=Require;Trust Server Certificate=true
```

Ejecuta nuevamente:
```powershell
.\fix-azure-connection.ps1
```

---

### **Error: "No se pudo inicializar la base de datos"**

**Causa**: Las migraciones de Entity Framework no se han aplicado.

**Solución**:

Verifica que las migraciones existan en Supabase:

```powershell
# Conectarse a Supabase
$env:PGPASSWORD = "CwJ4jyBHT2FBX_x"
psql "postgresql://postgres@db.xakfuxhafqbelwankypo.supabase.co:5432/postgres?sslmode=require"

# Dentro de psql, ejecuta:
\dt

# Deberías ver tablas como:
# AspNetUsers
# Productos
# Categorias
# __EFMigrationsHistory
```

Si **NO** ves las tablas, ejecuta la migración:

```powershell
.\migrate-db.ps1
```

---

## ?? Proceso Completo de Re-Deployment

Si nada funciona, ejecuta todo el proceso de nuevo:

```powershell
# 1. Corregir configuración de Azure
.\fix-azure-connection.ps1

# 2. Migrar base de datos (si las tablas no existen)
.\migrate-db.ps1

# 3. Re-desplegar aplicación
.\deploy-app.ps1

# 4. Esperar 2 minutos y verificar
Start-Sleep -Seconds 120

# 5. Abrir en navegador
start https://compuhipermegared.azurewebsites.net

# 6. Ver logs en tiempo real
az webapp log tail --name compuhipermegared --resource-group CompuHiperMegaRed
```

---

## ?? Verificación Final

### 1. Verificar Connection String en Azure

```powershell
az webapp config connection-string list `
    --name compuhipermegared `
    --resource-group CompuHiperMegaRed `
    -o table
```

Debe mostrar:
```
Name         Type      Value
------------------  ------------  -----------------------------------------
DefaultConnection   PostgreSQL    Host=db.xakfuxhafqbelwankypo.supabase...
```

### 2. Verificar Tablas en Supabase

```powershell
$env:PGPASSWORD = "CwJ4jyBHT2FBX_x"
psql "postgresql://postgres@db.xakfuxhafqbelwankypo.supabase.co:5432/postgres?sslmode=require" -c "\dt"
```

Debe mostrar:
```
       List of relations
 Schema |         Name          | Type  |  Owner
--------+-----------------------+-------+----------
 public | AspNetUsers  | table | postgres
 public | Productos     | table | postgres
 public | Categorias            | table | postgres
 ...
```

### 3. Verificar Estado de la App

```powershell
az webapp show `
    --name compuhipermegared `
    --resource-group CompuHiperMegaRed `
    --query "{State:state, DefaultHostName:defaultHostName}" `
    -o table
```

Debe mostrar:
```
State     DefaultHostName
--------  -----------------------------------------------
Running   compuhipermegared.azurewebsites.net
```

### 4. Probar en Navegador

Abre: https://compuhipermegared.azurewebsites.net

Deberías ver:
- ? Página de inicio carga
- ? Productos se muestran
- ? Puedes hacer login

---

## ?? Soporte Adicional

Si después de seguir todos los pasos aún no funciona:

### Ver Logs Detallados

```powershell
# Habilitar logging de aplicación
az webapp log config `
    --name compuhipermegared `
  --resource-group CompuHiperMegaRed `
    --application-logging filesystem `
    --level verbose `
    --docker-container-logging filesystem

# Reiniciar
az webapp restart `
    --name compuhipermegared `
    --resource-group CompuHiperMegaRed

# Ver logs en tiempo real
az webapp log tail `
    --name compuhipermegared `
    --resource-group CompuHiperMegaRed
```

### Busca estos patrones en los logs:

- `?? DIAGNÓSTICO DE CONFIGURACIÓN` ? Información de configuración
- `? Connection String encontrado` ? Connection string OK
- `? Connection String NO encontrado` ? Problema de configuración
- `? Conexión a base de datos exitosa` ? BD funcionando
- `? No se pudo conectar` ? Problema de conectividad

---

## ?? Checklist de Diagnóstico

- [ ] `diagnose-supabase.ps1` ejecutado
- [ ] Connection String tipo **PostgreSQL** (no SQLAzure)
- [ ] SSL Mode = Require en Connection String
- [ ] Tablas existen en Supabase (`\dt` muestra AspNetUsers, etc.)
- [ ] `fix-azure-connection.ps1` ejecutado
- [ ] `deploy-app.ps1` ejecutado
- [ ] Logs muestran "Conexión a base de datos exitosa"
- [ ] Aplicación en estado "Running"
- [ ] Sitio web carga en navegador
- [ ] Productos se muestran

---

## ?? Éxito

Si todos los checkboxes están ?, tu aplicación debería estar funcionando correctamente con la base de datos de Supabase.

**URL**: https://compuhipermegared.azurewebsites.net

**Credenciales Admin**:
- Email: admin@computerhipermegared.com
- Password: E_commerce123$
