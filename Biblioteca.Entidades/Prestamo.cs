using System;
using System.Collections.Generic;

namespace Biblioteca.Entidades
{
    /// <summary>
    /// Entidad POCO que representa la cabecera de un Préstamo.
    /// </summary>
    public class Prestamo
    {
        public int PrestamoId { get; set; }
        public int SocioId { get; set; }
        public DateTime FechaPrestamo { get; set; } = DateTime.Now;
        public DateTime FechaLimite { get; set; }
        public string Estado { get; set; } = "Activo";

        // Propiedad auxiliar para UI
        public string NombreSocio { get; set; } = string.Empty;

        // Lista de detalles asociados
        public List<DetallePrestamo> Detalles { get; set; } = new List<DetallePrestamo>();
    }
}
