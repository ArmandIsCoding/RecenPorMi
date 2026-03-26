# 👤 DisplayName: Nombre para Mostrar en Usuarios

## 🎯 Objetivo

Agregar un campo **DisplayName** (Nombre para Mostrar) a los usuarios para:
- ✅ Evitar mostrar el email completo en intenciones públicas (problema de UI y spam)
- ✅ Hacer el campo obligatorio en el registro
- ✅ Permitir edición en la página de perfil
- ✅ Traducir y rediseñar las páginas de Account/Manage al español con diseño moderno

---

## 📊 Cambios en el Modelo de Datos

### **ApplicationUser.cs**

**ANTES:**
```csharp
public class ApplicationUser : IdentityUser
{
}
```

**DESPUÉS:**
```csharp
public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(50)]
    public string DisplayName { get; set; } = string.Empty;
}
```

### **Migración Necesaria**

**Comando:**
```powershell
dotnet ef migrations add AgregarDisplayNameAUsuarios
dotnet ef database update
```

**Resultado:**
- Nueva columna `DisplayName` (nvarchar(50), NOT NULL) en tabla `AspNetUsers`

---

## 🔧 Cambios Implementados

### 1️⃣ **Register.razor** - Formulario de Registro

**Cambios en la UI:**
- ✅ Agregado campo "Nombre para Mostrar" como **primer campo** del formulario
- ✅ Emoji contextual: 👤
- ✅ Placeholder: "Ej: Juan Pérez"
- ✅ Hint: "Este nombre se mostrará en tus intenciones públicas"
- ✅ Validación: Required + MaxLength(50)

**Cambios en el código:**
```csharp
public async Task RegisterUser(EditContext editContext)
{
    var user = CreateUser();
    
    // ✅ Asignar DisplayName
    user.DisplayName = Input.DisplayName.Trim();

    await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
    // ... resto del código
}
```

**InputModel actualizado:**
```csharp
private sealed class InputModel
{
    [Required(ErrorMessage = "El nombre para mostrar es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
    [Display(Name = "Nombre para Mostrar")]
    public string DisplayName { get; set; } = "";

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    // ... resto de propiedades
}
```

---

### 2️⃣ **PeticionCard.razor** - Visualización del Nombre

**ANTES:**
```csharp
private string ObtenerNombreAutor()
{
    if (Peticion.PublicarAnonimamente)
        return "Anónimo";
    
    // ❌ Mostraba el email completo (username@example.com)
    return Peticion.Usuario?.UserName ?? "Usuario";
}
```

**DESPUÉS:**
```csharp
private string ObtenerNombreAutor()
{
    if (Peticion.PublicarAnonimamente)
        return "Anónimo";
    
    // ✅ Muestra el DisplayName (Juan Pérez)
    return Peticion.Usuario?.DisplayName ?? "Usuario";
}
```

---

### 3️⃣ **Account/Manage/Index.razor** - Página de Perfil

**Transformación completa:**

#### **Diseño Visual**
- ✅ Fondo con gradiente suave (#f5f7fa → #c3cfe2)
- ✅ Card con header morado matching el branding
- ✅ Inputs modernos con border radius y focus states
- ✅ Botones con hover effects
- ✅ Cards de opciones adicionales (Cambiar Contraseña, Datos Personales)

#### **Campos del Formulario**
1. **Email** (read-only)
   - Emoji: 📧
   - Disabled
   - Hint: "Tu email no puede ser modificado"

2. **DisplayName** (editable)
   - Emoji: 🏷️
   - Obligatorio
   - MaxLength: 50
   - Hint: "Este nombre se mostrará en tus intenciones no anónimas"

3. **PhoneNumber** (opcional)
   - Emoji: 📱
   - Opcional
   - Validación: Phone format

#### **Opciones Adicionales**
- 🔐 **Cambiar Contraseña** → Link a `/Account/Manage/ChangePassword`
- 🗃️ **Datos Personales** → Link a `/Account/Manage/PersonalData`

#### **Código Actualizado**
```csharp
protected override async Task OnInitializedAsync()
{
    Input ??= new();

    user = await UserManager.GetUserAsync(HttpContext.User);
    if (user is null)
    {
        RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
        return;
    }

    username = await UserManager.GetUserNameAsync(user);
    phoneNumber = await UserManager.GetPhoneNumberAsync(user);
    displayName = user.DisplayName; // ✅ Cargar DisplayName

    Input.PhoneNumber ??= phoneNumber;
    Input.DisplayName ??= displayName; // ✅ Pre-llenar DisplayName
}

private async Task OnValidSubmitAsync()
{
    if (user is null)
    {
        RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
        return;
    }

    // ✅ Actualizar DisplayName
    if (Input.DisplayName != displayName)
    {
        user.DisplayName = Input.DisplayName?.Trim() ?? string.Empty;
        var updateResult = await UserManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            RedirectManager.RedirectToCurrentPageWithStatus("Error: No se pudo actualizar el nombre.", HttpContext);
            return;
        }
    }

    // Actualizar teléfono...
    
    await SignInManager.RefreshSignInAsync(user);
    RedirectManager.RedirectToCurrentPageWithStatus("✅ Tu perfil ha sido actualizado correctamente.", HttpContext);
}
```

---

## 🎨 CSS Personalizado (Account/Manage/Index.razor)

### **Clases Principales**
```css
.manage-container       // Contenedor con gradiente de fondo
.manage-header          // Header con título gradient text
.manage-card            // Card principal blanca con shadow
.card-header            // Header morado del card
.manage-input           // Inputs con border y focus states
.manage-option-card     // Cards de opciones adicionales con hover
.option-icon            // Iconos grandes en option cards
```

### **Efectos**
- Gradient text en título (matching branding)
- Border radius modernos (12px-16px)
- Focus states con glow effect
- Hover en option cards: translateY(-2px) + shadow
- Responsive: padding adaptativo

---

## 🚀 Flujo Completo de Usuario

### **Registro**
1. Usuario va a `/Account/Register`
2. Completa:
   - ✅ **Nombre para Mostrar** (Obligatorio)
   - Email
   - Password
   - Confirm Password
3. Submit → DisplayName se guarda en la base de datos
4. Auto-login y redirección al home

### **Publicar Intención**
1. Usuario autenticado va a `/publicar`
2. Escribe intención
3. Puede elegir:
   - ☑️ **Publicar anónimamente** → Se muestra "Anónimo"
   - ☐ **Publicar con nombre** → Se muestra su **DisplayName** (ej: "Juan Pérez")

### **Editar Perfil**
1. Usuario va a `/Account/Manage`
2. Ve su información:
   - Email (no editable)
   - DisplayName (editable)
   - Teléfono (opcional)
3. Puede cambiar su DisplayName
4. Submit → Cambios guardados en DB

---

## 📋 Migración - Pasos para Aplicar

### **Paso 1: Detener la Aplicación**
Cierra Visual Studio o detén el debugging (Shift+F5)

### **Paso 2: Crear la Migración**
```powershell
# Desde la raíz del proyecto
dotnet ef migrations add AgregarDisplayNameAUsuarios

# O desde Package Manager Console
Add-Migration AgregarDisplayNameAUsuarios
```

### **Paso 3: Revisar la Migración Generada**
Archivo: `Data\Migrations\[timestamp]_AgregarDisplayNameAUsuarios.cs`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "DisplayName",
        table: "AspNetUsers",
        type: "nvarchar(50)",
        maxLength: 50,
        nullable: false,
        defaultValue: "Usuario");  // ⚠️ Default para usuarios existentes
}
```

**⚠️ IMPORTANTE:** Si ya tienes usuarios en la base de datos, necesitarás un valor por defecto.

**Opción A: Default temporal**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "DisplayName",
        table: "AspNetUsers",
        type: "nvarchar(50)",
        maxLength: 50,
        nullable: false,
        defaultValue: "Usuario");
}
```

**Opción B: Asignar DisplayName desde Email**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Agregar columna como nullable temporalmente
    migrationBuilder.AddColumn<string>(
        name: "DisplayName",
        table: "AspNetUsers",
        type: "nvarchar(50)",
        maxLength: 50,
        nullable: true);

    // Actualizar con parte local del email
    migrationBuilder.Sql(@"
        UPDATE AspNetUsers 
        SET DisplayName = LEFT(Email, CHARINDEX('@', Email) - 1)
        WHERE DisplayName IS NULL
    ");

    // Hacer NOT NULL
    migrationBuilder.AlterColumn<string>(
        name: "DisplayName",
        table: "AspNetUsers",
        type: "nvarchar(50)",
        maxLength: 50,
        nullable: false,
        oldClrType: typeof(string),
        oldNullable: true);
}
```

### **Paso 4: Aplicar la Migración**
```powershell
dotnet ef database update

# O desde Package Manager Console
Update-Database
```

### **Paso 5: Verificar en SQL Server**
```sql
SELECT TOP 10 Id, Email, DisplayName, UserName 
FROM AspNetUsers;

-- Verificar estructura
SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'DisplayName';
```

---

## 🧪 Testing Checklist

### **Registro con DisplayName**
- [ ] Ir a `/Account/Register`
- [ ] Dejar DisplayName vacío → error "El nombre para mostrar es requerido"
- [ ] Ingresar DisplayName con más de 50 caracteres → error de validación
- [ ] Registro exitoso guarda DisplayName en DB

### **Visualización en Intenciones**
- [ ] Publicar intención con "Publicar anónimamente" DESACTIVADO
- [ ] Verificar que se muestra DisplayName, no el email
- [ ] Verificar que intenciones anónimas siguen mostrando "Anónimo"

### **Edición de Perfil**
- [ ] Ir a `/Account/Manage`
- [ ] Verificar que DisplayName se carga correctamente
- [ ] Cambiar DisplayName
- [ ] Submit → Cambios se guardan
- [ ] Verificar en intenciones que se actualiza el nombre mostrado

### **SQL Verification**
```sql
-- Verificar que todos los usuarios tienen DisplayName
SELECT Id, Email, DisplayName 
FROM AspNetUsers 
WHERE DisplayName IS NULL OR DisplayName = '';

-- ✅ Resultado esperado: 0 filas
```

---

## 🔄 Comparación Antes/Después

### **Visualización en Intenciones**

**ANTES:**
```
┌─────────────────────────────┐
│ usuario@example.com         │  ❌ Email completo (spam risk)
│ Hace 5 min                  │
├─────────────────────────────┤
│ Por favor recen por mi      │
│ familia...                  │
├─────────────────────────────┤
│ 🙏 5                        │
└─────────────────────────────┘
```

**DESPUÉS:**
```
┌─────────────────────────────┐
│ Juan Pérez                  │  ✅ DisplayName (limpio)
│ Hace 5 min                  │
├─────────────────────────────┤
│ Por favor recen por mi      │
│ familia...                  │
├─────────────────────────────┤
│ 🙏 5                        │
└─────────────────────────────┘
```

### **Formulario de Registro**

**ANTES:**
- Email
- Password
- Confirm Password

**DESPUÉS:**
- **👤 Nombre para Mostrar** (Nuevo, Obligatorio, Primero)
- 📧 Email
- 🔒 Password
- ✅ Confirm Password

### **Página de Perfil**

**ANTES:**
- Fea, genérica, en inglés
- Solo Username (disabled) y Phone Number

**DESPUÉS:**
- ✅ Diseño moderno con gradientes
- ✅ Todo en español
- ✅ DisplayName editable
- ✅ Cards de opciones adicionales
- ✅ Hover effects y animaciones

---

## 🎨 Elementos UI Rediseñados

### **Account/Manage/Index.razor**

**Header:**
- Título: "👤 Mi Perfil"
- Subtítulo: "Administra tu información personal"
- Gradient text matching branding

**Form:**
- 3 campos con iconos y hints
- Inputs con focus glow
- Botón principal: "💾 Guardar Cambios"

**Option Cards:**
```
┌────────────────────────────────────┐
│ 🔐  Cambiar Contraseña             │  ← Hover effect
│     Actualiza tu contraseña        │
└────────────────────────────────────┘

┌────────────────────────────────────┐
│ 🗃️  Datos Personales               │  ← Hover effect
│     Descarga o elimina tus datos   │
└────────────────────────────────────┘
```

---

## 📝 Archivos Modificados

### **Modelos y Datos**
1. `RecenPorMi\Data\ApplicationUser.cs` - Agregado DisplayName
2. Nueva migración: `[timestamp]_AgregarDisplayNameAUsuarios.cs`

### **Páginas de Autenticación**
3. `RecenPorMi\Components\Account\Pages\Register.razor` - Campo DisplayName agregado
4. `RecenPorMi\Components\Account\Pages\Manage\Index.razor` - Rediseñada completamente

### **Componentes de UI**
5. `RecenPorMi\Components\Shared\PeticionCard.razor` - Muestra DisplayName en lugar de UserName

---

## 🔒 Seguridad y Validaciones

### **DisplayName**
- ✅ Required: Campo obligatorio
- ✅ MaxLength: 50 caracteres máximo
- ✅ Trim: Se eliminan espacios al inicio y final
- ✅ No contiene email: Se muestra nombre, no se expone email

### **Edición de Perfil**
- ✅ Solo usuario autenticado puede editar
- ✅ Solo puede editar su propio perfil
- ✅ Email no editable (read-only)
- ✅ Refresh de SignIn después de actualizar

---

## ✅ Resultado Final

### **Beneficios Implementados**
1. ✅ **UI más limpia**: "Juan Pérez" en lugar de "usuario@example.com"
2. ✅ **Privacidad**: Email no se expone en intenciones públicas
3. ✅ **Anti-spam**: No se revela el email a scraper bots
4. ✅ **UX mejorada**: Nombres más legibles y amigables
5. ✅ **Diseño moderno**: Página de perfil espectacular en español
6. ✅ **Editable**: Usuario puede cambiar su DisplayName cuando quiera

### **Usuarios Nuevos**
- Deben ingresar DisplayName al registrarse (obligatorio)
- Se muestra DisplayName en todas las intenciones no anónimas

### **Usuarios Existentes (si los hay)**
- Migración les asigna DisplayName por defecto
- Pueden editarlo desde `/Account/Manage`

---

**Cambios implementados por:** GitHub Copilot  
**Fecha:** 26 de marzo de 2026  
**Archivos modificados:** 5  
**Migración requerida:** ✅ Sí (`AgregarDisplayNameAUsuarios`)
