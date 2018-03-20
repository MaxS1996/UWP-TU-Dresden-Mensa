using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace MensaExtractor
{
    class MensaMeal : INotifyPropertyChanged
    {
        private string name;
        private List<string> prices = new List<string>();
        private List<string> labels = new List<string>();
        private Uri link;
        private Uri picture = new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg");
        private bool vital = false;
        private bool evening = false;
        private bool retPicture = false;
        private string source;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public MensaMeal(string source, bool debug=false)
        {
            if (debug)
            {
                this.source = source;
            }
            //GeneratePictureUri();
            Analyze(source);
        }

        
        

        private void Analyze(string source)
        {
            Regex regMealName = new Regex("<td class=\"text\">(.*?)</td>");
            Regex regMealLabels = new Regex("<td class=\"stoffe\">(.*?)</td>");
            Regex regMealLabelSingle = new Regex("<img alt=\"(.*?)\"");
            Regex regMealPrice = new Regex("<td class=\"preise\">(.*?)</a></td>");
            Regex regMealLink = new Regex("\"(.*?)\"");
            Regex regMealVital = new Regex("<div class=\"mensavitalicon\">(.*?)</div>");
            Regex regMealEvening = new Regex("Abendangebot: ");
            Regex regHTMLTags = new Regex("<(.*?)>");

            //Name + Fit +Evening
            string nameHelper = regMealName.Match(source).Value;
            if (regMealVital.IsMatch(nameHelper))
            {
                this.vital = true;
                nameHelper = regMealVital.Replace(nameHelper, "Fit&Vital: ");
            }

            if (regMealEvening.IsMatch(nameHelper))
            {
                this.evening = true;
            }
            nameHelper = regHTMLTags.Replace(nameHelper, "");
            nameHelper = nameHelper.Replace("<td class=\"text\">", "");
            name = nameHelper.Replace("</td>", "");


            //Price + Link
            string mealPrices = regMealPrice.Match(source).Value;
            mealPrices = mealPrices.Replace("<td class=\"preise\">", "");

            string mealLink = regMealLink.Match(mealPrices).Value;
            mealLink = "https://www.studentenwerk-dresden.de/mensen/speiseplan/" + mealLink;
            mealLink = mealLink.Replace(" ", "");
            mealLink = mealLink.Replace("\"", "");
            this.link = new Uri(mealLink);

            mealPrices = mealPrices.Replace("</a></td>", "");
            mealPrices = regHTMLTags.Replace(mealPrices, "");
            string[] helpPrices = mealPrices.Replace(" ", "").Split('/');
            foreach(string price in helpPrices)
            {
                if(!price.Replace(" ", "").Equals(String.Empty))
                {
                    this.prices.Add(price);
                }
            }

            //Labels
            string mealLabels = regMealLabels.Match(source).Value;
            mealLabels = mealLabels.Replace("<td class=\"stoffe\">", "");
            mealLabels = mealLabels.Replace("</td>", "");
            MatchCollection labelMatches = regMealLabelSingle.Matches(mealLabels);
            foreach (Match labelMatch in labelMatches)
            {
                this.labels.Add(labelMatch.Value.Replace("<img alt=\"", "").Replace("\"", ""));
            }
        }

        public bool GetPictureUri()
        {
            if (retPicture)
            {
                return true;
            }
            else
            {
                string source = String.Empty;
                HttpClient http = new HttpClient();

                try
                {
                    //var response = await http.GetByteArrayAsync(this.link);
                    var response = http.GetByteArrayAsync(this.link).Result;
                    source = System.Text.Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                    source = System.Net.WebUtility.HtmlDecode(source);

                    if(source != null)
                    {
                        Regex div = new Regex("<div id=\"essenbild\">(.*?)</div>");
                        Regex link = new Regex("href=\"(.*?)\"");

                        source = div.Match(source).Value;
                        source = link.Match(source).Value;
                        // "href=\"//bilderspeiseplan.studentenwerk-dresden.de/m35/201709/191469.jpg?date=201709281541\""
                        if (source.Length > 0)
                        {
                            source = source.Replace("href=\"//", "");
                            source = source.Replace("\"", "");
                            source = "http://" + source;
                        }
                        this.retPicture = true;
                        this.picture =  new Uri(source);
                        NotifyPropertyChanged();
                        return true;
                    }
                }catch(Exception e)
                {
                    return false;
                }
                return false;
            }
        }

        public async Task<bool> GetPictureUriAsync()
        {
            if (retPicture)
            {
                return true;
            }
            else
            {
                string source = String.Empty;
                HttpClient http = new HttpClient();

                try
                {
                    var response = await http.GetByteArrayAsync(this.link);
                    //var response = http.GetByteArrayAsync(this.link).Result;
                    source = System.Text.Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                    source = System.Net.WebUtility.HtmlDecode(source);

                    if (source != null)
                    {
                        Regex div = new Regex("<div id=\"essenbild\">(.*?)</div>");
                        Regex link = new Regex("href=\"(.*?)\"");

                        source = div.Match(source).Value;
                        source = link.Match(source).Value;
                        // "href=\"//bilderspeiseplan.studentenwerk-dresden.de/m35/201709/191469.jpg?date=201709281541\""
                        if (source.Length > 0)
                        {
                            source = source.Replace("href=\"//", "");
                            source = source.Replace("\"", "");
                            source = "http://" + source;
                        }
                        this.retPicture = true;
                        this.picture = new Uri(source);
                        NotifyPropertyChanged();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
                return false;
            }
        }

        public string Name { get => name; }
        public string StudentPrice
        {
            get
            {
                if(prices.Count > 0)
                {
                    return prices[0];
                }
                else
                {
                    return "";
                }
                
            }
        }
        public string OtherPrice { get
            {

                switch (this.Prices.Count())
                {
                    case 1:
                        return prices[0];
                        //break;
                    case 2:
                        return prices[1];
                        //break;
                    default:
                        return "";
                        //break;
                }
            }
        }
        public string[] Prices { get => prices.ToArray(); }
        public Uri Link { get => link; }
        public bool PictureRetrieved { get => retPicture; }
        public Uri Picture { get
            {
                if (!retPicture)
                {
                    bool test = GetPictureUri();
                }
                return this.picture;
            } }
        public bool Vital { get => vital; }
        public bool Evening { get => evening; }
        public List<string> Labels { get => labels; }
        public string Entry
        {
            get
            {

                string entry = this.Name + " ";

                switch (this.prices.Count())
                {
                    case 1:
                        entry = entry + " | " + this.prices[0];
                        break;
                    case 2:
                        entry = entry + " | " + this.prices[0] + "/"+this.prices[1];
                        break;
                    default:
                        break;
                }
                return entry;
            }
        }
    }
}
