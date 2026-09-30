namespace Biblioteca.Entidades
{
    /// <summary>
    /// Entidad POCO que representa a un Socio de la biblioteca.
    /// </summary>
    public class Socio
    {
        public int SocioId { get; set; }
        public string DNI { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool Activo { get; set; } = true;

        public override string ToString()
        {
            return $"{Nombre} (DNI: {DNI})";
        }
    }
}
