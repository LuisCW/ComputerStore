# Script para diagnosticar conexión a Supabase
Write-Host "?? DIAGNÓSTICO DE CONEXIÓN A SUPABASE" -ForegroundColor Cyan

$SupabaseHost = "db.xakfuxhafqbelwankypo.supabase.co"
$SupabaseDB = "postgres"
$SupabaseUser = "postgres"
$SupabasePassword = "CwJ4jyBHT2FBX_x"
$SupabasePort = "5432"

# Verificar si psql existe
$psql = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psql) {
    Write-Host "? psql no encontrado. Instala PostgreSQL client tools." -ForegroundColor Red
    exit 1
}

Write-Host "`n?? Verificando tablas en Supabase..." -ForegroundColor Cyan

$queries = @"
-- Listar todas las tablas
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- Contar registros en tablas principales
SELECT 
    'AspNetUsers' as tabla,
    COUNT(*) as registros
FROM "AspNetUsers"
UNION ALL
SELECT 
    'Productos' as tabla,
    COUNT(*) as registros
FROM "Productos"
UNION ALL
SELECT 
    'Pedidos' as tabla,
    COUNT(*) as registros
FROM "Pedidos"
UNION ALL
SELECT 
    'Categorias' as tabla,
    COUNT(*) as registros
FROM "Categorias"
ORDER BY tabla;

-- Verificar migraciones de Entity Framework
SELECT "MigrationId", "ProductVersion" 
FROM "__EFMigrationsHistory" 
ORDER BY "MigrationId";
"@

$env:PGPASSWORD = $SupabasePassword

Write-Host "`n?? Conectando a Supabase..." -ForegroundColor Yellow
$queries | psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? Conexión exitosa a Supabase" -ForegroundColor Green
} else {
    Write-Host "`n? Error de conexión a Supabase" -ForegroundColor Red
}

# Cleanup
$env:PGPASSWORD = $null

Write-Host "`n?? Connection String usado en Azure:" -ForegroundColor Cyan
Write-Host "Host=db.xakfuxhafqbelwankypo.supabase.co;Database=postgres;Username=postgres;Password=CwJ4jyBHT2FBX_x;Port=5432;SSL Mode=Require;Trust Server Certificate=true" -ForegroundColor Yellow
