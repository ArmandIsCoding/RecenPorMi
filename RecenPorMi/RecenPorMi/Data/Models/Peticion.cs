using System.ComponentModel.DataAnnotations;

namespace RecenPorMi.Data.Models
{
    public class Peticion
    {
        public int Id { get; set; }

        // FK al usuario creador
        [Required]
        public string UserId { get; set; } = string.Empty;

        // ✅ Descripción breve (obligatoria) - se muestra en cards
        [Required(ErrorMessage = "La descripción breve es requerida")]
        [MaxLength(200)]
        public string DescripcionBreve { get; set; } = string.Empty;

        // ✅ Contenido completo (opcional) - se muestra en página de detalle
        public string? ContenidoCompleto { get; set; }

        public DateTime FechaPublicacion { get; set; } = DateTime.Now;

        public int ContadorRezos { get; set; } = 0;

        // Control de anonimato
        public bool PublicarAnonimamente { get; set; } = true;

        // Relación con el usuario
        public virtual ApplicationUser? Usuario { get; set; }

        // Relación con Rezos
        public virtual ICollection<Rezo> Rezos { get; set; } = new List<Rezo>();

        // ✅ Relación con Imágenes
        public virtual ICollection<PeticionImagen> Imagenes { get; set; } = new List<PeticionImagen>();
    }
}
