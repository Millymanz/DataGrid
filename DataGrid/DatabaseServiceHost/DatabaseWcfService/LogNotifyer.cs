using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace DatabaseWcfService
{
    public static class LogNotifyer
    {
        public static void SendMessage(String msg)
        {
            UdpClient udp = new UdpClient();

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, 11020);

            byte[] sendBytes4 = Encoding.ASCII.GetBytes(msg);
            udp.Send(sendBytes4, sendBytes4.Length, groupEP);
        }
    }
}
