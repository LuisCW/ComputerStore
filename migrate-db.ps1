# Script para Migrar Base de Datos Local a Supabase
# Ejecutar ANTES de deploy-app.ps1

Write-Host "???  Migrando Base de Datos a Supabase..." -ForegroundColor Green

$LocalDB = "ComputerStoreDB"
$LocalHost = "localhost"
$LocalUser = "postgres"
$LocalPassword = "Abcdefghij123"

$SupabaseHost = "db.xakfuxhafqbelwankypo.supabase.co"
$SupabaseDB = "postgres"
$SupabaseUser = "postgres"
$SupabasePassword = "CwJ4jyBHT2FBX_x"
$SupabasePort = "5432"

# Verificar si pg_dump existe
Write-Host "`n?? Verificando pg_dump..." -ForegroundColor Cyan
$pgDump = Get-Command pg_dump -ErrorAction SilentlyContinue

if (-not $pgDump) {
    Write-Host "? pg_dump no encontrado. Instala PostgreSQL client tools." -ForegroundColor Red
    Write-Host "   Descarga desde: https://www.postgresql.org/download/" -ForegroundColor Yellow
    exit 1
}

# 1. Exportar base de datos local
Write-Host "`n?? Exportando base de datos local..." -ForegroundColor Cyan
$env:PGPASSWORD = $LocalPassword
pg_dump -h $LocalHost -U $LocalUser -d $LocalDB -f backup.sql --clean --if-exists --no-owner --no-privileges

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al exportar base de datos local" -ForegroundColor Red
    exit 1
}

Write-Host "? Backup creado: backup.sql" -ForegroundColor Green

# 2. Importar a Supabase
Write-Host "`n?? Importando a Supabase..." -ForegroundColor Cyan
$env:PGPASSWORD = $SupabasePassword
psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -f backup.sql

if ($LASTEXITCODE -ne 0) {
    Write-Host "??  Algunas queries fallaron (normal si es primera vez)" -ForegroundColor Yellow
} else {
    Write-Host "? Base de datos migrada exitosamente" -ForegroundColor Green
}

# 3. Verificar migración
Write-Host "`n?? Verificando migración..." -ForegroundColor Cyan
$verification = @"
SELECT 
    (SELECT COUNT(*) FROM ""AspNetUsers"") as usuarios,
    (SELECT COUNT(*) FROM ""Productos"") as productos,
    (SELECT COUNT(*) FROM ""Pedidos"") as pedidos;
"@

$env:PGPASSWORD = $SupabasePassword
$verification | psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort

Write-Host "`n? Migración completada!" -ForegroundColor Green
Write-Host "`n??  Siguiente paso: Ejecutar deploy-app.ps1" -ForegroundColor Magenta

# Cleanup
$env:PGPASSWORD = $null
