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
            new MainFunctions().ConsoleWrite("[GDMP Log] -> Waiting for Geometry Dash...", ConsoleColor.Gray);
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
        void thread_InitializeTCP()
        {
            Config cfg = new Config();
            MainFunctions mf = new MainFunctions();

            cfg.IPEnd = new IPEndPoint(IPAddress.Parse(Config.sIPAddress), cfg.iPort);

            switch (iType)
            {
                case (int) Config.Types.TCP_HOST:
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
                        using (Socket accepted = server.Accept())
                        {
                            SendSocketMessage(accepted, "connection_success");

                            while (true)
                            {
                                string connectionbuffer = ReceiveSocketMessage(accepted, 18);
                                if (connectionbuffer == "connection_success")
                                {
                                    mf.ConsoleWrite("Player 2 Connected Successfully!", ConsoleColor.Green);
                                    new MainFunctions().ConsoleWrite("[GDMP Notice] Player2's Color will be the opposite of Player1's color!", ConsoleColor.Yellow);
                                    connectionbuffer = null;
                                }
                                connectionbuffer = ReceiveSocketMessage(accepted, 6);
                                if (GDNative.GetAsyncKeyState(Keys.Space) || GDNative.GetAsyncKeyState(Keys.LButton))
                                {
                                    SendSocketMessage(accepted, "P1Jump");
                                }
                                if (connectionbuffer == "P2Jump")
                                {
                                    mf.PlayerJump(2);
                                }
                            }

                        }
                    }
                    break;
                case (int) Config.Types.TCP_CLIENT:
                    new MainFunctions().ConsoleWrite($"[GDMP Log] -> Detected Type: 2", ConsoleColor.White);
                    using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        new MainFunctions().ConsoleWrite($"[GDMP] Connecting to IP: {cfg.IPEnd.Address} with Port: {cfg.IPEnd.Port}...",
                            ConsoleColor.White);
                        try
                        {
                            client.Connect(cfg.IPEnd);
                        }
                        catch (Exception)
                        {
                            new MainFunctions().ConsoleWrite("[GDMP Log] -> Connection Refused => 8-1", ConsoleColor.Red);
                        }


                        SendSocketMessage(client, "connection_success");
                        while (client.Connected)
                        {
                            try
                            {
                                string connectionbuffer = ReceiveSocketMessage(client, 18);
                                if (connectionbuffer == "connection_success")
                                {
                                    mf.ConsoleWrite("Connected to Server!", ConsoleColor.Green);
                                    new MainFunctions().ConsoleWrite(
                                        "[GDMP Notice] Your Color will be the opposite of Player1's color!",
                                        ConsoleColor.Yellow);
                                    connectionbuffer = null;
                                }
                                connectionbuffer = ReceiveSocketMessage(client, 6);
                                if (GDNative.GetAsyncKeyState(Keys.Space) || GDNative.GetAsyncKeyState(Keys.LButton))
                                {
                                    SendSocketMessage(client, "P2Jump");
                                }
                                if (connectionbuffer == "P1Jump")
                                {
                                    mf.PlayerJump(1);
                                }
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
