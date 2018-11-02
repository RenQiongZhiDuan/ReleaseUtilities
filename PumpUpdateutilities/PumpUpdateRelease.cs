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
            Task<List<IReleaseItem>> GetFileFromDir = new Task<List<IReleaseItem>>(()=> GetDirItem());
            GetFileFromDir.Start();

            Task<List<IReleaseItem>> GetNotesFromPendingNotes = new Task<List<IReleaseItem>>(() => GetPendingNotes());
            GetNotesFromPendingNotes.Start();

            Task<List<IReleaseItem>> GetLocalNotesFromPumpUpdateNotes = new Task<List<IReleaseItem>>(() => GetLocalPumpUpdateNotes());
             GetLocalNotesFromPumpUpdateNotes.Start();

            Task<List<IReleaseItem>> GetVersionFromTable = new Task<List<IReleaseItem>>(() => GetVersionListFromPage());
            GetVersionFromTable.Start();

            // wait for all tasks to complete
            Task.WaitAll(GetFileFromDir, GetNotesFromPendingNotes, GetLocalNotesFromPumpUpdateNotes, GetVersionFromTable);

            List<IReleaseItem> DirItems = GetFileFromDir.Result;
            List<IReleaseItem> PendingItems = GetNotesFromPendingNotes.Result;
            List<IReleaseItem> PumpUpdateItems = GetLocalNotesFromPumpUpdateNotes.Result;
            List<IReleaseItem> TableVersions = GetVersionFromTable.Result;

            // this merged content is used to replace the one in pumpupdate
            List<IReleaseItem> MergedContent = MergeNotes(PendingItems, PumpUpdateItems);

            StringBuilder outputStr = new StringBuilder(); 
            foreach(IReleaseItem item in MergedContent)
            {
                outputStr.AppendLine(item.NameWithTag);

                foreach(var notes in item.Notes)
                {
                    outputStr.AppendLine(notes);
                }
            }

            YLog.WriteTo(Parameters.GetPumpUpdateTempHtmPath, true, outputStr.ToString());

            // compare construction version with PumpUpdate Table version
            YLog.WriteTo(Parameters.GetPumpUpdateTempTxtPath, true, VersionComparer.CompareVersions(GetConstructionFolder, DirItems, Parameters.GetLocalPumpUpdatePath + " - Driver Table", TableVersions));

            // compare temp version with merged content
            YLog.WriteTo(Parameters.GetPumpUpdateTempTxtPath, false, VersionComparer.CompareVersions(GetConstructionFolder, DirItems, Parameters.GetPumpUpdateTempHtmPath, MergedContent));

            // Console.ReadKey();
        }


        string GetConstructionFolder
        {
            get
            {
                string releaseDateFormat = _releaseDate.ToString("yyyy.MM.dd.0");
                return Path.Combine(Parameters.ServerConstructionPath, releaseDateFormat);
            }
        }


        #region Support Methods

        private List<IReleaseItem> GetDirItem()
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
        private List<IReleaseItem> GetPendingNotes()
        {
            WebPageAnalyzer webAnalyzer = new WebPageAnalyzer(Parameters.GetLocalPendingPath);
            return webAnalyzer.AnalyzeSectionContent(Parameters.PendingChanges.ReadyToRelease);
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
        private List<IReleaseItem> GetLocalPumpUpdateNotes()
        {
            WebPageAnalyzer webAnalyzer = new WebPageAnalyzer(Parameters.GetLocalPumpUpdatePath);
            return webAnalyzer.AnalyzeSectionContent(_releaseDate.ToString("dd MMMM yyyy"));
        }

        /// <summary>
        /// Get version from PumpUpdate Driver table
        /// 
        /// </summary>
        /// <returns></returns>
        private List<IReleaseItem> GetVersionListFromPage()
        {
            WebPageAnalyzer webAnalyzer = new WebPageAnalyzer(Parameters.GetLocalPumpUpdatePath);
            return webAnalyzer.AnalyzeTable();
        }

        private List<IReleaseItem> MergeNotes(List<IReleaseItem> sourceItems, List<IReleaseItem> targetItems)
        {
            return WebPageAnalyzer.Merge(sourceItems, targetItems);
        }
        #endregion
    }
}
