using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
