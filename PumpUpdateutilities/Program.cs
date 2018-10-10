using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpUpdateutilities
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input the Release Date to be checked: eg: 2018-10-05");

            /*
             * 
             * Test Section
             * */

            WebPageAnalyzer analyzer = new WebPageAnalyzer(System.IO.Path.Combine(Parameters.InternalNotesPath, Parameters.PendingReleaselNotes));
            analyzer.AnalyzeNotes("Release");

            string inputString = Console.ReadLine();

            do
            {
                if (inputString == null)
                {
                    Console.WriteLine("Invalid input date");
                    inputString = Console.ReadLine();
                }
                else
                    break;
            }
            while (inputString != null);

            try
            {
                DateTime releaseDate = Convert.ToDateTime(inputString);
                VersionComparer verionComparer = new VersionComparer(releaseDate);
                verionComparer.Start();
            }
            catch (System.FormatException)
            {
                Console.WriteLine("Unable to convert to dateTime{0}", args[0]);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }

            Console.WriteLine("Completed");
            Console.ReadKey();
        }
    }
}
