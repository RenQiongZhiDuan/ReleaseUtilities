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

        #region Public Methods

        public void ReloadHtml(string htmlPath)
        {
            htmlFilePath = htmlPath;
            doc.Load(htmlFilePath);
        }

        // TODO:
        // Issue: Needs to be able to output the driver version, database name,
        // this requires to refactoring the IReleaseNotes class
        public static void SaveTo(string path, List<IReleaseNotes> content)
        {
            if(!File.Exists(path))
            {
                File.Create(path).Close();
            }

            using (StreamWriter sw = new StreamWriter(path, false))
            {
                foreach(IReleaseNotes ReleaseNotes in content)
                {
                    string title = string.Format(ReleaseNotes.PageTitle);
                    sw.WriteLine(title);
                    sw.WriteLine("<"+SupportedHTMLTag.UnorderedTag.ToString()+ " class=\"spacedlist\">");
                    foreach(string notes in ReleaseNotes.Notes)
                    {
                        sw.WriteLine(notes);
                    }
                    sw.WriteLine("</" + SupportedHTMLTag.UnorderedTag.ToString() + ">");
                }
            }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SectionName">Full/Part of the Content of the Secion</param>
        /// <returns></returns>
        public List<IReleaseNotes> AnalyzeSectionContent(string sectionName)
        {
            // Driver/Database name + Release Notes
            List<IReleaseNotes> ContentList = new List<IReleaseNotes>();

            SupportedHTMLTag HeaderTag = SupportedHTMLTag.HeaderOneTag;
            // all section tags are using h1
            string xPath = string.Format(@"//{0}[contains(.,'{1}')]", HeaderTag, sectionName);
            // starts from the first node that is using the h1 node
            HtmlNode sectionNode = doc.DocumentNode.SelectSingleNode(xPath);

            // unable to find the section you need
            if(sectionNode == null)
            {
                HeaderTag = SupportedHTMLTag.HeaderTwoTag;
                // for Release Notes, h2 is the Release Date and Time.
                xPath = string.Format(@"//{0}[contains(.,'{1}')]", HeaderTag, sectionName);
                sectionNode = doc.DocumentNode.SelectSingleNode(xPath);
                if(sectionNode == null)
                    return null;
            }

            // Get all sub driver/ firmware node from the selected section
            var sectionNextSibling = sectionNode.NextSibling;

            while( sectionNextSibling != null && 
                !sectionNextSibling.Name.Equals(
                HeaderTag.ToString(), StringComparison.CurrentCultureIgnoreCase) )
            {
                 // If this is a h3 tag - for Drivers and database
                if(sectionNextSibling.NodeType == HtmlNodeType.Element && 
                    sectionNextSibling.Name.Equals(SupportedHTMLTag.HeaderThreeTag.ToString(), StringComparison.CurrentCultureIgnoreCase)) // driver entry
                {
                    IReleaseNotes ReleaseNotes = GetNodeContent(sectionNextSibling);

                    if (ReleaseNotes == null || !ValidateContent(ReleaseNotes.DisplayCoreTitle))
                    {
                        sectionNextSibling = sectionNextSibling.NextSibling;
                        continue;
                    }

                    var releaseNode = sectionNextSibling.SelectSingleNode(string.Format("//{0}", SupportedHTMLTag.UnorderedTag.ToString()));
                    
                    var NewReleaseNotes = GetReleaseNotesFromContent(sectionNextSibling, SupportedHTMLTag.UnorderedTag);

                    // only add into if there is a release notes for the driver
                    if (NewReleaseNotes.Count > 0)
                    {
                        if(ContentList.Exists(r => r.DisplayCoreTitle.Equals
                            (ReleaseNotes.DisplayCoreTitle, StringComparison.CurrentCultureIgnoreCase))) // here the name comes with version: Example Pump Driver Name (ExampleDriver.DLL) v0.0.0
                        {
                            IReleaseNotes existingReleaseNotes = ContentList.Find(r => r.DisplayCoreTitle.Equals(ReleaseNotes.DisplayCoreTitle, StringComparison.CurrentCultureIgnoreCase));

                            if(ReleaseNotes is DriverReleaseNotes)
                            {

                                if(((DriverReleaseNotes)ReleaseNotes).DriverVersion.CompareTo
                                    (((DriverReleaseNotes)existingReleaseNotes).DriverVersion) < 0) // new version > existing version
                                {
                                    // update 
                                    ReleaseNotes = existingReleaseNotes;
                                }
                            }
                            List<string> ExistingReleaseNotes = existingReleaseNotes.Notes;

                            NewReleaseNotes = MergeNotes(NewReleaseNotes, ExistingReleaseNotes);

                            // remove it from the Directory
                            ContentList.Remove(existingReleaseNotes);
                            // Update the notes
                            ReleaseNotes.Notes = NewReleaseNotes;
                        }
                        else
                        {
                            ReleaseNotes.Notes = NewReleaseNotes;
                        }
                        // add into the group
                        ContentList.Add(ReleaseNotes);

                        Console.WriteLine("{0,-15} With Release Notes Count: {1,5}", ReleaseNotes.DisplayCoreTitle, NewReleaseNotes.Count);
                        foreach (string notes in NewReleaseNotes)
                            Console.WriteLine("\t{0}", notes);
                        Console.WriteLine("\t\n");
                    }
                }
                sectionNextSibling = sectionNextSibling.NextSibling;
            }

            return ContentList;
        }

        /// <summary>
        /// Merge changes with target section, then display to the user
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static List<IReleaseNotes> Merge(List<IReleaseNotes> sourceContent, List<IReleaseNotes> targetContent)
        {
            #region Idears
            /*
                        // bool updateResult = false;
                        // Option One:

                        //foreach item in the content, search the target file 
                        // if can locate the item, then compare the sub notes
                        // if cannot locate the item, add it in


                        // Option Two:

                        // Get the whole content from the target 
                        // Compare two lists and remove the whole content from the target 
                        // after modified the content, add it back to the target
            */
            #endregion

            // option Three: :)
            // merge the changes and display it to the user to decide what to put in
           
            foreach (IReleaseNotes sourceItem in sourceContent)
            {
                int index = targetContent.FindIndex(t => t.DisplayCoreTitle.Equals(sourceItem.DisplayCoreTitle, StringComparison.CurrentCultureIgnoreCase));

                if(index ==  -1)
                {
                    targetContent.Add(sourceItem);
                }
                else
                {
                    List<string> mergedNotes = MergeNotes(sourceItem.Notes, targetContent[index].Notes);
                    
                    if(sourceItem is DriverReleaseNotes)
                    {
                        var tempSourceItem = (DriverReleaseNotes)sourceItem;
                     
                        if(tempSourceItem.DriverVersion.CompareTo(((DriverReleaseNotes)targetContent[index]).DriverVersion) > 0)
                        {
                            //tempNotes = sourceItem;
                           
                            targetContent[index] = sourceItem;
                        }
                    }

                    targetContent[index].Notes = mergedNotes;
                }
            }

           // targetContent.Sort();

            return targetContent;
        }

        #endregion


        #region Private Support Methods
        private static List<string> MergeNotes(List<string> sourceNotes, List<string> targetNotes)
        {
            List<string> mergedNotes = new List<string>();

            mergedNotes.AddRange(targetNotes);

            foreach(string sourceItem in sourceNotes)
            {
                if(!targetNotes.Contains(sourceItem))
                {
                    mergedNotes.Add(sourceItem);
                }
            }

            return mergedNotes;
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
                if (match.Success)
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


        private IReleaseNotes GetNodeContent(HtmlNode node)
        {
            string pattern = @"(.*)\((.*\.dll)\)\s+v(\d+\.\d+\.?\d*)";
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = reg.Match(node.InnerText);
            if (match.Success)
            {
                DriverReleaseNotes DriverNotes = new DriverReleaseNotes();
                GroupCollection groups = match.Groups;
                DriverNotes.DisplayCoreTitle = groups[2].ToString(); 
                DriverNotes.PageTitle = node.OuterHtml; // full format of the title
                //DriverNotes.DriverDllName = groups[2].ToString();
                DriverNotes.DriverVersion = new Version(groups[3].ToString());
                DriverNotes.Notes = new List<string>();
                return DriverNotes;
            }
            else// this may be a Database entry
            {
                OtherReleaseNotes OtherNotes = new OtherReleaseNotes();
                OtherNotes.DisplayCoreTitle = node.InnerText;
                OtherNotes.PageTitle = node.OuterHtml;
                OtherNotes.Notes = new List<string>();
                return OtherNotes;
            }
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

        private List<String> GetReleaseNotesFromContent(HtmlNode currentNode, SupportedHTMLTag tagExtractFrom, bool tagHasAtt = false)
        {
            List<string> notes = new List<string>();

            HtmlNode ulNode = currentNode.NextSibling;

            while (ulNode != null &&
                !ulNode.Name.Equals(tagExtractFrom.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                ulNode = ulNode.NextSibling;
            }

            if (ulNode.NodeType == HtmlNodeType.Element)
            {
                 var liNodes = ulNode.Descendants(SupportedHTMLTag.ListTag.ToString());
                //var liNodes = ulNode.Descendants();
                foreach (HtmlNode liNode in liNodes)
                {
                    if (liNode.NodeType != HtmlNodeType.Element)
                        continue;

                    if (liNode.HasChildNodes && liNode.FirstChild.Name.Equals(SupportedHTMLTag.SpanTag.ToString(), StringComparison.CurrentCultureIgnoreCase)) // It reports TRUE for HasChildNodes but has no Child at all
                    {
                        var firstChild = liNode.FirstChild;
                        //if (firstChild.HasAttributes)
                        //{
                        //    if (isEmbRelease && firstChild.InnerText.Equals(Parameters.ReleaseType.EMBEDDED, StringComparison.CurrentCultureIgnoreCase))
                        //    {
                        //        if (liNode.InnerText.Length > 0)
                        //        {
                        //            HtmlNode tempLiNode = liNode;
                        //            RemoveUnneededTag(SupportedHTMLTag.SpanTag, ref tempLiNode);
                        //            notes.Add(tempLiNode.OuterHtml);
                        //            // tempLiNode.Remove();
                        //        }
                        //    }
                        //    else if (!isEmbRelease && firstChild.InnerText.Equals(Parameters.ReleaseType.DESKTOP, StringComparison.CurrentCultureIgnoreCase))
                        //    {
                        //        if (liNode.InnerText.Length > 0)
                        //        {
                        //            HtmlNode tempLiNode = liNode;
                        //            RemoveUnneededTag(SupportedHTMLTag.SpanTag, ref tempLiNode);
                        //            notes.Add(tempLiNode.OuterHtml);
                        //            // tempLiNode.Remove();
                        //        }
                        //    }

                            if(isEmbRelease)
                            {
                                if (firstChild.InnerText.Equals(Parameters.ReleaseType.EMBEDDED, StringComparison.CurrentCultureIgnoreCase))
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
                            else
                            {
                                if (firstChild.InnerText.Equals(Parameters.ReleaseType.DESKTOP, StringComparison.CurrentCultureIgnoreCase))
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

                        //TODO:
                        //the first SPAN for desktop has been removed, and the second SPAN still here and is AnalyzeContent Element, with the value is EMBEDDED, and added into the notes


                        if (liNode.InnerText.Length > 0)
                            {
                                HtmlNode tempLiNode = liNode;
                                notes.Add(tempLiNode.OuterHtml);
                                // tempLiNode.Remove();
                            }
                        //if (liNode.InnerText.Length > 0)
                        //    notes.Add(liNode.OuterHtml);
                    }
                }
            }

            return notes;
        }

        private void RemoveUnneededTag(SupportedHTMLTag tagNeedsToRemove, ref HtmlNode node)
        {
            var childrenNodes = node.ChildNodes;
            List<HtmlNode> nodesNeedToRemove = new List<HtmlNode>();
            foreach (HtmlNode childNode in childrenNodes)
            {
                if (childNode.Name.Equals(tagNeedsToRemove.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    nodesNeedToRemove.Add(childNode);
                }
            }

            foreach (HtmlNode tempNode in nodesNeedToRemove)
                node.RemoveChild(tempNode);
        }

        /// <summary>
        /// If the driver is an example, then ignore the item
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool ValidateContent(string content)
        {
            bool validateResult = false;

            if (Parameters.InvalidContent.Find(i => i.Equals(content, StringComparison.CurrentCultureIgnoreCase)) == null)
                validateResult = true;

            return validateResult;
        }
        #endregion
    }
}
