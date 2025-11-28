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

        private int capacidadActual_;
        public int CapacidadActual
        {
            get => capacidadActual_;
            set
            {
                if (capacidadActual_ != value)
                {
                    capacidadActual_ = value;
                    OnPropertyChanged(nameof(CapacidadActual));
                }
            }
        }

        private EstadoMesa estado_;
        public EstadoMesa Estado
        {
            get => estado_;
            set
            {
                if (estado_ != value)
                {
                    estado_ = value;
                    OnPropertyChanged(nameof(Estado));
                }
            }
        }

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

        private Comanda comandaActiva_;
        public Comanda ComandaActiva
        {
            get => comandaActiva_;
            set
            {
                if (!ReferenceEquals(comandaActiva_, value))
                {
                    comandaActiva_ = value;
                    OnPropertyChanged(nameof(ComandaActiva));
                }
            }
        }

        public Mesa(int id, int capacidadMaxima)
        {
            Id = id;
            CapacidadMaxima = capacidadMaxima;
            capacidadActual_ = 0;
            estado_ = EstadoMesa.Libre;
            MesaSeleccionada = false;
            comandaActiva_ = null;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}