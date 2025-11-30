using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TrabajoIGU.Models
{
    public class Sesion : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public const int Filas = 4;
        public const int Columnas = 3;
        public Mesa[,] Disposicion_;  // matriz 4x3
        public Mesa[,] Disposicion
        {
            get => Disposicion_;
            set
            {
                if (Disposicion_ != value)
                {
                    Disposicion_ = value;
                    OnPropertyChanged(nameof(Disposicion));
                }
            }
        }


        public List<Mesa> Mesas { get; set; }
        public List<Plato> Menu { get; set; }
        public List<Comanda> ComandasHistoricas { get; set; }

        public Sesion()
        {
            Disposicion = new Mesa[Filas, Columnas];
            Mesas = new List<Mesa>();
            Menu = new List<Plato>();
            ComandasHistoricas = new List<Comanda>();
        }

        public void IniciarSesion()
        {
            Disposicion = new Mesa[Filas, Columnas];
            Mesas = new List<Mesa>();
            //Menu = new List<Plato>(); NO TIENE SENTIDO BORRAR EL MENU A MENOS QUE FUERA DEL DIA Y TENGA QUE CAMBIAR
            ComandasHistoricas = new List<Comanda>();
        }

        public Comanda ObtenerComandaActual(int idMesa)
        {
            var mesa = Mesas.FirstOrDefault(m => m.Id == idMesa);
            return mesa?.ComandaActiva;
        }

        public void SeleccionarMesa(Mesa mesaSeleccionada)
        {
            if (Mesas == null || Mesas.Count == 0)
                return;

            if (mesaSeleccionada == null)
            {
                foreach (var m in Mesas)
                    m.MesaSeleccionada = false;
                return;
            }

            var objetivo = Mesas.FirstOrDefault(m => ReferenceEquals(m, mesaSeleccionada) || m.Id == mesaSeleccionada.Id);

            if (objetivo == null)
            {
                foreach (var m in Mesas)
                    m.MesaSeleccionada = false;
                return;
            }

            foreach (var m in Mesas)
                m.MesaSeleccionada = (m == objetivo);
        }

        public void AñadirMesa(Mesa nuevaMesa, int fila, int columna)
        {
            if (fila < 0 || fila >= Filas || columna < 0 || columna >= Columnas)
                throw new ArgumentOutOfRangeException("Fila o columna fuera de rango.");
            Disposicion[fila, columna] = nuevaMesa;
            Mesas.Add(nuevaMesa);
            OnPropertyChanged(nameof(Disposicion));
        }

        public void EliminarMesa(Mesa mesaAEliminar)
        {
            if (mesaAEliminar == null)
                return;
            for (int i = 0; i < Filas; i++)
            {
                for (int j = 0; j < Columnas; j++)
                {
                    if (ReferenceEquals(Disposicion[i, j], mesaAEliminar))
                    {
                        Disposicion[i, j] = null;
                    }
                }
            }
            Mesas.RemoveAll(m => m.Id == mesaAEliminar.Id);
            OnPropertyChanged(nameof(Disposicion));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

