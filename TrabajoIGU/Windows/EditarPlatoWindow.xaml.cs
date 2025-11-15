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
    public partial class EditarPlatoWindow : Window
    {
        private Plato platoOriginal;

        public bool Eliminado { get; private set; } = false;
        public Plato PlatoModificado { get; private set; }

        public EditarPlatoWindow(Plato plato)
        {
            InitializeComponent();

            platoOriginal = plato;

            // Rellenar campos
            txtNombre.Text = plato.Nombre;
            cbCategoria.SelectedIndex = (int)plato.Categoria;
            txtDescripcion.Text = plato.Descripcion;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                $"¿Seguro que deseas eliminar el plato '{platoOriginal.Nombre}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                Eliminado = true;
                DialogResult = true;
                Close();
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El nombre del plato es obligatorio.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            CategoriaPlato categoria = (CategoriaPlato)cbCategoria.SelectedIndex;

            PlatoModificado = new Plato(
                nombre,
                categoria,
                txtDescripcion.Text.Trim()
            );

            DialogResult = true;
            Close();
        }
    }
}

