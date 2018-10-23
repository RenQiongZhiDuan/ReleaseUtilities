using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpUpdateutilities
{
    public interface IReleaseNotes
    {
        /// <summary>
        /// What you see from the page
        /// For Driver - this is the name.dll
        /// For other - this is the content
        /// </summary>
        string DisplayCoreTitle { get; set; }
        /// <summary>
        /// What you see from the page source
        /// For Driver - this is the <h3>XXX (XXX.dll) v1.2.3</h3>
        /// For other - this is the <h3>XXX</h3>
        /// </summary>
        string PageTitle { get; set; }
        List<string> Notes { get; set; }

        IReleaseNotes Clone(IReleaseNotes notes);
        
    }

    public class DriverReleaseNotes :  IReleaseNotes
    {
        public Version DriverVersion;

        public string DisplayCoreTitle { get; set; }

        public string PageTitle { get; set; }
        public List<string> Notes { get; set; }

        public IReleaseNotes Clone(IReleaseNotes notes)
        {
            DriverReleaseNotes newDriverRN = null;
            if (notes is DriverReleaseNotes)
            {
                newDriverRN = new DriverReleaseNotes();
                newDriverRN.DisplayCoreTitle = notes.DisplayCoreTitle;
                newDriverRN.PageTitle = ((DriverReleaseNotes)notes).PageTitle;
                newDriverRN.DriverVersion = ((DriverReleaseNotes)notes).DriverVersion;
                newDriverRN.Notes = ((DriverReleaseNotes)notes).Notes.ToList();
            }
            return newDriverRN;
        }
    }

    public class OtherReleaseNotes :  IReleaseNotes
    {
        public string DisplayCoreTitle { get; set; }
        public string PageTitle { get; set; }
        public List<string> Notes { get; set; }

        public IReleaseNotes Clone(IReleaseNotes notes)
        {
            OtherReleaseNotes newNotes = null;
            if (notes is OtherReleaseNotes)
            {
                newNotes = new OtherReleaseNotes();
                newNotes.DisplayCoreTitle = notes.DisplayCoreTitle;
                newNotes.PageTitle = notes.PageTitle;
                newNotes.Notes = ((OtherReleaseNotes)notes).Notes.ToList();
            }
            return newNotes;
        }
    }
}
