﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpUpdateutilities
{
    static public class Parameters
    {
        public static string ServerConstructionPath = @"\\appserv\files\Releases\Construction\PumpUpdate\";

        public static string ServerReleaseNotePath = @"\\appserv\files\Releases\Internal Releases\";

        public static string InternalNotesPath = @"C:\WORK\Enabler\Install\PumpUpdate\";
        
        public static string PendingReleaselNotes = @"PendingChanges.htm";

        public static string PumpUpdateReleaseNotes = "PumpUpdate.htm";

        public static List<string> ValidPumpDriverUpdateExtList = new List<string>() { ".dll", ".app", ".nxe", ".nei" };

        public static string PumpDriverExt = @".dll";

        public static string[] FirmwareExtList = { "app", "nxe", "nei" };

        public static class ReleaseType
        {
            public static string DESKTOP = "Desktop";
            public static string Embedded = "Embedded";
        }

        public static class HTMLTag
        {
            public static string SectionTag = "h1";
            public static string DriverOrDBTag = "h3";
            public static string ReleaseNotesTag = "li";
        }
    }

    public sealed class SupportedHTMLTag
    {
        private readonly int _value;
        private readonly string _tag;

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

        public override string ToString()
        {
            return _tag;
        }

    }
        
}