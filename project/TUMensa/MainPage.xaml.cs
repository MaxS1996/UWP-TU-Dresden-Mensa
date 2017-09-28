using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using System.Net.Http;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media.Imaging;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace TUMensa
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        String sourcefile = "";
        bool menu = true;
        HashSet<Mensa> Mensen = new HashSet<Mensa>();
        Mensa selected;
        Meal chosenMeal;

        String today = "https://www.studentenwerk-dresden.de/mensen/speiseplan/?print=1";
        String tomorrow = "https://www.studentenwerk-dresden.de/mensen/speiseplan/morgen.html?print=1";

        String page = "https://www.studentenwerk-dresden.de/mensen/speiseplan/?print=1";

        public MainPage()
        {
            this.InitializeComponent();
            
            TextBlockMealName.Text = "Bitte wählen Sie eine Mensa aus";
            TextBlockMealLabels.Text = "";
            TextBlockMealPrice.Text = "";
            LoadMensen();
        }

        public async void DownloadPageAsync()
        {
            HttpClient http = new HttpClient();
            try
            {
                var response = await http.GetByteArrayAsync(page);
                String source = System.Text.Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = System.Net.WebUtility.HtmlDecode(source);

                if (source != null)
                {
                    sourcefile = source;
                    TextBlockTitle.Text = getTitle(sourcefile);
                    Analyze(sourcefile);
                    return;
                }
            }
            catch (Exception)
            {
                showConnectionError();
            }
        }

        private void Button1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MensaListView.Items.Clear();
            MealListView.Items.Clear();
            TextBlockMealLabels.Text = "";
            TextBlockMealName.Text = "";
            TextBlockMealPrice.Text = "";
            LoadMensen();
        }

        private void Button2_Tapped(object sender, TappedRoutedEventArgs e)
        {
            menu = !menu;
        }

        private void Button3_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (page == today)
            {
                Button3.Content = "Tomorrow";
                page = tomorrow;
                LoadMensen();
            }
            else
            {
                page = today;
                Button3.Content = "Today";
                LoadMensen();
            }
        }

        private void LoadMensen()
        {
            Mensen = new HashSet<Mensa>();
            MensaListView.Items.Clear();
            MealListView.Items.Clear();
            DownloadPageAsync();
        }

        private void Analyze(String source)
        {
            sourcefile = sourcefile.Replace("\n", "");
            Regex foodExp = new Regex("<div id=\"spalterechtsnebenmenue\">(.*)</div>");
            Regex matchTitle = new Regex("<div id=\"spalterechtsnebenmenue\">(.*?)<h1>(.*?)</h1>");
            string food = foodExp.Match(sourcefile).Value;
            food = matchTitle.Replace(food, "");
            food = food.Replace("\\t", "");

            MensaListView.Items.Clear();
            MensaGenerator God = new MensaGenerator(food);
            while (God.isMensa())
            {
                Mensa newMensa = God.createMensa();
                Mensen.Add(newMensa);
            }
            fillListView();
            
        }

        private String getTitle(String source)
        {
            Regex title = new Regex(@"<title>(.*)</title>");
            String matched = title.Match(source).Value;
            matched = matched.Replace("<title>", "");
            matched = matched.Replace("</title>", "");
            //TextBlockTitle.Text = matched;
            return matched;

        }

        private void fillListView()
        {
            foreach (Mensa mensa in Mensen)
            {
                MensaListView.Items.Add(mensa.getName());
            }
        }

        private void MensaListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            int index = MensaListView.SelectedIndex;
            if (index == -1)
            {
                return;
            }
            MySplitView.IsPaneOpen = menu;
            MealListView.Items.Clear();
            chosenMeal = new Meal("", "");
            selected = Mensen.ElementAt(index);
            showMensa(selected);
        }

        private void showMensa(Mensa mensa)
        {
            TextBlock1.Text = mensa.getName();
            MealListView.Items.Clear();
            foreach (Meal meal in mensa.getMeals())
            {
                MealListView.Items.Add(meal.getName());
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void MealListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = MealListView.SelectedIndex;
            showMeal(index);            
        }

        private void showMeal(int index)
        {
            if(index < MealListView.Items.Count&& index >=0)
            {
                chosenMeal = selected.getMeals().ElementAt(index);
                TextBlockMealName.Text = chosenMeal.getName();
                TextBlockMealPrice.Text = chosenMeal.getPrice();
                TextBlockMealLabels.Text = "";
                foreach (String label in chosenMeal.getLabels())
                {
                    TextBlockMealLabels.Text = TextBlockMealLabels.Text + "\n" + label;
                }
                try
                {
                    MealImage.Source = new BitmapImage(new Uri(chosenMeal.getPicturelink()));
                }
                catch(Exception e)
                {
                    MealImage.Source = new BitmapImage(new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg"));
                }
                
            }
            else
            {
                TextBlockMealName.Text = "";
                TextBlockMealPrice.Text = "";
                TextBlockMealLabels.Text = "";
                MealImage.Source = new BitmapImage(new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg"));

            }
        }

        public void showConnectionError()
        {
            TextBlock1.Text = "Verbindungsfehler";
            TextBlockTitle.Text = "Verbindungsfehler";
            TextBlockMealPrice.Text = "Versuchen Sie eine WLAN oder Mobilfunkverbindung herzustellen!";
            TextBlockMealName.Text = "Verbindungsfehler";
            TextBlockMealLabels.Text = "Verbindungsfehler";
        }
    }
}
