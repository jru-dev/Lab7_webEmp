using System;

namespace Biblioteca.Entidades
{
    /// <summary>
    /// Objeto DTO para representar las filas del reporte de préstamos por rango de fechas,
    /// obtenido con el INNER JOIN de Prestamos, DetallePrestamo, Libros y Socios.
    /// </summary>
    public class ReportePrestamoDto
    {
        public int PrestamoId { get; set; }
        public int SocioId { get; set; }
        public string Socio { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Libro { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaLimite { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
