# Script para Ver Productos en Supabase
# Este script muestra todos los productos que hay actualmente en Supabase

Write-Host "========================================" -ForegroundColor Green
Write-Host "?? PRODUCTOS EN SUPABASE" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

$SupabaseHost = "db.xakfuxhafqbelwankypo.supabase.co"
$SupabaseDB = "postgres"
$SupabaseUser = "postgres"
$SupabasePassword = "CwJ4jyBHT2FBX_x"
$SupabasePort = "5432"

# Función para encontrar psql
function Find-Psql {
    # Intentar encontrar psql en PATH
  $psqlCmd = Get-Command psql -ErrorAction SilentlyContinue
    if ($psqlCmd) {
        return $psqlCmd.Source
    }
    
    # Buscar en rutas comunes de PostgreSQL
    $commonPaths = @(
        "C:\Program Files\PostgreSQL\17\bin\psql.exe",
        "C:\Program Files\PostgreSQL\16\bin\psql.exe",
        "C:\Program Files\PostgreSQL\15\bin\psql.exe",
        "C:\Program Files\PostgreSQL\14\bin\psql.exe",
        "C:\Program Files\PostgreSQL\13\bin\psql.exe",
    "C:\Program Files (x86)\PostgreSQL\16\bin\psql.exe",
        "C:\Program Files (x86)\PostgreSQL\15\bin\psql.exe",
        "C:\PostgreSQL\16\bin\psql.exe",
        "C:\PostgreSQL\15\bin\psql.exe"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            Write-Host "? PostgreSQL encontrado en: $path" -ForegroundColor Green
 return $path
        }
    }
  
    # Buscar en todo Program Files (más lento pero exhaustivo)
    Write-Host "?? Buscando PostgreSQL en el sistema..." -ForegroundColor Yellow
    $found = Get-ChildItem -Path "C:\Program Files" -Filter psql.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($found) {
 Write-Host "? PostgreSQL encontrado en: $($found.FullName)" -ForegroundColor Green
        return $found.FullName
    }
    
    return $null
}

# Buscar psql
Write-Host "?? Buscando PostgreSQL..." -ForegroundColor Cyan
$psqlPath = Find-Psql

if (-not $psqlPath) {
    Write-Host "`n? psql NO encontrado en el sistema`n" -ForegroundColor Red
    Write-Host "Por favor verifica:" -ForegroundColor Yellow
    Write-Host "1. PostgreSQL está instalado" -ForegroundColor Gray
    Write-Host "2. La ruta de PostgreSQL está en las variables de entorno" -ForegroundColor Gray
    Write-Host "`nPara agregar a PATH manualmente:" -ForegroundColor Yellow
    Write-Host '   $env:Path += ";C:\Program Files\PostgreSQL\17\bin"' -ForegroundColor Cyan
    Write-Host "`nO cierra y abre PowerShell nuevamente.`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "? PostgreSQL encontrado: $psqlPath`n" -ForegroundColor Green

# Usar la ruta completa de psql
$psql = $psqlPath

# 1. Contar total de productos
Write-Host "?? Contando productos..." -ForegroundColor Cyan

$env:PGPASSWORD = $SupabasePassword
$result = & $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -t -c 'SELECT COUNT(*) FROM "Productos";' 2>&1

# Filtrar solo las líneas que son números
$totalProducts = "0"
foreach ($line in $result) {
    if ($line -match '^\s*\d+\s*$') {
        $totalProducts = $line.ToString().Trim()
        break
    }
}

Write-Host "?? Total de productos en Supabase: $totalProducts`n" -ForegroundColor Yellow

if ([int]$totalProducts -eq 0) {
    Write-Host "??  NO HAY PRODUCTOS EN SUPABASE`n" -ForegroundColor Red
    Write-Host "Esto explica por qué no aparecen en el sitio de Azure.`n" -ForegroundColor Yellow
    
    Write-Host "? Solución:" -ForegroundColor Green
    Write-Host "   Ejecuta: .\migrate-db-complete.ps1`n" -ForegroundColor Cyan
    
    Remove-Item Env:\PGPASSWORD
    exit
}

# 2. Contar productos activos vs inactivos
Write-Host "?? Desglose de productos:" -ForegroundColor Cyan

$statusQuery = 'SELECT "Activo", COUNT(*) as cantidad FROM "Productos" GROUP BY "Activo" ORDER BY "Activo" DESC;'

$env:PGPASSWORD = $SupabasePassword
& $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -c $statusQuery 2>&1 | Where-Object { $_ -notmatch 'WARNING' } | Write-Host
Write-Host ""

# 3. Mostrar productos con stock
Write-Host "?? Productos con stock disponible:" -ForegroundColor Cyan

$stockQuery = 'SELECT "ProductoId", "Nombre", "Precio", "Stock", "Activo" FROM "Productos" WHERE "Stock" > 0 AND "Activo" = true ORDER BY "ProductoId" LIMIT 20;'

$env:PGPASSWORD = $SupabasePassword
& $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -c $stockQuery 2>&1 | Where-Object { $_ -notmatch 'WARNING' } | Write-Host
Write-Host ""

# 4. Mostrar productos SIN stock (agotados)
Write-Host "?? Productos agotados:" -ForegroundColor Yellow

$outOfStockQuery = 'SELECT "ProductoId", "Nombre", "Stock", "Activo" FROM "Productos" WHERE "Stock" = 0 ORDER BY "ProductoId";'

$env:PGPASSWORD = $SupabasePassword
& $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -c $outOfStockQuery 2>&1 | Where-Object { $_ -notmatch 'WARNING' } | Write-Host
Write-Host ""

# 5. Mostrar productos por categoría
Write-Host "?? Productos por categoría:" -ForegroundColor Cyan

$categoryQuery = 'SELECT c."Nombre" as categoria, COUNT(p."ProductoId") as cantidad_productos FROM "Categorias" c LEFT JOIN "Productos" p ON p."CategoriaId" = c."CategoriaId" GROUP BY c."Nombre" ORDER BY cantidad_productos DESC;'

$env:PGPASSWORD = $SupabasePassword
& $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -c $categoryQuery 2>&1 | Where-Object { $_ -notmatch 'WARNING' } | Write-Host
Write-Host ""

# 6. Comparar con BD Local
Write-Host "?? Comparación con BD Local:" -ForegroundColor Cyan

$LocalHost = "localhost"
$LocalDB = "ComputerStoreDB"
$LocalUser = "postgres"
$LocalPassword = "Abcdefghij123"

$env:PGPASSWORD = $LocalPassword
$localResult = & $psql -h $LocalHost -U $LocalUser -d $LocalDB -t -c 'SELECT COUNT(*) FROM "Productos"' 2>&1

# Filtrar solo números
$localCount = "0"
foreach ($line in $localResult) {
    if ($line -match '^\s*\d+\s*$') {
        $localCount = $line.ToString().Trim()
        break
    }
}

if ($LASTEXITCODE -eq 0 -and [int]$localCount -gt 0) {
    Write-Host " Local: $localCount productos" -ForegroundColor Gray
    Write-Host "   Supabase: $totalProducts productos" -ForegroundColor Gray
    
    if ($localCount -eq $totalProducts) {
        Write-Host "   ? Cantidades coinciden`n" -ForegroundColor Green
    } else {
     $difference = [int]$localCount - [int]$totalProducts
      Write-Host "   ? Diferencia: $difference productos faltan en Supabase`n" -ForegroundColor Red
        
  Write-Host "?? Solución:" -ForegroundColor Yellow
        Write-Host "   Ejecuta: .\migrate-db-complete.ps1`n" -ForegroundColor Cyan
    }
} else {
    Write-Host "   ??  No se pudo conectar a BD local`n" -ForegroundColor Yellow
}

Remove-Item Env:\PGPASSWORD

# Resumen
Write-Host "========================================" -ForegroundColor Green
Write-Host "?? RESUMEN" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

if ([int]$totalProducts -gt 0) {
    Write-Host "? Supabase tiene $totalProducts productos" -ForegroundColor Green
  Write-Host "? La base de datos está configurada`n" -ForegroundColor Green
    
    # Verificar si hay productos activos con stock
  $env:PGPASSWORD = $SupabasePassword
    $activeResult = & $psql -h $SupabaseHost -U $SupabaseUser -d $SupabaseDB -p $SupabasePort -t -c 'SELECT COUNT(*) FROM "Productos" WHERE "Stock" > 0 AND "Activo" = true;' 2>&1
    
    $activeWithStock = "0"
    foreach ($line in $activeResult) {
      if ($line -match '^\s*\d+\s*$') {
        $activeWithStock = $line.ToString().Trim()
    break
      }
    }
    Remove-Item Env:\PGPASSWORD
    
    if ([int]$activeWithStock -eq 0) {
 Write-Host "??  PROBLEMA: No hay productos activos con stock" -ForegroundColor Red
 Write-Host "   Esto explica por qué solo ves productos 'Agotado' en Azure`n" -ForegroundColor Yellow
    
        Write-Host "? Solución:" -ForegroundColor Green
    Write-Host "   1. Ejecuta: .\migrate-db-complete.ps1" -ForegroundColor Cyan
        Write-Host "   2. Luego: .\deploy-app.ps1`n" -ForegroundColor Cyan
    } else {
        Write-Host "? Hay $activeWithStock productos activos con stock" -ForegroundColor Green
        Write-Host "   Si no aparecen en Azure, ejecuta:" -ForegroundColor Cyan
        Write-Host "   1. .\fix-azure-connection.ps1" -ForegroundColor Gray
        Write-Host "   2. .\deploy-app.ps1`n" -ForegroundColor Gray
    }
} else {
    Write-Host "? Supabase NO tiene productos" -ForegroundColor Red
    Write-Host "   Ejecuta: .\migrate-db-complete.ps1`n" -ForegroundColor Cyan
}
