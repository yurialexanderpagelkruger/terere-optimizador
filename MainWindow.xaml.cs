using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace terere
{
    public partial class MainWindow : Window
    {
        private IntPtr _cursorPersonalizado = IntPtr.Zero;
        private bool _cargandoEstado = true;

        private static readonly string CarpetaConfig =
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "terere");

        private static readonly string ArchivoConfig =
            System.IO.Path.Combine(CarpetaConfig, "ventana.txt");

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            StateChanged += MainWindow_StateChanged;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RestaurarEstadoVentana();
            GenerarCorazones();
            CursorPersonalizadoRosa();
            AnimarBarraBrillo();
            _cargandoEstado = false;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            GuardarEstadoVentana();
        }

        private void RestaurarEstadoVentana()
        {
            try
            {
                if (!File.Exists(ArchivoConfig))
                    return;

                var lineas = File.ReadAllLines(ArchivoConfig);

                if (lineas.Length < 1)
                    return;

                string estado = lineas[0].Trim();

                if (estado == "Maximized")
                {
                    WindowState = WindowState.Maximized;
                }
                else if (estado == "Normal" && lineas.Length >= 5)
                {
                    if (double.TryParse(lineas[1], out double left)) Left = left;
                    if (double.TryParse(lineas[2], out double top)) Top = top;
                    if (double.TryParse(lineas[3], out double width)) Width = width;
                    if (double.TryParse(lineas[4], out double height)) Height = height;

                    WindowState = WindowState.Normal;
                }
            }
            catch { }
        }

        private void GuardarEstadoVentana()
        {
            try
            {
                if (!Directory.Exists(CarpetaConfig))
                    Directory.CreateDirectory(CarpetaConfig);

                if (WindowState == WindowState.Maximized)
                {
                    File.WriteAllLines(ArchivoConfig, new[] { "Maximized" });
                }
                else
                {
                    var datos = new[]
                    {
                        "Normal",
                        Left.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        Top.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        Height.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    };
                    File.WriteAllLines(ArchivoConfig, datos);
                }
            }
            catch { }
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (_cargandoEstado)
                return;

            if (WindowState == WindowState.Maximized)
            {
                MarcoPrincipal.CornerRadius = new CornerRadius(0);
                MarcoPrincipal.BorderThickness = new Thickness(0);
                MarcoPrincipal.Margin = new Thickness(0);
                MarcoPrincipal.Effect = null;

                AjustarAlMonitorActual();
            }
            else
            {
                MarcoPrincipal.CornerRadius = new CornerRadius(24);
                MarcoPrincipal.BorderThickness = new Thickness(1);
                MarcoPrincipal.Margin = new Thickness(6);
                MarcoPrincipal.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromArgb(255, 218, 24, 132),
                    BlurRadius = 35,
                    ShadowDepth = 0,
                    Opacity = 0.5
                };
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        private void AjustarAlMonitorActual()
        {
            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                IntPtr monitor = MonitorFromWindow(handle, 0x00000002);

                if (monitor == IntPtr.Zero)
                    return;

                var info = new MONITORINFO();
                info.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

                if (!GetMonitorInfo(monitor, ref info))
                    return;

                var dpi = VisualTreeHelper.GetDpi(this);

                double factorX = dpi.DpiScaleX > 0 ? dpi.DpiScaleX : 1.0;
                double factorY = dpi.DpiScaleY > 0 ? dpi.DpiScaleY : 1.0;

                double anchoLogico = (info.rcWork.Right - info.rcWork.Left) / factorX;
                double altoLogico = (info.rcWork.Bottom - info.rcWork.Top) / factorY;
                double leftLogico = info.rcWork.Left / factorX;
                double topLogico = info.rcWork.Top / factorY;

                Left = leftLogico;
                Top = topLogico;
                Width = anchoLogico;
                Height = altoLogico;
            }
            catch { }
        }

        private void GenerarCorazones()
        {
            var rnd = new Random();
            double ancho = Width;
            double alto = Height;

            for (int i = 0; i < 45; i++)
            {
                CrearCorazon(rnd, ancho, alto, true);
            }
        }

        private void CrearCorazon(Random rnd, double ancho, double alto, bool altoInicial)
        {
            double tam = rnd.NextDouble() * 22 + 16;

            var corazon = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 12,20 C 12,20 2,13 2,7 C 2,3 5,1 8,1 C 10,1 12,3 12,5 C 12,3 14,1 16,1 C 19,1 22,3 22,7 C 22,13 12,20 12,20 Z"),
                Fill = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(
                        (byte)(rnd.Next(120, 230)), 218, 24, 132)),
                Opacity = rnd.NextDouble() * 0.4 + 0.35,
                RenderTransformOrigin = new System.Windows.Point(0.5, 0.5),
                Stretch = System.Windows.Media.Stretch.Uniform,
                Width = tam,
                Height = tam
            };

            corazon.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Color.FromArgb(255, 218, 24, 132),
                BlurRadius = 22,
                ShadowDepth = 0,
                Opacity = 0.95
            };

            double posX = rnd.NextDouble() * ancho;
            double posY = altoInicial ? rnd.NextDouble() * alto : -tam * 2;

            Canvas.SetLeft(corazon, posX);
            Canvas.SetTop(corazon, posY);
            LienzoCorazones.Children.Add(corazon);

            double duracion = rnd.NextDouble() * 6 + 7;

            var caida = new DoubleAnimation
            {
                From = posY,
                To = alto + tam * 2,
                Duration = TimeSpan.FromSeconds(duracion),
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(rnd.NextDouble() * 3)
            };
            corazon.BeginAnimation(Canvas.TopProperty, caida);

            double giro = rnd.NextDouble() * 40 - 20;
            var rotacion = new DoubleAnimation
            {
                From = -giro,
                To = giro,
                Duration = TimeSpan.FromSeconds(rnd.NextDouble() * 3 + 2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            corazon.RenderTransform = new System.Windows.Media.RotateTransform();
            corazon.RenderTransform.BeginAnimation(
                System.Windows.Media.RotateTransform.AngleProperty, rotacion);

            var brillo = new DoubleAnimation
            {
                From = 0.35,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(rnd.NextDouble() * 2 + 1.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            corazon.BeginAnimation(UIElement.OpacityProperty, brillo);
        }

        private void CursorPersonalizadoRosa()
        {
            try
            {
                int tam = 32;

                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    var brochaCentro = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(255, 218, 24, 132));
                    var brochaBorde = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(200, 255, 255, 255));
                    var brochaHalo = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(90, 218, 24, 132));

                    dc.DrawEllipse(brochaHalo, null,
                        new System.Windows.Point(tam / 2.0, tam / 2.0),
                        tam / 2.0, tam / 2.0);

                    dc.DrawEllipse(brochaCentro, null,
                        new System.Windows.Point(tam / 2.0, tam / 2.0),
                        tam / 4.0, tam / 4.0);

                    dc.DrawEllipse(null,
                        new System.Windows.Media.Pen(brochaBorde, 1.5),
                        new System.Windows.Point(tam / 2.0, tam / 2.0),
                        tam / 2.0 - 2, tam / 2.0 - 2);
                }

                var bmp = new RenderTargetBitmap(tam, tam, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(visual);

                var codificador = new PngBitmapEncoder();
                codificador.Frames.Add(BitmapFrame.Create(bmp));

                using (var ms = new MemoryStream())
                {
                    codificador.Save(ms);
                    ms.Position = 0;

                    using (var bmp32 = new System.Drawing.Bitmap(tam, tam,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (var g = System.Drawing.Graphics.FromImage(bmp32))
                        {
                            g.DrawImage(System.Drawing.Image.FromStream(ms), 0, 0, tam, tam);
                        }

                        IntPtr hIcon = bmp32.GetHicon();
                        var infoCursor = new ICONINFO
                        {
                            fIcon = false,
                            hbmMask = IntPtr.Zero,
                            hbmColor = IntPtr.Zero
                        };
                        _cursorPersonalizado = CreateIconIndirect(ref infoCursor);
                        DestroyIcon(hIcon);
                    }
                }

                if (_cursorPersonalizado != IntPtr.Zero)
                {
                    Cursor = CursorInteropHelper.Create(new SafeCursorHandle(_cursorPersonalizado));
                }
            }
            catch
            {
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            public bool fIcon;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref ICONINFO iconInfo);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private class SafeCursorHandle : SafeHandle
        {
            public SafeCursorHandle(IntPtr handle) : base(IntPtr.Zero, true)
            {
                SetHandle(handle);
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle() => DestroyIcon(handle);
        }

        private void AnimarBarraBrillo()
        {
            var pulso = new DoubleAnimation
            {
                From = 0.6,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1.1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            BarraProgreso.BeginAnimation(UIElement.OpacityProperty, pulso);
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private async void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            Panel1.Visibility = Visibility.Collapsed;
            Panel2.Visibility = Visibility.Visible;

            ReiniciarProgreso();

            await Task.Run(async () =>
            {
                await EjecutarPasoAsync(
                    1,
                    "Limpiando archivos temporales...",
                    0, 25,
                    () =>
                    {
                        LimpiarTemporales();
                        VaciarPapelera();
                    });

                await EjecutarPasoAsync(
                    2,
                    "Reiniciando tu conexión de red...",
                    25, 55,
                    () => EjecutarComandoOculto("netsh winsock reset"));

                await EjecutarPasoAsync(
                    3,
                    "Limpiando el caché del navegador...",
                    55, 75,
                    () => EjecutarComandoOculto("ipconfig /flushdns"));

                await EjecutarPasoAsync(
                    4,
                    "Comprobando que todo funcione bien...",
                    75, 100,
                    () => EjecutarComandoOculto("sfc /scannow"));
            });

            await Task.Delay(600);

            Panel2.Visibility = Visibility.Collapsed;
            Panel1.Visibility = Visibility.Visible;
        }

        private void ReiniciarProgreso()
        {
            TxtEstado.Text = "Iniciando el proceso...";
            TxtPorcentaje.Text = "0 %";
            BarraProgreso.Width = 0;

            var gris = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(102, 255, 255, 255));

            IconoPaso1.Text = "○";
            IconoPaso1.Foreground = gris;
            IconoPaso2.Text = "○";
            IconoPaso2.Foreground = gris;
            IconoPaso3.Text = "○";
            IconoPaso3.Foreground = gris;
            IconoPaso4.Text = "○";
            IconoPaso4.Foreground = gris;
        }

        private async Task EjecutarPasoAsync(
            int numeroPaso,
            string textoEstado,
            double desde,
            double hasta,
            Action accion)
        {
            Dispatcher.Invoke(() =>
            {
                TxtEstado.Text = textoEstado;
                MarcarIconoActivo(numeroPaso);
            });

            double anchoTotal = 0;
            Dispatcher.Invoke(() => anchoTotal = ObtenerAnchoBarra());

            var tareaComando = Task.Run(accion);

            int totalIncrementos = 40;
            double pasoUnitario = (hasta - desde) / totalIncrementos;
            double actual = desde;

            for (int i = 1; i <= totalIncrementos; i++)
            {
                if (tareaComando.IsCompleted)
                    break;

                actual = desde + pasoUnitario * i;
                if (actual > hasta) actual = hasta;

                double valor = actual;
                Dispatcher.Invoke(() =>
                {
                    AnimarBarra(anchoTotal, valor, 0.18);
                    TxtPorcentaje.Text = ((int)valor) + " %";
                });

                await Task.Delay(120);
            }

            await tareaComando;

            double actualFinal = desde;
            Dispatcher.Invoke(() => actualFinal = ObtenerPorcentajeActual());

            double restante = hasta - actualFinal;
            if (restante > 0)
            {
                int pasosFinales = 12;
                for (int j = 1; j <= pasosFinales; j++)
                {
                    double val = actualFinal + (restante / pasosFinales) * j;
                    if (val > hasta) val = hasta;

                    double valor = val;
                    Dispatcher.Invoke(() =>
                    {
                        AnimarBarra(anchoTotal, valor, 0.12);
                        TxtPorcentaje.Text = ((int)valor) + " %";
                    });

                    await Task.Delay(70);
                }
            }

            Dispatcher.Invoke(() =>
            {
                AnimarBarra(anchoTotal, hasta, 0.2);
                TxtPorcentaje.Text = ((int)hasta) + " %";
                MarcarIconoCompletado(numeroPaso);
            });

            await Task.Delay(250);
        }

        private double ObtenerPorcentajeActual()
        {
            string texto = TxtPorcentaje.Text.Replace(" %", "").Trim();
            if (double.TryParse(texto, out double valor))
                return valor;
            return 0;
        }

        private void AnimarBarra(double anchoTotal, double porcentaje, double segundos)
        {
            double objetivo = anchoTotal * (porcentaje / 100.0);
            var anim = new DoubleAnimation
            {
                To = objetivo,
                Duration = TimeSpan.FromSeconds(segundos),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            BarraProgreso.BeginAnimation(FrameworkElement.WidthProperty, anim);
        }

        private double ObtenerAnchoBarra()
        {
            if (BarraProgreso.Parent is FrameworkElement contenedor)
            {
                return contenedor.ActualWidth;
            }
            return 550;
        }

        private void MarcarIconoActivo(int paso)
        {
            var rosa = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(255, 218, 24, 132));

            switch (paso)
            {
                case 1:
                    IconoPaso1.Text = "●";
                    IconoPaso1.Foreground = rosa;
                    break;
                case 2:
                    IconoPaso2.Text = "●";
                    IconoPaso2.Foreground = rosa;
                    break;
                case 3:
                    IconoPaso3.Text = "●";
                    IconoPaso3.Foreground = rosa;
                    break;
                case 4:
                    IconoPaso4.Text = "●";
                    IconoPaso4.Foreground = rosa;
                    break;
            }
        }

        private void MarcarIconoCompletado(int paso)
        {
            var verde = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(255, 90, 240, 160));

            switch (paso)
            {
                case 1:
                    IconoPaso1.Text = "✓";
                    IconoPaso1.Foreground = verde;
                    break;
                case 2:
                    IconoPaso2.Text = "✓";
                    IconoPaso2.Foreground = verde;
                    break;
                case 3:
                    IconoPaso3.Text = "✓";
                    IconoPaso3.Foreground = verde;
                    break;
                case 4:
                    IconoPaso4.Text = "✓";
                    IconoPaso4.Foreground = verde;
                    break;
            }
        }

        private void LimpiarTemporales()
        {
            string temp = System.IO.Path.GetTempPath();
            BorrarContenidoCarpeta(temp);

            string winTemp = @"C:\Windows\Temp";
            BorrarContenidoCarpeta(winTemp);
        }

        private void BorrarContenidoCarpeta(string ruta)
        {
            try
            {
                if (!Directory.Exists(ruta)) return;

                foreach (var archivo in Directory.GetFiles(ruta))
                {
                    try { File.Delete(archivo); } catch { }
                }

                foreach (var carpeta in Directory.GetDirectories(ruta))
                {
                    try { Directory.Delete(carpeta, true); } catch { }
                }
            }
            catch { }
        }

        private void VaciarPapelera()
        {
            try
            {
                SHEmptyRecycleBin(IntPtr.Zero, null, 0x00000007);
            }
            catch { }
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

        private void EjecutarComandoOculto(string comando)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c " + comando,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var proceso = Process.Start(psi))
                {
                    if (proceso != null)
                    {
                        proceso.OutputDataReceived += (s, e) => { };
                        proceso.ErrorDataReceived += (s, e) => { };
                        proceso.BeginOutputReadLine();
                        proceso.BeginErrorReadLine();
                        proceso.WaitForExit();
                    }
                }
            }
            catch { }
        }
    }
}
