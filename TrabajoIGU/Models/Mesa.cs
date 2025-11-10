using System;
using System.Collections.Generic;
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

    public class Mesa
    {
        public int Id { get; set; }
        public int CapacidadMaxima { get; set; }
        public int CapacidadActual { get; set; }
        public EstadoMesa Estado { get; set; }

        public Mesa(int id, int capacidadMaxima)
        {
            Id = id;
            CapacidadMaxima = capacidadMaxima;
            CapacidadActual = 0;
            Estado = EstadoMesa.Libre;
        }

        public override string ToString()
        {
            return $"Mesa {Id} ({Estado})";
        }
    }
}
