using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace modInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            WebClient WC = new WebClient();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string minecraftPath = $"{appData}\\.minecraft";

            if (!Directory.Exists(minecraftPath))
            {
                Console.WriteLine($"You do not have minecraft installed, or its in a different directory.");
            }
            else
            {
                if(!Directory.Exists($"{minecraftPath}/mods"))
                {
                    Console.WriteLine($"No mods folder detected, creating one");
                    Directory.CreateDirectory($"{minecraftPath}/mods");
                }
                if(!IsDirectoryEmpty($"{minecraftPath}/mods"))
                {
                    Console.WriteLine($"You already some mods installed ({minecraftPath}\\mods)");
                    Console.WriteLine("Would you like to delete them [Y/N]");
                    if(Console.ReadLine().ToLower() == "y")
                    {
                        clearFolder($"{minecraftPath}/mods");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Cleared all files from mods folder");
                        DownloadMods();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Cancelled.");
                    }
                }
                else
                {
                    DownloadMods();
                }

            }
            
            void DownloadMods()
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Attempting to download mods from host (https://owowhatsthis.xyz/Mods.zip) this may take a while");
                WC.DownloadFile("https://owowhatsthis.xyz/Mods.zip", "temp.zip");
                ExtractZipContent("temp.zip", null, $"{minecraftPath}/mods");
                Console.WriteLine("Finished.");
                File.Delete("temp.zip");
                WC.Dispose();
            }
            bool IsDirectoryEmpty(string path)
            {
                IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
                using (IEnumerator<string> en = items.GetEnumerator())
                {
                    return !en.MoveNext();
                }
            }
            void clearFolder(string FolderName)
            {
                DirectoryInfo dir = new DirectoryInfo(FolderName);

                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }

                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    clearFolder(di.FullName);
                    di.Delete();
                }
            }
           
            void ExtractZipContent(string FileZipPath, string password, string OutputFolder)
            {
                ZipFile file = null;
                try
                {
                    FileStream fs = File.OpenRead(FileZipPath);
                    file = new ZipFile(fs);

                    if (!String.IsNullOrEmpty(password))
                    {
                        // AES encrypted entries are handled automatically
                        file.Password = password;
                    }

                    foreach (ZipEntry zipEntry in file)
                    {
                        if (!zipEntry.IsFile)
                        {
                            // Ignore directories
                            continue;
                        }

                        String entryFileName = zipEntry.Name;
                        // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                        // Optionally match entrynames against a selection list here to skip as desired.
                        // The unpacked length is available in the zipEntry.Size property.

                        // 4K is optimum
                        byte[] buffer = new byte[4096];
                        Stream zipStream = file.GetInputStream(zipEntry);

                        // Manipulate the output filename here as desired.
                        String fullZipToPath = Path.Combine(OutputFolder, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);

                        if (directoryName.Length > 0)
                        {
                            Directory.CreateDirectory(directoryName);
                        }

                        // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                        // of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
                finally
                {
                    if (file != null)
                    {
                        file.IsStreamOwner = true; // Makes close also shut the underlying stream
                        file.Close(); // Ensure we release resources
                    }
                }
            }

        }
    }
}
