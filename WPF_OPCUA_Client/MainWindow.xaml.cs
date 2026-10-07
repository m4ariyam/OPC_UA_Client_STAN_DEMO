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
using System.Windows.Threading;
using WPF_OPCUA_Client.ViewModels;
using WPF_OPCUA_Client.Views;
using OpcUaHelper;
using Opc.Ua.Client;
using System.ComponentModel.DataAnnotations;

namespace WPF_OPCUA_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private Button _activeButton;
        private int _lastActiveView = -1;
        OpcUaManager opcManager = new OpcUaManager();

        public MainWindow()
        {

            // Пример чтения данных
            try
            {
                OpcUaManager.Connect("opc.tcp://192.168.1.3:4840");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении данных: {ex.Message}");
            }
            InitializeComponent();
            InitializeClock();
            InitializeStartView();
          

        }

        private void ToggleView(Button button, string activeImageUri, string inactiveImageUri, Action changeViewAction)
        {
            // Сбросить состояние всех кнопок, если активная есть
            if (_activeButton != null)
            {
                ResetButtonState(_activeButton);
            }

            if (_activeButton == button)
            {
                // Если нажата та же кнопка, сбросить её
                ResetButtonState(button);
                _activeButton = null;
                ((MainViewModel)Application.Current.MainWindow.DataContext).ShowInitialView();
            }
            else
            {
                // Установить новую активную кнопку
                SetButtonState(button, activeImageUri);
                _activeButton = button;

                // Выполнить действие смены представления
                changeViewAction?.Invoke();
            }
        }

        // Метод для сброса состояния кнопки
        private void ResetButtonState(Button button)
        {
            if (button == null) return;

            // Сброс изображения кнопки на "неактивное"
            var buttonName = button.Name.ToLower();
            string inactiveImageUri = buttonName switch
            {
                "btn1" => "pack://application:,,,/IMG/Button РАБОТА OFF.png",
                "btn2" => "pack://application:,,,/IMG/Button Диагност OFF.png",
                "btn3" => "pack://application:,,,/IMG/Button Настройка OFF.png",
                "btn4" => "pack://application:,,,/IMG/Button История OFF.png",
                "btn5" => "pack://application:,,,/IMG/Button Инфо OFF.png",
                _ => null
            };

            if (!string.IsNullOrEmpty(inactiveImageUri))
            {
                var brush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(inactiveImageUri, UriKind.Absolute))
                };
                button.Background = brush;
            }
        }

        // Метод для установки активного состояния кнопки
        private void SetButtonState(Button button, string activeImageUri)
        {
            if (button == null) return;

            // Установить изображение кнопки на "активное"
            var brush = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(activeImageUri, UriKind.Absolute))
            };
            button.Background = brush;
        }
        private void InitializeStartView()
        {
            
            var startButton = btn1; 

            if (startButton != null)
            {
                // Установить кнопку активной и показать View2
                SetButtonState(startButton, "pack://application:,,,/IMG/Button РАБОТА ON.png");
                _activeButton = startButton;

                // Установить начальное представление
                ((MainViewModel)Application.Current.MainWindow.DataContext).ShowLastActiveView();
            }
        }
        

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            
            ToggleView(
               sender as Button,
               "pack://application:,,,/IMG/Button РАБОТА ON.png",
               "pack://application:,,,/IMG/Button РАБОТА OFF.png",
               () => ((MainViewModel)Application.Current.MainWindow.DataContext).ShowLastActiveView()
           );
            _lastActiveView = 2;
        }
        

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            ToggleView(
                sender as Button,
                "pack://application:,,,/IMG/Button Диагност ON.png",
                "pack://application:,,,/IMG/Button Диагност OFF.png",
                () => ((MainViewModel)Application.Current.MainWindow.DataContext).ShowView4()
            );
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            ToggleView(
                sender as Button,
                "pack://application:,,,/IMG/Button Настройка ON.png",
                "pack://application:,,,/IMG/Button Настройка OFF.png",
                () => ((MainViewModel)Application.Current.MainWindow.DataContext).ShowView5()
            );
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            ToggleView(
                sender as Button,
                "pack://application:,,,/IMG/Button История ON.png",
                "pack://application:,,,/IMG/Button История OFF.png",
                () => ((MainViewModel)Application.Current.MainWindow.DataContext).ShowView6()
            );
        }

        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            ToggleView(
                sender as Button,
                "pack://application:,,,/IMG/Button Инфо ON.png",
                "pack://application:,,,/IMG/Button Инфо OFF.png",
                () => ((MainViewModel)Application.Current.MainWindow.DataContext).ShowView7()
            );
        }


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void InitializeClock()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateText.Foreground = Brushes.White;
            TimeText.Foreground = Brushes.White;
            DateText.Text = DateTime.Now.ToString("dd MMMM yyyy");
            TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

    }
}
