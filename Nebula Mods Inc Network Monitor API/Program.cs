using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NebulaMods
{
    public class Program
    {

        #region Look & Feel

        //const int SWP_NOSIZE = 0x0001;
        //[DllImport("kernel32.dll", ExactSpelling = true)]
        //private static extern IntPtr GetConsoleWindow();

        //private static readonly IntPtr MyConsole = GetConsoleWindow();

        //[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        //public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        private static void LookandFeel()
        {
            Console.Title = "Nebula Mods Inc. [Network API] ~ Nebula";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //int xpos = 550;
                //int ypos = 300;
          //      SetWindowPos(MyConsole, 0, xpos, ypos, 0, 0, SWP_NOSIZE);
                Console.SetWindowSize(85, 20);
                Console.SetBufferSize(85, 20);
            }
        }
        #endregion

        static async Task Main(string[] args) => await new NebulaModsApplication(args).RunAsync();
    }
}
