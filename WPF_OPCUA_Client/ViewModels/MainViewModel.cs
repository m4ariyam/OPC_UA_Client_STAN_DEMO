using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using WPF_OPCUA_Client.Views;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows;
using Org.BouncyCastle.Ocsp;
using Opc.Ua;

namespace WPF_OPCUA_Client.ViewModels
{
    
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly View1 _view1 = new View1();
        private readonly View2 _view2 = new View2();
        private readonly View3 _view3 = new View3();
        private readonly View4 _view4 = new View4();
        private readonly View5 _view5 = new View5();
        private readonly View6 _view6 = new View6();
        private readonly View7 _view7 = new View7();

        private DispatcherTimer _switchDisabledMonitorTimer;
        private bool _previousSwitchDisabled = false;

        private object _currentView;
        private bool _isInitialView = true;
        private int? _lastActiveViewIndex = null;
        private DispatcherTimer _updateTimer;
        private int AutomatMode;
        private string _axis2PositionText = "0.00 мм";
        private string _axis3PositionText = "0.00 мм";
        private string _axis4PositionText = "0.00 мм";
        private string _rotAxis1SpeedText = "0.00 об/мин";
        private string Amperage = "0.0 А";
        private string Voltage = "0.0 В"; // 11.1
        private string Power = "0.0 кВт";// 12.1
        private string Rotation = "0°0`";

        public string RotationText
        {
            get => Rotation;
            set { Rotation = value; OnPropertyChanged(); }
        }
        public string VoltageText
        {
            get => Voltage;
            set { Voltage = value; OnPropertyChanged(); }
        }
        public string PowerText
        {
            get => Power;
            set { Power = value; OnPropertyChanged(); }
        }
        public string AmperageText
        {
            get => Amperage;
            set { Amperage = value; OnPropertyChanged(); }
        }
        public string RotAxis1SpeedText
        {
            get => _rotAxis1SpeedText;
            set { _rotAxis1SpeedText = value; OnPropertyChanged(); }
        }
        public string Axis2PositionText
        {
            get => _axis2PositionText;
            set { _axis2PositionText = value; OnPropertyChanged(); }
        }

        public string Axis3PositionText
        {
            get => _axis3PositionText;
            set { _axis3PositionText = value; OnPropertyChanged(); }
        }

        public string Axis4PositionText
        {
            get => _axis4PositionText;
            set { _axis4PositionText = value; OnPropertyChanged(); }
        }

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        private string _freqText = "0.0 Гц";

        public string FreqText
        {
            get => _freqText;
            set { _freqText = value; OnPropertyChanged(); }
        }


        public bool IsInitialView
        {
            get => _isInitialView;
            set
            {
                if (_isInitialView != value)
                {
                    _isInitialView = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _sprayerLedSource = "/IMG/LED1OFF.png";
        public string SprayerLedSource
        {
            get => _sprayerLedSource;
            set { _sprayerLedSource = value; OnPropertyChanged(); }
        }

        private string _heaterLedSource = "/IMG/LED2OFF.png";
        public string HeaterLedSource
        {
            get => _heaterLedSource;
            set { _heaterLedSource = value; OnPropertyChanged(); }
        }

        private string _alarmsLedSource = "/IMG/LED3OFF.png";
        public string AlarmsLedSource
        {
            get => _alarmsLedSource;
            set
            {
                if (_alarmsLedSource != value)
                {
                    _alarmsLedSource = value;
                    OnPropertyChanged(nameof(AlarmsLedSource));
                }
            }
        }
        private DispatcherTimer _workTimer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;

        private string _runTime = "00:00";
        public string RunTime
        {
            get => _runTime;
            set
            {
                if (_runTime != value)
                {
                    _runTime = value;
                    OnPropertyChanged(nameof(RunTime));
                }
            }
        }
        private DispatcherTimer _viewCheckTimer;
        private object _lastTrackedView = null;
        public MainViewModel()
        {
            Axis2PositionText = "0.0 мм";
            Axis3PositionText = "0.0 мм";
            Axis4PositionText = "0.0 мм";
            RotAxis1SpeedText = "0.0 об/мин";
            AmperageText = "0.0 А";
            PowerText = "0.0 кВт";
            VoltageText = "0.0 В";
            CurrentView = null;
            StartMonitoringSwitchDisabled();
            InitDataForView5();
            InitDataForView1();
            StartUpdatingAxisPositions();
            _workTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _workTimer.Tick += WorkTimer_Tick;

            _viewCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _viewCheckTimer.Tick += CheckCurrentView;
            _viewCheckTimer.Start();
        }
        private void CheckCurrentView(object sender, EventArgs e)
        {
            if (_currentView == null)
                return;

            // Если View не изменился, ничего не делаем
            if (_currentView == _lastTrackedView)
                return;

            _lastTrackedView = _currentView;

            short viewNumber = 0;
            if (_currentView == _view1) viewNumber = 1;
            else if (_currentView == _view2) viewNumber = 2;
            else if (_currentView == _view3) viewNumber = 3;

            if (viewNumber > 0)
            {
                try
                {
                    WriteToOpcServer(viewNumber);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при записи текущего View в OPC: {ex.Message}");
                }
            }
        }
        private void WorkTimer_Tick(object sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            RunTime = _elapsedTime.ToString(@"mm\:ss");
        }

        private void StartMonitoringSwitchDisabled()
        {
            _switchDisabledMonitorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _switchDisabledMonitorTimer.Tick += MonitorSwitchDisabled;
            _switchDisabledMonitorTimer.Start();
        }

        private void MonitorSwitchDisabled(object sender, EventArgs e)
        {
            bool currentSwitchDisabled;

            try
            {
                currentSwitchDisabled = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.SwitchDisabled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при чтении SwitchDisabled: {ex.Message}");
                return;
            }

            if (!_previousSwitchDisabled && currentSwitchDisabled)
            {
                MessageBox.Show(
                    "Перед переходом в автоматический режим убедитесь, что вы остановили все движения.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            _previousSwitchDisabled = currentSwitchDisabled;
        }

        public void ShowInitialView()
        {
            IsInitialView = true;
            CurrentView = _view2;  // Используем предварительно созданный View2
        }
        private void WriteToOpcServer(short mode)
        {
            string tag = "ns=4;s=|var|LS.Application.PLC_PRG.AutomatModes";
            OpcUaManager.OpcWrite(tag, mode);
        }
        public void ShowView1()
        {
            IsInitialView = false;
            SetLastActiveView(1);
            CurrentView = _view1;

        }

        public void ShowView2()
        {
            IsInitialView = false;
            SetLastActiveView(2);
            CurrentView = _view2;
            WriteToOpcServer(2);
        }

        public void ShowView3()
        {
            IsInitialView = false;
            SetLastActiveView(3);
            CurrentView = _view3;
            WriteToOpcServer(3);
        }

        public void ShowView4()
        {
            IsInitialView = false;
            CurrentView = _view4;
        }

        public void ShowView5()
        {
            IsInitialView = false;
            CurrentView = _view5;
        }

        public void ShowView6()
        {
            IsInitialView = false;
            CurrentView = _view6;
        }

        public void ShowView7()
        {
            IsInitialView = false;
            CurrentView = _view7;
        }

        private void SetLastActiveView(int viewIndex)
        {
            _lastActiveViewIndex = viewIndex;
            
        }

        public void ShowLastActiveView()
        {
            if (_lastActiveViewIndex.HasValue)
            {
                switch (_lastActiveViewIndex.Value)
                {
                    case 1:
                        ShowView1();
                        break;
                    case 2:
                        ShowView2();
                        break;
                    case 3:
                        ShowView3();
                        break;
                }
            }
            else
            {
                ShowInitialView();
            }
        }
        private bool _alreadyWarned = false;
        public void InitDataForView5()
        {
            _view5.InitializeData();
        }
        public void InitDataForView1()
        {
            _view1.LoadCheckBoxStatesAndSendToServer();
        }

        void CheckSwitchDisabled()
        {
            bool switchDisabled = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.SwitchDisabled");

            if (switchDisabled && !_alreadyWarned)
            {
                _alreadyWarned = true;

                MessageBox.Show(
                    "Перед переходом в автоматический режим убедитесь, что вы остановили все движения.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else if (!switchDisabled)
            {
                _alreadyWarned = false;
            }
        }

        private void StartUpdatingAxisPositions()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _updateTimer.Tick += (s, e) =>
            {
                try
                {
                    var axis2 = OpcUaManager.OpcRead<float>("ns=4;s=|var|LS.Application.PLC_PRG.fActualPosition_Axis2");
                    var axis3 = OpcUaManager.OpcRead<float>("ns=4;s=|var|LS.Application.PLC_PRG.fActualPosition_Axis3");
                    var axis4 = OpcUaManager.OpcRead<float>("ns=4;s=|var|LS.Application.PLC_PRG.fActualPosition_Axis4");
                    var axis1 = OpcUaManager.OpcRead<float>("ns=4;s=|var|LS.Application.PLC_PRG.CurrentRotSpeed") * 60 / 7;
                    var ampers = OpcUaManager.OpcRead<float>("ns=4;s=|var|LS.Application.PLC_PRG.CurrentOutput") / 10;
                    ushort rawPower = OpcUaManager.OpcRead<ushort>("ns=4;s=|var|LS.Application.PLC_PRG.WriteOptions[2]");
                    ushort rawVoltage = OpcUaManager.OpcRead<ushort>("ns=4;s=|var|LS.Application.PLC_PRG.WriteOptions[1]");
                    ushort rawFreq = OpcUaManager.OpcRead<ushort>("ns=4;s=|var|LS.Application.PLC_PRG.Read[16]");
                    // Преобразование и форматирование
                    float powers = rawPower / 10f;
                    float voltage = rawVoltage;
                    float freq = rawFreq / 1000f;
                    bool sprayerState = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.SprayerPump");
                    SprayerLedSource = sprayerState ? "/IMG/LED1ON.png" : "/IMG/LED1OFF.png";

                    ushort heaterState = OpcUaManager.OpcRead<ushort>("ns=4;s=|var|LS.Application.PLC_PRG.Read[1]");
                    HeaterLedSource = heaterState == 1 ? "/IMG/LED2ON.png" : "/IMG/LED2OFF.png";

                    bool startLed = OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.StartLed");
                    short mode = OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.PLC_PRG.Modes");

                   float rotation = OpcUaManager.OpcRead<float>("ns=4;s=|var|LS.Application.PLC_PRG.Rotation");
                    int degrees = (int)Math.Truncate(rotation);

                    float minutes = (rotation - degrees) * 60f;

                    if (startLed && (mode == 2 || mode == 3))
                    {
                        if (!_workTimer.IsEnabled)
                        {
                            _elapsedTime = TimeSpan.Zero; // если нужно каждый раз заново
                            _workTimer.Start();
                        }
                    }
                    else
                    {
                        if (_workTimer.IsEnabled)
                        {
                            _workTimer.Stop();
                            _elapsedTime = TimeSpan.Zero;
                            RunTime = "00:00";
                        }
                    }

                    Axis2PositionText = $"{axis2:F1} мм";
                    Axis3PositionText = $"{axis3:F1} мм";
                    Axis4PositionText = $"{axis4:F1} мм";
                    RotAxis1SpeedText = $"{axis1:F1}";
                    AmperageText = $"{ampers:F1} А";
                    VoltageText = $"{voltage:F1} В";
                    PowerText = $"{powers:F0} кВт";
                    FreqText = $"{freq:F0} кГц";
                    RotationText = $"{degrees:F0}°{minutes:F0}`";
                    bool isAlarmOn = false;


                    ushort read1 = OpcUaManager.OpcRead<ushort>("ns=4;s=|var|LS.Application.PLC_PRG.Read[1]");
                    if (read1 == 2)
                        isAlarmOn = true;


                    short[] axisStates = new short[]
                    {
                        OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis1.nAxisState"),
                        OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis2.nAxisState"),
                        OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis3.nAxisState"),
                        OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis4.nAxisState"),
                        OpcUaManager.OpcRead<short>("ns=4;s=|var|LS.Application.IoConfig_Globals.Axis5.nAxisState"),
                    };

                    if (axisStates.Any(state => state == 1))
                        isAlarmOn = true;

                    // Проверка bool-индикаторов
                    string[] boolTags = new string[]
                    {
                        
                        "ns=4;s=|var|LS.Application.IoConfig_Globals_Mapping.HighestLimit",
                        
                    };
                    if (OpcUaManager.OpcRead<bool>("ns=4;s=|var|LS.Application.PLC_PRG.bLinkStable") == false)
                        isAlarmOn = true;
                    
                    AlarmsLedSource = isAlarmOn ? "/IMG/LED3On.png" : "/IMG/LED3Off.png";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка обновления позиций: {ex.Message}");
                }
            };
            _updateTimer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Property
        {
            get => default;
            set
            {
            }
        }
    }
}

