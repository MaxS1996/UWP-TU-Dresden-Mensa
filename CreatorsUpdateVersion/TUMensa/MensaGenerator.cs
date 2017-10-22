using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TUMensa
{
    class MensaGenerator
    {
        protected String source;


        public MensaGenerator (String source)
        {
            setSource(source);
        }

        public bool setSource(String source)
        {
            if(source != null)
            {
                this.source = source;
                return true;
            }
            return false;
        }
        //just for debugging, remove when class has been finished
        public String getSource()
        {
            return source;
        }

        public bool isMensa()
        {
            Regex plan = new Regex("<table class=\"speiseplan\">(.*?)</table>");
            return plan.IsMatch(source);
        }

        public Mensa createMensa()
        {
            if (!isMensa())
            {
                return null;
            }


            Regex plan = new Regex("<table class=\"speiseplan\">(.*?)</table>");
            String speiseplan = plan.Match(source).Value;
            source = source.Replace(speiseplan, "");
            String mname = createName(speiseplan);
            if(mname == null)
            {
                //Exception kommt bald
                mname = "ERROR";
            }

            //Mensa wird erzeugt
            Mensa newMensa = new Mensa(mname, speiseplan);


            //Mensa wird mit Gerichten befüllt
            Regex mealExp = new Regex("<tr class=\"(.*?)row\">(.*?)</tr>");
            String meal = "";
            while (mealExp.IsMatch(speiseplan))
            {
                meal = mealExp.Match(speiseplan).Value;
                Meal newMeal = fillMensa(meal);
                if(newMeal.getName() == "" || newMeal.getName() == String.Empty)
                {

                }
                else
                {
                    newMensa.addMeal(newMeal);
                }
                speiseplan = speiseplan.Replace(meal, "");
            }

            return newMensa;
        }

        protected String createName(String speiseplan)
        {
            String mname = null;
            Regex mnameExp = new Regex("<th class=\"text\">(.*?)</th><th class=\"stoffe\">Infos</th>");
            mname = mnameExp.Match(speiseplan).ToString();
            mname = mname.Replace("<th class=\"text\">", "");
            mname = mname.Replace("</th><th class=\"stoffe\">Infos</th>", "");
            mname = mname.Replace("Angebote ", "");

            return mname;
        }

        protected Meal fillMensa(String plan)
        {
            Regex nExp = new Regex("<td class=\"text\">(.*?)</td>");
            Regex lsExp = new Regex("<td class=\"stoffe\">(.*?)</td>");
            Regex pExp = new Regex("<td class=\"preise\">(.*?)</a></td>");
            Regex mensaVital = new Regex("<div class=\"mensavitalicon\">(.*?)</div>");
            Regex htmlTags = new Regex("<(.*?)>");

            //extract name
            String name = nExp.Match(plan).Value;
            name = mensaVital.Replace(name, "MENSA VITAL: ");
            name = htmlTags.Replace(name, " ");
            name = name.Replace("<td class=\"text\">", "");
            name = name.Replace("</td>", "");

            //extract prices
            String prices = pExp.Match(plan).Value;
            prices = prices.Replace("<td class=\"preise\">", "");
            String mealLink = generateMealLink(prices);
            prices = prices.Replace("</a></td>", "");
            Regex clean = new Regex("<a href=(.*?)>");
            prices = clean.Replace(prices, " ");

            //generate Meal
            Meal newMeal = new Meal(name, prices, mealLink);

            //fill Meal with warning labels
            //extract labels
            String labels = lsExp.Match(plan).Value;
            labels = labels.Replace("<td class=\"stoffe\">", "");
            labels = labels.Replace("</td>", "");

            Regex getSingleLabel = new Regex("<img alt=\"(.*?)\"");
            String label = null;
            while (getSingleLabel.IsMatch(labels))
            {
                label = getSingleLabel.Match(labels).Value;
                labels = labels.Replace(label, "");

                label = label.Replace("<img alt=\"", "");
                label = label.Replace("\"", "");
                newMeal.addLabels(label);
            }

            

            return newMeal;
        }


        public String generateMealLink(String text)
        {
            Regex link = new Regex("\"(.*?)\"");
            text = link.Match(text).Value;
            String mensa = "https://www.studentenwerk-dresden.de/mensen/speiseplan/";

            if (text != "")
            {
                
                text = text.Replace("\"", "");
                text = mensa + text;
            }
            return text;
        }

    }
}
