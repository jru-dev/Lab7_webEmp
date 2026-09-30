using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Excepciones;
using Biblioteca.Negocio.Interfaces;

namespace Biblioteca.Negocio
{
    /// <summary>
    /// Adaptador que implementa ILibroRepositorio delegando en LibroDatos.
    /// Esto permite inyección de dependencias sin generar dependencia circular entre proyectos.
    /// </summary>
    public class LibroDatosAdapter : ILibroRepositorio
    {
        private readonly LibroDatos _datos = new LibroDatos();

        public Task<List<Libro>> ListarAsync() => _datos.ListarAsync();
        public Task<List<Libro>> BuscarAsync(string filtro) => _datos.BuscarAsync(filtro);
        public Task<Libro?> ObtenerPorIdAsync(int libroId) => _datos.ObtenerPorIdAsync(libroId);
        public Task<int> InsertarAsync(Libro libro) => _datos.InsertarAsync(libro);
        public Task ActualizarAsync(Libro libro) => _datos.ActualizarAsync(libro);
        public Task EliminarLogicoAsync(int libroId) => _datos.EliminarLogicoAsync(libroId);
        public Task<bool> ExisteISBNAsync(string isbn, int libroId = 0) => _datos.ExisteISBNAsync(isbn, libroId);
        public Task<bool> TienePrestamosPendientesAsync(int libroId) => _datos.TienePrestamosPendientesAsync(libroId);
        public Task DescontarEjemplarAsync(int libroId) => _datos.DescontarEjemplarAsync(libroId);
        public Task DevolverEjemplarAsync(int libroId) => _datos.DevolverEjemplarAsync(libroId);
    }

    /// <summary>
    /// Reglas de negocio para la gestión de Libros.
    /// Valida duplicados de ISBN, obligatoriedad de campos y restricciones de eliminación.
    /// Recibe ILibroRepositorio por constructor (Inyección de Dependencias - Punto Extra).
    /// </summary>
    public class LibroNegocio
    {
        private readonly ILibroRepositorio _repositorio;

        // Constructor por defecto que inyecta el repositorio concreto
        public LibroNegocio() : this(new LibroDatosAdapter())
        {
        }

        // Constructor con Inyección de Dependencias (Punto Extra)
        public LibroNegocio(ILibroRepositorio repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public async Task<List<Libro>> ListarAsync()
        {
            return await _repositorio.ListarAsync();
        }

        public async Task<List<Libro>> BuscarAsync(string filtro)
        {
            return await _repositorio.BuscarAsync(filtro);
        }

        public async Task<Libro?> ObtenerPorIdAsync(int libroId)
        {
            if (libroId <= 0)
                throw new ReglaNegocioException("El identificador del libro no es válido.");

            return await _repositorio.ObtenerPorIdAsync(libroId);
        }

        public async Task GuardarAsync(Libro libro)
        {
            if (libro == null)
                throw new ReglaNegocioException("Los datos del libro son nulos.");

            if (string.IsNullOrWhiteSpace(libro.Titulo))
                throw new ReglaNegocioException("El título del libro es obligatorio.");

            if (string.IsNullOrWhiteSpace(libro.ISBN))
                throw new ReglaNegocioException("El código ISBN del libro es obligatorio.");

            if (libro.AutorId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un autor válido para el libro.");

            if (libro.Ejemplares < 0)
                throw new ReglaNegocioException("El número de ejemplares no puede ser negativo.");

            // Regla: No permitir ISBN duplicados
            bool existe = await _repositorio.ExisteISBNAsync(libro.ISBN.Trim(), libro.LibroId);
            if (existe)
            {
                throw new ReglaNegocioException($"El ISBN '{libro.ISBN.Trim()}' ya está registrado para otro libro en el sistema.");
            }

            if (libro.LibroId == 0)
            {
                await _repositorio.InsertarAsync(libro);
            }
            else
            {
                await _repositorio.ActualizarAsync(libro);
            }
        }

        public async Task EliminarLogicoAsync(int libroId)
        {
            if (libroId <= 0)
                throw new ReglaNegocioException("El identificador del libro no es válido.");

            // Regla: No dar de baja un libro con préstamos pendientes
            bool tienePendientes = await _repositorio.TienePrestamosPendientesAsync(libroId);
            if (tienePendientes)
            {
                throw new ReglaNegocioException("No se puede dar de baja el libro porque tiene ejemplares en préstamos pendientes de devolución.");
            }

            await _repositorio.EliminarLogicoAsync(libroId);
        }
    }
}
