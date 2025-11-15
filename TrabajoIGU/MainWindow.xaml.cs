using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrabajoIGU.Data;
using TrabajoIGU.Models;
using TrabajoIGU.Windows;

namespace TrabajoIGU
{
    public partial class MainWindow : Window
    {
        private Sesion sesion;
        private Mesa mesaSeleccionada;
        private Dictionary<UIElement, Mesa> mapaMesas = new Dictionary<UIElement, Mesa>();
        private SecondaryWindow secondaryWindow;
        private bool inicializandoMenu = true;

        public MainWindow()
        {
            InitializeComponent();
            sesion = SeedData.CrearSesionDePrueba();
            DibujarMesas();
            Loaded += MainWindow_Loaded;

        }

        //INTERFAZ
        #region Actualización de interfaz
        private void DibujarMesas()
        {
            canvasSala.Children.Clear();
            mapaMesas.Clear();

            double espacioX = 160;
            double espacioY = 130;
            double inicioX = 50;
            double inicioY = 50;

            for (int fila = 0; fila < Sesion.Filas; fila++)
            {
                for (int col = 0; col < Sesion.Columnas; col++)
                {
                    Mesa mesa = sesion.Disposicion[fila, col];
                    double x = inicioX + col * espacioX;
                    double y = inicioY + fila * espacioY;

                    if (mesa == null)
                    {
                        // Dibuja marco transparente con número de celda
                        int numeroCelda = fila * Sesion.Columnas + col + 1;

                        Border celdaVacia = new Border
                        {
                            Width = 80,
                            Height = 80,
                            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                            BorderThickness = new Thickness(2),
                            Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                            Cursor = Cursors.Hand,
                            Tag = numeroCelda
                        };

                        Canvas.SetLeft(celdaVacia, x);
                        Canvas.SetTop(celdaVacia, y);

                        celdaVacia.MouseLeftButtonDown += CeldaVacia_LeftClick;
                        celdaVacia.MouseRightButtonDown += CeldaVacia_RightClick;

                        canvasSala.Children.Add(celdaVacia);

                        // Etiqueta con el número de celda
                        TextBlock lbl = new TextBlock
                        {
                            Text = numeroCelda.ToString(),
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Black,
                            TextAlignment = TextAlignment.Center,
                            Opacity = 0.6,
                            IsHitTestVisible = false
                        };
                        Canvas.SetLeft(lbl, x + 33);
                        Canvas.SetTop(lbl, y + 30);
                        canvasSala.Children.Add(lbl);
                        continue;

                    }
                    else
                    {

                        // Imagen según el estado de la mesa
                        Image imgMesa = new Image
                        {
                            Width = 80,
                            Height = 80,
                            Source = new BitmapImage(new Uri(GetRutaImagenPorEstado(mesa.Estado), UriKind.Relative)),
                            Cursor = Cursors.Hand,
                            Tag = mesa // para acceder fácilmente luego
                        };

                        // Creamos el borde que contendrá la imagen
                        Border borde = new Border
                        {
                            BorderThickness = new Thickness(3),
                            BorderBrush = Brushes.Black,
                            Child = imgMesa,
                        };

                        // Asignamos posición
                        Canvas.SetLeft(borde, x);
                        Canvas.SetTop(borde, y);

                        // Evento clic para seleccionar
                        borde.MouseLeftButtonDown += Mesa_LeftClick;
                        borde.MouseRightButtonDown += Mesa_RightClick;

                        // Añadimos al Canvas
                        canvasSala.Children.Add(borde);
                        mapaMesas.Add(borde, mesa); // el diccionario guarda el borde como clave

                        // Etiqueta con el número de mesa
                        TextBlock label = new TextBlock
                        {
                            Text = mesa.Id.ToString(),
                            FontWeight = FontWeights.Bold,
                            FontSize = 14,
                            Foreground = Brushes.White,
                            Background = Brushes.Black,
                            TextAlignment = TextAlignment.Center,
                            Width = 20,
                            Opacity = 0.6,
                            IsHitTestVisible = false
                        };
                        Canvas.SetLeft(label, x + 33);
                        Canvas.SetTop(label, y + 60);
                        canvasSala.Children.Add(label);
                    }
                }
            }
            ActualizarSeleccionVisual();
            MostrarDatosMesa();
        }

        private void ActualizarSeleccionVisual()
        {
            foreach (var kvp in mapaMesas)
            {
                var borde = kvp.Key as Border;
                if (borde == null) continue;

                if (mesaSeleccionada != null && kvp.Value.Id == mesaSeleccionada.Id)
                {
                    borde.BorderBrush = Brushes.Red;
                }
                else
                {
                    borde.BorderBrush = Brushes.Black;
                }
            }
        }

        private void MostrarDatosMesa()
        {
            int activas = sesion.Disposicion.Cast<Mesa>().Count(m => m != null);
            txtResumenRestaurante.Text = $"Mesas activas: {activas} / {Sesion.Filas * Sesion.Columnas}";

            int personasActuales = sesion.Disposicion.Cast<Mesa>().Where(m => m != null).Sum(m => m.CapacidadActual);
            int aforoMaximo = sesion.Disposicion.Cast<Mesa>().Where(m => m != null).Sum(m => m.CapacidadMaxima); txtAforoRestaurante.Text = $"Aforo: {personasActuales} / {aforoMaximo}";

            if (mesaSeleccionada == null)
            {
                LimpiarPanel();
                return;
            }

            txtIdMesa.Text = mesaSeleccionada.Id.ToString();
            txtCapacidad.Text = mesaSeleccionada.CapacidadMaxima.ToString();
            txtEstado.Text = mesaSeleccionada.Estado.ToString();
            txtComensales.Text = mesaSeleccionada.CapacidadActual.ToString();

            if (mesaSeleccionada.Estado == EstadoMesa.OcupadaConComanda)
            {
                var comandaActual = sesion.ObtenerComandaActual(mesaSeleccionada.Id);
                int totalPlatos = comandaActual?.TotalPlatos() ?? 0;
                txtPlatos.Text = totalPlatos.ToString();
            }
            else
            {
                txtPlatos.Text = "0";
            }
            secondaryWindow?.ActualizarVista(sesion, mesaSeleccionada);
        }

        private void LimpiarPanel()
        {
            txtIdMesa.Text = "Ninguna mesa seleccionada";
            txtCapacidad.Text = "Ninguna mesa seleccionada";
            txtEstado.Text = "Ninguna mesa seleccionada";
            txtComensales.Text = "Ninguna mesa seleccionada";
            txtPlatos.Text = "Ninguna mesa seleccionada";
        }

        private string GetRutaImagenPorEstado(EstadoMesa estado)
        {
            switch (estado)
            {
                case EstadoMesa.Libre:
                    return "Imagenes/mesaLibre.png";
                case EstadoMesa.Reservada:
                    return "Imagenes/mesaReservada.png";
                case EstadoMesa.OcupadaSinComanda:
                    return "Imagenes/mesaOcupada.png";
                case EstadoMesa.OcupadaConComanda:
                    return "Imagenes/mesaComanda.png";
                default:
                    return "Imagenes/mesaLibre.png";
            }
        }

        private void MesaSeleccionadaDesdeSecundaria(Mesa mesa)
        {
            mesaSeleccionada = mesa;
            MostrarDatosMesa();
            ActualizarSeleccionVisual();
        }
        #endregion

        //CONTROL DE EVENTOS
        #region Controladores de eventos
        private void Mesa_LeftClick(object sender, MouseButtonEventArgs e)
        {
            var borde = sender as Border;
            if (borde == null) return;

            if (mapaMesas.TryGetValue(borde, out var mesa))
            {
                mesaSeleccionada = mesa;
                MostrarDatosMesa();
                ActualizarSeleccionVisual();
            }
        }

        private void Mesa_RightClick(object sender, MouseButtonEventArgs e)
        {
            Mesa_LeftClick(sender, e);

            var borde = sender as Border;
            if (borde == null) return;

            if (!mapaMesas.TryGetValue(borde, out var mesa))
                return;

            // Crear menú contextual dinámicamente
            ContextMenu menu = new ContextMenu();

            // Crear una opción principal “Cambiar estado”
            MenuItem itemCambiar = new MenuItem { Header = "Cambiar estado..." };

            // Añadir subopciones según el estado actual
            foreach (var nuevoEstado in ObtenerEstadosPosibles(mesa.Estado))
            {
                MenuItem subItem = new MenuItem { Header = nuevoEstado.ToString() };
                subItem.Click += (s, ev) =>
                {
                    if (nuevoEstado == EstadoMesa.OcupadaConComanda && mesa.CapacidadActual == 0)
                    {
                        MessageBox.Show(
                            "No puedes abrir una comanda sin comensales.\nPor favor, edita el número de comensales antes de continuar.",
                            "Advertencia",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    mesa.Estado = nuevoEstado;
                    if (mesa.Estado == EstadoMesa.Libre)
                        mesa.CapacidadActual = 0;
                    DibujarMesas();
                };
                itemCambiar.Items.Add(subItem);
            }
            menu.Items.Add(itemCambiar);

            if (mesa.Estado == EstadoMesa.Reservada || mesa.Estado == EstadoMesa.OcupadaSinComanda || mesa.Estado == EstadoMesa.OcupadaConComanda)
            {
                MenuItem itemEditar = new MenuItem { Header = "Editar nº comensales actuales" };
                itemEditar.Click += (s, ev) =>
                {
                    var input = new InputComensalesWindow(mesa.CapacidadActual, mesa.CapacidadMaxima, mesa.Estado)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    if (input.ShowDialog() == true)
                    {
                        mesa.CapacidadActual = input.NumComensales;
                        if (mesa.Estado == EstadoMesa.OcupadaConComanda && mesa.CapacidadActual == 0)
                            mesa.Estado = EstadoMesa.Libre;
                        DibujarMesas();
                    }
                };
                menu.Items.Add(itemEditar);
            }

            MenuItem itemEliminar = new MenuItem { Header = "Eliminar mesa" };
            itemEliminar.Click += (s, ev) =>
            {
                for (int f = 0; f < Sesion.Filas; f++)
                {
                    for (int c = 0; c < Sesion.Columnas; c++)
                    {
                        if (sesion.Disposicion[f, c]?.Id == mesa.Id)
                        {
                            sesion.Disposicion[f, c] = null;
                            DibujarMesas();
                            LimpiarPanel();
                            return;
                        }
                    }
                }
            };
            menu.Items.Add(itemEliminar);

            // Mostrar el menú contextual justo donde se hizo clic
            menu.IsOpen = true;

            e.Handled = true;

        }

        private void CeldaVacia_RightClick(object sender, MouseButtonEventArgs e)
        {
            var celda = sender as Border;
            if (celda == null) return;

            int numeroCelda = (int)celda.Tag;

            ContextMenu menu = new ContextMenu();
            MenuItem itemAdd = new MenuItem { Header = "Añadir mesa" };
            itemAdd.Click += (s, ev) =>
            {
                var input = new InputMesaWindow(numeroCelda)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                if (input.ShowDialog() == true)
                {
                    int fila = (numeroCelda - 1) / Sesion.Columnas;
                    int col = (numeroCelda - 1) % Sesion.Columnas;

                    Mesa nueva = new Mesa(numeroCelda, input.CapacidadMaxima);
                    sesion.Disposicion[fila, col] = nueva;

                    mesaSeleccionada = nueva;

                    DibujarMesas();
                }
            };
            menu.Items.Add(itemAdd);
            menu.Placement = PlacementMode.MousePoint;
            menu.HorizontalOffset = 10;
            menu.IsOpen = true;

            e.Handled = true;
        }

        private void CeldaVacia_LeftClick(object sender, MouseButtonEventArgs e)
        {
            // Deselecciona cualquier mesa y limpia el panel
            mesaSeleccionada = null;
            LimpiarPanel();
            ActualizarSeleccionVisual();
            secondaryWindow?.ActualizarVista(sesion, mesaSeleccionada);
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            sesion.ReiniciarSesion();
            DibujarMesas();
            LimpiarPanel();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Si se hace clic en el canvas vacío, deseleccionar
            if (e.Source == canvasSala)
            {
                mesaSeleccionada = null;
                LimpiarPanel();
                ActualizarSeleccionVisual();
                secondaryWindow?.ActualizarVista(sesion, mesaSeleccionada);
            }
        }

        private void BtnVistaRestaurante_Click(object sender, RoutedEventArgs e)
        {
            // Si ya está abierta, la traemos al frente
            if (secondaryWindow != null && secondaryWindow.IsVisible)
            {
                secondaryWindow.Activate();
                return;
            }

            // Crear nueva instancia
            secondaryWindow = new SecondaryWindow(sesion)
            {
                Owner = this
            };

            // Suscripción a evento de selección desde la ventana secundaria
            secondaryWindow.MesaSeleccionadaDesdeSecundaria += MesaSeleccionadaDesdeSecundaria;

            // Mostrar ventana no modal
            secondaryWindow.Show();

            // Actualizar su vista inicial
            secondaryWindow.ActualizarVista(sesion, mesaSeleccionada);
        }

        private void BtnGestionMenu_Click(object sender, RoutedEventArgs e)
        {
            gridRestaurante.Visibility = Visibility.Collapsed;
            gridGestionMenu.Visibility = Visibility.Visible;

            AplicarFiltrosMenu();
        }

        private void BtnVolverGestionMenu_Click(object sender, RoutedEventArgs e)
        {
            gridGestionMenu.Visibility = Visibility.Collapsed;
            gridRestaurante.Visibility = Visibility.Visible;
        }

        private void BtnAgregarPlatoMenu_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new Windows.NuevoPlatoWindow()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (ventana.ShowDialog() == true)
            {
                // Añadir el plato a la sesión
                sesion.Menu.Add(ventana.PlatoCreado);

                // Refrescar lista
                AplicarFiltrosMenu();
            }
        }

        private void BtnLimpiarFiltrosMenu_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarMenu.Text = "";
            cbCategoriaMenu.SelectedIndex = 0;
        }

        private void LvPlatosMenu_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var platoSeleccionado = lvPlatosMenu.SelectedItem as Plato;
            if (platoSeleccionado == null)
                return;

            var ventana = new Windows.EditarPlatoWindow(platoSeleccionado)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (ventana.ShowDialog() == true)
            {
                if (ventana.Eliminado)
                {
                    sesion.Menu.Remove(platoSeleccionado);
                }
                else if (ventana.PlatoModificado != null)
                {
                    platoSeleccionado.Nombre = ventana.PlatoModificado.Nombre;
                    platoSeleccionado.Categoria = ventana.PlatoModificado.Categoria;
                    platoSeleccionado.Descripcion = ventana.PlatoModificado.Descripcion;
                }

                AplicarFiltrosMenu();
            }
        }

        private void LvPlatosMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Comprobar si el click fue sobre un ListBoxItem
            var item = ItemsControl.ContainerFromElement(lvPlatosMenu, e.OriginalSource as DependencyObject)
                       as ListBoxItem;

            // Si NO fue sobre una tarjeta → deseleccionar
            if (item == null)
            {
                lvPlatosMenu.SelectedItem = null;
                return;
            }

            // Si sí fue sobre una tarjeta → selección normal
        }
        #endregion

        //AUXULIARES
        #region Métodos auxiliares
        private List<EstadoMesa> ObtenerEstadosPosibles(EstadoMesa estadoActual)
        {
            var lista = new List<EstadoMesa>();

            switch (estadoActual)
            {
                case EstadoMesa.Libre:
                    lista.Add(EstadoMesa.Reservada);
                    lista.Add(EstadoMesa.OcupadaSinComanda);
                    break;

                case EstadoMesa.Reservada:
                    lista.Add(EstadoMesa.Libre);
                    lista.Add(EstadoMesa.OcupadaSinComanda);
                    break;

                case EstadoMesa.OcupadaSinComanda:
                    lista.Add(EstadoMesa.Libre);
                    lista.Add(EstadoMesa.OcupadaConComanda);
                    break;

                case EstadoMesa.OcupadaConComanda:
                    lista.Add(EstadoMesa.Libre);
                    break;
            }

            return lista;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            inicializandoMenu = false;
            AplicarFiltrosMenu();
        }

        private void FiltroMenu_Changed(object sender, EventArgs e)
        {
            if (inicializandoMenu)
                return;

            AplicarFiltrosMenu();
        }

        private void AplicarFiltrosMenu()
        {
            if (sesion?.Menu == null)
                return;

            var lista = sesion.Menu.ToList();

            // Texto
            string texto = txtBuscarMenu.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(texto))
            {
                lista = lista.Where(p =>
                       p.Nombre.ToLower().Contains(texto) ||
                       (p.Descripcion ?? "").ToLower().Contains(texto)
                ).ToList();
            }

            // Categoría
            var item = cbCategoriaMenu.SelectedItem as ComboBoxItem;
            string categoria = item?.Content?.ToString();

            if (!string.IsNullOrEmpty(categoria) && categoria != "Todas")
            {
                if (Enum.TryParse<CategoriaPlato>(categoria, out var cat))
                {
                    lista = lista.Where(p => p.Categoria == cat).ToList();
                }
            }

            lvPlatosMenu.ItemsSource = lista;
        }
        #endregion

    }
}
