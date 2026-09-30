namespace Biblioteca.Entidades
{
    /// <summary>
    /// Entidad POCO que representa a un Libro en la base de datos.
    /// Contiene la propiedad de navegación o apoyo NombreAutor para visualización en la UI.
    /// </summary>
    public class Libro
    {
        public int LibroId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int AutorId { get; set; }
        public int Ejemplares { get; set; }
        public bool Activo { get; set; } = true;

        // Propiedad calculada/mapeada de la consulta JOIN para mostrar en WPF
        public string NombreAutor { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Titulo} (ISBN: {ISBN}) - Stock: {Ejemplares}";
        }
    }
}
