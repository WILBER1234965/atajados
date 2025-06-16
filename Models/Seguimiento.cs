using System;
using System.ComponentModel.DataAnnotations;

namespace atajados.Models
{
    public class Seguimiento
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int AtajadoId { get; set; }
        public Atajado Atajado { get; set; }
        [Required]
        public int ItemId { get; set; }
        public Item Item { get; set; }
        [Range(0, 100)]
        public int Porcentaje { get; set; } = 0;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public string FotoRuta { get; set; }
    }
}
