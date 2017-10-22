using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

namespace MensaApp2
{
    class Extractor
    {
        public delegate void EventDelegate();
        public event EventDelegate MyEvent;
        public void OnEvent()
        {
            // Prüft ob das Event überhaupt einen Abonnenten hat.
            if (MyEvent != null)
                MyEvent();
        }


        /// <summary>
        /// the content of the target website
        /// </summary>
        protected string source;

        /// <summary>
        /// the title of this page
        /// </summary>
        protected string title;

        /// <summary>
        /// the url from where you want to get your information
        /// </summary>
        protected Uri url;

        /// <summary>
        /// Collection of the Mensen
        /// </summary>
        protected HashSet<Mensa> mensen;

        protected bool load;



        /// <summary>
        /// creates a new Extractor object an sets its desired target uri
        /// </summary>
        /// <param name="link"></param>
        public Extractor(Uri link)
        {
            this.url = link;
            this.source = "";
            this.load = false;
            this.mensen = new HashSet<Mensa>();
        }
        /// <summary>
        /// DEPRECATED
        /// just for debugging! DO NOT USE
        /// </summary>
        /// <param name="link"></param>
        public Extractor(string link)
        {
            this.url = new Uri(link);
            this.load = false;
            this.source = "";
            this.mensen = new HashSet<Mensa>();
        }


        /// <summary>
        /// async void DownloadPage gets the HTML file
        /// </summary>
        public async Task DownloadPage()
        {
            this.source = "";
            this.load = false;
            this.mensen = new HashSet<Mensa>();

            string help;
            HttpClient http = new HttpClient();

            var response = await http.GetByteArrayAsync(this.url);
            help = System.Text.Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = System.Net.WebUtility.HtmlDecode(help);
            load = true;
        }

        /// <summary>
        /// gets the Mensen and the Meals from the HTML document
        /// </summary>
        public void Extract()
        {
            source = source.Replace("\n", "");
            Regex regPlan = new Regex("<div id=\"spalterechtsnebenmenue\">(.*)</div>");
            Regex regH1 = new Regex("<div id=\"spalterechtsnebenmenue\">(.*?)<h1>(.*?)</h1>");
            Regex regTitle = new Regex(@"<title>(.*)</title>");

            //set the title of this extraction
            this.title = regTitle.Match(source).Value;
            this.title = this.title.Replace("<title>", "");
            this.title = this.title.Replace("</title>", "");

            //gets the important part of the website
            string food = regPlan.Match(source).Value;
            food = regH1.Replace(food, "");
            food = food.Replace("\\t", "");

            CreateMensen(food);
        }

        protected static bool IsMensa(string text)
        {
            Regex regMensa = new Regex("<table class=\"speiseplan\">(.*?)</table>");
            return regMensa.IsMatch(text);
        }

        protected static bool IsMeal(string text)
        {
            Regex regMeal = new Regex("<tr class=\"(.*?)row\">(.*?)</tr>");
            return regMeal.IsMatch(text);
        }


        protected void CreateMensen(string food)
        {
            while (IsMensa(food))
            {
                // find the corresponding part of the HTML document for a mensa
                Regex regMensa = new Regex("<table class=\"speiseplan\">(.*?)</table>");
                string mensaText = regMensa.Match(food).Value;
                food = food.Replace(mensaText, "");

                // search for the name of the mensa
                string mensaName = null;
                Regex regMensaName = new Regex("<th class=\"text\">(.*?)</th><th class=\"stoffe\">Infos</th>");
                mensaName = regMensaName.Match(mensaText).Value;
                mensaName = mensaName.Replace("<th class=\"text\">", "");
                mensaName = mensaName.Replace("</th><th class=\"stoffe\">Infos</th>", "");
                mensaName = mensaName.Replace("Angebote ", "");

                //don't go on, if you have no name!
                if (mensaName.Equals(String.Empty) || mensaName == null)
                {
                    break;
                }

                //create the Mensa, set its name
                Mensa newMensa = new Mensa(mensaName);

                while (IsMeal(mensaText))
                {
                    mensaText = CreateMeal(newMensa, mensaText);
                }

                if (newMensa.Meals.Count > 0)
                {
                    mensen.Add(newMensa);
                }
            }
        }

        protected string CreateMeal(Mensa mensa, string mensaText)
        {
            Regex regMeal = new Regex("<tr class=\"(.*?)row\">(.*?)</tr>");
            Regex regMealName = new Regex("<td class=\"text\">(.*?)</td>");
            Regex regMealLabels = new Regex("<td class=\"stoffe\">(.*?)</td>");
            Regex regMealLabelSingle = new Regex("<img alt=\"(.*?)\"");
            Regex regMealPrice = new Regex("<td class=\"preise\">(.*?)</a></td>");
            Regex regMealLink = new Regex("\"(.*?)\"");
            Regex regMealVital = new Regex("<div class=\"mensavitalicon\">(.*?)</div>");
            Regex regMealEvening = new Regex("Abendangebot: ");
            Regex regHTMLTags = new Regex("<(.*?)>");

            //extract the meals name and vital boolean
            bool vital = false;
            bool evening = false;
            string mealText = regMeal.Match(mensaText).Value;
            string mealName = regMealName.Match(mealText).Value;
            if (regMealVital.IsMatch(mealName))
            {
                vital = true;
                mealName = regMealVital.Replace(mealName, "Fit&Vital: ");
            }

            if (regMealEvening.IsMatch(mealName))
            {
                evening = true;
            }
            mealName = regHTMLTags.Replace(mealName, "");
            mealName = mealName.Replace("<td class=\"text\">", "");
            mealName = mealName.Replace("</td>", "");

            //extract prices and the mealLink
            string mealPrices = regMealPrice.Match(mealText).Value;
            mealPrices = mealPrices.Replace("<td class=\"preise\">", "");

            string mealLink = regMealLink.Match(mealPrices).Value;
            mealLink = "https://www.studentenwerk-dresden.de/mensen/speiseplan/" + mealLink;
            mealLink = mealLink.Replace(" ", "");
            mealLink = mealLink.Replace("\"", "");

            mealPrices = mealPrices.Replace("</a></td>", "");
            mealPrices = regHTMLTags.Replace(mealPrices, "");

            //generate Meal
            Meal newMeal = new Meal(mealName, mealPrices, new Uri(mealLink));
            newMeal.Vital = vital;
            newMeal.Evening = evening;
            newMeal.GeneratePictureUri();

            //extract labels
            string mealLabels = regMealLabels.Match(mealText).Value;
            mealLabels = mealLabels.Replace("<td class=\"stoffe\">", "");
            mealLabels = mealLabels.Replace("</td>", "");

            while (regMealLabelSingle.IsMatch(mealLabels))
            {
                string singleLabel = regMealLabelSingle.Match(mealLabels).Value;
                mealLabels = mealLabels.Replace(singleLabel, "");

                singleLabel = singleLabel.Replace("<img alt=\"", "");
                singleLabel = singleLabel.Replace("\"", "");
                newMeal.AddLabel(singleLabel);
            }

            mensa.AddMeal(newMeal);
            mensaText = mensaText.Replace(mealText, "");
            return mensaText;
        }

        public Mensa FindMensaByName(string name)
        {
            foreach (Mensa mensa in mensen)
            {
                if (mensa.Name == name)
                {
                    return mensa;
                }
            }
            return null;
        }

        /// <summary>
        /// the sourcecode of the received HTML site
        /// </summary>
        public String Source { get => source; }

        /// <summary>
        /// all Mensen, which are safed by this extractor
        /// </summary>
        public HashSet<Mensa> Mensen { get => mensen; }

        /// <summary>
        /// the URL, on which the Extractor is going to request/ has been requesting data
        /// </summary>
        public Uri URL { get => url; }

        /// <summary>
        /// the Title of the HTML document
        /// </summary>
        public string Title { get => title; }


        /// <summary>
        /// shows, if the Async PageDonwload has been finished
        /// </summary>
        public bool IsFinished { get => load; }
    }
}
