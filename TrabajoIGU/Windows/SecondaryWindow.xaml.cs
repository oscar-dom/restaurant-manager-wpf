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

            // Rellenar la tabla de mesas
            var listaMesas = sesion.Disposicion.Cast<Mesa>().Where(m => m != null).ToList();

            dgMesas.ItemsSource = listaMesas;

            // Seleccionar la mesa actual si existe
            if (mesaSeleccionada != null)
            {
                dgMesas.SelectedItem = listaMesas.FirstOrDefault(m => m.Id == mesaSeleccionada.Id);
            }

            // Actualizar la tabla de platos
            ActualizarPlatos(mesaSeleccionada);
        }

        private void ActualizarPlatos(Mesa mesa)
        {
            if (mesa == null || mesa.Estado != EstadoMesa.OcupadaConComanda)
            {
                dgPlatos.ItemsSource = null;
                return;
            }

            var comandaActual = sesion.ObtenerComandaActual(mesa.Id);
            if (comandaActual == null)
            {
                dgPlatos.ItemsSource = null;
                return;
            }

            var datos = comandaActual.Platos.Select(p => new
            {
                Nombre = p.Key.Nombre,
                Cantidad = p.Value
            }).ToList();

            dgPlatos.ItemsSource = datos;
        }

        private void dgMesas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMesas.SelectedItem is Mesa mesa)
            {
                ActualizarPlatos(mesa);
                MesaSeleccionadaDesdeSecundaria?.Invoke(mesa);
            }
        }
    }
}
