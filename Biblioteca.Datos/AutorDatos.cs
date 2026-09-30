using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    /// <summary>
    /// Acceso a datos para la entidad Autor.
    /// Utiliza únicamente procedimientos almacenados y SqlParameters.
    /// Devuelve listas y entidades POCO.
    /// </summary>
    public class AutorDatos
    {
        public async Task<List<Autor>> ListarAsync()
        {
            var lista = new List<Autor>();
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Autores_Listar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Autor
                            {
                                AutorId = reader.GetInt32(reader.GetOrdinal("AutorId")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Nacionalidad = reader.IsDBNull(reader.GetOrdinal("Nacionalidad")) ? null : reader.GetString(reader.GetOrdinal("Nacionalidad")),
                                Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<Autor?> ObtenerPorIdAsync(int autorId)
        {
            Autor? autor = null;
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Autores_ObtenerPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@AutorId", SqlDbType.Int) { Value = autorId });

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            autor = new Autor
                            {
                                AutorId = reader.GetInt32(reader.GetOrdinal("AutorId")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Nacionalidad = reader.IsDBNull(reader.GetOrdinal("Nacionalidad")) ? null : reader.GetString(reader.GetOrdinal("Nacionalidad")),
                                Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
                            };
                        }
                    }
                }
            }
            return autor;
        }
    }
}
