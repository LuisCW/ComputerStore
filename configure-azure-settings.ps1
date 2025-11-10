# Script para Configurar Variables de Entorno en Azure
# Ejecutar en PowerShell

Write-Host "?? Configurando variables de entorno en Azure..." -ForegroundColor Green

$ResourceGroup = "CompuHiperMegaRed"
$WebAppName = "compuhipermegared"

# 1. Configurar ASPNETCORE_ENVIRONMENT
Write-Host "`n?? Configurando ASPNETCORE_ENVIRONMENT..." -ForegroundColor Cyan
az webapp config appsettings set `
    --resource-group $ResourceGroup `
 --name $WebAppName `
    --settings ASPNETCORE_ENVIRONMENT=Production

# 2. Configurar Connection String
Write-Host "`n?? Configurando Connection String..." -ForegroundColor Cyan
az webapp config connection-string set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
  --settings DefaultConnection="Host=db.xakfuxhafqbelwankypo.supabase.co;Database=postgres;Username=postgres;Password=CwJ4jyBHT2FBX_x;Port=5432;SSL Mode=Require;Trust Server Certificate=true" `
    --connection-string-type PostgreSQL

# 3. Generar Encryption Key
Write-Host "`n?? Generando Encryption Key..." -ForegroundColor Cyan
$EncryptionKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

# 4. Configurar todas las variables de aplicación
Write-Host "`n? Configurando variables de PayU, Email y Encryption..." -ForegroundColor Cyan
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
        "Encryption__Key=$EncryptionKey"

# 5. Reiniciar la aplicación
Write-Host "`n?? Reiniciando aplicación..." -ForegroundColor Cyan
az webapp restart --name $WebAppName --resource-group $ResourceGroup

Write-Host "`n? Configuración completada!" -ForegroundColor Green
Write-Host "`n?? URL de tu aplicación: https://$WebAppName.azurewebsites.net" -ForegroundColor Cyan
Write-Host "`n?? Encryption Key generada: $EncryptionKey" -ForegroundColor Yellow
Write-Host "`n??  Si aún ves el error, espera 2-3 minutos para que Azure aplique los cambios" -ForegroundColor Magenta
