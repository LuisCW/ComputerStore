# Script Maestro - Solucionar y Desplegar en Azure
# Este script ejecuta todo el proceso de corrección automáticamente

Write-Host "========================================" -ForegroundColor Green
Write-Host "?? PROCESO COMPLETO DE CORRECCIÓN" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

$ErrorActionPreference = "Continue"

# Configuración
$ResourceGroup = "CompuHiperMegaRed"
$WebAppName = "compuhipermegared"

Write-Host "?? Este script ejecutará:" -ForegroundColor Cyan
Write-Host "   1??  Diagnóstico de Supabase"
Write-Host "   2??  Corrección de configuración en Azure"
Write-Host "   3??  Re-despliegue de la aplicación"
Write-Host "   4??  Verificación final`n"

$confirm = Read-Host "¿Deseas continuar? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "? Operación cancelada" -ForegroundColor Red
  exit
}

# ========================================
# PASO 1: DIAGNÓSTICO
# ========================================
Write-Host "`n========================================" -ForegroundColor Yellow
Write-Host "1??  DIAGNÓSTICO" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Yellow

Write-Host "?? Verificando conexión a Supabase..." -ForegroundColor Cyan

$SupabaseHost = "db.xakfuxhafqbelwankypo.supabase.co"
$SupabasePort = 5432

try {
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    $tcpClient.Connect($SupabaseHost, $SupabasePort)
    $tcpClient.Close()
    Write-Host "? Supabase es accesible`n" -ForegroundColor Green
} catch {
    Write-Host "??  No se puede conectar a Supabase directamente" -ForegroundColor Yellow
    Write-Host "   Esto es normal si hay firewall. Continuando...`n" -ForegroundColor Gray
}

Write-Host "?? Verificando estado de la aplicación en Azure..." -ForegroundColor Cyan

$appState = az webapp show `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query state `
    -o tsv 2>&1

if ($appState -eq "Running") {
    Write-Host "? Aplicación en estado: Running`n" -ForegroundColor Green
} else {
    Write-Host "??  Aplicación en estado: $appState`n" -ForegroundColor Yellow
}

# ========================================
# PASO 2: CORRECCIÓN DE CONFIGURACIÓN
# ========================================
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "2??  CORRECCIÓN DE CONFIGURACIÓN" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Yellow

$SupabaseConnectionString = "Host=db.xakfuxhafqbelwankypo.supabase.co;Database=postgres;Username=postgres;Password=CwJ4jyBHT2FBX_x;Port=5432;SSL Mode=Require;Trust Server Certificate=true"

Write-Host "?? Configurando Connection String en Azure..." -ForegroundColor Cyan

az webapp config connection-string set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings DefaultConnection="$SupabaseConnectionString" `
    --connection-string-type PostgreSQL | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Connection String configurado correctamente`n" -ForegroundColor Green
} else {
    Write-Host "? Error al configurar Connection String" -ForegroundColor Red
    Write-Host "   Verifica que estés autenticado en Azure (az login)`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "?? Habilitando logging detallado..." -ForegroundColor Cyan

az webapp log config `
    --resource-group $ResourceGroup `
--name $WebAppName `
    --application-logging filesystem `
    --level information `
    --docker-container-logging filesystem | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Logging habilitado`n" -ForegroundColor Green
} else {
    Write-Host "??  Advertencia al configurar logging (continuando...)`n" -ForegroundColor Yellow
}

# ========================================
# PASO 3: RE-DESPLIEGUE
# ========================================
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "3??  RE-DESPLIEGUE DE LA APLICACIÓN" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Yellow

Write-Host "???  Compilando aplicación..." -ForegroundColor Cyan

dotnet clean | Out-Null
dotnet publish -c Release -o ./publish 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Compilación exitosa`n" -ForegroundColor Green
} else {
    Write-Host "? Error en la compilación" -ForegroundColor Red
    Write-Host "   Ejecuta 'dotnet build' para ver los errores`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "?? Creando paquete ZIP..." -ForegroundColor Cyan

Push-Location publish
Compress-Archive -Path * -DestinationPath ../app.zip -Force
Pop-Location

Write-Host "? Paquete creado`n" -ForegroundColor Green

Write-Host "??  Desplegando a Azure (esto puede tardar 2-3 minutos)..." -ForegroundColor Cyan

az webapp deployment source config-zip `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --src app.zip 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Despliegue exitoso`n" -ForegroundColor Green
} else {
    Write-Host "? Error en el despliegue" -ForegroundColor Red
    Write-Host "   Revisa los logs con: az webapp log tail --name $WebAppName --resource-group $ResourceGroup`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "?? Reiniciando aplicación..." -ForegroundColor Cyan

az webapp restart --name $WebAppName --resource-group $ResourceGroup | Out-Null

Write-Host "? Aplicación reiniciada`n" -ForegroundColor Green

# Cleanup
Remove-Item -Path ./publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ./app.zip -Force -ErrorAction SilentlyContinue

# ========================================
# PASO 4: VERIFICACIÓN
# ========================================
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "4??  VERIFICACIÓN FINAL" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Yellow

Write-Host "? Esperando que la aplicación inicie (30 segundos)..." -ForegroundColor Cyan
Start-Sleep -Seconds 30

Write-Host "?? Verificando estado..." -ForegroundColor Cyan

$finalState = az webapp show `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query state `
    -o tsv 2>&1

Write-Host "?? Estado final: $finalState`n" -ForegroundColor $(if ($finalState -eq "Running") { "Green" } else { "Red" })

# ========================================
# RESULTADO FINAL
# ========================================
Write-Host "========================================" -ForegroundColor Green
Write-Host "?? PROCESO COMPLETADO" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Write-Host "? Configuración corregida" -ForegroundColor Green
Write-Host "? Aplicación desplegada" -ForegroundColor Green
Write-Host "? Logs habilitados`n" -ForegroundColor Green

Write-Host "?? URL de la aplicación:" -ForegroundColor Cyan
Write-Host "   https://$WebAppName.azurewebsites.net`n" -ForegroundColor White

Write-Host "?? Credenciales de Admin:" -ForegroundColor Cyan
Write-Host "   Email: admin@computerhipermegared.com" -ForegroundColor White
Write-Host "   Password: E_commerce123$`n" -ForegroundColor White

Write-Host "?? Próximos pasos:`n" -ForegroundColor Yellow

Write-Host "1??  Abre la aplicación en tu navegador:" -ForegroundColor Cyan
Write-Host "   start https://$WebAppName.azurewebsites.net`n" -ForegroundColor Gray

Write-Host "2??  Si ves errores, descarga los logs:" -ForegroundColor Cyan
Write-Host "   az webapp log download --name $WebAppName --resource-group $ResourceGroup --log-file logs.zip`n" -ForegroundColor Gray

Write-Host "3??  Busca en los logs estos mensajes:" -ForegroundColor Cyan
Write-Host "   ? 'Connection String encontrado'" -ForegroundColor Green
Write-Host "   ? 'Conexión a base de datos exitosa'" -ForegroundColor Green
Write-Host "   ? 'Base de datos inicializada'" -ForegroundColor Green
Write-Host "   ? 'Aplicación iniciada correctamente'`n" -ForegroundColor Green

Write-Host "4??  Si hay problemas, ejecuta el diagnóstico detallado:" -ForegroundColor Cyan
Write-Host "   .\diagnose-supabase.ps1`n" -ForegroundColor Gray

# Preguntar si quiere abrir el navegador
$openBrowser = Read-Host "¿Deseas abrir la aplicación en el navegador ahora? (S/N)"
if ($openBrowser -eq "S" -or $openBrowser -eq "s") {
    Write-Host "`n?? Abriendo navegador..." -ForegroundColor Cyan
    start "https://$WebAppName.azurewebsites.net"
}

# Preguntar si quiere ver logs en tiempo real
Write-Host ""
$showLogs = Read-Host "¿Deseas ver los logs en tiempo real? (S/N)"
if ($showLogs -eq "S" -or $showLogs -eq "s") {
    Write-Host "`n?? Logs en tiempo real (Ctrl+C para salir):`n" -ForegroundColor Cyan
    Write-Host "Busca mensajes con:" -ForegroundColor Yellow
  Write-Host "  ?? = Diagnóstico" -ForegroundColor Gray
 Write-Host "  ? = Éxito" -ForegroundColor Gray
    Write-Host "  ? = Error`n" -ForegroundColor Gray
    
    Start-Sleep -Seconds 2
    az webapp log tail --name $WebAppName --resource-group $ResourceGroup
}

Write-Host "`n? ¡Listo! Tu aplicación debería estar funcionando." -ForegroundColor Green
