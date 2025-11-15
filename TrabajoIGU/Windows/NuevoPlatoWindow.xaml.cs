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
    public partial class NuevoPlatoWindow : Window
    {
        public Plato PlatoCreado { get; private set; }

        public NuevoPlatoWindow()
        {
            InitializeComponent();
            cbCategoria.SelectedIndex = 0; // Valor por defecto
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string descripcion = txtDescripcion.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El nombre del plato es obligatorio.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            CategoriaPlato categoria = (CategoriaPlato)cbCategoria.SelectedIndex;

            PlatoCreado = new Plato(nombre, categoria, descripcion);

            DialogResult = true;
            Close();
        }
    }
}
