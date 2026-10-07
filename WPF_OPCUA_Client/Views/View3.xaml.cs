using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPF_OPCUA_Client.ViewModels;

namespace WPF_OPCUA_Client.Views
{
    /// <summary>
    /// Логика взаимодействия для View3.xaml
    /// </summary>
    public partial class View3 : UserControl
    {
        [DllImport("user32.dll")]
        static extern ushort GetKeyState(int nVirtKey);

        private const int VK_NUMLOCK = 0x90;
        public View3()
        {
            InitializeComponent();
            InitializeUpdateTimer();
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
            // Индуктор вверх
            if (!IsNumLockEnabled() && e.Key == Key.Home)
            {
                BtnUp1_PreviewMouseLeftButtonDown(BtnUp1, null);
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
                BtnUp1_PreviewMouseLeftButtonUp(BtnUp1, null);
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

        private const string ToothCountTag = "ns=4;s=|var|LS.Application.PLC_PRG.CurrentTooth";
        private DispatcherTimer _updateTimer;
        private DispatcherTimer _startStatusTimer;
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
        private void InitializeUpdateTimer()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // Интервал опроса 500 мс
            };

            _updateTimer.Tick += (s, e) =>
            {
                try
                {
                    short currentTooth = OpcUaManager.OpcRead<short>(ToothCountTag);
                    ToothCount.Text = currentTooth.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка чтения тега: {ex.Message}");
                    ToothCount.Text = "ERROR";
                }
            };

            _updateTimer.Start();
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)System.Windows.Application.Current.MainWindow.DataContext;

            // Проверка условий для кнопки 1
            if (!CheckStartStatus())
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
            if (!CheckStartStatus())
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

        private bool CheckAutomatMode(short mode)
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
        private void BtnUp1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void BtnUp1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[2,11]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[2,12]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[2,0]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[2,0]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[2,1]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[2,1]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[2,2]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[2,2]";
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
        private Int16[] serverData = new Int16[15]; // Глобальный массив для записи на сервер
        private bool[] serverComboBoxData = new bool[3];
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
        private void AllZone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var allZone = sender as Grid;

                if (allZone == null)
                {
                    MessageBox.Show("Не удалось найти AllZone.");
                    return;
                }

                // Собираем все TextBox внутри AllZone
                var allTextBoxes = allZone
                    .Children
                    .OfType<FrameworkElement>()
                    .SelectMany(child => GetAllTextBoxes(child))
                    .ToList();

                Debug.WriteLine($"Всего TextBox: {allTextBoxes.Count}");

                bool hasNegative = false;
                short[] tempData = new short[serverData.Length];

                for (int i = 0; i < allTextBoxes.Count && i < serverData.Length; i++)
                {
                    var textBox = allTextBoxes[i];
                    try
                    {
                        short value = ConvertToInt16(textBox.Text);
                        tempData[i] = value;

                        if (value < 0)
                            hasNegative = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка обработки TextBox {i}: {ex.Message}");
                        return;
                    }
                }

                // Проверка условий:
                if (tempData.Length >= 2 && tempData[0] > tempData[1])
                {
                    MessageBox.Show("Начало зоны оси Z выше конца зоны. Процесс не будет начат, данные не внесены.", "Предупреждениеб", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (tempData.Length >= 10 && tempData[8] > tempData[9])
                {
                    MessageBox.Show("Начало зоны оси X выше конца зоны. Процесс не будет начат, данные не внесены.", "Предупреждениеб", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (hasNegative)
                {
                    MessageBox.Show("Вы внесли отрицательное значение, данные не были записаны. Процесс может начаться с ошибкой.",
                                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Если все проверки пройдены — записываем в OPC
                for (int i = 0; i < allTextBoxes.Count && i < serverData.Length; i++)
                {
                    var value = tempData[i];
                    serverData[i] = value;

                    string tag = $"ns=4;s=|var|LS.Application.PLC_PRG.OpcArr2[{i}]";
                    bool writeSuccess = OpcUaManager.OpcWrite(tag, value);

                    if (!writeSuccess)
                    {
                        MessageBox.Show($"Ошибка записи значения {value} в {tag}");
                        return;
                    }
                }

                MessageBox.Show("Все значения успешно записаны.");
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                // Получаем индекс ComboBox в массиве serverComboBoxData
                int comboBoxIndex = GetComboBoxGlobalIndex(comboBox);

                if (comboBoxIndex != -1 && comboBoxIndex < serverComboBoxData.Length)
                {
                    try
                    {
                        // Определяем значение в зависимости от выбранного элемента ComboBox

                        bool value = false;
                        if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                        {
                            string selectedText = selectedItem.Content.ToString().Trim().ToUpper();

                            value = selectedText switch
                            {
                                "ВКЛ." => true,
                                "ВЛЕВО" => true,
                                "ОТКЛ." => false,
                                "ВПРАВО" => false,
                                _ => throw new Exception($"Неподдерживаемое значение: {selectedItem.Content}")
                            };
                        }

                        // Обновляем массив по индексу ComboBox
                        serverComboBoxData[comboBoxIndex] = value;

                        // Формируем тэг для текущего элемента массива
                        string tag = $"ns=4;s=|var|LS.Application.PLC_PRG.OpcArrCmBxs2[{comboBoxIndex}]";

                        // Пишем обновлённый элемент на сервер
                        bool writeSuccess = OpcUaManager.OpcWrite(tag, value);

                        if (!writeSuccess)
                        {
                            MessageBox.Show($"Ошибка записи значения {value} в {tag}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обработке изменения значения ComboBox: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show($"ComboBox не найден или индекс находится за пределами массива. " +
                                    $"Найденный индекс: {comboBoxIndex}, Размер массива: {serverComboBoxData.Length}");
                }
            }
        }

        private int GetComboBoxGlobalIndex(ComboBox comboBox)
        {
            // Находим родительский Grid "AllZone"
            var allZone = FindParent<Grid>(comboBox);

            if (allZone != null)
            {
                // Собираем все ComboBox внутри AllZone, включая вложенные
                var allComboBoxes = allZone
                    .Children
                    .OfType<FrameworkElement>()
                    .SelectMany(child => GetAllComboBoxes(child))
                    .ToList();

                // Отладка: выводим количество найденных ComboBox
                Debug.WriteLine($"Найдено ComboBox: {allComboBoxes.Count}");

                // Возвращаем индекс текущего ComboBox
                return allComboBoxes.IndexOf(comboBox);
            }

            MessageBox.Show("Элемент AllZone не найден.");
            return -1; // ComboBox не найден
        }

        private IEnumerable<ComboBox> GetAllComboBoxes(FrameworkElement element)
        {
            if (element is ComboBox comboBox)
            {
                yield return comboBox;
            }
            else if (element is Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is FrameworkElement childElement)
                    {
                        foreach (var nestedComboBox in GetAllComboBoxes(childElement))
                        {
                            yield return nestedComboBox;
                        }
                    }
                }
            }
            else if (element is ContentControl contentControl && contentControl.Content is FrameworkElement contentElement)
            {
                foreach (var nestedComboBox in GetAllComboBoxes(contentElement))
                {
                    yield return nestedComboBox;
                }
            }
            else if (element is Border border && border.Child is FrameworkElement childElement)
            {
                foreach (var nestedComboBox in GetAllComboBoxes(childElement))
                {
                    yield return nestedComboBox;
                }
            }
        }

        private int GetTextBoxGlobalIndex(TextBox textBox)
        {
            // Находим родительский Grid "AllZone"
            var allZone = FindParent<Grid>(textBox);

            if (allZone != null)
            {
                // Собираем все TextBox внутри AllZone, включая вложенные
                var allTextBoxes = allZone
                    .Children
                    .OfType<FrameworkElement>()
                    .SelectMany(child => GetAllTextBoxes(child))
                    .ToList();

                // Отладка: выводим количество найденных TextBox
                Debug.WriteLine($"Найдено TextBox: {allTextBoxes.Count}");

                // Возвращаем индекс текущего TextBox
                return allTextBoxes.IndexOf(textBox);
            }

            MessageBox.Show("Элемент AllZone не найден.");
            return -1; // TextBox не найден
        }

        private IEnumerable<TextBox> GetAllTextBoxes(FrameworkElement element)
        {
            if (element is TextBox textBox)
            {
                yield return textBox;
            }
            else if (element is Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is FrameworkElement childElement)
                    {
                        foreach (var nestedTextBox in GetAllTextBoxes(childElement))
                        {
                            yield return nestedTextBox;
                        }
                    }
                }
            }
            else if (element is ContentControl contentControl && contentControl.Content is FrameworkElement contentElement)
            {
                foreach (var nestedTextBox in GetAllTextBoxes(contentElement))
                {
                    yield return nestedTextBox;
                }
            }
            else if (element is Border border && border.Child is FrameworkElement childElement)
            {
                foreach (var nestedTextBox in GetAllTextBoxes(childElement))
                {
                    yield return nestedTextBox;
                }
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        private readonly byte[] encryptionKey = Encoding.UTF8.GetBytes("1234567890123456"); // 16 байт = 128 бит ключ (замени на свой)

        private byte[] EncryptString(string plainText, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); // случайный IV

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    // Сначала пишем IV в начало потока
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, Encoding.UTF8))
                    {
                        sw.Write(plainText);
                    }

                    return ms.ToArray();
                }
            }
        }

        private string DecryptString(byte[] cipherData, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Первый блок байт — это IV
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(cipherData, 0, iv, 0, iv.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(cipherData, iv.Length, cipherData.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        private void SendLoadedDataToServer(string[] values)
        {
            try
            {
                int textBoxCount = serverData.Length;
                int comboBoxCount = serverComboBoxData.Length;

                // Считаем, что значения в values идут в правильном порядке:
                // TextBox0, TextBox1, ComboBox0, ComboBox1, TextBox2, ... как у тебя в формате

                int textBoxIndex = 0;
                int comboBoxIndex = 0;

                for (int i = 0; i < values.Length; i++)
                {
                    string val = values[i];

                    // Если это ComboBox - определяем по твоему формату, например по позиции или по содержимому
                    // Предположим, что ComboBox значения в values идут как "ВКЛ." или "ВПРАВО", а TextBox - числовые строки

                    if (IsComboBoxValue(val))
                    {
                        // Обработка ComboBox
                        if (comboBoxIndex >= comboBoxCount)
                            continue;

                        bool comboValue = val.Trim().ToUpper() switch
                        {
                            "ВКЛ." => true,
                            "ВЛЕВО" => true,
                            "ОТКЛ." => false,
                            "ВПРАВО" => false,
                            _ => throw new Exception($"Недопустимое значение ComboBox: {val}")
                        };


                        serverComboBoxData[comboBoxIndex] = comboValue;

                        string tag = $"ns=4;s=|var|LS.Application.PLC_PRG.OpcArrCmBxs2[{comboBoxIndex}]";
                        bool writeSuccess = OpcUaManager.OpcWrite(tag, comboValue);

                        if (!writeSuccess)
                        {
                            MessageBox.Show($"Ошибка записи ComboBox[{comboBoxIndex}] = {comboValue} в {tag}");
                            return;
                        }

                        comboBoxIndex++;
                    }
                    else
                    {
                        // Обработка TextBox (числовых значений)
                        if (textBoxIndex >= textBoxCount)
                            continue;

                        try
                        {
                            short textBoxValue = ConvertToInt16(val);
                            serverData[textBoxIndex] = textBoxValue;

                            string tag = $"ns=4;s=|var|LS.Application.PLC_PRG.OpcArr2[{textBoxIndex}]";
                            bool writeSuccess = OpcUaManager.OpcWrite(tag, textBoxValue);

                            if (!writeSuccess)
                            {
                                MessageBox.Show($"Ошибка записи TextBox[{textBoxIndex}] = {textBoxValue} в {tag}");
                                return;
                            }

                            textBoxIndex++;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка преобразования значения TextBox[{textBoxIndex}]: {ex.Message}");
                            return;
                        }
                    }
                }

                MessageBox.Show("Все данные успешно отправлены на сервер.", "Отправка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке данных на сервер: " + ex.Message);
            }
        }

        // Пример простой проверки, можно адаптировать под свои значения ComboBox
        private bool IsComboBoxValue(string val)
        {
            var comboBoxValues = new HashSet<string> { "ВКЛ.", "ОТКЛ.", "ВПРАВО", "ВЛЕВО" };
            return comboBoxValues.Contains(val);
        }




        private void NewReg_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement child in AllZone.Children)
            {
                // Для каждого дочернего элемента ищем вложенные TextBox и ComboBox
                var textBoxes = FindVisualChildren<TextBox>(child);
                foreach (var tb in textBoxes)
                {
                    tb.Text = "0";
                }

                var comboBoxes = FindVisualChildren<ComboBox>(child);
                foreach (var cb in comboBoxes)
                {
                    var items = cb.Items.OfType<ComboBoxItem>().Select(i => i.Content?.ToString().Trim().ToUpper()).ToList();

                    if (items.Contains("ОТКЛ."))
                    {
                        var offItem = cb.Items.OfType<ComboBoxItem>()
                            .FirstOrDefault(i => i.Content?.ToString().Trim().ToUpper() == "ОТКЛ.");
                        if (offItem != null)
                            cb.SelectedItem = offItem;
                    }
                    else if (items.Contains("ВЛЕВО"))
                    {
                        var leftItem = cb.Items.OfType<ComboBoxItem>()
                            .FirstOrDefault(i => i.Content?.ToString().Trim().ToUpper() == "ВЛЕВО");
                        if (leftItem != null)
                            cb.SelectedItem = leftItem;
                    }
                    else
                    {
                        cb.SelectedIndex = -1;
                    }
                }
            }

            MessageBox.Show("Все значения сброшены.", "Новый проект", MessageBoxButton.OK, MessageBoxImage.Information);
            ProjectName.Text = "Новый проект";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sb = new StringBuilder();

                // ОДИН РАЗ обходим всё дерево:
                var textBoxes = FindVisualChildren<TextBox>(AllZone).Distinct().ToList();
                var comboBoxes = FindVisualChildren<ComboBox>(AllZone).Distinct().ToList();

                foreach (var tb in textBoxes)
                    sb.Append(tb.Text + ";");

                foreach (var cb in comboBoxes)
                {
                    if (cb.SelectedItem is ComboBoxItem selectedItem)
                        sb.Append(selectedItem.Content.ToString() + ";");
                    else
                        sb.Append(";");
                }

                var plainText = sb.ToString().TrimEnd(';');
                var encrypted = EncryptString(plainText, encryptionKey);

                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Gear files (*.gear)|*.gear",
                    DefaultExt = ".gear",
                    FileName = "config"
                };

                if (dlg.ShowDialog() == true)
                {
                    File.WriteAllBytes(dlg.FileName, encrypted);
                    MessageBox.Show("Данные сохранены успешно.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                    ProjectName.Text = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void OpenReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Gear files (*.gear)|*.gear"
                };

                if (dlg.ShowDialog() == true)
                {
                    var encrypted = File.ReadAllBytes(dlg.FileName);
                    var decrypted = DecryptString(encrypted, encryptionKey);
                    var values = decrypted.Split(';');

                    int index = 0;

                    var textBoxes = FindVisualChildren<TextBox>(AllZone).Distinct().ToList();
                    var comboBoxes = FindVisualChildren<ComboBox>(AllZone).Distinct().ToList();

                    ProjectName.Text = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);

                    foreach (var tb in textBoxes)
                    {
                        if (index < values.Length)
                            tb.Text = values[index++];
                    }

                    foreach (var cb in comboBoxes)
                    {
                        if (index < values.Length)
                        {
                            var val = values[index++];
                            if (!string.IsNullOrEmpty(val))
                            {
                                var item = cb.Items.OfType<ComboBoxItem>()
                                    .FirstOrDefault(i => i.Content.ToString() == val);
                                if (item != null)
                                    cb.SelectedItem = item;
                                else
                                    cb.SelectedIndex = -1;
                            }
                            else
                            {
                                cb.SelectedIndex = -1;
                            }
                        }
                    }
                    if (values.Length > 18)
                        values = values.Take(18).ToArray();

                    SendLoadedDataToServer(values);

                   

                    MessageBox.Show("Настройки загружены.", "Загрузка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }
        private bool _suppressEvents = false;
        private void TeethTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_suppressEvents) return;

            if (int.TryParse(TeethTextBox.Text, out int teeth) && teeth > 0)
            {
                double anglePerTooth = 360.0 / teeth;
                int degrees = (int)anglePerTooth;
                int minutes = (int)Math.Round((anglePerTooth - degrees) * 60);

                _suppressEvents = true;
                DegreesTextBox.Text = degrees.ToString();
                MinutesTextBox.Text = minutes.ToString();
                _suppressEvents = false;
            }
        }

        private void AngleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_suppressEvents) return;

            bool isDegValid = int.TryParse(DegreesTextBox.Text, out int degrees);
            bool isMinValid = int.TryParse(MinutesTextBox.Text, out int minutes);

            if (isDegValid && isMinValid && (degrees > 0 || minutes > 0))
            {
                double angle = degrees + (minutes / 60.0);
                int teeth = (int)Math.Round(360.0 / angle);

                _suppressEvents = true;
                TeethTextBox.Text = teeth.ToString();
                _suppressEvents = false;
            }
        }

        
    }
}
