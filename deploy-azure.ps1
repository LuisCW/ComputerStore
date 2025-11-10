# Azure Deployment Script para ComputerStore
# Ejecutar en PowerShell como Administrador

Write-Host "?? Iniciando Despliegue en Azure..." -ForegroundColor Green

# Variables
$ResourceGroup = "CompuHiperMegaRed"
$Location = "canadacentral"
$AppServicePlan = "ASP-CompuHiperMegaRed-ac32"
$WebAppName = "compuhipermegared"
$Runtime = "DOTNETCORE:8.0"

# 1. Login en Azure
Write-Host "`n?? Iniciando sesión en Azure..." -ForegroundColor Cyan
az login --use-device-code

# 2. Crear Resource Group
Write-Host "`n?? Creando Resource Group..." -ForegroundColor Cyan
az group create --name $ResourceGroup --location $Location

# 3. Crear App Service Plan F1 (Free)
Write-Host "`n???Creando App Service Plan F1..." -ForegroundColor Cyan
az appservice plan create `
    --name $AppServicePlan `
    --resource-group $ResourceGroup `
    --sku F1 `
    --is-linux

# 4. Crear Web App
Write-Host "`n?? Creando Web App..." -ForegroundColor Cyan
az webapp create `
    --resource-group $ResourceGroup `
    --plan $AppServicePlan `
    --name $WebAppName `
    --runtime "DOTNET:8.0"

# 5. Configurar Connection String (Supabase PostgreSQL)
Write-Host "`n?? Configurando Connection String..." -ForegroundColor Cyan
az webapp config connection-string set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings DefaultConnection="Host=db.xakfuxhafqbelwankypo.supabase.co;Database=postgres;Username=postgres;Password=CwJ4jyBHT2FBX_x;Port=5432;SSL Mode=Require;Trust Server Certificate=true" `
 --connection-string-type PostgreSQL

# 6. Configurar App Settings (PayU, Email, Encryption)
Write-Host "`n??  Configurando Variables de Entorno..." -ForegroundColor Cyan

# Generar Encryption Key
$EncryptionKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings `
        "PayU__ApiKey=4Vj8eK4rloUd272L48hsrarnUA" `
        "PayU__MerchantId=508029" `
"PayU__AccountId=512321" `
      "PayU__ApiLogin=pRRXKOl8ikMmt9u" `
        "PayU__IsTest=true" `
        "EmailSettings__SmtpServer=smtp.gmail.com" `
        "EmailSettings__SmtpPort=587" `
        "EmailSettings__FromEmail=lugapemu98@gmail.com" `
      "EmailSettings__FromName=CompuHiperMegaRed" `
        "EmailSettings__Password=cqlnnaavreoahunu" `
        "Encryption__Key=$EncryptionKey" `
        "ASPNETCORE_ENVIRONMENT=Production"

Write-Host "`n? Configuración de Azure completada!" -ForegroundColor Green
Write-Host "`n?? Encryption Key generada: $EncryptionKey" -ForegroundColor Yellow
Write-Host "`n?? URL de la aplicación: https://$WebAppName.azurewebsites.net" -ForegroundColor Cyan
Write-Host "`n??  Siguiente paso: Ejecutar deploy-app.ps1 para publicar la aplicación" -ForegroundColor Magenta
