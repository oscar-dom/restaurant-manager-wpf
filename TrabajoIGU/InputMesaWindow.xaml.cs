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

namespace TrabajoIGU
{
    public partial class InputMesaWindow : Window
    {
        public int CapacidadMaxima { get; private set; }
        private int idMesa;

        public InputMesaWindow(int id)
        {
            InitializeComponent();
            idMesa = id;
            Title = $"Añadir Mesa #{idMesa}";
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtCapacidad.Text, out int cap) && cap >= 2 && cap <= 8)
            {
                CapacidadMaxima = cap;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("La capacidad debe ser un número comrendido entre (2-8).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
