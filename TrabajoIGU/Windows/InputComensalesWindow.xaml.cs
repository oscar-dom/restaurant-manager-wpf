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

    public partial class InputComensalesWindow : Window
    {
        private int capacidadMaxima;
        private EstadoMesa estadoMesa;

        public int NumComensales { get; private set; }

        public InputComensalesWindow(int valorActual, int maxima, EstadoMesa estado)
        {
            InitializeComponent();

            capacidadMaxima = maxima;
            estadoMesa = estado;

            // Configura el control IntegerUpDown (numComensales viene del XAML)
            numComensales.Minimum = 0;
            numComensales.Maximum = maxima;
            numComensales.Value = valorActual;
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            int valor = numComensales.Value ?? 0;

            if (valor < 0 || valor > capacidadMaxima)
            {
                MessageBox.Show($"El número de comensales debe estar entre 0 y {capacidadMaxima}.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (estadoMesa == EstadoMesa.OcupadaConComanda && valor == 0)
            {
                var respuesta = MessageBox.Show(
                    "Todos los comensales abandonarán la mesa.\n" +
                    "Se guardará la comanda y la mesa quedará libre.\n\n¿Deseas continuar?",
                    "Advertencia",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.No)
                    return;
            }

            NumComensales = valor;
            DialogResult = true;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == true) return;
            DialogResult = false;
        }
    }
}
