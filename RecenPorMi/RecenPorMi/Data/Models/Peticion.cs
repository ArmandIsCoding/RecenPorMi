using System.ComponentModel.DataAnnotations;

namespace RecenPorMi.Data.Models
{
    public class Peticion
    {
        public int Id { get; set; }

        // FK al usuario creador
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La intención de oración es requerida")]
        [MaxLength(500)]
        public string Contenido { get; set; } = string.Empty;

        public DateTime FechaPublicacion { get; set; } = DateTime.Now;

        public int ContadorRezos { get; set; } = 0;

        // Control de anonimato
        public bool PublicarAnonimamente { get; set; } = true;

        // Relación con el usuario
        public virtual ApplicationUser? Usuario { get; set; }

        // Relación con Rezos
        public virtual ICollection<Rezo> Rezos { get; set; } = new List<Rezo>();
    }
}
