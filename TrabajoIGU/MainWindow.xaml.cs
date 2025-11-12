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

            double x = 50;
            double y = 50;

            foreach (var mesa in sesion.Mesas)
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

                // Asignamos posición
                Canvas.SetLeft(imgMesa, x);
                Canvas.SetTop(imgMesa, y);

                // Evento clic para seleccionar
                imgMesa.MouseLeftButtonDown += Mesa_LeftClick;
                imgMesa.MouseRightButtonDown += Mesa_RightClick;

                // Añadimos al Canvas
                canvasSala.Children.Add(imgMesa);
                mapaMesas.Add(imgMesa, mesa);

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
                Canvas.SetLeft(label, x + 30);
                Canvas.SetTop(label, y + 60);
                canvasSala.Children.Add(label);

                // Cambiar posición para la siguiente mesa
                x += 120;
                if (x > 400)
                {
                    x = 50;
                    y += 120;
                }
            }
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
            var elemento = sender as UIElement;
            if (elemento == null) return;

            if (mapaMesas.TryGetValue(elemento, out var mesa))
            {
                mesaSeleccionada = mesa;
                MostrarDatosMesa();
            }
        }

        private void Mesa_RightClick(object sender, MouseButtonEventArgs e)
        {
            var elemento = sender as UIElement;
            if (elemento == null) return;

            if (!mapaMesas.TryGetValue(elemento, out var mesa))
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
                    mesa.Estado = nuevoEstado;
                    DibujarMesas();
                    MostrarDatosMesa();
                };
                itemCambiar.Items.Add(subItem);
            }

            menu.Items.Add(itemCambiar);

            // Mostrar el menú contextual justo donde se hizo clic
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
            }
        }
    }
}
