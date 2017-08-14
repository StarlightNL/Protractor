using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Change_Scan_Time
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private string _consoleLog = String.Empty;
        public string ConsoleLog
        {
            get { return _consoleLog; }
            set
            {
                if (_consoleLog != value)
                {
                    _consoleLog = value;
                    OnPropertyChanged("ConsoleLog");
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
