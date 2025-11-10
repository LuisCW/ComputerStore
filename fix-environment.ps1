# Script para corregir el ambiente a Production
Write-Host "?? Configurando ASPNETCORE_ENVIRONMENT a Production..." -ForegroundColor Green

$ResourceGroup = "CompuHiperMegaRed"
$WebAppName = "compuhipermegared"

# Método alternativo usando Azure REST API
Write-Host "`n?? Actualizando configuración..." -ForegroundColor Cyan

# Obtener la configuración actual
$settings = az webapp config appsettings list --resource-group $ResourceGroup --name $WebAppName | ConvertFrom-Json

# Crear un hashtable con la configuración
$newSettings = @{}
foreach ($setting in $settings) {
$newSettings[$setting.name] = $setting.value
}

# Actualizar o agregar ASPNETCORE_ENVIRONMENT
$newSettings["ASPNETCORE_ENVIRONMENT"] = "Production"

# Convertir a array de strings en formato key=value
$settingsArray = @()
foreach ($key in $newSettings.Keys) {
    if ($newSettings[$key]) {
        $settingsArray += "$key=$($newSettings[$key])"
    }
}

# Aplicar configuración
Write-Host "`n? Aplicando configuración..." -ForegroundColor Cyan
az webapp config appsettings set `
 --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings $settingsArray

# Reiniciar app
Write-Host "`n?? Reiniciando aplicación..." -ForegroundColor Cyan
az webapp restart --name $WebAppName --resource-group $ResourceGroup

Write-Host "`n? ¡Configuración actualizada!" -ForegroundColor Green
Write-Host "`n?? Espera 30-60 segundos y recarga: https://compuhipermegared-bfa4huctcxhzbdbk.canadacentral-01.azurewebsites.net" -ForegroundColor Cyan
Write-Host "`n??  El error de 'Modo de desarrollo' debería desaparecer" -ForegroundColor Magenta
