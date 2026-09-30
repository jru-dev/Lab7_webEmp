using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    /// <summary>
    /// Acceso a datos para la entidad Socio.
    /// Operaciones mediante Stored Procedures con SqlParameter.
    /// </summary>
    public class SocioDatos
    {
        public async Task<List<Socio>> ListarAsync()
        {
            var lista = new List<Socio>();
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_Listar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(MapearSocio(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<List<Socio>> BuscarAsync(string filtro)
        {
            var lista = new List<Socio>();
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_Buscar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Filtro", SqlDbType.VarChar, 100) { Value = filtro ?? string.Empty });

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(MapearSocio(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<Socio?> ObtenerPorIdAsync(int socioId)
        {
            Socio? socio = null;
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_ObtenerPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SocioId", SqlDbType.Int) { Value = socioId });

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            socio = MapearSocio(reader);
                        }
                    }
                }
            }
            return socio;
        }

        public async Task<int> InsertarAsync(Socio socio)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_Insertar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.VarChar, 15) { Value = socio.DNI });
                    cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = socio.Nombre });
                    cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 100) { Value = (object?)socio.Email ?? DBNull.Value });

                    var paramId = new SqlParameter("@NuevoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(paramId);

                    await cmd.ExecuteNonQueryAsync();
                    return (int)paramId.Value;
                }
            }
        }

        public async Task ActualizarAsync(Socio socio)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_Actualizar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@SocioId", SqlDbType.Int) { Value = socio.SocioId });
                    cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.VarChar, 15) { Value = socio.DNI });
                    cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = socio.Nombre });
                    cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 100) { Value = (object?)socio.Email ?? DBNull.Value });

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task EliminarLogicoAsync(int socioId)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_EliminarLogico", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SocioId", SqlDbType.Int) { Value = socioId });

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> ExisteDNIAsync(string dni, int socioId = 0)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_ExisteDNI", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.VarChar, 15) { Value = dni });
                    cmd.Parameters.Add(new SqlParameter("@SocioId", SqlDbType.Int) { Value = socioId });

                    var result = await cmd.ExecuteScalarAsync();
                    int count = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    return count > 0;
                }
            }
        }

        public async Task<bool> TienePrestamosPendientesAsync(int socioId)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_TienePrestamosPendientes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SocioId", SqlDbType.Int) { Value = socioId });

                    var result = await cmd.ExecuteScalarAsync();
                    int count = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    return count > 0;
                }
            }
        }

        public async Task<int> ContarLibrosPendientesAsync(int socioId)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Socios_ContarLibrosPendientes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SocioId", SqlDbType.Int) { Value = socioId });

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private static Socio MapearSocio(SqlDataReader reader)
        {
            return new Socio
            {
                SocioId = reader.GetInt32(reader.GetOrdinal("SocioId")),
                DNI = reader.GetString(reader.GetOrdinal("DNI")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
            };
        }
    }
}
