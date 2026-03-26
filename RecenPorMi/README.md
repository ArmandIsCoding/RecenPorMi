# 🙏 Recen Por Mí
### *"Recen por mí" - Papa Francisco*

Una aplicación web de intenciones de oración construida con **Blazor Server (.NET 10)**, inspirada en la humilde petición del Papa Francisco.

---

## ✨ Características

- 📝 **Publicación de Intenciones**: Los usuarios pueden compartir sus peticiones de oración de forma anónima o con su nombre
- 🙏 **Botón de Rezo**: Cada intención tiene un botón que permite a otros usuarios indicar que han rezado
- 🔄 **Actualizaciones en Tiempo Real**: SignalR mantiene los contadores actualizados para todos los usuarios conectados
- 🛡️ **Sistema Anti-Spam**: Previene múltiples rezos desde la misma IP en un período corto
- 🎨 **Diseño Minimalista**: Interfaz solemne y moderna con gradientes morados/azules
- 📱 **Responsive**: Funciona perfectamente en dispositivos móviles y de escritorio

---

## 🏗️ Arquitectura Técnica

### **Stack Tecnológico**
- **.NET 10** con Blazor Server
- **Entity Framework Core** (Code-First)
- **SQL Server Express** para persistencia
- **SignalR** para comunicación en tiempo real
- **Bootstrap 5** + CSS personalizado

### **Estructura del Proyecto**

```
RecenPorMi/
├── Data/
│   ├── Models/
│   │   ├── Peticion.cs         # Entidad de peticiones
│   │   └── Rezo.cs             # Entidad de rezos
│   ├── ApplicationDbContext.cs # Contexto EF Core
│   └── Migrations/             # Migraciones de base de datos
├── Services/
│   └── PeticionService.cs      # Lógica de negocio
├── Hubs/
│   └── PeticionHub.cs          # Hub de SignalR
├── Components/
│   ├── Pages/
│   │   └── Home.razor          # Página principal
│   └── Shared/
│       └── PeticionCard.razor  # Componente de tarjeta
├── wwwroot/
│   └── app.css                 # Estilos personalizados
└── Program.cs                  # Configuración de servicios
```

---

## 🗄️ Modelo de Datos

### **Peticion**
- `Id` (int, PK)
- `Alias` (string, default "Anónimo")
- `Contenido` (string, requerido, max 500 chars)
- `FechaPublicacion` (DateTime)
- `ContadorRezos` (int)

### **Rezo**
- `Id` (int, PK)
- `PeticionId` (int, FK)
- `Fecha` (DateTime)
- `IpHash` (string, SHA256) - **Anti-spam**

---

## 🚀 Instalación y Configuración

### **Prerrequisitos**
- Visual Studio 2022+ con .NET 10 SDK
- SQL Server Express instalado
- Cuenta `sa` configurada o Windows Authentication

### **Paso 1: Configurar la Cadena de Conexión**

Edita `appsettings.json` si tu instancia de SQL Server tiene un nombre diferente:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=RecenPorMiDB;User Id=sa;Password=123456;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### **Paso 2: Ejecutar Migraciones**

Abre la **Consola del Administrador de Paquetes** y ejecuta:

```powershell
Add-Migration InitialCreatePeticionesYRezos
Update-Database
```

### **Paso 3: Ejecutar la Aplicación**

Presiona **F5** en Visual Studio o ejecuta:

```bash
dotnet run
```

La aplicación estará disponible en `https://localhost:5001`

---

## 🎨 Personalización de Estilos

Los colores principales están definidos en `wwwroot/app.css`. Para cambiar el esquema de colores, busca:

```css
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
```

**Paletas sugeridas:**
- Verde esperanza: `#11998e 0%, #38ef7d 100%`
- Azul celestial: `#4facfe 0%, #00f2fe 100%`
- Rosa cálido: `#f093fb 0%, #f5576c 100%`

---

## 🔐 Sistema Anti-Spam

El sistema implementa las siguientes protecciones:

1. **Hash de IP**: Las IPs se almacenan como SHA256, nunca en texto plano
2. **Límite temporal**: Un usuario solo puede rezar por la misma intención cada 5 minutos
3. **Validación en servidor**: Toda la lógica está en `PeticionService.cs`

---

## 📡 Comunicación en Tiempo Real (SignalR)

### **Eventos del Hub**

```csharp
// Notifica cuando se publica una nueva petición
await Clients.All.SendAsync("NuevaPeticionPublicada");

// Notifica cuando alguien reza por una petición
await Clients.All.SendAsync("RezoActualizado", peticionId, nuevoContador);
```

### **Clientes Blazor**

Los componentes se suscriben automáticamente a estos eventos para actualizar la UI sin recargar la página.

---

## 🐛 Solución de Problemas

### **Error: "Cannot open database"**
```powershell
# Verificar que SQL Server está corriendo
Start-Service -Name "MSSQL$SQLEXPRESS"
```

### **Error: "Login failed for user 'sa'"**
Cambia a Windows Authentication:
```json
"Server=.\\SQLEXPRESS;Database=RecenPorMiDB;Integrated Security=True;TrustServerCertificate=True;"
```

### **Error: SignalR no conecta**
Verifica que el Hub esté mapeado en `Program.cs`:
```csharp
app.MapHub<PeticionHub>("/peticionhub");
```

---

## 📊 Optimizaciones de Rendimiento

- **Índices en Base de Datos**: 
  - `Peticiones.FechaPublicacion` para ordenamiento rápido
  - `Rezos.(PeticionId, IpHash, Fecha)` para validación anti-spam

- **Límite de Resultados**: Solo se cargan las 50 peticiones más recientes

- **SignalR con Reconexión Automática**: 
  ```csharp
  .WithAutomaticReconnect()
  ```

---

## 🔮 Roadmap (Mejoras Futuras)

- [ ] **IP Real del Usuario**: Implementar `IHttpContextAccessor` para obtener la IP real
- [ ] **Moderación de Contenido**: Panel admin para revisar/eliminar peticiones
- [ ] **Categorías**: Clasificar intenciones (salud, familia, trabajo, etc.)
- [ ] **Estadísticas**: Dashboard con métricas de rezos por día/semana
- [ ] **Notificaciones**: Avisar al autor cuando alguien reza por su intención
- [ ] **Compartir en Redes**: Botones para compartir peticiones en Facebook/Twitter
- [ ] **Modo Oscuro**: Toggle para usuarios que prefieren temas oscuros
- [ ] **Internacionalización**: Soporte para múltiples idiomas
- [ ] **PWA**: Convertir a Progressive Web App con soporte offline

---

## 🤝 Contribuciones

Este es un proyecto Open Source. Si querés contribuir:

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Sentite libre de usarlo, modificarlo y compartirlo.

---

## 🙏 Agradecimientos

- Inspirado por la humildad del Papa Francisco
- Construido con ❤️ para la comunidad católica global
- Gracias a todos los que rezan por las intenciones compartidas

---

## 📞 Contacto y Soporte

- **GitHub Issues**: Para reportar bugs o sugerir features
- **Documentación Completa**: Ver `INSTRUCCIONES_MIGRACION.md`

---

**¡Que Dios bendiga este proyecto y a todos los que lo usan! 🙏✨**
