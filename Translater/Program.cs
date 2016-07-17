using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MicrosoftTranslatorSdk.HttpSamples
{
    class Program
    {
        static AdmAccessToken admToken;
        static void Main(string[] args)
        {
            string headerValue;
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication("TEdit", "yucBdwxsg7cYQNs+Uf1JUvL8FGJHwZC2eYhbeCfITrk=");
            try
            {
                admToken = admAuth.GetAccessToken();
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.access_token;
                //TranslateMethod(headerValue);
                //Udp();
                Tcp();
            }
            catch (WebException e)
            {
                ProcessWebException(e);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
        private static void DetectMethod(string authToken)
        {
            Console.WriteLine("Enter Text to detect language:");
            string textToDetect = Console.ReadLine();
            //Keep appId parameter blank as we are sending access token in authorization header.
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Detect?text=" + textToDetect;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            WebResponse response = null;
            try
            {
                response = httpWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    string languageDetected = (string)dcs.ReadObject(stream);
                    Console.WriteLine(string.Format("Language detected:{0}", languageDetected));
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }
        private static string TranslateMethod(/*string authToken, */string text="")
        {
            //Console.WriteLine("Enter Text to tranlate language:");
            //text = Console.ReadLine();
            string from = "en";
            string to = "zh";
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + System.Web.HttpUtility.UrlEncode(text) + "&from=" + from + "&to=" + to;
            string authToken = "Bearer " + admToken.access_token;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            WebResponse response = null;
            string result = "";
            try
            {
                response = httpWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    result = (string)dcs.ReadObject(stream);
                    //Console.WriteLine(string.Format("Text translate:{0}", languageDetected));
                    //Console.WriteLine("Press any key to continue...");
                    //if(Console.ReadLine() == "Q")
                    //{

                    //}else
                    //{
                    //    TranslateMethod(authToken);
                    //}

                }
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }
        private static void ProcessWebException(WebException e)
        {
            Console.WriteLine("{0}", e.ToString());
            // Obtain detailed error information
            string strResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)e.Response)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                    {
                        strResponse = sr.ReadToEnd();
                    }
                }
            }
            Console.WriteLine("Http status code={0}, error message={1}", e.Status, strResponse);
        }


        static void Udp()
        {
            int recv;
            byte[] data = new byte[4096];

            //得到本机IP，设置TCP端口号         
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 12001);
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //绑定网络地址
            newsock.Bind(ip); newsock.ReceiveBufferSize = 4096;

            Console.WriteLine("This is a Server, host name is {0}", ip.Address);


            //得到客户机IP
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            Task t = Task.Factory.StartNew(new Action(() =>
            {
                while (true)
                {
                    try
                    {
                        //等待客户机连接
                        Console.WriteLine("\n等待客户机连接");
                        recv = newsock.ReceiveFrom(data, ref Remote);
                        string text = Encoding.ASCII.GetString(data, 0, recv);
                        if(text == "_exit_")
                        {
                            //break;
                        }
                        Console.Write("\nGet " + text);
                        text = TranslateMethod(text);
                        data = Encoding.ASCII.GetBytes(text);
                        //发送信息
                        newsock.SendTo(data, data.Length, SocketFlags.None, Remote);
                        Console.Write("Return " + text);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }));

            Task.Factory.StartNew(new Action(() =>
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                client.Connect(IPAddress.Parse("127.0.0.1"), 12001);
                client.Send(Encoding.ASCII.GetBytes("welcome"));
                client.Disconnect(true);
                client.Close(5);
            }));

            t.Wait();
        }

        static void Tcp()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 12001));//ip为服务器IP地址，port为监听的端口
            //listener.Start();//开启监听
            Console.WriteLine("Begin linsten...");
            Task t = Task.Factory.StartNew(new Action(() =>
            {
                while (true)
                {
                    try
                    {
                        //Console.WriteLine("Begin linsten...");
                        listener.Start();
                        TcpClient c = listener.AcceptTcpClient();
                        c.ReceiveBufferSize = 512;
                        NetworkStream streamToClient = c.GetStream();
                        Task.Factory.StartNew(new Action(() =>
                        {
                            DoSend(streamToClient);
                        }));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        listener.Stop();
                    }
                }
            }));

            t.Wait();
        }
        static void DoSend(NetworkStream streamToClient)
        {
            //Console.WriteLine("Begin receive...");
            byte[] buffer = new byte[512];  // BufferSize为缓存的大小
            int bytesRead;
            while (true)
            {

                lock (streamToClient)//为了保证数据的完整性以及安全性  锁定数据流
                {
                    try
                    {
                        bytesRead = streamToClient.Read(buffer, 0, 512);
                        string text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        if (text == "_exit_")
                        {
                            break;
                        }
                        Console.Write("\nGet: " + text);
                        text = TranslateMethod(text);
                        buffer = Encoding.UTF8.GetBytes(text);
                        //发送信息
                        streamToClient.Write(buffer, 0, buffer.Length);

                        Console.Write("\nReturn: " + text);
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e.ToString());
                        //streamToClient.Close();
                        //break;
                    }
                }
            }
            streamToClient.Close();
        }
    }
    [DataContract]
    public class AdmAccessToken
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string scope { get; set; }
    }
    public class AdmAuthentication
    {
        public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        private string clientId;
        private string clientSecret;
        private string request;
        private AdmAccessToken token;
        private Timer accessTokenRenewer;
        //Access token expires every 10 minutes. Renew it every 9 minutes only.
        private const int RefreshTokenDuration = 9;
        public AdmAuthentication(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            //If clientid or client secret has special characters, encode before sending request
            this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
            this.token = HttpPost(DatamarketAccessUri, this.request);
            //renew the token every specfied minutes
            accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback), this, TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
        }
        public AdmAccessToken GetAccessToken()
        {
            return this.token;
        }
        private void RenewAccessToken()
        {
            AdmAccessToken newAccessToken = HttpPost(DatamarketAccessUri, this.request);
            //swap the new token with old one
            //Note: the swap is thread unsafe
            this.token = newAccessToken;
            Console.WriteLine(string.Format("Renewed token for user: {0} is: {1}", this.clientId, this.token.access_token));
        }
        private void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                RenewAccessToken();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed renewing access token. Details: {0}", ex.Message));
            }
            finally
            {
                try
                {
                    accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
                }
            }
        }
        private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
        {
            //Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
            webRequest.ContentLength = bytes.Length;
            using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                //Get deserialized object from JSON stream
                AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                return token;
            }
        }
    }
}