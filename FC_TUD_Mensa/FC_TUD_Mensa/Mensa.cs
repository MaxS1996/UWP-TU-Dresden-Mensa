using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MensaExtractor
{
    class Mensa
    {
        private string name;
        //private string[] openings; 
        //not implemented yet
        private HashSet<MensaMeal> meals = new HashSet<MensaMeal>();
        private string source;

        public Mensa(string source, bool debug = false)
        {
            if (debug)
            {
                this.source = source;
            }
            this.name = ExtractName(source);
            ExtractMeals(source);
        }

        public static string ExtractName(string source)
        {
            string mensaName;
            Regex regMensaName = new Regex("<th class=\"text\">(.*?)</th><th class=\"stoffe\">Infos</th>");
            mensaName = regMensaName.Match(source).Value;
            mensaName = mensaName.Replace("<th class=\"text\">", "");
            mensaName = mensaName.Replace("</th><th class=\"stoffe\">Infos</th>", "");
            mensaName = mensaName.Replace("Angebote ", "");

            return mensaName;
        }

        private void ExtractMeals(string source)
        {
            Regex regMeal = new Regex("<tr class=\"(.*?)row\">(.*?)</tr>");
            Regex regLast = new Regex("<tr class=\"last\">(.*?)</tr>");
            MatchCollection mealMatches = regMeal.Matches(source);
            MatchCollection mealLasts = regLast.Matches(source);
            foreach (Match mealMatch in mealMatches)
            {
                MensaMeal newMeal = new MensaMeal(mealMatch.Value);
                if (!newMeal.Name.Equals(String.Empty))
                {
                    meals.Add(newMeal);
                }
            }
            foreach (Match mealMatch in mealLasts)
            {
                MensaMeal newMeal = new MensaMeal(mealMatch.Value);
                if (!newMeal.Name.Equals(String.Empty))
                {
                    meals.Add(newMeal);
                }
            }
        }

        public string Name{ get => name; }
        public HashSet<MensaMeal> Meals { get => meals; }
        public bool IsOpen { get { return !(Meals.Count == 0); } }
    }
}
