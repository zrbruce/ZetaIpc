﻿namespace ZetaIpc.Runtime.Client
{
    using Helper;
    using System.IO;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Simple HTTP-based client to send strings to an IpcServer instance and 
    /// get back strings in response.
    /// </summary>
    public class IpcClient
    {
        private int _port;

        /// <summary>
        /// Initialized to connect to an IcpServer running on 127.0.0.1:port.
        /// </summary>
        public void Initialize(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Sends a string to the server, gets the response string back.
        /// Works synchronously, so better call it from within a background
        /// thread to keep the UI responsive.
        /// </summary>
        public string Send(string request)
        {
            using (var wc = new MyWebClient())
            {
                try
                {
                    return wc.UploadString(url, @"POST", request ?? string.Empty);
                }
                catch (WebException x)
                {
                    // Try to give user more details (which might have been
                    // marshalled from the server).

                    if (x.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (x.Response is HttpWebResponse response &&
                            response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                if (stream != null)
                                {
                                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                                    {
                                        var resp = reader.ReadToEnd();
                                        if (ExceptionFromXmlLight.IsSerializedException(resp))
                                        {
                                            throw new IpcClientException(new ExceptionFromXmlLight(resp), x);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    throw;
                }
            }
        }

        private string url => $@"http://127.0.0.1:{_port}";
    }
}