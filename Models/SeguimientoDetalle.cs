namespace atajados.Models
{
    public class SeguimientoDetalle
    {
        public int Id { get; set; }
        public int SeguimientoId { get; set; }
        public int ItemId { get; set; }
        public int PorcentajeAvance { get; set; }
        public string FotoPath { get; set; } = string.Empty;  // Valor por defecto
    }
}
