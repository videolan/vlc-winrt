using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Model
{
    [DataContract]
    public class Feedback
    {
        private string platform = "Windows";
        private string platformVersion = "8.1/10";
        private string appVersion = Strings.AppVersion;

        public string Id { get; set; }

        [DataMember(Name = nameof(Comment))]
        public string Comment { get; set; }

        [DataMember(Name = nameof(Summary))]
        public string Summary { get; set; }

        [DataMember(Name = nameof(BackendLog))]
        public string BackendLog { get; set; }

        [DataMember(Name = nameof(FrontendLog))]
        public string FrontendLog { get; set; }

        [DataMember(Name = nameof(Platform))]
        public string Platform
        {
            get { return platform; }
            set { platform = value; }
        }

        [DataMember(Name = nameof(PlatformVersion))]
        public string PlatformVersion
        {
            get { return platformVersion; }
            set { platformVersion = value; }
        }

        [DataMember(Name = nameof(AppVersion))]
        public string AppVersion
        {
            get { return appVersion; }
            set { appVersion = value; }
        }
    }
}
