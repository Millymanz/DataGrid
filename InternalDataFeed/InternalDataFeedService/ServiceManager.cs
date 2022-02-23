using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace InternalDataFeedService
{
    public class ServiceManager
    {
        DataFeedRunner _dataRunner = null;
        FaultyCommmunication _handler = null;
        String _name;
        String _modeStr;

        public static bool Ignore = false;

        public static FaultyCommmunication GlobalhandlerComFailure = null;
        public static bool DataFeedRestarted = false;

        public static readonly object _lock = new object();

        static Timer _timer;
        static bool _allowErrorCall = true;

        public ServiceManager(String name, String modeStr)
        {
            _timer = new Timer(120000); //2min
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true; // Enable it


            _name = name;
            _modeStr = modeStr;

            _handler += CommunicationFaultRestart;
            GlobalhandlerComFailure = _handler;

            var realtimeRun = System.Configuration.ConfigurationManager.AppSettings["REALTIME_RUN"];
            if (realtimeRun == "TRUE")
            {
                var startTh = new System.Threading.ThreadStart(AppAliveConfirmerTCP);
                var thread = new System.Threading.Thread(startTh);
                thread.Start();
            }


            _dataRunner = new DataFeedRunner(_handler, name, modeStr);
        }

        public void AppAliveConfirmer()
        {
            // PopulateMachinePairing();

            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }

            UdpClient udpClient = new UdpClient(11874);

            while (true)
            {
                udpClient.EnableBroadcast = true;
                String errorKey = "";
                try
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(localIP), 0);

                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    if (returnData == "IDF_Alive" && ServiceManager.Ignore == false)
                    {
                        SendMessage("IDF_Alive_Confirmed_DoNotWorry", 11875);
                    }
                    else
                    {
                        Logger.log.Info("AppAliveConfirmer :: Ignored :: No Stream Data");
                        Library.WriteErrorLog(DateTime.Now + "AppAliveConfirmer :: Ignored :: No Stream Data");

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Occurred With Key : - " + errorKey);
                    Console.WriteLine(e.ToString());
                }
            }
            udpClient.Close();
        }
        public void AppAliveConfirmerTCP()
        {

            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }

            Socket sock = null;

            try
            {
                IPAddress ipAd = IPAddress.Parse(localIP);
                // use local m/c IP address, and 
                // use the same in the client

                /* Initializes the Listener */
                TcpListener myList = new TcpListener(ipAd, 11874);

                /* Start Listeneting at the specified port */
                myList.Start();

                while (true)
                {
                    Console.WriteLine("The server is running at port 8001...");
                    Console.WriteLine("The local End point is  :" +
                                      myList.LocalEndpoint);
                    Console.WriteLine("Waiting for a connection.....");

                    Socket s = myList.AcceptSocket();
                    sock = s;
                    Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                    byte[] b = new byte[100];
                    int k = s.Receive(b);
                    Console.WriteLine("Recieved...");
                    for (int i = 0; i < k; i++)
                        Console.Write(Convert.ToChar(b[i]));
                    
                    ASCIIEncoding asen = new ASCIIEncoding();
                    
                    if (ServiceManager.Ignore == false)
                    {                       
                        s.Send(asen.GetBytes("The string was recieved by the server."));
                        Console.WriteLine("\nSent Acknowledgement");
                    }
                    else
                    {
                        s.Send(asen.GetBytes("Failed"));
                        Logger.log.Info("AppAliveConfirmerTCP :: Ignored :: No Stream Data");
                        Library.WriteErrorLog(DateTime.Now + "AppAliveConfirmerTCP :: Ignored :: No Stream Data");
                    }


                }
                /* clean up */
                sock.Close();
                myList.Stop();

            }
            catch (Exception e)
            {
                if (sock != null) sock.Close();
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }


        public void Start()
        {
            _dataRunner.Run();
        }
        
        private void CommunicationFaultRestart()
        {
           // ServiceManager.Ignore = true;

            Console.WriteLine(DateTime.Now + ":: [CommunicationFaultRestart]");
            Logger.log.Info("[CommunicationFaultRestart]");
            Library.WriteErrorLog(DateTime.Now + ":: [CommunicationFaultRestart]");


        }

        private void CommunicationFaultRestartOld()
        {
            lock (_lock)
            {
                 var realtimeRun = System.Configuration.ConfigurationManager.AppSettings["REALTIME_RUN"];

                 if (realtimeRun == "TRUE")
                 {
                     if (DataFeedRestarted == false)
                     {
                         DataFeedRestarted = true;
                         SendFaultEmail();

                         var launch = System.Configuration.ConfigurationManager.AppSettings["RESTARTSYSTEMS"];
                         SendMessage(launch, 11098); //live
                         RestartDevApps(launch); //dev

                         SendMessage("IDF_Fault");                 
                     }
                 }
                 else
                 {
                     //only allow one error call per minute or 2 min
                     //set timer to reset flag for allowing error calls
                     //if allowErrorCall
                     //allowErrorCall = false

                     //timer then resets to true
                     if (_allowErrorCall)
                     {
                         _allowErrorCall = false;
                         SendMessage("IDF_Fault_OnDemand", 19020);
                         SendMessage("IDF_Fault_OnDemand", 19074);

                         RestartDataConsumer("IDF_Fault_OnDemand");
                     }
                 }
            }
        }

        static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _allowErrorCall = true;
        }

        private void SendFaultEmail()
        {
            var message = new StringBuilder();

            message.AppendLine("Internal DataFeed Encountered an error\n");
            message.AppendLine("Check Error Log File, it might be WCF Fault State Error Or IqFeed Failure\n");
            message.AppendLine("Date :" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            new EmailManager().Send(null, "Internal DataFeed Communication Fault Error ", message.ToString());
            Console.WriteLine("Fault Email Sent");
            Logger.log.Info("Email is now Sent");
        }

        public void SendMessage(String msg)
        {
            //Send to LIVE Machines as well
            var recipientsStr = System.Configuration.ConfigurationManager.AppSettings["NOTIFICATION_RECIPIENT"];
            var recipients = recipientsStr.Split('|');

            foreach (var item in recipients)
            {
                UdpClient udp = new UdpClient();

                var groupEP = new IPEndPoint(IPAddress.Parse(item), 19020); //make dynamic
                var sendBytes4 = Encoding.ASCII.GetBytes(msg);
                udp.Send(sendBytes4, sendBytes4.Length, groupEP);
            }
        }

        public void RestartDevApps(String msg)
        {
            UdpClient udp = new UdpClient();
            var recipientsStr = System.Configuration.ConfigurationManager.AppSettings["DEVMACHINES"];

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(recipientsStr), 11024); 

            byte[] sendBytes4 = Encoding.ASCII.GetBytes(msg);
            udp.Send(sendBytes4, sendBytes4.Length, groupEP);
        }

        public void RestartDataConsumer(String msg)
        {
            UdpClient udp = new UdpClient();
            var recipientsStr = System.Configuration.ConfigurationManager.AppSettings["DEVMACHINES"];

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(recipientsStr), 11078);

            byte[] sendBytes4 = Encoding.ASCII.GetBytes(msg);
            udp.Send(sendBytes4, sendBytes4.Length, groupEP);
        } 

        public void SendMessage(String msg, int port)
        {
            //Send to LIVE Machines as well
            var recipientsStr = System.Configuration.ConfigurationManager.AppSettings["NOTIFICATION_RECIPIENT"];
            var recipients = recipientsStr.Split('|');

            foreach (var item in recipients)
            {
                UdpClient udp = new UdpClient();

                var groupEP = new IPEndPoint(IPAddress.Parse(item), port); //make dynamic
                var sendBytes4 = Encoding.ASCII.GetBytes(msg);
                udp.Send(sendBytes4, sendBytes4.Length, groupEP);
            }

        }

        private void ReportFaultState()
        {
            //send email to admin
        }
    }
}
