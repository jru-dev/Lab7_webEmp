using System.Windows;
using Biblioteca.WPF.Views;

namespace Biblioteca.WPF
{
    /// <summary>
    /// Ventana principal de la aplicación con panel de navegación lateral.
    /// Switch de vistas sin referenciar a la Capa de Datos.
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibrosView? _librosView;
        private SociosView? _sociosView;
        private PrestamosView? _prestamosView;
        private DevolucionesView? _devolucionesView;
        private ReportesView? _reportesView;

        public MainWindow()
        {
            InitializeComponent();
            NavegarALibros();
        }

        private void btnNavLibros_Checked(object sender, RoutedEventArgs e)
        {
            NavegarALibros();
        }

        private void btnNavSocios_Checked(object sender, RoutedEventArgs e)
        {
            _sociosView ??= new SociosView();
            MainContentArea.Content = _sociosView;
        }

        private void btnNavPrestamos_Checked(object sender, RoutedEventArgs e)
        {
            _prestamosView ??= new PrestamosView();
            MainContentArea.Content = _prestamosView;
        }

        private void btnNavDevoluciones_Checked(object sender, RoutedEventArgs e)
        {
            _devolucionesView ??= new DevolucionesView();
            MainContentArea.Content = _devolucionesView;
        }

        private void btnNavReportes_Checked(object sender, RoutedEventArgs e)
        {
            _reportesView ??= new ReportesView();
            MainContentArea.Content = _reportesView;
        }

        private void NavegarALibros()
        {
            _librosView ??= new LibrosView();
            if (MainContentArea != null)
                MainContentArea.Content = _librosView;
        }
    }
}
