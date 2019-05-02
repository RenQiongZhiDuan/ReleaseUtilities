using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseUtilities
{
    public static class YLog
    {
        class LogParameters
        {
            double FileMSize = 8; // m
        }

        public static void WriteTo(string filePath, bool overwrite, string message, params object[] parameters)
        {
            if(File.Exists(filePath))
            {
                if (overwrite)
                {
                    File.Delete(filePath);
                    File.Create(filePath).Close();// make sure we close the 
                }  
            }
            else
            {
                File.Create(filePath).Close();
            }

            if (parameters.Length != 0)
                message = String.Format(message, parameters);

            // add date and time
           // message = String.Format("{0}: {1}", DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff"), message);

            using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                StreamWriter write = new StreamWriter(stream);
                write.WriteLine(message);
                write.Flush();
                Console.WriteLine(message);
            }
        }

        public static void WriteToHtml(string filePath, bool overwrite, string innerText, string HtmlFormat, string color)
        {

        }
    }
}
