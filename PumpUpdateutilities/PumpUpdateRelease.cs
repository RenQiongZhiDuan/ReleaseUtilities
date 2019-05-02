using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseUtilities
{
    /*
     * A:   Get versions from Construction Folder   -   see E
     * B:   Get ReleaseNotes from PendingRelease
     * C:   Get ReleaseNotes from PumpUpdate
     * D:   Merge ReleaseNotes and output to a temp file        -   MergedNotes.htm
     * E:   Compare Version in temp file with the Construction folder
     * F:   Output the different if there is any in temp file 2 -   VersionDiff.htm
     * */
    class PumpUpdateRelease
    {
        DateTime _releaseDate;
        
        public PumpUpdateRelease(DateTime releaseDate)
        {
            _releaseDate = releaseDate;
            //VersionComparer.SendMessage += VersionComparer_SendMessage;
        }

        ~PumpUpdateRelease()
        {
            // must do this, otherwise, the GC will not collect it.
            //VersionComparer.SendMessage -= VersionComparer_SendMessage;
        }
        
        public void Start()
        {
            Task<KeyValuePair<string, List<IReleaseItem>>> GetFileFromDir = new Task<KeyValuePair<string, List<IReleaseItem>>>(()=> GetDirItem());
            GetFileFromDir.Start();

            Task<KeyValuePair<string, List<IReleaseItem>>> GetNotesFromPendingNotes = new Task<KeyValuePair<string, List<IReleaseItem>>>(() => GetPendingNotes());
            GetNotesFromPendingNotes.Start();

            Task<KeyValuePair<string, List<IReleaseItem>>> GetLocalNotesFromPumpUpdateNotes = new Task<KeyValuePair<string, List<IReleaseItem>>>(() => GetLocalPumpUpdateNotes());
             GetLocalNotesFromPumpUpdateNotes.Start();

            Task<KeyValuePair<string, List<IReleaseItem>>> GetVersionFromTable = new Task<KeyValuePair<string, List<IReleaseItem>>>(() => GetVersionListFromPage());
            GetVersionFromTable.Start();

            // wait for all tasks to complete
            Task.WaitAll(GetFileFromDir, GetNotesFromPendingNotes, GetLocalNotesFromPumpUpdateNotes, GetVersionFromTable);

            KeyValuePair<string, List<IReleaseItem>> DirItems = GetFileFromDir.Result;
            KeyValuePair<string, List<IReleaseItem>> PendingItems = GetNotesFromPendingNotes.Result;
            KeyValuePair<string, List<IReleaseItem>> PumpUpdateItems = GetLocalNotesFromPumpUpdateNotes.Result;
            KeyValuePair<string, List<IReleaseItem>> TableVersions = GetVersionFromTable.Result;

            // this merged content is used to replace the one in pump update
            List<IReleaseItem> MergedContent = MergeNotes(PendingItems.Value, PumpUpdateItems.Value);

            StringBuilder outputStr = new StringBuilder(); 
            // page content
            foreach(HTMLElement item in MergedContent)
            {
                outputStr.AppendLine(item.OuterHtml);

                foreach(var notes in item.Notes)
                {
                    outputStr.AppendLine(notes);
                }
            }

            YLog.WriteTo(Parameters.GetPumpUpdateTempHtmPath, true, outputStr.ToString());

            // compare construction version with PumpUpdate Table version
            // file compare with html
            YLog.WriteTo(Parameters.GetPumpUpdateTempTxtPath, true, VersionComparer.CompareVersions( DirItems,  TableVersions));

            // compare temp version with merged content
            // html compare with html
            YLog.WriteTo(Parameters.GetPumpUpdateTempTxtPath, false, VersionComparer.CompareVersions( DirItems, new KeyValuePair<string, List<IReleaseItem>>(Parameters.GetPumpUpdateTempHtmPath, MergedContent)));
        }

        string GetConstructionFolder
        {
            get
            {
                string releaseDateFormat = _releaseDate.ToString("yyyy.MM.dd.0");
                return Path.Combine(Parameters.ServerPumpUpdateConstructionPath, releaseDateFormat);
            }
        }

        #region Support Methods

        private KeyValuePair<string, List<IReleaseItem>> GetDirItem()
        {
            return DirectoryOperator.GetFileVersionsFromDir(GetConstructionFolder, Parameters.ReleaseType.PUMPUPDATE);
        }

        /// <summary>
        /// Get Notes from Pending Changes under "Changes Ready for Release" section
        /// Get all nodes under <h3>
        /// <h3>Example Pump Driver Name (ExampleDriver.DLL) v0.0.0</h3>
        ///     <ul class="spacedlist">
	    ///         <li><span class="capsule-blue">Desktop</span>Description of a change.</li>
	    ///         <li><span class="capsule-green">Embedded</span>Description of a change.</li>
        ///     </ul>
        /// </summary>
        /// <returns></returns>
        private KeyValuePair<string, List<IReleaseItem>> GetPendingNotes()
        {
            WebPageAnalyzer webAnalyzer = new WebPageAnalyzer(Parameters.GetLocalPendingPath);
            return new KeyValuePair<string, List<IReleaseItem>>(Parameters.GetLocalPendingPath, webAnalyzer.AnalyzeSectionContent(Parameters.PendingChanges.ReadyToRelease));
        }

        /// <summary>
        /// Get notes under Release date <h2>Pump Update - 05 October 2018</h2>
        /// Get all nodes from <h3>
        /// <h3>Example Pump Driver Name (ExampleDriver.DLL) v0.0.0</h3>
        ///     <ul class="spacedlist">
	    ///         <li><span class="capsule-blue">Desktop</span>Description of a change.</li>
	    ///         <li><span class="capsule-green">Embedded</span>Description of a change.</li>
        ///     </ul> 
        /// </summary>
        /// <returns></returns>
        private KeyValuePair<string, List<IReleaseItem>> GetLocalPumpUpdateNotes()
        {
            WebPageAnalyzer webAnalyzer = new WebPageAnalyzer(Parameters.GetLocalPumpUpdatePath);
            return new KeyValuePair<string, List<IReleaseItem>>(Parameters.GetLocalPumpUpdatePath, webAnalyzer.AnalyzeSectionContent(_releaseDate.ToString("dd MMMM yyyy")));
        }

        /// <summary>
        /// Get version from PumpUpdate Driver table
        /// 
        /// </summary>
        /// <returns></returns>
        private KeyValuePair<string, List<IReleaseItem>> GetVersionListFromPage()
        {
            WebPageAnalyzer webAnalyzer = new WebPageAnalyzer(Parameters.GetLocalPumpUpdatePath);
            return new KeyValuePair<string, List<IReleaseItem>>("Table in "+Parameters.GetLocalPumpUpdatePath, webAnalyzer.AnalyzeTable());
        }

        private List<IReleaseItem> MergeNotes(List<IReleaseItem> sourceItems, List<IReleaseItem> targetItems)
        {
            return WebPageAnalyzer.MergeHTMLNotes(sourceItems, targetItems);
        }
        #endregion
    }
}
