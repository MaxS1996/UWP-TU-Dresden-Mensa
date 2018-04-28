using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MensaExtractor
{
    class MensaDay
    {
        private HashSet<Mensa> mensen = new HashSet<Mensa>();
        private DateTime day;
        private Uri address;
        private string title;
        private string source;

        public MensaDay(string source, Uri link, bool debug = false)
        {
            this.address = link;
            this.title = ExtractTitle(source);
            this.day = ParseDateFromTitle(this.title);
            source = CleanSource(source);
            if (debug)
            {
                this.source = source;
            }
            ExtractMensen(source);
        }

        private static string ExtractTitle(string source)
        {
            Regex regTitle = new Regex(@"<title>(.*)</title>");
            string title = regTitle.Match(source).Value;
            title = title.Replace("<title>", "");
            title = title.Replace("</title>", "");

            return title;
        }

        private static DateTime ParseDateFromTitle(string title)
        {
            string[] help = title.Split(',');
            return DateTime.Parse(help[1]);
        }

        public static DateTime ParseDateTimeFromSource(string source)
        {
            return ParseDateFromTitle(ExtractTitle(source));
        }

        private static string CleanSource(string source)
        {
            Regex regPlan = new Regex("<div id=\"spalterechtsnebenmenue\">(.*)</div>");
            return regPlan.Match(source).Value;
        }

        public void ExtractMensen(string cleanSource)
        {
            mensen.Clear();
            Regex regMensa = new Regex("<table class=\"speiseplan\">(.*?)</table>");

            MatchCollection mensaMatches = regMensa.Matches(cleanSource);
            foreach (Match mensaMatch in mensaMatches)
            {
                Mensa mensa = new Mensa(mensaMatch.Value);

                if (mensa.IsOpen)
                {
                    mensen.Add(mensa);
                }
            }
        }

        public string Title { get => title; }
        public DateTime Day { get => day; }
        public HashSet<Mensa> Mensen { get => mensen; }
        public Uri Link { get => address; }
        public bool HasOfferings { get { return !(Mensen.Count == 0); } }


    }
}
