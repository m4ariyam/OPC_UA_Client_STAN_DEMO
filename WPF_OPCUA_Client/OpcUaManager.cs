using OpcUaHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_OPCUA_Client
{
    public class OpcUaManager
    {
        private static OpcUaClient _client;
        public static OpcUaClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new OpcUaClient();
                }
                return _client;
            }
        }

        public static void Connect(string serverUrl)
        {
            if (!Client.Connected)
            {
                Client.ConnectServer(serverUrl);
            }
        }

        public static void Disconnect()
        {
            if (Client.Connected)
            {
                Client.Disconnect();
            }
        }

        public  static T OpcRead<T>(string tag)
        {
            try
            {
                // Чтение значения с указанным типом
                var value = _client.ReadNode<T>(tag);
                return value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении тега {tag}: {ex.Message}");
                throw;
            }
        }
        public static bool OpcWrite<T>(string tag, T value)
        {
            try
            {
                
                var result = Client.WriteNode(tag, value); // Запись значения
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в тег {tag}: {ex.Message}");
                throw;
            }
        }

    }

}
