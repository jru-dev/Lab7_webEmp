using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    /// <summary>
    /// Acceso a datos para Préstamos y DetallePrestamo.
    /// Garantiza la integridad transaccional (SqlTransaction) en operaciones multi-tabla.
    /// </summary>
    public class PrestamoDatos
    {
        public async Task<int> RegistrarPrestamoTransaccionalAsync(Prestamo prestamo, List<int> libroIds)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int nuevoPrestamoId;

                        // 1. Insertar Cabecera de Préstamo
                        using (var cmdHeader = new SqlCommand("sp_Prestamos_Insertar", conn, transaction))
                        {
                            cmdHeader.CommandType = CommandType.StoredProcedure;
                            cmdHeader.Parameters.Add(new SqlParameter("@SocioId", SqlDbType.Int) { Value = prestamo.SocioId });
                            cmdHeader.Parameters.Add(new SqlParameter("@FechaPrestamo", SqlDbType.Date) { Value = prestamo.FechaPrestamo });
                            cmdHeader.Parameters.Add(new SqlParameter("@FechaLimite", SqlDbType.Date) { Value = prestamo.FechaLimite });
                            cmdHeader.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 20) { Value = prestamo.Estado });

                            var paramId = new SqlParameter("@NuevoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            cmdHeader.Parameters.Add(paramId);

                            await cmdHeader.ExecuteNonQueryAsync();
                            nuevoPrestamoId = (int)paramId.Value;
                        }

                        // 2. Insertar Detalles y Descontar Stock por cada libro
                        foreach (int libroId in libroIds)
                        {
                            // Inserción en DetallePrestamo
                            using (var cmdDetail = new SqlCommand("sp_DetallePrestamo_Insertar", conn, transaction))
                            {
                                cmdDetail.CommandType = CommandType.StoredProcedure;
                                cmdDetail.Parameters.Add(new SqlParameter("@PrestamoId", SqlDbType.Int) { Value = nuevoPrestamoId });
                                cmdDetail.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });

                                await cmdDetail.ExecuteNonQueryAsync();
                            }

                            // Descontar ejemplar del Libro
                            using (var cmdStock = new SqlCommand("sp_Libros_DescontarEjemplar", conn, transaction))
                            {
                                cmdStock.CommandType = CommandType.StoredProcedure;
                                cmdStock.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });

                                await cmdStock.ExecuteNonQueryAsync();
                            }
                        }

                        // Confirmar transacción
                        transaction.Commit();
                        return nuevoPrestamoId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<List<Prestamo>> ListarActivosAsync()
        {
            var lista = new List<Prestamo>();
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Prestamos_ListarActivos", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Prestamo
                            {
                                PrestamoId = reader.GetInt32(reader.GetOrdinal("PrestamoId")),
                                NombreSocio = reader.GetString(reader.GetOrdinal("Socio")),
                                FechaPrestamo = reader.GetDateTime(reader.GetOrdinal("FechaPrestamo")),
                                FechaLimite = reader.GetDateTime(reader.GetOrdinal("FechaLimite")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<Prestamo?> ObtenerPorIdAsync(int prestamoId)
        {
            Prestamo? prestamo = null;
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Prestamos_ObtenerPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@PrestamoId", SqlDbType.Int) { Value = prestamoId });

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            prestamo = new Prestamo
                            {
                                PrestamoId = reader.GetInt32(reader.GetOrdinal("PrestamoId")),
                                SocioId = reader.GetInt32(reader.GetOrdinal("SocioId")),
                                FechaPrestamo = reader.GetDateTime(reader.GetOrdinal("FechaPrestamo")),
                                FechaLimite = reader.GetDateTime(reader.GetOrdinal("FechaLimite")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            };
                        }
                    }
                }
            }
            return prestamo;
        }

        public async Task<List<DetallePrestamo>> ObtenerDetallesPorPrestamoAsync(int prestamoId)
        {
            var lista = new List<DetallePrestamo>();
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_DetallePrestamo_ListarPorPrestamo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@PrestamoId", SqlDbType.Int) { Value = prestamoId });

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new DetallePrestamo
                            {
                                PrestamoId = reader.GetInt32(reader.GetOrdinal("PrestamoId")),
                                LibroId = reader.GetInt32(reader.GetOrdinal("LibroId")),
                                TituloLibro = reader.GetString(reader.GetOrdinal("Titulo")),
                                FechaDevolucion = reader.IsDBNull(reader.GetOrdinal("FechaDevolucion")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaDevolucion"))
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task RegistrarDevolucionTransaccionalAsync(int prestamoId, int libroId, DateTime fechaDevolucion)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Actualizar fecha de devolución del libro
                        using (var cmdDev = new SqlCommand("sp_DetallePrestamo_RegistrarDevolucion", conn, transaction))
                        {
                            cmdDev.CommandType = CommandType.StoredProcedure;
                            cmdDev.Parameters.Add(new SqlParameter("@PrestamoId", SqlDbType.Int) { Value = prestamoId });
                            cmdDev.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });
                            cmdDev.Parameters.Add(new SqlParameter("@FechaDevolucion", SqlDbType.Date) { Value = fechaDevolucion });

                            await cmdDev.ExecuteNonQueryAsync();
                        }

                        // 2. Incrementar stock del libro devuelto
                        using (var cmdStock = new SqlCommand("sp_Libros_DevolverEjemplar", conn, transaction))
                        {
                            cmdStock.CommandType = CommandType.StoredProcedure;
                            cmdStock.Parameters.Add(new SqlParameter("@LibroId", SqlDbType.Int) { Value = libroId });

                            await cmdStock.ExecuteNonQueryAsync();
                        }

                        // 3. Verificar si quedan libros pendientes en este préstamo
                        int pendientes = 0;
                        using (var cmdCheck = new SqlCommand("sp_Prestamos_ContarPendientes", conn, transaction))
                        {
                            cmdCheck.CommandType = CommandType.StoredProcedure;
                            cmdCheck.Parameters.Add(new SqlParameter("@PrestamoId", SqlDbType.Int) { Value = prestamoId });

                            var res = await cmdCheck.ExecuteScalarAsync();
                            pendientes = res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
                        }

                        // 4. Si ya no hay libros pendientes, cambiar estado del préstamo a "Devuelto" (o "Cerrado")
                        if (pendientes == 0)
                        {
                            using (var cmdStatus = new SqlCommand("sp_Prestamos_ActualizarEstado", conn, transaction))
                            {
                                cmdStatus.CommandType = CommandType.StoredProcedure;
                                cmdStatus.Parameters.Add(new SqlParameter("@PrestamoId", SqlDbType.Int) { Value = prestamoId });
                                cmdStatus.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 20) { Value = "Devuelto" });

                                await cmdStatus.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<List<ReportePrestamoDto>> ObtenerReportePorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var lista = new List<ReportePrestamoDto>();
            using (var conn = Conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_Prestamos_ReportePorFechas", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@FechaInicio", SqlDbType.Date) { Value = fechaInicio });
                    cmd.Parameters.Add(new SqlParameter("@FechaFin", SqlDbType.Date) { Value = fechaFin });

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new ReportePrestamoDto
                            {
                                PrestamoId = reader.GetInt32(reader.GetOrdinal("PrestamoId")),
                                SocioId = reader.GetInt32(reader.GetOrdinal("SocioId")),
                                Socio = reader.GetString(reader.GetOrdinal("Socio")),
                                DNI = reader.GetString(reader.GetOrdinal("DNI")),
                                Libro = reader.GetString(reader.GetOrdinal("Libro")),
                                ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                                FechaPrestamo = reader.GetDateTime(reader.GetOrdinal("FechaPrestamo")),
                                FechaLimite = reader.GetDateTime(reader.GetOrdinal("FechaLimite")),
                                FechaDevolucion = reader.IsDBNull(reader.GetOrdinal("FechaDevolucion")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaDevolucion")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}
