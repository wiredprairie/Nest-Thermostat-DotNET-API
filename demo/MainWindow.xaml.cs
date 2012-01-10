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

using System;
using System.Windows;
using WiredPrairieUS.Demo.Properties;
using WiredPrairieUS.Devices;
using System.IO;

namespace WiredPrairieUS.Demo
{
    public partial class MainWindow : Window
    {
        private Nest _nest;
        private dynamic _status;
        private MainWindowViewModel _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = new MainWindowViewModel();
            txtLogin.Text = Settings.Default.Email;
            txtPassword.Password = Settings.Default.Password;

            this.DataContext = _model;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            _nest = new Nest("test@example", "password");
            // you'll need your own test file (you can build one by grabbing the output of the calls
            string body = File.ReadAllText(@"d:\Aaron\Documents\nest-login1.txt");
            _nest.ParseAuthenticationResponse(body);
            // you'll need your own test file (you can build one by grabbing the output of the calls
            body = File.ReadAllText(@"d:\Aaron\Documents\nest-status1.txt");
            _nest.DeconstructStatus(body);
            UpdateDevices();
#else
            _nest = new Nest(txtLogin.Text, txtPassword.Password);
            _nest.AuthenticationComplete += new EventHandler<EventArgs>(Nest_AuthenticationComplete);
            _nest.StatusUpdated += new EventHandler<NestStatusUpdatedEventArgs>(Nest_StatusUpdated);
            _nest.BeginAuthenticate();

#endif
        }

        void Nest_StatusUpdated(object sender, NestStatusUpdatedEventArgs e)
        {
            this._status = e.Status;
            UpdateDevices();
        }

        private void UpdateDevices()
        {
            foreach (var structure in _nest.Structures)
            {
                foreach (var device in structure.Devices)
                {
                    _model.Devices.Add(device);
                }
            }
        }

        void Nest_AuthenticationComplete(object sender, EventArgs e)
        {
            Settings.Default.Email = txtLogin.Text;
            Settings.Default.Password = txtPassword.Password ;
            Settings.Default.Save();
            btnLogin.IsEnabled = false;

            _nest.BeginGetCurrentStatus();
        }
    }
}
