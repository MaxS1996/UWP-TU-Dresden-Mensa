using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUMensa
{
    class Meal
    {
        protected String name;
        protected String price;
        protected HashSet<String> labels;
        protected String picturelink;

        public Meal(String name, String price)
        {
            setName(name);
            setPrice(price);
            labels = new HashSet<String>();
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

    }
}
