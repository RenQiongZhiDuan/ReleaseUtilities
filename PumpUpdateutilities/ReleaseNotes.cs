using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseUtilities
{
    //public interface IReleaseItem
    //{
    //    /// <summary>
    //    /// What you see from the page
    //    /// For Driver - this is the name.dll
    //    /// For other - this is the content
    //    /// </summary>
    //    string Name { get; set; }

    //    Version Version { get; set; }
    //    /// <summary>
    //    /// What you see from the page source
    //    /// For Driver - this is the <h3>XXX (XXX.dll) v1.2.3</h3>
    //    /// For other - this is the <h3>XXX</h3>
    //    /// </summary>
    //    string NameWithTag { get; set; }

    //    List<string> Notes { get; set; }

    //  //  IReleaseItem Clone(IReleaseItem notes);
        
    //}

    //public class DriverItem :  IReleaseItem
    //{
    //    public string Name { get; set; }

    //    public string NameWithTag { get; set; }

    //    public Version Version { get; set; }

    //    public List<string> Notes { get; set; }

    //    //public IReleaseItem Clone(IReleaseItem notes)
    //    //{
    //    //    DriverItem newDriverRN = null;
    //    //    if (notes is DriverItem)
    //    //    {
    //    //        newDriverRN = new DriverItem();
    //    //        newDriverRN.Name = notes.Name;
    //    //        newDriverRN.NameWithTag = ((DriverItem)notes).NameWithTag;
    //    //        newDriverRN.DriverVersion = ((DriverItem)notes).DriverVersion;
    //    //        newDriverRN.Notes = ((DriverItem)notes).Notes.ToList();
    //    //    }
    //    //    return newDriverRN;
    //    //}
    //}

    //public class OtherItem :  IReleaseItem
    //{
    //    public string Name { get; set; }
    //    public Version Version { get; set; }
    //    public string NameWithTag { get; set; }
    //    public List<string> Notes { get; set; }

    //    //public IReleaseItem Clone(IReleaseItem notes)
    //    //{
    //    //    OtherItem newNotes = null;
    //    //    if (notes is OtherItem)
    //    //    {
    //    //        newNotes = new OtherItem();
    //    //        newNotes.Name = notes.Name;
    //    //        newNotes.NameWithTag = notes.NameWithTag;
    //    //        newNotes.Notes = ((OtherItem)notes).Notes.ToList();
    //    //    }
    //    //    return newNotes;
    //    //}
    //}

    public interface IReleaseItem
    {
        string Name { get; set; }
        Version Version { get; set; }
    }

    public interface IHTMLElement : IReleaseItem
    {
        //Version Version { get; set; }
        List<string> Notes { get; set; }
    }

    public interface IFileElement : IReleaseItem
    {
        FileInfo FileInfo { get; set; } 
    }

    public class HTMLElement : IHTMLElement
    {
        public string Name { get; set; }

        public Version Version { get; set; }

        public List<string> Notes { get; set; }

        // self element
        public string OuterHtml { get; set; }
    }

    public class FileElement : IFileElement
    {
        public FileInfo FileInfo { get; set; }

        public string Name { get; set; }

        public Version Version { get; set; }
    }
}
