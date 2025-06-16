using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace atajados.Models
{
    public class Atajado
    {
        [Key] public int Id { get; set; }

        [Required] public string Comunidad { get; set; } = string.Empty;
        [Required] public string NumeroAtajado { get; set; } = string.Empty;
        [Required] public string Nombre { get; set; } = string.Empty;
        [Required] public string CI { get; set; } = string.Empty;

        // Relación 1-N con Seguimiento
        public List<Seguimiento> Seguimientos { get; set; } = new();
    }
}
