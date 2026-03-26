using System.ComponentModel.DataAnnotations;

namespace RecenPorMi.Data.Models
{
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

        // Relación con Peticion
        public virtual Peticion? Peticion { get; set; }
    }
}
