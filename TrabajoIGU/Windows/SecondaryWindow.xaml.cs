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
using System.Windows.Shapes;
using TrabajoIGU.Models;

namespace TrabajoIGU.Windows
{
    public partial class SecondaryWindow : Window
    {
        public event Action<Mesa> MesaSeleccionadaDesdeSecundaria;

        private Sesion sesion;

        public SecondaryWindow(Sesion sesionActiva)
        {
            InitializeComponent();
            sesion = sesionActiva;
        }

        public void ActualizarVista(Sesion sesionActiva, Mesa mesaSeleccionada)
        {
            sesion = sesionActiva;

            var listaMesas = sesion.Disposicion.Cast<Mesa>().Where(m => m != null).ToList();

            dgMesas.ItemsSource = listaMesas;

            if (mesaSeleccionada != null)
            {
                dgMesas.SelectedItem = listaMesas.FirstOrDefault(m => m.Id == mesaSeleccionada.Id);
                dgMesas.UpdateLayout(); 
                dgMesas.Focus();
            }
            else
            {
                dgMesas.SelectedItem = null;
                dgMesas.UpdateLayout();
            }

            ActualizarPlatos(mesaSeleccionada);
        }

        private void ActualizarPlatos(Mesa mesa)
        {
            if (mesa == null || mesa.Estado != EstadoMesa.OcupadaConComanda)
            {
                dgPlatos.ItemsSource = null;
                dgPlatos.Visibility = Visibility.Collapsed;
                txtSinComanda.Visibility = Visibility.Visible;
                return;
            }

            var comandaActual = sesion.ObtenerComandaActual(mesa.Id);

            if (comandaActual == null)
            {
                dgPlatos.ItemsSource = null;
                dgPlatos.Visibility = Visibility.Collapsed;
                txtSinComanda.Visibility = Visibility.Visible;
                return;
            }

            var datos = comandaActual.Platos.Select(p => new
            {
                Categoria = p.Key.Categoria.ToString(),
                Nombre = p.Key.Nombre,
                Descripcion = p.Key.Descripcion,
                Cantidad = p.Value
            }).ToList();

            dgPlatos.ItemsSource = datos;

            dgPlatos.Visibility = Visibility.Visible;
            txtSinComanda.Visibility = Visibility.Collapsed;
        }

        private void dgMesas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMesas.SelectedItem is Mesa mesa)
            {
                ActualizarPlatos(mesa);
                MesaSeleccionadaDesdeSecundaria?.Invoke(mesa);
            }
        }
        private void dgMesas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Comprobamos si el click NO fue sobre una fila
            var row = ItemsControl.ContainerFromElement(dgMesas, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row == null)
            {
                dgMesas.SelectedItem = null;

                dgPlatos.ItemsSource = null;
                txtSinComanda.Visibility = Visibility.Visible;
                dgPlatos.Visibility = Visibility.Hidden;

                MesaSeleccionadaDesdeSecundaria?.Invoke(null);
            }
        }
    }
}
