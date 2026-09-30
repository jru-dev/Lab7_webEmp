using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Negocio;
using Biblioteca.Negocio.Excepciones;

namespace Biblioteca.WPF.Views
{
    /// <summary>
    /// Vista de Reporte de Préstamos por intervalo de fechas.
    /// Ejecuta de forma asíncrona la consulta multi-tabla.
    /// </summary>
    public partial class ReportesView : UserControl
    {
        private readonly PrestamoNegocio _prestamoNegocio;

        public ReportesView()
        {
            InitializeComponent();
            _prestamoNegocio = new PrestamoNegocio();

            // Rango predeterminado: Mes actual
            DateTime hoy = DateTime.Today;
            dpFechaInicio.SelectedDate = new DateTime(hoy.Year, hoy.Month, 1);
            dpFechaFin.SelectedDate = hoy.AddDays(30);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await GenerarReporteAsync();
        }

        private async void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            await GenerarReporteAsync();
        }

        private async Task GenerarReporteAsync()
        {
            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Por favor, seleccione un rango completo de fechas.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime inicio = dpFechaInicio.SelectedDate.Value;
            DateTime fin = dpFechaFin.SelectedDate.Value;

            try
            {
                var reporte = await _prestamoNegocio.ObtenerReportePorFechasAsync(inicio, fin);
                dgReporte.ItemsSource = reporte;
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
