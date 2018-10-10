using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PumpUpdateutilities
{
    public class VersionComparer
    {
        DateTime _releaseDate;
        Dictionary<string, Version> constuctionVerions;
        Dictionary<string, Version> releaseTableVersions;
        Dictionary<string, Version> releaseContentVersions;
        public VersionComparer(DateTime releaseDate)
        {
            _releaseDate = releaseDate;
        }

        public void Start()
        {
            // get versions from construction folder
            string releaseDateString = string.Format("{0}.0", _releaseDate.ToString("yyyy.MM.dd"));

            // get versions from construction folder
            string constructionPath = Path.Combine(Parameters.ServerConstructionPath, releaseDateString);

            GetListFrom(constructionPath);

            // get versions from release notes
            string releaseFolder = string.Format("PumpUpdate {0}", releaseDateString);

            releaseFolder = Path.Combine(Parameters.ServerReleaseNotePath, releaseFolder);
            string releasePath = Path.Combine(releaseFolder, Parameters.PumpUpdateReleaseNotes);

            GetListFrom(releasePath);

            if(constuctionVerions!= null &&
                releaseContentVersions != null &&
                releaseTableVersions != null)
            {
                CompareDifference();
            }
        }

        private void GetListFrom(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine("{0} doesn't exist");
                return;
            }

            if(File.Exists(path)) // get file content versions
            {
                WebPageAnalyzer analyzer = new WebPageAnalyzer(path);
                releaseTableVersions = analyzer.AnalyzeTable();
                releaseContentVersions = analyzer.AnalyzeContent();
            }
            else if(Directory.Exists(path)) // get listed file versions
            {
                constuctionVerions = GetFileVersions(path);
            }
        }

        /// <summary>
        /// path a directory and get all versions from the directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileVersions"></param>
        private Dictionary<string, Version> GetFileVersions(string directory)
        {
            Dictionary<string, Version> fileVersions = new Dictionary<string, Version>();

            var files = new DirectoryInfo(directory).GetFiles("*.*").Where(f => Parameters.ValidPumpDriverUpdateExtList.Contains(Path.GetExtension(f.Name.ToLower())));

            foreach(FileInfo file in files)
            {
                if(fileVersions.ContainsKey(Path.GetFileNameWithoutExtension(file.Name.ToLower())))
                {
                    fileVersions.Remove(file.Name);
                    Console.WriteLine("Update duplicate file version {0}", file.Name);
                }
                try
                {
                    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);
                    fileVersions.Add(Path.GetFileNameWithoutExtension(file.Name.ToLower()), new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart));
                }
                catch (Exception ex)
                {
                    // maybe the file is enb445b2
                    // try to get the version from the file name
                    string pattern = @"enb(\d)(\d+)b(\d+)";
                    Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
                    Match match = reg.Match(file.Name);
                    if(match.Success)
                    {
                        int major = Convert.ToInt32(match.Groups[1].ToString());
                        int minor = Convert.ToInt32(match.Groups[2].ToString());
                        int build = Convert.ToInt32(match.Groups[3].ToString());
                        Version firmwareVersion = new Version(major, minor, build);
                        if (fileVersions.ContainsKey(Path.GetFileNameWithoutExtension(file.Name.ToLower())))
                        {
                            fileVersions.Remove(Path.GetFileNameWithoutExtension(file.Name.ToLower()));
                            Console.WriteLine("Update duplicate file version {0}", file.Name);
                        }

                        fileVersions.Add(Path.GetFileNameWithoutExtension(file.Name.ToLower()), firmwareVersion);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                Console.WriteLine("File:\t{0}\t\tVersion:{1}", file.Name, FileVersionInfo.GetVersionInfo(file.FullName).FileVersion);
            }
            return fileVersions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="contentVersions"></param>
        private void GetFileTableVersions(string filePath, out Dictionary<string, Version> contentVersions)
        {
            WebPageAnalyzer analyzer = new WebPageAnalyzer(filePath);
            contentVersions = analyzer.AnalyzeTable();
            analyzer.AnalyzeContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Correct date</param>
        /// <param name="target">Date to compare</param>
        private void CompareDifference()
        {
            Console.WriteLine("\n\n\n");
            Console.WriteLine("********************* Start Comparing *********************");

            Console.WriteLine("Compare with Release Notes Table:");
            Console.WriteLine("Source: Construction Folder\t\nTarget: Release Notes Table\t");
            Console.WriteLine("--------------------------------------------------------");

            KeyValuePair<string, Dictionary<string, Version>> constructionPair = new KeyValuePair<string, Dictionary<string, Version>>("Construction Folder", constuctionVerions);
            KeyValuePair<string, Dictionary<string, Version>> releaseTablePair = new KeyValuePair<string, Dictionary<string, Version>>("ReleaseNotes Table", releaseTableVersions);

            KeyValuePair<string, Dictionary<string, Version>> releaseContentPair = new KeyValuePair<string, Dictionary<string, Version>>("ReleaseNotes Content", releaseContentVersions);

            Compare(constructionPair, releaseTablePair);

            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("Compare with Release Notes Content:");
            Console.WriteLine("Source: Construction Folder\t\nTarget: Release Notes Content\t");
            Console.WriteLine("--------------------------------------------------------");
            Compare(releaseContentPair, constructionPair);
            Console.WriteLine("********************* End Comparing *********************");
        }

        private void Compare(KeyValuePair<string, Dictionary<string, Version>> source, KeyValuePair<string, Dictionary<string, Version>> target)
        {
            foreach (KeyValuePair<string, Version> item in source.Value)
            {
                if (target.Value.ContainsKey(item.Key))
                {
                    // compare the version
                    Version targetVersion = target.Value[item.Key];
                    string outputString = string.Format(
                        "File:{0,15}\t{1,15}\tVersion:{2,5} <=> {3,15}\tVersion:{4,5}\t",
                    item.Key, source.Key, item.Value.ToString(), target.Key, targetVersion.ToString());


                    if (targetVersion.Equals(item.Value))
                    {
                        Console.WriteLine(outputString + "OK\n");
                    }
                    else
                    {
                        Console.WriteLine(outputString + "Diff\n");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("Unable to find {0, -3} in {1, 15}", item.Key, target.Key);
                    Console.ReadKey();
                }
            }
        }
    }
}
