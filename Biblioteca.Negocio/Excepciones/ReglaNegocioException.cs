using System;

namespace Biblioteca.Negocio.Excepciones
{
    /// <summary>
    /// Excepción personalizada para capturar y notificar violaciones a las Reglas de Negocio.
    /// Permite desplegar mensajes amigables y en español hacia la capa de presentación (WPF).
    /// </summary>
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje)
        {
        }

        public ReglaNegocioException(string mensaje, Exception innerException) : base(mensaje, innerException)
        {
        }
    }
}
