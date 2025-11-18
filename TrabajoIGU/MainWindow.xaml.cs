using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrabajoIGU.Data;
using TrabajoIGU.Models;
using TrabajoIGU.Windows;

namespace TrabajoIGU
{
    public partial class MainWindow : Window
    {
        private Sesion sesion;
        private Mesa mesaSeleccionada;
        private Dictionary<UIElement, Mesa> mapaMesas = new Dictionary<UIElement, Mesa>();
        private SecondaryWindow secondaryWindow;
        private bool inicializandoMenu = true;
        // Colores consistentes por plato (para el gráfico 7.2)
        private Dictionary<string, Brush> mapaColoresPlatos = new Dictionary<string, Brush>();


        public MainWindow()
        {
            InitializeComponent();
            sesion = SeedData.CrearSesionDePrueba();
            DibujarMesas();
            Loaded += MainWindow_Loaded;

        }

        //INTERFAZ
        #region Actualización de interfaz
        private void DibujarMesas()
        {
            if (canvasSala.ActualWidth == 0 || canvasSala.ActualHeight == 0)
                return;

            canvasSala.Children.Clear();
            mapaMesas.Clear();

            // Tamaño de la rejilla (4 filas × 3 columnas)
            int filas = Sesion.Filas;
            int columnas = Sesion.Columnas;

            // Margen porcentual
            double margenX = canvasSala.ActualWidth * 0.05;
            double margenY = canvasSala.ActualHeight * 0.05;

            // Tamaño útil
            double espacioUtilX = canvasSala.ActualWidth - (2 * margenX);
            double espacioUtilY = canvasSala.ActualHeight - (2 * margenY);

            // Tamaño de cada celda
            double celdaAncho = espacioUtilX / columnas;
            double celdaAlto = espacioUtilY / filas;

            // Tamaño de la mesa dentro de la celda (75% del tamaño)
            double mesaSize = Math.Min(celdaAncho, celdaAlto) * 0.75;

            for (int fila = 0; fila < filas; fila++)
            {
                for (int col = 0; col < columnas; col++)
                {
                    Mesa mesa = sesion.Disposicion[fila, col];

                    // Posición superior izquierda de la celda
                    double x = margenX + col * celdaAncho + (celdaAncho - mesaSize) / 2;
                    double y = margenY + fila * celdaAlto + (celdaAlto - mesaSize) / 2;

                    if (mesa == null)
                    {
                        int numeroCelda = fila * columnas + col + 1;

                        Border celdaVacia = new Border
                        {
                            Width = mesaSize,
                            Height = mesaSize,
                            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                            BorderThickness = new Thickness(2),
                            Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                            Cursor = Cursors.Hand,
                            Tag = numeroCelda
                        };

                        Canvas.SetLeft(celdaVacia, x);
                        Canvas.SetTop(celdaVacia, y);

                        celdaVacia.MouseLeftButtonDown += CeldaVacia_LeftClick;
                        celdaVacia.MouseRightButtonDown += CeldaVacia_RightClick;

                        canvasSala.Children.Add(celdaVacia);


                        TextBlock lbl = new TextBlock
                        {
                            Text = numeroCelda.ToString(),
                            FontSize = mesaSize * 0.22,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Black,
                            TextAlignment = TextAlignment.Center,
                            Width = mesaSize * 0.8,
                            Height = mesaSize * 0.22 * 1.3,
                            Opacity = 0.6,
                            IsHitTestVisible = false
                        };

                        Canvas.SetLeft(lbl, x + (mesaSize - lbl.Width) / 2);
                        Canvas.SetTop(lbl, y + (mesaSize - lbl.Height) / 2);

                        canvasSala.Children.Add(lbl);

                        continue;
                    }

                    // Imagen de la mesa
                    Image imgMesa = new Image
                    {
                        Source = new BitmapImage(new Uri(GetRutaImagenPorEstado(mesa.Estado), UriKind.Relative)),
                        Cursor = Cursors.Hand,
                        Tag = mesa
                    };

                    Border borde = new Border
                    {
                        Width = mesaSize,
                        Height = mesaSize,
                        BorderThickness = new Thickness(3),
                        BorderBrush = Brushes.Black,
                        Child = imgMesa
                    };

                    Canvas.SetLeft(borde, x);
                    Canvas.SetTop(borde, y);

                    borde.MouseLeftButtonDown += Mesa_LeftClick;
                    borde.MouseRightButtonDown += Mesa_RightClick;

                    canvasSala.Children.Add(borde);
                    mapaMesas.Add(borde, mesa);

                    TextBlock label = new TextBlock
                    {
                        Text = mesa.Id.ToString(),
                        FontSize = mesaSize * 0.14,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        Background = Brushes.Black,
                        TextAlignment = TextAlignment.Center,
                        Width = mesaSize * 0.25,
                        Opacity = 0.6,
                        IsHitTestVisible = false
                    };

                    Canvas.SetLeft(label, x + mesaSize / 2.7);
                    Canvas.SetTop(label, y + mesaSize * 0.75);
                    canvasSala.Children.Add(label);
                }
            }

            ActualizarSeleccionVisual();
            MostrarDatosMesa();
        }

        private void canvasSala_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DibujarMesas();
        }

        private void ActualizarSeleccionVisual()
        {
            foreach (var kvp in mapaMesas)
            {
                var borde = kvp.Key as Border;
                if (borde == null) continue;

                if (mesaSeleccionada != null && kvp.Value.Id == mesaSeleccionada.Id)
                {
                    borde.BorderBrush = Brushes.Red;
                }
                else
                {
                    borde.BorderBrush = Brushes.Black;
                }
            }
        }

        private void MostrarDatosMesa()
        {
            // Resumen restaurante
            int activas = sesion.Disposicion.Cast<Mesa>().Count(m => m != null);
            txtResumenRestaurante.Text = $"Mesas activas: {activas} / {Sesion.Filas * Sesion.Columnas}";

            int personasActuales = sesion.Disposicion.Cast<Mesa>()
                                   .Where(m => m != null)
                                   .Sum(m => m.CapacidadActual);

            int aforoMaximo = sesion.Disposicion.Cast<Mesa>()
                                 .Where(m => m != null)
                                 .Sum(m => m.CapacidadMaxima);

            txtAforoRestaurante.Text = $"Aforo: {personasActuales} / {aforoMaximo}";

            // Si no hay mesa seleccionada
            if (mesaSeleccionada == null)
            {
                LimpiarPanel();
                secondaryWindow?.ActualizarVista(sesion, null);
                return;
            }

            // Datos básicos de la mesa
            txtIdMesa.Text = mesaSeleccionada.Id.ToString();
            txtCapacidad.Text = mesaSeleccionada.CapacidadMaxima.ToString();
            txtEstado.Text = mesaSeleccionada.Estado.ToString();
            txtComensales.Text = mesaSeleccionada.CapacidadActual.ToString();

            // ✅ AHORA LEEMOS SIEMPRE DE ComandaActiva, NO DEL HISTÓRICO
            var comandaActual = mesaSeleccionada.ComandaActiva;

            if (comandaActual != null && comandaActual.Platos.Count > 0)
            {
                int totalPlatos = comandaActual.TotalPlatos();
                txtPlatos.Text = totalPlatos.ToString();
            }
            else
            {
                txtPlatos.Text = "0";
            }

            // Actualizar ventana secundaria
            secondaryWindow?.ActualizarVista(sesion, mesaSeleccionada);

            if (gridEstadisticasMesa.Visibility == Visibility.Visible && mesaSeleccionada != null)
            {
                txtTituloEstadisticaMesa.Text = $"Estadísticas de la mesa {mesaSeleccionada.Id}";
                DibujarEstadisticasMesa();
            }
        }

        private void LimpiarPanel()
        {
            txtIdMesa.Text = "Ninguna mesa seleccionada";
            txtCapacidad.Text = "Ninguna mesa seleccionada";
            txtEstado.Text = "Ninguna mesa seleccionada";
            txtComensales.Text = "Ninguna mesa seleccionada";
            txtPlatos.Text = "Ninguna mesa seleccionada";
        }

        private string GetRutaImagenPorEstado(EstadoMesa estado)
        {
            switch (estado)
            {
                case EstadoMesa.Libre:
                    return "Imagenes/mesaLibre.png";
                case EstadoMesa.Reservada:
                    return "Imagenes/mesaReservada.png";
                case EstadoMesa.OcupadaSinComanda:
                    return "Imagenes/mesaOcupada.png";
                case EstadoMesa.OcupadaConComanda:
                    return "Imagenes/mesaComanda.png";
                default:
                    return "Imagenes/mesaLibre.png";
            }
        }

        private void MesaSeleccionadaDesdeSecundaria(Mesa mesa)
        {
            mesaSeleccionada = mesa;
            MostrarDatosMesa();
            ActualizarSeleccionVisual();
        }

        private void DibujarEstadisticasGlobales()
        {
            canvasEstadisticasGlobales.Children.Clear();

            // ===============================
            // 1) OBTENER MESAS ACTIVAS
            // ===============================
            var mesasActivas = sesion.Disposicion
                .Cast<Mesa>()
                .Where(m => m != null)
                .ToList();

            // ===============================
            // 2) OBTENER MESAS SOLO HISTÓRICAS
            // ===============================
            var idsHistoricas = sesion.ComandasHistoricas
                .Select(c => c.IdMesa)
                .Distinct()
                .Where(id => !mesasActivas.Any(m => m.Id == id));

            // Creamos "mesas fantasma" solo para mostrar en el gráfico
            var mesasHistoricas = idsHistoricas
                .Select(id => new Mesa(id, 0))
                .ToList();

            // ===============================
            // 3) LISTA TOTAL DE MESAS A GRAFICAR
            // ===============================
            var todasLasMesas = mesasActivas
                .Concat(mesasHistoricas)
                .OrderBy(m => m.Id)
                .ToList();

            if (todasLasMesas.Count == 0)
                return;

            // ===============================
            // 4) CALCULAR DATOS POR MESA
            // ===============================
            var datos = todasLasMesas.Select(m => new
            {
                Mesa = m,
                Total = TotalPlatosMesa(m)
            }).ToList();

            int max = datos.Max(d => d.Total);
            if (max == 0) max = 1; // evitar división por cero

            // ===============================
            // 5) DIMENSIONES DEL CANVAS
            // ===============================
            double anchoCanvas = canvasEstadisticasGlobales.ActualWidth;
            double altoCanvas = canvasEstadisticasGlobales.ActualHeight;

            if (anchoCanvas <= 0 || altoCanvas <= 0)
                return;

            double espacio = 20; // separación entre columnas
            double anchoColumna = (anchoCanvas - espacio * (datos.Count + 1)) / datos.Count;
            if (anchoColumna < 10) anchoColumna = 10;

            double altoMaxColumna = altoCanvas - 40; // margen inferior + etiquetas

            int index = 0;

            foreach (var d in datos)
            {
                double x = espacio + index * (anchoColumna + espacio);

                double altura = (d.Total / (double)max) * altoMaxColumna;
                double y = altoCanvas - altura - 20; // 20px margen inferior

                bool esActiva = mesasActivas.Any(m => m.Id == d.Mesa.Id);

                // ===============================
                // 6) COLUMNA
                // ===============================
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = anchoColumna,
                    Height = altura,
                    Fill = esActiva
                        ? new SolidColorBrush(Color.FromRgb(70, 130, 180))   // azul suave
                        : new SolidColorBrush(Color.FromRgb(150, 150, 150))  // gris para mesas solo históricas
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                canvasEstadisticasGlobales.Children.Add(rect);

                // ===============================
                // 7) ETIQUETA VALOR (TOTAL PLATOS)
                // ===============================
                var lblValor = new TextBlock
                {
                    Text = d.Total.ToString(),
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(lblValor, x + anchoColumna / 2 - 10);
                Canvas.SetTop(lblValor, y - 20);
                canvasEstadisticasGlobales.Children.Add(lblValor);

                // ===============================
                // 8) ETIQUETA MESA (ID)
                // ===============================
                var lblMesa = new TextBlock
                {
                    Text = "Mesa " + d.Mesa.Id,
                    FontSize = 14,
                    TextAlignment = TextAlignment.Center,
                    Width = anchoColumna
                };

                Canvas.SetLeft(lblMesa, x);
                Canvas.SetTop(lblMesa, altoCanvas - 18);
                canvasEstadisticasGlobales.Children.Add(lblMesa);

                index++;
            }
        }

        private void canvasEstadisticasGlobales_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (gridEstadisticasGlobales.Visibility == Visibility.Visible)
                DibujarEstadisticasGlobales();
        }

        private void DibujarEstadisticasMesa()
        {
            canvasEstadisticasMesa.Children.Clear();

            if (mesaSeleccionada == null)
            {
                var msg = new TextBlock
                {
                    Text = "No hay ninguna mesa seleccionada.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(msg, 20);
                Canvas.SetTop(msg, 20);
                canvasEstadisticasMesa.Children.Add(msg);
                return;
            }

            double ancho = canvasEstadisticasMesa.ActualWidth;
            double alto = canvasEstadisticasMesa.ActualHeight;

            if (ancho <= 0 || alto <= 0)
                return;

            // Categorías a representar
            var categorias = new[] { CategoriaPlato.Primero, CategoriaPlato.Segundo, CategoriaPlato.Postre };
            var nombresCategorias = new Dictionary<CategoriaPlato, string>
    {
        { CategoriaPlato.Primero, "Primeros" },
        { CategoriaPlato.Segundo, "Segundos" },
        { CategoriaPlato.Postre,  "Postres" }
    };

            // Datos por categoría
            var datosPorCategoria = new Dictionary<CategoriaPlato, Dictionary<string, int>>();
            var sumaPorCategoria = new Dictionary<CategoriaPlato, int>();

            // Para la leyenda: totales por plato en toda la mesa
            var totalesPlatoGlobal = new Dictionary<string, int>();

            foreach (var cat in categorias)
            {
                var dict = ObtenerPlatosPorCategoriaMesaIncluyendoActiva(mesaSeleccionada, cat);
                datosPorCategoria[cat] = dict;

                int suma = dict.Values.Sum();
                sumaPorCategoria[cat] = suma;

                foreach (var kvp in dict)
                {
                    if (!totalesPlatoGlobal.ContainsKey(kvp.Key))
                        totalesPlatoGlobal[kvp.Key] = 0;

                    totalesPlatoGlobal[kvp.Key] += kvp.Value;
                }
            }

            int maxColumna = sumaPorCategoria.Values.Max();
            if (maxColumna == 0)
            {
                var msg = new TextBlock
                {
                    Text = "Esta mesa no tiene platos registrados.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(msg, 20);
                Canvas.SetTop(msg, 20);
                canvasEstadisticasMesa.Children.Add(msg);
                return;
            }

            // Margenes del gráfico
            double margenIzq = 60;
            double margenDer = 20;
            double margenSup = 20;
            double margenInf = 40;

            double anchoUtil = ancho - margenIzq - margenDer;
            double altoUtil = alto - margenSup - margenInf;
            if (altoUtil <= 0) altoUtil = 10;

            // 3 columnas: una por categoría
            int numCols = categorias.Length;
            double separacionColumnas = anchoUtil / (numCols * 2.0); // pequeña separación
            double anchoColumna = anchoUtil / (numCols * 1.5);       // ancho razonable

            // Dibujo columnas apiladas
            for (int i = 0; i < numCols; i++)
            {
                var cat = categorias[i];
                var datosCat = datosPorCategoria[cat];

                // X de la columna
                double xCol = margenIzq + i * (anchoColumna + separacionColumnas);
                double baseY = margenSup + altoUtil; // empieza desde abajo

                // Orden opcional de platos (p. ej. por nombre)
                foreach (var kvp in datosCat.OrderBy(k => k.Key))
                {
                    string nombrePlato = kvp.Key;
                    int cantidad = kvp.Value;

                    if (cantidad <= 0)
                        continue;

                    double alturaSeg = (cantidad / (double)maxColumna) * altoUtil;
                    if (alturaSeg < 2) alturaSeg = 2; // mínimo visible

                    double ySeg = baseY - alturaSeg;

                    var rect = new System.Windows.Shapes.Rectangle
                    {
                        Width = anchoColumna,
                        Height = alturaSeg,
                        Fill = GetColorParaPlato(nombrePlato),
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5,
                        ToolTip = $"{nombrePlato}\nPedidos: {cantidad}"
                    };

                    Canvas.SetLeft(rect, xCol);
                    Canvas.SetTop(rect, ySeg);
                    canvasEstadisticasMesa.Children.Add(rect);

                    // Mostrar número dentro si la barra es suficientemente alta
                    if (alturaSeg > 18)
                    {
                        var lblCant = new TextBlock
                        {
                            Text = cantidad.ToString(),
                            FontSize = 12,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.White
                        };

                        Canvas.SetLeft(lblCant, xCol + anchoColumna / 2 - 8);
                        Canvas.SetTop(lblCant, ySeg + alturaSeg / 2 - 8);
                        canvasEstadisticasMesa.Children.Add(lblCant);
                    }

                    baseY -= alturaSeg;
                }

                // Etiqueta de la categoría bajo la columna
                var lblCat = new TextBlock
                {
                    Text = nombresCategorias[cat],
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Width = anchoColumna
                };

                Canvas.SetLeft(lblCat, xCol);
                Canvas.SetTop(lblCat, margenSup + altoUtil + 5);
                canvasEstadisticasMesa.Children.Add(lblCat);
            }

            
        }

        private void canvasEstadisticasMesa_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (gridEstadisticasMesa.Visibility == Visibility.Visible && mesaSeleccionada != null)
            {
                DibujarEstadisticasMesa();
            }
        }


        #endregion

        //CONTROL DE EVENTOS
        #region Controladores de eventos
        private void Mesa_LeftClick(object sender, MouseButtonEventArgs e)
        {
            var borde = sender as Border;
            if (borde == null) return;

            if (mapaMesas.TryGetValue(borde, out var mesa))
            {
                mesaSeleccionada = mesa;
                MostrarDatosMesa();
                ActualizarSeleccionVisual();
            }
        }

        private void Mesa_RightClick(object sender, MouseButtonEventArgs e)
        {
            Mesa_LeftClick(sender, e);

            var borde = sender as Border;
            if (borde == null) return;

            if (!mapaMesas.TryGetValue(borde, out var mesa))
                return;

            ContextMenu menu = new ContextMenu();


            // =========================================================
            // OPCIÓN ─── CAMBIAR ESTADO...
            // =========================================================
            MenuItem itemCambiar = new MenuItem { Header = "Cambiar estado..." };

            foreach (var nuevoEstado in ObtenerEstadosPosibles(mesa.Estado))
            {
                MenuItem subItem = new MenuItem { Header = nuevoEstado.ToString() };

                subItem.Click += (s, ev) =>
                {
                    // ───────────────────────────────────────────────
                    // NO PERMITIR crear comanda sin comensales
                    // ───────────────────────────────────────────────
                    if (nuevoEstado == EstadoMesa.OcupadaConComanda && mesa.CapacidadActual == 0)
                    {
                        while (mesa.CapacidadActual < 1)
                        {
                            MessageBox.Show(
                            "No puedes abrir una comanda sin comensales.\nPor favor edita los comensales.",
                            "Advertencia",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                            var input = new InputComensalesWindow(mesa.CapacidadActual, mesa.CapacidadMaxima, mesa.Estado)
                            {
                                Owner = this,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner
                            };

                            if (input.ShowDialog() == true)
                                mesa.CapacidadActual = input.NumComensales;
                            else
                                return;
                        }
                    }

                    // ───────────────────────────────────────────────
                    // ENTRAR A EDITAR COMANDA AUTOMÁTICAMENTE
                    // ───────────────────────────────────────────────
                    if (nuevoEstado == EstadoMesa.OcupadaConComanda)
                    {
                        var win = new GestionComandaWindow(sesion, mesa)
                        {
                            Owner = this,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };

                        win.ShowDialog();

                        // Revisar comanda activa
                        var comanda = mesa.ComandaActiva;

                        if (comanda == null || comanda.Platos.Count == 0)
                        {
                            mesa.ComandaActiva = null;
                            mesa.Estado = EstadoMesa.OcupadaSinComanda;
                        }
                        else
                        {
                            mesa.Estado = EstadoMesa.OcupadaConComanda;
                        }

                        DibujarMesas();
                        return;
                    }

                    // ───────────────────────────────────────────────
                    // ESTADO → LIBRE (vaciar comanda si existe)
                    // ───────────────────────────────────────────────
                    if (nuevoEstado == EstadoMesa.Libre && mesa.CapacidadActual != 0)
                    {
                        if (mesa.Estado == EstadoMesa.OcupadaConComanda)
                        {
                            var r2 = MessageBox.Show(
                                "Si realiza esta acción se dará por finalizada la comanda actual.\n" +
                                "La mesa quedará libre.",
                                "Cerrar mesa",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);

                            if (r2 == MessageBoxResult.No)
                                return;

                            if (mesa.ComandaActiva != null)
                                sesion.ComandasHistoricas.Add(mesa.ComandaActiva);

                            mesa.ComandaActiva = null;
                        }

                        mesa.CapacidadActual = 0;
                        mesa.Estado = EstadoMesa.Libre;
                        DibujarMesas();
                        return;
                    }

                    // ───── Otros cambios de estado normales ───────────────────
                    mesa.Estado = nuevoEstado;
                    DibujarMesas();
                };

                itemCambiar.Items.Add(subItem);
            }
            menu.Items.Add(itemCambiar);


            // ============================================================
            // OPCIÓN ─── EDITAR Nº DE COMENSALES
            // ============================================================
            if (mesa.Estado == EstadoMesa.Reservada ||
                mesa.Estado == EstadoMesa.OcupadaSinComanda ||
                mesa.Estado == EstadoMesa.OcupadaConComanda)
            {
                MenuItem itemEditar = new MenuItem { Header = "Editar nº comensales actuales" };

                itemEditar.Click += (s, ev) =>
                {
                    var input = new InputComensalesWindow(mesa.CapacidadActual, mesa.CapacidadMaxima, mesa.Estado)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    if (input.ShowDialog() == true)
                    {
                        mesa.CapacidadActual = input.NumComensales;

                        if (mesa.Estado == EstadoMesa.OcupadaConComanda && mesa.CapacidadActual == 0)
                        {
                            mesa.Estado = EstadoMesa.Libre;
                            mesa.ComandaActiva = null;
                        }

                        DibujarMesas();
                    }
                };
                menu.Items.Add(itemEditar);
            }


            // ============================================================
            // OPCIÓN ─── EDITAR COMANDA
            // ============================================================
            if ((mesa.Estado == EstadoMesa.OcupadaSinComanda && mesa.CapacidadActual != 0) ||
                 mesa.Estado == EstadoMesa.OcupadaConComanda)
            {
                MenuItem itemComanda = new MenuItem { Header = "Editar comanda" };

                itemComanda.Click += (s, ev) =>
                {
                    var win = new GestionComandaWindow(sesion, mesa)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    win.ShowDialog();

                    if (mesa.ComandaActiva == null || mesa.ComandaActiva.Platos.Count == 0)
                    {
                        mesa.ComandaActiva = null;
                        mesa.Estado = EstadoMesa.OcupadaSinComanda;
                    }
                    else
                    {
                        mesa.Estado = EstadoMesa.OcupadaConComanda;
                    }

                    DibujarMesas();
                };

                menu.Items.Add(itemComanda);
            }


            // ============================================================
            // ⭐ OPCIÓN NUEVA ─── FINALIZAR COMANDA
            // ============================================================
            if (mesa.Estado == EstadoMesa.OcupadaConComanda && mesa.ComandaActiva != null)
            {
                MenuItem itemFinalizar = new MenuItem { Header = "Finalizar comanda" };

                itemFinalizar.Click += (s, ev) =>
                {
                    var r = MessageBox.Show(
                        "Se dará por finalizada la comanda.\n" +
                        "La mesa volverá a estar libre.\n\n" +
                        "¿Desea continuar?",
                        "Finalizar comanda",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (r == MessageBoxResult.No)
                        return;

                    // Pasar al histórico
                    sesion.ComandasHistoricas.Add(mesa.ComandaActiva);

                    mesa.ComandaActiva = null;
                    mesa.CapacidadActual = 0;
                    mesa.Estado = EstadoMesa.Libre;

                    DibujarMesas();
                };

                menu.Items.Add(itemFinalizar);
            }


            // ============================================================
            // OPCIÓN ─── ELIMINAR MESA
            // ============================================================
            MenuItem itemEliminar = new MenuItem { Header = "Eliminar mesa" };
            itemEliminar.Click += (s, ev) =>
            {
                for (int f = 0; f < Sesion.Filas; f++)
                {
                    for (int c = 0; c < Sesion.Columnas; c++)
                    {
                        if (sesion.Disposicion[f, c]?.Id == mesa.Id)
                        {
                            sesion.Disposicion[f, c] = null;
                            DibujarMesas();
                            LimpiarPanel();
                            return;
                        }
                    }
                }
            };
            menu.Items.Add(itemEliminar);


            // Mostrar menú
            menu.IsOpen = true;
            e.Handled = true;
        }

        private void CeldaVacia_RightClick(object sender, MouseButtonEventArgs e)
        {
            var celda = sender as Border;
            if (celda == null) return;

            int numeroCelda = (int)celda.Tag;

            ContextMenu menu = new ContextMenu();
            MenuItem itemAdd = new MenuItem { Header = "Añadir mesa" };
            itemAdd.Click += (s, ev) =>
            {
                var input = new InputMesaWindow(numeroCelda)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                if (input.ShowDialog() == true)
                {
                    int fila = (numeroCelda - 1) / Sesion.Columnas;
                    int col = (numeroCelda - 1) % Sesion.Columnas;

                    Mesa nueva = new Mesa(numeroCelda, input.CapacidadMaxima);
                    sesion.Disposicion[fila, col] = nueva;

                    mesaSeleccionada = nueva;

                    DibujarMesas();
                }
            };
            menu.Items.Add(itemAdd);
            menu.Placement = PlacementMode.MousePoint;
            menu.HorizontalOffset = 10;
            menu.IsOpen = true;

            e.Handled = true;
        }

        private void CeldaVacia_LeftClick(object sender, MouseButtonEventArgs e)
        {
            // Deselecciona cualquier mesa y limpia el panel
            mesaSeleccionada = null;
            LimpiarPanel();
            ActualizarSeleccionVisual();
            secondaryWindow?.ActualizarVista(sesion, mesaSeleccionada);
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            sesion.ReiniciarSesion();
            DibujarMesas();
            LimpiarPanel();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Si se hace clic en el canvas vacío, deseleccionar
            if (e.Source == canvasSala)
            {
                mesaSeleccionada = null;
                LimpiarPanel();
                ActualizarSeleccionVisual();
                secondaryWindow?.ActualizarVista(sesion, mesaSeleccionada);
            }
        }

        private void BtnVistaRestaurante_Click(object sender, RoutedEventArgs e)
        {
            // Si ya está abierta, la traemos al frente
            if (secondaryWindow != null && secondaryWindow.IsVisible)
            {
                secondaryWindow.Activate();
                return;
            }

            // Crear nueva instancia
            secondaryWindow = new SecondaryWindow(sesion)
            {
                Owner = this
            };

            // Suscripción a evento de selección desde la ventana secundaria
            secondaryWindow.MesaSeleccionadaDesdeSecundaria += MesaSeleccionadaDesdeSecundaria;

            // Mostrar ventana no modal
            secondaryWindow.Show();

            // Actualizar su vista inicial
            secondaryWindow.ActualizarVista(sesion, mesaSeleccionada);
        }

        private void BtnGestionMenu_Click(object sender, RoutedEventArgs e)
        {
            gridRestaurante.Visibility = Visibility.Collapsed;
            gridGestionMenu.Visibility = Visibility.Visible;

            AplicarFiltrosMenu();
        }

        private void BtnVolverGestionMenu_Click(object sender, RoutedEventArgs e)
        {
            gridGestionMenu.Visibility = Visibility.Collapsed;
            gridRestaurante.Visibility = Visibility.Visible;
        }

        private void BtnAgregarPlatoMenu_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new Windows.NuevoPlatoWindow()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (ventana.ShowDialog() == true)
            {
                // Añadir el plato a la sesión
                sesion.Menu.Add(ventana.PlatoCreado);

                // Refrescar lista
                AplicarFiltrosMenu();
            }
        }

        private void BtnLimpiarFiltrosMenu_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarMenu.Text = "";
            cbCategoriaMenu.SelectedIndex = 0;
        }

        private void LvPlatosMenu_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var platoSeleccionado = lvPlatosMenu.SelectedItem as Plato;
            if (platoSeleccionado == null)
                return;

            var ventana = new Windows.EditarPlatoWindow(platoSeleccionado)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (ventana.ShowDialog() == true)
            {
                if (ventana.Eliminado)
                {
                    sesion.Menu.Remove(platoSeleccionado);
                }
                else if (ventana.PlatoModificado != null)
                {
                    platoSeleccionado.Nombre = ventana.PlatoModificado.Nombre;
                    platoSeleccionado.Categoria = ventana.PlatoModificado.Categoria;
                    platoSeleccionado.Descripcion = ventana.PlatoModificado.Descripcion;
                }

                AplicarFiltrosMenu();
            }
        }

        private void LvPlatosMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Comprobar si el click fue sobre un ListBoxItem
            var item = ItemsControl.ContainerFromElement(lvPlatosMenu, e.OriginalSource as DependencyObject)
                       as ListBoxItem;

            // Si NO fue sobre una tarjeta → deseleccionar
            if (item == null)
            {
                lvPlatosMenu.SelectedItem = null;
                return;
            }

            // Si sí fue sobre una tarjeta → selección normal
        }

        private void BtnEstadisticasGlobales_Click(object sender, RoutedEventArgs e)
        {
            // Oculta el restaurante y cualquier otra vista
            gridRestaurante.Visibility = Visibility.Collapsed;

            // Muestra este grid
            gridEstadisticasGlobales.Visibility = Visibility.Visible;

            // Dibujamos
            DibujarEstadisticasGlobales();
        }

        private void BtnVolverEstadisticasGlobales_Click(object sender, RoutedEventArgs e)
        {
            gridEstadisticasGlobales.Visibility = Visibility.Collapsed;
            gridEstadisticasMesa.Visibility = Visibility.Collapsed;

            // Vuelvo al restaurante
            gridRestaurante.Visibility = Visibility.Visible;
        }

        private void BtnEstadisticasMesa_Click(object sender, RoutedEventArgs e)
        {
            if (mesaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una mesa primero.", "Aviso");
                return;
            }

            // Oculto todo
            gridRestaurante.Visibility = Visibility.Collapsed;
            gridGestionMenu.Visibility = Visibility.Collapsed;
            gridEstadisticasGlobales.Visibility = Visibility.Collapsed;

            // Muestro este
            gridEstadisticasMesa.Visibility = Visibility.Visible;

            // Actualizo el título
            txtTituloEstadisticaMesa.Text = $"Estadísticas de la mesa {mesaSeleccionada.Id}";

            // Y dibujo la gráfica
            DibujarEstadisticasMesa();
        }

        private void BtnVolverEstadisticasMesa_Click(object sender, RoutedEventArgs e)
        {
            gridEstadisticasMesa.Visibility = Visibility.Collapsed;

            gridRestaurante.Visibility = Visibility.Visible;
        }

        #endregion

        //AUXULIARES
        #region Métodos auxiliares
        private List<EstadoMesa> ObtenerEstadosPosibles(EstadoMesa estadoActual)
        {
            var lista = new List<EstadoMesa>();

            switch (estadoActual)
            {
                case EstadoMesa.Libre:
                    lista.Add(EstadoMesa.Reservada);
                    lista.Add(EstadoMesa.OcupadaSinComanda);
                    break;

                case EstadoMesa.Reservada:
                    lista.Add(EstadoMesa.Libre);
                    lista.Add(EstadoMesa.OcupadaSinComanda);
                    break;

                case EstadoMesa.OcupadaSinComanda:
                    lista.Add(EstadoMesa.Libre);
                    lista.Add(EstadoMesa.OcupadaConComanda);
                    break;

                case EstadoMesa.OcupadaConComanda:
                    lista.Add(EstadoMesa.Libre);
                    break;
            }

            return lista;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            inicializandoMenu = false;
            AplicarFiltrosMenu();
        }

        private void FiltroMenu_Changed(object sender, EventArgs e)
        {
            if (inicializandoMenu)
                return;

            AplicarFiltrosMenu();
        }

        private void AplicarFiltrosMenu()
        {
            if (sesion?.Menu == null)
                return;

            var lista = sesion.Menu.ToList();

            // Texto
            string texto = txtBuscarMenu.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(texto))
            {
                lista = lista.Where(p =>
                       p.Nombre.ToLower().Contains(texto) ||
                       (p.Descripcion ?? "").ToLower().Contains(texto)
                ).ToList();
            }

            // Categoría
            var item = cbCategoriaMenu.SelectedItem as ComboBoxItem;
            string categoria = item?.Content?.ToString();

            if (!string.IsNullOrEmpty(categoria) && categoria != "Todas")
            {
                if (Enum.TryParse<CategoriaPlato>(categoria, out var cat))
                {
                    lista = lista.Where(p => p.Categoria == cat).ToList();
                }
            }

            lvPlatosMenu.ItemsSource = lista;
        }

        private int TotalPlatosMesa(Mesa mesa)
        {
            int total = 0;

            // Histórico
            foreach (var c in sesion.ComandasHistoricas.Where(c => c.IdMesa == mesa.Id))
                total += c.TotalPlatos();

            // Comanda activa
            if (mesa.ComandaActiva != null)
                total += mesa.ComandaActiva.TotalPlatos();

            return total;
        }

        private Brush GetColorParaPlato(string nombrePlato)
        {
            if (mapaColoresPlatos.ContainsKey(nombrePlato))
                return mapaColoresPlatos[nombrePlato];

            // Paleta fija de colores sobrios
            Brush[] paleta = new Brush[]
            {
        new SolidColorBrush(Color.FromRgb(70,130,180)),   // SteelBlue
        new SolidColorBrush(Color.FromRgb(46,139,87)),    // SeaGreen
        new SolidColorBrush(Color.FromRgb(255,140,0)),    // DarkOrange
        new SolidColorBrush(Color.FromRgb(123,104,238)),  // MediumSlateBlue
        new SolidColorBrush(Color.FromRgb(220,20,60)),    // Crimson
        new SolidColorBrush(Color.FromRgb(189,183,107)),  // DarkKhaki
            };

            int index = mapaColoresPlatos.Count % paleta.Length;
            Brush elegido = paleta[index];
            mapaColoresPlatos[nombrePlato] = elegido;
            return elegido;
        }

        private Dictionary<string, int> ObtenerPlatosPorCategoriaMesaIncluyendoActiva(Mesa mesa, CategoriaPlato categoria)
        {
            Dictionary<string, int> resultado = new Dictionary<string, int>();

            // 1️⃣ Comandas históricas (solo sumar las que sean de la mesa)
            var historicas = sesion.ComandasHistoricas.Where(c => c.IdMesa == mesa.Id);

            foreach (var com in historicas)
            {
                foreach (var kvp in com.Platos)
                {
                    if (kvp.Key.Categoria != categoria)
                        continue;

                    if (!resultado.ContainsKey(kvp.Key.Nombre))
                        resultado[kvp.Key.Nombre] = 0;

                    resultado[kvp.Key.Nombre] += kvp.Value;
                }
            }

            // 2️⃣ Comanda activa (solo una, y no debe duplicarse)
            if (mesa.ComandaActiva != null)
            {
                foreach (var kvp in mesa.ComandaActiva.Platos)
                {
                    if (kvp.Key.Categoria != categoria)
                        continue;

                    if (!resultado.ContainsKey(kvp.Key.Nombre))
                        resultado[kvp.Key.Nombre] = 0;

                    resultado[kvp.Key.Nombre] += kvp.Value;
                }
            }

            return resultado;
        }


        #endregion

    }
}
