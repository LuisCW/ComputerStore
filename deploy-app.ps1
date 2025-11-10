# Script para Publicar la Aplicación en Azure
# Ejecutar después de deploy-azure.ps1

Write-Host "?? Publicando aplicación en Azure..." -ForegroundColor Green

$ResourceGroup = "CompuHiperMegaRed"
$WebAppName = "compuhipermegared"

# 1. Compilar en modo Release
Write-Host "`n?? Compilando aplicación..." -ForegroundColor Cyan
dotnet clean
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error en la compilación" -ForegroundColor Red
    exit 1
}

# 2. Crear archivo ZIP
Write-Host "`n?? Creando paquete ZIP..." -ForegroundColor Cyan
Push-Location publish
Compress-Archive -Path * -DestinationPath ../app.zip -Force
Pop-Location

# 3. Desplegar a Azure
Write-Host "`n?? Desplegando a Azure..." -ForegroundColor Cyan
az webapp deployment source config-zip `
--resource-group $ResourceGroup `
    --name $WebAppName `
    --src app.zip

# 4. Reiniciar Web App
Write-Host "`n?? Reiniciando aplicación..." -ForegroundColor Cyan
az webapp restart --name $WebAppName --resource-group $ResourceGroup

# 5. Verificar deployment
Write-Host "`n? Deployment completado!" -ForegroundColor Green
Write-Host "`n?? URL: https://$WebAppName.azurewebsites.net" -ForegroundColor Cyan
Write-Host "`n?? Ver logs:" -ForegroundColor Yellow
Write-Host "   az webapp log tail --name $WebAppName --resource-group $ResourceGroup" -ForegroundColor Gray

# Cleanup
Remove-Item -Path ./publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ./app.zip -Force -ErrorAction SilentlyContinue

Write-Host "`n?? ¡Aplicación desplegada exitosamente!" -ForegroundColor Green
