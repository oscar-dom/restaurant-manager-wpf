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
            Menu = new List<Plato>();
            ComandasHistoricas = new List<Comanda>();
        }

        public void ReiniciarSesion()
        {
            IniciarSesion();
        }

        public void RegistrarComanda(Comanda comanda)
        {
            ComandasHistoricas.Add(comanda);
            var mesa = Mesas.FirstOrDefault(m => m.Id == comanda.IdMesa);
            if (mesa != null)
            {
                mesa.Estado = EstadoMesa.OcupadaConComanda;
            }
        }

        public int TotalPlatosPorMesa(int idMesa)
        {
            return ComandasHistoricas
                .Where(c => c.IdMesa == idMesa)
                .Sum(c => c.TotalPlatos());
        }

        public Dictionary<string, int> PlatosPorCategoriaMesa(int idMesa, CategoriaPlato categoria)
        {
            var resultado = new Dictionary<string, int>();

            var comandas = ComandasHistoricas
                .Where(c => c.IdMesa == idMesa);

            foreach (var comanda in comandas)
            {
                foreach (var kvp in comanda.Platos)
                {
                    if (kvp.Key.Categoria == categoria)
                    {
                        if (!resultado.ContainsKey(kvp.Key.Nombre))
                            resultado[kvp.Key.Nombre] = 0;
                        resultado[kvp.Key.Nombre] += kvp.Value;
                    }
                }
            }
            return resultado;
        }
    }
}

