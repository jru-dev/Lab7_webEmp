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
    /// Vista de Mantenimiento de Libros.
    /// Interactúa únicamente con la Capa de Negocio (LibroNegocio y AutorNegocio) y Entidades.
    /// </summary>
    public partial class LibrosView : UserControl
    {
        private readonly LibroNegocio _libroNegocio;
        private readonly AutorNegocio _autorNegocio;
        private Libro? _libroSeleccionado;

        public LibrosView()
        {
            InitializeComponent();
            _libroNegocio = new LibroNegocio();
            _autorNegocio = new AutorNegocio();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarAutoresAsync();
            await CargarLibrosAsync();
        }

        private async Task CargarAutoresAsync()
        {
            try
            {
                var autores = await _autorNegocio.ListarAsync();
                cboAutor.ItemsSource = autores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la lista de autores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarLibrosAsync()
        {
            try
            {
                string filtro = txtBuscar.Text.Trim();
                var libros = string.IsNullOrEmpty(filtro)
                    ? await _libroNegocio.ListarAsync()
                    : await _libroNegocio.BuscarAsync(filtro);

                dgLibros.ItemsSource = libros;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al listar libros: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            await CargarLibrosAsync();
        }

        private async void txtBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await CargarLibrosAsync();
            }
        }

        private void dgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLibros.SelectedItem is Libro libro)
            {
                _libroSeleccionado = libro;
                txtTitulo.Text = libro.Titulo;
                txtISBN.Text = libro.ISBN;
                cboAutor.SelectedValue = libro.AutorId;
                txtEjemplares.Text = libro.Ejemplares.ToString();
            }
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(txtEjemplares.Text, out int ejemplares))
                {
                    MessageBox.Show("Ingrese una cantidad numérica válida de ejemplares.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int autorId = cboAutor.SelectedValue != null ? (int)cboAutor.SelectedValue : 0;

                var libro = new Libro
                {
                    LibroId = _libroSeleccionado != null ? _libroSeleccionado.LibroId : 0,
                    Titulo = txtTitulo.Text.Trim(),
                    ISBN = txtISBN.Text.Trim(),
                    AutorId = autorId,
                    Ejemplares = ejemplares,
                    Activo = true
                };

                await _libroNegocio.GuardarAsync(libro);

                MessageBox.Show("¡Libro guardado exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
                await CargarLibrosAsync();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error no esperado al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_libroSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un libro de la tabla para dar de baja.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Está seguro de dar de baja (eliminación lógica) al libro '{_libroSeleccionado.Titulo}'?",
                "Confirmar Eliminación Lógica",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    await _libroNegocio.EliminarLogicoAsync(_libroSeleccionado.LibroId);
                    MessageBox.Show("El libro fue dado de baja correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarFormulario();
                    await CargarLibrosAsync();
                }
                catch (ReglaNegocioException ex)
                {
                    MessageBox.Show(ex.Message, "Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar libro: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _libroSeleccionado = null;
            dgLibros.SelectedItem = null;
            txtTitulo.Text = string.Empty;
            txtISBN.Text = string.Empty;
            cboAutor.SelectedIndex = -1;
            txtEjemplares.Text = "1";
        }
    }
}
