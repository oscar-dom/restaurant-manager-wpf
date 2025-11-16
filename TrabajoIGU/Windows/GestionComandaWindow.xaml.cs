using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrabajoIGU.Models;

namespace TrabajoIGU.Windows
{
    public partial class GestionComandaWindow : Window
    {
        private Sesion sesion;
        private Mesa mesa;
        private Dictionary<Plato, int> comandaTemp;
        private Dictionary<Plato, int> comandaOriginal;
        private bool cambiosRealizados = false;
        private bool cierreDesdeCancelar = false;
        private bool cierreDesdeGuardar = false;


        public GestionComandaWindow(Sesion sesion, Mesa mesa)
        {
            InitializeComponent();
            this.sesion = sesion;
            this.mesa = mesa;

            CargarComanda();
            CargarMenu();
        }

        private void CargarMenu()
        {
            lvMenu.ItemsSource = sesion.Menu.OrderBy(p => p.Categoria).ToList();
        }

        private void CargarComanda()
        {
            var comanda = sesion.ObtenerComandaActual(mesa.Id);

            if (comanda != null)
                comandaTemp = new Dictionary<Plato, int>(comanda.Platos);
            else
                comandaTemp = new Dictionary<Plato, int>();

            RefrescarComanda();
        }

        private void RefrescarComanda()
        {
            lvComanda.ItemsSource = null;
            lvComanda.ItemsSource = comandaTemp.ToList();
        }



        // =============
        // BOTONES + / -
        // =============

        private void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;
            Plato plato = b.Tag as Plato;
            if (plato == null) return;

            comandaTemp[plato]++;
            cambiosRealizados = true;
            RefrescarComanda();
        }

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            var plato = (sender as Button).Tag as Plato;

            comandaTemp[plato]--;
            cambiosRealizados = true;

            if (comandaTemp[plato] <= 0)
                comandaTemp.Remove(plato);

            RefrescarComanda();
        }

        // ===================
        // DOBLE CLICK EN MENÚ
        // ===================
        private void LvMenu_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var plato = lvMenu.SelectedItem as Plato;
            if (plato == null) return;

            if (!comandaTemp.ContainsKey(plato))
                comandaTemp[plato] = 1;
            else
                comandaTemp[plato]++;

            cambiosRealizados = true;
            RefrescarComanda();
        }

        // ================
        // GUARDAR CAMBIOS
        // ================
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Crear o reemplazar comanda
            var comanda = sesion.ObtenerComandaActual(mesa.Id);
            if (comanda == null)
            {
                comanda = new Comanda(mesa.Id);
                sesion.ComandasHistoricas.Add(comanda);
            }

            comanda.Platos = new Dictionary<Plato, int>(comandaTemp);

            mesa.Estado = EstadoMesa.OcupadaConComanda;
            cierreDesdeGuardar = true;
            Close();
        }

        // ================
        // CANCELAR CAMBIOS
        // ================
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            cierreDesdeCancelar = true;
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // 1. Si viene del botón GUARDAR → NO preguntar
            if (cierreDesdeGuardar)
            {
                return;
            }

            if (ComandasSonIguales())
            {
                cambiosRealizados = false;
            }

            // 2. Si viene del botón CANCELAR → preguntar si hay cambios
            if (cierreDesdeCancelar)
            {
                if (cambiosRealizados)
                {
                    var r = MessageBox.Show(
                        "Hay cambios sin guardar.\n¿Salir sin guardar?",
                        "Cerrar",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (r == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        cierreDesdeCancelar = false;
                        return;
                    }
                }

                return;
            }

            // 3. Si la ventana se cierra con la X del sistema → preguntar
            if (cambiosRealizados)
            {
                var r = MessageBox.Show(
                    "Hay cambios sin guardar.\n¿Salir sin guardar?",
                    "Cerrar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (r == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private bool ComandasSonIguales()
        {
            CargarComandaOriginal();
            if (comandaTemp.Count != comandaOriginal.Count)
                return false;

            foreach (var kvp in comandaTemp)
            {
                if (!comandaOriginal.ContainsKey(kvp.Key))
                    return false;

                if (comandaOriginal[kvp.Key] != kvp.Value)
                    return false;
            }

            return true;
        }

        private void CargarComandaOriginal()
        {
            var comanda = sesion.ObtenerComandaActual(mesa.Id);
            comandaOriginal = comanda != null
                ? new Dictionary<Plato, int>(comanda.Platos)
                : new Dictionary<Plato, int>();

            // Marcamos cambios cuando se modifique comandaTemp
        }

        private void AplicarFiltros()
        {
            if (sesion == null || sesion.Menu == null)
                return;

            var lista = sesion.Menu.ToList();

            // Filtro texto
            string t = txtBuscar.Text.Trim().ToLower();
            if (t != "")
            {
                lista = lista.Where(p =>
                    p.Nombre.ToLower().Contains(t) ||
                    (p.Descripcion ?? "").ToLower().Contains(t)
                ).ToList();
            }

            // Filtro categoría
            if (cbCategoria.SelectedItem is ComboBoxItem item)
            {
                string cat = item.Content.ToString();
                if (cat != "Todas")
                {
                    CategoriaPlato c;
                    if (Enum.TryParse(cat, out c))
                        lista = lista.Where(p => p.Categoria == c).ToList();
                }
            }

            lvMenu.ItemsSource = lista;
        }

        private void Filtro_Changed(object sender, RoutedEventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            cbCategoria.SelectedIndex = 0;
            AplicarFiltros();
        }
    }
}
