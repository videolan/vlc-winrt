using VLC.Helpers;
using Windows.System.Profile;

namespace VLC.Utils
{
    /// <summary>
    /// Magic numbers belong here
    /// </summary>
    public static class Numbers
    {
        // Under this number, list should not display SemanticZoom (letters or anything), only a flat list
        public static readonly int SemanticZoomItemCountThreshold = 25;

        // Database Version 
        public static readonly int DbVersion = 9;
        public static bool NeedsToDrop()
        {
            if (ApplicationSettingsHelper.Contains(Strings.AlreadyLaunched, true) && ApplicationSettingsHelper.Contains(Strings.DatabaseVersion, true) && (int)ApplicationSettingsHelper.ReadSettingsValue(Strings.DatabaseVersion, true) == Numbers.DbVersion)
            {
                LogHelper.Log("DB does not need to be dropped.");
                return false;
            }
            else
            {
                LogHelper.Log("DB needs to be dropped.");
                ApplicationSettingsHelper.SaveSettingsValue(Strings.DatabaseVersion, Numbers.DbVersion, true);
                ApplicationSettingsHelper.SaveSettingsValue(Strings.AlreadyLaunched, true, true);
                return true;
            }
        }

        public static double NotAvailableFileItemOpacity = 0.6;
        public static ulong OSVersion
        {
            get
            {
                string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                ulong v = ulong.Parse(sv);
                return (v & 0x00000000FFFF0000L) >> 16;
            }
        }
    }
}