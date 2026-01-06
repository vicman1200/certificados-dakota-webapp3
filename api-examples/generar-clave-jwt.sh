#!/bin/bash
# Script para generar una clave secreta JWT segura (Linux/Mac)
# Ejecutar: chmod +x generar-clave-jwt.sh && ./generar-clave-jwt.sh

echo "========================================"
echo "Generador de Clave Secreta para JWT"
echo "========================================"
echo ""

# Generar clave usando OpenSSL (si está disponible)
if command -v openssl &> /dev/null; then
    CLAVE=$(openssl rand -base64 48 | tr -d "=+/" | cut -c1-64)
else
    # Alternativa usando /dev/urandom
    CLAVE=$(cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 64 | head -n 1)
fi

echo "Clave generada:"
echo "$CLAVE"
echo ""
echo "Longitud: ${#CLAVE} caracteres"
echo ""
echo "Copia esta clave y pégala en appsettings.json:"
echo '"Jwt": {'
echo '  "SecretKey": "'$CLAVE'",'
echo '  "Issuer": "miapi.com",'
echo '  "Audience": "miapi.com",'
echo '  "ExpirationMinutes": "60"'
echo '}'
echo ""

# Intentar copiar al portapapeles (Linux)
if command -v xclip &> /dev/null; then
    echo "$CLAVE" | xclip -selection clipboard
    echo "La clave se ha copiado al portapapeles!"
elif command -v pbcopy &> /dev/null; then
    echo "$CLAVE" | pbcopy
    echo "La clave se ha copiado al portapapeles!"
fi

