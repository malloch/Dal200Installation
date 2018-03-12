using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dal200Instalation.Model;
using Dal200Instalation.Utils;

namespace Dal200Instalation.ViewModel
{
    class MainWindowViewModel: INotifyPropertyChanged
    {
        public int DTDTPort { get; set; } = 5001;
        public int DwellRadius { get; set; } = 2;
        public int DwellTime { get; set; } = 3;
        public FixedSizeObservablelist<string> OscMessages { get; }

        private Dal200Control exhibitControl;

        private ICommand startCommand;
        public ICommand StartCommand
        {
            get { return startCommand ?? (startCommand = new RelayCommand(call => StartDalControll())); }
        }

        private ICommand closeCommand;

        public ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand(call => HandleShutdown())); }
        }
        
        public MainWindowViewModel()
        {
            OscMessages = new FixedSizeObservablelist<string>(15);
        }

        private void StartDalControll()
        {
            exhibitControl = new Dal200Control(DTDTPort, DwellRadius, DwellTime);
            exhibitControl.dtdtHandler.OnDataReceived += data => OscMessages.Add($"{DateTime.UtcNow.ToString("T")} -> {data.ToString()}");
        }

        private void HandleShutdown()
        {
            exhibitControl.Shutdown();
        }




        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
