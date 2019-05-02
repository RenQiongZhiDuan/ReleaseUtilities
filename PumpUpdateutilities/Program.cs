using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseUtilities
{
    class Program
    {

        /*  Level
         *  -   Release Type:
         *      1-   PumpUpdate
         *          2-   Release Date 
         *                          Get Version From Construction Folder 
         *                          Merge PendingNotes and PumpUpdate -> PumpUpdate
         *                          Compare the version from C to the merged htm -> Output
         *      1-   Embedded
         *          2-   Release Version
         *          2-   Previous Released Version
         *                          Get components version from Construction folder and                                 compare with previous release -> List differences
         *                          Get version from EMBReleaseNotes, compare with the same                             component in the Construction folder -> Output.htm
         *      1-   Desktop
         *          2-   Release Version
         *          2-   Previous Released Version
         *                          Same as Embedded
         * 
         * */

        enum ProcessResults
        {
            GOOD,
            NEXT,
            NEXT2,
            BACK,
        }

        static Parameters.ReleaseType menuLevel = Parameters.ReleaseType.NONE;
        static ProcessResults result;
        static string releaseDetails;
        static Version currentRelease, lastRelease;
        static DateTime pumpUpdateReleaseDateTime;

        static void Main(string[] args)
        {
            char choice;
            do
            {
                ShowHelp();
                ShowMenu();

                switch (menuLevel)
                {
                    case Parameters.ReleaseType.DESKTOP:
                    case Parameters.ReleaseType.EMBEDDED:
                        EnablerRelease eRelease = new EnablerRelease(menuLevel, currentRelease, lastRelease);
                        eRelease.Start();
                        break;
                    case Parameters.ReleaseType.PUMPUPDATE:
                        PumpUpdateRelease pRelease = new PumpUpdateRelease(Convert.ToDateTime(releaseDetails));
                        pRelease.Start();
                        break;
                    case Parameters.ReleaseType.NONE:
                        break;
                }
                Console.WriteLine("Completed, Run another one? y");
                choice = Console.ReadLine()[0];
            } while (choice == 'y');
            
            Console.ReadKey();
        }

        static void ShowHelp()
        {
            string helpStr = "Utilities Usage: \n" ;
            
            helpStr += "Select Release Type and Enter required info to start the comparison.\t" +
                "Release Type:\n" +
                "1:\tPump Update Release;\n" +
                "2:\tEnabler Embedded Release;\n" +
                "3:\tEnabler Desktop Release;\n";

            Console.WriteLine(helpStr);
        }

        static void ShowMenu()
        {
            releaseDetails = string.Empty;
            result = ProcessResults.BACK;
            do
            {
                DisplayMenu();
                switch(menuLevel)
                {
                    case Parameters.ReleaseType.NONE:
                        char choice = Console.ReadLine()[0];
                        if (choice >= '1' && choice <= '3')
                        {
                            menuLevel = (Parameters.ReleaseType)Convert.ToInt32(choice - '0');
                            result = ProcessResults.NEXT;
                        }
                        else if (choice == 'q')
                            result = ProcessResults.GOOD;
                        else
                            result = ProcessResults.BACK;
                        break;
                    default:
                        releaseDetails = Console.ReadLine();
                        ProcessSubInput(releaseDetails, ref result);
                        break;
                }
            } while (result != ProcessResults.GOOD);

            // start doing 
            Console.WriteLine("Menu Level is {0}, input is {1}", menuLevel, releaseDetails);
        }

        static void DisplayMenu()
        {
            string menuStr = "Not a string";
            switch (menuLevel)
            {
                case Parameters.ReleaseType.NONE:
                    menuStr = string.Format("Select Release Type:\n" +
                        "1:\t{0}\n" +
                        "2:\t{1}\n" +
                        "3:\t{2}\n"+
                        "Q:\tQuit", Parameters.ReleaseTypeStr[1], Parameters.ReleaseTypeStr[2], Parameters.ReleaseTypeStr[3]);
                    break;
                case Parameters.ReleaseType.PUMPUPDATE:
                    menuStr = "Please Enter Release Date:\t(2018-10-05) or 'b' Go Back";
                    break;
                case Parameters.ReleaseType.DESKTOP:
                case Parameters.ReleaseType.EMBEDDED:
                    string releaseMode = "Current ";
                    if (result == ProcessResults.NEXT2)
                    {
                        releaseMode = "Last ";
                    }
                    menuStr = String.Format("Please Enter {0}Release Version:\t(1.2.3.0) or 'b' Go Back", releaseMode);
                    break;
            }
            Console.WriteLine(menuStr);
        }

        /// <summary>
        /// Return Value: q- go back to main menu; o - keep processing
        /// </summary>
        /// <param name="subMenuInput"></param>
        /// <returns></returns>
        static void ProcessSubInput(string subMenuInput, ref ProcessResults result)
        {
            if (subMenuInput == "b")
            {
                menuLevel = Parameters.ReleaseType.NONE;
                result = ProcessResults.BACK;
                return ; // exit to main menu
            }

            if (subMenuInput.Trim() == string.Empty)
                return;

            switch(menuLevel)
            {
                case Parameters.ReleaseType.PUMPUPDATE:
                    // expecting to have a date time
                    if (ValidateDateTime(subMenuInput))
                        result = ProcessResults.GOOD;
                    break;
                case Parameters.ReleaseType.DESKTOP:
                case Parameters.ReleaseType.EMBEDDED:
                    // expecting to have a version
                    if (ValidateVersion(subMenuInput))
                        if (result == ProcessResults.NEXT2)
                            result = ProcessResults.GOOD;
                        else
                            result = ProcessResults.NEXT2;
                    break;
            }
        }

        static bool ValidateDateTime(string dateTimeStr)
        {
            try
            {
                pumpUpdateReleaseDateTime = Convert.ToDateTime(dateTimeStr);
                return true;
            }
            catch (System.FormatException)
            {
                Console.WriteLine("Unable to convert to dateTime {0}", dateTimeStr);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }
            return false;
        }

        static bool ValidateVersion(string versionStr)
        {
            try
            {
                Version version = new Version(versionStr);
                if (result == ProcessResults.NEXT)
                    currentRelease = version;
                else
                    lastRelease = version;
                return true;
            }
            catch(System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
    }
}
