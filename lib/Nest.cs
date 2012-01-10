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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WiredPrairieUS.Devices
{
    public class Nest : INotifyPropertyChanged
    {
        public static string AuthenticationUrl = "https://home.nest.com/user/login"; //"https://home.nest.com/accounts/login";

        public event EventHandler<EventArgs> AuthenticationComplete;
        public event EventHandler<FailedAuthenticationEventArgs> AuthenticationFailed;
        public event EventHandler<NestStatusUpdatedEventArgs> StatusUpdated;
        public event EventHandler<NestExceptionEventArgs> ErrorOccurred;

        public string UserName { get; private set; }
        public string Password { get; private set; }

        public string AccessToken { get; private set; }
        public DateTime AccessTokenExpiration { get; private set; }
        public string UserId { get; private set; }
        public string APIUrl { get; private set; }
        public string TransportAPIUrl { get; private set; }
        public string WeatherUrl { get; private set; }

        // results
        private ObservableCollection<Structure> _structures = new ObservableCollection<Structure>();



        public Nest(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }

        // Properties
        public ObservableCollection<Structure> Structures
        {
            get { return _structures; }
        }


        public static int CelsiusToFohrenheit(double c)
        {
            return (int) Math.Round(c * (9 / 5.0) + 32.0);
        }

        // HttpUtility.UrlEncode(iCalStr);
        public void BeginAuthenticate()
        {
            HttpWebRequest request = WebRequest.Create(AuthenticationUrl) as HttpWebRequest;
            request.Method = "POST";
            request.UserAgent = "Nest/1.1.0.10 CFNetwork/548.0.4";            
            request.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(WriteEncodedFormParameter("username", this.UserName));
                writer.Write("&");
                writer.Write(WriteEncodedFormParameter("password", this.Password));
            }

            request.BeginGetResponse(AuthenticateCallback, 
                new AsyncHttpRequestState { Request = request });
        }

        private void AuthenticateCallback(IAsyncResult result)
        {
            AsyncHttpRequestState state = result.AsyncState as AsyncHttpRequestState;
            if (state != null)
            {
                try
                {
                    HttpWebResponse response = state.Request.EndGetResponse(result) as HttpWebResponse;
                    string sessionid = GetCookieValueOrDefault(response.Cookies, "sessionid");
                    string cztoken = GetCookieValueOrDefault(response.Cookies, "cztoken");
                    string responseBody;

                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            responseBody = reader.ReadToEnd();
                            try
                            {
                                ParseAuthenticationResponse(responseBody);
                                OnAuthenticationComplete();
                            }
                            catch (Exception ex)
                            {
                                OnAuthenticationFailure(ex);
                                Debug.WriteLine(ex);
                            }
                        }
                    }

                    response.Close();
                }
                catch (Exception ex)
                {
                    OnAuthenticationFailure(ex);
                    Debug.WriteLine(ex);
                }

            }
        }

        public void ParseAuthenticationResponse(string responseBody)
        {
            dynamic jBody = JsonConvert.DeserializeObject(responseBody);
            if (jBody != null)
            {
                AccessToken = jBody.access_token;
                UserId = jBody.userid;
                var urls = jBody.urls;
                if (urls != null)
                {
                    APIUrl = urls.rubyapi_url;
                    TransportAPIUrl = urls.transport_url;
                    WeatherUrl = urls.weather_url;
                }
            }
        }



        protected virtual void OnAuthenticationFailure(Exception ex)
        {
            if (AuthenticationFailed != null)
            {
                AuthenticationFailed(this, new FailedAuthenticationEventArgs(ex));
            }
        }

        protected virtual void OnAuthenticationComplete ()
        {
            if (AuthenticationComplete != null)
            {
                AuthenticationComplete(this, EventArgs.Empty);
            }
        }

        public void BeginGetCurrentStatus()
        {
            var transport = string.Format("{0}/v2/mobile/user.{1}",
                TransportAPIUrl,
                UserId); 

            HttpWebRequest request = WebRequest.Create(transport) as HttpWebRequest;            
            request.Method = "GET";
            request.UserAgent = "Nest/1.1.0.10 CFNetwork/548.0.4";
            request.Host = new Uri(TransportAPIUrl).Host;            
            request.Headers.Set("X-nl-client-timestamp", GetDateAsJsonDate());
            request.Headers.Set("X-nl-protocol-version", "1");
            request.Headers.Set("Accept-Language", "en-us");
            request.Headers.Set("X-nl-user-id", UserId);
            request.Headers.Set("Authorization", string.Format("Basic {0}", AccessToken));

            request.BeginGetResponse(GetCurrentStatusCallback,
                new AsyncHttpRequestState { Request = request });
        }

        private void GetCurrentStatusCallback(IAsyncResult result)
        {
            AsyncHttpRequestState state = result.AsyncState as AsyncHttpRequestState;
            if (state != null)
            {
                try
                {
                    HttpWebResponse response = state.Request.EndGetResponse(result) as HttpWebResponse;
                    string responseBody;

                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            responseBody = reader.ReadToEnd();
                            try
                            {
                                dynamic jBody = JsonConvert.DeserializeObject(responseBody);
                                if (jBody != null)
                                {
                                    DeconstructStatus(jBody);
                                    OnStatusUpdated(jBody);
                                }
                            }
                            catch (Exception ex)
                            {
                                OnNestException(ex);
                                Debug.WriteLine(ex);
                            }
                        }
                    }

                    response.Close();
                }
                catch (Exception ex)
                {
                    OnNestException(ex);
                    Debug.WriteLine(ex);
                }

            }
        }

        public void DeconstructStatus(string jsonBody)
        {
            DeconstructStatus(JsonConvert.DeserializeObject(jsonBody));
        }

        public void DeconstructStatus(dynamic jBody)
        {            

            JArray structures = jBody.user[UserId].structures;
            if (structures != null)
            {
                foreach (var structure in structures)
                {
                    _structures.Add(new Structure(jBody, (string) structure));
                }
            }
            
        }

        protected virtual void OnStatusUpdated(dynamic status)
        {
            if (StatusUpdated != null)
            {
                StatusUpdated(this, new NestStatusUpdatedEventArgs(status));
            }
        }

        protected virtual void OnNestException(Exception ex)
        {
            if (ErrorOccurred != null)
            {
                ErrorOccurred(this, new NestExceptionEventArgs(ex));
            }
        }
        private string GetDateAsJsonDate()
        {
            return GetDateAsJsonDate(DateTime.Now);
        }

        private string GetDateAsJsonDate(DateTime date)
        {
            DateTime epoch = new DateTime(1970, 1, 1);
            DateTime ut = date.ToUniversalTime();
            TimeSpan ts = new TimeSpan(ut.Ticks - epoch.Ticks);
            return Math.Round(ts.TotalMilliseconds).ToString();
        }

        private string WriteEncodedFormParameter(string key, string value)
        {
            return WriteEncodedFormParameter(key, value, false);
        }

        private string WriteEncodedFormParameter(string key, string value, bool isUrl)
        {
            
            var encoded = string.Format("{0}={1}", Uri.EscapeUriString(key), Uri.EscapeUriString(value));
            return (isUrl ? encoded.Replace("%20", "+") : encoded);
        }

        private string GetCookieValueOrDefault(CookieCollection cookies, string name)
        {
            Cookie cookie = cookies[name];
            if (cookie == null) { return ""; }

            return cookie.Value as string;
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

public class AsyncHttpRequestState
{
    public HttpWebRequest Request { get; set; }
}


public class NestExceptionEventArgs : EventArgs
{
    public Exception Exception { get; private set; }

    public NestExceptionEventArgs(Exception ex)
    {
        this.Exception = ex;
    }
}

public class NestStatusUpdatedEventArgs : EventArgs
{
    public dynamic Status { get; private set;  }

    public NestStatusUpdatedEventArgs(dynamic status) 
    {
        this.Status = status;
    }


}

public class FailedAuthenticationEventArgs : EventArgs
{
    public Exception Exception { get; private set; }

    public FailedAuthenticationEventArgs(Exception ex)
    {
        this.Exception = ex;
    }


}