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
using System.ComponentModel;

namespace TrabajoIGU.Windows
{
    public partial class SecondaryWindow : Window
    {
        public event Action<Mesa> MesaSeleccionadaDesdeSecundaria;

        private Sesion sesion;

        private Mesa mesa;

        private readonly HashSet<Mesa> mesasSuscritas = new HashSet<Mesa>();

        private Comanda comandaSuscrita;

        public SecondaryWindow(Sesion sesionActiva, Mesa mesaSeleccionada)
        {
            InitializeComponent();
            this.sesion = sesionActiva;
            this.mesa = mesaSeleccionada;
            ActualizarVista();
        }

        public void ActualizarVista()
        {

            SuscribirMesasSesion();
            SuscribirComandaDeMesaActual();

            var listaMesas = sesion.Disposicion.Cast<Mesa>().Where(m => m != null).ToList();

            dgMesas.ItemsSource = listaMesas;

            if (mesa != null)
            {
                dgMesas.SelectedItem = listaMesas.FirstOrDefault(m => m.Id == mesa.Id);
                dgMesas.UpdateLayout();
                dgMesas.Focus();
            }
            else
            {
                dgMesas.SelectedItem = null;
                dgMesas.UpdateLayout();
            }

            ActualizarPlatos();
        }

        private void SuscribirMesasSesion()
        {
            // Desuscribimos primero
            foreach (var m in mesasSuscritas.ToList())
            {
                m.PropertyChanged -= MesaSeleccionadaOnChanged;
            }
            mesasSuscritas.Clear();

            if (sesion?.Mesas == null) return;

            foreach (var m in sesion.Mesas)
            {
                // evitamos suscripciones duplicadas
                m.PropertyChanged -= MesaSeleccionadaOnChanged;
                m.PropertyChanged += MesaSeleccionadaOnChanged;
                mesasSuscritas.Add(m);
            }
        }

        private void SuscribirComandaDeMesaActual()
        {
            // desuscribir la comanda previa
            if (comandaSuscrita != null)
            {   
                comandaSuscrita.PropertyChanged -= ComandaOnChanged;
                comandaSuscrita = null;
            }

            if (mesa?.ComandaActiva != null)
            {
                comandaSuscrita = mesa.ComandaActiva;
                comandaSuscrita.PropertyChanged -= ComandaOnChanged;
                comandaSuscrita.PropertyChanged += ComandaOnChanged;
            }
        }

        private void ActualizarPlatos()
        {
            if (mesa == null || mesa.ComandaActiva == null)
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
            if (dgMesas.SelectedItem is Mesa mesaSeleccionada)
            {
                sesion.SeleccionarMesa(mesaSeleccionada);
                mesa = mesaSeleccionada;
                ActualizarVista();
            }
        }

        private void dgMesas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Comprobamos si el click NO fue sobre una fila
            var row = ItemsControl.ContainerFromElement(dgMesas, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row == null)
            {
                dgMesas.SelectedItem = null;
                sesion.SeleccionarMesa(null);
                mesa = null;
                ActualizarVista();
            }
        }

        private void ComandaOnChanged(object sender, PropertyChangedEventArgs e)
        {
            ActualizarPlatos();
        }

        private void MesaSeleccionadaOnChanged(object sender, PropertyChangedEventArgs e)
        {
            // solo reaccionamos si cambia la propiedad MesaSeleccionada o ComandaActiva
            if (e.PropertyName != nameof(Mesa.MesaSeleccionada) && e.PropertyName != nameof(Mesa.ComandaActiva))
                return;

            var nueva = sender as Mesa;
            if (nueva == null) return;

            if (e.PropertyName == nameof(Mesa.ComandaActiva))
            {
                // si la comanda de esta mesa ha cambiado y es la mesa actual, re-suscribimos
                if (mesa != null && mesa.Id == nueva.Id)
                {
                    ActualizarVista();
                }
                return;
            }

            // si la mesa se ha marcado como seleccionada, la usamos; si se ha desmarcado y era la actual, la limpiamos
            if (nueva.MesaSeleccionada)
            {
                mesa = nueva;
                ActualizarVista();
            }
            else
            {
                if (mesa != null && mesa.Id == nueva.Id)
                {
                    mesa = null;
                    ActualizarVista();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // limpiar suscripciones a mesas
            foreach (var m in mesasSuscritas.ToList())
            {
                m.PropertyChanged -= MesaSeleccionadaOnChanged;
            }
            mesasSuscritas.Clear();

            // limpiar suscripción a comanda
            if (comandaSuscrita != null)
            {
                comandaSuscrita.PropertyChanged -= ComandaOnChanged;
                comandaSuscrita = null;
            }
        }
    }
}
