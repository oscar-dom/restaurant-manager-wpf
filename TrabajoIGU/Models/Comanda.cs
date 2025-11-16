using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoIGU.Models
{
    public class Comanda
    {
        public int IdMesa { get; set; }
        public DateTime Fecha { get; set; }
        public Dictionary<Plato, int> Platos { get; set; } // Plato -> cantidad

        public Comanda(int idMesa)
        {
            IdMesa = idMesa;
            Fecha = DateTime.Now;
            Platos = new Dictionary<Plato, int>();
        }

        public void AgregarPlato(Plato plato, int cantidad = 1)
        {
            if (Platos.ContainsKey(plato))
                Platos[plato] += cantidad;
            else
                Platos[plato] = cantidad;
        }

        public int TotalPlatos()
        {
            int total = 0;
            foreach (var p in Platos.Values)
                total += p;
            return total;
        }

        public class PlatoCantidad
        {
            public Plato Plato { get; set; }
            public int Cantidad { get; set; }

            public PlatoCantidad(Plato plato, int cantidad)
            {
                Plato = plato;
                Cantidad = cantidad;
            }
        }


        public void RestarPlato(Plato plato, int cantidad = 1)
        {
            if (!Platos.ContainsKey(plato))
                return;

            Platos[plato] -= cantidad;

            if (Platos[plato] <= 0)
                Platos.Remove(plato);
        }


        public override string ToString()
        {
            return $"Comanda Mesa {IdMesa} - {Platos.Count} platos";
        }
    }
}
