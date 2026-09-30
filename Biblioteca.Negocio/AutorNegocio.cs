using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    /// <summary>
    /// Reglas de negocio básicas para la entidad Autor.
    /// </summary>
    public class AutorNegocio
    {
        private readonly AutorDatos _autorDatos = new AutorDatos();

        public Task<List<Autor>> ListarAsync() => _autorDatos.ListarAsync();
        public Task<Autor?> ObtenerPorIdAsync(int autorId) => _autorDatos.ObtenerPorIdAsync(autorId);
    }
}
