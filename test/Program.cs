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

namespace test
{
    class Program
    {
        public static int iType;

        static void InitializeVariables()
        {
            Config cfg = new Config();
            new MainFunctions().ConsoleWrite("[GDMP Log] -> Waiting for Geometry Dash...", ConsoleColor.Gray);
            while (cfg.iGDWindow == IntPtr.Zero)
            {
                cfg.iGDWindow = GDNative.FindWindow(default(string), "Geometry Dash");
            }
            Console.Clear();
        }

        void thread_InitializeTCP()
        {
            Program prg = new Program();
            Config cfg = new Config();
            MainFunctions mf = new MainFunctions();
            NetworkStream networkStream;
            cfg.IPEnd = new IPEndPoint(IPAddress.Parse(Config.sIPAddress), cfg.iPort);
            switch (iType)
            {
                case (int) Config.Types.TCP_HOST:
                    using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        new MainFunctions().ConsoleWrite(
                            $"[GDMP Log] -> Bound IP Address: {cfg.IPEnd.Address} with Port: {cfg.IPEnd.Port}",
                            ConsoleColor.White);
                        server.Bind(cfg.IPEnd);
                        new MainFunctions().ConsoleWrite($"[GDMP Log] -> Listening for 1 client...", ConsoleColor.White);
                        server.Listen(1);
                        new MainFunctions().ConsoleWrite($"[GDMP] Server is running and is waiting for Player 2...",
                            ConsoleColor.Green);
                        using (Socket accepted = server.Accept())
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes("P1Connected");
                            byte[] recvbuffer = new byte[12];

                            while (true)
                            {
                                accepted.Send(buffer, SocketFlags.None);
                                accepted.Receive(recvbuffer, SocketFlags.None);
                                if (recvbuffer != null)
                                {
                                    string received = Encoding.UTF8.GetString(recvbuffer);
                                    mf.ConsoleWrite($"Player 2 Connected with message: {received}", ConsoleColor.Green);
                                    Console.ReadLine();
                                }
                            }


                        }
                    }
                    break;
                case (int) Config.Types.TCP_CLIENT:
                    using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        new MainFunctions().ConsoleWrite($"[GDMP] -> Connecting to IP: {cfg.IPEnd.Address} with Port: {cfg.IPEnd.Port}...",
                            ConsoleColor.White);
                        try
                        {
                            client.Connect(cfg.IPEnd);
                        }
                        catch (Exception)
                        {
                            new MainFunctions().ConsoleWrite("[GDMP Log] -> Connection Refused => 7-1", ConsoleColor.Red);
                        }
                        

                        byte[] conbuffer = new byte[12];
                        byte[] sendbuf = Encoding.UTF8.GetBytes("P2Connected");
                        while (client.Connected)
                        {
                            try
                            {
                                client.Receive(conbuffer, SocketFlags.None);
                                mf.ConsoleWrite($"Connected! With message {Encoding.UTF8.GetString(conbuffer, 0, 12)}",
                                    ConsoleColor.Green);
                                client.Send(sendbuf, SocketFlags.None);
                                Console.ReadLine();
                            }
                            catch
                                (SocketException ex)
                            {

                            }
                        }
                    }
                    break;
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
