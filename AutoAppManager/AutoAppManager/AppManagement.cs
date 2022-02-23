using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Configuration;

using System.Timers;

namespace AutoAppManager
{
    public class AppManagement
    {
        public void CheckAppIsAlive()
        {

            //2. 
            var launch = System.Configuration.ConfigurationManager.AppSettings["RunCheckBat"];

            Console.WriteLine("App Restart In Progress");
            Process processLaunch = Process.Start(launch);

            //3. Notify dependant apps to restart or reconnect
        }

        public void ReLaunchOnDemandInternalFeed()
        {
            var launch = System.Configuration.ConfigurationManager.AppSettings["RunOnDemandIDF"];

            Console.WriteLine("OnDemand Progress");
            Process processLaunch = Process.Start(launch);
        }

        //Listen out for internal feed requesting to be restarted if comm fault occurs
    }

    public static class CheckAppIsAlive // In App_Code folder
    {
        static Timer _timer; // From System.Timers
        static bool _appAlive = false;

        public static void Start()
        {
            var startTh = new System.Threading.ThreadStart(AliveConfirmListen);
            var thread = new System.Threading.Thread(startTh);
            thread.Start();

            int timer = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CheckTimerFrequency"].ToString());

            _timer = new Timer(timer); 

            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true; // Enable it
        }

        static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //var comListen = new CommandListenner();
            //comListen.SendMessage("IDF_Alive", 11874);  

            SendAliveTCP();

            if (_appAlive == false)
            {
                AppManagement app = new AppManagement();
                app.CheckAppIsAlive();
            }

            //var comListen = new CommandListenner();
            //comListen.SendMessage("IDF_Alive", 11874);            
        }

        static public void 
            AliveConfirmListen()
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

            UdpClient udpClient = new UdpClient(11875);

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

                    if (returnData == "IDF_Alive_Confirmed_DoNotWorry")
                    {
                        _appAlive = true;
                    }
                    else
                    {
                        _appAlive = false;
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
        
        static public void SendAliveTCP()
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

            try
            {
                TcpClient tcpclnt = new TcpClient();
                Console.WriteLine("Connecting.....");

                tcpclnt.Connect(localIP, 11874);
                // use the ipaddress as in the server program

                Console.WriteLine("Connected");

                String str = "IDF_Alive";
                Stream stm = tcpclnt.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(str);
                Console.WriteLine("Transmitting.....");

                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[100];
                int k = stm.Read(bb, 0, 100);

                _appAlive = true;

                tcpclnt.Close();
            }

            catch (Exception e)
            {
                _appAlive = false;

                CommandListenner gmg = new CommandListenner();
                var launch = System.Configuration.ConfigurationManager.AppSettings["RESTARTSYSTEMS"];
                gmg.SendMessage(launch, 11098);

                Console.WriteLine("App Not ALIVE");
            }
        }
    }


    public class ServerType
    {
        public String Type = "ALL";
        public int WaitingPeriod = 0;
    }

    public class CommandListenner
    {
        private String _machine = "";
        private ServerType _serverType = null;

        public CommandListenner()
        {

        }



        public void ListenForCommands()
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

            UdpClient udpClient = new UdpClient(19020);//make listenning port dynamic //10198

            while (true)
            {
                udpClient.EnableBroadcast = true;
                String errorKey = "";
                try
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(localIP), 0);

                    Console.WriteLine("");
                    Console.WriteLine("[Listenning for Fault Issues ..]");
                    Console.WriteLine("");


                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    if (returnData == "IDF_Fault")
                    {
                        AppManagement app = new AppManagement();
                        app.CheckAppIsAlive();

                        System.Threading.Thread.Sleep(40000);
                    }
                    else if (returnData == "IDF_Fault_OnDemand")
                    {
                        AppManagement app = new AppManagement();
                        app.ReLaunchOnDemandInternalFeed();

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

        public void SendMessage(String msg)
        {
            UdpClient udp = new UdpClient();

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, 19414); //restart dependpant apps

            byte[] sendBytes4 = Encoding.ASCII.GetBytes(msg);
            udp.Send(sendBytes4, sendBytes4.Length, groupEP);

            //Send to LIVE Machines as well
            var recipientsStr = System.Configuration.ConfigurationManager.AppSettings["NOTIFICATION_RECIPIENT"];
            var recipients = recipientsStr.Split('|');

            foreach (var item in recipients)
            {
                groupEP = new IPEndPoint(IPAddress.Parse(item), 19414); //make dynamic
                sendBytes4 = Encoding.ASCII.GetBytes(msg);
                udp.Send(sendBytes4, sendBytes4.Length, groupEP);
            }
        }

        public void SendMessage(String msg, int port)
        {
            UdpClient udp = new UdpClient();

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, port); //make dynamic

            byte[] sendBytes4 = Encoding.ASCII.GetBytes(msg);
            udp.Send(sendBytes4, sendBytes4.Length, groupEP);

            //Send to LIVE Machines as well
            var recipientsStr = System.Configuration.ConfigurationManager.AppSettings["NOTIFICATION_RECIPIENT"];
            var recipients = recipientsStr.Split('|');

            foreach (var item in recipients)
            {
                groupEP = new IPEndPoint(IPAddress.Parse(item), port); //make dynamic
                sendBytes4 = Encoding.ASCII.GetBytes(msg);
                udp.Send(sendBytes4, sendBytes4.Length, groupEP);
            }
        }

    }
}
