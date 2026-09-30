using System;

namespace Biblioteca.Entidades
{
    /// <summary>
    /// Entidad POCO que representa el detalle de un libro en un Préstamo.
    /// </summary>
    public class DetallePrestamo
    {
        public int PrestamoId { get; set; }
        public int LibroId { get; set; }
        public DateTime? FechaDevolucion { get; set; }

        // Propiedad de soporte para UI
        public string TituloLibro { get; set; } = string.Empty;
    }
}
