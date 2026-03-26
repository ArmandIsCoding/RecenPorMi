# 🔧 Fix Aplicado: Límites de SignalR y Configuración de Archivos

## 🎯 Problema Identificado

Los mensajes de `BrowserRefreshMiddleware` que viste son **normales** y no son la causa del problema.

El problema real era que **Blazor Server tiene límites de tamaño de mensaje** muy bajos por defecto:
- **SignalR MaximumReceiveMessageSize**: 32KB (predeterminado) ❌
- **Kestrel MaxRequestBodySize**: 30MB (predeterminado) ❌  
- **FormOptions MultipartBodyLengthLimit**: 128MB (predeterminado) ✅

Cuando intentas subir imágenes de 1-5MB, los datos se envían a través de SignalR y **exceden el límite de 32KB**, causando que la solicitud falle silenciosamente.

## ✅ Cambios Aplicados en `Program.cs`

### 1. **Configurar Kestrel para archivos grandes**
```csharp
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});
```

### 2. **Aumentar límite de SignalR**
```csharp
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
    options.EnableDetailedErrors = true; // Errores detallados
});
```

### 3. **Configurar FormOptions**
```csharp
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});
```

### 4. **Mejorar manejo de archivos en `Publicar.razor`**
- Agregado try-catch alrededor de cada archivo
- Usar `using` para el stream (mejor gestión de recursos)
- Más logging detallado

## 🧪 Cómo Probar Ahora

### **Paso 1: Detener la aplicación actual**
Presiona `Ctrl + C` en la terminal donde está corriendo la app.

### **Paso 2: Limpiar y reconstruir**
```powershell
cd RecenPorMi
dotnet clean
dotnet build
```

### **Paso 3: Ejecutar SIN dotnet watch** (para prueba inicial)
```powershell
dotnet run
```

> **Nota**: Usa `dotnet run` en lugar de `dotnet watch run` para la primera prueba. Esto elimina el middleware de refresh que podría estar interfiriendo.

### **Paso 4: Probar en el navegador**
1. Abre `https://localhost:XXXX` (el puerto que aparezca)
2. Inicia sesión o regístrate
3. Ve a `/publicar`
4. **Intenta estas combinaciones**:

   **✅ Test A**: Solo descripción breve
   ```
   Descripción: "Recen por mi familia"
   Contenido: [vacío]
   Imágenes: [ninguna]
   ```

   **✅ Test B**: Con contenido completo simple
   ```
   Descripción: "Recen por mi familia"
   Contenido: "Necesito muchas oraciones"
   Imágenes: [ninguna]
   ```

   **✅ Test C**: Con 1 imagen pequeña (<1MB)
   ```
   Descripción: "Recen por mi familia"
   Contenido: [vacío]
   Imágenes: [1 imagen JPG pequeña]
   ```

   **✅ Test D**: Con todo
   ```
   Descripción: "Recen por mi familia"
   Contenido: "Contexto completo aquí con <b>formato</b>"
   Imágenes: [2-3 imágenes]
   ```

### **Paso 5: Observar los logs**
En la consola verás ahora mensajes como:
```
Procesando archivo: foto.jpg, Tamaño: 1234567 bytes
Bytes leídos: 1234567
Preview creado exitosamente para: foto.jpg
Procesando 2 imágenes...
Creando directorio: D:\...\wwwroot\uploads\peticiones
Guardando imagen: a1b2c3d4-...jpg
Imagen guardada exitosamente: /uploads/peticiones/a1b2c3d4-...jpg
Creando petición - Breve: 20 chars, Completo: 45 chars, Imágenes: 2
Petición creada exitosamente
```

## 📊 Qué Esperar

### ✅ **Si funciona correctamente**
- Verás los logs detallados paso a paso
- El botón dirá "Publicando..." brevemente
- Aparecerá "✅ ¡Tu intención ha sido publicada con éxito!"
- Serás redirigido a Home
- La intención aparecerá con las imágenes

### ❌ **Si aún falla**
Verás un mensaje específico:
- `"❌ El archivo X excede el tamaño máximo de 5MB"` → Archivo muy grande
- `"❌ Error al procesar X: ..."` → Problema con archivo específico
- `"❌ Error al publicar la intención: ..."` → Error en guardado

## 🔍 Debugging Adicional

### Ver información detallada de SignalR
Si quieres ver más detalles de SignalR, edita `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore.SignalR": "Debug",
      "Microsoft.AspNetCore.Http.Connections": "Debug"
    }
  }
}
```

## 🎯 Límites Actuales Configurados

| Componente | Límite Anterior | Límite Nuevo |
|------------|----------------|--------------|
| SignalR Message Size | **32 KB** | **10 MB** ✅ |
| Kestrel Request Body | **30 MB** | **50 MB** ✅ |
| Form MultipartBody | **128 MB** | **50 MB** ✅ |
| Por archivo (validación) | - | **5 MB** ✅ |
| Total de imágenes | - | **5 archivos** ✅ |

Esto significa que ahora puedes subir:
- ✅ Hasta 5 imágenes
- ✅ Cada imagen hasta 5MB
- ✅ Total combinado hasta 25MB (5 × 5MB)
- ✅ Contenido completo sin límite de caracteres

## 🚀 Siguientes Pasos

1. **Prueba primero SIN `dotnet watch`**:
   ```powershell
   dotnet run
   ```

2. **Si funciona**, luego prueba CON `dotnet watch`:
   ```powershell
   dotnet watch run
   ```

3. **Comparte los logs** que veas en consola cuando intentes publicar

## 💡 Consejos

- **Usa imágenes pequeñas primero** (100-500KB) para probar
- **Después prueba con imágenes más grandes** (2-5MB)
- **Revisa la carpeta** `wwwroot/uploads/peticiones/` - las imágenes deberían aparecer ahí
- **Mira la base de datos** - verifica que los registros `PeticionImagen` se estén creando

---

**Estado**: Cambios aplicados y compilados ✅  
**Acción requerida**: Detén la app actual, ejecuta `dotnet run` y prueba nuevamente
