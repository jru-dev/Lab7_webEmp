using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    /// <summary>
    /// Acceso a datos para la entidad Libro.
    /// Opera estrictamente mediante procedimientos almacenados y parámetros SqlClient.
    /// </summary>
    public class LibroDatos
    {
        public async Task<List<Libro>> ListarAsync()
        {
            var lista = new List<Libro>();
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_Listar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(MapearLibro(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<List<Libro>> BuscarAsync(string filtro)
        {
            var lista = new List<Libro>();
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_Buscar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Filtro", SqlDbType.VarChar, 150) { Value = filtro ?? string.Empty });

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(MapearLibro(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<Libro?> ObtenerPorIdAsync(int libroId)
        {
            Libro? libro = null;
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_ObtenerPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            libro = MapearLibro(reader);
                        }
                    }
                }
            }
            return libro;
        }

        public async Task<int> InsertarAsync(Libro libro)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_Insertar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Titulo", SqlDbType.VarChar, 150) { Value = libro.Titulo });
                    cmd.Parameters.Add(new SqlParameter("@ISBN", SqlDbType.VarChar, 20) { Value = libro.ISBN });
                    cmd.Parameters.Add(new SqlParameter("@AutorId", SqlDbType.Int) { Value = libro.AutorId });
                    cmd.Parameters.Add(new SqlParameter("@Ejemplares", SqlDbType.Int) { Value = libro.Ejemplares });

                    var paramId = new SqlParameter("@NuevoId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(paramId);

                    await cmd.ExecuteNonQueryAsync();
                    return (int)paramId.Value;
                }
            }
        }

        public async Task ActualizarAsync(Libro libro)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_Actualizar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libro.LibroId });
                    cmd.Parameters.Add(new SqlParameter("@Titulo", SqlDbType.VarChar, 150) { Value = libro.Titulo });
                    cmd.Parameters.Add(new SqlParameter("@ISBN", SqlDbType.VarChar, 20) { Value = libro.ISBN });
                    cmd.Parameters.Add(new SqlParameter("@AutorId", SqlDbType.Int) { Value = libro.AutorId });
                    cmd.Parameters.Add(new SqlParameter("@Ejemplares", SqlDbType.Int) { Value = libro.Ejemplares });

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task EliminarLogicoAsync(int libroId)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_EliminarLogico", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> ExisteISBNAsync(string isbn, int libroId = 0)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_ExisteISBN", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ISBN", SqlDbType.VarChar, 20) { Value = isbn });
                    cmd.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });

                    var result = await cmd.ExecuteScalarAsync();
                    int count = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    return count > 0;
                }
            }
        }

        public async Task<bool> TienePrestamosPendientesAsync(int libroId)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_TienePrestamosPendientes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });

                    var result = await cmd.ExecuteScalarAsync();
                    int count = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    return count > 0;
                }
            }
        }

        public async Task DescontarEjemplarAsync(int libroId)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_DescontarEjemplar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DevolverEjemplarAsync(int libroId)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Libros_DevolverEjemplar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static Libro MapearLibro(SqlDataReader reader)
        {
            return new Libro
            {
                LibroId = reader.GetInt32(reader.GetOrdinal("LibroId")),
                Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                AutorId = reader.GetInt32(reader.GetOrdinal("AutorId")),
                Ejemplares = reader.GetInt32(reader.GetOrdinal("Ejemplares")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                NombreAutor = reader.IsDBNull(reader.GetOrdinal("NombreAutor")) ? string.Empty : reader.GetString(reader.GetOrdinal("NombreAutor"))
            };
        }
    }
}
