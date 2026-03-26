# 📋 INSTRUCCIONES DE CONFIGURACIÓN Y MIGRACIÓN
## Recen Por Mí - MVP

---

## ✅ PASO 1: Verificar la Cadena de Conexión

Abrí el archivo `appsettings.json` y verificá que la cadena de conexión sea correcta para tu instancia de SQL Server Express:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=RecenPorMiDB;User Id=sa;Password=123456;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

⚠️ **IMPORTANTE**: Si tu instancia de SQL Server Express tiene un nombre diferente (por ejemplo, `TU-PC\SQLEXPRESS`), cambiá el `.\\SQLEXPRESS` por el nombre correcto.

Para verificar tu instancia, ejecutá en PowerShell:
```powershell
Get-Service -Name "MSSQL*"
```

---

## ✅ PASO 2: Crear la Migración Inicial

Abrí la **Consola del Administrador de Paquetes** en Visual Studio:
- Menú: `Herramientas` → `Administrador de paquetes NuGet` → `Consola del Administrador de paquetes`

Ejecutá los siguientes comandos:

```powershell
# 1. Agregar la migración inicial
Add-Migration InitialCreatePeticionesYRezos

# 2. Aplicar la migración a la base de datos
Update-Database
```

Esto va a crear:
- La base de datos `RecenPorMiDB` en tu SQL Server Express
- Las tablas `Peticiones` y `Rezos` con sus relaciones
- Las tablas de Identity (si no existen)

---

## ✅ PASO 3: Ejecutar la Aplicación

Presioná **F5** o ejecutá el proyecto desde Visual Studio.

La aplicación va a abrir en tu navegador y vas a ver:
- ✨ El formulario para publicar intenciones
- 📜 El feed de peticiones recientes
- 🙏 El botón de rezo con contador

---

## 🔧 SOLUCIÓN DE PROBLEMAS COMUNES

### ❌ Error: "Cannot open database RecenPorMiDB"
**Solución**: Verificá que SQL Server Express esté corriendo:
```powershell
Start-Service -Name "MSSQL$SQLEXPRESS"
```

### ❌ Error: "Login failed for user 'sa'"
**Solución**: 
1. Verificá que el usuario `sa` esté habilitado
2. O cambiá la cadena de conexión para usar Windows Authentication:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=RecenPorMiDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### ❌ Error: "A network-related or instance-specific error"
**Solución**: 
1. Verificá que SQL Server Browser esté corriendo:
```powershell
Start-Service -Name "SQLBrowser"
```
2. Habilitá TCP/IP en SQL Server Configuration Manager

---

## 🚀 FUNCIONALIDADES IMPLEMENTADAS

✅ **Entidades**:
- `Peticion` con Alias, Contenido, FechaPublicacion y ContadorRezos
- `Rezo` con sistema anti-spam (IpHash + límite de 5 minutos)

✅ **Base de Datos**:
- Entity Framework Core Code-First
- Relaciones entre entidades
- Índices para optimizar consultas

✅ **UI Blazor Server**:
- Formulario reactivo para crear peticiones
- Feed de peticiones con tarjetas estilizadas
- Botón de rezo con contador en tiempo real
- Validaciones en cliente y servidor

✅ **SignalR**:
- Actualizaciones en tiempo real cuando alguien reza
- Notificaciones cuando se publica una nueva petición

✅ **Estilos**:
- Diseño minimalista y solemne
- Gradientes morados/azules
- Animaciones suaves
- Responsive (mobile-friendly)

---

## 📊 ESTRUCTURA DE LA BASE DE DATOS

```sql
-- Tabla Peticiones
CREATE TABLE Peticiones (
    Id INT PRIMARY KEY IDENTITY,
    Alias NVARCHAR(50) NOT NULL DEFAULT 'Anónimo',
    Contenido NVARCHAR(500) NOT NULL,
    FechaPublicacion DATETIME2 NOT NULL,
    ContadorRezos INT NOT NULL DEFAULT 0
);

-- Tabla Rezos
CREATE TABLE Rezos (
    Id INT PRIMARY KEY IDENTITY,
    PeticionId INT NOT NULL,
    Fecha DATETIME2 NOT NULL,
    IpHash NVARCHAR(64) NOT NULL,
    FOREIGN KEY (PeticionId) REFERENCES Peticiones(Id) ON DELETE CASCADE
);
```

---

## 🎨 PERSONALIZACIÓN

Si querés cambiar los colores del gradiente, editá el archivo `wwwroot/app.css`:

```css
/* Buscá estas líneas y cambiá los colores */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
```

Colores sugeridos:
- Verde esperanza: `#11998e 0%, #38ef7d 100%`
- Azul cielo: `#4facfe 0%, #00f2fe 100%`
- Dorado: `#f093fb 0%, #f5576c 100%`

---

## 📝 PRÓXIMAS MEJORAS (Post-MVP)

- [ ] Obtener la IP real del usuario (actualmente usa 127.0.0.1)
- [ ] Sistema de moderación de contenido
- [ ] Categorías de peticiones (salud, familia, trabajo, etc.)
- [ ] Estadísticas de rezos por día/semana
- [ ] Compartir peticiones en redes sociales
- [ ] Notificaciones push cuando alguien reza por tu petición
- [ ] Modo oscuro

---

## 📞 SOPORTE

Si tenés algún problema o duda:
1. Revisá la consola de Visual Studio para ver errores
2. Verificá la consola del navegador (F12)
3. Consultá los logs de SQL Server

---

**¡Que Dios bendiga este proyecto! 🙏✨**
