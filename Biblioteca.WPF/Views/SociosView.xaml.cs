using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.Negocio.Excepciones;

namespace Biblioteca.WPF.Views
{
    /// <summary>
    /// Vista de Mantenimiento de Socios.
    /// Opera mediante SocioNegocio sin invocar referencias de acceso a datos directamente.
    /// </summary>
    public partial class SociosView : UserControl
    {
        private readonly SocioNegocio _socioNegocio;
        private Socio? _socioSeleccionado;

        public SociosView()
        {
            InitializeComponent();
            _socioNegocio = new SocioNegocio();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarSociosAsync();
        }

        private async Task CargarSociosAsync()
        {
            try
            {
                string filtro = txtBuscar.Text.Trim();
                var socios = string.IsNullOrEmpty(filtro)
                    ? await _socioNegocio.ListarAsync()
                    : await _socioNegocio.BuscarAsync(filtro);

                dgSocios.ItemsSource = socios;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la lista de socios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            await CargarSociosAsync();
        }

        private async void txtBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await CargarSociosAsync();
            }
        }

        private void dgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSocios.SelectedItem is Socio socio)
            {
                _socioSeleccionado = socio;
                txtDNI.Text = socio.DNI;
                txtNombre.Text = socio.Nombre;
                txtEmail.Text = socio.Email;
            }
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var socio = new Socio
                {
                    SocioId = _socioSeleccionado != null ? _socioSeleccionado.SocioId : 0,
                    DNI = txtDNI.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Activo = true
                };

                await _socioNegocio.GuardarAsync(socio);

                MessageBox.Show("¡Socio guardado con éxito!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
                await CargarSociosAsync();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error no esperado al guardar socio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_socioSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un socio de la lista para dar de baja.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Está seguro de dar de baja (eliminación lógica) al socio '{_socioSeleccionado.Nombre}'?",
                "Confirmar Baja Lógica",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    await _socioNegocio.EliminarLogicoAsync(_socioSeleccionado.SocioId);
                    MessageBox.Show("El socio ha sido dado de baja exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarFormulario();
                    await CargarSociosAsync();
                }
                catch (ReglaNegocioException ex)
                {
                    MessageBox.Show(ex.Message, "Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al dar de baja al socio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _socioSeleccionado = null;
            dgSocios.SelectedItem = null;
            txtDNI.Text = string.Empty;
            txtNombre.Text = string.Empty;
            txtEmail.Text = string.Empty;
        }
    }
}
