using System;
using System.Collections.Generic;
using System.Text;

namespace ReleaseUtilities
{
    public static class VersionComparer
    {
        public delegate void MessageDelegate(string message);
        public static event MessageDelegate SendMessage;
        /// <summary>
        /// The one contains more element should be the source
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Target"></param>
        /// <returns></returns>
        public static string CompareVersions(
            KeyValuePair<string, List<IReleaseItem>> Source,
            KeyValuePair<string, List<IReleaseItem>> Target)
        {
            StringBuilder compareResult = new StringBuilder();

            compareResult.AppendFormat("Comparing '{0}' with '{1}'\n\n\n", Source.Key, Target.Key);
            
            foreach (IReleaseItem sourceItem in Source.Value)
            {
                int indexInTarget = Target.Value.FindIndex(i => i.Name.Equals(sourceItem.Name, StringComparison.CurrentCultureIgnoreCase));
                if (indexInTarget >= 0)
                {
                    // location - name - type - data \n 
                    string outputString =
                       "{0, -50}\t\n{1,-25}\t{6}:{2,-6}\t\n" +
                       "{3, -50}\t\n{4,-25}\t{6}:{5,-30}\t";
                    string compareType = "Version";

                    // compare the version
                    Version targetVersion = Target.Value[indexInTarget].Version;
                    Version ZeroVersionOne = new Version(0, 0, 0);
                    Version ZeroVersionTwo = new Version(0, 0, 0, 0);
                    bool isZeroVersion = targetVersion.Equals(ZeroVersionOne) || targetVersion.Equals(ZeroVersionTwo);

                    if (targetVersion != null && !isZeroVersion )
                    {
                        outputString = string.Format(outputString, Source.Key, sourceItem.Name, sourceItem.Version,
                  Target.Key, Target.Value[indexInTarget].Name, targetVersion, compareType);

                        if (targetVersion.Equals(sourceItem.Version))
                        {
                            //Console.WriteLine(outputString + Parameters.Same);
                            outputString += Parameters.Same;
                        }
                        else
                        {
                            //Console.WriteLine(outputString + Parameters.Diff);
                            outputString += Parameters.Diff;
                            if (SendMessage != null)
                                SendMessage(outputString);
                        }
                        //compareResult.Append(outputString);
                    }
                    else // no version, we compare size and last write date time
                    {
                        compareType = "Size";
                        FileElement sourceEle = null, targetEle = null;
                        if (sourceItem is FileElement)
                            sourceEle = (FileElement)sourceItem;
                        if (Target.Value[indexInTarget] is FileElement)
                            targetEle = (FileElement)Target.Value[indexInTarget];

                        if(sourceEle != null && targetEle != null)
                        {
                            outputString =
                       "{0, -50}\t\n{4}:{1,-6}\t\n" +
                       "{2, -50}\t\n{4}:{3,-30}\t";

                            if (sourceEle.FileInfo.LastWriteTime == targetEle.FileInfo.LastWriteTime)
                            {
                                outputString = string.Format(outputString,  sourceEle.FileInfo.FullName, sourceEle.FileInfo.Length,  targetEle.FileInfo.FullName, targetEle.FileInfo.Length, compareType);
                               
                                if (sourceEle.FileInfo.Length != targetEle.FileInfo.Length)
                                {
                                    outputString += Parameters.Diff;
                                }
                                else
                                {
                                    outputString += Parameters.Same;
                                }
                            }
                            else
                            {
                                compareType = "Last Write Time";
                                outputString = string.Format(outputString,  sourceEle.FileInfo.FullName, sourceEle.FileInfo.LastWriteTime,  targetEle.FileInfo.FullName, targetEle.FileInfo.LastWriteTime, compareType);

                                outputString += Parameters.Diff;
                            }
                            //compareResult.Append(outputString);
                        }
                        else // not all are file elements
                        {
                            compareResult.Append("----------------------------------");
                        }
                    }
                    outputString += " - " + compareType +"\r\n";
                    compareResult.Append(outputString);
                }
                else
                {
                    Console.WriteLine("Unable to find {0, -3} in {1}", sourceItem.Name, Target.Key);
                    if (SendMessage != null)
                        SendMessage("Unable to compare with " + sourceItem.Name);
                    compareResult.AppendLine(String.Format("Unable to find {0, -3} in {1}\n", sourceItem.Name, Target.Key));
                }
                compareResult.AppendLine();
            }// end foreach


            //Console.WriteLine(compareResult.ToString());
            return compareResult.ToString();
        }
    }
}
