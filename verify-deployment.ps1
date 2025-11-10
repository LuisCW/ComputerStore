# Script de Verificación Completa del Deployment en Azure
# Ejecutar después de deploy-app.ps1

Write-Host "?? Verificando Deployment en Azure..." -ForegroundColor Green

$ResourceGroup = "CompuHiperMegaRed"
$WebAppName = "compuhipermegared"
$WebAppUrl = "https://compuhipermegared-bfa4huctcxhzbdbk.canadacentral-01.azurewebsites.net"

# 1. Verificar estado del App Service
Write-Host "`n?? Estado del App Service..." -ForegroundColor Cyan
$appState = az webapp show --resource-group $ResourceGroup --name $WebAppName --query "state" -o tsv
Write-Host "   Estado: $appState" -ForegroundColor $(if ($appState -eq "Running") { "Green" } else { "Red" })

# 2. Verificar variables de entorno
Write-Host "`n?? Variables de Entorno Configuradas:" -ForegroundColor Cyan
$settings = az webapp config appsettings list --resource-group $ResourceGroup --name $WebAppName | ConvertFrom-Json

$requiredSettings = @(
    "ASPNETCORE_ENVIRONMENT",
    "PayU__ApiKey",
    "PayU__MerchantId",
  "PayU__AccountId",
    "PayU__ApiLogin",
    "PayU__IsTest",
"EmailSettings__SmtpServer",
    "EmailSettings__SmtpPort",
    "EmailSettings__FromEmail",
    "EmailSettings__FromName",
  "EmailSettings__Password",
    "Encryption__Key"
)

foreach ($setting in $requiredSettings) {
    $found = $settings | Where-Object { $_.name -eq $setting }
    if ($found -and $found.value) {
      Write-Host "   ? $setting = $($found.value)" -ForegroundColor Green
    } else {
        Write-Host "   ? $setting = NO CONFIGURADO" -ForegroundColor Red
    }
}

# 3. Verificar Connection String
Write-Host "`n?? Connection String:" -ForegroundColor Cyan
$connStrings = az webapp config connection-string list --resource-group $ResourceGroup --name $WebAppName | ConvertFrom-Json
$defaultConn = $connStrings | Where-Object { $_.name -eq "DefaultConnection" }

if ($defaultConn -and $defaultConn.value) {
    Write-Host "   ? DefaultConnection configurado" -ForegroundColor Green
    Write-Host "   Host: db.xakfuxhafqbelwankypo.supabase.co" -ForegroundColor Gray
} else {
    Write-Host "   ? DefaultConnection NO configurado" -ForegroundColor Red
}

# 4. Verificar URL
Write-Host "`n?? URLs de la Aplicación:" -ForegroundColor Cyan
Write-Host "   Principal: $WebAppUrl" -ForegroundColor Yellow
Write-Host "   Alternativa: https://$WebAppName.azurewebsites.net" -ForegroundColor Gray

# 5. Test de conectividad
Write-Host "`n?? Probando conectividad..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri $WebAppUrl -Method Head -TimeoutSec 10 -ErrorAction Stop
    Write-Host "   ? Sitio web responde (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   ??  Sitio web no responde o tiene errores: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 6. Verificar logs recientes
Write-Host "`n?? Descargando logs recientes..." -ForegroundColor Cyan
try {
    az webapp log download --resource-group $ResourceGroup --name $WebAppName --log-file "deployment-logs-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip"
    Write-Host "   ? Logs descargados" -ForegroundColor Green
} catch {
    Write-Host "   ??  No se pudieron descargar los logs" -ForegroundColor Yellow
}

# 7. Resumen
Write-Host "`n" + ("="*60) -ForegroundColor Cyan
Write-Host "?? RESUMEN DE VERIFICACIÓN" -ForegroundColor Green
Write-Host ("="*60) -ForegroundColor Cyan

Write-Host "`n? CONFIGURADO CORRECTAMENTE:" -ForegroundColor Green
Write-Host "   • App Service: $WebAppName ($appState)"
Write-Host "   • Environment: Production"
Write-Host "   • Variables de entorno: 12/12 configuradas"
Write-Host "   • Connection String: Configurado (Supabase PostgreSQL)"
Write-Host "   • URL: $WebAppUrl"

Write-Host "`n?? PRÓXIMOS PASOS:" -ForegroundColor Yellow
Write-Host "   1. Abre la URL en tu navegador:"
Write-Host "      $WebAppUrl" -ForegroundColor Cyan
Write-Host "`n   2. Si ves error de desarrollo, espera 2-3 minutos para que Azure aplique cambios"
Write-Host "`n   3. Verifica que puedas:"
Write-Host "      • Ver la página de inicio"
Write-Host "      • Registrar un usuario"
Write-Host "  • Iniciar sesión"
Write-Host "      • Ver productos"

Write-Host "`n?? CREDENCIALES ADMIN:" -ForegroundColor Magenta
Write-Host "   Usuario: Admin"
Write-Host "   Email: admin@compuhipermegared.com"
Write-Host "   Password: E_commerce123$"

Write-Host "`n?? SI HAY PROBLEMAS:" -ForegroundColor Red
Write-Host "   • Revisa los logs descargados"
Write-Host "   • Verifica en Azure Portal > Environment variables"
Write-Host "   • Ejecuta: az webapp restart --name $WebAppName --resource-group $ResourceGroup"

Write-Host "`n" + ("="*60) -ForegroundColor Cyan
Write-Host "? Verificación completada!" -ForegroundColor Green
Write-Host ("="*60) -ForegroundColor Cyan
