using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        private Dictionary<string, Brush> mapaColoresPlatos = new Dictionary<string, Brush>();
        private readonly HashSet<Mesa> mesasSuscritas = new HashSet<Mesa>();


        public MainWindow()
        {
            InitializeComponent();
            sesion = SeedData.CrearSesionDePrueba();
            DibujarMesas();
        }

        //INTERFAZ
        #region Actualización de interfaz
        private void DibujarMesas()
        {
            SuscribirMesasSesion();
            if (canvasSala.ActualWidth == 0 || canvasSala.ActualHeight == 0)
                return;

            canvasSala.Children.Clear();
            mapaMesas.Clear();

            int filas = Sesion.Filas;
            int columnas = Sesion.Columnas;

            double margenX = canvasSala.ActualWidth * 0.05;
            double margenY = canvasSala.ActualHeight * 0.05;

            double espacioUtilX = canvasSala.ActualWidth - (2 * margenX);
            double espacioUtilY = canvasSala.ActualHeight - (2 * margenY);

            double celdaAncho = espacioUtilX / columnas;
            double celdaAlto = espacioUtilY / filas;

            double mesaSize = Math.Min(celdaAncho, celdaAlto) * 0.75;

            for (int fila = 0; fila < filas; fila++)
            {
                for (int col = 0; col < columnas; col++)
                {
                    Mesa mesa = sesion.Disposicion[fila, col];

                    double x = margenX + col * celdaAncho + (celdaAncho - mesaSize) / 2;
                    double y = margenY + fila * celdaAlto + (celdaAlto - mesaSize) / 2;

                    // ------------------ CELDA VACÍA ------------------
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

                    // ------------- MESA OCUPADA (RECT / CÍRCULO + IMAGEN) -------------
                    var imageBrush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(GetRutaImagenPorEstado(mesa.Estado), UriKind.Relative)),
                        Stretch = Stretch.UniformToFill
                    };

                    Border bordeMesa;
                    Shape formaContenido;

                    if (mesa.Id % 2 == 1)
                    {
                        double interior = mesaSize - (3 * 2);

                        formaContenido = new Rectangle
                        {
                            Fill = imageBrush,
                            Width = interior,
                            Height = interior,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        bordeMesa = new Border
                        {
                            Width = mesaSize,
                            Height = mesaSize,
                            BorderThickness = new Thickness(3),
                            BorderBrush = Brushes.Black,
                            Child = formaContenido,
                            Tag = mesa,
                            Cursor = Cursors.Hand
                        };
                    }
                    else
                    {
                        formaContenido = new Ellipse
                        {
                            Fill = imageBrush,
                            Width = mesaSize - 6,
                            Height = mesaSize - 6
                        };

                        bordeMesa = new Border
                        {
                            Width = mesaSize,
                            Height = mesaSize,
                            CornerRadius = new CornerRadius(mesaSize / 2),
                            BorderThickness = new Thickness(3),
                            BorderBrush = Brushes.Black,
                            Child = formaContenido,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Tag = mesa,
                            Cursor = Cursors.Hand
                        };
                    }

                    Canvas.SetLeft(bordeMesa, x);
                    Canvas.SetTop(bordeMesa, y);

                    bordeMesa.MouseLeftButtonDown += Mesa_LeftClick;
                    bordeMesa.MouseRightButtonDown += Mesa_RightClick;

                    canvasSala.Children.Add(bordeMesa);
                    mapaMesas.Add(bordeMesa, mesa);

                    // ------------------ ETIQUETA MESA ------------------
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

            int personasActuales = sesion.Disposicion.Cast<Mesa>().Where(m => m != null).Sum(m => m.CapacidadActual);

            int aforoMaximo = sesion.Disposicion.Cast<Mesa>().Where(m => m != null).Sum(m => m.CapacidadMaxima);

            txtAforoRestaurante.Text = $"Aforo: {personasActuales} / {aforoMaximo}";

            // Si no hay mesa seleccionada
            if (mesaSeleccionada == null)
            {
                LimpiarPanel();
                return;
            }

            // Datos básicos de la mesa
            txtIdMesa.Text = mesaSeleccionada.Id.ToString();
            txtCapacidad.Text = mesaSeleccionada.CapacidadMaxima.ToString();
            txtEstado.Text = mesaSeleccionada.Estado.ToString();
            txtComensales.Text = mesaSeleccionada.CapacidadActual.ToString();

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

        }

        private void LimpiarPanel()
        {
            txtIdMesa.Text = "Ninguna mesa seleccionada";
            txtCapacidad.Text = "Ninguna mesa seleccionada";
            txtEstado.Text = "Ninguna mesa seleccionada";
            txtComensales.Text = "Ninguna mesa seleccionada";
            txtPlatos.Text = "Ninguna mesa seleccionada";
        }

        private void DibujarEstadisticasGlobales()
        {
            canvasEstadisticasGlobales.Children.Clear();

            var mesasActivas = sesion.Disposicion.Cast<Mesa>().Where(m => m != null).ToList();

            var idsHistoricas = sesion.ComandasHistoricas.Select(c => c.IdMesa).Distinct().Where(id => !mesasActivas.Any(m => m.Id == id));

            var mesasHistoricas = idsHistoricas.Select(id => new Mesa(id, 0)).ToList();

            var todasLasMesas = mesasActivas.Concat(mesasHistoricas).OrderBy(m => m.Id).ToList();

            if (todasLasMesas.Count == 0)
                return;

            var datos = todasLasMesas.Select(m => new
            {
                Mesa = m,
                Total = TotalPlatosMesa(m)
            }).ToList();

            int max = datos.Max(d => d.Total);
            if (max == 0) max = 1;

            double anchoCanvas = canvasEstadisticasGlobales.ActualWidth;
            double altoCanvas = canvasEstadisticasGlobales.ActualHeight;

            if (anchoCanvas <= 0 || altoCanvas <= 0)
                return;

            // ---------------------------------------------------
            // MÁRGENES
            // ---------------------------------------------------
            double margenIzq = 50;
            double margenInf = 30;
            double margenSup = 20;

            double altoUtil = altoCanvas - margenInf - margenSup;
            if (altoUtil <= 0) altoUtil = 1;

            double espacio = 20;
            double anchoColumna = (anchoCanvas - margenIzq - espacio * (datos.Count + 1)) / datos.Count;
            if (anchoColumna < 10) anchoColumna = 10;

            double ejeX_Y = margenSup + altoUtil;

            int index = 0;

            // ---------------------------------------------------
            // DIBUJAR BARRAS
            // ---------------------------------------------------
            foreach (var d in datos)
            {
                double x = margenIzq + espacio + index * (anchoColumna + espacio);

                double altura = (d.Total / (double)max) * altoUtil;
                double y = ejeX_Y - altura;

                bool esActiva = mesasActivas.Any(m => m.Id == d.Mesa.Id);

                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = anchoColumna,
                    Height = altura,
                    Fill = esActiva
                        ? new SolidColorBrush(Color.FromRgb(70, 130, 180))
                        : new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                    ToolTip = $"Mesa {d.Mesa.Id}\nTotal platos: {d.Total}"
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                canvasEstadisticasGlobales.Children.Add(rect);

                var lblValor = new TextBlock
                {
                    Text = d.Total.ToString(),
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Foreground = Brushes.Black
                };

                lblValor.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size sizeValor = lblValor.DesiredSize;

                Canvas.SetLeft(lblValor, x + (anchoColumna - sizeValor.Width) / 2);
                Canvas.SetTop(lblValor, y - sizeValor.Height - 2);
                canvasEstadisticasGlobales.Children.Add(lblValor);

                var lblMesa = new TextBlock
                {
                    Text = "Mesa " + d.Mesa.Id,
                    FontSize = Math.Min(22, anchoColumna*0.15),
                    TextAlignment = TextAlignment.Center,
                    Width = anchoColumna
                };

                Canvas.SetLeft(lblMesa, x);
                Canvas.SetTop(lblMesa, ejeX_Y);
                canvasEstadisticasGlobales.Children.Add(lblMesa);

                index++;
            }

            // ---------------------------------------------------
            // EJES
            // ---------------------------------------------------
            int paso = ObtenerPasoGuia(max);

            var ejeY = new Line
            {
                X1 = margenIzq,
                Y1 = margenSup,
                X2 = margenIzq,
                Y2 = ejeX_Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvasEstadisticasGlobales.Children.Add(ejeY);

            var ejeX = new Line
            {
                X1 = margenIzq,
                Y1 = ejeX_Y,
                X2 = anchoCanvas,
                Y2 = ejeX_Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvasEstadisticasGlobales.Children.Add(ejeX);

            for (int v = 0; v <= max; v += paso)
            {
                double proporcion = v / (double)max;
                double Y = ejeX_Y - proporcion * altoUtil;

                if (v != 0)
                {
                    var lineaGuia = new Line
                    {
                        X1 = margenIzq,
                        Y1 = Y,
                        X2 = anchoCanvas,
                        Y2 = Y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 2, 2 }
                    };
                    canvasEstadisticasGlobales.Children.Add(lineaGuia);
                }

                TextBlock lbl = new TextBlock
                {
                    Text = v.ToString(),
                    FontSize = 12
                };

                lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size size = lbl.DesiredSize;

                Canvas.SetLeft(lbl, margenIzq - size.Width - 5);
                Canvas.SetTop(lbl, Y - size.Height / 2);
                canvasEstadisticasGlobales.Children.Add(lbl);
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
            panelLeyendaMesa.Children.Clear();

            if (mesaSeleccionada == null)
            {
                txtTituloEstadisticaMesa.Text = $"Ninguna mesa seleccionada";
                return;
            }

            txtTituloEstadisticaMesa.Text = $"Estadísticas de la mesa {mesaSeleccionada.Id}";

            double ancho = canvasEstadisticasMesa.ActualWidth;
            double alto = canvasEstadisticasMesa.ActualHeight;

            if (ancho <= 0 || alto <= 0)
                return;

            // -----------------------------------------------------
            // Cargar datos por categoría
            // -----------------------------------------------------
            var categorias = new[] { CategoriaPlato.Primero, CategoriaPlato.Segundo, CategoriaPlato.Postre };
            var nombresCategorias = new Dictionary<CategoriaPlato, string>
    {
        { CategoriaPlato.Primero, "Primeros" },
        { CategoriaPlato.Segundo, "Segundos" },
        { CategoriaPlato.Postre,  "Postres" }
    };

            var datosPorCategoria = new Dictionary<CategoriaPlato, Dictionary<string, int>>();
            var sumaPorCategoria = new Dictionary<CategoriaPlato, int>();
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
                msg.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size sizeMsg = msg.DesiredSize;

                Canvas.SetLeft(msg, (ancho - sizeMsg.Width) / 2);
                Canvas.SetTop(msg, (alto - sizeMsg.Height) / 2);
                canvasEstadisticasMesa.Children.Add(msg);
                return;
            }

            // -----------------------------------------------------
            // Márgenes y área útil (igual que en estadísticas globales)
            // -----------------------------------------------------
            double margenIzq = 60;
            double margenSup = 20;
            double margenInf = 40;

            double anchoUtil = ancho - margenIzq;
            double altoUtil = alto - margenSup - margenInf;

            double ejeX_Y = altoUtil + margenSup;

            int numCols = categorias.Length;

            double espacio = 20;

            double anchoColumna = (anchoUtil - espacio * (numCols + 1)) / numCols;
            if (anchoColumna < 10) anchoColumna = 10;

            // -----------------------------------------------------
            // DIBUJAR COLUMNAS APILADAS
            // -----------------------------------------------------
            for (int i = 0; i < numCols; i++)
            {
                var cat = categorias[i];
                var datosCat = datosPorCategoria[cat];

                double xCol = margenIzq + espacio + i * (anchoColumna + espacio);
                double baseY = ejeX_Y;

                int totalCat = sumaPorCategoria[cat];
                double alturaTot = (totalCat / (double)maxColumna) * altoUtil;
                double yTopColumna = ejeX_Y - alturaTot;

                foreach (var kvp in datosCat.OrderBy(k => k.Key))
                {
                    string nombrePlato = kvp.Key;
                    int cantidad = kvp.Value;

                    if (cantidad <= 0)
                        continue;

                    double alturaSeg = (cantidad / (double)maxColumna) * altoUtil;
                    if (alturaSeg < 2) alturaSeg = 2;

                    double ySeg = baseY - alturaSeg;

                    var rect = new Rectangle
                    {
                        Width = anchoColumna,
                        Height = alturaSeg,
                        Fill = GetColorParaPlato(nombrePlato),
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5,
                        ToolTip = $"Plato: {nombrePlato}\nCantidad: {cantidad}"
                    };

                    Canvas.SetLeft(rect, xCol);
                    Canvas.SetTop(rect, ySeg);
                    canvasEstadisticasMesa.Children.Add(rect);

                    if (alturaSeg > 18)
                    {
                        var lblCant = new TextBlock
                        {
                            Text = cantidad.ToString(),
                            FontSize = 12,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.White
                        };

                        lblCant.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        Size sCant = lblCant.DesiredSize;

                        Canvas.SetLeft(lblCant, xCol + (anchoColumna - sCant.Width) / 2);
                        Canvas.SetTop(lblCant, ySeg + (alturaSeg - sCant.Height) / 2);
                        canvasEstadisticasMesa.Children.Add(lblCant);
                    }

                    baseY -= alturaSeg;
                }

                var lblTotal = new TextBlock
                {
                    Text = totalCat.ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                lblTotal.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size sTot = lblTotal.DesiredSize;

                Canvas.SetLeft(lblTotal, xCol + (anchoColumna - sTot.Width) / 2);
                Canvas.SetTop(lblTotal, yTopColumna - sTot.Height - 4);
                canvasEstadisticasMesa.Children.Add(lblTotal);

                var lblCat = new TextBlock
                {
                    Text = nombresCategorias[cat],
                    FontSize = Math.Min(22, anchoColumna * 0.1),
                    FontWeight = FontWeights.Bold,
                    Width = anchoColumna,
                    TextAlignment = TextAlignment.Center
                };

                Canvas.SetLeft(lblCat, xCol);
                Canvas.SetTop(lblCat, ejeX_Y + 5);
                canvasEstadisticasMesa.Children.Add(lblCat);
            }

            // -----------------------------------------------------
            // Ejes y guías
            // -----------------------------------------------------
            int paso = ObtenerPasoGuia(maxColumna);

            canvasEstadisticasMesa.Children.Add(new Line
            {
                X1 = margenIzq,
                X2 = margenIzq + anchoUtil,
                Y1 = ejeX_Y,
                Y2 = ejeX_Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            });

            canvasEstadisticasMesa.Children.Add(new Line
            {
                X1 = margenIzq,
                X2 = margenIzq,
                Y1 = 0,
                Y2 = ejeX_Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            });

            for (int v = 0; v <= maxColumna; v += paso)
            {
                double prop = v / (double)maxColumna;
                double y = ejeX_Y - prop * altoUtil;

                if (v != 0)
                {
                    canvasEstadisticasMesa.Children.Add(new Line
                    {
                        X1 = margenIzq,
                        X2 = margenIzq + anchoUtil,
                        Y1 = y,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 2, 2 }
                    });
                }

                var lblY = new TextBlock
                {
                    Text = v.ToString(),
                    FontSize = 12
                };
                lblY.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size sizeY = lblY.DesiredSize;

                Canvas.SetLeft(lblY, margenIzq - sizeY.Width - 5);
                Canvas.SetTop(lblY, y - sizeY.Height / 2);
                canvasEstadisticasMesa.Children.Add(lblY);
            }

            // -----------------------------------------------------
            // LEYENDA
            // -----------------------------------------------------
            var tituloLeyenda = new TextBlock
            {
                Text = "Leyenda",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panelLeyendaMesa.Children.Add(tituloLeyenda);

            foreach (var kvp in totalesPlatoGlobal.OrderBy(k => k.Key))
            {
                var fila = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5, 2, 5, 2)
                };

                fila.Children.Add(new Rectangle
                {
                    Width = 16,
                    Height = 16,
                    Fill = GetColorParaPlato(kvp.Key),
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    Margin = new Thickness(0, 0, 5, 0)
                });

                fila.Children.Add(new TextBlock
                {
                    Text = $"{kvp.Key} ({kvp.Value})",
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center
                });

                panelLeyendaMesa.Children.Add(fila);
            }
        }

        private void canvasEstadisticasMesa_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (gridEstadisticasMesa.Visibility == Visibility.Visible)
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
                sesion.SeleccionarMesa(mesaSeleccionada);
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
                        if (mesa.ComandaActiva == null)
                        {
                            mesa.ComandaActiva = new Comanda(mesa.Id);
                            mesa.ComandaActiva.PropertyChanged += ComandaOnChanged;
                        }
                        mesa.ComandaActiva.PropertyChanged += ComandaOnChanged;
                        var win = new GestionComandaWindow(sesion, mesa)
                        {
                            Owner = this,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };

                        win.ShowDialog();
                        mesa.ComandaActiva.PropertyChanged -= ComandaOnChanged;

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
                    // ESTADO → LIBRE (vaciar y guardar comanda si existe)
                    // ───────────────────────────────────────────────
                    if (nuevoEstado == EstadoMesa.Libre && mesa.CapacidadActual != 0)
                    {
                        if (mesa.Estado == EstadoMesa.OcupadaConComanda)
                        {
                            var r2 = MessageBox.Show(
                                "Todos los comensales abandonarán la mesa.\n" +
                                "Se guardará la comanda y la mesa quedará libre.\n\n¿Deseas continuar?",
                                "Advertencia",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);

                            if (r2 == MessageBoxResult.No)
                                return;

                            var comanda = mesa.ComandaActiva;
                            GenerarFactura(mesa, comanda);

                            sesion.ComandasHistoricas.Add(mesa.ComandaActiva);

                            mesa.ComandaActiva = null;
                            mesa.CapacidadActual = 0;
                            mesa.Estado = EstadoMesa.Libre;

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
                            GenerarFactura(mesa, mesa.ComandaActiva);
                            sesion.ComandasHistoricas.Add(mesa.ComandaActiva);
                            mesa.Estado = EstadoMesa.Libre;
                            mesa.ComandaActiva = null;
                        }
                        if (mesa.Estado == EstadoMesa.OcupadaSinComanda && mesa.CapacidadActual == 0)
                        {
                            mesa.Estado = EstadoMesa.Libre;
                        }

                        DibujarMesas();
                    }
                };
                menu.Items.Add(itemEditar);
            }


            // ============================================================
            // OPCIÓN ─── EDITAR COMANDA
            // ============================================================
            if ((mesa.Estado == EstadoMesa.OcupadaSinComanda && mesa.CapacidadActual != 0) || mesa.Estado == EstadoMesa.OcupadaConComanda)
            {
                MenuItem itemComanda = new MenuItem { Header = "Editar comanda" };

                itemComanda.Click += (s, ev) =>
                {
                    if (mesa.ComandaActiva == null)
                    {
                        mesa.ComandaActiva = new Comanda(mesa.Id);
                        mesa.ComandaActiva.PropertyChanged += ComandaOnChanged;
                    }
                    mesa.ComandaActiva.PropertyChanged += ComandaOnChanged;
                    var win = new GestionComandaWindow(sesion, mesa)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    win.ShowDialog();
                    mesa.ComandaActiva.PropertyChanged -= ComandaOnChanged;

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
            // OPCIÓN ─── FINALIZAR COMANDA
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
                    
                    var comanda = mesa.ComandaActiva;
                    GenerarFactura(mesa, comanda);

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
                            var r = MessageBox.Show(
                                    "Si elimina la mesa se perderá la comanda actual.\n" +
                                    "Este cambio será irreversible.\n\n" +
                                    "¿Desea continuar?",
                                    "Eliminar mesa",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                            if (r == MessageBoxResult.No)
                                return;

                            sesion.EliminarMesa(mesaSeleccionada);
                            mesaSeleccionada = null;
                            sesion.SeleccionarMesa(mesaSeleccionada);
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
                    sesion.AñadirMesa(nueva,fila,col);

                    mesaSeleccionada = nueva;
                    sesion.SeleccionarMesa(mesaSeleccionada);

                    DibujarMesas();
                }
            };
            menu.Items.Add(itemAdd);
            menu.Placement = PlacementMode.MousePoint;
            menu.IsOpen = true;

            e.Handled = true;
        }

        private void CeldaVacia_LeftClick(object sender, MouseButtonEventArgs e)
        {
            mesaSeleccionada = null;
            sesion.SeleccionarMesa(mesaSeleccionada);
            LimpiarPanel();
            ActualizarSeleccionVisual();
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show(
                "Se reiniciará el restaurante:\n" +
                "• Se eliminará el historial de comandas.\n" +
                "• Se borrarán las mesas.\n" +
                "¿Desea continuar?",
                "Reiniciar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (r != MessageBoxResult.Yes)
                return;

            sesion.IniciarSesion();
            mesaSeleccionada = null;
            sesion.SeleccionarMesa(mesaSeleccionada);
            DibujarMesas();
            LimpiarPanel();
        }

        private void BtnVaciarMesas_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show(
                "Se vaciarán todas las mesas activas:\n" +
                "• Se eliminará la comanda activa.\n" +
                "• Los comensales actuales pasarán a 0.\n" +
                "• Las mesas volverán a estado Libre.\n" +
                "¿Desea continuar?",
                "Vaciar todas las mesas",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (r != MessageBoxResult.Yes)
                return;

            foreach (var mesa in sesion.Disposicion)
            {
                if (mesa == null) continue;

                mesa.CapacidadActual = 0;
                mesa.ComandaActiva = null;
                mesa.Estado = EstadoMesa.Libre;
            }
            DibujarMesas();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == canvasSala)
            {
                mesaSeleccionada = null;
                sesion.SeleccionarMesa(mesaSeleccionada);
                LimpiarPanel();
                ActualizarSeleccionVisual();
            }
        }

        private void BtnVistaRestaurante_Click(object sender, RoutedEventArgs e)
        {
            if (secondaryWindow != null && secondaryWindow.IsVisible)
            {
                secondaryWindow.Activate();
                return;
            }

            secondaryWindow = new SecondaryWindow(sesion, mesaSeleccionada)
            {
                Owner = this
            };

            secondaryWindow.Show();
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
                sesion.Menu.Add(ventana.PlatoCreado);

                AplicarFiltrosMenu();
            }
        }

        private void FiltroMenu_Changed(object sender, EventArgs e)
        {
            AplicarFiltrosMenu();
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
            var item = ItemsControl.ContainerFromElement(lvPlatosMenu, e.OriginalSource as DependencyObject)
                       as ListBoxItem;

            if (item == null)
            {
                lvPlatosMenu.SelectedItem = null;
                return;
            }
        }

        private void BtnEstadisticasGlobales_Click(object sender, RoutedEventArgs e)
        {
            gridRestaurante.Visibility = Visibility.Collapsed;
            gridEstadisticasGlobales.Visibility = Visibility.Visible;

            DibujarEstadisticasGlobales();
        }

        private void BtnVolverEstadisticasGlobales_Click(object sender, RoutedEventArgs e)
        {
            gridEstadisticasGlobales.Visibility = Visibility.Collapsed;
            gridRestaurante.Visibility = Visibility.Visible;
        }

        private void BtnEstadisticasMesa_Click(object sender, RoutedEventArgs e)
        {
            if (mesaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una mesa primero.", "Aviso");
                return;
            }

            gridRestaurante.Visibility = Visibility.Collapsed;
            gridEstadisticasMesa.Visibility = Visibility.Visible;

            txtTituloEstadisticaMesa.Text = $"Estadísticas de la mesa {mesaSeleccionada.Id}";

            DibujarEstadisticasMesa();
        }

        private void BtnVolverEstadisticasMesa_Click(object sender, RoutedEventArgs e)
        {
            gridEstadisticasMesa.Visibility = Visibility.Collapsed;
            gridRestaurante.Visibility = Visibility.Visible;
        }

        private void ComandaOnChanged(object sender, PropertyChangedEventArgs e)
        {
            if (mesaSeleccionada != null && mesaSeleccionada.ComandaActiva != null)
            {
                MostrarDatosMesa();
            }
        }

        private void MesaSeleccionadaOnChanged(object sender, PropertyChangedEventArgs e)
        {
            // Solo reaccionamos a cambios de la propiedad MesaSeleccionada
            if (e.PropertyName != nameof(Mesa.MesaSeleccionada))
                return;

            var m = sender as Mesa;
            if (m == null) return;

            // Asegurarnos de ejecutar la actualización en el hilo UI
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => MesaSeleccionadaOnChanged(sender, e));
                return;
            }

            // Si la mesa se marca como seleccionada, la adoptamos; si se desmarca y era la actual, la limpiamos
            if (m.MesaSeleccionada)
            {
                mesaSeleccionada = m;
            }
            else
            {
                if (mesaSeleccionada != null && mesaSeleccionada.Id == m.Id)
                    mesaSeleccionada = null;
            }

            MostrarDatosMesa();
            ActualizarSeleccionVisual();
            DibujarEstadisticasMesa();

        }
        #endregion

        //AUXULIARES
        #region Métodos auxiliares
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

        private void AplicarFiltrosMenu()
        {
            if (sesion?.Menu == null)
                return;

            //Menú sin filtros
            var lista = sesion.Menu.OrderBy(p => p.Categoria).ToList();

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

            // Paleta fija de colores
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

        public static void GenerarFactura(Mesa mesa, Comanda comanda)
        {
            if (comanda == null || comanda.Platos.Count == 0)
                return;

            string fecha = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            string nombreArchivo = $"Factura_Mesa{mesa.Id}_{fecha}.txt";


            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RESTAURANTE LOS COMIDITAS");
            sb.AppendLine($"Factura de Mesa {mesa.Id}");
            sb.AppendLine($"Fecha: {fecha}");
            sb.AppendLine();
            sb.AppendLine("Platos consumidos:");
            sb.AppendLine();

            int total = 0;

            foreach (var kvp in comanda.Platos)
            {
                string nombre = kvp.Key.Nombre;
                int cantidad = kvp.Value;
                total += cantidad;

                sb.AppendLine($"- {nombre}  .......... x{cantidad}");
            }

            sb.AppendLine();
            sb.AppendLine($"TOTAL DE PLATOS: {total}");
            sb.AppendLine();
            sb.AppendLine("Gracias por su visita.");

            var dialog = new SaveFileDialog
            {
                Title = "Guardar factura",
                FileName = nombreArchivo,
                Filter = "Archivo de texto (*.txt)|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                File.WriteAllText(dialog.FileName, sb.ToString());
            }
        }

        private void SuscribirMesasSesion()
        {
            foreach (var m in mesasSuscritas.ToList())
            {
                m.PropertyChanged -= MesaSeleccionadaOnChanged;
            }
            mesasSuscritas.Clear();

            if (sesion?.Mesas == null) return;

            foreach (var m in sesion.Mesas)
            {
                m.PropertyChanged -= MesaSeleccionadaOnChanged;
                m.PropertyChanged += MesaSeleccionadaOnChanged;
                mesasSuscritas.Add(m);
            }
        }

        private int ObtenerPasoGuia(int max)
        {
            if (max <= 10) return 1;
            if (max <= 20) return 2;
            if (max <= 50) return 5;
            return 10;
        }

        #endregion
    }
}
