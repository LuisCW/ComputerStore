# ComputerStore - Sistema de E-commerce con PostgreSQL

Sistema completo de e-commerce para tienda de computadores con integración a PayU, gestión de usuarios, inventario y envíos.

##  Características

- **Autenticación completa** con ASP.NET Core Identity
- **Base de datos PostgreSQL** 
- **Gestión de usuarios** y roles (Admin/User)
- **Catálogo de productos** con inventario
- **Carrito de compras** persistente
- **Integración PayU** (Tarjetas, PSE, EFECTY, NEQUI)
- **Sistema de envíos** con seguimiento
- **Panel de administración**
- **Diseño responsive** con Bootstrap 5

## ?? Requisitos Previos

### Software Necesario
- **.NET 8.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL 15+** - [Descargar](https://www.postgresql.org/download/)
- **Git** - [Descargar](https://git-scm.com/)
- **Visual Studio 2022** o **Visual Studio Code** (opcional)

### Configuración de PostgreSQL
Durante la instalación de PostgreSQL, configura:
- **Usuario:** `postgres`
- **Contraseña:** `admin123`
- **Puerto:** `5432`
- **Base de datos inicial:** `postgres`

## ??? Instalación y Configuración

### Opción 1: Instalación Automática (Windows)

1. **Clonar el repositorio:**
```bash
git clone https://github.com/LuisCW/ComputerStore
cd ComputerStore
```

2. **Ejecutar script de instalación:**
```powershell
# Ejecutar como administrador
.\setup-database.ps1
```

3. **Ejecutar la aplicación:**
```bash
dotnet run
```

### Opción 2: Instalación Manual

1. **Clonar el repositorio:**
```bash
git clone https://github.com/LuisCW/ComputerStore
cd ComputerStore
```

2. **Restaurar paquetes NuGet:**
```bash
dotnet restore
```

3. **Instalar herramientas de Entity Framework:**
```bash
dotnet tool install --global dotnet-ef
```

4. **Configurar cadena de conexión:**
Editar `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ComputerStoreDB;Username=postgres;Password=TU_CONTRASEÑA;Port=5432"
  }
}
```

5. **Crear y aplicar migraciones:**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

6. **Ejecutar la aplicación:**
```bash
dotnet run
```

### Opción 3: Docker (Próximamente)
```bash
docker-compose up -d
```

## ?? Credenciales por Defecto

### Administrador
- **Email:** `admin@compuhipermegared.com`
- **Contraseña:** `Admin123!`

### Base de Datos
- **Host:** `localhost:5432`
- **Base de datos:** `ComputerStoreDB`
- **Usuario:** `postgres`
- **Contraseña:** `admin123`

## ?? URLs de la Aplicación

- **Aplicación principal:** https://localhost:7100
- **Login:** https://localhost:7100/Account/Login
- **Registro:** https://localhost:7100/Account/Register
- **Panel Admin:** https://localhost:7100/Admin (requiere rol Admin)

## ?? Estructura del Proyecto

```
ComputerStore/
??? Data/                          # Entity Framework DbContext
??? Models/                        # Modelos de datos
?   ??? ViewModels/               # ViewModels para formularios
?   ??? ApplicationUser.cs        # Usuario extendido
?   ??? ProductEntity.cs          # Productos en BD
?   ??? ...
??? Pages/                         # Razor Pages
?   ??? Account/                  # Autenticación
?   ??? Admin/                    # Panel de administración
?   ??? Shared/                   # Layouts compartidos
?   ??? ...
??? Services/                      # Servicios de negocio
?   ??? PayUService.cs            # Integración PayU
?   ??? LittleCarService.cs       # Carrito de compras
?   ??? ...
??? wwwroot/                       # Archivos estáticos
??? Migrations/                    # Migraciones de EF
??? setup-database.ps1            # Script de instalación Windows
??? setup-database.sh             # Script de instalación Linux/macOS
??? README.md                     # Este archivo
```

## ?? Configuración de Desarrollo

### Variables de Entorno
Crear archivo `.env` (opcional):
```
DATABASE_URL=Host=localhost;Database=ComputerStoreDB;Username=postgres;Password=admin123;Port=5432
PAYU_API_KEY=tu_api_key_aqui
PAYU_MERCHANT_ID=tu_merchant_id_aqui
```

### Configuración de PayU
En `appsettings.Development.json`:
```json
{
  "PayUSettings": {
    "ApiKey": "TU_API_KEY",
    "ApiLogin": "TU_API_LOGIN",
    "MerchantId": "TU_MERCHANT_ID",
    "AccountId": "TU_ACCOUNT_ID",
    "Url": "https://sandbox.api.payulatam.com/payments-api/4.0/service.cgi"
  }
}
```

## ?? Datos de Prueba PayU

### Tarjetas de Crédito (Sandbox)
**Visa Aprobada:**
- Número: `4097440000000004`
- CVV: `123`
- Fecha: `12/25`
- Nombre: `APPROVED`

**Visa Rechazada:**
- Número: `4111111111111111`
- CVV: `666`
- Fecha: `12/25`
- Nombre: `REJECTED`

### PSE (Sandbox)
- Banco: Cualquier banco de prueba
- Tipo de documento: CC
- Documento: `123456789`

## ?? Base de Datos

### Tablas Principales
- `AspNetUsers` - Usuarios del sistema
- `Productos` - Catálogo de productos
- `Pedidos` - Órdenes de compra
- `PedidoDetalles` - Detalles de productos por pedido
- `Envios` - Información de envíos
- `Transacciones` - Registro de transacciones PayU

### Diagrama ER
```
Users (AspNetUsers)
??? Pedidos (1:N)
?   ??? PedidoDetalles (1:N)
?   ?   ??? Productos (N:1)
?   ??? Envios (1:1)
?   ??? Transacciones (1:N)
??? Roles (N:N via AspNetUserRoles)
```

## ?? Comandos Útiles

### Entity Framework
```bash
# Crear nueva migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Revertir migración
dotnet ef database update MigracionAnterior

# Eliminar última migración
dotnet ef migrations remove

# Ver SQL de migración
dotnet ef migrations script
```

### Desarrollo
```bash
# Ejecutar en modo desarrollo
dotnet run

# Ejecutar con hot reload
dotnet watch run

# Limpiar y compilar
dotnet clean && dotnet build

# Publicar para producción
dotnet publish -c Release
```

## ?? Solución de Problemas

### Error de Conexión a PostgreSQL
1. Verificar que PostgreSQL esté ejecutándose
2. Verificar credenciales en `appsettings.json`
3. Verificar que el puerto 5432 esté disponible

### Error de Migraciones
```bash
# Eliminar todas las migraciones y recrear
rm -rf Migrations/
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Error de Permisos
```bash
# Windows: Ejecutar como administrador
# Linux/macOS: Usar sudo para comandos de base de datos
```

## ?? Funcionalidades por Implementar

- [ ] **Email de confirmación** de registro
- [ ] **Recuperación de contraseña** por email
- [ ] **Notificaciones push** para envíos
- [ ] **Chat en línea** con soporte
- [ ] **Reportes de ventas** para admin
- [ ] **API REST** para móviles
- [ ] **Integración con más pasarelas** de pago

## ?? Contribución

1. Fork el proyecto
2. Crear rama para nueva funcionalidad (`git checkout -b feature/NuevaFuncionalidad`)
3. Commit los cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/NuevaFuncionalidad`)
5. Abrir Pull Request

## ?? Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

## ?? Soporte

- **Email:** lugapemu98@gmail.com
- **Teléfono:** +57 323 768 4390.
- **WhatsApp:** [+57 323 768 4390](https://wa.me/573237684390)
- **Dirección:** Cra. 15 #78-33, Chapinero, Bogotá

---

**CompuHiperMegaRed** - Tu tienda de confianza para computadoras y tecnología ???.


## Equipo De desarrollo

- Michael Betancourt
- Manuela Pardo
- Juan Mendoza
- Luis Peraza
