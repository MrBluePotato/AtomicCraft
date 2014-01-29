// Copyright 2009-2014 Matvei Stefarov <me@matvei.org>
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using fCraft.Events;
using JetBrains.Annotations;
using System.IO;
using System.Text.RegularExpressions;

namespace fCraft
{
    /// <summary> Checks for updates, and keeps track of current version/revision. </summary>
    public static class Updater
    {

        public static readonly ReleaseInfo CurrentRelease = new ReleaseInfo(
            301,
            1,
            ReleaseFlags.Bugfix
#if DEBUG
 | ReleaseFlags.Dev
#endif
);

        public static string UserAgent
        {
            get { return "AtomicCraft " + CurrentRelease.VersionString; }
        }

        public const string LatestStable = "0.301_r1";
        public static string UpdateUrl { get; set; }
        public static int WebVersion;
        public static string WebVersionFullString;
        public static string DownloadLocation;
        public static string PublicUpdaterLocation;
        public static string DevUpdaterLocation;
        public static string Changelog;
        public static string DevVersion;
        public static string CurrentDevVersion;

        public static bool UpdateCheck()
        {
            try
            {
                ReleaseMode mode = ConfigKey.ReleaseMode.GetEnum<ReleaseMode>();
                using (WebClient client = new WebClient())
                {
                    if (mode == ReleaseMode.Dev)
                    {
                        using (Stream stream = client.OpenRead("http://build.atomiccraft.net/guestAuth/app/rest/buildTypes/id:bt34/builds/status:SUCCESS/number"))
                        {
                            stream.ReadTimeout = 1000;
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                DevVersion = reader.ReadLine();
                            }
                        }
                        if (!File.Exists("dev.txt"))
                        {
                            File.WriteAllText("dev.txt", DevVersion);

                        }
                        using (StreamReader reader = new StreamReader("dev.txt"))
                        {
                            if (File.Exists("dev.txt"))
                            {
                                CurrentDevVersion = reader.ReadLine();
                            }
                        }
                        if (CurrentDevVersion == DevVersion)
                        {
                            return false;
                        }
                        if (CurrentDevVersion != DevVersion)
                        {
                            DevUpdaterLocation = ("http://build.AtomicCraft.net/guestAuth/repository/download/bt34/.lastSuccessful/AtomicCraft+-+Build+" + Updater.DevVersion + ".zip!UpdateInstaller.exe");
                            File.Delete("dev.txt");
                            File.WriteAllText("dev.txt", DevVersion);
                            return true;
                        }
                    }

                    else if (mode == ReleaseMode.Public)
                    {
                        using (Stream stream = client.OpenRead("http://dl.atomiccraft.net/AtomicCraft/public/update.txt"))
                        {
                            stream.ReadTimeout = 1000;
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string s = reader.ReadLine();
                                if (!int.TryParse(s.Replace(".", ""), out WebVersion))
                                {
                                    Logger.Log(LogType.Warning, "Could not parse version value in updater ({0})", s);
                                    return false;
                                }
                                WebVersionFullString = reader.ReadLine();
                                DownloadLocation = reader.ReadLine();
                                PublicUpdaterLocation = reader.ReadLine();
                                Changelog = reader.ReadToEnd();
                            }
                        }
                    }

                    if (WebVersion != 0 && DownloadLocation != null && PublicUpdaterLocation != null)
                    {
                        if (WebVersion > Updater.CurrentRelease.Version)
                        {
                            Logger.Log(LogType.Warning, "An update of AtomicCraft is available, you can get it at: " + DownloadLocation);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Logger.Log(LogType.SystemActivity, "Failed: " + e);
                return false;
            }
        }

        public static bool RunAtShutdown { get; set; }


        #region Events

        /// <summary> Occurs when fCraft is about to check for updates (cancellable).
        /// The update Url may be overridden. </summary>
        public static event EventHandler<CheckingForUpdatesEventArgs> CheckingForUpdates;


        /// <summary> Occurs when fCraft has just checked for updates. </summary>
        public static event EventHandler<CheckedForUpdatesEventArgs> CheckedForUpdates;


        static bool RaiseCheckingForUpdatesEvent(ref string updateUrl)
        {
            var h = CheckingForUpdates;
            if (h == null) return false;
            var e = new CheckingForUpdatesEventArgs(updateUrl);
            h(null, e);
            updateUrl = e.Url;
            return e.Cancel;
        }


        static void RaiseCheckedForUpdatesEvent(string url, UpdaterResult result)
        {
            var h = CheckedForUpdates;
            if (h != null) h(null, new CheckedForUpdatesEventArgs(url, result));
        }

        #endregion
    }


    public sealed class UpdaterResult
    {
        public static UpdaterResult NoUpdate
        {
            get
            {
                return new UpdaterResult(false, null, new ReleaseInfo[0]);
            }
        }
        internal UpdaterResult(bool updateAvailable, Uri downloadUri, IEnumerable<ReleaseInfo> releases)
        {
            UpdateAvailable = updateAvailable;
            DownloadUri = downloadUri;
            History = releases.OrderByDescending(r => r.Revision).ToArray();
            LatestRelease = releases.FirstOrDefault();
        }
        public bool UpdateAvailable { get; private set; }
        public Uri DownloadUri { get; private set; }
        public ReleaseInfo[] History { get; private set; }
        public ReleaseInfo LatestRelease { get; private set; }
    }


    public sealed class ReleaseInfo
    {
        internal ReleaseInfo(int version, int revision, ReleaseFlags releaseType)
        {
            Version = version;
            Revision = revision;
            Flags = releaseType;
        }

        public ReleaseFlags Flags { get; private set; }

        public string FlagsString { get { return ReleaseFlagsToString(Flags); } }

        public string[] FlagsList { get { return ReleaseFlagsToStringArray(Flags); } }

        public int Version { get; private set; }

        public int Revision { get; private set; }

        public DateTime Date { get; private set; }

        public TimeSpan Age
        {
            get
            {
                return DateTime.UtcNow.Subtract(Date);
            }
        }

        public string VersionString
        {
            get
            {
                string formatString = "{0:0.000}_r{1}";
                if (IsFlagged(ReleaseFlags.Dev))
                {
                    formatString += "_dev";
                }
                if (IsFlagged(ReleaseFlags.Unstable))
                {
                    formatString += "_u";
                }
                return String.Format(CultureInfo.InvariantCulture, formatString,
                                      Decimal.Divide(Version, 1000),
                                      Revision);
            }
        }

        public string Summary { get; private set; }

        public string[] ChangeLog { get; private set; }

        public static ReleaseFlags StringToReleaseFlags([NotNull] string str)
        {
            if (str == null) throw new ArgumentNullException("str");
            ReleaseFlags flags = ReleaseFlags.None;
            for (int i = 0; i < str.Length; i++)
            {
                switch (Char.ToUpper(str[i]))
                {
                    case 'A':
                        flags |= ReleaseFlags.APIChange;
                        break;
                    case 'B':
                        flags |= ReleaseFlags.Bugfix;
                        break;
                    case 'C':
                        flags |= ReleaseFlags.ConfigFormatChange;
                        break;
                    case 'D':
                        flags |= ReleaseFlags.Dev;
                        break;
                    case 'F':
                        flags |= ReleaseFlags.Feature;
                        break;
                    case 'M':
                        flags |= ReleaseFlags.MapFormatChange;
                        break;
                    case 'P':
                        flags |= ReleaseFlags.PlayerDBFormatChange;
                        break;
                    case 'S':
                        flags |= ReleaseFlags.Security;
                        break;
                    case 'U':
                        flags |= ReleaseFlags.Unstable;
                        break;
                    case 'O':
                        flags |= ReleaseFlags.Optimized;
                        break;
                }
            }
            return flags;
        }

        public static string ReleaseFlagsToString(ReleaseFlags flags)
        {
            StringBuilder sb = new StringBuilder();
            if ((flags & ReleaseFlags.APIChange) == ReleaseFlags.APIChange) sb.Append('A');
            if ((flags & ReleaseFlags.Bugfix) == ReleaseFlags.Bugfix) sb.Append('B');
            if ((flags & ReleaseFlags.ConfigFormatChange) == ReleaseFlags.ConfigFormatChange) sb.Append('C');
            if ((flags & ReleaseFlags.Dev) == ReleaseFlags.Dev) sb.Append('D');
            if ((flags & ReleaseFlags.Feature) == ReleaseFlags.Feature) sb.Append('F');
            if ((flags & ReleaseFlags.MapFormatChange) == ReleaseFlags.MapFormatChange) sb.Append('M');
            if ((flags & ReleaseFlags.PlayerDBFormatChange) == ReleaseFlags.PlayerDBFormatChange) sb.Append('P');
            if ((flags & ReleaseFlags.Security) == ReleaseFlags.Security) sb.Append('S');
            if ((flags & ReleaseFlags.Unstable) == ReleaseFlags.Unstable) sb.Append('U');
            if ((flags & ReleaseFlags.Optimized) == ReleaseFlags.Optimized) sb.Append('O');
            return sb.ToString();
        }

        public static string[] ReleaseFlagsToStringArray(ReleaseFlags flags)
        {
            List<string> list = new List<string>();
            if ((flags & ReleaseFlags.APIChange) == ReleaseFlags.APIChange) list.Add("API Changes");
            if ((flags & ReleaseFlags.Bugfix) == ReleaseFlags.Bugfix) list.Add("Fixes");
            if ((flags & ReleaseFlags.ConfigFormatChange) == ReleaseFlags.ConfigFormatChange) list.Add("Config Changes");
            if ((flags & ReleaseFlags.Dev) == ReleaseFlags.Dev) list.Add("Developer");
            if ((flags & ReleaseFlags.Feature) == ReleaseFlags.Feature) list.Add("New Features");
            if ((flags & ReleaseFlags.MapFormatChange) == ReleaseFlags.MapFormatChange) list.Add("Map Format Changes");
            if ((flags & ReleaseFlags.PlayerDBFormatChange) == ReleaseFlags.PlayerDBFormatChange) list.Add("PlayerDB Changes");
            if ((flags & ReleaseFlags.Security) == ReleaseFlags.Security) list.Add("Security Patch");
            if ((flags & ReleaseFlags.Unstable) == ReleaseFlags.Unstable) list.Add("Unstable");
            if ((flags & ReleaseFlags.Optimized) == ReleaseFlags.Optimized) list.Add("Optimized");
            return list.ToArray();
        }

        public bool IsFlagged(ReleaseFlags flag)
        {
            return (Flags & flag) == flag;
        }
    }


    #region Enums

    /// <summary> Updater behavior. </summary>
    public enum UpdaterMode
    {
        /// <summary> Does not check for updates. </summary>
        Disabled,

        /// <summary> Checks for updates and notifies of availability (in console/log). </summary>
        Notify,

        /// <summary> Checks for updates, downloads them if available, and prompts to install.
        /// Behavior is frontend-specific: in ServerGUI, a dialog is shown with the list of changes and
        /// options to update immediately or next time. In ServerCLI, asks to type in 'y' to confirm updating
        /// or press any other key to skip. '''Note: Requires user interaction
        /// (if you restart the server remotely while unattended, it may get stuck on this dialog).''' </summary>
        Prompt,

        /// <summary> Checks for updates, automatically downloads and installs the updates, and restarts the server. </summary>
        Auto,
    }

    /// <summary> Release Mode. </summary>
    public enum ReleaseMode
    {
        /// <summary> Releases that are expected to have bugs, and have not been thorughly tested. </summary>
        Dev,

        /// <summary> Releases that have undergone bug testing and are intended for the general public. </summary>
        Public,
    }


    /// <summary> A list of release flags/attributes.
    /// Use binary flag logic (value & flag == flag) or Release.IsFlagged() to test for flags. </summary>
    [Flags]
    public enum ReleaseFlags
    {
        None = 0,

        /// <summary> The API was notably changed in this release. </summary>
        APIChange = 1,

        /// <summary> Bugs were fixed in this release. </summary>
        Bugfix = 2,

        /// <summary> Config.xml format was changed (and version was incremented) in this release. </summary>
        ConfigFormatChange = 4,

        /// <summary> This is a developer-only release, not to be used on live servers.
        /// Untested/undertested releases are often marked as such. </summary>
        Dev = 8,

        /// <summary> A notable new feature was added in this release. </summary>
        Feature = 16,

        /// <summary> The map format was changed in this release (rare). </summary>
        MapFormatChange = 32,

        /// <summary> The PlayerDB format was changed in this release. </summary>
        PlayerDBFormatChange = 64,

        /// <summary> A security issue was addressed in this release. </summary>
        Security = 128,

        /// <summary> There are known or likely stability issues in this release. </summary>
        Unstable = 256,

        /// <summary> This release contains notable optimizations. </summary>
        Optimized = 512
    }

    #endregion
}


namespace fCraft.Events
{
    public sealed class CheckingForUpdatesEventArgs : EventArgs, ICancellableEvent
    {
        internal CheckingForUpdatesEventArgs(string url)
        {
            Url = url;
        }

        public string Url { get; set; }
        public bool Cancel { get; set; }
    }


    public sealed class CheckedForUpdatesEventArgs : EventArgs
    {
        internal CheckedForUpdatesEventArgs(string url, UpdaterResult result)
        {
            Url = url;
            Result = result;
        }

        public string Url { get; private set; }
        public UpdaterResult Result { get; private set; }
    }
}