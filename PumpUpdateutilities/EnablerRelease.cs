using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseUtilities
{
    class EnablerRelease
    {
        /*
         * For Desktop:
         *          A:  Get versions from: S:\Releases\Construction\Enabler\Enabler v4.6.0.N
         *          B:  Compare with last release for difference -> out.txt
         *          C:  Get release notes and version from C:\WORK\Enabler\Install\EnablerV4
         *          D:  Compare version for each component -> output.txt
         *          
         *          
         * For Embedded:
         *          A: Get version from S:\Releases\Construction\Embedded\EMB v1.3.2
         *          B: Compare with last release -> out
         *          C: Get notes from C:\WORK\Enabler\Embedded\Installs\EnablerEmbedded\EnablerEMBInstall.htm
         *          D: 
         *              1: Compare notes table version with construction folder -> out.txt
         *              2: Compare component version with construction folder -> out.txt
         * */
        Version _releaseVersion, _lastReleaseVersion;
        Parameters.ReleaseType _releaseType;
        public EnablerRelease(Parameters.ReleaseType releaseType, Version releaseVersion, Version lastReleaseVersion)
        {
            _releaseType = releaseType;
            _releaseVersion = releaseVersion;
            _lastReleaseVersion = lastReleaseVersion;
        }

        public void Start()
        {
            // Get all items from construction folder
            Task<KeyValuePair<string, List<IReleaseItem>>> GetFilesCurReleaseCons = new Task<KeyValuePair<string, List<IReleaseItem>>>(() => GetItemsFromConstructionFolder(_releaseVersion));
            GetFilesCurReleaseCons.Start();

            // Get all items from previous construction released folder
            Task<KeyValuePair<string, List<IReleaseItem>>> GetFilesPreReleaseCons = new Task<KeyValuePair<string, List<IReleaseItem>>>(() => GetItemsFromConstructionFolder(_lastReleaseVersion));
            GetFilesPreReleaseCons.Start();

            Task.WaitAll(GetFilesCurReleaseCons, GetFilesPreReleaseCons);

            // file compare with file
            string filePath = Parameters.GetTempFile(String.Format("{0} {1} vs {2}.txt", Parameters.ReleaseTypeStr[(int)_releaseType], _releaseVersion, _lastReleaseVersion));
            YLog.WriteTo(filePath, true, VersionComparer.CompareVersions(GetFilesPreReleaseCons.Result, GetFilesCurReleaseCons.Result));

            Process.Start(filePath);
        }

        private KeyValuePair<string, List<IReleaseItem>> GetItemsFromConstructionFolder(Version releaeVersion)
        {
            string conPath = Path.Combine(Parameters.ServerEnablerConstructionPath, string.Format("Enabler v{0}.N", releaeVersion.ToString()));
            return DirectoryOperator.GetFileVersionsFromDir(conPath, _releaseType);
        }
    }
}
