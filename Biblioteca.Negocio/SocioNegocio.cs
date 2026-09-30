using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Excepciones;

namespace Biblioteca.Negocio
{
    /// <summary>
    /// Reglas de negocio para la gestión de Socios.
    /// Valida DNI único, campos requeridos y bloquea la baja lógica si existen préstamos pendientes.
    /// </summary>
    public class SocioNegocio
    {
        private readonly SocioDatos _socioDatos;

        public SocioNegocio()
        {
            _socioDatos = new SocioDatos();
        }

        public async Task<List<Socio>> ListarAsync()
        {
            return await _socioDatos.ListarAsync();
        }

        public async Task<List<Socio>> BuscarAsync(string filtro)
        {
            return await _socioDatos.BuscarAsync(filtro);
        }

        public async Task<Socio?> ObtenerPorIdAsync(int socioId)
        {
            if (socioId <= 0)
                throw new ReglaNegocioException("El identificador del socio no es válido.");

            return await _socioDatos.ObtenerPorIdAsync(socioId);
        }

        public async Task GuardarAsync(Socio socio)
        {
            if (socio == null)
                throw new ReglaNegocioException("Los datos del socio son nulos.");

            if (string.IsNullOrWhiteSpace(socio.Nombre))
                throw new ReglaNegocioException("El nombre del socio es obligatorio.");

            if (string.IsNullOrWhiteSpace(socio.DNI))
                throw new ReglaNegocioException("El DNI del socio es obligatorio.");

            string dniLimpio = socio.DNI.Trim();
            if (dniLimpio.Length != 8)
                throw new ReglaNegocioException("El DNI debe contener exactamente 8 caracteres.");

            // Regla: No permitir DNI duplicados
            bool existeDNI = await _socioDatos.ExisteDNIAsync(dniLimpio, socio.SocioId);
            if (existeDNI)
            {
                throw new ReglaNegocioException($"El DNI '{dniLimpio}' ya se encuentra registrado para otro socio en la biblioteca.");
            }

            socio.DNI = dniLimpio;

            if (socio.SocioId == 0)
            {
                await _socioDatos.InsertarAsync(socio);
            }
            else
            {
                await _socioDatos.ActualizarAsync(socio);
            }
        }

        public async Task EliminarLogicoAsync(int socioId)
        {
            if (socioId <= 0)
                throw new ReglaNegocioException("El identificador del socio no es válido.");

            // Regla: No dar de baja a socio con préstamos pendientes
            bool tienePendientes = await _socioDatos.TienePrestamosPendientesAsync(socioId);
            if (tienePendientes)
            {
                throw new ReglaNegocioException("No se puede dar de baja al socio porque registra libros en préstamo pendientes de devolución.");
            }

            await _socioDatos.EliminarLogicoAsync(socioId);
        }

        public async Task<int> ContarLibrosPendientesAsync(int socioId)
        {
            return await _socioDatos.ContarLibrosPendientesAsync(socioId);
        }
    }
}
