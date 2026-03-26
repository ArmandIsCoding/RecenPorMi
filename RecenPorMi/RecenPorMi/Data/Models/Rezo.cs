using System.ComponentModel.DataAnnotations;

namespace RecenPorMi.Data.Models
{
    public class Rezo
    {
        public int Id { get; set; }

        public int PeticionId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [MaxLength(64)]
        public string IpHash { get; set; } = string.Empty;

        // Relación con Peticion
        public virtual Peticion? Peticion { get; set; }
    }
}
