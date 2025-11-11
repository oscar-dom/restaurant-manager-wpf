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
        private Dictionary<Shape, Mesa> mapaMesas = new Dictionary<Shape, Mesa>();

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
                // Creamos un círculo por cada mesa
                Ellipse figura = new Ellipse
                {
                    Width = 60,
                    Height = 60,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Fill = GetColorPorEstado(mesa.Estado),
                    Cursor = Cursors.Hand
                };

                // Guardamos su posición
                Canvas.SetLeft(figura, x);
                Canvas.SetTop(figura, y);

                // Asociamos evento clic
                figura.MouseLeftButtonDown += Mesa_Click;

                // Mostramos su número encima
                var label = new TextBlock
                {
                    Text = mesa.Id.ToString(),
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(label, x + 20);
                Canvas.SetTop(label, y + 18);

                // Añadimos al Canvas y al diccionario
                canvasSala.Children.Add(figura);
                canvasSala.Children.Add(label);
                mapaMesas.Add(figura, mesa);

                // Mover posición para la siguiente mesa
                x += 100;
                if (x > 400)
                {
                    x = 50;
                    y += 100;
                }
            }
        }

        private Brush GetColorPorEstado(EstadoMesa estado)
        {
            switch (estado)
            {
                case EstadoMesa.Libre:
                    return Brushes.LightGreen;
                case EstadoMesa.Reservada:
                    return Brushes.Gold;
                case EstadoMesa.OcupadaSinComanda:
                    return Brushes.Orange;
                case EstadoMesa.OcupadaConComanda:
                    return Brushes.Tomato;
                default:
                    return Brushes.LightGray;
            }
        }

        private void Mesa_Click(object sender, MouseButtonEventArgs e)
        {
            var figura = sender as Shape;
            if (figura != null && mapaMesas.ContainsKey(figura))
            {
                mesaSeleccionada = mapaMesas[figura];
                MostrarDatosMesa();
            }
        }

        private void MostrarDatosMesa()
        {
            if (mesaSeleccionada == null) return;

            txtIdMesa.Text = mesaSeleccionada.Id.ToString();
            txtCapacidad.Text = mesaSeleccionada.CapacidadMaxima.ToString();
            txtEstado.Text = mesaSeleccionada.Estado.ToString();
            txtComensales.Text = mesaSeleccionada.CapacidadActual.ToString();
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
