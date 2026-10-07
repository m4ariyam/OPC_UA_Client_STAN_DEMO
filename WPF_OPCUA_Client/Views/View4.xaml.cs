using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace WPF_OPCUA_Client.Views
{
    /// <summary>
    /// Логика взаимодействия для View4.xaml
    /// </summary>
    public partial class View4 : UserControl
    {
        private DispatcherTimer _updateTimer;

        public View4()
        {
            InitializeComponent();

            // Запускаем таймер для обновления каждые 500 мс
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _updateTimer.Tick += async (s, e) => await ReadAllTagsAsync();
            _updateTimer.Start();
        }

        private async Task ReadAllTagsAsync()
        {
            try
            {
                // Примеры чтения булевых тегов
                UpdateLimitTextBlock("LimitIndForw", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitIndForw"));
                UpdateLimitTextBlock("LimitIndBack", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitIndBack"));
                UpdateLimitTextBlock("LimitIndLeft", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitIndLeft"));
                UpdateLimitTextBlock("LimitIndRight", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitIndRight"));
                UpdateLimitTextBlock("LimitIndUp", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitIndUp"));
                UpdateLimitTextBlock("LimitIndDown", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitIndDown"));
                UpdateLimitTextBlock("LimitClampDown", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitClampDown"));
                UpdateLimitTextBlock("LimitClampUp", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitClampUp"));
                UpdateLimitTextBlock("LimitIndDown2", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LimitClampDown"));
                UpdateLimitTextBlock("LowerLimit", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.LowerLimit"));
                UpdateLimitTextBlock("MiddleLimit", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.MiddleLimit"));
                UpdateLimitTextBlock("HighestLimit", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.HighestLimit"));

                var axis2Comm = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis2.bCommunication");
                var axis3Comm = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis3.bCommunication");
                var axis4Comm = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis4.bCommunication");
                var axis5Comm = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis5.bCommunication");
                var axis1Comm = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis1.bCommunication");

                UpdateAxisConnectionTextBlock("Axis2Connection", axis2Comm);
                UpdateAxisConnectionTextBlock("Axis3Connection", axis3Comm);
                UpdateAxisConnectionTextBlock("Axis4Connection", axis4Comm);
                UpdateAxisConnectionTextBlock("Axis5Connection", axis5Comm);
                UpdateAxisConnectionTextBlock("Axis1Connection", axis1Comm);

                UpdateBoolTextBlockYesNo("ClampUp", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.ClampUp"));
                UpdateBoolTextBlockYesNo("ClampDowm", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.ClampDown"));

                UpdateBoolTextBlockYesNo("IndUp", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndUp"));
                UpdateBoolTextBlockYesNo("IndDown", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndDown"));

                UpdateBoolTextBlockYesNo("IndLeft", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndLeft"));
                UpdateBoolTextBlockYesNo("IndRight", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndRight"));

                UpdateBoolTextBlockYesNo("IndForw", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndForw"));
                UpdateBoolTextBlockYesNo("IndBack", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndBack"));

                UpdateBoolTextBlockYesNo("RotLeft", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.Rotleft"));
                UpdateBoolTextBlockYesNo("RotRight", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.RotRight"));

                UpdateBoolTextBlockYesNo("Sprayer", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.Sprayer"));
                UpdateBoolTextBlockYesNo("Automat", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.Automat"));
                UpdateBoolTextBlockYesNo("Start", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.Start"));
                UpdateBoolTextBlockYesNo("Stop", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.StopHeater"));
                UpdateBoolTextBlockYesNo("ProgrammBlock", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.ProgrammBlock"));

                UpdateDoubleTextBlock("Axis4Vel", Math.Abs(OpcUaManager.OpcRead<double>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis4.fActVelocity") * 60));
                UpdateDoubleTextBlock("Axis5Vel", Math.Abs(OpcUaManager.OpcRead<double>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis5.fActVelocity") * 60));
                UpdateDoubleTextBlock("Axis3Vel", Math.Abs(OpcUaManager.OpcRead<double>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis3.fActVelocity") * 60));
                UpdateDoubleTextBlock("Axis2Vel", Math.Abs(OpcUaManager.OpcRead<double>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis2.fActVelocity") * 60));
                UpdateDoubleTextBlock("Axis1Vel", OpcUaManager.OpcRead<float>("ns=4;s=|var|LS.Application.PLC_PRG.CurrentRotSpeed") * 60 / 7);
                // Здесь ты можешь, например, обновить UI или передать данные в ViewModel
                UpdateBoolTextBlockInverted("ProtokInd", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.ProtokInd"));
                UpdateBoolTextBlockInverted("ProtokPCh", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.ProtokPCh"));
                UpdateBoolTextBlockInverted("ProtokTSU", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.ProtokTSU"));
                UpdateUInt16TextBlock("VprPch", "ns=4;s=|var|LS.Application.PLC_PRG.Read[18]");
                UpdateAxisConnectionTextBlock("HeaterConnection", OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.bLinkStable"));

            }
            catch (Exception ex)
            {
                // Обработка ошибок OPC UA
                Console.WriteLine($"Ошибка чтения OPC UA: {ex.Message}");
            }
        }
        private void UpdateDoubleTextBlock(string textBlockName, double value, string format = "F2")
        {
             
            Application.Current.Dispatcher.Invoke(() =>
            {
                var textBlock = this.FindName(textBlockName) as TextBlock;
                if (textBlock != null)
                {
                    textBlock.Text = value.ToString(format, CultureInfo.InvariantCulture);
                    textBlock.Foreground = Brushes.Black;
                }
            });
        }

        private void UpdateLimitTextBlock(string textBlockName, bool isClosed)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var textBlock = this.FindName(textBlockName) as TextBlock;
                if (textBlock != null)
                {
                    if (isClosed)
                    {
                        textBlock.Text = "Замкнут";
                        textBlock.Foreground = Brushes.Red;
                    }
                    else
                    {
                        textBlock.Text = "Разомкнут";
                        textBlock.Foreground = Brushes.Black;
                    }
                }
            });
        }
        private void UpdateAxisConnectionTextBlock(string textBlockName, bool isConnected)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var textBlock = this.FindName(textBlockName) as TextBlock;
                if (textBlock != null)
                {
                    if (isConnected)
                    {
                        textBlock.Text = "Подключен";
                        textBlock.Foreground = Brushes.Green;
                    }
                    else
                    {
                        textBlock.Text = "Отключен";
                        textBlock.Foreground = Brushes.Red;
                    }
                }
            });
        }
        private void UpdateBoolTextBlockInverted(string textBlockName, bool tagValue)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var textBlock = this.FindName(textBlockName) as TextBlock;
                if (textBlock != null)
                {
                    bool bLinkStable = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.bLinkStable");

                    if (!bLinkStable)
                    {
                        textBlock.Text = "нет данных";
                        textBlock.Foreground = Brushes.Red;
                    }
                    else
                    {
                        if (tagValue)
                        {
                            textBlock.Text = "Нет";
                            textBlock.Foreground = Brushes.Red;
                        }
                        else
                        {
                            textBlock.Text = "Есть";
                            textBlock.Foreground = Brushes.Black;
                        }
                    }
                }
            });
        }
        private void UpdateUInt16TextBlock(string textBlockName, string tagNodeId)
        {
            try
            {
                ushort value = OpcUaManager.OpcRead<ushort>(tagNodeId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var textBlock = this.FindName(textBlockName) as TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Text = value.ToString();
                        textBlock.Foreground = Brushes.Black;
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения UInt16 тега: {ex.Message}");
            }
        }

        private void UpdateBoolTextBlockYesNo(string textBlockName, bool isActive)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var textBlock = this.FindName(textBlockName) as TextBlock;
                if (textBlock != null)
                {
                    if (isActive)
                    {
                        textBlock.Text = "Да";
                        textBlock.Foreground = Brushes.Red;
                    }
                    else
                    {
                        textBlock.Text = "Нет";
                        textBlock.Foreground = Brushes.Black;
                    }
                }
            });
        }


    }
}
