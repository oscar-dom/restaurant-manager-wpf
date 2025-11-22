using System;
using System.Collections.Generic;
using System.IO;
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
            var rutaMenu = System.IO.Path.Combine("Data", "menu.txt");
            sesion.Menu = CargarMenuDesdeArchivo(rutaMenu);

            /*sesion.Menu = new List<Plato>
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
            };*/

            // ============================================================
            // COMANDAS HISTÓRICAS → SOLO antiguas, NO las activas
            // ============================================================

            // Mesa 3 tuvo dos grupos -> ambas van a histórico
            var c3a = new Comanda(3);
            c3a.AgregarPlato(sesion.Menu[0], 2);
            c3a.AgregarPlato(sesion.Menu[3], 2);

            sesion.ComandasHistoricas.Add(c3a);

            // ============================================================
            // COMANDAS ACTIVAS → SOLO la última comanda de cada mesa activa
            // ============================================================

            // Mesa 1 actual
            var mesa1 = sesion.Mesas[0];
            mesa1.ComandaActiva = new Comanda(1);
            mesa1.ComandaActiva.AgregarPlato(sesion.Menu[0], 2);
            mesa1.ComandaActiva.AgregarPlato(sesion.Menu[3], 2);
            mesa1.ComandaActiva.AgregarPlato(sesion.Menu[6], 2);

            // Mesa 2 actual
            var mesa2 = sesion.Mesas[1];
            mesa2.ComandaActiva = new Comanda(2);
            mesa2.ComandaActiva.AgregarPlato(sesion.Menu[1], 3);
            mesa2.ComandaActiva.AgregarPlato(sesion.Menu[5], 3);
            mesa2.ComandaActiva.AgregarPlato(sesion.Menu[8], 3);

            var mesa3 = sesion.Mesas[2];
            mesa3.ComandaActiva= new Comanda(3);
            mesa3.ComandaActiva.AgregarPlato(sesion.Menu[2], 2);
            mesa3.ComandaActiva.AgregarPlato(sesion.Menu[4], 2);
            mesa3.ComandaActiva.AgregarPlato(sesion.Menu[7], 2);


            // ===== ESTADOS DE MESAS =====
            sesion.Mesas[0].Estado = EstadoMesa.OcupadaConComanda;
            sesion.Mesas[1].Estado = EstadoMesa.OcupadaConComanda;
            sesion.Mesas[2].Estado = EstadoMesa.OcupadaConComanda;
            sesion.Mesas[3].Estado = EstadoMesa.Reservada;
            sesion.Mesas[4].Estado = EstadoMesa.Libre;

            // ===== COMENSALES ACTUALES =====
            sesion.Mesas[0].CapacidadActual = 2;
            sesion.Mesas[1].CapacidadActual = 3;
            sesion.Mesas[2].CapacidadActual = 2;
            sesion.Mesas[3].CapacidadActual = 2; // reservada
            sesion.Mesas[4].CapacidadActual = 2; // libre

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

        private static List<Plato> CargarMenuDesdeArchivo(string ruta)
        {
            var lista = new List<Plato>();

            if (!File.Exists(ruta))
                throw new FileNotFoundException($"No se encontró el archivo de menú: {ruta}");

            foreach (var linea in File.ReadAllLines(ruta))
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;
                if (linea.Trim().StartsWith("#")) continue;

                var partes = linea.Split('|');
                if (partes.Length < 2) continue;

                string nombre = partes[0].Trim();
                string categoriaTexto = partes[1].Trim();
                string descripcion = partes.Length >= 3 ? partes[2].Trim() : "";

                CategoriaPlato categoria;
                if (!Enum.TryParse(categoriaTexto, out categoria))
                    continue;

                lista.Add(new Plato(nombre, categoria, descripcion));
            }

            return lista;
        }

    }
}

