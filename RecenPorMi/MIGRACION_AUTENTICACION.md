# 🔐 Migración: Sistema de Autenticación

## Cambios Realizados

### 1. Modelo Peticion
- ✅ Eliminado campo `Alias` (string)
- ✅ Agregado campo `UserId` (FK a ApplicationUser) - REQUIRED
- ✅ Agregado campo `PublicarAnonimamente` (bool) - Default: true
- ✅ Agregada navegación `Usuario` (ApplicationUser)

### 2. ApplicationDbContext
- ✅ Configurada relación Peticion → Usuario (OnDelete: Cascade)
- ✅ Agregado índice en `UserId` para optimizar queries

### 3. PeticionService
- ✅ Actualizado `CrearPeticionAsync` para recibir `userId` y `publicarAnonimamente`
- ✅ Actualizado `ObtenerPeticionesRecientesAsync` para incluir `.Include(p => p.Usuario)`

### 4. Página Publicar.razor
- ✅ Agregado `@attribute [Authorize]` - requiere autenticación
- ✅ Inyectado `AuthenticationStateProvider`
- ✅ Obtención de `UserId` del usuario autenticado via Claims
- ✅ Checkbox "Publicar anónimamente" reemplaza campo de alias
- ✅ Validación: redirecciona a login si no hay usuario autenticado

### 5. PeticionCard.razor
- ✅ Método `ObtenerNombreAutor()`:
  - Si `PublicarAnonimamente == true` → muestra "Anónimo"
  - Si `PublicarAnonimamente == false` → muestra `Usuario.UserName`

---

## 📋 Pasos para Aplicar la Migración

### Paso 1: Crear la Nueva Migración

Abre **Package Manager Console** en Visual Studio y ejecuta:

```powershell
Add-Migration AgregarAutenticacionAPeticiones
```

Esto generará una nueva migración que:
- Eliminará la columna `Alias` de la tabla `Peticiones`
- Agregará la columna `UserId` (nvarchar(450), NOT NULL)
- Agregará la columna `PublicarAnonimamente` (bit, NOT NULL, default 1)
- Creará FK constraint `FK_Peticiones_AspNetUsers_UserId`
- Creará índice `IX_Peticiones_UserId`

### Paso 2: Revisar la Migración Generada

Verifica el archivo generado en `Data\Migrations\[timestamp]_AgregarAutenticacionAPeticiones.cs`:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ⚠️ IMPORTANTE: Si ya tienes datos en Peticiones, necesitarás un usuario por defecto
    
    migrationBuilder.DropColumn(
        name: "Alias",
        table: "Peticiones");

    migrationBuilder.AddColumn<string>(
        name: "UserId",
        table: "Peticiones",
        type: "nvarchar(450)",
        nullable: false,
        defaultValue: "");  // ⚠️ Puede causar error si hay datos existentes

    migrationBuilder.AddColumn<bool>(
        name: "PublicarAnonimamente",
        table: "Peticiones",
        type: "bit",
        nullable: false,
        defaultValue: true);

    migrationBuilder.CreateIndex(
        name: "IX_Peticiones_UserId",
        table: "Peticiones",
        column: "UserId");

    migrationBuilder.AddForeignKey(
        name: "FK_Peticiones_AspNetUsers_UserId",
        table: "Peticiones",
        column: "UserId",
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
}
```

### ⚠️ Paso 2.5: Si Ya Tienes Datos en la Base de Datos

Si ya ejecutaste la migración anterior y tienes peticiones en la DB, necesitas modificar la migración para asignar un usuario por defecto:

**Opción A: Eliminar datos existentes (solo para desarrollo)**
```sql
-- Ejecutar ANTES de Update-Database
DELETE FROM Rezos;
DELETE FROM Peticiones;
```

**Opción B: Asignar a un usuario existente (recomendado)**

Edita el archivo de migración y agrega ANTES del `AddColumn` de `UserId`:

```csharp
// 1. Agregar columna como nullable temporalmente
migrationBuilder.AddColumn<string>(
    name: "UserId",
    table: "Peticiones",
    type: "nvarchar(450)",
    nullable: true);

// 2. Asignar un usuario por defecto (reemplaza con un UserId real de tu DB)
migrationBuilder.Sql(@"
    UPDATE Peticiones 
    SET UserId = (SELECT TOP 1 Id FROM AspNetUsers)
");

// 3. Hacer la columna NOT NULL
migrationBuilder.AlterColumn<string>(
    name: "UserId",
    table: "Peticiones",
    type: "nvarchar(450)",
    nullable: false,
    oldClrType: typeof(string),
    oldNullable: true);
```

### Paso 3: Aplicar la Migración

```powershell
Update-Database
```

### Paso 4: Verificar en SQL Server

```sql
-- Verificar estructura actualizada
SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Peticiones';

-- Debería mostrar:
-- - UserId (nvarchar(450), NOT NULL)
-- - PublicarAnonimamente (bit, NOT NULL)
-- - NO debería existir Alias

-- Verificar FK
SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
WHERE CONSTRAINT_NAME LIKE '%Peticiones%';
```

---

## 🔒 Comportamiento del Sistema

### Usuario No Autenticado
- ✅ Puede ver todas las intenciones en `/` (Home)
- ✅ Puede rezar por las intenciones (registra IP hash)
- ❌ **NO puede acceder a `/publicar`** → redirige a `/Account/Login`

### Usuario Autenticado
- ✅ Acceso completo a toda la aplicación
- ✅ Puede publicar intenciones desde `/publicar`
- ✅ Checkbox "Publicar anónimamente":
  - ☑️ **Activado (default)**: Intención aparece como "Anónimo"
  - ☐ **Desactivado**: Intención muestra el `UserName` del usuario

### Visualización en PeticionCard
```csharp
// Lógica implementada:
if (Peticion.PublicarAnonimamente)
    return "Anónimo";
else
    return Peticion.Usuario?.UserName ?? "Usuario";
```

---

## 🧪 Testing

### 1. Test: Usuario sin autenticar intenta publicar
```
1. Cerrar sesión (logout)
2. Ir a https://localhost:xxxx/publicar
3. ✅ Resultado esperado: Redirección a /Account/Login
```

### 2. Test: Usuario autenticado publica anónimamente
```
1. Iniciar sesión
2. Ir a /publicar
3. Escribir intención
4. Dejar checkbox "Publicar anónimamente" ACTIVADO ☑️
5. Publicar
6. ✅ Resultado: En home, la intención muestra "Anónimo"
```

### 3. Test: Usuario autenticado publica con nombre
```
1. Iniciar sesión con usuario "JuanPerez"
2. Ir a /publicar
3. Escribir intención
4. DESACTIVAR checkbox "Publicar anónimamente" ☐
5. Publicar
6. ✅ Resultado: En home, la intención muestra "JuanPerez"
```

### 4. Test: Verificar FK cascade delete
```sql
-- En SSMS:
-- 1. Crear usuario de prueba
-- 2. Publicar intención con ese usuario
-- 3. Eliminar el usuario
DELETE FROM AspNetUsers WHERE UserName = 'UsuarioPrueba';
-- ✅ Sus peticiones deben eliminarse automáticamente (CASCADE)
```

---

## ⚡ Próximos Pasos Sugeridos

1. **IP Real del Usuario**: Actualizar PeticionCard.razor para obtener IP real vía HttpContext
2. **Mi Perfil**: Página para ver historial de intenciones publicadas por el usuario
3. **Editar/Eliminar**: Permitir al usuario editar/eliminar solo sus propias intenciones
4. **Moderación**: Panel admin para revisar intenciones reportadas
5. **Notificaciones**: Avisar al usuario cuando alguien reza por su intención

---

## 🐛 Troubleshooting

### Error: "Cannot insert NULL into column 'UserId'"
**Causa**: Intentaste aplicar la migración con datos existentes en Peticiones  
**Solución**: Sigue el "Paso 2.5" para asignar un usuario por defecto

### Error: "The ALTER TABLE statement conflicted with the FOREIGN KEY constraint"
**Causa**: El `UserId` asignado no existe en `AspNetUsers`  
**Solución**: 
```sql
-- Verificar usuarios existentes
SELECT Id, UserName FROM AspNetUsers;
-- Actualizar con un Id válido
UPDATE Peticiones SET UserId = '[ID-VALIDO]';
```

### Error: "Sequence contains no elements" al publicar
**Causa**: `userId` es null o no se encontró el Claim  
**Solución**: Verificar que el usuario esté realmente autenticado y el claim exista

---

## 📊 Cambios en la Base de Datos

### Antes
```
Tabla: Peticiones
+------------------+---------------+
| Columna          | Tipo          |
+------------------+---------------+
| Id               | int (PK)      |
| Alias            | nvarchar(50)  |
| Contenido        | nvarchar(500) |
| FechaPublicacion | datetime2     |
| ContadorRezos    | int           |
+------------------+---------------+
```

### Después
```
Tabla: Peticiones
+----------------------+---------------+--------------------+
| Columna              | Tipo          | Constraints        |
+----------------------+---------------+--------------------+
| Id                   | int (PK)      |                    |
| UserId               | nvarchar(450) | FK → AspNetUsers   |
| Contenido            | nvarchar(500) | NOT NULL           |
| FechaPublicacion     | datetime2     |                    |
| ContadorRezos        | int           |                    |
| PublicarAnonimamente | bit           | NOT NULL, Default=1|
+----------------------+---------------+--------------------+

Índices:
- IX_Peticiones_FechaPublicacion (existente)
- IX_Peticiones_UserId (NUEVO)
```

---

✅ **Migración lista para ejecutar!**
