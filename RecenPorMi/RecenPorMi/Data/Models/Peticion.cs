using System.ComponentModel.DataAnnotations;

namespace RecenPorMi.Data.Models
{
    public class Peticion
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Alias { get; set; } = "Anónimo";

        [Required(ErrorMessage = "La intención de oración es requerida")]
        [MaxLength(500)]
        public string Contenido { get; set; } = string.Empty;

        public DateTime FechaPublicacion { get; set; } = DateTime.Now;

        public int ContadorRezos { get; set; } = 0;

        // Relación con Rezos
        public virtual ICollection<Rezo> Rezos { get; set; } = new List<Rezo>();
    }
}
