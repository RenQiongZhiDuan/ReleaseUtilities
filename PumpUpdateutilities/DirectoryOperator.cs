using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReleaseUtilities
{
    static class DirectoryOperator
    {
        /// <summary>
        /// Get file versions from a directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>Dictionary<string, Version></returns>
        public static KeyValuePair<string, List<IReleaseItem>> GetFileVersionsFromDir(string directory, Parameters.ReleaseType releaseType)
        {
            //Dictionary<string, Version> fileVersions = new Dictionary<string, Version>();
            List<IReleaseItem> dirItems = new List<IReleaseItem>();

            FileInfo[] Files;

            if(releaseType == Parameters.ReleaseType.PUMPUPDATE)
                Files = new DirectoryInfo(directory).GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => Parameters.ValidPumpDriverUpdateExtList.Contains(Path.GetExtension(f.Name.ToLower()))).ToArray();
            else
                Files = new DirectoryInfo(directory).GetFiles("*.*", SearchOption.AllDirectories).ToArray();

            foreach (FileInfo file in Files)
            {
                FileElement item;
               /* if (releaseType == Parameters.ReleaseType.PUMPUPDATE)
                    item = new DriverItem();
                else
                    item = new OtherItem();
                    */
                item = new FileElement();

                if(dirItems.Contains(item))
                {
                    dirItems.Remove(item);
                    Console.WriteLine("Update duplicate file version {0}", file.Name);
                }
                try
                {
                    // todo: handle driver and other items
                    item.Name = file.Name;
                    item.FileInfo = file;

                    if (releaseType == Parameters.ReleaseType.PUMPUPDATE)
                    {
                        if (Parameters.ValidPumpDriverUpdateExtList.Contains(file.Extension, StringComparer.CurrentCultureIgnoreCase))
                        {
                            // firmware file
                            string pattern = @"enb(\d)(\d+)b(\d+)";
                            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
                            Match match = reg.Match(file.Name);
                            if (match.Success)
                            {
                                int major = Convert.ToInt32(match.Groups[1].ToString());
                                int minor = Convert.ToInt32(match.Groups[2].ToString());
                                int build = Convert.ToInt32(match.Groups[3].ToString());
                                item.Version = new Version(major, minor, build);
                            }
                            else
                            {
                                // other driver files
                                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);

                                item.Version = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart);//, versionInfo.FilePrivatePart); <- some file has private part? 
                            }
                        }
                    }
                    else // desktop / embedded release
                    {
                        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);

                        item.Version = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart);
                    }
                    
                    dirItems.Add(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Got an exception:"+ex.Message);
                }

                Console.WriteLine("File {0, 2}:\t{1,-25}Version:\t{2,-15}",dirItems.Count,  item.Name, item.Version);
            }// end foreach

            return new KeyValuePair<string, List<IReleaseItem>>(directory, dirItems);
        }

      
    }
}
