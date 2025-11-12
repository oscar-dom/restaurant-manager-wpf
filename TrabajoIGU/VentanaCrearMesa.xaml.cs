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
    public partial class VentanaCrearMesa : Window
    {
        private Sesion sesion;

        public VentanaCrearMesa(Sesion sesion)
        {
            InitializeComponent();
            this.sesion = sesion;
        }

        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtCapacidad.Text, out int capacidad) || capacidad <= 0)
            {
                MessageBox.Show("Introduce una capacidad válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtPosicion.Text, out int posicion) || posicion < 1 || posicion > 12)
            {
                MessageBox.Show("La posición debe estar entre 1 y 12.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int fila = (posicion - 1) / Sesion.Columnas;
            int col = (posicion - 1) % Sesion.Columnas;

            if (sesion.Disposicion[fila, col] != null)
            {
                MessageBox.Show("Esa posición ya está ocupada.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int nuevoId = sesion.Mesas.Count > 0 ? sesion.Mesas.Max(m => m.Id) + 1 : 1;
            Mesa nueva = new Mesa(nuevoId, capacidad);

            sesion.Mesas.Add(nueva);
            sesion.Disposicion[fila, col] = nueva;

            txtResultado.Text = $"Mesa {nuevoId} creada correctamente.";

            if (Owner is MainWindow main)
                main.RefrescarRestaurante();
        }
    }
}

