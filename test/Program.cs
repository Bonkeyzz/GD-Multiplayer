using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test
{
    class Program
    {
        public static int iType;
        static void InitializeVariables()
        {
            Config cfg = new Config();
            new MainFunctions().ConsoleWrite("[GDMP Log] -> Waiting for Geometry Dash...", ConsoleColor.White);
            while (Config.iGDWindow == IntPtr.Zero)
            {
                Config.iGDWindow = GDNative.FindWindow(default(string), "Geometry Dash");
            }
            Console.Clear();
        }

        void SendSocketMessage(Socket Sock, string Data)
        {
            byte[] temp = Encoding.UTF8.GetBytes(Data);
            try
            {
                Sock.Send(temp, SocketFlags.None);
            }
            catch (Exception)
            {
                new MainFunctions().ConsoleWrite("[GDMP Log] -> Failed to send message!", ConsoleColor.Red);
            }
            new MainFunctions().ConsoleWrite("[GDMP Log] -> Message Sent.", ConsoleColor.White);
        }
        string ReceiveSocketMessage(Socket Sock, int Size)
        {
            byte[] temp = new byte[Size];
            Sock.Receive(temp, SocketFlags.None);
            if (temp != null)
            {
                return Encoding.UTF8.GetString(temp);
            }
            return null;
        }

        private bool P1Jump = false;
        private bool P2Jump = false;
        void thread_InitializeTCP()
        {
            Config cfg = new Config();
            MainFunctions mf = new MainFunctions();
            Thread keylistener = new Thread(thread_KeyPressDetection);
            Thread tcpthread = new Thread(thread_tcpreceivesend);
            cfg.IPEnd = new IPEndPoint(IPAddress.Parse(Config.sIPAddress), cfg.iPort);

            switch (iType)
            {
                case (int) Config.Types.TCP_HOST:
                {
                    new MainFunctions().ConsoleWrite($"[GDMP Log] -> Detected Type: 1", ConsoleColor.White);
                    using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        new MainFunctions().ConsoleWrite(
                            $"[GDMP Log] -> Bound IP Address: {cfg.IPEnd.Address} with Port: {cfg.IPEnd.Port}",
                            ConsoleColor.White);
                        server.Bind(cfg.IPEnd);
                        new MainFunctions().ConsoleWrite($"[GDMP Log] -> Listening for 1 client...", ConsoleColor.White);
                        server.Listen(1); // Number of players that are can join.
                        new MainFunctions().ConsoleWrite($"[GDMP] Server is running and is waiting for Player 2...",
                            ConsoleColor.Green);
                            // using (Socket accepted = server.Accept())
                        Config.tcp_server = server.Accept();
                            SendSocketMessage(Config.tcp_server, "connection_success");

                            while (true)
                            {
                                string connectionbuffer = ReceiveSocketMessage(Config.tcp_server, 18);
                                if (connectionbuffer == "connection_success")
                                {
                                    mf.ConsoleWrite("Player 2 Connected Successfully!", ConsoleColor.Green);
                                    new MainFunctions().ConsoleWrite(
                                        "[GDMP Notice] Player2's Color will be the opposite of Player1's color!",
                                        ConsoleColor.Yellow);
                                    break;
                                }

                            }
                            keylistener.Start();
                            new MainFunctions().ConsoleWrite("[GDMP Log] -> Key listener thread started!",
                                ConsoleColor.Yellow);
                            tcpthread.Start();
                            new MainFunctions().ConsoleWrite("[GDMP Log] -> TCP Handler thread started!",
                                ConsoleColor.Yellow);

                    }
                        break;
               }
                case (int) Config.Types.TCP_CLIENT:
                   {
                    new MainFunctions().ConsoleWrite($"[GDMP Log] -> Detected Type: 2", ConsoleColor.White);
                        new MainFunctions().ConsoleWrite(
                            $"[GDMP] Connecting to IP: {cfg.IPEnd.Address} with Port: {cfg.IPEnd.Port}...",
                            ConsoleColor.White);
                        try
                        {
                            Config.tcp_client.Connect(cfg.IPEnd);
                        }
                        catch (Exception)
                        {
                            new MainFunctions().ConsoleWrite("[GDMP Log] -> Connection Refused", ConsoleColor.Red);
                        }



                        SendSocketMessage(Config.tcp_client, "connection_success");
                        while (Config.tcp_client.Connected)
                        {
                            try
                            {
                                string connectionbuffer = ReceiveSocketMessage(Config.tcp_client, 18);
                                if (connectionbuffer == "connection_success")
                                {
                                    mf.ConsoleWrite("Connected to Server!", ConsoleColor.Green);
                                    new MainFunctions().ConsoleWrite(
                                        "[GDMP Notice] Your Color will be the opposite of Player1's color!",
                                        ConsoleColor.Yellow);
                                        break;
                                }
                            }
                            catch
                                (SocketException ex)
                            {

                            }

                        }
                        keylistener.Start();
                        new MainFunctions().ConsoleWrite("[GDMP Log] -> Key listener thread started!",
                            ConsoleColor.Yellow);
                        tcpthread.Start();
                        new MainFunctions().ConsoleWrite("[GDMP Log] -> TCP Handler thread started!",
                            ConsoleColor.Yellow);

                    break;
                }
            }
        }
        void thread_KeyPressDetection()
        {
            while (true)
            {
                short P1state = GDNative.GetAsyncKeyState(Keys.LButton);
                short P2state = GDNative.GetAsyncKeyState(Keys.Up);
                if (iType == (int) Config.Types.TCP_HOST)
                {
                    if (P1state == -32767)
                    {
                        P2Jump = true;
                    }
                }
                else if (iType == (int) Config.Types.TCP_CLIENT)
                {
                    if (P2state == -32767)
                    {
                        P1Jump = true;
                    }
                }
            }
        }

        void thread_tcpreceivesend()
        {
            Config cfg = new Config();
            MainFunctions mf = new MainFunctions();
            string buffer = null;
            while (true)
            {
                switch (iType)
                {
                    case 1: //TYPE.TCP_HOST
                        if (P1Jump)
                        {
                            SendSocketMessage(Config.tcp_server, "P1Jump");
                            P1Jump = false;
                            new MainFunctions().ConsoleWrite("[GDMP Log] -> Player1 Jumped!", ConsoleColor.Cyan);
                        }
                        buffer = ReceiveSocketMessage(Config.tcp_server, 6);
                        if (buffer == "P2Jump")
                        {
                            mf.PlayerJump(2);
                            buffer = null;
                            new MainFunctions().ConsoleWrite("[GDMP Remote Log] -> Player2 Jumped!", ConsoleColor.DarkCyan);
                        }
                        break;
                    case 2: //TYPE.TCP_CLIENT
                        if (P2Jump)
                        {
                            SendSocketMessage(Config.tcp_server, "P2Jump");
                            P2Jump = false;
                            new MainFunctions().ConsoleWrite("[GDMP Log] -> Player2 Jumped!", ConsoleColor.Cyan);
                        }
                        buffer = ReceiveSocketMessage(Config.tcp_client, 6);
                        if (buffer == "P1Jump")
                        {
                            mf.PlayerJump(1);
                            buffer = null;
                            new MainFunctions().ConsoleWrite("[GDMP Remote Log] -> Player1 Jumped!", ConsoleColor.DarkCyan);
                        }
                        break;
                }
            }
            
        }
        static void Main(string[] args)
        {
            Console.Title = "GD Multiplayer - By Bonkeyzz -- Ver 1.0b";
            MainFunctions mf = new MainFunctions();
            Program prg = new Program();
            InitializeVariables();
            Thread tmain = new Thread(prg.thread_InitializeTCP);
            while (!new Config().Success)
            {
                mf.ConsoleWrite("[GDMP] Please Select Mode[Client = 2/Host = 1]:", ConsoleColor.Cyan);

                try
                {
                    iType = Convert.ToInt32(Console.ReadLine());
                    if (iType == (int)Config.Types.TCP_HOST)
                    {
                        Console.Title = "GD Multiplayer[SERVERMODE] - By Bonkeyzz -- Ver 1.0b";
                        mf.ConsoleWrite("[GDMP] Enter the IP Address for the server: ", ConsoleColor.Cyan);
                        Config.sIPAddress = Console.ReadLine();
                        if (!string.IsNullOrEmpty(Config.sIPAddress) && Config.sIPAddress.Contains("."))
                        {
                            mf.ConsoleWrite("[GDMP] Starting...", ConsoleColor.Cyan);
                            tmain.Start();
                            break;
                        }
                        else
                        {
                            mf.ConsoleWrite("[GDMP] IP Address is invalid! Try again.", ConsoleColor.Red);
                        }
                    }
                    else if (iType == (int)Config.Types.TCP_CLIENT)
                    {
                        Console.Title = "GD Multiplayer[CLIENTMODE] - By Bonkeyzz -- Ver 1.0b";
                        mf.ConsoleWrite("[GDMP] Enter the IP Address to connect: ", ConsoleColor.Cyan);
                        Config.sIPAddress = Console.ReadLine();
                        if (!string.IsNullOrEmpty(Config.sIPAddress))
                        {
                            mf.ConsoleWrite("[GDMP] Connecting...", ConsoleColor.Cyan);
                            tmain.Start();
                            break;
                        }
                        else
                        {
                            mf.ConsoleWrite("[GDMP] IP Address is invalid! Try again.", ConsoleColor.Red);
                        }

                    }
                    else
                    {
                        mf.ConsoleWrite("[GDMP] Incorrect Type -> 1-1-6", ConsoleColor.Red);
                        iType = 0;
                    }
                }
                catch
                {
                    mf.ConsoleWrite("[GDMP] Incorrect Type -> 1-2-2", ConsoleColor.Red);
                }
            }
        }
    }
}
