#!/bin/bash

# Script de instalación y configuración de PostgreSQL para ComputerStore
# Para sistemas Linux/macOS

echo "=== INSTALACIÓN DE POSTGRESQL PARA COMPUTERSTORE ==="

# 1. Verificar si PostgreSQL está instalado
echo "Verificando instalación de PostgreSQL..."
if ! command -v psql &> /dev/null; then
    echo "PostgreSQL no está instalado."
    echo "Para instalar en Ubuntu/Debian: sudo apt-get install postgresql postgresql-contrib"
    echo "Para instalar en macOS: brew install postgresql"
    echo "Para instalar en CentOS/RHEL: sudo yum install postgresql-server postgresql-contrib"
    exit 1
fi

# 2. Verificar si el servicio está ejecutándose
echo "Verificando servicio de PostgreSQL..."
if ! sudo systemctl is-active --quiet postgresql 2>/dev/null && ! brew services list | grep postgresql | grep started &> /dev/null; then
    echo "Iniciando servicio PostgreSQL..."
    if command -v systemctl &> /dev/null; then
        sudo systemctl start postgresql
        sudo systemctl enable postgresql
    elif command -v brew &> /dev/null; then
        brew services start postgresql
    fi
fi

# 3. Configurar usuario postgres
echo "Configurando usuario postgres..."
sudo -u postgres psql -c "ALTER USER postgres PASSWORD 'admin123';" 2>/dev/null || {
    echo "Configurando usuario postgres (método alternativo)..."
    sudo -u postgres createuser --interactive --pwprompt postgres 2>/dev/null
}

# 4. Crear base de datos ComputerStoreDB
echo "Creando base de datos ComputerStoreDB..."
PGPASSWORD='admin123' createdb -h localhost -U postgres ComputerStoreDB 2>/dev/null || {
    echo "Base de datos ya existe o error creándola"
}

# 5. Verificar herramientas de .NET
echo "Verificando herramientas de Entity Framework..."
if ! dotnet tool list -g | grep -q "dotnet-ef"; then
    echo "Instalando dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
fi

# 6. Restaurar paquetes NuGet
echo "Restaurando paquetes NuGet..."
dotnet restore

# 7. Crear migración inicial
echo "Creando migración inicial..."
dotnet ef migrations add InitialCreate

# 8. Aplicar migración a la base de datos
echo "Aplicando migración a la base de datos..."
dotnet ef database update

echo ""
echo "=== CONFIGURACIÓN COMPLETADA ==="
echo "Base de datos: ComputerStoreDB"
echo "Servidor: localhost:5432"
echo "Usuario: postgres"
echo "Contraseña: admin123"
echo ""
echo "Credenciales de administrador por defecto:"
echo "Email: admin@compuhipermegared.com"
echo "Contraseña: Admin123!"
echo ""
echo "Para ejecutar la aplicación:"
echo "dotnet run"
echo ""
echo "La aplicación estará disponible en: https://localhost:7100"