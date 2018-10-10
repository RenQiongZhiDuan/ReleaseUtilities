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
    }
    public class DriverReleaseNotes :  IReleaseNotes
    {
        string DriverName;
        public string DriverDllName;

        public Version DriverVersion;
         List<string> DriverNotes;

        public string Name { get { return DriverName; } set { DriverName = value; }}
        public List<string> Notes { get { return Notes; } set { Notes = value; }}
    }

    public class DatabaseReleaseNotes :  IReleaseNotes
    {
        private string DatabaseName;
        private List<string> DriverNotes;

        public string Name { get { return DatabaseName; } set { DatabaseName = value; } }
        public List<string> Notes { get { return Notes; } set { Notes = value; } }
    }
}
