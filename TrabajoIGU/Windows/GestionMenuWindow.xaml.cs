using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrabajoIGU.Models;

namespace TrabajoIGU.Windows
{
    public partial class GestionMenuWindow : Window
    {
        private Sesion sesion;

        // Flag para evitar ejecutar los filtros antes de cargar
        private bool inicializando = true;

        public GestionMenuWindow(Sesion sesionActiva)
        {
            InitializeComponent();

            sesion = sesionActiva;

            Loaded += GestionMenuWindow_Loaded;
        }

        private void GestionMenuWindow_Loaded(object sender, RoutedEventArgs e)
        {
            inicializando = false;
            AplicarFiltros();
        }

        // -----------------------------
        // Filtros
        // -----------------------------
        private void Filtro_Changed(object sender, EventArgs e)
        {
            if (inicializando)
                return;

            AplicarFiltros();
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            cbCategoria.SelectedIndex = 0;
        }

        private void AplicarFiltros()
        {
            if (sesion?.Menu == null)
                return;

            var lista = sesion.Menu.ToList();

            // Texto
            string texto = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(texto))
            {
                lista = lista.Where(p =>
                       p.Nombre.ToLower().Contains(texto) ||
                       p.Descripcion.ToLower().Contains(texto)
                ).ToList();
            }

            // Categoría
            var item = cbCategoria.SelectedItem as ComboBoxItem;
            string categoria = item.Content.ToString();

            if (categoria != "Todas")
            {
                if (Enum.TryParse<CategoriaPlato>(categoria, out var cat))
                {
                    lista = lista.Where(p => p.Categoria == cat).ToList();
                }
            }

            lvPlatos.ItemsSource = lista;
        }

        // -----------------------------
        // Añadir Plato
        // -----------------------------
        private void BtnAgregarPlato_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí abriremos la ventana para añadir platos.",
                            "Función en construcción",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
