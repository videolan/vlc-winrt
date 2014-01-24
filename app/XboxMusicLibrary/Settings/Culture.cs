using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XboxMusicLibrary.Settings
{
    public class Culture
    {
        public string Language { get; set; }
        public string Country { get; set; }

        public Culture(string language, string country)
        {
            this.Language = language;
            this.Country = country;
        }
    }
}
