using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
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
using WPF_OPCUA_Client.Classes;
using WPF_OPCUA_Client.ViewModels;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WPF_OPCUA_Client.Views
{
    /// <summary>
    /// Логика взаимодействия для View1.xaml
    /// </summary>
    public partial class View1 : UserControl
    {
        private Button _activeButton;
        private DispatcherTimer _startStatusTimer;
        [DllImport("user32.dll")]
        static extern ushort GetKeyState(int nVirtKey);

        private const int VK_NUMLOCK = 0x90;
        public View1()
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
        private bool isMoving = false;

        // Метод: можно ли выполнять движение
        private bool CanMove()
        {
            short[] axisStates = new short[]
            {
                OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis1.nAxisState"),
                OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis2.nAxisState"),
                OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis3.nAxisState"),
                OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis4.nAxisState"),
                OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis5.nAxisState"),
            };

            return axisStates.All(state => state == 3);
        }
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsNumLockEnabled() || isMoving)
                return;

            if (!CanMove())
                return;

            isMoving = true;

            if (e.Key == Key.Home)
            {
                BtnUp1_PreviewMouseLeftButtonDown(BtnUp1, null);
                e.Handled = true;
            }
            else if (e.Key == Key.End)
            {
                BtnDown1_PreviewMouseLeftButtonDown(BtnDown1, null);
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp)
            {
                BtnUp2_PreviewMouseLeftButtonDown(BtnUp2, null);
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                BtnDown2_PreviewMouseLeftButtonDown(BtnDown2, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                BtnUp3_PreviewMouseLeftButtonDown(BtnUp3, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                BtnDown3_PreviewMouseLeftButtonDown(BtnDown3, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                BtnLeft1_PreviewMouseLeftButtonDown(BtnLeft1, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                BtnRight1_PreviewMouseLeftButtonDown(BtnRight1, null);
                e.Handled = true;
            }
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (IsNumLockEnabled())
                return;

            isMoving = false;

            if (e.Key == Key.Home)
            {
                BtnUp1_PreviewMouseLeftButtonUp(BtnUp1, null);
                e.Handled = true;
            }
            else if (e.Key == Key.End)
            {
                BtnDown1_PreviewMouseLeftButtonUp(BtnDown1, null);
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp)
            {
                BtnUp2_PreviewMouseLeftButtonUp(BtnUp2, null);
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                BtnDown2_PreviewMouseLeftButtonUp(BtnDown2, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                BtnUp3_PreviewMouseLeftButtonUp(BtnUp3, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                BtnDown3_PreviewMouseLeftButtonUp(BtnDown3, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                BtnLeft1_PreviewMouseLeftButtonUp(BtnLeft1, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                BtnRight1_PreviewMouseLeftButtonUp(BtnRight1, null);
                e.Handled = true;
            }
        }
        public void LoadCheckBoxStatesAndSendToServer()
        {
            var distanceData = LoadDataFromSection("Distance");

            foreach (var kvp in distanceData)
            {
                string key = kvp.Key;
                string value = kvp.Value;

                // Ожидается формат ключа: "CheckBox0", "CheckBox1", ...
                if (key.StartsWith("Checkbox") && int.TryParse(key.Replace("Checkbox", ""), out int index))
                {
                    bool isChecked = value == "1";
                    serverCheckBoxData[index] = isChecked;

                    // Устанавливаем состояние CheckBox в UI, если найден
                    if (FindName($"Checkbox{index}") is CheckBox checkBox)
                    {
                        checkBox.IsChecked = isChecked;

                        if (isChecked)
                            _lastCheckedCheckBox = checkBox;
                    }

                    // Отправляем значение на сервер
                    SendCheckBoxDataToServer(index);
                }
            }
        }

        private void StartStatusTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                bool startActive = CheckStartStatus();
                short currentMode = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.PLC_PRG.Modes");

                

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

               
               
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке статуса: {ex.Message}");
            }
        }
        public static Dictionary<string, string> LoadDataFromSection(string section)
        {
            var filePath = FileManager.GetFilePath();
            if (!File.Exists(filePath)) return new Dictionary<string, string>();

            var lines = File.ReadAllLines(filePath);
            Dictionary<string, string> result = new Dictionary<string, string>();

            bool inSection = false;
            foreach (var line in lines)
            {
                if (line == $"[{section}]")
                {
                    inSection = true;
                    continue;
                }
                if (inSection && line.StartsWith("["))
                {
                    break; // Выходим, если началась новая секция
                }
                if (inSection && line.Contains("="))
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        result[parts[0]] = parts[1];
                    }
                }
            }
            return result;
        }
        public static string LoadSingleValueFromSection(string section)
        {
            var filePath2 = FileManager.GetFilePath2();
            if (!File.Exists(filePath2)) return string.Empty;

            var lines = File.ReadAllLines(filePath2);
            bool inSection = false;

            foreach (var line in lines)
            {
                if (line == $"[{section}]")
                {
                    inSection = true;
                    continue;
                }
                if (inSection && line.StartsWith("["))
                {
                    break;
                }
                if (inSection && !string.IsNullOrWhiteSpace(line))
                {
                    return line.Trim();
                }
            }

            return string.Empty;
        }
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Проверка условий для кнопки 1 (переход возможен только если AutomatModes равен 1)
                if (!CheckStartStatus() )
                {
                    ((MainViewModel)Application.Current.MainWindow.DataContext).ShowView1();
                }
                else
                {
                    MessageBox.Show("Переход в ручной режим невозможен. Необходимо выключить предыдущий процесс");
                }
            }
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Проверка условий для кнопки 2 (переход возможен только если Start не активен и AutomatModes равен 2)
                if (!CheckStartStatus() )
                {
                    // Запись в OPC сервер
                    WriteToOpcServer(2);  // Записываем значение 2 в AutomatModes
                    ((MainViewModel)Application.Current.MainWindow.DataContext).ShowView2();
                }
                else
                {
                    MessageBox.Show("Переход в режим закалки вала невозможен. Необходимо выключить предыдущий процесс.");
                }
            }
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Проверка условий для кнопки 3 (переход возможен только если Start не активен и AutomatModes равен 3)
                if (!CheckStartStatus() )
                {
                    // Запись в OPC сервер
                    WriteToOpcServer(3);  // Записываем значение 3 в AutomatModes
                    ((MainViewModel)Application.Current.MainWindow.DataContext).ShowView3();
                }
                else
                {
                    MessageBox.Show("Переход в режим закалки шестерни невозможен. Необходимо выключить предыдущий процесс.");
                }
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,11]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,12]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,0]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,0]";
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

        private void BtnClr2_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,1]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,1]";
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

        private void BtnClr3_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,2]";
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
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcBtnHandler[1,2]";
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
        private bool[] serverCheckBoxData = new bool[7];
        private CheckBox _lastCheckedCheckBox; // Хранит последний выбранный CheckBox

        private int GetCheckBoxIndex(CheckBox checkBox)
        {
            var parentGrid = checkBox.Parent as Grid;
            if (parentGrid != null)
            {
                var checkBoxes = parentGrid.Children.OfType<CheckBox>().ToList();
                return checkBoxes.IndexOf(checkBox);
            }
            return -1;
        }

        private void AttachCheckBoxEvents(DependencyObject parent)
        {
            if (parent == null) return;

            if (parent is CheckBox checkBox)
            {
                checkBox.Checked += CheckBoxStateChanged;
                checkBox.Unchecked += CheckBoxStateChanged;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                AttachCheckBoxEvents(child);
            }
        }

        private void CheckBoxStateChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox currentCheckBox)
            {
                int checkBoxIndex = GetCheckBoxIndex(currentCheckBox);

                if (checkBoxIndex == -1) return;

                // Если CheckBox был отмечен
                if (currentCheckBox.IsChecked == true)
                {
                    // Снимаем выделение с предыдущего CheckBox
                    if (_lastCheckedCheckBox != null && _lastCheckedCheckBox != currentCheckBox)
                    {
                        _lastCheckedCheckBox.IsChecked = false;
                    }

                    _lastCheckedCheckBox = currentCheckBox;

                    // Обновляем массив - все false, кроме текущего
                    for (int i = 0; i < serverCheckBoxData.Length; i++)
                    {
                        serverCheckBoxData[i] = (i == checkBoxIndex);
                    }

                    // Отправляем данные на сервер
                    SendCheckBoxDataToServer(checkBoxIndex);
                }
                else // Если CheckBox был снят
                {
                    // Если снимаем текущий выбранный CheckBox
                    if (_lastCheckedCheckBox == currentCheckBox)
                    {
                        _lastCheckedCheckBox = null;
                        serverCheckBoxData[checkBoxIndex] = false;
                        SendCheckBoxDataToServer(checkBoxIndex);
                    }
                }
            }
        }

        private void SendCheckBoxDataToServer(int checkBoxIndex)
        {
            string tag = $"ns=4;s=|var|LS.Application.PLC_PRG.OpcArrChBxs2[{checkBoxIndex}]";
            bool value = serverCheckBoxData[checkBoxIndex];
            bool writeSuccess = OpcUaManager.OpcWrite(tag, value);

            if (!writeSuccess)
            {
               // MessageBox.Show("Ошибка записи на сервер.");
                return;
            }

            // Сохраняем в файл
            var distanceData = LoadDataFromSection2("Distance");

            // Можно использовать имя CheckBox, если нужно, иначе индекс
            string key = $"Checkbox{checkBoxIndex}";
            string valueStr = value ? "1" : "0";

            distanceData[key] = valueStr;
            SaveData("Distance", distanceData);
        }

        public static void SaveData(string section, Dictionary<string, string> data)
        {
            var filePath = FileManager.GetFilePath();
            var lines = File.Exists(filePath) ? File.ReadAllLines(filePath).ToList() : new List<string>();

            // Удаляем старые записи этой секции
            int startIndex = lines.FindIndex(line => line == $"[{section}]");
            if (startIndex >= 0)
            {
                int endIndex = lines.FindIndex(startIndex + 1, line => line.StartsWith("["));
                if (endIndex == -1) endIndex = lines.Count;
                lines.RemoveRange(startIndex, endIndex - startIndex);
            }

            // Добавляем новую секцию и её значения
            lines.Add($"[{section}]");
            foreach (var kvp in data)
            {
                lines.Add($"{kvp.Key}={kvp.Value}");
            }

            File.WriteAllLines(filePath, lines);
        }
        public static Dictionary<string, string> LoadDataFromSection2(string section)
        {
            var filePath = FileManager.GetFilePath();
            if (!File.Exists(filePath)) return new Dictionary<string, string>();

            var lines = File.ReadAllLines(filePath);
            Dictionary<string, string> result = new Dictionary<string, string>();

            bool inSection = false;
            foreach (var line in lines)
            {
                if (line == $"[{section}]")
                {
                    inSection = true;
                    continue;
                }
                if (inSection && line.StartsWith("["))
                {
                    break; // Выходим, если началась новая секция
                }
                if (inSection && line.Contains("="))
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        result[parts[0]] = parts[1];
                    }
                }
            }
            return result;
        }
        private void InitializeAllControlsMonitoring()
        {
            AttachCheckBoxEvents(AllZone);
            
        }

    }
    
}
