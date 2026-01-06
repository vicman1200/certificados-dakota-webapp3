# Script para generar una clave secreta JWT segura
# Ejecutar en PowerShell: .\generar-clave-jwt.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Generador de Clave Secreta para JWT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Generar clave de 64 caracteres
$clave = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_})

Write-Host "Clave generada:" -ForegroundColor Green
Write-Host $clave -ForegroundColor Yellow
Write-Host ""
Write-Host "Longitud: $($clave.Length) caracteres" -ForegroundColor Green
Write-Host ""
Write-Host "Copia esta clave y pégala en appsettings.json:" -ForegroundColor Cyan
Write-Host '"Jwt": {' -ForegroundColor Gray
Write-Host '  "SecretKey": "' -NoNewline -ForegroundColor Gray
Write-Host $clave -NoNewline -ForegroundColor Yellow
Write-Host '",' -ForegroundColor Gray
Write-Host '  "Issuer": "miapi.com",' -ForegroundColor Gray
Write-Host '  "Audience": "miapi.com",' -ForegroundColor Gray
Write-Host '  "ExpirationMinutes": "60"' -ForegroundColor Gray
Write-Host '}' -ForegroundColor Gray
Write-Host ""

# Copiar al portapapeles (opcional)
Set-Clipboard -Value $clave
Write-Host "La clave se ha copiado al portapapeles!" -ForegroundColor Green

