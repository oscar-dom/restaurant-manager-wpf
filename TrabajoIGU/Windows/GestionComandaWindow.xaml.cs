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
            txtComandaMesa.Text = $"Comanda de la mesa {mesa.Id}";
            CargarComanda();
            CargarMenu();
        }

        private void CargarMenu()
        {
            lvMenu.ItemsSource = sesion.Menu.OrderBy(p => p.Categoria).ToList();
        }

        private void CargarComanda()
        {
            if (mesa.ComandaActiva != null)
                comandaTemp = new Dictionary<Plato, int>(mesa.ComandaActiva.Platos);
            else
                comandaTemp = new Dictionary<Plato, int>();

            comandaOriginal = new Dictionary<Plato, int>(comandaTemp);

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
            var plato = (sender as Button)?.Tag as Plato;
            if (plato == null) return;

            if (!comandaTemp.ContainsKey(plato))
                comandaTemp[plato] = 1;
            else
                comandaTemp[plato]++;

            cambiosRealizados = true;
            RefrescarComanda();
        }

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            var plato = (sender as Button)?.Tag as Plato;
            if (plato == null) return;

            comandaTemp[plato]--;

            if (comandaTemp[plato] <= 0)
                comandaTemp.Remove(plato);

            cambiosRealizados = true;
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
            cierreDesdeGuardar = true;

            bool esNueva = (mesa.ComandaActiva == null);

            // ───────────────────────────────
            // CASO 1 → COMANDA (NUEVA O EXISTENTE) VACÍA
            // ───────────────────────────────
            if (comandaTemp.Count == 0)
            {
                if (esNueva)
                {
                    // Nueva comanda sin platos → no se crea
                    var r = MessageBox.Show(
                        "No ha añadido ningún plato.\nNo se creará la comanda.\n\n¿Desea continuar?",
                        "Comanda vacía",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (r == MessageBoxResult.No)
                    {
                        cierreDesdeGuardar = false;
                        return;
                    }

                    // No se crea nada, no tocamos ComandaActiva ni estados
                    Close();
                    return;
                }
                else
                {
                    // Comanda existente que ha quedado vacía → se elimina
                    var r = MessageBox.Show(
                        "La comanda ha quedado vacía.\nSe eliminará la comanda.\n\n¿Desea continuar?",
                        "Comanda vacía",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (r == MessageBoxResult.No)
                    {
                        cierreDesdeGuardar = false;
                        return;
                    }

                    // Eliminamos la comanda ACTIVA de la mesa
                    mesa.ComandaActiva = null;

                    // No tocamos estados: tú mandas con la lógica de estados
                    Close();
                    return;
                }
            }

            // ───────────────────────────────
            // CASO 2 → HAY PLATOS EN comandaTemp
            // ───────────────────────────────

            // Si es nueva, crear la comanda activa ahora
            if (esNueva)
            {
                mesa.ComandaActiva = new Comanda(mesa.Id);
            }

            // Actualizar los platos de la comanda activa
            mesa.ComandaActiva.Platos = new Dictionary<Plato, int>(comandaTemp);

            // Tampoco tocamos estados aquí: los gestionas tú en MainWindow
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

            if (cierreDesdeGuardar)
                return;

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
                }
            }
        }

        private bool ComandasSonIguales()
        {
            if (mesa.ComandaActiva == null && comandaTemp.Count == 0)
                return true;

            if (mesa.ComandaActiva == null)
                return false;

            if (mesa.ComandaActiva.Platos.Count != comandaTemp.Count)
                return false;

            foreach (var kv in mesa.ComandaActiva.Platos)
            {
                if (!comandaTemp.ContainsKey(kv.Key))
                    return false;

                if (comandaTemp[kv.Key] != kv.Value)
                    return false;
            }

            return true;
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
