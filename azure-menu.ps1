# Script de Acceso Rápido - CompuHiperMegaRed
# Menú interactivo para gestionar tu deployment en Azure

$ResourceGroup = "CompuHiperMegaRed"
$WebAppName = "compuhipermegared"
$WebAppUrl = "https://compuhipermegared-bfa4huctcxhzbdbk.canadacentral-01.azurewebsites.net"

function Show-Menu {
    Clear-Host
    Write-Host "?????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "?       ?? CompuHiperMegaRed - Azure Management          ?" -ForegroundColor Cyan
    Write-Host "?????????????????????????????????????????????????????????????" -ForegroundColor Cyan
 Write-Host ""
    Write-Host "?? URL: $WebAppUrl" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1??  Abrir Sitio Web" -ForegroundColor Green
    Write-Host "2??  Ver Estado del App Service" -ForegroundColor Green
    Write-Host "3??  Desplegar Nueva Versión" -ForegroundColor Green
    Write-Host "4??  Reiniciar Aplicación" -ForegroundColor Green
    Write-Host "5??  Ver Logs en Tiempo Real" -ForegroundColor Green
    Write-Host "6??  Descargar Logs" -ForegroundColor Green
    Write-Host "7??  Verificar Configuración" -ForegroundColor Green
    Write-Host "8??  Ver Variables de Entorno" -ForegroundColor Green
    Write-Host "9??  Abrir Azure Portal" -ForegroundColor Green
    Write-Host "0??  Salir" -ForegroundColor Red
    Write-Host ""
}

function Open-Website {
    Write-Host "?? Abriendo sitio web..." -ForegroundColor Cyan
    Start-Process $WebAppUrl
    Write-Host "? Sitio abierto en el navegador" -ForegroundColor Green
Start-Sleep -Seconds 2
}

function Show-AppStatus {
    Write-Host "?? Consultando estado..." -ForegroundColor Cyan
    $state = az webapp show --resource-group $ResourceGroup --name $WebAppName --query "state" -o tsv
    $location = az webapp show --resource-group $ResourceGroup --name $WebAppName --query "location" -o tsv
    $sku = az appservice plan show --resource-group $ResourceGroup --name "ASP-CompuHiperMegaRed-ac32" --query "sku.name" -o tsv
    
  Write-Host ""
    Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "Estado: $state" -ForegroundColor $(if ($state -eq "Running") { "Green" } else { "Red" })
  Write-Host "Ubicación: $location" -ForegroundColor Yellow
    Write-Host "SKU: $sku" -ForegroundColor Yellow
    Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "Presiona Enter para continuar"
}

function Deploy-App {
    Write-Host "?? Iniciando deployment..." -ForegroundColor Cyan
    .\deploy-app.ps1
    Read-Host "Presiona Enter para continuar"
}

function Restart-App {
    Write-Host "?? Reiniciando aplicación..." -ForegroundColor Cyan
    az webapp restart --resource-group $ResourceGroup --name $WebAppName
    Write-Host "? Aplicación reiniciada" -ForegroundColor Green
    Start-Sleep -Seconds 2
}

function Show-Logs {
    Write-Host "?? Mostrando logs en tiempo real..." -ForegroundColor Cyan
    Write-Host "   (Presiona Ctrl+C para detener)" -ForegroundColor Yellow
    Write-Host ""
    az webapp log tail --resource-group $ResourceGroup --name $WebAppName
}

function Download-Logs {
    $filename = "logs-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip"
    Write-Host "?? Descargando logs..." -ForegroundColor Cyan
    az webapp log download --resource-group $ResourceGroup --name $WebAppName --log-file $filename
    Write-Host "? Logs descargados: $filename" -ForegroundColor Green
  Start-Sleep -Seconds 2
}

function Verify-Config {
  Write-Host "?? Verificando configuración..." -ForegroundColor Cyan
    .\verify-deployment.ps1
    Read-Host "Presiona Enter para continuar"
}

function Show-EnvVars {
    Write-Host "??  Obteniendo variables de entorno..." -ForegroundColor Cyan
    Write-Host ""
    Write-Host "??? APP SETTINGS ???" -ForegroundColor Yellow
    az webapp config appsettings list --resource-group $ResourceGroup --name $WebAppName --query "[].{Name:name, Value:value}" -o table
    Write-Host ""
    Write-Host "??? CONNECTION STRINGS ???" -ForegroundColor Yellow
    az webapp config connection-string list --resource-group $ResourceGroup --name $WebAppName --query "[].{Name:name, Type:type}" -o table
    Write-Host ""
    Read-Host "Presiona Enter para continuar"
}

function Open-AzurePortal {
    Write-Host "?? Abriendo Azure Portal..." -ForegroundColor Cyan
    $portalUrl = "https://portal.azure.com/#@/resource/subscriptions/6ec25c44-eca0-4c0e-9d8c-34775b166cae/resourceGroups/CompuHiperMegaRed/providers/Microsoft.Web/sites/compuhipermegared/appServices"
    Start-Process $portalUrl
    Write-Host "? Portal abierto en el navegador" -ForegroundColor Green
    Start-Sleep -Seconds 2
}

# Menú Principal
do {
    Show-Menu
    $choice = Read-Host "Selecciona una opción"
    
    switch ($choice) {
        '1' { Open-Website }
     '2' { Show-AppStatus }
      '3' { Deploy-App }
        '4' { Restart-App }
      '5' { Show-Logs }
        '6' { Download-Logs }
 '7' { Verify-Config }
    '8' { Show-EnvVars }
    '9' { Open-AzurePortal }
   '0' { 
Write-Host ""
    Write-Host "?? ¡Hasta luego!" -ForegroundColor Cyan
            Write-Host ""
      exit
  }
 default {
            Write-Host "? Opción inválida" -ForegroundColor Red
   Start-Sleep -Seconds 1
        }
    }
} while ($true)
