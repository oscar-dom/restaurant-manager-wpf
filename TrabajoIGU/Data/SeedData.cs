using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrabajoIGU.Models;

namespace TrabajoIGU.Data
{
    public static class SeedData
    {
        public static Sesion CrearSesionDePrueba()
        {
            var sesion = new Sesion();

            // ===== MESAS =====
            sesion.Mesas = new List<Mesa>
            {
                new Mesa(1, 2),
                new Mesa(2, 4),
                new Mesa(3, 4),
                new Mesa(4, 6),
                new Mesa(5, 2)
            };

            // ===== MENÚ =====
            sesion.Menu = new List<Plato>
            {
                // Primeros
                new Plato("Ensalada mixta", CategoriaPlato.Primero, "Lechuga fresca, tomate y cebolla"),
                new Plato("Sopa de verduras", CategoriaPlato.Primero, "Caldo suave con verduras de temporada"),
                new Plato("Gazpacho andaluz", CategoriaPlato.Primero, "Tomate, pepino, ajo y aceite de oliva"),

                // Segundos
                new Plato("Pollo al horno", CategoriaPlato.Segundo, "Pollo marinado asado lentamente"),
                new Plato("Merluza a la plancha", CategoriaPlato.Segundo, "Filete de merluza con aceite y limón"),
                new Plato("Filete con patatas", CategoriaPlato.Segundo, "Carne de ternera acompañada de patatas fritas"),

                // Postres
                new Plato("Tarta de queso", CategoriaPlato.Postre, "Tarta casera cremosa al horno"),
                new Plato("Fruta del tiempo", CategoriaPlato.Postre, "Selección fresca de frutas de temporada"),
                new Plato("Flan casero", CategoriaPlato.Postre, "Flan tradicional con caramelo")
            };

            // ===== COMANDAS =====
            // Mesa 1 (un grupo actual)
            var c1 = new Comanda(1);
            c1.AgregarPlato(sesion.Menu[0], 2); // Ensalada mixta
            c1.AgregarPlato(sesion.Menu[3], 2); // Pollo al horno
            c1.AgregarPlato(sesion.Menu[6], 2); // Tarta de queso
            sesion.RegistrarComanda(c1);

            // Mesa 2 (un grupo actual)
            var c2 = new Comanda(2);
            c2.AgregarPlato(sesion.Menu[1], 3); // Sopa
            c2.AgregarPlato(sesion.Menu[5], 3); // Filete
            c2.AgregarPlato(sesion.Menu[8], 3); // Flan
            sesion.RegistrarComanda(c2);

            // Mesa 3 (dos grupos consecutivos)
            var c3a = new Comanda(3);
            c3a.AgregarPlato(sesion.Menu[0], 2);
            c3a.AgregarPlato(sesion.Menu[3], 2);
            sesion.RegistrarComanda(c3a);

            // Segundo grupo (en la misma mesa)
            var c3b = new Comanda(3);
            c3b.AgregarPlato(sesion.Menu[2], 2);
            c3b.AgregarPlato(sesion.Menu[4], 2);
            c3b.AgregarPlato(sesion.Menu[7], 2);
            sesion.RegistrarComanda(c3b);

            // ===== ESTADOS DE MESAS =====
            sesion.Mesas[0].Estado = EstadoMesa.OcupadaConComanda; // Mesa 1
            sesion.Mesas[1].Estado = EstadoMesa.OcupadaConComanda; // Mesa 2
            sesion.Mesas[2].Estado = EstadoMesa.OcupadaConComanda; // Mesa 3
            sesion.Mesas[3].Estado = EstadoMesa.Reservada;         // Mesa 4
            sesion.Mesas[4].Estado = EstadoMesa.Libre;             // Mesa 5

            // ===== COMENSALES ACTUALES =====
            sesion.Mesas[0].CapacidadActual = 2; // igual que los platos pedidos
            sesion.Mesas[1].CapacidadActual = 3;
            sesion.Mesas[2].CapacidadActual = 2; // último grupo atendido
            sesion.Mesas[3].CapacidadActual = 0; // reservada, sin clientes aún
            sesion.Mesas[4].CapacidadActual = 0; // libre

            // ===== ASIGNAR MESAS A LA MATRIZ =====
            int index = 0;
            for (int f = 0; f < Sesion.Filas; f++)
            {
                for (int c = 0; c < Sesion.Columnas; c++)
                {
                    if (index < sesion.Mesas.Count)
                    {
                        sesion.Disposicion[f, c] = sesion.Mesas[index];
                        index++;
                    }
                }
            }

            return sesion;
        }
    }
}

