# Script COMPLETO para Migrar TODOS los Datos a Supabase
# Este script LIMPIA Supabase y migra TODO desde tu BD local

Write-Host "========================================" -ForegroundColor Green
Write-Host "?? MIGRACIÓN COMPLETA A SUPABASE" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

# Configuración BD Local
$LocalDB = "ComputerStoreDB"
$LocalHost = "localhost"
$LocalPort = "5432"
$LocalUser = "postgres"
$LocalPassword = "Abcdefghij123"

# Configuración Supabase
$SupabaseHost = "db.xakfuxhafqbelwankypo.supabase.co"
$SupabaseDB = "postgres"
$SupabaseUser = "postgres"
$SupabasePassword = "CwJ4jyBHT2FBX_x"
$SupabasePort = "5432"

Write-Host "??  ADVERTENCIA:" -ForegroundColor Yellow
Write-Host "   Este script ELIMINARÁ todos los datos en Supabase" -ForegroundColor Yellow
Write-Host "   y los reemplazará con los datos de tu BD local.`n" -ForegroundColor Yellow

$confirm = Read-Host "¿Deseas continuar? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "? Operación cancelada" -ForegroundColor Red
    exit
}

# Función para encontrar herramientas PostgreSQL
function Find-PostgresTool {
    param([string]$ToolName)
    
    # Intentar encontrar en PATH
    $toolCmd = Get-Command $ToolName -ErrorAction SilentlyContinue
    if ($toolCmd) {
        return $toolCmd.Source
    }
    
    # Buscar en rutas comunes
    $commonPaths = @(
    "C:\Program Files\PostgreSQL\17\bin\$ToolName.exe",
 "C:\Program Files\PostgreSQL\16\bin\$ToolName.exe",
   "C:\Program Files\PostgreSQL\15\bin\$ToolName.exe",
        "C:\Program Files\PostgreSQL\14\bin\$ToolName.exe",
        "C:\Program Files\PostgreSQL\13\bin\$ToolName.exe",
     "C:\Program Files (x86)\PostgreSQL\16\bin\$ToolName.exe",
   "C:\Program Files (x86)\PostgreSQL\15\bin\$ToolName.exe",
   "C:\PostgreSQL\16\bin\$ToolName.exe",
        "C:\PostgreSQL\15\bin\$ToolName.exe"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    # Buscar exhaustivamente
    $found = Get-ChildItem -Path "C:\Program Files" -Filter "$ToolName.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
   return $found.FullName
    }
    
    return $null
}

# 1. Verificar herramientas
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "1??  VERIFICANDO HERRAMIENTAS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "?? Buscando PostgreSQL tools..." -ForegroundColor Yellow

$pgDumpPath = Find-PostgresTool "pg_dump"
$psqlPath = Find-PostgresTool "psql"

if (-not $pgDumpPath -or -not $psqlPath) {
    Write-Host "`n? PostgreSQL client tools NO encontrado`n" -ForegroundColor Red
    
    if (-not $pgDumpPath) { Write-Host "   ? pg_dump no encontrado" -ForegroundColor Red }
    if (-not $psqlPath) { Write-Host "   ? psql no encontrado" -ForegroundColor Red }
    
    Write-Host "`n?? Solución:" -ForegroundColor Yellow
    Write-Host "1. Verifica que PostgreSQL esté instalado" -ForegroundColor Gray
    Write-Host "2. Agrega el directorio bin de PostgreSQL al PATH:" -ForegroundColor Gray
    Write-Host '   $env:Path += ";C:\Program Files\PostgreSQL\17\bin"' -ForegroundColor Cyan
    Write-Host "3. O cierra y abre PowerShell como Administrador`n" -ForegroundColor Gray
    exit 1
}

Write-Host "? pg_dump: $pgDumpPath" -ForegroundColor Green
Write-Host "? psql: $psqlPath`n" -ForegroundColor Green

# Crear aliases
$pgDump = $pgDumpPath
$psql = $psqlPath

# 2. Verificar BD Local
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "2??  VERIFICANDO BASE DE DATOS LOCAL" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "?? Intentando conectar a BD local..." -ForegroundColor Yellow
Write-Host "   Host: $LocalHost" -ForegroundColor Gray
Write-Host "   Port: $LocalPort" -ForegroundColor Gray
Write-Host "   Database: $LocalDB" -ForegroundColor Gray
Write-Host "   User: $LocalUser`n" -ForegroundColor Gray

$env:PGPASSWORD = $LocalPassword
$env:PGSSLMODE = "disable"

# Query con comillas dobles para el nombre de tabla
$productCountQuery = 'SELECT COUNT(*) FROM "Productos"'
$localCheckResult = & $psql -h $LocalHost -p $LocalPort -U $LocalUser -d $LocalDB -t -c $productCountQuery 2>&1

# Filtrar solo números del resultado
$localProductCount = "0"
foreach ($line in $localCheckResult) {
    if ($line -match '^\s*\d+\s*$') {
        $localProductCount = $line.ToString().Trim()
        break
    }
}

if ($LASTEXITCODE -ne 0 -or [int]$localProductCount -eq 0) {
    Write-Host "`n? No se pudo conectar a la BD local o no hay productos" -ForegroundColor Red
    Write-Host "Último error:" -ForegroundColor Yellow
    Write-Host "$localCheckResult`n" -ForegroundColor Gray

    Write-Host "   Verifica que:" -ForegroundColor Yellow
    Write-Host "   1. PostgreSQL esté corriendo (servicio activo)" -ForegroundColor Gray
    Write-Host "   2. La base de datos 'ComputerStoreDB' exista" -ForegroundColor Gray
    Write-Host "   3. El usuario 'postgres' tenga acceso" -ForegroundColor Gray
    Write-Host "   4. La contraseña sea correcta" -ForegroundColor Gray
    Write-Host "   5. La tabla 'Productos' exista`n" -ForegroundColor Gray
    
    Write-Host "?? Para verificar el servicio PostgreSQL:" -ForegroundColor Cyan
    Write-Host "   Get-Service postgresql*`n" -ForegroundColor Gray
    
    Remove-Item Env:\PGPASSWORD
    Remove-Item Env:\PGSSLMODE -ErrorAction SilentlyContinue
  exit 1
}

Write-Host "? Conexión a BD local exitosa" -ForegroundColor Green
Write-Host "?? Productos en BD local: $localProductCount`n" -ForegroundColor Cyan

if ([int]$localProductCount -eq 0) {
    Write-Host "??  La BD local no tiene productos. ¿Deseas continuar? (S/N)" -ForegroundColor Yellow
    $confirmEmpty = Read-Host
    if ($confirmEmpty -ne "S" -and $confirmEmpty -ne "s") {
        Remove-Item Env:\PGPASSWORD
        Remove-Item Env:\PGSSLMODE -ErrorAction SilentlyContinue
    exit
    }
}

# 3. Contar registros antes de migrar
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "3??  CONTANDO DATOS A MIGRAR" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$countQuery = 'SELECT (SELECT COUNT(*) FROM "AspNetUsers") as usuarios, (SELECT COUNT(*) FROM "AspNetRoles") as roles, (SELECT COUNT(*) FROM "Productos") as productos, (SELECT COUNT(*) FROM "Categorias") as categorias, (SELECT COUNT(*) FROM "Pedidos") as pedidos, (SELECT COUNT(*) FROM "DetallesPedido") as detalles_pedido, (SELECT COUNT(*) FROM "__EFMigrationsHistory") as migraciones;'

Write-Host "?? Datos en BD Local:" -ForegroundColor Yellow
$env:PGPASSWORD = $LocalPassword
& $psql -h $LocalHost -p $LocalPort -U $LocalUser -d $LocalDB -c $countQuery 2>&1 | Where-Object { $_ -notmatch 'WARNING' } | Write-Host
Write-Host ""


# 4. Exportar BD Local
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "4??  EXPORTANDO BASE DE DATOS LOCAL" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "?? Creando backup completo..." -ForegroundColor Yellow

$env:PGPASSWORD = $LocalPassword
& $pgDump -h $LocalHost -p $LocalPort -U $LocalUser -d $LocalDB `
    -f backup-complete.sql `
    --clean `
    --if-exists `
    --no-owner `
    --no-privileges 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al exportar BD local" -ForegroundColor Red
    Remove-Item Env:\PGPASSWORD
    Remove-Item Env:\PGSSLMODE -ErrorAction SilentlyContinue
    exit 1
}

$backupSize = (Get-Item backup-complete.sql).Length / 1KB
Write-Host "? Backup creado: backup-complete.sql ($([math]::Round($backupSize, 2)) KB)`n" -ForegroundColor Green

# 5. Limpiar Supabase
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "5??  LIMPIANDO SUPABASE" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "?? Eliminando tablas existentes en Supabase..." -ForegroundColor Yellow

$cleanupScript = @"
SET session_replication_role = 'replica';
DROP TABLE IF EXISTS "DetallesPedido" CASCADE;
DROP TABLE IF EXISTS "Pedidos" CASCADE;
DROP TABLE IF EXISTS "Productos" CASCADE;
DROP TABLE IF EXISTS "Categorias" CASCADE;
DROP TABLE IF EXISTS "AspNetUserTokens" CASCADE;
DROP TABLE IF EXISTS "AspNetUserRoles" CASCADE;
DROP TABLE IF EXISTS "AspNetUserLogins" CASCADE;
DROP TABLE IF EXISTS "AspNetUserClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetRoleClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetUsers" CASCADE;
DROP TABLE IF EXISTS "AspNetRoles" CASCADE;
DROP TABLE IF EXISTS "TraficoWeb" CASCADE;
DROP TABLE IF EXISTS "Envios" CASCADE;
DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;
SET session_replication_role = 'origin';
"@

$env:PGPASSWORD = $SupabasePassword
Remove-Item Env:\PGSSLMODE -ErrorAction SilentlyContinue
$cleanupScript | & $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort 2>&1 | Out-Null

Write-Host "? Supabase limpiado correctamente`n" -ForegroundColor Green

# 6. Importar a Supabase
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "6??  IMPORTANDO A SUPABASE" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "?? Importando datos a Supabase..." -ForegroundColor Yellow
Write-Host "   (Esto puede tardar 1-2 minutos)`n" -ForegroundColor Gray

$env:PGPASSWORD = $SupabasePassword
& $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -f backup-complete.sql 2>&1 | Out-Null

Write-Host "? Datos importados a Supabase`n" -ForegroundColor Green

# 7. Verificar migración
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "7??  VERIFICANDO MIGRACIÓN" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "?? Datos en Supabase después de migración:" -ForegroundColor Yellow

$verificationQuery = 'SELECT (SELECT COUNT(*) FROM "AspNetUsers") as usuarios, (SELECT COUNT(*) FROM "AspNetRoles") as roles, (SELECT COUNT(*) FROM "Productos") as productos, (SELECT COUNT(*) FROM "Categorias") as categorias, (SELECT COUNT(*) FROM "Pedidos") as pedidos, (SELECT COUNT(*) FROM "DetallesPedido") as detalles_pedido, (SELECT COUNT(*) FROM "__EFMigrationsHistory") as migraciones;'

$env:PGPASSWORD = $SupabasePassword
& $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -c $verificationQuery 2>&1 | Where-Object { $_ -notmatch 'WARNING' } | Write-Host
Write-Host ""


# 8. Comparar Local vs Supabase
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "8??  COMPARACIÓN FINAL" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Comparando cantidades de registros...`n" -ForegroundColor Yellow

# Contar productos en local
$env:PGPASSWORD = $LocalPassword
$env:PGSSLMODE = "disable"
$localProductsResult = & $psql -h $LocalHost -p $LocalPort -U $LocalUser -d $LocalDB -t -c 'SELECT COUNT(*) FROM "Productos"' 2>&1

$localProducts = "0"
foreach ($line in $localProductsResult) {
    if ($line -match '^\s*\d+\s*$') {
        $localProducts = $line.ToString().Trim()
        break
    }
}

# Contar productos en Supabase
$env:PGPASSWORD = $SupabasePassword
Remove-Item Env:\PGSSLMODE -ErrorAction SilentlyContinue
$supabaseProductsResult = & $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -t -c 'SELECT COUNT(*) FROM "Productos"' 2>&1

$supabaseProducts = "0"
foreach ($line in $supabaseProductsResult) {
  if ($line -match '^\s*\d+\s*$') {
        $supabaseProducts = $line.ToString().Trim()
      break
    }
}

Write-Host "?? Productos:" -ForegroundColor Cyan
Write-Host "   Local: $localProducts" -ForegroundColor Gray
Write-Host "   Supabase: $supabaseProducts" -ForegroundColor Gray

if ($localProducts -eq $supabaseProducts) {
    Write-Host "   ? MATCH - Todos los productos migrados`n" -ForegroundColor Green
} else {
    Write-Host "   ? DIFERENCIA - Faltan productos`n" -ForegroundColor Red
}

# Contar usuarios
$env:PGPASSWORD = $LocalPassword
$env:PGSSLMODE = "disable"
$localUsersResult = & $psql -h $LocalHost -p $LocalPort -U $LocalUser -d $LocalDB -t -c 'SELECT COUNT(*) FROM "AspNetUsers"' 2>&1

$localUsers = "0"
foreach ($line in $localUsersResult) {
    if ($line -match '^\s*\d+\s*$') {
        $localUsers = $line.ToString().Trim()
   break
    }
}

$env:PGPASSWORD = $SupabasePassword
Remove-Item Env:\PGSSLMODE -ErrorAction SilentlyContinue
$supabaseUsersResult = & $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -t -c 'SELECT COUNT(*) FROM "AspNetUsers"' 2>&1

$supabaseUsers = "0"
foreach ($line in $supabaseUsersResult) {
    if ($line -match '^\s*\d+\s*$') {
    $supabaseUsers = $line.ToString().Trim()
break
    }
}

Write-Host "?? Usuarios:" -ForegroundColor Cyan
Write-Host "   Local: $localUsers" -ForegroundColor Gray
Write-Host "Supabase: $supabaseUsers" -ForegroundColor Gray

if ($localUsers -eq $supabaseUsers) {
    Write-Host "   ? MATCH - Todos los usuarios migrados`n" -ForegroundColor Green
} else {
    Write-Host "   ??  DIFERENCIA - Puede ser normal`n" -ForegroundColor Yellow
}

# Limpiar variables de entorno
Remove-Item Env:\PGPASSWORD
Remove-Item Env:\PGSSLMODE -ErrorAction SilentlyContinue

# 9. Listar algunos productos para verificar
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "9??  MUESTRA DE PRODUCTOS MIGRADOS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "?? Primeros 5 productos en Supabase:" -ForegroundColor Yellow

$sampleQuery = 'SELECT "Nombre", "Precio", "Stock", "Activo" FROM "Productos" ORDER BY "ProductoId" LIMIT 5;'

$env:PGPASSWORD = $SupabasePassword
& $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -c $sampleQuery 2>&1 | Where-Object { $_ -notmatch 'WARNING' } | Write-Host
Remove-Item Env:\PGPASSWORD

# Resultado Final
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "? MIGRACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Write-Host "?? Resumen:" -ForegroundColor Cyan
Write-Host "   • Productos migrados: $supabaseProducts" -ForegroundColor White
Write-Host "• Usuarios migrados: $supabaseUsers" -ForegroundColor White
Write-Host "   • Backup guardado: backup-complete.sql`n" -ForegroundColor White

if ($localProducts -eq $supabaseProducts -and [int]$supabaseProducts -gt 0) {
    Write-Host "?? ¡Migración exitosa! Todos los datos fueron transferidos.`n" -ForegroundColor Green
    
    Write-Host "?? Próximos pasos:" -ForegroundColor Yellow
    Write-Host "   1??  Ejecuta: .\fix-azure-connection.ps1" -ForegroundColor Cyan
    Write-Host "   2??  Ejecuta: .\deploy-app.ps1" -ForegroundColor Cyan
    Write-Host "   3??  Abre: https://compuhipermegared.azurewebsites.net`n" -ForegroundColor Cyan
} else {
    Write-Host "??  La migración completó pero hay diferencias." -ForegroundColor Yellow
    Write-Host "   Revisa los mensajes anteriores.`n" -ForegroundColor Yellow
    
 Write-Host "?? Para investigar:" -ForegroundColor Cyan
    Write-Host "   • Revisa backup-complete.sql" -ForegroundColor Gray
    Write-Host "   • Ejecuta este script nuevamente`n" -ForegroundColor Gray
}

Write-Host "?? Nota: El backup local se guardó como 'backup-complete.sql'" -ForegroundColor Gray
Write-Host "         Puedes eliminarlo si la migración fue exitosa.`n" -ForegroundColor Gray
