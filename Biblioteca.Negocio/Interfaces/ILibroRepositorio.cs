using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio.Interfaces
{
    /// <summary>
    /// Interfaz para el patrón Repositorio de Libros.
    /// Permite desacoplar la Capa de Negocio de la implementación concreta de acceso a datos.
    /// (Requerimiento Opcional / Punto Extra)
    /// </summary>
    public interface ILibroRepositorio
    {
        Task<List<Libro>> ListarAsync();
        Task<List<Libro>> BuscarAsync(string filtro);
        Task<Libro?> ObtenerPorIdAsync(int libroId);
        Task<int> InsertarAsync(Libro libro);
        Task ActualizarAsync(Libro libro);
        Task EliminarLogicoAsync(int libroId);
        Task<bool> ExisteISBNAsync(string isbn, int libroId = 0);
        Task<bool> TienePrestamosPendientesAsync(int libroId);
        Task DescontarEjemplarAsync(int libroId);
        Task DevolverEjemplarAsync(int libroId);
    }
}
