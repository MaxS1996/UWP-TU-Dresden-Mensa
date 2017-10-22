using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace TUMensa
{
    class Meal
    {
        protected String name;
        protected String price;
        protected HashSet<String> labels;
        protected String picturelink;
        protected String meallink;

        public Meal(String name, String price)
        {
            setName(name);
            setPrice(price);
            labels = new HashSet<String>();
            meallink = "";
            picturelink = "";
        }

        public Meal(String name, String price, String meallink)
        {
            setName(name);
            setPrice(price);
            setMealLink(meallink);
            labels = new HashSet<String>();
            generatepictureLink();
        }


        public bool setName(String name)
        {
            if(name != null)
            {
                this.name = name;
                return true;
            }
            return false;
        }

        public bool setPrice(String price)
        {
            if (price != null)
            {
                this.price = price;
                return true;
            }
            return false;
        }

        public bool addLabels(String label)
        {
            if (label != null)
            {
                return labels.Add(label);
            }
            return false;
        }

        public bool setPicture(String link)
        {
            if (link != null)
            {
                this.picturelink = link;
                return true;
            }
            return false;
        }

        public String getName()
        {
            return name;
        }

        public String getPrice()
        {
            return price;
        }

        public HashSet<String> getLabels()
        {
            return labels;
        }

        public String getPicturelink()
        {
            return picturelink;
        }

        public Boolean setMealLink(String meal)
        {
            if(meal !=null || meal != "")
            {
                meallink = meal;
                return true;
            }
            return false;
        }

        public String getMealLink()
        {
            return meallink;
        }

        public async void generatepictureLink()
        {
            if (meallink == null || meallink == "")
            {
                picturelink = null;
                return;
            }


            String source;
            HttpClient http = new HttpClient();
            try
            {
                var response = await http.GetByteArrayAsync(meallink);
                source = System.Text.Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = System.Net.WebUtility.HtmlDecode(source);

                if (source != null)
                {
                    picturelink = extractPictureLink(source);
                }
            }
            catch (Exception)
            {
                picturelink = "fehler";
                return;
            }
        }

        protected String extractPictureLink(String source)
        {
            Regex div = new Regex("<div id=\"essenbild\">(.*?)</div>");
            Regex link = new Regex("href=\"(.*?)\"");
            source = div.Match(source).Value;
            source = link.Match(source).Value;
            // "href=\"//bilderspeiseplan.studentenwerk-dresden.de/m35/201709/191469.jpg?date=201709281541\""
            if(source.Length > 0)
            {
                source = source.Replace("href=\"//", "");
                source = source.Replace("\"", "");
                source = "http://" + source;
            }

            return source;
        }

    }
}
