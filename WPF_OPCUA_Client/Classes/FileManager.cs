using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_OPCUA_Client.Classes
{
    public static class FileManager
    {
        private static readonly string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "STAN");
        private static readonly string filePath = Path.Combine(folderPath, "optionsSTAN.txt");
        private static readonly string filePath2 = Path.Combine(folderPath, "info.txt");

        public static void EnsureFileExists()
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "");
            }

            if (!File.Exists(filePath2))
            {
                File.WriteAllText(filePath2, "");
            }

            EnsureFileHasDefaultValues();
           // EnsureFile2HasDefaultValues();
        }

        private static void EnsureFileHasDefaultValues()
        {
            if (new FileInfo(filePath).Length == 0)
            {
                var defaultData = new List<string>
                {
                    "[Options]",
                    "VAxis2=15",
                    "VShadingZ=0",
                    "VAxis3=15",
                    "VAxis5=8",
                    "MinPosAxis2=-200",
                    "MaxVAxis4=20",
                    "MaxPosAxis2=200",
                    "MinRotSpeed=5",
                    "MinPosAxis3=-200",
                    "MaxRotSpeed=20",
                    "MaxPosAxis3=200",
                    "AccAxis2=200",
                    "MinPosAxis4=-20",
                    "AccAxis3=200",
                    "MaxPosAxis4=200",
                    "AccAxis5=200",
                    "AccAxis4=200",
                    "AccAxis1=20",
                    "Amp=20",
                    "MomentAxis2=0",
                    "Voltage=600",
                    "MomentAxis3=0",
                    "Power=600",
                    "MomentAxis4=0",
                    "MomentAxis1=20",
                    "DistanceIndSpr=20",
                    "Return=",
                    "OnOffAxis1=",
                    "MomentClamp",
                    "Tok=",
                    "RotationSpeed=",
                    "[Distance]",
                    "Checkbox0=",
                    "Checkbox1=",
                    "Checkbox2=",
                    "Checkbox3=",
                    "Checkbox4=",
                    "Checkbox5=",
                    "Checkbox6=",

                };

                File.WriteAllLines(filePath, defaultData);
            }
        }

        private static void EnsureFile2HasDefaultValues()
        {
            if (new FileInfo(filePath2).Length == 0)
            {
                var defaultData2 = new List<string>
                {
                    "[Model]",
                    "TIS 100/AC-HRP",
                    "[Power]",
                    "100 кВт",
                    "[InfoNum]",
                    "0507",
                    "[Date]",
                    "03/2025"
                };

                File.WriteAllLines(filePath2, defaultData2);
            }
        }

        public static string GetFilePath() => filePath;
        public static string GetFilePath2() => filePath2;

    }
}
