using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReleaseUtilities
{
    public static class VersionComparer
    {
        public delegate void MessageDelegate(string message);
        public static event MessageDelegate SendMessage;
        public static string CompareVersions(
            string sourcePath, List<IReleaseItem> source,
            string targetPath, List<IReleaseItem> target)
        {
            StringBuilder compareResult = new StringBuilder();

            compareResult.AppendFormat("Comparing '{0}' with '{1}'\n\n\n", sourcePath, targetPath);
            
            foreach (IReleaseItem item in source)
            {
                int index = target.FindIndex(i => i.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase));
                if (index >= 0)
                {
                    // compare the version
                    Version targetVersion = target[index].Version;

                    string outputString = string.Format(
                        "{0, -50}\t\n{1,-25}\tVersion:{2,-6}\t\n" +
                        "{3, -50}\t\n{4,-25}\tVersion:{5,-30}\t", sourcePath, item.Name, item.Version, 
                  targetPath, target[index].Name, targetVersion);

                    compareResult.Append(outputString);

                    if (targetVersion.Equals(item.Version))
                    {
                        Console.WriteLine(outputString + "OK\n");
                        compareResult.AppendLine("OK\n");
                    }
                    else
                    {
                        Console.WriteLine(outputString + "Diff\n");
                        compareResult.AppendLine("Diff\n");
                        if (SendMessage != null)
                            SendMessage(outputString);
                    }
                }
                else
                {
                    Console.WriteLine("Unable to find {0, -3} in {1}", item.Name, targetPath);
                    if (SendMessage != null)
                        SendMessage("Unable to compare with " + item.Name);
                    compareResult.AppendLine(String.Format("Unable to find {0, -3} in {1}\n", item.Name, targetPath));
                }
            }

            return compareResult.ToString();
        }
    }
}
