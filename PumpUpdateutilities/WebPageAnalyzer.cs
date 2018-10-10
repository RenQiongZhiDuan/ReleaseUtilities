using HtmlAgilityPack;
using System.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PumpUpdateutilities
{
    class WebPageAnalyzer
    {
        string htmlFilePath;
        
        HtmlDocument doc ;

        bool isEmbRelease = false;

        Dictionary<string, Version> fileVersions;

        public WebPageAnalyzer(string path)
        {
            htmlFilePath = path;
            doc = new HtmlDocument();
            doc.Load(htmlFilePath);
        }

        public void ReloadHtml(string htmlPath)
        {
            htmlFilePath = htmlPath;
            doc.Load(htmlFilePath);
        }
        /// <summary>
        /// analyze specific section
        /// </summary>
        public Dictionary<string, Version> AnalyzeContent()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(htmlFilePath);

            string directoryName = Path.GetFileName(Path.GetDirectoryName(htmlFilePath));

            string[] releaseFolderName = directoryName.Split(' ');

            if (releaseFolderName.Length != 2)
                return null;

            DateTime releaseDate = Convert.ToDateTime(releaseFolderName[1].Substring(0, releaseFolderName[1].Length-2));

            fileVersions = new Dictionary<string, Version>();

            bool sectionFound = false;
            foreach( var h2Nodes in doc.DocumentNode.Descendants("h2"))
            {
                if (sectionFound)
                    break;

                // h2Nodes contains the release date
                string pattern = @"(.*)\s+-\s+(.*)";
                Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
                Match match = reg.Match(h2Nodes.InnerText);

                // group 2 should contain the release date
                DateTime h2Date = Convert.ToDateTime(match.Groups[2].ToString());

                if (h2Date.Equals(releaseDate))
                    sectionFound = true;

                HtmlNode nextNode = h2Nodes.NextSibling;

                while (nextNode != null && !nextNode.Name.ToLower().Equals("h2"))
                {
                    if(nextNode.NodeType == HtmlNodeType.Element && 
                        nextNode.Name.ToLower().Equals("h3"))
                    {
                        Console.WriteLine(nextNode.InnerText);
                        pattern = @"(.*)(\()(.*nxe.*|.*dll)(\))\s+(v\d+\.\d+.*\d.*)"; 
                        // 5 groups
                        /* 1: xxx
                         * 2: (
                         * 3: Name
                         * 4: )
                         * 5: 3.1.3 or 4.42 build 
                         */
                        reg = new Regex(pattern, RegexOptions.IgnoreCase);
                        match = reg.Match(nextNode.InnerText);

                        if(match.Success)
                        {
                            GroupCollection groups = match.Groups;
                            string fileName = groups[3].ToString().ToLower().Trim();
                            ValidName(ref fileName);
                            Version fileVersion = new Version();
                            if(GetVersionFromString(groups[5].ToString(), ref fileVersion))
                            {
                                fileVersions.Add(fileName, fileVersion);
                                Console.WriteLine("From section {0} verison {1} added", fileName, fileVersion.ToString());
                            }
                        }
                        else { Console.WriteLine("miss match"); }
                    }
                    nextNode = nextNode.NextSibling;
                }

            } // end foreach
            return fileVersions;
        }

        private void ValidName(ref string contentString)
        {
            if (contentString.ToLower().Contains(Parameters.PumpDriverExt))
            {
                contentString = Path.GetFileNameWithoutExtension(contentString);
            }
            else if (contentString.ToLower().Contains(Parameters.FirmwareExtList[0]))
            {
                string[] nameWithExt = contentString.Split('.');
                contentString = nameWithExt[0];
            }
            else
                contentString = null;
        }

        /// <summary>
        /// analyze table content
        /// </summary>
        public Dictionary<string, Version> AnalyzeTable()
        {
            HtmlDocument doc = new HtmlDocument();
            fileVersions = new Dictionary<string, Version>();

            Dictionary<string, FileVersionInfo> list = new Dictionary<string, FileVersionInfo>();

            doc.Load(htmlFilePath);

            foreach(var tableNode in doc.DocumentNode.Descendants("table"))
            {
                foreach(var node in tableNode.Descendants("tr"))
                {
                    string fileName = null;
                    Version fileVersion = null;
                    foreach (var element in node.ChildNodes)
                    {
                        // all version starts with 'v'
                        if (element.Name.ToLower().Equals("td"))
                        {
                            string contentString = element.InnerText;
                            Console.WriteLine(contentString);
                            if (GetVersionFromString(contentString, ref fileVersion))
                                continue;

                            ValidName(ref contentString);
                            fileName = contentString;
                        } //end if
 
                    } //end foreach
                    if(fileName !=null && fileVersion != null)
                    {
                        fileVersions.Add(fileName.Trim().ToLower(), fileVersion);
                        Console.WriteLine("Adding {0} version {1} into", fileName, fileVersion.ToString());
                    }
                }
            }
            return fileVersions;
        }

        private bool GetVersionFromString(string contentString, ref Version fileVersion)
        {
            bool isVersion = false;
            string pattern = @"v\d+\.\d+\.\d+"; // only numbers
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = reg.Match(contentString);
            if (match.Success)
            {
                string subContent = contentString.Substring(1, contentString.Length - 1);
                fileVersion = new Version(subContent);
                isVersion = true;
            }
            else // the version may contain a 'build' 
            {
                pattern = @"v(\d+)\.(\d+)\s+build\s+(\d+)";
                reg = new Regex(pattern, RegexOptions.IgnoreCase);
                match = reg.Match(contentString);
                if(match.Success)
                {
                    int major = Convert.ToInt32(match.Groups[1].ToString());
                    int minor = Convert.ToInt32(match.Groups[2].ToString());
                    int build = Convert.ToInt32(match.Groups[3].ToString());
                    fileVersion = new Version(major, minor, build);
                    isVersion = true;
                }
            }
            return isVersion;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SectionName">Full/Part of the Content of the Secion</param>
        /// <returns></returns>
        public Dictionary<string, ReleaseNotes> AnalyzeNotes(string SectionName)
        {
            Dictionary<string, ReleaseNotes> DriverNotes = new Dictionary<string, ReleaseNotes>();

            // all section tags are using h1
            string xPath = string.Format(@"//{0}[contains(.,'{1}')]", SupportedHTMLTag.HeaderOneTag ,SectionName);
            // starts from the first node that is using the h1 node
            HtmlNode sectionNode = doc.DocumentNode.SelectSingleNode(xPath);

            // unable to find the section you need
            if(sectionNode == null)
            {
                return null;
            }

            // Get all sub driver/ firmware node from the selected section
            var nextNode = sectionNode.NextSibling;
            while( nextNode != null && 
                !nextNode.Name.Equals(
                SupportedHTMLTag.HeaderOneTag.ToString(), StringComparison.CurrentCultureIgnoreCase) )
            {
                 // If this is a h3 tag - for Drivers and database
                if(nextNode.NodeType == HtmlNodeType.Element && 
                    nextNode.Name.Equals(SupportedHTMLTag.HeaderThreeTag.ToString(), StringComparison.CurrentCultureIgnoreCase)) // driver entry
                {
                    IReleaseNotes DriverReleaseNotes = ProcessTitleContent(nextNode.InnerText);
                    if (DriverReleaseNotes == null)
                    {
                        nextNode = nextNode.NextSibling;
                        continue;
                    }

                    string driverDllName = DriverReleaseNotes.DriverDllName;
                    var releaseNode = nextNode.SelectSingleNode(string.Format("//{0}", SupportedHTMLTag.UnorderedTag.ToString()));
                    
                    var pendingReleaseNotes = GetNodeContent(nextNode, SupportedHTMLTag.HeaderThreeTag);

                    // only add into if there is a release notes for the driver
                    if (pendingReleaseNotes.Count > 0)
                    {
                        if(DriverNotes.ContainsKey(driverDllName)) // here the name comes with version: Example Pump Driver Name (ExampleDriver.DLL) v0.0.0
                        {
                            ReleaseNotes TempReleaseNotes = DriverNotes[driverDllName];
                            List<string> existingNodes = DriverNotes[driverDllName].Notes;
                            pendingReleaseNotes.AddRange(existingNodes);
                            DriverNotes.Remove(driverDllName);
                            TempReleaseNotes.Notes = pendingReleaseNotes;
                            DriverReleaseNotes = TempReleaseNotes;
                        }
                        else
                        {
                            DriverReleaseNotes.Notes = pendingReleaseNotes;
                        }
                        DriverNotes.Add(driverDllName, DriverReleaseNotes);
                        Console.WriteLine("{0,-15} With Release Notes Count: {1,5}", driverDllName, pendingReleaseNotes.Count);
                        foreach (string notes in pendingReleaseNotes)
                            Console.WriteLine("\t{0}", notes);
                        Console.WriteLine("\t\n");
                    }
                }
                nextNode = nextNode.NextSibling;
            }

            return DriverNotes;
        }

        /// <summary>
        /// Private a node, and get all inner text 
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="extractFrom">Tag that will be extracted content from </param>
        /// <param name="tagHasAtt">the tag contains content has attribute or not</param>
        /* 
         *  Release Notes Formate:
            <h3>Example Pump Driver Name(ExampleDriver.DLL) v0.0.0</h3>
                <ul class="spacedlist">
	                <li><span class="capsule-blue">Desktop</span> Description of a change.</li>
		
	                <li><span class="capsule-green">Embedded</span> Description of a change.</li>
		
                </ul>
        */
        /// 
        /// Passing NodeH2 in, and get content from li, return 1,2,3 in a list
        /// 
       
        private List<String> GetNodeContent(HtmlNode currentNode, SupportedHTMLTag tagExtractFrom, bool tagHasAtt = false)
        {
            List<string> notes = new List<string>();

            HtmlNode ulNode = currentNode.NextSibling;

            while(ulNode != null &&
                !ulNode.Name.Equals(SupportedHTMLTag.UnorderedTag.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                ulNode = ulNode.NextSibling;
            }

            if (ulNode.NodeType == HtmlNodeType.Element)
            {
                var liNodes = ulNode.Descendants(SupportedHTMLTag.ListTag.ToString());

                foreach (HtmlNode liNode in liNodes)
                {
                    if (liNode.HasChildNodes && liNode.FirstChild.Name.Equals(SupportedHTMLTag.SpanTag.ToString(), StringComparison.CurrentCultureIgnoreCase)) // It reports TRUE for HasChildNodes but has no Child at all
                    {
                        var firstChild = liNode.FirstChild;
                        if (firstChild.HasAttributes)
                        {
                            if (isEmbRelease && firstChild.InnerText.Equals(Parameters.ReleaseType.Embedded, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (liNode.InnerText.Length > 0)
                                {
                                    HtmlNode tempLiNode = liNode;
                                    RemoveUnneededTag(SupportedHTMLTag.SpanTag, ref tempLiNode);
                                    notes.Add(tempLiNode.OuterHtml);
                                   // tempLiNode.Remove();
                                }
                            }
                            else if (!isEmbRelease && firstChild.InnerText.Equals(Parameters.ReleaseType.DESKTOP, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (liNode.InnerText.Length > 0)
                                {
                                    HtmlNode tempLiNode = liNode;
                                    RemoveUnneededTag(SupportedHTMLTag.SpanTag, ref tempLiNode);
                                    notes.Add(tempLiNode.OuterHtml);
                                   // tempLiNode.Remove();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (liNode.InnerText.Length > 0)
                        {
                            HtmlNode tempLiNode = liNode;
                            notes.Add(tempLiNode.OuterHtml);
                           // tempLiNode.Remove();
                        }
                    }
                }
            }

            return notes;
        }

        private void RemoveUnneededTag(SupportedHTMLTag tagNeedsToRemove, ref HtmlNode node)
        {
            var childrenNodes = node.ChildNodes;
            List<HtmlNode> nodesNeedToRemove = new List<HtmlNode>();
            foreach(HtmlNode childNode in childrenNodes)
            {
                if(childNode.Name.Equals(tagNeedsToRemove.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    nodesNeedToRemove.Add(childNode);
                }
            }

            foreach (HtmlNode tempNode in nodesNeedToRemove)
                node.RemoveChild(tempNode);
        }

        private IReleaseNotes ProcessTitleContent(string title)
        {
            string pattern = @"(.*)\((.*\.dll)\)\s+v(\d+\.\d+\.?\d*)";
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = reg.Match(title);
            if(match.Success)
            {
                DriverReleaseNotes DriverNotes = new DriverReleaseNotes();
                GroupCollection groups = match.Groups;
                DriverNotes.Name = groups[1].ToString();
                DriverNotes.DriverDllName = groups[2].ToString();
                DriverNotes.DriverVersion = new Version(groups[3].ToString());
                DriverNotes.Notes = new List<string>();
                return DriverNotes;
            }
            else// this may be a Database entry
            {
                Console.WriteLine("Unable to process: {0,15}", title);
                DatabaseReleaseNotes DatabaseNotes = new DatabaseReleaseNotes();
                DatabaseNotes.Notes = new List<string>();
                DatabaseNotes.Name = title;
                return DatabaseNotes;
            }
        }
    }
}
