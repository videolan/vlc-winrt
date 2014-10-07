using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace VLC_WINRT_APP.Helpers
{
    public static class TitleDecrapifier
    {
        public static string Decrapify(string title)
        {
            string[] ignoredWords = null;
            ignoredWords = new[]
            {
                @"xvid",
                @"h264",
                @"dvd",
                @"rip",
                @"divx",
                @"[fr]",
                @"720p",
                @"1080i",
                @"1080p",
                @"x264",
                @"hdtv",
                @"aac",
                @"bluray"
            };

            title = ignoredWords.Aggregate(title, (current, word) => current.Replace(word, ""));
            title = title.Replace(".", " ");
            title = title.Replace("_", " ");
            title = title.Replace("+", " ");
            title = title.Replace("-", " ");
            title = title.Replace("  ", " ");

            if (title.Length > 2)
            {
                try
                {
                    if (title.ElementAt(0) == 0x20)
                    {
                        title = title.Remove(0, 1);
                    }
                }
                catch
                {

                }
            }
            return title;
        }

        public static bool isTvShowEpisodeTitle(string title)
        {
            title = title.ToLower();
            for (int i = 0; i < title.Length - 5; i++)
            {
                if (title[i] == 's' &&
                    isDigit(title[i + 1]) &&
                    isDigit(title[i + 2]) &&
                    title[i + 3] == 'e' &&
                    isDigit(title[i + 4]) &&
                    isDigit(title[i + 5]))
                    return true;
            }
            return false;
        }

        public static Dictionary<string, string> tvShowEpisodeInfoFromString(string title)
        {
            if (string.IsNullOrEmpty(title)) return null;
            title = title.ToLower();
            bool successfulSearch = false;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            int stringLength = title.Length;

            if (stringLength < 6) return null;

            // Search for s01e10.
            for (int i = 0; i < stringLength - 5; i++)
            {
                if (title.ElementAt(i) == 's' &&
                    isDigit(title.ElementAt(i + 1)) &&
                    isDigit(title.ElementAt(i + 2)) &&
                    title.ElementAt(i + 3) == 'e' &&
                    isDigit(title.ElementAt(i + 4)) &&
                    isDigit(title.ElementAt(i + 5)))
                {
                    string season = title.ElementAt(i + 1).ToString() + title.ElementAt(i + 2).ToString();
                    string episode;
                    if (isDigit(title.ElementAt(i + 6)))
                        episode = title.ElementAt(i + 4).ToString() + title.ElementAt(i + 5).ToString() + title.ElementAt(i + 6).ToString();
                    else
                        episode = title.ElementAt(i + 4).ToString() + title.ElementAt(i + 5).ToString();

                    string tvShowName = i > 0 ? title.Substring(0, i) : "Untitled show";
                    if (tvShowName != null)
                    {
                        tvShowName = CapitalizedString(Decrapify(tvShowName));
                    }

                    string episodeName = stringLength > i + 4 ? title.Substring(0, i + 6) : null;
                    if (episodeName != null)
                    {
                        episodeName = Decrapify(episodeName);
                    }

                    if (season != null)
                    {
                        dictionary["season"] = season;
                    }
                    if (episode != null)
                    {
                        dictionary["episode"] = episode;
                    }
                    if (tvShowName != null && !string.IsNullOrEmpty(tvShowName))
                    {
                        dictionary["tvShowName"] = tvShowName;
                    }
                    if (episodeName != null && !string.IsNullOrEmpty(episodeName))
                    {
                        dictionary["tvEpisodeName"] = CapitalizedString(episodeName);
                    }
                    successfulSearch = true;
                }
            }

            // search for 0x00
            if (!successfulSearch)
            {
                for (int i = 0; i < stringLength - 4; i++)
                {
                    if (isDigit(title.ElementAt(i)) &&
                        title.ElementAt(i + 1) == 'x' &&
                        isDigit(title.ElementAt(i + 2)) &&
                        isDigit(title.ElementAt(i + 3)))
                    {
                        string season = title.ElementAt(i).ToString();
                        string episode = title.ElementAt(i + 2).ToString() + title.ElementAt(i + 3).ToString();

                        string tvShowName = i > 0 ? title.Substring(0, i) : "Untitled show";
                        if (tvShowName != null)
                        {
                            tvShowName = CapitalizedString(Decrapify(tvShowName));
                        }

                        string episodeName = stringLength > i + 4 ? title.Substring(0, i + 4) : null;
                        if (episodeName != null)
                        {
                            episodeName = Decrapify(episodeName);
                        }

                        if (season != null)
                        {
                            dictionary["season"] = season;
                        }
                        if (episode != null)
                        {
                            dictionary["episode"] = episode;
                        }
                        if (tvShowName != null && !string.IsNullOrEmpty(tvShowName))
                        {
                            dictionary["tvShowName"] = tvShowName;
                        }
                        if (episodeName != null && !string.IsNullOrEmpty(episodeName))
                        {
                            dictionary["tvEpisodeName"] = CapitalizedString(episodeName);
                        }
                        successfulSearch = true;
                    }
                }
            }
            if (successfulSearch)
                return dictionary;
            return null;
        }

        static bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static string CapitalizedString(string input)
        {
            string[] arr = input.Split(' ');
            string result = string.Empty;
            foreach (string s in arr)
            {
                if(s.Length > 1)
                    result = result + (s.First().ToString().ToUpper() + s.Substring(1) + " ");
            }
            return result.Trim();
        }

    }
}
