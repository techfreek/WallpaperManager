using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperManager
{
    class main
    {
        static void main(string[] args)
        {
            string dir = null;
            do
            {
                Console.Write("Enter a directory (absolute path): ");
                dir = Console.ReadLine();
            } while (!Directory.Exists(dir));
        }
    }
}
