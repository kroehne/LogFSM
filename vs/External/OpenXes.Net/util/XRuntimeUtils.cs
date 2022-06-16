using System;
using System.IO;

namespace OpenXesNet.util
{
    /// <summary>
    /// This class provides runtime utilities for library components. Its main 
    /// purpose is to identify the host OS, and to locate a standard support folder 
    /// location on each platform.
    /// </summary>
    public static class XRuntimeUtils
    {
        /// <summary>
        /// Version string for the supported XES standard.
        /// </summary>
        public static readonly String XES_VERSION = "2.0";
        /// <summary>
        /// Version string for the OpenXES library implementation.
        /// </summary>
        public static readonly String OPENXES_VERSION = "2.21";
        /// <summary>
        /// Current host platform.
        /// </summary>
        public static OS? currentOs;

        /// <summary>
        /// Determines the current host platform.
        /// </summary>
        /// <returns>The os.</returns>
        public static OS? DetermineOS()
        {
            if (currentOs == null)
            {
                String osString = System.Environment.GetEnvironmentVariable("os.name").Trim().ToLower();
                if (osString.StartsWith("windows", StringComparison.Ordinal))
                    currentOs = OS.WIN32;
                else if (osString.StartsWith("mac os x", StringComparison.Ordinal))
                    currentOs = OS.MACOSX;
                else if (osString.StartsWith("mac os", StringComparison.Ordinal))
                    currentOs = OS.MACOSCLASSIC;
                else if (osString.StartsWith("risc os", StringComparison.Ordinal))
                    currentOs = OS.RISCOS;
                else if ((osString.IndexOf("linux", StringComparison.Ordinal) > -1) || (osString.IndexOf("debian", StringComparison.Ordinal) > -1)
                        || (osString.IndexOf("redhat", StringComparison.Ordinal) > -1) || (osString.IndexOf("lindows", StringComparison.Ordinal) > -1))
                {
                    currentOs = OS.LINUX;
                }
                else if ((osString.IndexOf("freebsd", StringComparison.Ordinal) > -1)
                         || (osString.IndexOf("openbsd", StringComparison.Ordinal) > -1)
                         || (osString.IndexOf("netbsd", StringComparison.Ordinal) > -1)
                         || (osString.IndexOf("irix", StringComparison.Ordinal) > -1)
                         || (osString.IndexOf("solaris", StringComparison.Ordinal) > -1)
                         || (osString.IndexOf("sunos", StringComparison.Ordinal) > -1)
                         || (osString.IndexOf("hp/ux", StringComparison.Ordinal) > -1)
                         || (osString.IndexOf("risc ix", StringComparison.Ordinal) > -1)
                         || (osString.IndexOf("dg/ux", StringComparison.Ordinal) > -1))
                {
                    currentOs = OS.BSD;
                }
                else if (osString.IndexOf("beos", StringComparison.Ordinal) > -1)
                    currentOs = OS.BEOS;
                else
                {
                    currentOs = OS.UNKNOWN;
                }
            }
            return currentOs;
        }

        /// <summary>
        /// Checks whether the current platform is Windows.
        /// </summary>
        /// <returns><c>true</c>, if running windows was ised, <c>false</c> otherwise.</returns>
        public static bool IsRunningWindows()
        {
            return DetermineOS().Equals(OS.WIN32);
        }

        /// <summary>
        /// Checks whether the current platform is Mac OS X.
        /// </summary>
        /// <returns><c>true</c>, if running mac os x was ised, <c>false</c> otherwise.</returns>
        public static bool IsRunningMacOsX()
        {
            return DetermineOS().Equals(OS.MACOSX);
        }

        /// <summary>
        /// Checks whether the current platform is Linux.
        /// </summary>
        /// <returns><c>true</c>, if running linux was ised, <c>false</c> otherwise.</returns>
        public static bool IsRunningLinux()
        {
            return DetermineOS().Equals(OS.LINUX);
        }

        /// <summary>
        /// Checks whether the current platform is some flavor of Unix.
        /// </summary>
        /// <returns><c>true</c>, if running unix was ised, <c>false</c> otherwise.</returns>
        public static bool IsRunningUnix()
        {
            OS? os = DetermineOS();

            return ((os.Equals(OS.BSD)) || (os.Equals(OS.LINUX)) || (os.Equals(OS.MACOSX)));
        }

        /// <summary>
        /// Retrieves the path of the platform-dependent OpenXES.Net support folder.
        /// </summary>
        /// <returns>The support folder.</returns>
        public static string GetSupportFolder()
        {
            String homedir = System.Environment.GetEnvironmentVariable("user.home");
            String dirName = "OpenXES";
            if (IsRunningWindows())
            {
                return Directory.CreateDirectory(Path.Combine(homedir, dirName)).FullName;
            }
            if (IsRunningMacOsX())
            {
                return Directory.CreateDirectory(Path.Combine(homedir, "/Library/Application Support/", dirName)).FullName;
            }
            return Directory.CreateDirectory(Path.Combine(homedir, "/.", dirName)).FullName;
        }

        /// <summary>
        /// Retrieves the directory file of the platform-dependent OpenXES.Net extension definition file folder.
        /// </summary>
        /// <returns>The extension cache folder.</returns>
        public static string GetExtensionCacheFolder()
        {
            return Directory.CreateDirectory(Path.Combine(GetSupportFolder(), "ExtensionCache")).FullName;
        }

        /// <summary>
        /// Enum for defining host platforms.
        /// </summary>
        public enum OS
        {
            WIN32, MACOSX, MACOSCLASSIC, LINUX, BSD, RISCOS, BEOS, UNKNOWN
        }
    }
}
