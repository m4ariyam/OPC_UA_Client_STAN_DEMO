using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.IO;
using System.Windows.Threading;
using System.Globalization;

namespace WPF_OPCUA_Client.Views
{
    /// <summary>
    /// Логика взаимодействия для View5.xaml
    /// </summary>
    public partial class View5 : UserControl
    {
        public View5()
        {
            InitializeComponent();
            StartOpcReadLoop();
        }
        private readonly string tagX = "ns=4;s=|var|LS.Application.PLC_PRG.calibration2";
        private readonly string tagY = "ns=4;s=|var|LS.Application.PLC_PRG.calibration3";
        private readonly string tagZ = "ns=4;s=|var|LS.Application.PLC_PRG.calibration4";
        private DispatcherTimer opcReadTimer;
        private void StartOpcReadLoop()
        {
            opcReadTimer = new DispatcherTimer();
            opcReadTimer.Interval = TimeSpan.FromMilliseconds(500);
            opcReadTimer.Tick += OpcReadTimer_Tick;
            opcReadTimer.Start();
        }

        private void OpcReadTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                bool valX = OpcUaManager.OpcRead<bool>(tagX);
                bool valY = OpcUaManager.OpcRead<bool>(tagY);
                bool valZ = OpcUaManager.OpcRead<bool>(tagZ);

                ResetCalibrationX.Background = valX ? Brushes.Yellow : Brushes.Transparent;
                ResetCalibrationY.Background = valY ? Brushes.Yellow : Brushes.Transparent;
                ResetCalibrationZ.Background = valZ ? Brushes.Yellow : Brushes.Transparent;
            }
            catch (Exception ex)
            {
                // опционально: логировать ошибку или выводить в UI
                Console.WriteLine("Ошибка чтения OPC: " + ex.Message);
            }
        }
        private readonly DispatcherTimer velocityTimer = new DispatcherTimer();
        private void StartVelocityMonitoring()
        {
            velocityTimer.Interval = TimeSpan.FromMilliseconds(500);
            velocityTimer.Tick += VelocityTimer_Tick;
            velocityTimer.Start();
        }
        private void VelocityTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var data = LoadDataFromSection("Params");

                ReadAndSaveToFile("ns=4;s=|var|LS.Application.PLC_PRG.CurrentMoveSpeed", "Velocity", data);
                ReadAndSaveToFile("ns=4;s=|var|LS.Application.PLC_PRG.CurrentOutput", "Tok", data);
                ReadAndSaveToFile("ns=4;s=|var|LS.Application.PLC_PRG.CurrentRotSpeed", "RotationSpeed", data);

                SaveData("Params", data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении параметров: {ex.Message}");
            }
        }
        private void ReadAndSaveToFile(string tag, string key, Dictionary<string, string> data)
        {
            float? value = OpcUaManager.OpcRead<float>(tag);
            if (value.HasValue)
            {
                data[key] = value.Value.ToString("0.###", CultureInfo.InvariantCulture);
            }
        }

        private void SaveVelocityToFile(float velocity)
        {
            var data = LoadDataFromSection("Params");
            data["Velocity"] = velocity.ToString("0.###", CultureInfo.InvariantCulture);
            SaveData("Params", data);
        }


        private void AllZone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var dataToSave = new Dictionary<string, string>();
                string baseTag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcArrOptions";

               
                int stackPanelIndex = 0;

            
                for (int i = 0; i < AllZone.Children.Count; i++)
                {
                    if (AllZone.Children[i] is StackPanel stackPanel)
                    {
                        int textBoxIndex = 0;

                        foreach (var child in stackPanel.Children)
                        {
                            if (child is TextBox textBox)
                            {
                                string rawText = textBox.Text?.Trim();
                                if (string.IsNullOrEmpty(rawText)) continue;

                                if (!float.TryParse(rawText, out float value))
                                {
                                    MessageBox.Show($"Ошибка: введите число в поле {textBox.Name}");
                                    return;
                                }


                                string tag = $"{baseTag}[{stackPanelIndex},{textBoxIndex}]";

                                // Пытаемся записать значение в OPC UA сервер
                                if (!OpcUaManager.OpcWrite(tag, value))
                                {
                                    MessageBox.Show($"Ошибка записи по тэгу: {tag}");
                                    return;
                                }


                                dataToSave[textBox.Name] = value.ToString("0.#####");


                                textBoxIndex++;

                            }
                            else if (child is CheckBox checkBox)
                            {
                                float value = checkBox.IsChecked == true ? 1.0f : 0.0f;

                                string tag = $"{baseTag}[{stackPanelIndex},{textBoxIndex}]";

                                if (!OpcUaManager.OpcWrite(tag, value))
                                {
                                    MessageBox.Show($"Ошибка записи по тэгу: {tag}");
                                    return;
                                }

                                dataToSave[checkBox.Name] = value.ToString("0"); // сохраняем как 1 или 0
                                textBoxIndex++;
                            }
                        }


                        stackPanelIndex++;
                    }
                }

               
                SaveData("Options", dataToSave);

                //MessageBox.Show("Все данные успешно отправлены на сервер и сохранены в файл.");
            }
        }
        public void InitializeData()
        {
            FileManager.EnsureFileExists();
            StartVelocityMonitoring(); // Запускаем мониторинг скорости

            var loadedData = LoadDataFromSection("Options");
            var loadedParams = LoadDataFromSection("Params");

            // 1. Заполняем UI из [Options]
            foreach (var kvp in loadedData)
            {
                string controlName = kvp.Key;
                string value = kvp.Value;

                if (FindName(controlName) is TextBox textBox)
                {
                    textBox.Text = value;
                }
                else if (FindName(controlName) is CheckBox checkBox)
                {
                    checkBox.IsChecked = value == "1";
                }
            }

            var dataToSave = new Dictionary<string, string>();
            string baseTag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcArrOptions";
            int stackPanelIndex = 0;

            // 2. Проход по StackPanel-ам
            for (int i = 0; i < AllZone.Children.Count; i++)
            {
                if (AllZone.Children[i] is StackPanel stackPanel)
                {
                    int elementIndex = 0;

                    foreach (var child in stackPanel.Children)
                    {
                        if (child is TextBox textBox)
                        {
                            string rawText = textBox.Text?.Trim();
                            if (string.IsNullOrEmpty(rawText)) continue;

                            if (!float.TryParse(rawText, out float value))
                            {
                                MessageBox.Show($"Ошибка: введите число в поле {textBox.Name}");
                                return;
                            }

                            string tag = $"{baseTag}[{stackPanelIndex},{elementIndex}]";

                            if (!OpcUaManager.OpcWrite(tag, value))
                            {
                                //MessageBox.Show($"Ошибка записи по тэгу: {tag}");
                                return;
                            }

                            dataToSave[textBox.Name] = value.ToString("0.#####");
                            elementIndex++;
                        }
                        else if (child is CheckBox checkBox)
                        {
                            float value = checkBox.IsChecked == true ? 1.0f : 0.0f;
                            string tag = $"{baseTag}[{stackPanelIndex},{elementIndex}]";

                            if (!OpcUaManager.OpcWrite(tag, value))
                            {
                               // MessageBox.Show($"Ошибка записи по тэгу: {tag}");
                                return;
                            }

                            dataToSave[checkBox.Name] = value.ToString("0");
                            elementIndex++;
                        }
                    }

                    stackPanelIndex++;
                }
            }

            SaveData("Options", dataToSave);

            // 3. Отдельная запись в тэги из Params
            TryWriteParam("Velocity", "ns=4;s=|var|LS.Application.PLC_PRG.CurrentMoveSpeed", loadedParams);
            TryWriteParam("Tok", "ns=4;s=|var|LS.Application.PLC_PRG.CurrentOutput", loadedParams);
            TryWriteParam("RotationSpeed", "ns=4;s=|var|LS.Application.PLC_PRG.CurrentRotSpeed", loadedParams);
        }

        private void TryWriteParam(string key, string tag, Dictionary<string, string> loadedParams)
        {
            if (loadedParams.TryGetValue(key, out string valueStr) &&
                float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                if (!OpcUaManager.OpcWrite(tag, value))
                {
                    MessageBox.Show($"Ошибка записи значения '{key}' по тэгу: {tag}");
                }
            }
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            FileManager.EnsureFileExists();
            var loadedData = LoadDataFromSection("Options");

            foreach (var kvp in loadedData)
            {
                string elementName = kvp.Key;
                string value = kvp.Value;

                // Обработка TextBox
                if (FindName(elementName) is TextBox textBox)
                {
                    textBox.Text = value;
                }

                // Обработка CheckBox
                else if (FindName(elementName) is CheckBox checkBox)
                {
                    bool isChecked = value == "1";
                    checkBox.IsChecked = isChecked;

                    // Получение индексов
                    int stackPanelIndex = GetStackPanelIndex(checkBox);
                    int checkBoxIndex = GetElementIndexInStackPanel(checkBox);

                    if (stackPanelIndex != -1 && checkBoxIndex != -1)
                    {
                        float numericValue = isChecked ? 1.0f : 0.0f;
                        SendToServer(stackPanelIndex, checkBoxIndex, numericValue);
                    }
                }
            }

            var dataToSave = new Dictionary<string, string>();
            string baseTag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcArrOptions";
            int stackPanelIndexLoop = 0;

            for (int i = 0; i < AllZone.Children.Count; i++)
            {
                if (AllZone.Children[i] is StackPanel stackPanel)
                {
                    int elementIndex = 0;

                    foreach (var child in stackPanel.Children)
                    {
                        if (child is TextBox textBox)
                        {
                            string rawText = textBox.Text?.Trim();
                            if (string.IsNullOrEmpty(rawText)) continue;

                            if (!float.TryParse(rawText, out float value))
                            {
                                MessageBox.Show($"Ошибка: введите число в поле {textBox.Name}");
                                return;
                            }

                            string tag = $"{baseTag}[{stackPanelIndexLoop},{elementIndex}]";

                            if (!OpcUaManager.OpcWrite(tag, value))
                            {
                                MessageBox.Show($"Ошибка записи по тэгу: {tag}");
                                return;
                            }

                            dataToSave[textBox.Name] = value.ToString("0.#####");
                            elementIndex++;
                        }

                        else if (child is CheckBox checkBox)
                        {
                            float value = checkBox.IsChecked == true ? 1.0f : 0.0f;

                            string tag = $"{baseTag}[{stackPanelIndexLoop},{elementIndex}]";

                            if (!OpcUaManager.OpcWrite(tag, value))
                            {
                                MessageBox.Show($"Ошибка записи по тэгу: {tag}");
                                return;
                            }

                            dataToSave[checkBox.Name] = value.ToString("0");
                            elementIndex++;
                        }
                    }

                    stackPanelIndexLoop++;
                }
            }

            SaveData("Options", dataToSave);

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
        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                int stackPanelIndex = GetStackPanelIndex(checkBox);
                int checkBoxIndex = GetElementIndexInStackPanel(checkBox);

                if (stackPanelIndex != -1 && checkBoxIndex != -1)
                {
                    try
                    {
                        float value = checkBox.IsChecked == true ? 1.0f : 0.0f;

                        // Отправляем данные на сервер
                        SendToServer(stackPanelIndex, checkBoxIndex, value);

                        // Загружаем текущие данные из файла
                        var data = LoadDataFromSection("Options");

                        // Обновляем или добавляем значение CheckBox
                        data[checkBox.Name] = value.ToString("0");

                        // Сохраняем обратно в файл
                        SaveData("Options", data);
                    }
                    catch (Exception ex)
                    {
                       // MessageBox.Show($"Ошибка при обработке состояния CheckBox: {ex.Message}");
                    }
                }
            }
        }


        private void SendToServer(int stackPanelIndex, int elementIndex,float value)
        {
            // Формируем тэг для текущего элемента массива
            string baseTag = "ns=4;s=|var|LS.Application.PLC_PRG.OpcArrOptions";
            string tag = $"{baseTag}[{stackPanelIndex},{elementIndex}]";

            // Пишем обновлённый элемент на сервер
            bool writeSuccess = OpcUaManager.OpcWrite(tag, value);

            if (!writeSuccess)
            {
                MessageBox.Show($"Ошибка записи значения {value} в {tag}.");
            }
        }

        private int GetElementIndexInStackPanel(UIElement element)
        {
            // Найти родительский StackPanel
            var parentStackPanel = LogicalTreeHelper.GetParent(element) as StackPanel;

            if (parentStackPanel != null)
            {
                // Отфильтровать только элементы TextBox и CheckBox
                var relevantChildren = parentStackPanel.Children
                    .OfType<UIElement>() // Ищем элементы, которые являются TextBox или CheckBox
                    .Where(child => child is TextBox || child is CheckBox)
                    .ToList();

                // Возвращаем индекс элемента внутри отфильтрованных детей
                return relevantChildren.IndexOf(element);
            }

            return -1; // Элемент не найден
        }


        private int GetStackPanelIndex(UIElement element)
        {
            var stackPanels = AllZone.Children.OfType<StackPanel>().ToList();

            foreach (var stackPanel in stackPanels)
            {
                // Используем LogicalTreeHelper для проверки
                if (stackPanel.Children.Contains(element))
                {
                    return stackPanels.IndexOf(stackPanel);
                }
            }

            return -1; // StackPanel не найден
        }

        

        private void ScrollViewer_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void OnOffAxis1_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                int stackPanelIndex = GetStackPanelIndex(checkBox);
                int checkBoxIndex = GetElementIndexInStackPanel(checkBox);

                if (stackPanelIndex != -1 && checkBoxIndex != -1)
                {
                    try
                    {
                        float value = checkBox.IsChecked == true ? 1.0f : 0.0f;

                        // Отправляем данные на сервер
                        SendToServer(stackPanelIndex, checkBoxIndex, value);

                        // Загружаем текущие данные из файла
                        var data = LoadDataFromSection("Options");

                        // Обновляем или добавляем значение CheckBox
                        data[checkBox.Name] = value.ToString("0");

                        // Сохраняем обратно в файл
                        SaveData("Options", data);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show($"Ошибка при обработке состояния CheckBox: {ex.Message}");
                    }
                }
            }
        }


        private void OnOffAxis1_Unchecked(object sender, RoutedEventArgs e)
        {

        }
        private bool isResetXPressed = false;
        private bool isResetYPressed = false;
        private bool isResetZPressed = false;
        private void ResetCalibrationX_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isResetXPressed = true;
            ResetCalibrationX.CaptureMouse();
            OpcUaManager.OpcWrite("ns=4;s=|var|LS.Application.PLC_PRG.calibration2", false);
        }

        private void ResetCalibrationX_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isResetXPressed)
            {
                isResetXPressed = false;
                ResetCalibrationX.ReleaseMouseCapture();
            }
        }

        private void ResetCalibrationY_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isResetYPressed = true;
            ResetCalibrationY.CaptureMouse();
            OpcUaManager.OpcWrite("ns=4;s=|var|LS.Application.PLC_PRG.calibration3", false);
        }

        private void ResetCalibrationY_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isResetYPressed)
            {
                isResetYPressed = false;
                ResetCalibrationY.ReleaseMouseCapture();
            }
        }

        private void ResetCalibrationZ_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isResetZPressed = true;
            ResetCalibrationZ.CaptureMouse();
            OpcUaManager.OpcWrite("ns=4;s=|var|LS.Application.PLC_PRG.calibration4", false);
        }

        private void ResetCalibrationZ_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isResetZPressed)
            {
                isResetZPressed = false;
                ResetCalibrationZ.ReleaseMouseCapture();
            }
        }

        private void ResetCalibrationX_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (isResetXPressed)
            {
                isResetXPressed = false;
            }
        }

        private void ResetCalibrationY_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (isResetYPressed)
            {
                isResetYPressed = false;
            }
        }

        private void ResetCalibrationZ_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (isResetZPressed)
            {
                isResetZPressed = false;
            }
        }
    }
}

