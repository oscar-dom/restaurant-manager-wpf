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

        public override bool Equals(object obj)
        {
            if (obj is Plato p)
                return Nombre == p.Nombre && Categoria == p.Categoria;
            return false;
        }

        public override int GetHashCode()
        {
            return (Nombre, Categoria).GetHashCode();
        }


        public override string ToString()
        {
            return $"{Nombre} ({Categoria})";
        }
    }
}

