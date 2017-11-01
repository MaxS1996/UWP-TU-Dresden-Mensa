using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace LegacyTUMensa
{
    class Meal
    {
        /// <summary>
        /// the name of the Meal
        /// </summary>
        protected string name;

        /// <summary>
        /// the two prices, one for students and one for employees are safed as string, because it is easier
        /// </summary>
        protected string price;

        /// <summary>
        /// the different labels for every meal are safed in a Set, to print them easier beneath each other
        /// </summary>
        protected HashSet<String> labels;

        /// <summary>
        /// the URL for the picture is safed here
        /// </summary>
        protected Uri pictureLink;

        /// <summary>
        /// the URL for the Meal is safed here
        /// </summary>
        protected Uri mealLink;

        /// <summary>
        /// shows if this Meal belongs to "Fit & Vital"
        /// </summary>
        protected bool vital = false;

        /// <summary>
        /// true -> this meal is going to be served in the evening
        /// false -> this meal is going to be served for lunch
        /// </summary>
        protected bool evening = false;


        /// <summary>
        /// complete constructor for a Meal
        /// </summary>
        /// <param name="name">the name of the Meal </param>
        /// <param name="price">the prices of the Meal</param>
        /// <param name="labels">the information labels of the Meal</param>
        /// <param name="picture">the picture URL of this Meal</param>
        /// <param name="link">the link to the Meal on studenten-dresden.de</param>
        public Meal(string name, string price, HashSet<String> labels, Uri picture, Uri link)
        {
            this.name = name;
            this.price = price;
            this.labels = labels;
            this.pictureLink = picture;
            this.mealLink = link;
        }

        /// <summary>
        /// constructor for Meal, which needs just a couple of information
        /// </summary>
        /// <param name="name"></param>
        /// <param name="price"></param>
        /// <param name="labels"></param>
        /// <param name="link"></param>
        public Meal(string name, string price, HashSet<String> labels, Uri link)
        {
            this.name = name;
            this.price = price;
            this.labels = labels;
            this.pictureLink = new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg");
            this.mealLink = link;
        }

        /// <summary>
        /// constructor for Meal, which needs just a couple of information
        /// </summary>
        /// <param name="name">the name of the Meal </param>
        /// <param name="price">the prices of the Meal</param>
        /// <param name="picture">the picture URL of this Meal</param>
        /// <param name="link">the link to the Meal on studenten-dresden.de</param>
        public Meal(string name, string price, Uri picture, Uri link)
        {
            this.name = name;
            this.price = price;
            this.pictureLink = picture;
            this.labels = new HashSet<string>();
            this.mealLink = link;
        }

        /// <summary>
        /// constructor for Meal, which needs just a couple of information
        /// </summary>
        /// <param name="name">the name of the Meal </param>
        /// <param name="price">the prices of the Meal</param>
        /// <param name="link">the link to the Meal on studenten-dresden.de</param>
        public Meal(string name, string price, Uri link)
        {
            this.name = name;
            this.price = price;
            this.labels = new HashSet<string>();
            this.pictureLink = new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg");
            this.mealLink = link;

        }

        /// <summary>
        /// Adds a new information label to this Meal
        /// </summary>
        /// <param name="newLabel">the label you want to add</param>
        /// <returns></returns>
        public bool AddLabel(string newLabel)
        {
            if (newLabel == "" || newLabel == String.Empty)
            {
                return false;
            }
            else
            {
                labels.Add(newLabel);
                return true;
            }
        }

        public async void GeneratePictureUri()
        {
            string source = "";
            HttpClient http = new HttpClient();

            try
            {
                var response = await http.GetByteArrayAsync(this.mealLink);
                source = System.Text.Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = System.Net.WebUtility.HtmlDecode(source);

                if (source != null)
                {
                    pictureLink = ExtractPictureLink(source);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private Uri ExtractPictureLink(string source)
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

            return new Uri(source);
        }

        /// <summary>
        /// Returns the name of the Meal
        /// </summary>
        public string Name { get => name; }


        /// <summary>
        /// Returns the prices of this Meal
        /// </summary>
        public string Price { get => price; }

        /// <summary>
        /// Returns all labels of this Meal as HashSet<String>
        /// </summary>
        public HashSet<string> Labels { get => labels; }

        /// <summary>
        /// Returns the Uri of the picture of this Meal
        /// </summary>
        public Uri PictureURI { get => pictureLink; }

        /// <summary>
        /// Returns the Uri of this Meal on studentenwerk-dresden.de
        /// </summary>
        public Uri MealURI { get => mealLink; }

        /// <summary>
        /// Returns the Vital value
        /// true means it belongs to "Fit&Vital"
        /// false means it is an ordinary meal
        /// </summary>
        public bool Vital { get => vital; set => vital = value; }

        /// <summary>
        /// Returns if the meal is just served in the evening
        /// </summary>
        public bool Evening { get => evening; set => evening = value; }

    }
}

