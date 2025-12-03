using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using TrabajoIGU.Models;

namespace TrabajoIGU.Data
{
    public static class SeedData
    {
        public static Sesion CrearSesionDePrueba()
        {
            var sesion = new Sesion();

            sesion.Mesas = new List<Mesa>
            {
                new Mesa(1, 2),
                new Mesa(2, 4),
                new Mesa(3, 4),
                new Mesa(4, 6),
                new Mesa(5, 2)
            };

            sesion.Menu = IntentarCargarMenuDesdeDialogo();

            if (sesion.Menu == null || sesion.Menu.Count == 0)
            {
                sesion.Menu = ObtenerMenuPorDefecto();
                MessageBox.Show("No se selecciono archivo o estaba vacio.\nSe ha cargado el menú por defecto.",
                                "Menu por defecto", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            var comandaMesa3Antigua = new Comanda(3);
            comandaMesa3Antigua.Platos[sesion.Menu[0]] = 2;
            comandaMesa3Antigua.Platos[sesion.Menu[3]] = 2;
            sesion.ComandasHistoricas.Add(comandaMesa3Antigua);

            var mesa1 = sesion.Mesas[0];
            mesa1.ComandaActiva = new Comanda(1);
            mesa1.ComandaActiva.Platos[sesion.Menu[0]] = 2;
            mesa1.ComandaActiva.Platos[sesion.Menu[3]] = 2;
            mesa1.ComandaActiva.Platos[sesion.Menu[6]] = 2;

            var mesa2 = sesion.Mesas[1];
            mesa2.ComandaActiva = new Comanda(2);
            mesa2.ComandaActiva.Platos[sesion.Menu[1]] = 3;
            mesa2.ComandaActiva.Platos[sesion.Menu[5]] = 3;
            mesa2.ComandaActiva.Platos[sesion.Menu[8]] = 3;

            var mesa3 = sesion.Mesas[2];
            mesa3.ComandaActiva = new Comanda(3);
            mesa3.ComandaActiva.Platos[sesion.Menu[2]] = 2;
            mesa3.ComandaActiva.Platos[sesion.Menu[4]] = 2;
            mesa3.ComandaActiva.Platos[sesion.Menu[7]] = 2;

            sesion.Mesas[0].Estado = EstadoMesa.OcupadaConComanda;
            sesion.Mesas[1].Estado = EstadoMesa.OcupadaConComanda;
            sesion.Mesas[2].Estado = EstadoMesa.OcupadaConComanda;
            sesion.Mesas[3].Estado = EstadoMesa.Reservada;
            sesion.Mesas[4].Estado = EstadoMesa.Libre;

            sesion.Mesas[0].CapacidadActual = 2;
            sesion.Mesas[1].CapacidadActual = 3;
            sesion.Mesas[2].CapacidadActual = 2;
            sesion.Mesas[3].CapacidadActual = 2;
            sesion.Mesas[4].CapacidadActual = 2;

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

        private static List<Plato> IntentarCargarMenuDesdeDialogo()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar archivo de menú",
                Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*"
            };

            bool? resultado = dialog.ShowDialog();

            if (resultado == true)
            {
                try
                {
                    var lista = CargarMenuDesdeArchivo(dialog.FileName);
                    MessageBox.Show("Archivo cargado correctamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return lista;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar el archivo:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            return null;
        }

        private static List<Plato> ObtenerMenuPorDefecto()
        {
            return new List<Plato>
            {
                // Primeros
                new Plato("Ensalada mixta", CategoriaPlato.Primero, "Lechuga fresca, tomate y cebolla"),
                new Plato("Sopa de verduras", CategoriaPlato.Primero, "Caldo suave con verduras de temporada"),
                new Plato("Gazpacho andaluz", CategoriaPlato.Primero, "Tomate, pepino, ajo y aceite de oliva"),

                // Segundos
                new Plato("Pollo al horno", CategoriaPlato.Segundo, "Pollo marinado asado lentamente"),
                new Plato("Merluza a la plancha", CategoriaPlato.Segundo, "Filete de merluza con aceite y limon"),
                new Plato("Filete con patatas", CategoriaPlato.Segundo, "Carne de ternera acompanada de patatas fritas"),

                // Postres
                new Plato("Tarta de queso", CategoriaPlato.Postre, "Tarta casera cremosa al horno"),
                new Plato("Fruta del tiempo", CategoriaPlato.Postre, "Seleccion fresca de frutas de temporada"),
                new Plato("Flan casero", CategoriaPlato.Postre, "Flan tradicional con caramelo")
            };
        }

        private static List<Plato> CargarMenuDesdeArchivo(string ruta)
        {
            var lista = new List<Plato>();

            if (!File.Exists(ruta))
                throw new FileNotFoundException($"No se encontró el archivo de menú:\n{ruta}");

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
