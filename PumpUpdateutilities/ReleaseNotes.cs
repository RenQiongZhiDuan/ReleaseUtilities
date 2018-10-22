using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpUpdateutilities
{
    public interface IReleaseNotes
    {
        string Name { get; set; }
        List<string> Notes { get; set; }

        IReleaseNotes Clone(IReleaseNotes notes);
        
    }

    public class DriverReleaseNotes :  IReleaseNotes
    {
        public string DriverDllName;

        public Version DriverVersion;

        public string Name { get; set; }
        public List<string> Notes { get; set; }

        public IReleaseNotes Clone(DriverReleaseNotes notes)
        {
            DriverReleaseNotes newDriverRN = new DriverReleaseNotes();

            newDriverRN.Name = notes.Name;

            return newDriverRN;
        }

        public IReleaseNotes Clone(IReleaseNotes notes)
        {
            DriverReleaseNotes newDriverRN = null;
            if (notes is DriverReleaseNotes)
            {
                newDriverRN = new DriverReleaseNotes();
                newDriverRN.Name = notes.Name;
                newDriverRN.DriverDllName = ((DriverReleaseNotes)notes).DriverDllName;
                newDriverRN.DriverVersion = ((DriverReleaseNotes)notes).DriverVersion;
                newDriverRN.Notes = ((DriverReleaseNotes)notes).Notes.ToList();
            }
            return newDriverRN;
        }
    }

    public class OtherReleaseNotes :  IReleaseNotes
    {
        public string Name { get; set; }
        public List<string> Notes { get; set; }

        public IReleaseNotes Clone(IReleaseNotes notes)
        {
            OtherReleaseNotes newNotes = null;
            if (notes is OtherReleaseNotes)
            {
                newNotes = new OtherReleaseNotes();
                newNotes.Name = notes.Name;
                newNotes.Notes = ((OtherReleaseNotes)notes).Notes.ToList();
            }
            return newNotes;
        }
    }
}
