using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_OPCUA_Client.Views
{
    public static class SharedLogs
    {
        public static ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();
    }
}
