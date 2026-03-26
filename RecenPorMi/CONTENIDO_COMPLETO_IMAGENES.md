# 📸 Sistema Completo: Descripción Breve + Contenido Completo + Imágenes

## 🎯 Objetivo

Implementar un sistema similar a prayerrequest.com con:
- ✅ **Descripción breve** (200 chars, obligatoria) → se muestra en cards
- ✅ **Contenido completo** (opcional, WYSIWYG) → se muestra en página de detalle
- ✅ **Múltiples imágenes** (opcional) → galería en página de detalle
- ✅ **Página de detalle** individual para cada intención

---

## 📊 Cambios en Modelos

### **1. Peticion.cs** - Actualizado

```csharp
public class Peticion
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;

    // ✅ NUEVO: Descripción breve (obligatoria)
    [Required(ErrorMessage = "La descripción breve es requerida")]
    [MaxLength(200)]
    public string DescripcionBreve { get; set; } = string.Empty;

    // ✅ NUEVO: Contenido completo (opcional)
    public string? ContenidoCompleto { get; set; }

    public DateTime FechaPublicacion { get; set; } = DateTime.Now;
    public int ContadorRezos { get; set; } = 0;
    public bool PublicarAnonimamente { get; set; } = true;

    public virtual ApplicationUser? Usuario { get; set; }
    public virtual ICollection<Rezo> Rezos { get; set; } = new List<Rezo>();
    
    // ✅ NUEVO: Relación con imágenes
    public virtual ICollection<PeticionImagen> Imagenes { get; set; } = new List<PeticionImagen>();
}
```

### **2. PeticionImagen.cs** - NUEVO

```csharp
public class PeticionImagen
{
    public int Id { get; set; }

    [Required]
    public int PeticionId { get; set; }

    [Required]
    [MaxLength(255)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string RutaImagen { get; set; } = string.Empty;

    public int Orden { get; set; } = 0;
    public DateTime FechaSubida { get; set; } = DateTime.Now;

    public virtual Peticion? Peticion { get; set; }
}
```

---

## 📋 Próximos Pasos

### **Paso 1: Migración de Base de Datos**

Ejecutar (con la app detenida):
```powershell
dotnet ef migrations add AgregarDescripcionContenidoImagenes
dotnet ef database update
```

Esta migración:
- Renombra `Contenido` → `DescripcionBreve`
- Agrega `ContenidoCompleto` (nullable)
- Crea tabla `PeticionImagenes` con FK a `Peticiones`

⚠️ **IMPORTANTE**: Si ya tienes datos, la migración necesitará:
```sql
-- En el método Up() de la migración
EXEC sp_rename 'Peticiones.Contenido', 'DescripcionBreve', 'COLUMN';
```

---

### **Paso 2: Actualizar Publicar.razor**

**Campos necesarios:**
1. **DescripcionBreve** - TextArea corto (200 chars)
2. **ContenidoCompleto** - Editor WYSIWYG (recomiendo QuillJS o Blazor.RichTextEditor)
3. **InputFile** - Multiple files para imágenes

**Opción A: Editor Simple (TextArea)**
```razor
<div class="mb-3">
    <label>Descripción Breve (obligatorio)</label>
    <InputTextArea @bind-Value="Input.DescripcionBreve" 
                   maxlength="200" 
                   rows="3" />
</div>

<div class="mb-3">
    <label>Contenido Completo (opcional)</label>
    <InputTextArea @bind-Value="Input.ContenidoCompleto" 
                   rows="8" />
</div>

<div class="mb-3">
    <label>Imágenes (opcional)</label>
    <InputFile OnChange="@HandleFileSelection" 
               multiple 
               accept="image/*" />
</div>
```

**Opción B: Editor WYSIWYG (QuillJS)**

Instalar paquete:
```powershell
dotnet add package Blazorise.RichTextEdit
```

Uso:
```razor
<RichTextEdit @ref="richTextEditRef"
              Theme="RichTextEditTheme.Snow"
              ContentChanged="@OnContentChanged"
              Placeholder="Escribe el contenido completo aquí...">
</RichTextEdit>

@code {
    RichTextEdit richTextEditRef;
    
    async Task OnContentChanged()
    {
        Input.ContenidoCompleto = await richTextEditRef.GetHTMLAsync();
    }
}
```

---

### **Paso 3: Crear DetalleIntención.razor**

**Ruta:** `RecenPorMi\Components\Pages\DetalleIntención.razor`

```razor
@page "/intenciones/{id:int}"
@rendermode InteractiveServer
@using RecenPorMi.Data.Models
@inject RecenPorMi.Services.IPeticionService PeticionService
@inject NavigationManager NavigationManager

<PageTitle>@(peticion?.DescripcionBreve ?? "Intención") - Recen Por Mí</PageTitle>

@if (cargando)
{
    <div class="text-center py-5">
        <div class="spinner-border"></div>
        <p>Cargando intención...</p>
    </div>
}
else if (peticion == null)
{
    <div class="alert alert-warning">
        <h4>Intención no encontrada</h4>
        <a href="/" class="btn btn-primary">Volver al inicio</a>
    </div>
}
else
{
    <div class="container py-5">
        <div class="row justify-content-center">
            <div class="col-lg-10 col-xl-8">
                
                <!-- Header -->
                <div class="detalle-header mb-4">
                    <a href="/" class="btn btn-link mb-3">← Volver a intenciones</a>
                    
                    <div class="d-flex justify-content-between align-items-start mb-3">
                        <div>
                            <h5 class="mb-1">@ObtenerNombreAutor()</h5>
                            <small class="text-muted">@FormatearFecha(peticion.FechaPublicacion)</small>
                        </div>
                        
                        <button class="btn btn-primary" @onclick="Rezar">
                            <span class="me-2">🙏</span>
                            Rezar (@peticion.ContadorRezos)
                        </button>
                    </div>
                    
                    <h1 class="display-6 fw-bold">@peticion.DescripcionBreve</h1>
                </div>

                <!-- Contenido Completo -->
                @if (!string.IsNullOrEmpty(peticion.ContenidoCompleto))
                {
                    <div class="detalle-contenido card p-4 mb-4">
                        <div class="contenido-html">
                            @((MarkupString)peticion.ContenidoCompleto)
                        </div>
                    </div>
                }

                <!-- Galería de Imágenes -->
                @if (peticion.Imagenes.Any())
                {
                    <div class="detalle-imagenes mb-4">
                        <h4 class="mb-3">Imágenes</h4>
                        <div class="row g-3">
                            @foreach (var imagen in peticion.Imagenes)
                            {
                                <div class="col-6 col-md-4">
                                    <img src="@imagen.RutaImagen" 
                                         alt="@imagen.NombreArchivo" 
                                         class="img-fluid rounded shadow-sm"
                                         @onclick="() => MostrarImagenCompleta(imagen.RutaImagen)" />
                                </div>
                            }
                        </div>
                    </div>
                }

            </div>
        </div>
    </div>
}

<style>
    .detalle-header h1 {
        color: #2d3748;
        line-height: 1.3;
    }

    .detalle-contenido {
        background: white;
        border: 1px solid #e2e8f0;
    }

    .contenido-html {
        font-size: 1.1rem;
        line-height: 1.8;
        color: #4a5568;
    }

    .contenido-html h2 {
        font-size: 1.5rem;
        margin-top: 1.5rem;
        margin-bottom: 1rem;
    }

    .contenido-html p {
        margin-bottom: 1rem;
    }

    .detalle-imagenes img {
        cursor: pointer;
        transition: transform 0.2s;
    }

    .detalle-imagenes img:hover {
        transform: scale(1.05);
    }
</style>

@code {
    [Parameter]
    public int Id { get; set; }

    private Peticion? peticion;
    private bool cargando = true;

    protected override async Task OnInitializedAsync()
    {
        await CargarPeticion();
    }

    private async Task CargarPeticion()
    {
        try
        {
            peticion = await PeticionService.ObtenerPeticionPorIdAsync(Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            cargando = false;
        }
    }

    private string ObtenerNombreAutor()
    {
        if (peticion.PublicarAnonimamente)
            return "Anónimo";
        
        return peticion.Usuario?.DisplayName ?? "Usuario";
    }

    private string FormatearFecha(DateTime fecha)
    {
        var diferencia = DateTime.Now - fecha;

        if (diferencia.TotalMinutes < 1)
            return "Hace un momento";
        if (diferencia.TotalHours < 1)
            return $"Hace {(int)diferencia.TotalMinutes} min";
        if (diferencia.TotalDays < 1)
            return $"Hace {(int)diferencia.TotalHours}h";
        if (diferencia.TotalDays < 7)
            return $"Hace {(int)diferencia.TotalDays}d";

        return fecha.ToString("dd/MM/yyyy");
    }

    private async Task Rezar()
    {
        try
        {
            var ipAddress = "127.0.0.1"; // TODO: Obtener IP real
            var exito = await PeticionService.RegistrarRezoAsync(peticion.Id, ipAddress);
            
            if (exito)
            {
                peticion.ContadorRezos++;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void MostrarImagenCompleta(string ruta)
    {
        // TODO: Implementar modal de imagen completa
    }
}
```

---

### **Paso 4: Actualizar PeticionCard.razor**

Cambiar para mostrar solo descripción breve y agregar botón "Ver detalles":

```razor
<div class="peticion-card">
    <div class="peticion-header">
        <span class="peticion-alias">@ObtenerNombreAutor()</span>
        <span class="peticion-fecha">@FormatearFecha(Peticion.FechaPublicacion)</span>
    </div>
    
    <div class="peticion-contenido">
        @Peticion.DescripcionBreve
        
        <!-- Indicador de contenido adicional -->
        @if (!string.IsNullOrEmpty(Peticion.ContenidoCompleto) || Peticion.Imagenes.Any())
        {
            <div class="mt-2">
                @if (!string.IsNullOrEmpty(Peticion.ContenidoCompleto))
                {
                    <span class="badge bg-info me-1">📄 Contenido completo</span>
                }
                @if (Peticion.Imagenes.Any())
                {
                    <span class="badge bg-secondary">📷 @Peticion.Imagenes.Count imagen(es)</span>
                }
            </div>
        }
    </div>
    
    <div class="peticion-footer">
        <button class="btn-rezo" @onclick="Rezar" disabled="@rezando">
            <span class="emoji-rezo">🙏</span>
            <span class="contador">@contadorLocal</span>
        </button>
        
        <a href="/intenciones/@Peticion.Id" class="btn btn-sm btn-outline-primary">
            Ver detalles →
        </a>
    </div>
</div>
```

---

## 🖼️ Manejo de Imágenes

### **Opción 1: Guardar en wwwroot (Desarrollo)**

```csharp
// En Publicar.razor
private async Task HandleFileSelection(InputFileChangeEventArgs e)
{
    var maxFiles = 5;
    var maxFileSize = 5 * 1024 * 1024; // 5MB
    
    if (e.FileCount > maxFiles)
    {
        mensaje = $"Máximo {maxFiles} imágenes permitidas";
        return;
    }

    rutasImagenes.Clear();

    foreach (var file in e.GetMultipleFiles(maxFiles))
    {
        if (file.Size > maxFileSize)
        {
            mensaje = $"La imagen {file.Name} es muy grande (máx 5MB)";
            continue;
        }

        var extension = Path.GetExtension(file.Name);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var savePath = Path.Combine("wwwroot", "uploads", "peticiones", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(savePath));

        await using FileStream fs = new(savePath, FileMode.Create);
        await file.OpenReadStream(maxFileSize).CopyToAsync(fs);

        rutasImagenes.Add($"/uploads/peticiones/{fileName}");
    }
}
```

### **Opción 2: Azure Blob Storage (Producción)**

```csharp
// Instalar:
// dotnet add package Azure.Storage.Blobs

private async Task SubirImagenesAzure(IReadOnlyList<IBrowserFile> files)
{
    var connectionString = Configuration["AzureStorage:ConnectionString"];
    var containerName = "peticion-imagenes";
    
    var blobServiceClient = new BlobServiceClient(connectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    await containerClient.CreateIfNotExistsAsync();

    foreach (var file in files)
    {
        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
        var blobClient = containerClient.GetBlobClient(blobName);
        
        await using var stream = file.OpenReadStream(5 * 1024 * 1024);
        await blobClient.UploadAsync(stream, overwrite: true);
        
        rutasImagenes.Add(blobClient.Uri.ToString());
    }
}
```

---

##resumen para migrar datos existentes


```sql
-- Script de migración de datos existentes
-- Ejecutar DESPUÉS de aplicar la migración EF Core

-- Paso 1: Renombrar columna Contenido → DescripcionBreve
EXEC sp_rename 'Peticiones.Contenido', 'DescripcionBreve', 'COLUMN';

-- Paso 2: Si DescripcionBreve supera 200 caracteres, truncar
UPDATE Peticiones
SET DescripcionBreve = LEFT(DescripcionBreve, 197) + '...'
WHERE LEN(DescripcionBreve) > 200;

-- Paso 3: ContenidoCompleto ya se creó como nullable, no necesita datos
```

---

## ✅ Checklist de Implementación

- [ ] Actualizar modelos (Peticion.cs, PeticionImagen.cs)
- [ ] Actualizar ApplicationDbContext
- [ ] Actualizar PeticionService
- [ ] Crear migración y aplicar
- [ ] Actualizar Publicar.razor (descripción breve + contenido + imágenes)
- [ ] Actualizar PeticionCard.razor (solo breve + botón "Ver detalles")
- [ ] Crear DetalleIntención.razor (página completa)
- [ ] Implementar upload de imágenes
- [ ] Actualizar NavMenu si es necesario
- [ ] Implementar modal para zoom de imágenes (opcional)
- [ ] Testing completo

---

**Próximo paso**: ¿Quieres que continúe con la implementación completa del formulario Publicar.razor con editor WYSIWYG y upload de imágenes?
