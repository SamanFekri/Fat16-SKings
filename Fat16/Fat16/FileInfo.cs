using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Fat16
{
    class FileInfo
    {
        public string name , ext;
        public byte attr;
        public uint year, month, day, hour, minute, seconds, firstCluster, fileSize;
        public bool isSubDir, isHidden, isDeleted , isAm;

        public FileInfo(byte[] mBytes)
        {
            isSubDir = false;
            isHidden = false;
            isDeleted = false;
            isAm = true;

            byte[] tmp = new byte[11];
            for (int i = 0; i < 11; i++)
            {
                tmp[i] = mBytes[i];
            }
            name = System.Text.Encoding.UTF8.GetString(tmp);

            ext = name.Substring(8);
            name = name.Substring(0, 8);
            name = name.Trim();
            if (name[0] == 65533)
            {
                isDeleted = true;
            }

            attr = mBytes[11];
            int y = attr;
            if ((attr & 0x02) == 0x02) {
                isHidden = true;
            }
            if ((attr & 0x10) == 0x10)
            {
                isSubDir = true;
            }

            tmp = new byte[2];
            tmp[0] = mBytes[22];
            tmp[1] = mBytes[23];
            uint x = BitConverter.ToUInt16(tmp, 0);

            hour = x / 2048;
            x = x % 2048;
            minute = x / 32;
            seconds = x % 32;
            seconds *= 2;

            if (hour >= 12)
            {
                hour -= 12;
                isAm = false;
            }

            tmp = new byte[2];
            tmp[0] = mBytes[24];
            tmp[1] = mBytes[25];
            x = BitConverter.ToUInt16(tmp, 0);
            
            year = 1980 + (x / 512);
            x = x % 512;
            month = x / 32;
            day = x % 32;

            tmp = new byte[2];
            tmp[0] = mBytes[26];
            tmp[1] = mBytes[27];
            firstCluster = BitConverter.ToUInt16(tmp, 0);
            
            tmp = new byte[4];
            tmp[0] = mBytes[28];
            tmp[1] = mBytes[29];
            tmp[2] = mBytes[30];
            tmp[3] = mBytes[31];
            fileSize = BitConverter.ToUInt16(tmp, 0);
        }

        public void show()
        {
            String s;
            if (isHidden != true && isDeleted != true)
            {
                s = month.ToString("D2") + "/" + day.ToString("D2") + "/" + year + "  " + 
                    hour.ToString("D2") + ":" + minute.ToString("D2") + " ";
                if (isAm) 
                {
                    s += "AM    ";
                }
                else
                {
                    s += "PM    ";
                }
                if (isSubDir)
                {
                    s += "<DIR>";
                }
                else {
                    s += "     ";
                }
                string ss = fileSize+"";
                if (fileSize != 0)
                {
                    int j = (ss.Length / 3 );
                    
                    for (int i = 0; i < (9 - (ss.Length + j - 1)); i++)
                    {
                        s += " ";
                    }
                    int jj = ss.Length % 3;
                    if (j >= 0)
                    {
                        s += ss.Substring(0, jj);
                        if (j != 0)
                        {
                            s += ",";
                        }
                        ss = ss.Substring(jj);
                    }
                    while (j > 0)
                    {
                        s += ss.Substring(0, 3);
                        if (j != 1) { s += ","; }
                        ss = ss.Substring(3);
                        j--;
                    }
                }
                else
                {
                    s += "          ";
                }

                s += " "+name;
                if (ext.Trim() != "")
                {
                    s += "." + ext;
                }

                Console.WriteLine(s);

            }
        }

    }
}
