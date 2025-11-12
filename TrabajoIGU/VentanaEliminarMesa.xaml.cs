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

namespace TrabajoIGU
{
    public partial class VentanaEliminarMesa : Window
    {
        private Sesion sesion;

        public VentanaEliminarMesa(Sesion sesion)
        {
            InitializeComponent();
            this.sesion = sesion;
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show("Introduce un ID válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mesa = sesion.Mesas.FirstOrDefault(m => m.Id == id);
            if (mesa == null)
            {
                MessageBox.Show("No existe ninguna mesa con ese ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Buscar y eliminar de la matriz
            for (int f = 0; f < Sesion.Filas; f++)
            {
                for (int c = 0; c < Sesion.Columnas; c++)
                {
                    if (sesion.Disposicion[f, c] == mesa)
                    {
                        sesion.Disposicion[f, c] = null;
                        break;
                    }
                }
            }

            sesion.Mesas.Remove(mesa);
            txtResultado.Text = $"Mesa {id} eliminada correctamente.";

            if (Owner is MainWindow main)
                main.RefrescarRestaurante();
        }
    }
}
