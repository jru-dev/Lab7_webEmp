using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Excepciones;

namespace Biblioteca.Negocio
{
    /// <summary>
    /// Reglas de Negocio complejas para Registro de Préstamos y Devoluciones.
    /// Valida límites de libros por socio (máximo 3), stock disponible, estado activo,
    /// transaccionalidad y cálculo exacto de multas por morosidad (S/ 1.50 por día).
    /// </summary>
    public class PrestamoNegocio
    {
        private readonly PrestamoDatos _prestamoDatos;
        private readonly SocioDatos _socioDatos;
        private readonly LibroDatos _libroDatos;

        public const decimal TarifaMultaDiaria = 1.50m;

        public PrestamoNegocio()
        {
            _prestamoDatos = new PrestamoDatos();
            _socioDatos = new SocioDatos();
            _libroDatos = new LibroDatos();
        }

        public async Task<int> RegistrarPrestamoAsync(int socioId, List<int> libroIds, DateTime fechaLimite)
        {
            if (socioId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un socio válido para registrar el préstamo.");

            if (libroIds == null || libroIds.Count == 0)
                throw new ReglaNegocioException("Debe seleccionar al menos un libro para incluir en el préstamo.");

            if (fechaLimite.Date < DateTime.Today)
                throw new ReglaNegocioException("La fecha límite del préstamo no puede ser anterior a la fecha de hoy.");

            // 1. Validar existencia y estado Activo del socio
            var socio = await _socioDatos.ObtenerPorIdAsync(socioId);
            if (socio == null || !socio.Activo)
            {
                throw new ReglaNegocioException("El socio seleccionado no se encuentra activo o no existe en la base de datos.");
            }

            // 2. REGLA CLAVE: Un socio no puede acumular más de 3 libros pendientes
            int librosPendientesActuales = await _socioDatos.ContarLibrosPendientesAsync(socioId);
            int cantidadSolicitada = libroIds.Count;

            if (librosPendientesActuales + cantidadSolicitada > 3)
            {
                throw new ReglaNegocioException(
                    $"No se puede registrar el préstamo: El socio {socio.Nombre} registra actualmente {librosPendientesActuales} libro(s) pendiente(s). " +
                    $"Con los {cantidadSolicitada} libro(s) solicitados superaría el límite permitido de máximo 3 libros.");
            }

            // 3. Validar estado Activo y Ejemplares disponibles de cada libro
            foreach (int libroId in libroIds)
            {
                var libro = await _libroDatos.ObtenerPorIdAsync(libroId);
                if (libro == null || !libro.Activo)
                {
                    throw new ReglaNegocioException($"El libro (ID: {libroId}) se encuentra inactivo o fuera de catálogo.");
                }

                if (libro.Ejemplares <= 0)
                {
                    throw new ReglaNegocioException($"El libro '{libro.Titulo}' no cuenta con ejemplares suficientes en stock.");
                }
            }

            var prestamo = new Prestamo
            {
                SocioId = socioId,
                FechaPrestamo = DateTime.Today,
                FechaLimite = fechaLimite,
                Estado = "Activo"
            };

            // Ejecuta en una sola SqlTransaction
            return await _prestamoDatos.RegistrarPrestamoTransaccionalAsync(prestamo, libroIds);
        }

        /// <summary>
        /// Calcula la multa por retraso en la devolución: S/ 1.50 por día de retraso.
        /// </summary>
        public decimal CalcularMulta(DateTime fechaLimite, DateTime fechaDevolucion)
        {
            if (fechaDevolucion.Date <= fechaLimite.Date)
            {
                return 0.00m;
            }

            int diasMora = (int)(fechaDevolucion.Date - fechaLimite.Date).TotalDays;
            return diasMora * TarifaMultaDiaria;
        }

        public async Task<decimal> RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion)
        {
            if (prestamoId <= 0)
                throw new ReglaNegocioException("Identificador de préstamo no válido.");

            if (libroId <= 0)
                throw new ReglaNegocioException("Identificador de libro no válido.");

            var prestamo = await _prestamoDatos.ObtenerPorIdAsync(prestamoId);
            if (prestamo == null)
                throw new ReglaNegocioException("El préstamo indicado no fue encontrado.");

            if (fechaDevolucion.Date < prestamo.FechaPrestamo.Date)
            {
                throw new ReglaNegocioException("La fecha de devolución no puede ser anterior a la fecha en que se realizó el préstamo.");
            }

            // Calcular multa en la Capa de Negocio
            decimal multa = CalcularMulta(prestamo.FechaLimite, fechaDevolucion);

            // Registrar la devolución y actualización de stock/estado de préstamo en transacción
            await _prestamoDatos.RegistrarDevolucionTransaccionalAsync(prestamoId, libroId, fechaDevolucion.Date);

            return multa;
        }

        public async Task<List<Prestamo>> ListarActivosAsync()
        {
            return await _prestamoDatos.ListarActivosAsync();
        }

        public async Task<List<DetallePrestamo>> ObtenerDetallesPorPrestamoAsync(int prestamoId)
        {
            if (prestamoId <= 0)
                throw new ReglaNegocioException("Identificador de préstamo no válido.");

            return await _prestamoDatos.ObtenerDetallesPorPrestamoAsync(prestamoId);
        }

        public async Task<List<ReportePrestamoDto>> ObtenerReportePorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaFin.Date < fechaInicio.Date)
            {
                throw new ReglaNegocioException("La fecha final no puede ser menor a la fecha inicial para el reporte.");
            }

            return await _prestamoDatos.ObtenerReportePorFechasAsync(fechaInicio.Date, fechaFin.Date);
        }
    }
}
