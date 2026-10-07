using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using WPF_OPCUA_Client.ViewModels;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace WPF_OPCUA_Client.Views
{
    /// <summary>
    /// Логика взаимодействия для View2.xaml
    /// </summary>
    public partial class View2 : UserControl
    {
        // KeyEventArgs enterPressedArgs = new KeyEventArgs(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0, Key.Enter);
        private DispatcherTimer _startStatusTimer;
        [DllImport("user32.dll")]
        static extern ushort GetKeyState(int nVirtKey);

        private const int VK_NUMLOCK = 0x90;
        public View2()
        {
           
            InitializeComponent();
            InitializeAllControlsMonitoring();
            _startStatusTimer = new DispatcherTimer();
            _startStatusTimer.Interval = TimeSpan.FromMilliseconds(500); // проверка 2 раза в секунду
            _startStatusTimer.Tick += StartStatusTimer_Tick;
            _startStatusTimer.Start();
            this.Loaded += (s, e) => {
                this.Focusable = true;
                this.Focus();
            };

            // Подписываемся на события клавиатуры
            this.PreviewKeyDown += UserControl_PreviewKeyDown;
            this.PreviewKeyUp += UserControl_PreviewKeyUp;
        }
        private bool IsNumLockEnabled()
        {
            return (GetKeyState(VK_NUMLOCK) & 0xFFFF) != 0;
        }
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsNumLockEnabled() && e.Key == Key.Home)
            {
                BtnUp1_MouseLeftButtonDown(BtnUp1, null);
                e.Handled = true;
            }
            // Индуктор вниз
            if (!IsNumLockEnabled() && e.Key == Key.End)
            {
                BtnDown1_PreviewMouseLeftButtonDown(BtnDown1, null);
                e.Handled = true;
            }

            if (!IsNumLockEnabled() && e.Key == Key.PageUp)
            {
                BtnUp2_PreviewMouseLeftButtonDown(BtnUp2, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.PageDown)
            {
                BtnDown2_PreviewMouseLeftButtonDown(BtnDown2, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.Up)
            {
                BtnUp3_PreviewMouseLeftButtonDown(BtnUp3, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.Down)
            {
                BtnDown3_PreviewMouseLeftButtonDown(BtnDown3, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.Left)
            {
                BtnLeft1_PreviewMouseLeftButtonDown(BtnLeft1, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.Right)
            {
                BtnRight1_PreviewMouseLeftButtonDown(BtnRight1, null);
                e.Handled = true;
            }
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!IsNumLockEnabled() && e.Key == Key.Home)
            {
                BtnUp1_MouseLeftButtonUp(BtnUp1, null);
                e.Handled = true;
            }

            if (!IsNumLockEnabled() && e.Key == Key.End)
            {
                BtnDown1_PreviewMouseLeftButtonUp(BtnDown1, null);
                e.Handled = true;
            }

            if (!IsNumLockEnabled() && e.Key == Key.PageUp)
            {
                BtnUp2_PreviewMouseLeftButtonUp(BtnUp2, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.PageDown)
            {
                BtnDown2_PreviewMouseLeftButtonUp(BtnDown2, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.Up)
            {
                BtnUp3_PreviewMouseLeftButtonUp(BtnUp3, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.Down)
            {
                BtnDown3_PreviewMouseLeftButtonUp(BtnDown3, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.Left)
            {
                BtnLeft1_PreviewMouseLeftButtonUp(BtnLeft1, null);
                e.Handled = true;
            }
            if (!IsNumLockEnabled() && e.Key == Key.Right)
            {
                BtnRight1_PreviewMouseLeftButtonUp(BtnRight1, null);
                e.Handled = true;
            }
        }
        private void StartStatusTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                bool startActive = CheckStartStatus();
                short currentMode = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.PLC_PRG.Modes");

                // Блокировка/разблокировка AllZone
                AllZone.IsEnabled = !startActive;
                AllZone.Opacity = startActive ? 0.5 : 1.0;
                
                // Блокировка кнопок при режиме 0, 2, 3
                bool restrictMode = currentMode == 0 || currentMode == 2 || currentMode == 3 || currentMode == 100;

                BtnClr1.IsEnabled = !restrictMode;
                BtnClr2.IsEnabled = !restrictMode;
                BtnClr3.IsEnabled = !restrictMode;
                BtnUp1.IsEnabled = !restrictMode;
                BtnDown1.IsEnabled = !restrictMode;
                BtnUp2.IsEnabled = !restrictMode;
                BtnDown2.IsEnabled = !restrictMode;
                BtnLeft1.IsEnabled = !restrictMode;
                BtnUp3.IsEnabled = !restrictMode;
                BtnDown3.IsEnabled = !restrictMode;
                BtnRight1.IsEnabled = !restrictMode;
                BtnH1.IsEnabled = !restrictMode;
                BtnZ1.IsEnabled = !restrictMode;

                // Дополнительная блокировка других кнопок при активном старте
                bool additionalLock = startActive;

                NewReg.IsEnabled = !additionalLock;
                btnSave.IsEnabled = !additionalLock;
                OpenReg.IsEnabled = !additionalLock;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке статуса: {ex.Message}");
            }
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)System.Windows.Application.Current.MainWindow.DataContext;

            // Проверка условий для кнопки 1
            if (!CheckStartStatus() )
            {
                viewModel.ShowView1();
            }
            else
            {
                // Уведомление пользователя или логирование
                MessageBox.Show("Переход в ручной режим невозможен. Необходимо выключить предыдущий процесс");
            }
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)System.Windows.Application.Current.MainWindow.DataContext;

            // Проверка условий для кнопки 2
            if (!CheckStartStatus() )
            {
                // Запись в OPC сервер
                WriteToOpcServer(2);  // Записываем значение 2 в AutomatModes
                viewModel.ShowView2();
            }
            else
            {
                // Уведомление пользователя или логирование
                MessageBox.Show("Переход в режим закалки вала невозможен. Необходимо выключить предыдущий процесс.");
            }
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)System.Windows.Application.Current.MainWindow.DataContext;

            // Проверка условий для кнопки 3
            if (!CheckStartStatus())
            {
                // Запись в OPC сервер
                WriteToOpcServer(3);  // Записываем значение 3 в AutomatModes
                viewModel.ShowView3();
            }
            else
            {
                // Уведомление пользователя или логирование
                MessageBox.Show("Переход в режим закалки шестерни невозможен. Необходимо выключить предыдущий процесс.");
            }
        }
        private bool CheckStartStatus()
        {
            // Проверка состояния тега Start из OPC сервера
            string tag = "ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.StartLed";
            var startStatus = OpcUaManager.OpcRead<bool>(tag);
            return startStatus;
        }

        private bool CheckAutomatMode(int mode)
        {
            // Проверка состояния тега AutomatModes из OPC сервера
            var automatMode = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.PLC_PRG.AutomatModes");
            return automatMode == mode;
        }

        private void WriteToOpcServer(short mode)
        {
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.AutomatModes";
            OpcUaManager.OpcWrite(tag, mode);
        }
        private void BtnClr1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnClr2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnClr3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnUp1_Click(object sender, RoutedEventArgs e)
        {
             
            
        }
        private void BtnUp1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,3]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrUpON.png"));
                    

                }
            }
        }
        private void BtnUp1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,3]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена

                    OpcUaManager.OpcWrite(tag, false);
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrUpOFF.png"));
                    

                }
            }
        }

        private void BtnDown1_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void BtnDown1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,4]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrDownON.png"));


                }
            }
        }

        private void BtnDown1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,4]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена

                    OpcUaManager.OpcWrite(tag, false);
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrDownOFF.png"));


                }
            }
        }

        private void BtnUp2_Click(object sender, RoutedEventArgs e)
        {
           
        }
        private void BtnUp2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,5]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrUpON.png"));


                }
            }
        }

        private void BtnUp2_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,5]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена

                    OpcUaManager.OpcWrite(tag, false);
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrUpOFF.png"));


                }
            }
        }
        private void BtnDown2_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void BtnDown2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,6]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrDownON.png"));


                }
            }
        }

        private void BtnDown2_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,6]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, false);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrDownOFF.png"));


                }
            }
        }

        private void BtnUp3_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void BtnUp3_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,7]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrUpON.png"));


                }
            }
        }

        private void BtnUp3_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,7]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, false);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrUpOFF.png"));


                }
            }
        }

        private void BtnDown3_Click(object sender, RoutedEventArgs e)
        {
           
        }
        private void BtnDown3_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,8]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrDownON.png"));


                }
            }
        }

        private void BtnDown3_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,8]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, false);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrDownOFF.png"));


                }
            }
        }
        private void BtnLeft1_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void BtnLeft1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,9]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrLeftON.png"));


                }
            }
        }

        private void BtnLeft1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,9]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, false);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrLeftOFF.png"));


                }
            }
        }
        private void BtnRight1_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void BtnRight1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,10]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrRightON.png"));


                }
            }
        }

        private void BtnRight1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,10]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, false);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/АrrRightOFF.png"));


                }
            }
        }

        private void BtnH1_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[0,11]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    if (imageBrush.ImageSource.ToString().Contains("BtnHOFF.png"))
                    {
                        OpcUaManager.OpcWrite(tag, true);
                        imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnHON.png"));
                    }
                    else
                    {
                        OpcUaManager.OpcWrite(tag, false);
                        imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnHOFF.png"));
                    }
                }
            }
        }

        private void BtnZ1_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[0,12]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    if (imageBrush.ImageSource.ToString().Contains("BtnZOFF.png"))
                    {
                        OpcUaManager.OpcWrite(tag, true);
                        imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnZON.png"));
                    }
                    else
                    {
                        OpcUaManager.OpcWrite(tag, false);
                        imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnZOFF.png"));
                    }
                }
            }
        }

        private void BtnClr1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[0,0]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена

                    OpcUaManager.OpcWrite(tag, true);
                   
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnClrOn.png"));


                }
            }
        }

        private void BtnClr1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[0,0]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена

                    OpcUaManager.OpcWrite(tag, false);
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnClrOFF.png"));


                }
            }
        }

        private void BtnClr2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[0,1]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnClrOn.png"));


                }
            }
        }

        private void BtnClr2_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[0,1]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, false);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnClrOFF.png"));


                }
            }
        }

        private void BtnClr3_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[0,2]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, true);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnClrOn.png"));


                }
            }
        }

        private void BtnClr3_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[0,2]";
            if (button != null)
            {
                var imageBrush = button.Background as ImageBrush;
                if (imageBrush != null)
                {
                    // Проверка текущего изображения и его замена
                    OpcUaManager.OpcWrite(tag, false);

                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/IMG/BtnClrOFF.png"));


                }
            }
        }

        // Метод для получения индекса StackPanel
        private int GetStackPanelIndex(DependencyObject element)
        {
            if (element == null) return -1;

            // Рекурсивно ищем ближайший StackPanel
            DependencyObject parent = element;
            while (parent != null && !(parent is StackPanel && VisualTreeHelper.GetParent(parent) == AllZone))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            // Если нашли StackPanel, определяем его индекс
            if (parent is StackPanel stackPanel)
            {
                return AllZone.Children.IndexOf(stackPanel);
            }

            return -1;
        }

        // Метод для обработки действий TextBox
        private Int16[] serverData = new Int16[8]; // Массив для TextBox внутри одного StackPanel
        private bool[,] serverCheckBoxData = new bool[20, 1];
        private bool[,] serverComboBoxData = new bool[20, 2];

        private void AllZone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string baseTag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcArr";
                int stackPanelIndex = 0;

                foreach (var panel in AllZone.Children)
                {
                    if (panel is StackPanel stackPanel)
                    {
                        Array.Clear(serverData, 0, serverData.Length);
                        int textBoxIndex = 0;
                        List<short> currentValues = new();

                        foreach (var child in stackPanel.Children)
                        {
                            if (child is TextBox textBox)
                            {
                                string rawText = textBox.Text?.Trim();
                                if (string.IsNullOrEmpty(rawText))
                                    continue;

                                if (!Int16.TryParse(rawText, out short value))
                                {
                                    MessageBox.Show($"Ошибка: введите число в поле {textBox.Name}");
                                    return;
                                }

                                // Проверка на отрицательное значение
                                if (value < 0)
                                {
                                    int zoneNum = stackPanelIndex + 1;
                                    MessageBox.Show(
                                        $"Вы ввели отрицательное значение в зоне {zoneNum}. Убедитесь что значение правильное, иначе оно может привести к " +
                                        "аварии. Данные не будут записаны".Replace("аварии", "АВАРИИ "),
                                        "Предупреждение",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                                    return;
                                }

                                if (textBoxIndex < serverData.Length)
                                    serverData[textBoxIndex] = value;

                                currentValues.Add(value);
                                textBoxIndex++;
                            }
                        }

                        //  Проверка: если первое значение больше второго
                        if (currentValues.Count >= 2 && currentValues[0] > currentValues[1])
                        {
                            int zoneNum = stackPanelIndex + 1;
                            MessageBox.Show(
                                $"Начало зоны {zoneNum} выше конца зоны, процесс может не начаться. Введите другие значения.",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Пишем в OPC UA
                        for (int i = 0; i < serverData.Length; i++)
                        {
                            string tag = $"{baseTag}[{stackPanelIndex},{i}]";
                            bool writeSuccess = OpcUaManager.OpcWrite(tag, serverData[i]);

                            if (!writeSuccess)
                            {
                                //MessageBox.Show($"Ошибка записи в {tag}");
                                return;
                            }
                        }

                        stackPanelIndex++;
                    }
                }
            }
        }


        private int GetTextBoxIndex(TextBox textBox)
        {
            // Находим родительский StackPanel
            var parentStackPanel = textBox.Parent as StackPanel;

            if (parentStackPanel != null)
            {
                // Собираем все TextBox в текущем StackPanel
                var textBoxes = parentStackPanel.Children.OfType<TextBox>().ToList();

                // Возвращаем индекс текущего TextBox в списке
                return textBoxes.IndexOf(textBox);
            }
            return -1; // Не найден
        }
        private int GetCheckBoxIndex(CheckBox checkBox)
        {
            var parentStackPanel = checkBox.Parent as StackPanel;
            if (parentStackPanel != null)
            {
                var checkBoxes = parentStackPanel.Children.OfType<CheckBox>().ToList();
                return checkBoxes.IndexOf(checkBox);
            }
            return -1;
        }

        // Метод для получения индекса ComboBox в StackPanel
        private int GetComboBoxIndex(ComboBox comboBox)
        {
            var parentStackPanel = comboBox.Parent as StackPanel;
            if (parentStackPanel != null)
            {
                var comboBoxes = parentStackPanel.Children.OfType<ComboBox>().ToList();
                return comboBoxes.IndexOf(comboBox);
            }
            return -1;
        }
      



        // Метод для добавления обработчиков событий ко всем CheckBox внутри AllZone
        private void AttachCheckBoxEvents(DependencyObject parent)
        {
            if (parent == null) return;

            // Если текущий элемент - CheckBox, добавляем обработчики
            if (parent is CheckBox checkBox)
            {
                checkBox.Checked += CheckBoxStateChanged;
                checkBox.Unchecked += CheckBoxStateChanged;
            }

            // Рекурсивно обходим дочерние элементы
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                AttachCheckBoxEvents(child);
            }
        }

        // Обработчик изменения состояния CheckBox
        private void CheckBoxStateChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                int stackPanelIndex = GetStackPanelIndex(checkBox);
                int checkBoxIndex = GetCheckBoxIndex(checkBox);

                if (stackPanelIndex != -1 && checkBoxIndex != -1)
                {
                    // Обновляем значение для CheckBox в массиве
                    serverCheckBoxData[stackPanelIndex, checkBoxIndex] = checkBox.IsChecked == true;

                    // Отправляем данные на сервер
                    SendCheckBoxDataToServer(stackPanelIndex, checkBoxIndex);
                }
            }
        }
        private void SendCheckBoxDataToServer(int stackPanelIndex, int checkBoxIndex)
        {
            string tag = $"ns=4;s=|var|LS.Application.PLC_PRG.OpcArrChBxs[{stackPanelIndex},{checkBoxIndex}]";
            bool writeSuccess = OpcUaManager.OpcWrite(tag, serverCheckBoxData[stackPanelIndex, checkBoxIndex]);

            if (writeSuccess)
            {
                //MessageBox.Show("SENT Check");
            }
            else
            {
                MessageBox.Show("Ошибка записи на сервер.");
            }
        }

        // Метод для привязки обработчиков событий ко всем ComboBox внутри AllZone
        private void AttachComboBoxEvents(DependencyObject parent)
        {
            if (parent == null) return;

            // Если текущий элемент - ComboBox, добавляем обработчик
            if (parent is ComboBox comboBox)
            {
                comboBox.SelectionChanged += ComboBoxStateChanged;
            }

            // Рекурсивно обходим дочерние элементы
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                AttachComboBoxEvents(child);
            }
        }

        // Обработчик изменения состояния ComboBox
        private void ComboBoxStateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                var selectedItem = comboBox.SelectedItem as ComboBoxItem;

                if (selectedItem != null)
                {
                    bool state = selectedItem.Content.ToString() == "ВКЛ.";
                    int stackPanelIndex = GetStackPanelIndex(comboBox);
                    int comboBoxIndex = GetComboBoxIndex(comboBox);

                    if (stackPanelIndex != -1 && comboBoxIndex != -1)
                    {
                        // Обновляем значение в массиве
                        serverComboBoxData[stackPanelIndex, comboBoxIndex] = state;

                        // Отправляем данные на сервер
                        SendComboBoxDataToServer(stackPanelIndex, comboBoxIndex);
                    }
                }
            }
        }
        private void SendComboBoxDataToServer(int stackPanelIndex, int comboBoxIndex)
        {
            string tag = $"ns=4;s=|var|LS.Application.PLC_PRG.OpcArrCmBxs[{stackPanelIndex},{comboBoxIndex}]";
            bool writeSuccess = OpcUaManager.OpcWrite(tag, serverComboBoxData[stackPanelIndex, comboBoxIndex]);

            if (writeSuccess)
            {
                //MessageBox.Show("SENT Combo");
            }
            else
            {
                MessageBox.Show("Ошибка записи на сервер.");
            }
        }

        // Метод для конвертации строки в Int16 с дефолтным значением 0
        private Int16 ConvertToInt16(string text)
        {
            if (Int16.TryParse(text, out Int16 result))
            {
                return result;
            }
            else
            {
                return 0; // Если конвертация не удалась, возвращаем 0
            }
        }

        // Инициализация обработчиков для всех элементов внутри AllZone
        private void InitializeAllControlsMonitoring()
        {
            AttachCheckBoxEvents(AllZone);
            AttachComboBoxEvents(AllZone);
            
        }
        private byte[] EncryptString(string plainText, string password = "opc_secure_key")
        {
            byte[] salt = Encoding.UTF8.GetBytes("static_salt");
            var key = new Rfc2898DeriveBytes(password, salt, 10000);
            using var aes = Aes.Create();
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);
            sw.Write(plainText);
            sw.Close();
            return ms.ToArray();
        }

        private string DecryptBytes(byte[] cipherBytes, string password = "opc_secure_key")
        {
            byte[] salt = Encoding.UTF8.GetBytes("static_salt");
            var key = new Rfc2898DeriveBytes(password, salt, 10000);
            using var aes = Aes.Create();
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }


        private void NewReg_Click(object sender, RoutedEventArgs e)
        {
            int stackPanelIndex = 0;

            foreach (var panel in AllZone.Children)
            {
                if (panel is StackPanel stackPanel)
                {
                    // Очистка TextBox'ов
                    foreach (var textBox in stackPanel.Children.OfType<TextBox>())
                    {
                        textBox.Text = "0";
                    }

                    // Очистка CheckBox'ов
                    foreach (var checkBox in stackPanel.Children.OfType<CheckBox>())
                    {
                        checkBox.IsChecked = false;
                    }

                    // Установка ComboBox в "ОТКЛ."
                    foreach (var comboBox in stackPanel.Children.OfType<ComboBox>())
                    {
                        var items = comboBox.Items.OfType<ComboBoxItem>().ToList();
                        var defaultItem = items.FirstOrDefault(item => item.Content?.ToString() == "ОТКЛ.");
                        if (defaultItem != null)
                        {
                            comboBox.SelectedItem = defaultItem;
                        }
                        else
                        {
                            // Если нет "ОТКЛ.", снимаем выбор
                            comboBox.SelectedIndex = -1;
                        }

                        // Обновление значения в массиве
                        int comboBoxIndex = GetComboBoxIndex(comboBox);
                        if (comboBoxIndex != -1)
                        {
                            serverComboBoxData[stackPanelIndex, comboBoxIndex] = false;
                            SendComboBoxDataToServer(stackPanelIndex, comboBoxIndex);
                        }
                    }

                    stackPanelIndex++;
                }
            }

            MessageBox.Show("Создан новый проект. Все значения сброшены.", "Новый проект", MessageBoxButton.OK, MessageBoxImage.Information);
            ProjectName.Text = "Новый проект";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder data = new();

            int stackPanelIndex = 0;
            foreach (var panel in AllZone.Children)
            {
                if (panel is StackPanel stackPanel)
                {
                    // Сохраняем TextBox значения
                    int textBoxIndex = 0;
                    foreach (var child in stackPanel.Children.OfType<TextBox>())
                    {
                        string value = child.Text.Trim();
                        data.AppendLine($"TXT[{stackPanelIndex},{textBoxIndex}]={value}");
                        textBoxIndex++;
                    }

                    // Сохраняем CheckBox состояния
                    int checkBoxIndex = 0;
                    foreach (var child in stackPanel.Children.OfType<CheckBox>())
                    {
                        bool? state = ((CheckBox)child).IsChecked;
                        data.AppendLine($"CHB[{stackPanelIndex},{checkBoxIndex}]={(state == true ? 1 : 0)}");
                        checkBoxIndex++;
                    }

                    // Сохраняем ComboBox состояния
                    int comboBoxIndex = 0;
                    foreach (var child in stackPanel.Children.OfType<ComboBox>())
                    {
                        string? selected = ((ComboBox)child).SelectedItem is ComboBoxItem item ? item.Content.ToString() : "";
                        data.AppendLine($"CMB[{stackPanelIndex},{comboBoxIndex}]={selected}");
                        comboBoxIndex++;
                    }

                    stackPanelIndex++;
                }
            }

            SaveFileDialog sfd = new()
            {
                Filter = "Encrypted Values (*.val)|*.val",
                DefaultExt = ".val"
            };

            if (sfd.ShowDialog() == true)
            {
                var encrypted = EncryptString(data.ToString());
                File.WriteAllBytes(sfd.FileName, encrypted);
                MessageBox.Show("Файл успешно сохранён.");
                ProjectName.Text = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
            }
        }

        private void OpenReg_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "Encrypted Values (*.val)|*.val",
                DefaultExt = ".val"
            };

            if (ofd.ShowDialog() == true)
            {
                var encrypted = File.ReadAllBytes(ofd.FileName);
                string decrypted;
                try
                {
                    decrypted = DecryptBytes(encrypted);
                }
                catch
                {
                    MessageBox.Show("Ошибка дешифровки. Возможно, файл повреждён или использован другой ключ.");
                    return;
                }
                ProjectName.Text = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                var lines = decrypted.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    if (line.StartsWith("TXT["))
                    {
                        var match = Regex.Match(line, @"TXT\[(\d+),(\d+)\]=(.*)");
                        int i = int.Parse(match.Groups[1].Value);
                        int j = int.Parse(match.Groups[2].Value);
                        string val = match.Groups[3].Value;
                        if (string.IsNullOrWhiteSpace(val))
                            val = "0";

                        if (AllZone.Children[i] is StackPanel sp)
                        {
                            var textBoxes = sp.Children.OfType<TextBox>().ToList();
                            if (j < textBoxes.Count)
                            {
                                textBoxes[j].Text = val;

                                // Попробуем преобразовать и отправить значение на OPC-сервер
                                if (Int16.TryParse(val, out short parsedVal))
                                {
                                    string tag = $"ns=4;s=|var|LS.Application.PLC_PRG.OpcArr[{i},{j}]";
                                    OpcUaManager.OpcWrite(tag, parsedVal);
                                }
                            }
                        }
                    }
                    else if (line.StartsWith("CHB["))
                    {
                        var match = Regex.Match(line, @"CHB\[(\d+),(\d+)\]=([01])");
                        int i = int.Parse(match.Groups[1].Value);
                        int j = int.Parse(match.Groups[2].Value);
                        bool state = match.Groups[3].Value == "1";

                        if (AllZone.Children[i] is StackPanel sp)
                        {
                            var checkBoxes = sp.Children.OfType<CheckBox>().ToList();
                            if (j < checkBoxes.Count)
                            {
                                checkBoxes[j].IsChecked = state;

                                // Обновляем массив и отправляем
                                serverCheckBoxData[i, j] = state;
                                SendCheckBoxDataToServer(i, j);
                            }
                        }
                    }
                    else if (line.StartsWith("CMB["))
                    {
                        var match = Regex.Match(line, @"CMB\[(\d+),(\d+)\]=(.*)");
                        if (match.Success &&
                            int.TryParse(match.Groups[1].Value, out int i) &&
                            int.TryParse(match.Groups[2].Value, out int j))
                        {
                            string selected = match.Groups[3].Value;

                            if (AllZone.Children[i] is StackPanel sp)
                            {
                                var comboBoxes = sp.Children.OfType<ComboBox>().ToList();
                                if (j < comboBoxes.Count)
                                {
                                    var items = comboBoxes[j].Items.OfType<ComboBoxItem>();
                                    foreach (var item in items)
                                    {
                                        if (item.Content?.ToString() == selected)
                                        {
                                            comboBoxes[j].SelectedItem = item;

                                            // Обновляем массив и отправляем
                                            bool state = selected == "ВКЛ.";
                                            serverComboBoxData[i, j] = state;
                                            SendComboBoxDataToServer(i, j);

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Некорректная строка: {line}", "Ошибка разбора", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }

                MessageBox.Show("Файл успешно загружен и применён.");
            }
        }
    }
}

