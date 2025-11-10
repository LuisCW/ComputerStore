# Script para Corregir Configuración de Connection String en Azure
# Este script asegura que el connection string esté configurado correctamente

Write-Host "?? CORRIGIENDO CONFIGURACIÓN DE AZURE" -ForegroundColor Green
Write-Host "======================================`n" -ForegroundColor Green

$ResourceGroup = "CompuHiperMegaRed"
$WebAppName = "compuhipermegared"

# Supabase Connection String
$SupabaseConnectionString = "Host=db.xakfuxhafqbelwankypo.supabase.co;Database=postgres;Username=postgres;Password=CwJ4jyBHT2FBX_x;Port=5432;SSL Mode=Require;Trust Server Certificate=true"

Write-Host "?? Configuración:" -ForegroundColor Cyan
Write-Host "   Resource Group: $ResourceGroup"
Write-Host "   Web App: $WebAppName"
Write-Host "   Base de Datos: Supabase PostgreSQL`n"

# 1. Verificar estado actual
Write-Host "?? Verificando configuración actual..." -ForegroundColor Yellow

$currentConnString = az webapp config connection-string list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='DefaultConnection'].value" `
    -o tsv

if ($currentConnString) {
    Write-Host "? Connection String actual encontrado" -ForegroundColor Green
    $safeConnString = $currentConnString -replace "Password=[^;]+", "Password=***"
    Write-Host "$safeConnString`n" -ForegroundColor Gray
} else {
  Write-Host "? Connection String NO encontrado`n" -ForegroundColor Red
}

# 2. Configurar Connection String como PostgreSQL
Write-Host "?? Configurando Connection String (PostgreSQL)..." -ForegroundColor Cyan

az webapp config connection-string set `
  --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings DefaultConnection="$SupabaseConnectionString" `
    --connection-string-type PostgreSQL | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Connection String configurado correctamente`n" -ForegroundColor Green
} else {
    Write-Host "? Error al configurar Connection String`n" -ForegroundColor Red
    exit 1
}

# 3. Verificar otras variables críticas
Write-Host "?? Verificando otras variables de entorno..." -ForegroundColor Yellow

$criticalSettings = @(
    "ASPNETCORE_ENVIRONMENT",
    "PayU__ApiKey",
    "EmailSettings__SmtpServer",
    "Encryption__Key"
)

$settings = az webapp config appsettings list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[].{Name:name, Value:value}" `
    -o json | ConvertFrom-Json

foreach ($setting in $criticalSettings) {
    $value = $settings | Where-Object { $_.Name -eq $setting } | Select-Object -ExpandProperty Value
    
    if ($value) {
        $displayValue = if ($setting -like "*Key*" -or $setting -like "*Password*") { "***" } else { $value }
        Write-Host "   ? $setting : $displayValue" -ForegroundColor Green
    } else {
      Write-Host "   ? $setting : NO CONFIGURADO" -ForegroundColor Red
    }
}

# 4. Habilitar logging detallado
Write-Host "`n?? Habilitando logging detallado..." -ForegroundColor Cyan

az webapp log config `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --application-logging filesystem `
    --level information `
    --docker-container-logging filesystem | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Logging habilitado`n" -ForegroundColor Green
} else {
    Write-Host "??  Advertencia al configurar logging`n" -ForegroundColor Yellow
}

# 5. Reiniciar aplicación para aplicar cambios
Write-Host "?? Reiniciando aplicación..." -ForegroundColor Cyan

az webapp restart `
  --resource-group $ResourceGroup `
    --name $WebAppName | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Aplicación reiniciada`n" -ForegroundColor Green
} else {
    Write-Host "? Error al reiniciar aplicación`n" -ForegroundColor Red
    exit 1
}

# 6. Esperar y verificar estado
Write-Host "? Esperando que la aplicación inicie (30 segundos)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

$state = az webapp show `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query state `
    -o tsv

Write-Host "?? Estado de la aplicación: $state`n" -ForegroundColor $(if ($state -eq "Running") { "Green" } else { "Red" })

# 7. Mostrar instrucciones para ver logs
Write-Host "======================================" -ForegroundColor Green
Write-Host "? CONFIGURACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "======================================`n" -ForegroundColor Green

Write-Host "?? Próximos pasos:`n" -ForegroundColor Cyan

Write-Host "1??  Verificar la aplicación:" -ForegroundColor Yellow
Write-Host "   https://$WebAppName.azurewebsites.net`n" -ForegroundColor Gray

Write-Host "2??  Ver logs en tiempo real:" -ForegroundColor Yellow
Write-Host "   az webapp log tail --name $WebAppName --resource-group $ResourceGroup`n" -ForegroundColor Gray

Write-Host "3??  Descargar logs completos:" -ForegroundColor Yellow
Write-Host "   az webapp log download --name $WebAppName --resource-group $ResourceGroup --log-file logs.zip`n" -ForegroundColor Gray

Write-Host "4??  Si hay errores, volver a desplegar:" -ForegroundColor Yellow
Write-Host "   .\deploy-app.ps1`n" -ForegroundColor Gray

Write-Host "?? Esperando logs de inicio..." -ForegroundColor Cyan
Write-Host "Presiona Ctrl+C para salir cuando veas 'Application started'`n" -ForegroundColor Gray

# Opcional: Mostrar logs en tiempo real
$showLogs = Read-Host "¿Deseas ver los logs en tiempo real? (S/N)"
if ($showLogs -eq "S" -or $showLogs -eq "s") {
    Write-Host "`n?? Logs en tiempo real (Ctrl+C para salir):`n" -ForegroundColor Cyan
    az webapp log tail --name $WebAppName --resource-group $ResourceGroup
}
