using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyTUMensa
{
    class Mensa
    {
        /// <summary>
        /// the name of this Mensa
        /// </summary>
        protected string name;

        /// <summary>
        /// the Meals, which are served at this Mensa are saved here
        /// </summary>
        protected HashSet<Meal> meals;


        /// <summary>
        /// creates a new Mensa, the name should not be empty
        /// </summary>
        /// <param name="name">the name of this Mensa, should not be empty</param>
        public Mensa(string name)
        {
            this.name = name;
            meals = new HashSet<Meal>();
        }

        /// <summary>
        /// adds a Meal to this Mensa
        /// </summary>
        /// <param name="meal">the Meal, you want to add</param>
        /// <returns></returns>
        public bool AddMeal(Meal meal)
        {
            if (meals.Contains(meal) || meal.Name.Equals(String.Empty))
            {
                return false;
            }
            else
            {
                meals.Add(meal);
                return true;
            }
        }

        /// <summary>
        /// Returns the name of this Mensa
        /// </summary>
        public string Name { get => name; }


        /// <summary>
        /// Returns a HashSet<Meal> of all Meals
        /// </summary>
        public HashSet<Meal> Meals { get => meals; }

    }
}
