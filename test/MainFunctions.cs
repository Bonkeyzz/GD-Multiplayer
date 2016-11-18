using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class MainFunctions
    {
        /// <summary>
        /// Writes colored text in console.
        /// </summary>
        /// <param name="sText">Text to display</param>
        /// <param name="con_color">Console color</param>
        public void ConsoleWrite(string sText, ConsoleColor con_color) // https://www.dotnetperls.com/console-color <-- From here
        {

            Console.ForegroundColor = con_color;
            Console.WriteLine(sText.PadRight(Console.WindowWidth - 1));
            Console.ResetColor();
        }

        public void PlayerJump(int PlayerNum)
        {
            //Player1(SPACEBAR/MOUSECLICK) = 1, Player2(UPARROW) = 2;
            switch (PlayerNum)
            {
                case 1: //Player 1(SPACEBAR)
                    GDNative.SendMessage(Config.iGDWindow, (uint) GDNative.WM.WM_KEYDOWN, (IntPtr) GDNative.VK.VK_SPACE,
                        IntPtr.Zero);
                    Thread.Sleep(200);
                    GDNative.SendMessage(Config.iGDWindow, (uint)GDNative.WM.WM_KEYUP, (IntPtr)GDNative.VK.VK_SPACE,
                        IntPtr.Zero);
                    break;
                case 2: //Player 2(UPARROW)
                    GDNative.SendMessage(Config.iGDWindow, (uint)GDNative.WM.WM_KEYDOWN, (IntPtr)GDNative.VK.VK_UP,
                        IntPtr.Zero);
                    Thread.Sleep(200);
                    GDNative.SendMessage(Config.iGDWindow, (uint)GDNative.WM.WM_KEYUP, (IntPtr)GDNative.VK.VK_UP,
                        IntPtr.Zero);
                    break;

            }

        }
    }
}
