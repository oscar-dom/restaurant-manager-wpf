using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrabajoIGU.Models;

namespace TrabajoIGU.Windows
{
    public partial class GestionComandaWindow : Window
    {
        private Sesion sesion;
        private Mesa mesa;

        private Dictionary<Plato, int> comandaOriginal;

        private bool cambiosRealizados = false;
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

        // ======================
        // CARGA DE MENÚ Y COMANDA
        // ======================

        private void CargarMenu()
        {
            lvMenu.ItemsSource = sesion.Menu.OrderBy(p => p.Categoria).ToList();
        }

        private void CargarComanda()
        {
            // Guardamos copia original
            comandaOriginal = new Dictionary<Plato, int>(mesa.ComandaActiva.Platos);

            RefrescarComanda();
        }

        private void RefrescarComanda()
        {
            lvComanda.ItemsSource = null;
            lvComanda.ItemsSource = mesa.ComandaActiva.Platos.ToList();
        }


        // =============
        // BOTONES + / -
        // =============

        private void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            var plato = (sender as Button)?.Tag as Plato;
            
            mesa.ComandaActiva.AñadirPlato(plato);

            cambiosRealizados = true;
            RefrescarComanda();
        }

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            var plato = (sender as Button)?.Tag as Plato;

            mesa.ComandaActiva.QuitarPlato(plato);

            cambiosRealizados = true;
            RefrescarComanda();
        }

        // ===================
        // DOBLE CLICK EN MENÚ
        // ===================

        private void LvMenu_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var plato = lvMenu.SelectedItem as Plato;

            mesa.ComandaActiva.AñadirPlato(plato);

            cambiosRealizados = true;
            RefrescarComanda();
        }


        // =============
        // GUARDAR
        // =============

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            cierreDesdeGuardar = true;
            Close();
        }


        // =============
        // CANCELAR
        // =============

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        // ===============================
        // GESTIÓN DE CIERRE DE VENTANA
        // ===============================

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Si se pulsó Guardar → no preguntar
            if (cierreDesdeGuardar) {
                if (mesa.ComandaActiva.Platos.Count == 0)
                {
                    var res = MessageBox.Show(
                    "La comanda ha quedado vacía y por ello se borrará.\n¿Desea proceder?",
                    "Descartar cambios",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                    if (res == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                return;
            }
                    

            // Detectar cambios
            cambiosRealizados = !ComandasSonIguales();

            if (!cambiosRealizados)
                return;

            // Preguntamos si de verdad quiere descartar cambios
            var r = MessageBox.Show(
                "Hay cambios sin guardar.\n¿Desea descartar los cambios?",
                "Descartar cambios",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (r == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }

            // Restauramos comanda original
            mesa.ComandaActiva.Platos = comandaOriginal;
        }


        // ======================
        // COMPARACIÓN DE COMANDAS
        // ======================

        private bool ComandasSonIguales()
        {
            var actual = mesa.ComandaActiva.Platos;

            if (actual.Count != comandaOriginal.Count)
                return false;

            foreach (var kv in comandaOriginal)
            {
                if (!actual.ContainsKey(kv.Key)) return false;
                if (actual[kv.Key] != kv.Value) return false;
            }

            return true;
        }


        // ======================
        // FILTROS
        // ======================

        private void AplicarFiltros()
        {
            if (sesion == null || sesion.Menu == null)
                return;

            var lista = sesion.Menu.ToList();

            // Texto
            string t = txtBuscar.Text.Trim().ToLower();
            if (t != "")
            {
                lista = lista.Where(p =>
                    p.Nombre.ToLower().Contains(t) ||
                    (p.Descripcion ?? "").ToLower().Contains(t)
                ).ToList();
            }

            // Categoría
            if (cbCategoria.SelectedItem is ComboBoxItem item)
            {
                string cat = item.Content.ToString();
                if (cat != "Todas")
                {
                    if (Enum.TryParse(cat, out CategoriaPlato c))
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
