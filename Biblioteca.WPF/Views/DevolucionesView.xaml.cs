using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.Negocio.Excepciones;

namespace Biblioteca.WPF.Views
{
    /// <summary>
    /// Vista para Procesar Devoluciones de Libros y Muestra de Multas Calculadas.
    /// Utiliza PrestamoNegocio.CalcularMulta para visualización previa y confirmación.
    /// </summary>
    public partial class DevolucionesView : UserControl
    {
        private readonly PrestamoNegocio _prestamoNegocio;
        private Prestamo? _prestamoSeleccionado;

        public DevolucionesView()
        {
            InitializeComponent();
            _prestamoNegocio = new PrestamoNegocio();
            dpFechaDevolucion.SelectedDate = DateTime.Today;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarPrestamosActivosAsync();
        }

        private async Task CargarPrestamosActivosAsync()
        {
            try
            {
                var prestamos = await _prestamoNegocio.ListarActivosAsync();
                cboPrestamoActivo.ItemsSource = prestamos;
                cboPrestamoActivo.SelectedIndex = -1;
                cboLibroDetalle.ItemsSource = null;
                ResetearFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar préstamos activos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void cboPrestamoActivo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPrestamoActivo.SelectedItem is Prestamo p)
            {
                _prestamoSeleccionado = p;
                txtFechaPrestamo.Text = p.FechaPrestamo.ToString("dd/MM/yyyy");
                txtFechaLimite.Text = p.FechaLimite.ToString("dd/MM/yyyy");

                try
                {
                    // Cargar los libros pendientes de este préstamo
                    var detalles = await _prestamoNegocio.ObtenerDetallesPorPrestamoAsync(p.PrestamoId);
                    var pendientes = detalles.Where(d => d.FechaDevolucion == null).ToList();

                    cboLibroDetalle.ItemsSource = pendientes;
                    if (pendientes.Count > 0)
                        cboLibroDetalle.SelectedIndex = 0;

                    CalcularMulta();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al consultar detalles del préstamo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                ResetearFormulario();
            }
        }

        private void cboLibroDetalle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalcularMulta();
        }

        private void dpFechaDevolucion_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalcularMulta();
        }

        private void CalcularMulta()
        {
            if (_prestamoSeleccionado == null || dpFechaDevolucion.SelectedDate == null)
            {
                txtMontoMulta.Text = "S/ 0.00";
                txtDetalleMora.Text = "Seleccione un préstamo y una fecha válida.";
                txtMontoMulta.Foreground = (System.Windows.Media.Brush)FindResource("AccentSuccessBrush");
                return;
            }

            DateTime fechaDev = dpFechaDevolucion.SelectedDate.Value;
            DateTime fechaLim = _prestamoSeleccionado.FechaLimite;

            decimal multa = _prestamoNegocio.CalcularMulta(fechaLim, fechaDev);
            txtMontoMulta.Text = $"S/ {multa:N2}";

            if (multa > 0)
            {
                int dias = (int)(fechaDev.Date - fechaLim.Date).TotalDays;
                txtDetalleMora.Text = $"⚠️ Retraso detectado: {dias} día(s) de morosidad (Mora: S/ 1.50 por día).";
                txtMontoMulta.Foreground = (System.Windows.Media.Brush)FindResource("AccentDangerBrush");
            }
            else
            {
                txtDetalleMora.Text = "✅ Devolución dentro del plazo establecido. Sin penalización.";
                txtMontoMulta.Foreground = (System.Windows.Media.Brush)FindResource("AccentSuccessBrush");
            }
        }

        private async void btnConfirmarDevolucion_Click(object sender, RoutedEventArgs e)
        {
            if (_prestamoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un préstamo activo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cboLibroDetalle.SelectedValue == null)
            {
                MessageBox.Show("Seleccione el libro que se va a devolver.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpFechaDevolucion.SelectedDate == null)
            {
                MessageBox.Show("Seleccione la fecha de devolución.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int prestamoId = _prestamoSeleccionado.PrestamoId;
            int libroId = (int)cboLibroDetalle.SelectedValue;
            DateTime fechaDevolucion = dpFechaDevolucion.SelectedDate.Value;

            try
            {
                decimal multaCobrada = await _prestamoNegocio.RegistrarDevolucionAsync(prestamoId, libroId, fechaDevolucion);

                string msg = $"¡Devolución registrada exitosamente!\n\nEjemplar reingresado al stock.";
                if (multaCobrada > 0)
                {
                    msg += $"\nMulta generada por retraso: S/ {multaCobrada:N2}";
                }

                MessageBox.Show(msg, "Devolución Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                await CargarPrestamosActivosAsync();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la devolución: {ex.Message}", "Error de Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetearFormulario()
        {
            _prestamoSeleccionado = null;
            txtFechaPrestamo.Text = "--";
            txtFechaLimite.Text = "--";
            txtMontoMulta.Text = "S/ 0.00";
            txtDetalleMora.Text = "Sin días de morosidad detectados.";
            txtMontoMulta.Foreground = (System.Windows.Media.Brush)FindResource("AccentSuccessBrush");
        }
    }
}
