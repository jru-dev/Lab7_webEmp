using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Vista para Registrar Préstamo de uno o varios libros a un socio.
    /// Valida en tiempo real la regla de máximo 3 libros pendientes por socio.
    /// </summary>
    public partial class PrestamosView : UserControl
    {
        private readonly PrestamoNegocio _prestamoNegocio;
        private readonly SocioNegocio _socioNegocio;
        private readonly LibroNegocio _libroNegocio;

        private readonly ObservableCollection<Libro> _librosSeleccionados = new ObservableCollection<Libro>();
        private int _librosPendientesActualesSocio = 0;

        public PrestamosView()
        {
            InitializeComponent();
            _prestamoNegocio = new PrestamoNegocio();
            _socioNegocio = new SocioNegocio();
            _libroNegocio = new LibroNegocio();

            dgLibrosSeleccionados.ItemsSource = _librosSeleccionados;
            dpFechaLimite.SelectedDate = DateTime.Today.AddDays(14); // Por defecto 14 días
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatosInicialesAsync();
        }

        private async Task CargarDatosInicialesAsync()
        {
            try
            {
                var socios = await _socioNegocio.ListarAsync();
                cboSocio.ItemsSource = socios;

                var libros = await _libroNegocio.ListarAsync();
                dgLibrosDisponibles.ItemsSource = libros;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar catálogos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void cboSocio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboSocio.SelectedItem is Socio socio)
            {
                try
                {
                    _librosPendientesActualesSocio = await _socioNegocio.ContarLibrosPendientesAsync(socio.SocioId);
                    int disponiblesParaPrestar = 3 - _librosPendientesActualesSocio;

                    if (disponiblesParaPrestar <= 0)
                    {
                        lblEstadoSocio.Text = $"⚠️ ATENCIÓN: {socio.Nombre} ya tiene {_librosPendientesActualesSocio} libros pendientes (¡Límite alcanzado!).";
                        lblEstadoSocio.Foreground = (System.Windows.Media.Brush)FindResource("AccentDangerBrush");
                    }
                    else
                    {
                        lblEstadoSocio.Text = $"ℹ️ {socio.Nombre} tiene {_librosPendientesActualesSocio} libro(s) pendiente(s). Puede solicitar hasta {disponiblesParaPrestar} libro(s) más.";
                        lblEstadoSocio.Foreground = (System.Windows.Media.Brush)FindResource("AccentSuccessBrush");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al consultar préstamos del socio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                lblEstadoSocio.Text = "Seleccione un socio para verificar sus préstamos pendientes.";
                lblEstadoSocio.Foreground = (System.Windows.Media.Brush)FindResource("AccentWarningBrush");
                _librosPendientesActualesSocio = 0;
            }
        }

        private void btnAgregarLibro_Click(object sender, RoutedEventArgs e)
        {
            if (dgLibrosDisponibles.SelectedItem is Libro libro)
            {
                if (libro.Ejemplares <= 0)
                {
                    MessageBox.Show($"El libro '{libro.Titulo}' no tiene ejemplares disponibles en stock.", "Sin Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_librosSeleccionados.Any(l => l.LibroId == libro.LibroId))
                {
                    MessageBox.Show($"El libro '{libro.Titulo}' ya fue agregado a la lista del préstamo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (_librosPendientesActualesSocio + _librosSeleccionados.Count + 1 > 3)
                {
                    MessageBox.Show($"No se puede agregar más libros: El socio acumula {_librosPendientesActualesSocio} pendientes y el límite máximo es de 3 libros.", "Límite Excedido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _librosSeleccionados.Add(libro);
            }
            else
            {
                MessageBox.Show("Seleccione un libro de la tabla de catálogo disponible.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnQuitarLibro_Click(object sender, RoutedEventArgs e)
        {
            if (dgLibrosSeleccionados.SelectedItem is Libro libro)
            {
                _librosSeleccionados.Remove(libro);
            }
        }

        private async void btnRegistrarPrestamo_Click(object sender, RoutedEventArgs e)
        {
            if (cboSocio.SelectedValue == null)
            {
                MessageBox.Show("Por favor, seleccione un socio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_librosSeleccionados.Count == 0)
            {
                MessageBox.Show("Agregue al menos un libro a la lista del préstamo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpFechaLimite.SelectedDate == null)
            {
                MessageBox.Show("Seleccione una fecha límite para el préstamo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int socioId = (int)cboSocio.SelectedValue;
            var libroIds = _librosSeleccionados.Select(l => l.LibroId).ToList();
            DateTime fechaLimite = dpFechaLimite.SelectedDate.Value;

            try
            {
                int prestamoId = await _prestamoNegocio.RegistrarPrestamoAsync(socioId, libroIds, fechaLimite);

                MessageBox.Show($"¡Préstamo N° {prestamoId} registrado exitosamente!", "Préstamo Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                _librosSeleccionados.Clear();
                await CargarDatosInicialesAsync();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de Negocio Incumplida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la transacción de préstamo: {ex.Message}", "Error de Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
