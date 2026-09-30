using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Biblioteca.Datos
{
    /// <summary>
    /// Clase encargada de proporcionar conexiones a la base de datos BibliotecaDB.
    /// Lee la cadena de conexión desde App.config del proyecto de presentación (WPF).
    /// </summary>
    public static class Conexion
    {
        private static string DefaultConnectionString =>
            @"Server=localhost;Database=BibliotecaDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection ObtenerConexion()
        {
            string? connectionString = ConfigurationManager.ConnectionStrings["BibliotecaDB"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Fallback por si acaso no esté en App.config durante pruebas
                connectionString = DefaultConnectionString;
            }

            return new SqlConnection(connectionString);
        }
    }
}
