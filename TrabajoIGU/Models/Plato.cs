using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoIGU.Models
{
    public enum CategoriaPlato
    {
        Primero,
        Segundo,
        Postre
    }

    public class Plato
    {
        public string Nombre { get; set; }
        public CategoriaPlato Categoria { get; set; }
        public string Descripcion { get; set; }

        public Plato(string nombre, CategoriaPlato categoria, string descripcion = "")
        {
            Nombre = nombre;
            Categoria = categoria;
            Descripcion = descripcion;
        }
    }
}

