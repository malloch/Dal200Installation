using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dal200Instalation.Model;
using Dal200Instalation.Utils;
using Microsoft.Win32;

namespace Dal200Instalation.ViewModel
{
    class MainWindowViewModel: INotifyPropertyChanged
    {
        public int DTDTPort { get; set; } = 6666;
        public int DwellRadius { get; set; } = 400;
        public int DwellTime { get; set; } = 1;
        public FixedSizeObservablelist<string> OscMessages { get; }
        public string wsServerAddr { get; private set; }

        public int DwellX { get; set; }
        public int DwellY { get; set; }
        public string Track { get; set; }
        public string Media { get; set; }

        private Dal200Control exhibitControl;

        private ICommand startCommand;
        public ICommand StartCommand
        {
            get { return startCommand ?? (startCommand = new RelayCommand(call => StartDalControll())); }
        }

        private ICommand updateWellCommand;

        public ICommand UpdateWellCommand
        {
            get { return updateWellCommand ?? (updateWellCommand = new RelayCommand(call => UpdateDwellRadius()));
            }
        }
        

        private ICommand closeCommand;

        public ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand(call => HandleShutdown())); }
        }

        private ICommand sendFakeDwellCommand;

        public ICommand SendFakeDwellCommand
        {
            get { return sendFakeDwellCommand ?? (sendFakeDwellCommand = new RelayCommand(call => SendFakeDwell())); }
        }


        public MainWindowViewModel()
        {
            OscMessages = new FixedSizeObservablelist<string>(15);
        }

        private void StartDalControll()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                exhibitControl = new Dal200Control(DTDTPort, DwellRadius, DwellTime, openFileDialog.FileName);
                exhibitControl.dtdtHandler.OnDataReceived += data =>
                    OscMessages.Add($"{DateTime.UtcNow.ToString("T")} -> {data.ToString()}");
                wsServerAddr = $"ws://{NetworkUtils.GetLocalIPAddress()}/Dall200";
                OnPropertyChanged(nameof(wsServerAddr));
            }
        }

        private void UpdateDwellRadius()
        {
            exhibitControl?.DwellableCollection.ChangeRadius(DwellRadius);
        }

        private void HandleShutdown()
        {
            exhibitControl.Shutdown();
        }

        private void SendFakeDwell()
        {
            exhibitControl.SendFakeDwell(DwellX,DwellY,Track,Media);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
