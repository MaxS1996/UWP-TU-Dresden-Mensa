using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUMensa
{
    class Mensa
    {
        protected String name;
        protected HashSet<Meal> Meals;
        protected String mensaSource;

        public Mensa (String name, String source)
        {
            setName(name);
            if(source != null)
            {
                mensaSource = source;
            }
            else
            {
                mensaSource = "";
            }
            Meals = new HashSet<Meal>();
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

        public bool addMeal(Meal meal)
        {
            return Meals.Add(meal);

        }

        public bool removeMeal(Meal meal)
        {
            return Meals.Remove(meal);
        }

        public HashSet<Meal> getMeals()
        {
            return Meals;
        }
        public String getName()
        {
            return name;
        }

        public String getMensaSource()
        {
            return mensaSource;
        }
    }
}
