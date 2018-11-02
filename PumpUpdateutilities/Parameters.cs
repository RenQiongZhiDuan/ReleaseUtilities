using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseUtilities

{
    static public class Parameters
    {
        public static string ServerConstructionPath = @"\\appserv\files\Releases\Construction\PumpUpdate\";

        public static string ServerReleaseNotePath = @"\\appserv\files\Releases\Internal Releases\";

        static string localPumpUpdateNotesPath = @"C:\WORK\Enabler\Install\PumpUpdate\";
        
        static string PendingReleaselNotes = @"PendingChanges.htm";

        static string PumpUpdateReleaseNotes = "PumpUpdate.htm";

        static string TempHtmFile = "Temp.htm";
        static string TempTxtFile = "Temp.txt";

        public static List<string> ValidPumpDriverUpdateExtList = new List<string>() { ".dll", ".app", ".nxe", ".nei" };

        public static string PumpDriverExt = @".dll";

        public static List<string> FirmwareExtList = new List<string> { "app", "nxe", "nei" };

        public static List<string> InvalidContent = new List<string> { "exampledriver.dll"};

        public static class PumpUpdate
        {
            public static string ReleaseSection = "Pump Update -";
        }

        public static class PendingChanges
        {
            public static string ReadyToRelease = "Changes Ready for Release";
        }

        public enum ReleaseType
        {
            DESKTOP,
            EMBEDDED,
            PUMPUPDATE,
        }

        public static string[] ReleaseTypeStr =  { "Desktop", "Embedded", "PumpUpdate" };

        public static string LogFolder = @"C:\";


        #region Support Properties

        public static string GetLocalPumpUpdateFolder
        {
            get
            {
                return Path.Combine(Parameters.localPumpUpdateNotesPath);
            }
        }

        public static string GetPumpUpdateTempHtmPath
        {
            get
            {
                return Path.Combine(Parameters.localPumpUpdateNotesPath, Parameters.TempHtmFile);
            }
        }

        public static string GetPumpUpdateTempTxtPath
        {
            get
            {
                return Path.Combine(Parameters.localPumpUpdateNotesPath, Parameters.TempTxtFile);
            }
        }

        public static string GetLocalPendingPath
        {
            get
            {
                return Path.Combine(GetLocalPumpUpdateFolder, Parameters.PendingReleaselNotes);
            }

        }

        public static string GetLocalPumpUpdatePath
        {
            get
            {
                return Path.Combine(GetLocalPumpUpdateFolder, Parameters.PumpUpdateReleaseNotes);
            }
        }
        #endregion
    }

    public sealed class TargetType
    {
        private string _target;
        private string _constructionPath;
        private string _releasePath;

        private TargetType(string target, string constructionPath, string releasePath)
        {
            _target = target;
            _constructionPath = constructionPath;
            _releasePath = releasePath;
        }

        public string GetTargetType()
        {
            return _target;
        }

        public string GetConstractionPath()
        {
            return _constructionPath;
        }

        public string GetReleasePath()
        {
            return _releasePath;
        }

    }

    public sealed class SupportedHTMLTag
    {
        private readonly int _value;
        private readonly string _tag;


        public static readonly SupportedHTMLTag TableTag = new SupportedHTMLTag("table");
        public static readonly SupportedHTMLTag TableRowTag = new SupportedHTMLTag("tr");
        public static readonly SupportedHTMLTag TableCellTag = new SupportedHTMLTag("td");
        /// <summary>
        /// Section Header
        /// </summary>
        public static readonly SupportedHTMLTag HeaderOneTag = new SupportedHTMLTag(1, "h1");
        /// <summary>
        /// Notes Header
        /// </summary>
        public static readonly SupportedHTMLTag HeaderTwoTag = new SupportedHTMLTag(2, "h2");
        /// <summary>
        /// Driver / Database Header
        /// </summary>
        public static readonly SupportedHTMLTag HeaderThreeTag = new SupportedHTMLTag(3, "h3");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SupportedHTMLTag UnorderedTag = new SupportedHTMLTag(3, "ul");
        /// <summary>
        /// Release Notes Header
        /// </summary>
        public static readonly SupportedHTMLTag ListTag = new SupportedHTMLTag(3, "li");
        /// <summary>
        /// Enabler / Embedded Span Tag
        /// </summary>
        public static readonly SupportedHTMLTag SpanTag = new SupportedHTMLTag(4, "span");

        private SupportedHTMLTag(int value, string tag)
        {
            _tag = tag;
            _value = value;
        }

        private SupportedHTMLTag(string tag)
        {
            _tag = tag;
        }

        public override string ToString()
        {
            return _tag;
        }

    }
        
    
}
