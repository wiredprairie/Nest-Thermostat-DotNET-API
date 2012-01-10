using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using WiredPrairieUS.Devices;
using System.Collections.ObjectModel;

namespace WiredPrairieUS.Demo
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Device> Devices { get; private set;}

        public MainWindowViewModel()
        {
            Devices = new ObservableCollection<Device>();
        }



        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
