using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoIGU.Models
{
    public class Sesion
    {
        public const int Filas = 4;
        public const int Columnas = 3;
        public Mesa[,] Disposicion { get; set; }  // matriz 4x3

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

    }
}

