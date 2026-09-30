namespace Biblioteca.Entidades
{
    /// <summary>
    /// Entidad POCO que representa a un Autor en la base de datos.
    /// </summary>
    public class Autor
    {
        public int AutorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Nacionalidad { get; set; }
        public bool Activo { get; set; } = true;

        public override string ToString()
        {
            return Nombre;
        }
    }
}
