using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fat16
{
    class WorkWithHard
    {
        public const uint GENERIC_READ = (0x80000000);
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
        unsafe static extern uint SetFilePointer(
              IntPtr hFile,
              [In] uint lDistanceToMove,
              [In, Out] uint* lpDistanceToMoveHigh,
              [In] int dwMoveMethod);

        [DllImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true)]
        extern static bool WriteFile(IntPtr hFile, [In] byte[] lpBuffer,
             uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);


        public IntPtr hFile , nowFile;
        public byte[] Fat_1,Fat_2;
        public string driveName , driveNAme_2;
        public bool exist;
        public BootSector bs;

        public WorkWithHard(string nameDrive)
        {
            driveName = nameDrive.ToUpper();
            string logicalPath = "\\\\.\\" + driveName + ":";
            hFile = CreateFile(logicalPath,
                                       GENERIC_READ | GENERIC_WRITE,
                                       0x00000001,
                                       IntPtr.Zero,
                                       3,
                                       0, IntPtr.Zero);
            byte[] buffer = new byte[512];
            exist = samReadFile(hFile, buffer);
            if (exist)
            {
                nowFile = CreateFile(logicalPath,
                                             GENERIC_READ,
                                             0x00000001,
                                             IntPtr.Zero,
                                             3,
                                             0, IntPtr.Zero);

                bs = new BootSector(buffer);
                Fat_1 = new byte[bs.sizeOfFat];
                Fat_2 = new byte[bs.sizeOfFat];
                seekToSector((uint)bs.fatStart, hFile);
                samReadFile(hFile, Fat_1);

                seekToSector((uint)(bs.fatStart+bs.sizeOfFat), hFile);
                samReadFile(hFile , Fat_2);

                seekToSector((uint)bs.RootDirectoryStart, hFile);
                buffer = new byte[bs.sizeOfCluster / 2];
                samReadFile(hFile, buffer);
                byte[] infFile = new byte[32];

                byte[] tmp = new byte[10];
                for (int i = 0; i < 10; i++)
                {
                    tmp[i] = buffer[i];
                }
                driveNAme_2 = System.Text.Encoding.UTF8.GetString(tmp);
                
                int error = GetLastError();
            }
        }

        public void dir(string directory)
        {
            if (String.Compare(directory,"\\") == 0)
            {
                List<FileInfo> files = new List<FileInfo>(); 
                seekToSector( (uint) bs.RootDirectoryStart, hFile);
                byte[] buffer = new byte[bs.sizeOfCluster / 2];
                samReadFile(hFile, buffer);
                byte[] infFile = new byte[32];
                int y = 0;
                FileInfo mf;
                for(int i = 32 ; i < bs.sizeOfCluster / 2; i += 32){
                    if (buffer[i]!=0)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            infFile[j] = buffer[i + j];
                        }
                        mf = new FileInfo(infFile);
                        if (mf.day > 0)
                        {
                            files.Add(mf);
                            
                        }
                    }
                }
                for(int i = 0; i < files.Count; i ++ ){
                    files.ElementAt(i).show();
                }
            }
            else
            {
                List<FileInfo> files = new List<FileInfo>();
                seekToSector(cd(directory), hFile);
                byte[] buffer = new byte[bs.sizeOfCluster / 2];
                samReadFile(hFile, buffer);
                byte[] infFile = new byte[32];


                uint loc = (uint)((cd(directory) - bs.RootDirectoryStart) * 2 / bs.sizeOfCluster) / 2 + 3;
                //loc--;
                loc *= 2;
 
                //loc -= 3;
                byte[] tmp = new byte[2];
                tmp[0] = Fat_1[loc - 2];
                tmp[1] = Fat_1[loc -1];

                //Console.WriteLine(tmp[0] + "   " + tmp[1]);
                uint loc_2 = 0;

                while (loc_2 < 0xfff0)
                {
                    FileInfo mf;
                    for (int i = 0; i < bs.sizeOfCluster / 2; i += 32)
                    {
                        if (buffer[i] != 0)
                        {
                            for (int j = 0; j < 32; j++)
                            {
                                infFile[j] = buffer[i + j];
                            }
                            mf = new FileInfo(infFile);
                            if (mf.day > 0)
                            {
                                files.Add(mf);
                                
                            }
                        }
                    }
                    
                    loc_2 = BitConverter.ToUInt16(tmp, 0);
                    Console.WriteLine(loc_2);
                    if (loc_2 < 0xfff0)
                    {
                        tmp[0] = Fat_1[loc_2 * 2];
                        tmp[1] = Fat_1[loc_2 * 2 + 1];
                    }

                    seekToSector((uint)(((loc_2 * 2) - 3) * bs.sizeOfCluster/2 + bs.RootDirectoryStart), hFile);
                    samReadFile(hFile, buffer);
                }
                int count = 0;
                for (int i = 0; i < files.Count; i++)
                {
                    files.ElementAt(i).show();
                    if (!files[i].isDeleted)
                    {
                        count++;
                    }
                }
                Console.WriteLine("number of files : " + count + "/" + files.Count);
            }
        }

        public uint cd(string Directory)
        {
            bool isExist = true;
            string[] subDirFile = Directory.Split('\\');
            int n = 0;
            uint location = (uint) bs.RootDirectoryStart;
            for (int i = 0; i < subDirFile.Count(); i++)
            {
                subDirFile[i] = subDirFile[i].ToUpper();
                if (subDirFile[i].Contains(".TXT"))
                {
                    isExist = false;
                }
            }
            if (isExist == false)
            {
                Console.WriteLine("The directory name is invalid.");
                Console.WriteLine();
                return 0;
            }
            else
            {
                while (isExist == true && n < subDirFile.Count())
                {
                    //seekToSector(0, hFile);
                    List<FileInfo> files = new List<FileInfo>();
                    seekToSector(location, hFile);
                    byte[] buffer = new byte[bs.sizeOfCluster / 2];
                    samReadFile(hFile, buffer);
                    byte[] infFile = new byte[32];

                    uint firstOfSubdir = 0;

                    FileInfo mf;
                    for (int i = 0; i < bs.sizeOfCluster / 2; i += 32)
                    {
                        if (buffer[i] != 0)
                        {
                            for (int j = 0; j < 32; j++)
                            {
                                infFile[j] = buffer[i + j];
                            }
                            mf = new FileInfo(infFile);
                            if (mf.day > 0)
                            {
                                files.Add(mf);
                            }
                        }
                    }

                    for (int i = 0; i < files.Count; i++)
                    {
                        if (String.Compare(files.ElementAt(i).name, subDirFile[n]  ) == 0)
                        {
                            firstOfSubdir = files.ElementAt(i).firstCluster;
                           
                        } else if (String.Compare(files.ElementAt(i).name + "." + files.ElementAt(i).ext, subDirFile[n]  ) == 0)
                        {
                            Console.WriteLine("The directory name is invalid.");
                            Console.WriteLine();
                            return 0;   
                        } 
                    }
                    if (firstOfSubdir == 0)
                    {
                        if (String.Compare(subDirFile[n], "") == 0)
                        {
                            n++;
                        }
                        else
                        {
                            Console.WriteLine("The system cannot find the path specified.");
                            Console.WriteLine();
                            isExist = false;
                        }
                    }
                    else
                    {
                        if (firstOfSubdir < 3)
                        {
                            location = (uint)((firstOfSubdir - 1) * (bs.sizeOfCluster / 2) + bs.RootDirectoryStart);
                        }
                        else if (firstOfSubdir == 3)
                        {
                            location = (uint)((firstOfSubdir) * (bs.sizeOfCluster / 2) + bs.RootDirectoryStart);
                        }
                        else
                        {
                            location = (uint)(((firstOfSubdir - 3) + firstOfSubdir) * (bs.sizeOfCluster / 2) + bs.RootDirectoryStart);
                           // location = (uint)((firstOfSubdir + 2) * (bs.sizeOfCluster/2) + bs.RootDirectoryStart);
                        }
                        /*
                        Console.WriteLine(location + "          " + bs.sizeOfCluster + "     "+firstOfSubdir);
                        Console.WriteLine(seekToSector((uint)(7*bs.sizeOfCluster/2+bs.RootDirectoryStart), hFile));
                        files.RemoveRange(0, files.Count);
                        samReadFile(hFile, buffer);
                        for (int i = 0; i < bs.sizeOfCluster / 2; i += 32)
                        {
                            if (buffer[i] != 0)
                            {
                                for (int j = 0; j < 32; j++)
                                {
                                    infFile[j] = buffer[i + j];
                                }
                                mf = new FileInfo(infFile);
                                if (mf.day > 0)
                                {
                                    files.Add(mf);
                                }
                            }
                        }
                        for (int i = 0; i < files.Count; i++)
                        {
                            files.ElementAt(i).show();

                        }*/
                        n++;
                    }
                }

                if (isExist == false)
                {
                    location = 0;
                }

                return location;
            }
            
        }

        public void undelete(string directory)
        {
            
            seekToSector( cd(directory) , hFile);

            byte[] buffer = new byte[bs.sizeOfCluster / 2];
            samReadFile(hFile, buffer);
            byte[] infFile = new byte[32];

            FileInfo mf;
            for (int i = 0; i < bs.sizeOfCluster / 2; i += 32)
            {
                if (buffer[i] != 0)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        infFile[j] = buffer[i + j];
                    }

                    mf = new FileInfo(infFile);
                    uint locationInFat = mf.firstCluster * 2;
                    bool canUndelete = true;
                    if (Fat_1[locationInFat] != 0 || Fat_1[locationInFat + 1] != 0)
                    {
                        canUndelete = false;
                    }

                    if (mf.isDeleted == true && mf.day > 0 && canUndelete)
                    {
                        Console.WriteLine("in this path : " + directory);
                        Console.Write("Enter first name character of --> " + mf.name);
                            if(String.Compare(mf.ext.Trim(),"") == 0){
                                Console.WriteLine();
                            } else{
                                Console.WriteLine("."+mf.ext);
                            }


                        String str = Console.ReadLine().ToUpper();
                        if (string.Compare(str,"") != 0)
                        {
                            char c = str[0];
                            if (c > 'a' && c < 'z')
                            {
                                c = Convert.ToChar(c - 32);
                            }
                            buffer[i] = Convert.ToByte(c);
                            Fat_1[locationInFat] = 255;
                            Fat_1[locationInFat + 1] = 255;
                            Fat_2[locationInFat] = 255;
                            Fat_2[locationInFat + 1] = 255;

                            if (mf.isSubDir == true)
                            {
                                buffer[i + 28] = 0;
                                buffer[i + 29] = 0;
                                buffer[i + 30] = 0;
                                buffer[i + 31] = 0;
                                if (String.Compare(directory, "\\") == 0) {
                                    directory = "";
                                }
                                undelete(directory + "\\" + mf.name);
                            }
                        }
                        else
                        {
                            Console.Write("You Don`t Recover " + mf.name);
                            if(String.Compare(mf.ext.Trim(),"") == 0){
                                Console.WriteLine(" .");
                            } else{
                                Console.WriteLine(" ."+mf.ext);
                            }
                        }
                        Console.WriteLine();   
                        
                    }
                }
            }

            seekToSector((uint)bs.fatStart, hFile);
            samWriteFile(hFile, Fat_1);

            seekToSector((uint)(bs.fatStart + bs.sizeOfFat), hFile);
            samWriteFile(hFile, Fat_2);

            seekToSector(cd(directory), hFile);
            samWriteFile(hFile, buffer);
        }

        public bool del(String directory, String delFileName , bool isQuickFormat)
        {
            char delChar = Convert.ToChar(0xE5);
            bool find = false;

            seekToSector(cd(directory), hFile);
            byte[] buffer = new byte[bs.sizeOfCluster / 2];
            samReadFile(hFile, buffer);
            byte[] infFile = new byte[32];

            FileInfo mf;
            for (int i = 0; i < bs.sizeOfCluster / 2; i += 32)
            {
                if (buffer[i] != 0)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        infFile[j] = buffer[i + j];
                    }
                    mf = new FileInfo(infFile);

                    if (mf.day > 0)
                    {
                        uint locationInFat = mf.firstCluster * 2;
                        if (String.Compare(mf.name, delFileName) == 0)
                        {
                            if (!isQuickFormat)
                            {
                                if (String.Compare(directory, "\\") != 0)
                                {
                                    Console.Write("{0}:{1}\\{2}\\*, Are you sure (Y/N)? ", driveName, directory, delFileName);
                                }
                                else
                                {
                                    Console.Write("{0}:{1}{2}\\*, Are you sure (Y/N)? ", driveName, directory, delFileName);
                                }

                                if (Console.ReadLine().ToUpper()[0] == 'Y')
                                {
                                    buffer[i] = (byte)Convert.ToByte(delChar);
                                    Fat_1[locationInFat] = 0;
                                    Fat_1[locationInFat + 1] = 0;
                                    Fat_2[locationInFat] = 0;
                                    Fat_2[locationInFat + 1] = 0;

                                    del(directory + "\\" + mf.name);
                                }
                            }
                            else {
                                buffer[i] = (byte)Convert.ToByte(delChar);
                                Fat_1[locationInFat] = 0;
                                Fat_1[locationInFat + 1] = 0;
                                Fat_2[locationInFat] = 0;
                                Fat_2[locationInFat + 1] = 0;

                                del(directory + "\\" + mf.name);
                            }
                            find = true;
                        }
                        else if (String.Compare(mf.name + "." + mf.ext.Trim(), delFileName) == 0)
                        {
                            buffer[i] =(byte) Convert.ToByte(delChar);
                            Fat_1[locationInFat] = 0;
                            Fat_1[locationInFat + 1] = 0;
                            Fat_2[locationInFat] = 0;
                            Fat_2[locationInFat + 1] = 0;
                            find = true;
                        }
                    }
                }
            }

            seekToSector((uint)bs.fatStart, hFile);
            samWriteFile(hFile, Fat_1);

            seekToSector((uint)(bs.fatStart + bs.sizeOfFat), hFile);
            samWriteFile(hFile, Fat_2);

            seekToSector(cd(directory), hFile);
            samWriteFile(hFile, buffer);

            return find;
        }

        public void del(String directory)
        {
            char delChar = Convert.ToChar(0xE5);

            seekToSector(cd(directory), hFile);
            byte[] buffer = new byte[bs.sizeOfCluster / 2];
            samReadFile(hFile, buffer);
            byte[] infFile = new byte[32];

            FileInfo mf;
            for (int i = 0; i < bs.sizeOfCluster / 2; i += 32)
            {
                if (buffer[i] != 0)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        infFile[j] = buffer[i + j];
                    }
                    mf = new FileInfo(infFile);

                    if (mf.day > 0)
                    {
                        uint locationInFat = mf.firstCluster * 2;
                        if (String.Compare(".", mf.name) != 0 && String.Compare("..", mf.name) != 0)
                        {
                            if (mf.isSubDir)
                            {
                                buffer[i] = (byte)Convert.ToByte(delChar);
                                Fat_1[locationInFat] = 0;
                                Fat_1[locationInFat + 1] = 0;
                                Fat_2[locationInFat] = 0;
                                Fat_2[locationInFat + 1] = 0;
                                del(directory + "\\" + mf.name);
                            }
                            else
                            {
                                buffer[i] = (byte)Convert.ToByte(delChar);
                                Fat_1[locationInFat] = 0;
                                Fat_1[locationInFat + 1] = 0;
                                Fat_2[locationInFat] = 0;
                                Fat_2[locationInFat + 1] = 0;
                            }
                        }
                    }
                }
            }

            seekToSector((uint)bs.fatStart, hFile);
            samWriteFile(hFile, Fat_1);

            seekToSector((uint)(bs.fatStart + bs.sizeOfFat), hFile);
            samWriteFile(hFile, Fat_2);

            seekToSector(cd(directory), hFile);
            samWriteFile(hFile, buffer);

        }

        public void format()
        {
            seekToSector((uint)bs.RootDirectoryStart, hFile);
            byte[] buffer = new byte[bs.sizeOfCluster / 2];
            samReadFile(hFile, buffer);
            byte[] infFile = new byte[32];

            FileInfo mf;
            for (int i = 32; i < bs.sizeOfCluster / 2; i += 32)
            {
                if (buffer[i] != 0)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        infFile[j] = buffer[i + j];
                    }
                    mf = new FileInfo(infFile);
                    if (mf.day > 0 && !mf.isDeleted)
                    {
                        if (mf.isSubDir)
                        {
                            del("\\" , mf.name , true);
                        }
                        else
                        {
                            del("\\" , mf.name + "." + mf.ext , true);
                        }
                    }
                }
            }
        }


        public void samCloseHandle(IntPtr mhFile)
        {
            if (!CloseHandle(mhFile))
            {
                Console.WriteLine("Error Closing file");
            }
        }


        public bool samReadFile(IntPtr mhFile, byte[] mBytes) {
            uint mlength = 0;
            if (!ReadFile(mhFile, mBytes, (uint) mBytes.Length, out mlength, IntPtr.Zero))
            {
                Console.WriteLine("Error reading file");
                return false;
            }
            return true;
        }

        public bool samWriteFile(IntPtr mhFile, byte[] mBytes)
        {
            uint mlength = 0;
            if (!WriteFile(mhFile, mBytes, (uint)mBytes.Length, out mlength, IntPtr.Zero))
            {
                Console.WriteLine("Error writting file");
                return false;
            }
            return true;
        }
        

        public uint seekToSector(uint mDistance , IntPtr mhFile)
        {
            unsafe
            {
                return SetFilePointer(mhFile, mDistance, null,0);
            }
        }
        
    }
}
