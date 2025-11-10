# Script para Diagnosticar la Conexión a Supabase
# Este script verifica que la configuración de Supabase esté correcta

Write-Host "?? DIAGNÓSTICO DE CONEXIÓN A SUPABASE" -ForegroundColor Green
Write-Host "======================================`n" -ForegroundColor Green

# Información de Supabase
$SupabaseHost = "db.xakfuxhafqbelwankypo.supabase.co"
$SupabasePort = 5432
$SupabaseDb = "postgres"
$SupabaseUser = "postgres"
$SupabasePassword = "CwJ4jyBHT2FBX_x"

Write-Host "?? Información de Supabase:" -ForegroundColor Cyan
Write-Host "   Host: $SupabaseHost"
Write-Host "   Puerto: $SupabasePort"
Write-Host "   Base de Datos: $SupabaseDb"
Write-Host "   Usuario: $SupabaseUser"
Write-Host "   Password: ***`n"

# 1. Verificar conectividad de red
Write-Host "1??  Verificando conectividad de red..." -ForegroundColor Yellow

try {
  $tcpClient = New-Object System.Net.Sockets.TcpClient
    $tcpClient.Connect($SupabaseHost, $SupabasePort)
    $tcpClient.Close()
    Write-Host "   ? Puerto $SupabasePort accesible`n" -ForegroundColor Green
} catch {
    Write-Host "   ? No se puede conectar al puerto $SupabasePort" -ForegroundColor Red
Write-Host "   Error: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 2. Verificar resolución DNS
Write-Host "2??  Verificando resolución DNS..." -ForegroundColor Yellow

try {
    $dnsResult = Resolve-DnsName -Name $SupabaseHost -ErrorAction Stop
    Write-Host "   ? DNS resuelto correctamente" -ForegroundColor Green
    foreach ($ip in $dnsResult.IPAddress) {
        Write-Host "   ?? IP: $ip" -ForegroundColor Gray
    }
Write-Host ""
} catch {
    Write-Host "   ? Error al resolver DNS" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 3. Verificar que psql esté instalado
Write-Host "3??  Verificando herramientas PostgreSQL..." -ForegroundColor Yellow

$psqlPath = Get-Command psql -ErrorAction SilentlyContinue

if ($psqlPath) {
    Write-Host "   ? psql encontrado: $($psqlPath.Source)`n" -ForegroundColor Green
    
    # 4. Intentar conexión con psql
    Write-Host "4??  Intentando conexión con psql..." -ForegroundColor Yellow
  
    $env:PGPASSWORD = $SupabasePassword
    $connectionString = "postgresql://${SupabaseUser}@${SupabaseHost}:${SupabasePort}/${SupabaseDb}?sslmode=require"
    
  Write-Host "   ?? Conectando a Supabase..." -ForegroundColor Cyan
    
    # Ejecutar query simple
    $query = "SELECT version();"
  $result = psql $connectionString -c $query 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Conexión exitosa!" -ForegroundColor Green
        Write-Host "   ?? Versión PostgreSQL:" -ForegroundColor Cyan
        Write-Host "   $($result[2])`n" -ForegroundColor Gray
        
      # 5. Verificar tablas
        Write-Host "5??  Verificando tablas en la base de datos..." -ForegroundColor Yellow
        
        $tableQuery = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;"
        $tables = psql $connectionString -t -c $tableQuery 2>&1
        
        if ($LASTEXITCODE -eq 0) {
     Write-Host "   ?? Tablas encontradas:" -ForegroundColor Cyan
    $tables | ForEach-Object {
   $tableName = $_.Trim()
          if ($tableName) {
         Write-Host "   - $tableName" -ForegroundColor Gray
          }
  }
        Write-Host ""
            
          # 6. Contar registros en tablas principales
            Write-Host "6??  Contando registros..." -ForegroundColor Yellow
       
         $countQueries = @{
                "AspNetUsers" = 'SELECT COUNT(*) FROM "AspNetUsers";'
            "Productos" = 'SELECT COUNT(*) FROM "Productos";'
                "Categorias" = 'SELECT COUNT(*) FROM "Categorias";'
  "Pedidos" = 'SELECT COUNT(*) FROM "Pedidos";'
          "__EFMigrationsHistory" = 'SELECT COUNT(*) FROM "__EFMigrationsHistory";'
   }
            
          foreach ($table in $countQueries.Keys) {
  $countQuery = $countQueries[$table]
                $count = psql $connectionString -t -c $countQuery 2>&1
           
              if ($LASTEXITCODE -eq 0) {
          Write-Host "   $table : $($count.Trim()) registros" -ForegroundColor Gray
      } else {
        Write-Host "   $table : ??  Tabla no existe o error" -ForegroundColor Yellow
            }
   }
            Write-Host ""
        }
    } else {
      Write-Host "   ? Error al conectar a Supabase" -ForegroundColor Red
      Write-Host "   $result`n" -ForegroundColor Red
    }

    Remove-Item Env:\PGPASSWORD
    
} else {
    Write-Host "   ??  psql no encontrado`n" -ForegroundColor Yellow
    Write-Host "   Para instalar PostgreSQL client tools:" -ForegroundColor Cyan
    Write-Host "   - Windows: https://www.postgresql.org/download/windows/" -ForegroundColor Gray
    Write-Host "   - O con Chocolatey: choco install postgresql`n" -ForegroundColor Gray
}

# 7. Verificar configuración en Azure
Write-Host "7??  Verificando configuración en Azure..." -ForegroundColor Yellow

$ResourceGroup = "CompuHiperMegaRed"
$WebAppName = "compuhipermegared"

Write-Host "   ?? Obteniendo Connection String de Azure..." -ForegroundColor Cyan

$azureConnString = az webapp config connection-string list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='DefaultConnection'].{Type:type, Value:value}" `
    -o json 2>&1 | ConvertFrom-Json

if ($azureConnString) {
    Write-Host "   ? Connection String configurado en Azure" -ForegroundColor Green
 Write-Host "   ?? Tipo: $($azureConnString.Type)" -ForegroundColor Gray
    
    $safeValue = $azureConnString.Value -replace "Password=[^;]+", "Password=***"
 Write-Host "   ?? Valor: $safeValue`n" -ForegroundColor Gray
    
    # Comparar con el esperado
    $expectedConnString = "Host=$SupabaseHost;Database=$SupabaseDb;Username=$SupabaseUser;Password=$SupabasePassword;Port=$SupabasePort;SSL Mode=Require;Trust Server Certificate=true"
    
    if ($azureConnString.Value -eq $expectedConnString) {
        Write-Host "   ? Connection String coincide con Supabase`n" -ForegroundColor Green
    } else {
        Write-Host "   ??  Connection String NO coincide exactamente" -ForegroundColor Yellow
        Write-Host "   ?? Ejecuta fix-azure-connection.ps1 para corregir`n" -ForegroundColor Cyan
  }
} else {
    Write-Host "   ? Connection String NO encontrado en Azure" -ForegroundColor Red
    Write-Host "   ?? Ejecuta fix-azure-connection.ps1 para configurar`n" -ForegroundColor Cyan
}

# 8. Verificar estado de la aplicación
Write-Host "8??  Verificando estado de la aplicación en Azure..." -ForegroundColor Yellow

$appState = az webapp show `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query state `
    -o tsv 2>&1

if ($appState -eq "Running") {
    Write-Host "   ? Aplicación en estado: $appState`n" -ForegroundColor Green
} else {
    Write-Host "   ??  Aplicación en estado: $appState`n" -ForegroundColor Yellow
}

# Resumen
Write-Host "======================================" -ForegroundColor Green
Write-Host "?? RESUMEN DEL DIAGNÓSTICO" -ForegroundColor Green
Write-Host "======================================`n" -ForegroundColor Green

Write-Host "Si todo está ?, la base de datos debería funcionar." -ForegroundColor Cyan
Write-Host "Si hay ?, ejecuta estos scripts en orden:`n" -ForegroundColor Cyan

Write-Host "1??  .\fix-azure-connection.ps1    # Corregir configuración" -ForegroundColor Yellow
Write-Host "2??  .\deploy-app.ps1        # Re-desplegar aplicación`n" -ForegroundColor Yellow

Write-Host "Para ver logs de la aplicación:" -ForegroundColor Cyan
Write-Host "   az webapp log tail --name $WebAppName --resource-group $ResourceGroup`n" -ForegroundColor Gray
