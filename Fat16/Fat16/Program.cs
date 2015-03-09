using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fat16
{
    class Program
    {
 /*       public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);
        public const uint GENERIC_EXECUTE = (0x20000000);
        public const uint GENERIC_ALL = (0x10000000);


        [DllImport("kernel32.dll",
                   CallingConvention = CallingConvention.Winapi,
                   CharSet = CharSet.Unicode,
                   EntryPoint = "CreateFileW", SetLastError = true)]
        extern static IntPtr CreateFile(String lpFileName,
                                 uint dwDesiredAccess,
                                 uint dwShareMode,
                                 IntPtr lpSecurityAttributes,
                                 uint dwCreationDisposition,
                                 uint dwFlagsAndAttributes,
                                 IntPtr hTemplateFile);

        [DllImport("kernel32.dll", EntryPoint = "GetLastError", SetLastError = true)]
        extern static int GetLastError();

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll", EntryPoint = "ReadFile", SetLastError = true)]
        static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer,
           uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", EntryPoint = "SetFilePointer")]
        unsafe static extern int SetFilePointer(
              IntPtr hFile,
              [In] int lDistanceToMove,
              [In, Out] int* lpDistanceToMoveHigh,
              [In] int dwMoveMethod);

        [DllImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true)]
        extern static bool WriteFile(IntPtr hFile, [In] byte[] lpBuffer,
             uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);*/

        static void Main(string[] args)
        {
            Console.WriteLine("(c) 2014-2015 SKings Corporation. All right reserved. ");
            Console.WriteLine();

            WorkWithHard work = null;
            string driveName = "" , path = "\\";
            String command = Console.ReadLine();
            command = command.ToUpper();
            while (!command.Equals("EXIT"))
            {
                int n = command.IndexOf(' ');
                string help = "";
                if (n >= 0)
                {
                    help = command.Substring(n + 1);
                    command = command.Substring(0, n);
                }
                switch (command)
                {
                    case "MOUNT":
                        if (work != null)
                        {
                            work.samCloseHandle(work.hFile);
                        }
                        n = help.IndexOf(' ');
                        if (n >= 0)
                        {
                            help = help.Substring(0, n);
                        }
                        work = new WorkWithHard(help);
                        if (work.exist == true)
                        {
                            driveName = work.driveName;
                            Console.WriteLine("Drive {0} mounted as a local directory.", driveName);
                        }
                        else
                        {
                            Console.WriteLine("Drive Not Found.");
                        }

                        /*
                        Console.WriteLine(work.bs.RootDirectoryStart);
                                                work.seekToSector(282624, work.hFile);
                        byte[] nn = new byte[work.bs.sizeOfCluster *8];
                        work.samReadFile(work.hFile, nn);
                        for (int k = 0; k < 8; k++)
                        {
                            for (int i = k * work.bs.sizeOfCluster / 2; i < (k+1)*work.bs.sizeOfCluster / 2; i = i + 64)
                            {
                                for (int j = 0; j < 64; j++)
                                {
                                    
                                    char c = (char)nn[i + j];
                                    if (c > 48 && c < 120)
                                    {
                                        Console.Write(c);
                                    }

                                }
                                Console.Write("\n");
                            }
                            Console.WriteLine("********");
                        }*/
                        break;
                    case "DIR":
                        if (work != null)
                        {
                            Console.WriteLine(" Volume in drive {0} is {1}\n\n Directory of {0}:{2}",
                                                work.driveName, work.driveNAme_2, path);
                            work.dir(path);
                            Console.WriteLine();
                        }
                        else{
                            Console.WriteLine("Drive Not Found!" +
                                "\nYou must mount it first.");
                        }
                        break;
                    case "CD":
                        if (work != null)
                        {
                            string prevPath = path;
                            if (help.Length > 0)
                            {
                                if (help[0] == '\\')
                                {
                                    help = analizeString(help);
                                    path = help;
                                }
                                else
                                {
                                    if (String.Compare(path, "\\") == 0)
                                    {
                                        path += help;
                                    }
                                    else
                                    {
                                        path += "\\" + help;
                                        path = analizeString(path);
                                    }
                                }
                                if (work.cd(path) == 0)
                                {
                                    path = prevPath;
                                }
                                else
                                {
                                    Console.WriteLine();
                                }
                                
                            }
                            else
                            {
                                Console.WriteLine(driveName + ":" + path);
                                Console.WriteLine();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Drive Not Found!" +
                                "\nYou must mount it first.");
                        }
                        break;
                    case "UNDELETE":
                        if (work != null) {
                            Console.WriteLine("If you want to recover any file you must insert first character of it."
                                                    + "\nElse it not recover!");
                            work.undelete(path);
                        }
                        else
                        {
                            Console.WriteLine("Drive Not Found!" +
                                "\nYou must mount it first.");
                        }
                        break;

                    case "DEL":
                        if (work != null)
                        {
                            if (String.Compare(help, "") == 0)
                            {
                                Console.WriteLine("The syntax of the command is incorrect.");
                            }
                            else if(!work.del(path, help , false))
                            {
                                if (String.Compare(path, "\\") != 0)
                                {
                                    Console.WriteLine("Could Not Find {0}:{1}\\{2}", driveName, path, help);
                                }
                                else
                                {
                                    Console.WriteLine("Could Not Find {0}:{1}{2}", driveName, path, help);
                                }
                            }
                            
                        }
                        else
                        {
                            Console.WriteLine("Drive Not Found!" +
                                "\nYou must mount it first.");
                        }
                        Console.WriteLine();
                        break;

                    case "FORMAT":
                        if (work != null)
                        {
                            work.format();
                        }
                        else
                        {
                            Console.WriteLine("Drive Not Found!" +
                                   "\nYou must mount it first.");
                        }
                        path = "\\";
                        Console.WriteLine();
                        break;

                    default:
                        Console.WriteLine("'{0}' is not recognized as an internal or external command," +
                            "\noperable program or batch file.", command);
                        break;
                }
                if (!driveName.Equals(""))
                {
                    Console.Write(driveName + ":" + path + ">");
                }
                command = Console.ReadLine().ToUpper();               
            }
        }


        static public string analizeString(string stringPath)
        {
            string s = "";
            string[] subDirFile = stringPath.Split('\\');
            int mySize = subDirFile.Count();
            for (int i = 0; i < mySize; i++)
            {
                if (i > 0)
                {
                    if (String.Compare(subDirFile[i], ".") == 0)
                    {
                        subDirFile[i] = "";
                    }
                    else if (String.Compare(subDirFile[i], "..") == 0)
                    {
                        subDirFile[i] = "";
                        subDirFile[i - 1] = "";
                    }
                }
            }
            for (int i = 0; i < mySize; i++)
            {
                if (!(String.Compare(subDirFile[i], "") == 0))
                {
                    s += "\\" + subDirFile[i];
                }
            }
            if ((String.Compare(s , "") == 0))
            {
                s = "\\";
            }
            return s;
        }
    }
}
