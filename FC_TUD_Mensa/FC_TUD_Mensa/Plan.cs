using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MensaExtractor
{
    class Plan
    {
        private ConcurrentDictionary<DateTime, MensaDay> MensaDay = new ConcurrentDictionary<DateTime, MensaDay>();

        private List<string> _w0 = new List<string>{
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d1.html?print=1" ,
                                    //Montag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d2.html?print=1",
                                    //Dienstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d3.html?print=1",
                                    //Mittwoch
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d4.html?print=1",
                                    //Donnerstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d5.html?print=1",
                                    //Freitag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d6.html?print=1",
                                    //Samstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d0.html?print=1"};
                                    //Sonntag

        private List<string> _w1 = new List<string> {
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d1.html?print=1",
                                    //nächste Woche Montag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d2.html?print=1",
                                    //nächste Woche Dienstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d3.html?print=1",
                                    //nächste Woche Mittwoch
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d4.html?print=1",
                                    //nächste Woche Donnerstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d5.html?print=1",
                                    //nächste Woche Freitag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d6.html?print=1",
                                    //nächste Woche Samstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d0.html?print=1"};
        //nächste Woche Sonntag
        
        private List<string> _w2 = new List<string>{
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w2-d1.html?print=1",
                                    //nächste Woche Montag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w2-d2.html?print=1",
                                    //nächste Woche Dienstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w2-d3.html?print=1",
                                    //nächste Woche Mittwoch
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w2-d4.html?print=1",
                                    //nächste Woche Donnerstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w2-d5.html?print=1",
                                    //nächste Woche Freitag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w2-d6.html?print=1",
                                    //nächste Woche Samstag
                                    "https://www.studentenwerk-dresden.de/mensen/speiseplan/w2-d0.html?print=1"};
                                    //nächste Woche Sonntag

        private const string today = "https://www.studentenwerk-dresden.de/mensen/speiseplan/?print=1";
        private const string tomorrow = "https://www.studentenwerk-dresden.de/mensen/speiseplan/morgen.html?print=1";

        public Plan()
        {
            //Download();
        }

        public void Download(bool debug = false, bool thisWeek = true, bool nextWeek = true, bool lastWeek = true)
        {
            MensaDay.Clear();
            List<string> links = new List<string>();

            if (thisWeek)
            {
                links.AddRange(_w0);
            }

            if (nextWeek)
            {
                links.AddRange(_w1);
            }

            if (lastWeek)
            {
                links.AddRange(_w2);
            }

            if (debug)
            {
                foreach (string currentLink in links)
                {
                    if (currentLink != null)
                    {
                        MensaDay newDay = GetMensaDay(new Uri(currentLink));
                        if (!MensaDay.Keys.Contains<DateTime>(newDay.Day) && newDay.HasOfferings)
                        {
                            MensaDay.TryAdd(newDay.Day, newDay);
                        }
                    }
                }
                return;
            }

            if (thisWeek)
            {
                links.AddRange(_w0);
            }

            if (nextWeek)
            {
                links.AddRange(_w1);
            }

            if (lastWeek)
            {
                links.AddRange(_w2);
            }


            Parallel.ForEach<string>(links, (currentLink) =>
            {
                if (currentLink != null)
                {
                    MensaDay newDay = GetMensaDay(new Uri(currentLink));
                    if (!MensaDay.Keys.Contains<DateTime>(newDay.Day) && newDay.HasOfferings)
                    {
                        MensaDay.TryAdd(newDay.Day, newDay);
                    }
                }
            });
        }

        private MensaDay GetMensaDay(Uri link, bool debug = false)
        {
            HttpClient client = new HttpClient();
            var response = client.GetByteArrayAsync(link).Result;
            string help = System.Text.Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            help = System.Net.WebUtility.HtmlDecode(help);
            help = help.Replace("\n", "");
            help = help.Replace("\\t", "");

            return new MensaDay(help, link, debug);
        }

        private async Task<MensaDay> GetMensaDayAsync(Uri link, bool debug = false)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(link);
            string help = System.Text.Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            help = System.Net.WebUtility.HtmlDecode(help);
            help = help.Replace("\n", "");
            help = help.Replace("\\t", "");

            return new MensaDay(help, link, debug);
        }

        public MensaDay GetByDay(DateTime Day)
        {
            MensaDay.TryGetValue(Day, out MensaDay askedDay);
            return askedDay;
        }

        public Dictionary<DateTime,MensaDay> Days { get => MensaDay.ToDictionary(kvp => kvp.Key,
                                                          kvp => kvp.Value); }


    }
}
