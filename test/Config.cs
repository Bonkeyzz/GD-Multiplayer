using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Config
    {
        public bool bGDWindowFound = false;
        public static IntPtr iGDWindow;
        public static string sIPAddress;
        public int iPort = 8612;
        public bool Success = false;

        public static Socket tcp_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static Socket tcp_server = null;
        public enum Types
        {
            TCP_HOST = 1,
            TCP_CLIENT = 2
        }

        public IPEndPoint IPEnd;

    }
}
