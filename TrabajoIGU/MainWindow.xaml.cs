using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrabajoIGU.Data;
using TrabajoIGU.Models;

namespace TrabajoIGU
{
    public partial class MainWindow : Window
    {
        private Sesion sesion;
        private Mesa mesaSeleccionada;
        private Dictionary<UIElement, Mesa> mapaMesas = new Dictionary<UIElement, Mesa>();

        public MainWindow()
        {
            InitializeComponent();
            sesion = SeedData.CrearSesionDePrueba();
            DibujarMesas();
        }

        private void DibujarMesas()
        {
            canvasSala.Children.Clear();
            mapaMesas.Clear();

            double espacioX = 120;
            double espacioY = 120;
            double inicioX = 50;
            double inicioY = 50;

            for (int fila = 0; fila < Sesion.Filas; fila++)
            {
                for (int col = 0; col < Sesion.Columnas; col++)
                {
                    Mesa mesa = sesion.Disposicion[fila, col];
                    if (mesa == null) continue; // hueco vacío

                    double x = inicioX + col * espacioX;
                    double y = inicioY + fila * espacioY;

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

                    // Posicionamos el borde (no la imagen)
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
            ActualizarSeleccionVisual();
            ActualizarInfoRestaurante();
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


        private void Mesa_RightClick(object sender, MouseButtonEventArgs e)
        {
            var borde = sender as Border;
            if (borde == null) return;

            if (!mapaMesas.TryGetValue(borde, out var mesa))
                return;

            // Crear menú contextual
            ContextMenu menu = new ContextMenu();

            MenuItem itemCambiar = new MenuItem { Header = "Cambiar estado..." };

            foreach (var nuevoEstado in ObtenerEstadosPosibles(mesa.Estado))
            {
                MenuItem subItem = new MenuItem { Header = nuevoEstado.ToString() };
                subItem.Click += (s, ev) =>
                {
                    mesa.Estado = nuevoEstado;
                    DibujarMesas();
                    MostrarDatosMesa();
                };
                itemCambiar.Items.Add(subItem);
            }

            menu.Items.Add(itemCambiar);

            // Posición cerca del puntero
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            menu.HorizontalOffset = 10;
            menu.IsOpen = true;

            e.Handled = true;
        }

        private void MostrarDatosMesa()
        {
            if (mesaSeleccionada == null) return;

            txtIdMesa.Text = mesaSeleccionada.Id.ToString();
            txtCapacidad.Text = mesaSeleccionada.CapacidadMaxima.ToString();
            txtEstado.Text = mesaSeleccionada.Estado.ToString();
            txtComensales.Text = mesaSeleccionada.CapacidadActual.ToString();
        }

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

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            sesion.ReiniciarSesion();
            DibujarMesas();
            LimpiarPanel();
            ActualizarInfoRestaurante();
        }

        private void LimpiarPanel()
        {
            txtIdMesa.Text = "";
            txtCapacidad.Text = "";
            txtEstado.Text = "";
            txtComensales.Text = "";
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Si se hace clic en el canvas vacío, deseleccionar
            if (e.Source == canvasSala)
            {
                mesaSeleccionada = null;
                LimpiarPanel();
                ActualizarSeleccionVisual();
            }
        }

        private void ActualizarInfoRestaurante()
        {
            int activas = sesion.Mesas.Count;
            txtMesasActivas.Text = activas.ToString();
        }

        private void BtnAbrirVentanaCrear_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaCrearMesa(sesion);
            ventana.Owner = this;
            ventana.Show(); // no modal
        }

        private void BtnAbrirVentanaEliminar_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaEliminarMesa(sesion);
            ventana.Owner = this;
            ventana.Show(); // no modal
        }

        public void RefrescarRestaurante()
        {
            DibujarMesas();
            ActualizarInfoRestaurante();
        }


    }
}
