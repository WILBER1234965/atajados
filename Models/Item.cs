using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace atajados.Models
{
    // Índice único para evitar números duplicados
    [Index(nameof(Numero), IsUnique = true)]
    public class Item
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required] public string Descripcion { get; set; } = string.Empty;

        public string Unidad { get; set; } = string.Empty;

        [Required, Range(0.0001, double.MaxValue)]
        public decimal Cantidad { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }

        [NotMapped] public decimal Total => Cantidad * PrecioUnitario;

        public bool UsarEnSeguimiento { get; set; }
    }
}
