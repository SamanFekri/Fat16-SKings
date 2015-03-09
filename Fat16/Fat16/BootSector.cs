using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fat16
{
    class BootSector
    {
        public string company;
        public byte[] bytes;
        public int bytesPerSector, sectorPerCluster, nReservedSectors, nFats, nDirectoryInRoot,
                   nSectorVolume, nSectorPerFat, nSectorPerTrack, nRWHeads, nHiddenSectors, RootDirectory,
                    fatStart, RootDirectoryStart , sizeOfFat , sizeOfCluster;
        public BootSector(byte[] mBytes)
        {
            bytes = new byte[512];
            bytes = mBytes;
            byte[] tmp = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                tmp[i] = mBytes[3 + i];
            }
            company = System.Text.Encoding.UTF8.GetString(tmp);

            tmp = new byte[2];
            tmp[0] = mBytes[11];
            tmp[1] = mBytes[12];
            bytesPerSector = BitConverter.ToInt16(tmp, 0);

            sectorPerCluster = mBytes[13];

            tmp = new byte[2];
            tmp[0] = mBytes[14];
            tmp[1] = mBytes[15];
            nReservedSectors = BitConverter.ToInt16(tmp, 0);

            nFats = mBytes[16];

            tmp = new byte[2];
            tmp[0] = mBytes[17];
            tmp[1] = mBytes[18];
            nDirectoryInRoot = BitConverter.ToInt16(tmp, 0);

            tmp = new byte[2];
            tmp[0] = mBytes[19];
            tmp[1] = mBytes[20];
            nSectorVolume = BitConverter.ToInt16(tmp, 0);

            tmp = new byte[2];
            tmp[0] = mBytes[22];
            tmp[1] = mBytes[23];
            nSectorPerFat = BitConverter.ToInt16(tmp, 0);

            tmp = new byte[2];
            tmp[0] = mBytes[24];
            tmp[1] = mBytes[25];
            nSectorPerTrack = BitConverter.ToInt16(tmp, 0);

            tmp = new byte[2];
            tmp[0] = mBytes[26];
            tmp[1] = mBytes[27];
            nRWHeads = BitConverter.ToInt16(tmp, 0);

            tmp = new byte[2];
            tmp[0] = mBytes[28];
            tmp[1] = mBytes[29];
            nHiddenSectors = BitConverter.ToInt16(tmp, 0);

            tmp = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                tmp[i] = mBytes[44 + i];
            }
            RootDirectory = BitConverter.ToInt16(tmp, 0);

            fatStart = nReservedSectors * bytesPerSector;

            RootDirectoryStart = fatStart + (nFats * nSectorPerFat * bytesPerSector);

            sizeOfFat = nSectorPerFat * bytesPerSector;

            sizeOfCluster = sectorPerCluster * bytesPerSector;

            Console.WriteLine("Manufacor And Version:                {0}\n" +
                              "Bytes Per Sector:                     {1}\n" +
                              "Sector Per Cluster:                   {2}\n" +
                              "Reserved Sectors:                     {3}\n" +
                              "FATs:                                 {4}\n" +
                              "Sectror Per Fat:                      {5}\n", 
                              company, bytesPerSector, sectorPerCluster, nReservedSectors, nFats, nSectorPerFat);
        }

    }
}
