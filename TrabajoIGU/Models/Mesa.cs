using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoIGU.Models
{
    public enum EstadoMesa
    {
        Libre,
        Reservada,
        OcupadaSinComanda,
        OcupadaConComanda
    }

    public class Mesa : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public int CapacidadMaxima { get; set; }
        public int CapacidadActual { get; set; }
        public EstadoMesa Estado { get; set; }
        private bool MesaSeleccionada_;
        public bool MesaSeleccionada
        {
            get { return MesaSeleccionada_; }
            set
            {
                if (MesaSeleccionada_ != value)
                {
                    MesaSeleccionada_ = value;
                    OnPropertyChanged(nameof(MesaSeleccionada));
                }
            }
        }
        public Comanda ComandaActiva { get; set; }


        public Mesa(int id, int capacidadMaxima)
        {
            Id = id;
            CapacidadMaxima = capacidadMaxima;
            CapacidadActual = 0;
            Estado = EstadoMesa.Libre;
            MesaSeleccionada = false;
        }
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}