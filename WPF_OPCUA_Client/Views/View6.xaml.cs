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

namespace WPF_OPCUA_Client.Views
{
    /// <summary>
    /// Логика взаимодействия для View6.xaml
    /// </summary>
    public partial class View6 : UserControl
    {
        private DispatcherTimer _logTimer;
        private bool _lastIndUp = false;
        private bool _lastIndDown = false;
        private bool _lastIndLeft = false;
        private bool _lastIndRight = false;
        private bool _lastIndForw = false;
        private bool _lastIndBack = false;

        private bool _lastLimitUp = false;
        private bool _lastLimitDown = false;
        private bool _lastLimitLeft = false;
        private bool _lastLimitRight = false;
        private bool _lastLimitForw = false;
        private bool _lastLimitBack = false;

        // Зажим
        private bool _lastClampUp = false;
        private bool _lastClampDown = false;

        // Концевики зажима
        private bool _lastLimitClampUp = false;
        private bool _lastLimitClampDown = false;
        private bool _lastLimitIndDown2 = false;

        // Вращение
        private bool _lastRotLeft = false;
        private bool _lastRotRight = false;

        private bool _lastAxis1Comm = false;
        private bool _lastAxis2Comm = false;
        private bool _lastAxis3Comm = false;
        private bool _lastAxis4Comm = false;
        private bool _lastAxis5Comm = false;

        private bool _lastSprayer = false;
        private bool _lastAutomat = false;
        private bool _lastStart = false;
        private bool _lastStop = false;
        private bool _lastProgrammBlock = false;

        private bool _lastLinkStable = true;
        private short _lastMode = -1;

        private short _lastAxis1State = -1;
        private short _lastAxis2State = -1;
        private short _lastAxis3State = -1;
        private short _lastAxis4State = -1;
        private short _lastAxis5State = -1;

        public View6()
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

        private void CheckTagsAndLog()
        {
            try
            {
                bool currentIndUp = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndUp");
                bool currentIndDown = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndDown");
                bool currentIndLeft = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndLeft");
                bool currentIndRight = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndRight");
                bool currentIndForw = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndForw");
                bool currentIndBack = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.IndBack");

                if (currentIndUp && !_lastIndUp)
                    AddLogMessage("Переключатель 'Индуктор вверх'");
                if (currentIndDown && !_lastIndDown)
                    AddLogMessage("Переключатель 'Индуктор вниз'");
                if (currentIndLeft && !_lastIndLeft)
                    AddLogMessage("Переключатель 'Индуктор влево'");
                if (currentIndRight && !_lastIndRight)
                    AddLogMessage("Переключатель 'Индуктор вправо'");
                if (currentIndForw && !_lastIndForw)
                    AddLogMessage("Переключатель 'Индуктор вперёд'");
                if (currentIndBack && !_lastIndBack)
                    AddLogMessage("Переключатель 'Индуктор назад'");

                // Остановка, если все нули и раньше что-то было активно
                if (!currentIndUp && !currentIndDown && !currentIndLeft && !currentIndRight && !currentIndForw && !currentIndBack &&
                    (_lastIndUp || _lastIndDown || _lastIndLeft || _lastIndRight || _lastIndForw || _lastIndBack))
                {
                    AddLogMessage("Индуктор остановлен");
                }
                CheckLimitSwitch(ref _lastLimitUp, "LimitIndUp", "вверх");
                CheckLimitSwitch(ref _lastLimitDown, "LimitIndDown", "вниз");
                CheckLimitSwitch(ref _lastLimitLeft, "LimitIndLeft", "влево");
                CheckLimitSwitch(ref _lastLimitRight, "LimitIndRight", "вправо");
                CheckLimitSwitch(ref _lastLimitForw, "LimitIndForw", "вперёд");
                CheckLimitSwitch(ref _lastLimitBack, "LimitIndBack", "назад");
                // === ЗАЖИМ ===
                bool currentClampUp = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.ClampUp");
                bool currentClampDown = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.ClampDown");

                if (currentClampUp && !_lastClampUp) AddLogMessage("Переключатель 'Зажим вверх'");
                if (currentClampDown && !_lastClampDown) AddLogMessage("Переключатель 'Зажим вниз'");

                if (!currentClampUp && !currentClampDown && (_lastClampUp || _lastClampDown))
                    AddLogMessage("Зажим остановлен");

                // === КОНЦЕВИКИ ЗАЖИМА ===
                CheckLimitSwitch(ref _lastLimitClampUp, "LimitClampUp", "зажима вверх");
                CheckLimitSwitch(ref _lastLimitClampDown, "LimitClampDown", "зажима вниз");
                CheckLimitSwitch(ref _lastLimitIndDown2, "LimitClampDown", "зажима (ручного режима)");

                // === ВРАЩЕНИЕ ===
                bool currentRotLeft = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.Rotleft");
                bool currentRotRight = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.RotRight");

                if (currentRotLeft && !_lastRotLeft) AddLogMessage("Переключатель 'Вращение влево'");
                if (currentRotRight && !_lastRotRight) AddLogMessage("Переключатель 'Вращение вправо'");

                if (!currentRotLeft && !currentRotRight && (_lastRotLeft || _lastRotRight))
                    AddLogMessage("Вращение остановлено");


                CheckAxisCommState(ref _lastAxis1Comm, "Axis1", "вращения", "индуктора");
                CheckAxisCommState(ref _lastAxis2Comm, "Axis2", "X", "индуктора");
                CheckAxisCommState(ref _lastAxis3Comm, "Axis3", "Y", "индуктора");
                CheckAxisCommState(ref _lastAxis4Comm, "Axis4", "Z", "индуктора");
                CheckAxisCommState(ref _lastAxis5Comm, "Axis5", "Z", "зажима");

                CheckBoolTagChange(ref _lastSprayer, "Sprayer", "Переключатель Спреер", "включен", "выключен");
                CheckBoolTagChange(ref _lastAutomat, "Automat", "Переключатель Автомат/Ручной", "Автомат", "Ручной");
                CheckBoolTagChange(ref _lastStart, "Start", "Начало процесса", "включено", "выключено");
                CheckBoolTagChange(ref _lastStop, "StopHeater", "Остановка процесса", "включена", "выключена");
                CheckBoolTagChange(ref _lastProgrammBlock, "ProgrammBlock", "Аварийный стоп", "включен", "выключен");
                CheckLinkToDrive();
                CheckWorkMode();

                short currentAxis1State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis1.nAxisState");
                CheckAxisState(currentAxis1State, _lastAxis1State, 1);
                _lastAxis1State = currentAxis1State;

                short currentAxis2State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis2.nAxisState");
                CheckAxisState(currentAxis2State, _lastAxis2State, 2);
                _lastAxis2State = currentAxis2State;

                short currentAxis3State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis3.nAxisState");
                CheckAxisState(currentAxis3State, _lastAxis3State, 3);
                _lastAxis3State = currentAxis3State;

                short currentAxis4State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis4.nAxisState");
                CheckAxisState(currentAxis4State, _lastAxis4State, 4);
                _lastAxis4State = currentAxis4State;

                short currentAxis5State = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis5.nAxisState");
                CheckAxisState(currentAxis5State, _lastAxis5State, 5);
                _lastAxis5State = currentAxis5State;

                // === Обновление состояний ===
                _lastClampUp = currentClampUp;
                _lastClampDown = currentClampDown;
                _lastRotLeft = currentRotLeft;
                _lastRotRight = currentRotRight;

                // Сохраняем предыдущее состояние
                _lastIndUp = currentIndUp;
                _lastIndDown = currentIndDown;
                _lastIndLeft = currentIndLeft;
                _lastIndRight = currentIndRight;
                _lastIndForw = currentIndForw;
                _lastIndBack = currentIndBack;
            }
            catch (Exception ex)
            {
                AddLogMessage($"Ошибка чтения тэгов: {ex.Message}");
            }
        }
        private void CheckAxisState(short currentState, short lastState, int axisNumber)
        {
            if (currentState != lastState)
            {
                switch (currentState)
                {
                    case 3:
                        AddLogMessage($"Ось {axisNumber} готова к движению", Brushes.Green);
                        break;
                    case 1:
                        AddLogMessage($"Ось {axisNumber} не готова к движению. Ошибка движения", Brushes.Red);
                        break;
                    
                }
            }
        }
        private void AddLogMessage(string message, Brush color = null)
        {
            var textBlock = new TextBlock
            {
                Text = $"{DateTime.Now:HH:mm:ss}  {message}",
                Margin = new Thickness(5, 2, 5, 2),
                FontSize = 18,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold

            };

            LogPanel.Children.Add(textBlock);
        }

        private void CheckLinkToDrive()
        {
            bool linkStable = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.bLinkStable");

            if (linkStable != _lastLinkStable)
            {
                if (linkStable)
                {
                    AddLogMessage("Связь с ПЧ - подключен");
                    // Можно убрать красное выделение, если оно ранее было
                }
                else
                {
                    AddLogMessage("Связь с ПЧ отсутствует. Переход в автоматический режим невозможен", Brushes.Red);
                }

                _lastLinkStable = linkStable;
            }
        }
        private void CheckWorkMode()
        {
            short currentMode = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.PLC_PRG.Modes");

            if (currentMode != _lastMode)
            {
                switch (currentMode)
                {
                    case 1:
                        AddLogMessage("Работа в ручном режиме");
                        break;
                    case 2:
                        AddLogMessage("Работа в автоматическом режиме закалки вала");
                        break;
                    case 3:
                        AddLogMessage("Работа в автоматическом режиме закалки шестерни");
                        break;
                    case 100:
                        AddLogMessage("Критическая ошибка. Отсутствует связь с ПЧ. Движение возможно исключительно в ручном режиме", Brushes.Red);
                        break;
                    default:
                        AddLogMessage($"Неизвестный режим работы: {currentMode}");
                        break;
                }

                _lastMode = currentMode;
            }
        }

        private void CheckBoolTagChange(ref bool lastState, string tagName, string description, string trueText, string falseText)
        {
            bool current = OpcUaManager.OpcRead<bool>($"ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.{tagName}");
            if (current != lastState)
            {
                string status = current ? trueText : falseText;
                AddLogMessage($"{description}: {status}");
                lastState = current;
            }
        }

        private void CheckAxisCommState(ref bool lastState, string axisTag, string axisName, string axisGroup)
        {
            bool current = OpcUaManager.OpcRead<bool>($"ns=4;s=|var|LS.Application.IoConfig_Globals.{axisTag}.bCommunication");
            if (current != lastState)
            {
                string statusText = current ? "подключен" : "отключен";
                AddLogMessage($"Связь с сервоприводом ось {axisName} {axisGroup}: {statusText}");
                lastState = current;
            }
        }

        private void CheckLimitSwitch(ref bool lastState, string tagName, string directionText)
        {
            bool current = OpcUaManager.OpcRead<bool>($"ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.{tagName}");
            if (current != lastState)
            {
                string stateText = current ? "замкнут" : "разомкнут";
                AddLogMessage($"Концевой выключатель индуктора {directionText} {stateText}");
                lastState = current;
            }
        }

        // Обновите метод AddLogMessage
        private void AddLogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                // Добавляем сообщение в общую коллекцию
                SharedLogs.LogMessages.Add($"{DateTime.Now:HH:mm:ss}  {message}");

                // Оригинальная логика
                var textBlock = new TextBlock
                {
                    Text = $"{DateTime.Now:HH:mm:ss}  {message}",
                    Margin = new Thickness(5, 2, 5, 2),
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap
                };

                LogPanel.Children.Add(textBlock);
                History.ScrollToEnd();
                LogPanel.UpdateLayout();
            });
        }
    }
}
