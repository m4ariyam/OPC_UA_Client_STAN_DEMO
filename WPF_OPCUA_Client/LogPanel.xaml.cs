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

namespace WPF_OPCUA_Client
{
    /// <summary>
    /// Логика взаимодействия для LogPanel.xaml
    /// </summary>
    public partial class LogPanel : UserControl
    {
        // Таймеры
        private DispatcherTimer _logTimer;

        // Состояния (перенесены из View1)
        private short _lastAxis1State = -1;
        private short _lastAxis2State = -1;
        private short _lastAxis3State = -1;
        private short _lastAxis4State = -1;
        private short _lastAxis5State = -1;
        private bool _lastLinkStable = true;
        private bool _lastProtokInd = false;
        private bool _lastProtokPCh = false;
        private bool _lastProtokTSU = false;
        private bool _lastOutBlock = false;
        private ushort _lastDriveFaultCode = 0;

        private readonly Dictionary<string, (string DirectionText, bool LastState)> _limitStates = new()
        {
            
            { "LimitIndBack", ("назад", false) },
            { "LimitIndDown", ("вниз", false) },
            { "LimitIndForw", ("вперёд", false) },
            { "LimitIndLeft", ("влево", false) },
            { "LimitIndRight", ("вправо", false) },
            { "LimitIndUp", ("вверх", false) },
        };

        public LogPanel()
        {
            InitializeComponent();
            StartLogging();
        }

        private void StartLogging()
        {
            _logTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            _logTimer.Tick += (s, e) => CheckTagsAndLog();
            _logTimer.Start();
        }
        private void CheckAxisState(short currentState, ref short lastState, int axisNumber)
        {
            bool wasError = lastState == 1;

            if (currentState != lastState)
            {
                if (currentState == 3 && axisNumber != 5)
                {
                    if (wasError)
                    {
                        AddLogMessage($"Ошибка оси {axisNumber} была сброшена. \nОсь {axisNumber} готова к движению", Brushes.Green);
                    }
                    // Не показываем сообщение при переходе из других состояний (например, 5 → 3)
                }
                else if (currentState == 1 && axisNumber!=5)
                {
                    AddLogMessage($"Критическая ошибка. Ось {axisNumber} в аварийной ситуации.\nЗажмите кнопку СТОП для сброса ошибки.", Brushes.Red);
                }

                lastState = currentState;
            }
        }

        

        private void CheckLimitAlarms()
        {
            foreach (var key in _limitStates.Keys.ToList())
            {
                bool current = OpcUaManager.OpcRead<bool>($"ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.{key}");
                var (direction, lastState) = _limitStates[key];

                if (current && !lastState)
                {
                    AddLogMessage(
                        $"Авария технического процесса. Движение {direction} невозможно. \nСмотрите подробнее в окнах Диагностика/История",
                        Brushes.Red
                    );
                }

                // Обновляем состояние
                _limitStates[key] = (direction, current);
            }
        }


        private void CheckDriveFault()
        {
            ushort currentCode = OpcUaManager.OpcRead<ushort>("ns=4;s=|var|LS.Application.PLC_PRG.Read[1]");

            if (currentCode != _lastDriveFaultCode)
            {
                if (currentCode == 2)
                {
                    AddLogMessage("Авария ПЧ. Смотрите меню Аварии ПЧ", Brushes.Red);
                }
                else if (_lastDriveFaultCode == 2 && currentCode == 0)
                {
                    AddLogMessage("Авария ПЧ сброшена", Brushes.Green);
                }

                _lastDriveFaultCode = currentCode;
            }
        }
        private void CheckLinkToDrive()
        {
            bool linkStable;

            try
            {
                linkStable = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.bLinkStable");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Ошибка чтения состояния связи с ПЧ: {ex.Message}", Brushes.Red);
                return;
            }

            if (linkStable != _lastLinkStable)
            {
                if (linkStable)
                {
                    AddLogMessage("Связь с ПЧ восстановлена", Brushes.Green);
                }
                else
                {
                    AddLogMessage("Связь с ПЧ отсутствует. \nРабота в автоматическом режиме невозможна", Brushes.Red);
                }

                _lastLinkStable = linkStable;
            }
        }
        private void CheckCoolingAndBlockStatus()
        {
            try
            {
                bool currentProtokInd = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.ProtokInd");
                if (currentProtokInd != _lastProtokInd)
                {
                    if (currentProtokInd)
                        AddLogMessage("Нет протока воды Индуктора", Brushes.Red);
                    else
                        AddLogMessage("Проток воды Индуктора восстановлен", Brushes.Green);

                    _lastProtokInd = currentProtokInd;
                }

                bool currentProtokPCh = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.ProtokPCh");
                if (currentProtokPCh != _lastProtokPCh)
                {
                    if (currentProtokPCh)
                        AddLogMessage("Нет протока воды ПЧ", Brushes.Red);
                    else
                        AddLogMessage("Проток воды ПЧ восстановлен", Brushes.Green);

                    _lastProtokPCh = currentProtokPCh;
                }

                bool currentProtokTSU = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.ProtokTSU");
                if (currentProtokTSU != _lastProtokTSU)
                {
                    if (currentProtokTSU)
                        AddLogMessage("Нет протока воды ТСУ", Brushes.Red);
                    else
                        AddLogMessage("Проток воды ТСУ восстановлен", Brushes.Green);

                    _lastProtokTSU = currentProtokTSU;
                }

                bool currentOutBlock = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.OutBlock");
                if (currentOutBlock != _lastOutBlock)
                {
                    if (currentOutBlock)
                        AddLogMessage("Активирована внешняя блокировка", Brushes.Red);
                    else
                        AddLogMessage("Внешняя блокировка деактивирована", Brushes.Green);

                    _lastOutBlock = currentOutBlock;
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Ошибка при чтении сигналов протока или блокировки: {ex.Message}", Brushes.Red);
            }
        }

        private void AddLogMessage(string message, Brush color = null)
        {
            var textBlock = new TextBlock
            {
                Text = $"{DateTime.Now:HH:mm:ss}  {message}",
                Margin = new Thickness(5, 2, 5, 2),
                FontSize = 14,
                Foreground = color ?? Brushes.Black,
                FontWeight = (color == Brushes.Red) ? FontWeights.ExtraBold : FontWeights.Bold
            };

            LogStackPanel.Children.Add(textBlock);
            MainScrollViewer.ScrollToEnd();
            LogStackPanel.UpdateLayout();
        }

        private void CheckTagsAndLog()
        {
            try
            {
                short currentAxis1State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis1.nAxisState");
                CheckAxisState(currentAxis1State, ref _lastAxis1State, 1);


                _lastAxis1State = currentAxis1State;

                short currentAxis2State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis2.nAxisState");
                CheckAxisState(currentAxis2State, ref _lastAxis2State, 2);


                _lastAxis2State = currentAxis2State;

                short currentAxis3State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis3.nAxisState");
                CheckAxisState(currentAxis3State, ref _lastAxis3State, 3);


                _lastAxis3State = currentAxis3State;

                short currentAxis4State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis4.nAxisState");
                CheckAxisState(currentAxis4State, ref _lastAxis4State, 4);


                _lastAxis4State = currentAxis4State;

                short currentAxis5State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis5.nAxisState");
                CheckAxisState(currentAxis5State, ref _lastAxis5State, 5);


                _lastAxis5State = currentAxis5State;

                CheckLinkToDrive();     // вызов метода контроля связи с ПЧ
                CheckCoolingAndBlockStatus();
                CheckDriveFault();
                CheckLimitAlarms();
            }
            catch (Exception ex)
            {
                AddLogMessage($"Ошибка чтения тэгов: {ex.Message}");
            }
        }
    }
}
