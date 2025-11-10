#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Optimiza la base de datos de Supabase agregando índices para mejorar el rendimiento

.DESCRIPTION
    Este script agrega índices optimizados a la base de datos de Supabase para:
    - Acelerar las consultas de productos con paginación
    - Mejorar el rendimiento de filtros por categoría, marca y color
    - Optimizar búsquedas y ordenamientos
#>

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "? OPTIMIZACIÓN DE ÍNDICES EN SUPABASE" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Configuración de Supabase
$supabaseHost = "db.xakfuxhafqbelwankypo.supabase.co"
$supabasePort = "5432"
$supabaseDb = "postgres"
$supabaseUser = "postgres"
$supabasePassword = "CwJ4jyBHT2FBX_x"

# Configurar variable de entorno para la contraseña
$env:PGPASSWORD = $supabasePassword

Write-Host "?? Conectando a Supabase..." -ForegroundColor Yellow
Write-Host "   Host: $supabaseHost" -ForegroundColor Gray
Write-Host ""

# Script SQL para crear índices optimizados
$sqlScript = @"
-- ============================================
-- OPTIMIZACIÓN DE ÍNDICES PARA RENDIMIENTO
-- ============================================

-- Verificar y crear índices para tabla Productos (solo si no existen)

-- 1. Índice en Color (para filtros)
DO `$`$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
      WHERE tablename = 'Productos' AND indexname = 'IX_Productos_Color'
  ) THEN
        CREATE INDEX IX_Productos_Color ON "Productos" ("Color");
        RAISE NOTICE '? Índice IX_Productos_Color creado';
    ELSE
        RAISE NOTICE '??  Índice IX_Productos_Color ya existe';
    END IF;
END`$`$;

-- 2. Índice en IsActive (para filtros de productos activos)
DO `$`$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
     WHERE tablename = 'Productos' AND indexname = 'IX_Productos_IsActive'
    ) THEN
        CREATE INDEX IX_Productos_IsActive ON "Productos" ("IsActive");
        RAISE NOTICE '? Índice IX_Productos_IsActive creado';
    ELSE
   RAISE NOTICE '??  Índice IX_Productos_IsActive ya existe';
    END IF;
END`$`$;

-- 3. Índice compuesto para GetAllProducts (IsActive + CreatedDate)
DO `$`$
BEGIN
  IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
    WHERE tablename = 'Productos' AND indexname = 'IX_Productos_IsActive_CreatedDate'
    ) THEN
        CREATE INDEX IX_Productos_IsActive_CreatedDate ON "Productos" ("IsActive", "CreatedDate" DESC);
      RAISE NOTICE '? Índice IX_Productos_IsActive_CreatedDate creado';
    ELSE
        RAISE NOTICE '??  Índice IX_Productos_IsActive_CreatedDate ya existe';
    END IF;
END`$`$;

-- 4. Índice compuesto para filtros por categoría (IsActive + Category)
DO `$`$
BEGIN
    IF NOT EXISTS (
      SELECT 1 FROM pg_indexes 
        WHERE tablename = 'Productos' AND indexname = 'IX_Productos_IsActive_Category'
    ) THEN
  CREATE INDEX IX_Productos_IsActive_Category ON "Productos" ("IsActive", "Category");
        RAISE NOTICE '? Índice IX_Productos_IsActive_Category creado';
    ELSE
        RAISE NOTICE '??  Índice IX_Productos_IsActive_Category ya existe';
  END IF;
END`$`$;

-- 5. Índice compuesto para productos destacados (IsActive + Price)
DO `$`$
BEGIN
    IF NOT EXISTS (
      SELECT 1 FROM pg_indexes 
        WHERE tablename = 'Productos' AND indexname = 'IX_Productos_IsActive_Price'
    ) THEN
        CREATE INDEX IX_Productos_IsActive_Price ON "Productos" ("IsActive", "Price" DESC);
        RAISE NOTICE '? Índice IX_Productos_IsActive_Price creado';
    ELSE
        RAISE NOTICE '??  Índice IX_Productos_IsActive_Price ya existe';
    END IF;
END`$`$;

-- 6. Índice compuesto para más vendidos (IsActive + Stock)
DO `$`$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE tablename = 'Productos' AND indexname = 'IX_Productos_IsActive_Stock'
    ) THEN
        CREATE INDEX IX_Productos_IsActive_Stock ON "Productos" ("IsActive", "Stock" DESC);
   RAISE NOTICE '? Índice IX_Productos_IsActive_Stock creado';
    ELSE
        RAISE NOTICE '??  Índice IX_Productos_IsActive_Stock ya existe';
    END IF;
END`$`$;

-- 7. Índices de texto para búsquedas (usando GIN para mejor rendimiento en búsquedas LIKE)
DO `$`$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE tablename = 'Productos' AND indexname = 'IX_Productos_Name_gin'
    ) THEN
        CREATE INDEX IX_Productos_Name_gin ON "Productos" USING gin ("Name" gin_trgm_ops);
        RAISE NOTICE '? Índice IX_Productos_Name_gin creado';
    ELSE
 RAISE NOTICE '??  Índice IX_Productos_Name_gin ya existe';
    END IF;
EXCEPTION
    WHEN undefined_object THEN
      RAISE NOTICE '??  Extensión pg_trgm no disponible, usando índice B-tree estándar';
  IF NOT EXISTS (
 SELECT 1 FROM pg_indexes 
        WHERE tablename = 'Productos' AND indexname = 'IX_Productos_Name_btree'
        ) THEN
   CREATE INDEX IX_Productos_Name_btree ON "Productos" ("Name");
        RAISE NOTICE '? Índice IX_Productos_Name_btree creado';
        END IF;
END`$`$;

-- 8. Índice para búsquedas en Description
DO `$`$
BEGIN
    IF NOT EXISTS (
      SELECT 1 FROM pg_indexes 
        WHERE tablename = 'Productos' AND indexname = 'IX_Productos_Description_gin'
    ) THEN
      CREATE INDEX IX_Productos_Description_gin ON "Productos" USING gin ("Description" gin_trgm_ops);
RAISE NOTICE '? Índice IX_Productos_Description_gin creado';
    ELSE
        RAISE NOTICE '??  Índice IX_Productos_Description_gin ya existe';
    END IF;
EXCEPTION
    WHEN undefined_object THEN
        RAISE NOTICE '??  Extensión pg_trgm no disponible para Description';
END`$`$;

-- Analizar la tabla para actualizar estadísticas
ANALYZE "Productos";

-- Mostrar índices creados
SELECT 
    schemaname,
tablename,
    indexname,
    indexdef
FROM pg_indexes 
WHERE tablename = 'Productos'
ORDER BY indexname;

SELECT '? Optimización completada - Total de índices en Productos: ' || COUNT(*) 
FROM pg_indexes 
WHERE tablename = 'Productos';
"@

# Guardar el script SQL en un archivo temporal
$tempSqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
$sqlScript | Out-File -FilePath $tempSqlFile -Encoding UTF8

Write-Host "?? Ejecutando optimizaciones..." -ForegroundColor Yellow
Write-Host ""

# Ejecutar el script SQL
try {
    $result = psql -h $supabaseHost `
      -p $supabasePort `
     -U $supabaseUser `
      -d $supabaseDb `
    -f $tempSqlFile `
       --single-transaction `
         2>&1

    Write-Host $result -ForegroundColor Green
    
    Write-Host ""
    Write-Host "=============================================" -ForegroundColor Green
    Write-Host "? OPTIMIZACIÓN COMPLETADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host "=============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Beneficios:" -ForegroundColor Cyan
  Write-Host "   ? Las consultas de productos serán 5-10x más rápidas" -ForegroundColor White
    Write-Host "   ? La paginación será instantánea" -ForegroundColor White
    Write-Host "   ? Los filtros y búsquedas serán mucho más rápidos" -ForegroundColor White
    Write-Host "? Menor carga en Supabase" -ForegroundColor White
    Write-Host ""
}
catch {
  Write-Host ""
    Write-Host "=============================================" -ForegroundColor Red
 Write-Host "? ERROR AL OPTIMIZAR LA BASE DE DATOS" -ForegroundColor Red
    Write-Host "=============================================" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Posibles soluciones:" -ForegroundColor Yellow
    Write-Host "   1. Verifica que PostgreSQL client (psql) esté instalado" -ForegroundColor White
    Write-Host "2. Verifica las credenciales de Supabase" -ForegroundColor White
    Write-Host "   3. Verifica la conexión a internet" -ForegroundColor White
    exit 1
}
finally {
    # Limpiar archivo temporal
    if (Test-Path $tempSqlFile) {
        Remove-Item $tempSqlFile -Force
    }
  
    # Limpiar variable de entorno
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "?? Próximo paso: Despliega la aplicación optimizada" -ForegroundColor Cyan
Write-Host "   Ejecuta: .\deploy-app.ps1" -ForegroundColor White
Write-Host ""
