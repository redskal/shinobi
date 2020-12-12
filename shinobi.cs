/*
 * shinobi
 * by Dorian Warboys (Red Skäl)
 * https://github.com/redskal
 * 
 * A crude secure deletion tool written in C# for portability.
 * 
 * I wouldn't suggest using option 'f' anymore. When I wrote this it
 * was rare to see a terabyte harddrive. Now large drives are
 * commonplace and I can't see managed code running through them
 * smoothly nor quickly.
 * 
 * TODO:
 *     1) Clean up the option selection code. Specifically 'f' -
 *        it's a mess!
 *     2) Error handling needs moving inside the loops.  Had some
 *        issues with crashing if drive access fails.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Shinobi
{
    class goemon
    {
        private readonly static Random rnd = new Random();
   
        static void Main(string[] args)
        {
            if (args.Length < 1 || args[0].Substring(0, 1) != "-")
            {
                banner();
                usage();
            }



            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-f":
                        try
                        {
                            banner();
                            string query = "";

                            if ((i+1) < args.Length) query = args[++i];

                            foreach (DriveInfo drive in DriveInfo.GetDrives())
                            {
                                // display target info or list drives
                                if (drive.Name.Substring(0, 1) == query.ToUpper().Substring(0, 1) || query.ToLower() == "list")
                                {
                                    Console.Write(" Drive:\t\t{0}\n" +
                                                  " Space:\t\t{1}Mb\n" +
                                                  " Root dir:\t{2}\n" +
                                                  " Label:\t\t{3}\n" +
                                                  " Format:\t{4}\n\n",
                                                  drive.Name, drive.AvailableFreeSpace/(1048576),
                                                  drive.RootDirectory, drive.VolumeLabel, drive.DriveFormat);
                                }

                                // the meat...
                                if (drive.Name.Substring(0,1) == query.ToUpper().Substring(0,1))
                                {
                                    // change filepath if need be but hopefully this is world-writeable.
                                    string obeseFile = drive.Name + "Users\\Public\\shino.bin";

                                    Console.Write(" Currently writing {0} bytes to disk...\n" +
                                                  " This may take a while!", (drive.AvailableFreeSpace * 30));
                                    shinobi(obeseFile, (Int32)drive.AvailableFreeSpace , 30);
                                    break;
                                }
                            }
                            
                        }
                        catch (Exception e)
                        {
                            Console.Write(" Error: {0}\n\n", e.Message);
                            usage();
                            return;
                        }
                        break;

                    case "-c":
                        // args[i++] needs stripping to numeric-only
                        try
                        {
                            int n = Convert.ToInt32(args[++i]);
                            if (n > 0) Console.Write(rndStr(n));
                        }
                        catch (Exception e)
                        {
                            banner();
                            Console.Write(" Error: {0}\n\n", e.Message);
                            usage();
                            return;
                        }
                        break;

                    case "-o":
                        try
                        {
                            banner();
                            i++;
                            if (File.Exists(args[i]))
                            {
                                long fileSize = new FileInfo(args[i]).Length;
                                shinobi(args[i], (Int32)fileSize, Int32.Parse(args[++i]));
                            }
                            else
                            {
                                Console.Write(" Error: File doesn't exist\n\n");
                                usage();
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Write(" Error: {0}\n\n", e.Message);
                            usage();
                            return;
                        }
                        break;

                    case "-?":
                    case "-h":
                        banner();
                        usage();
                        break;
                    default:
                        banner();
                        Console.Write(" Error: bad args\n\n");
                        usage();
                        return;

                }

            }

            return;
        }

        /*
         * Banner call-able from Main() as we want 'shinobi -c n' to be
         * pipe-able!
         */
        static void banner()
        {
            Console.Write("\n" +
                          " shinobi v0.1a - crude secure delete!\n" +
                          " by Dorian Warboys (Red Skäl)\n\n");
        }

        /*
         * Duhhhh
         */
        static void usage()
        {
            Console.Write(" Usage:\n" +
                           "\t-f <drive>\t- Fill free disk space with shino.bin\n" +
                           "\t\t\t  (running 'shinobi -f list' will list drives)\n" +
                           "\t-c <n>\t\t- Display n bytes to STDOUT.\n" +
                           "\t-o <file> <p>\t- Overwrite file with p passes of bollocks.\n" +
                           "\t\t\t  (suggest p = 30+)\n\n");

            return;
        }

        /*
         * Turn a file into bona fide Shinobi
         */
        static void shinobi(string fileName, int fileSize, int passes)
        {
            string buff = "";
            byte[] buffer = new byte[fileSize];

            // j00r readonly skillz R sux
            File.SetAttributes(fileName, FileAttributes.Normal);

            Console.Write(" Currently fux0ring: {0} ({1}bytes)\n", fileName, fileSize);

            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            for (int i = 1; i <= passes; i++)
            {
                // generate rnd string, generate byte[] from string.
                buff = rndStr(fileSize);
                buffer = GetBytes(buff);
                Console.Write(" \tPass:\t{0}\tBuffer size:\t{1}\n", i, buffer.Length);

                // set file offset, write data and flush to disk.
                fs.Position = 0;
                fs.Write(buffer, 0, fileSize);
                fs.Flush();
            }
            fs.SetLength(0);
            fs.Close();

            // ninja mode: activated
            DateTime ninjaTime = new DateTime(1977, 8, 3, 0, 0, 0);
            File.SetCreationTime(fileName, ninjaTime);
            File.SetCreationTimeUtc(fileName, ninjaTime);
            File.SetLastAccessTime(fileName, ninjaTime);
            File.SetLastAccessTimeUtc(fileName, ninjaTime);
            File.SetLastWriteTime(fileName, ninjaTime);
            File.SetLastWriteTimeUtc(fileName, ninjaTime);

            File.Delete(fileName);

            return;
        }

        static long getDriveSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                    return drive.TotalFreeSpace;
            }
            return -1;
        }

        /*
         * Generate a random string...
         */
        static string rndStr(int size)
        {
            if (size < 1) size = 0x10000;
            char[] buff = new char[size];

            for (int i = 0; i < size; i++)
                buff[i] = (char)rnd.Next(0x01, 0xff);

            return new string(buff);
        }

        /*
         * Ripped from stackoverflow.com
         * I forget who published it but it made things easier.
         */
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
