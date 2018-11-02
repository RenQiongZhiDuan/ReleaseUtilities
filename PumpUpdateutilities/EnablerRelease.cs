using System;
using System.Collections.Generic;
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
        public EnablerRelease(Version releaseVersion, Version lastReleaseVersion)
        {
            _releaseVersion = releaseVersion;
            _lastReleaseVersion = lastReleaseVersion;
        }

        public void Start()
        {

        }
    }
}
