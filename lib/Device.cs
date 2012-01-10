/*
The MIT License

Copyright 2012 WiredPrairie.Us

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.ComponentModel;

namespace WiredPrairieUS.Devices
{
    public class Device : INotifyPropertyChanged
    {
        public string Id { get; private set; }
        public dynamic AllProperties { get; private set; }    
        
        public Device(dynamic jbody, string id)
        {
            Id = id;
            Update(jbody);
        }

        public void Update(dynamic jbody)
        {
            AllProperties = jbody.shared[Id];

            dynamic shared = AllProperties;
            HvacFanState = shared.hvac_fan_state;
            AutoAway = (shared.auto_away == 0 ? false : true);
            Name = shared.name;
            CanHeat = shared.can_heat;
            CurrentTemperatureC = shared.current_temperature;
            TargetTemperatureHighC = shared.target_temperature_high;
        }

        private bool _hvacFanState;
        public bool HvacFanState
        {
            get { return _hvacFanState; }
            private set
            {
                if (value != _hvacFanState)
                {
                    _hvacFanState = value;
                    RaisePropertyChanged("HvacFanState");
                }
            }
        }

        private bool _autoAway;
        public bool AutoAway
        {
            get { return _autoAway; }
            set
            {
                if (value != _autoAway)
                {
                    _autoAway = value;
                    RaisePropertyChanged("AutoAway");
                }
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            private set
            {
                if (value != _name)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        private double _currentTemperatureC;
        public double CurrentTemperatureC
        {
            get { return _currentTemperatureC; }
            private set
            {
                if (value != _currentTemperatureC)
                {
                    _currentTemperatureC = value;
                    RaisePropertyChanged("CurrentTemperatureC");
                    // Celcius sets Fahrenheit, always
                    CurrentTemperatureF = Nest.CelsiusToFohrenheit(_currentTemperatureC);
                }
            }
        }

        private double _currentTemperatureF;
        public double CurrentTemperatureF
        {
            get { return _currentTemperatureF; }
            private set
            {
                if (value != _currentTemperatureF)
                {
                    _currentTemperatureF = value;
                    RaisePropertyChanged("CurrentTemperatureF");
                }
            }
        }

        private bool _canHeat;
        public bool CanHeat
        {
            get { return _canHeat; }
            set
            {
                if (value != _canHeat)
                {
                    _canHeat = value;
                    RaisePropertyChanged("CanHeat");
                }
            }
        }

        private double _targetTemperatureHighC;
        public double TargetTemperatureHighC
        {
            get { return _targetTemperatureHighC; }
            set
            {
                if (value != _targetTemperatureHighC)
                {
                    _targetTemperatureHighC = value;
                    RaisePropertyChanged("TargetTemperatureHighC");
                    TargetTemperatureHighF = Nest.CelsiusToFohrenheit(value);
                }
            }
        }

        private double _targetTemperatureHighF;
        public double TargetTemperatureHighF
        {
            get { return _targetTemperatureHighF; }
            set
            {
                if (value != _targetTemperatureHighF)
                {
                    _targetTemperatureHighF = value;
                    RaisePropertyChanged("TargetTemperatureHighF");

                }
            }
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
