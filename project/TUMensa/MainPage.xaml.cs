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
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

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

        static String today = "https://www.studentenwerk-dresden.de/mensen/speiseplan/?print=1";
        static String tomorrow = "https://www.studentenwerk-dresden.de/mensen/speiseplan/morgen.html?print=1";

        String page = today;

        public MainPage()
        {
            this.InitializeComponent();
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
            TextBlockDate.Text = DateTime.Today.ToString("D");
            LoadMensen();

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                menu = false;
                MySplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
            }

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
            LoadMensen();
        }

        private void Button2_Tapped(object sender, TappedRoutedEventArgs e)
        {
            menu = !menu;
        }

        private void Button3_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(!MySplitView.IsPaneOpen)
            {
                MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
            }

            if (page == today)
            {
                TextBlockDate.Text = DateTime.Today.AddDays(1).ToString("D");
                Button3.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                page = tomorrow;
                LoadMensen();
            }
            else
            {
                page = today;
                TextBlockDate.Text = DateTime.Today.ToString("D");
                Button3.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                LoadMensen();
            }
        }

        private void LoadMensen()
        {
            MensaListView.Items.Clear();
            MealListView.Items.Clear();
            TextBlockMealLabels.Text = "";
            TextBlockMealName.Text = "";
            TextBlockMealPrice.Text = "";
            TextBlock1.Text = "Mensen Studentenwerk Dresden";
            chosenMeal = null;
            selected = null;

            TextBlockMealName.Text = "Bitte wählen Sie eine Mensa aus";
            TextBlockMealLabels.Text = "";
            TextBlockMealPrice.Text = "";

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
                if(newMensa.getMeals().Count != 0)
                {
                    Mensen.Add(newMensa);
                }
            }
            fillListView();
            
        }

        private String getTitle(String source)
        {
            Regex title = new Regex(@"<title>(.*)</title>");
            String matched = title.Match(source).Value;
            matched = matched.Replace("<title>", "");
            matched = matched.Replace("</title>", "");
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
            chosenMeal = null;
            selected = Mensen.ElementAt(index);
            showMensa(selected);
        }

        private void showMensa(Mensa mensa)
        {
            TextBlock1.Text = mensa.getName();
            TextBlockMealName.Text = "Bitte wählen Sie ein Gericht";
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
                catch(Exception)
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

        private void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            String ContentText;

            if (chosenMeal != null)
            {
                ContentText = chosenMeal.getName() + "\n" + chosenMeal.getPrice() + "\n" + selected.getName() +"\n" + TextBlockDate.Text + "\n==================\n TU MENSA for WINDOWS";

                args.Request.Data.SetText(ContentText);
                args.Request.Data.Properties.Title = "Essensvorschlag!";
                args.Request.Data.SetWebLink(new Uri(chosenMeal.getMealLink()));

                if(chosenMeal.getPicturelink() != null&&chosenMeal.getPicturelink() != "")
                {
                    args.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(chosenMeal.getPicturelink())));
                }
            }
            else
            {
                args.Request.FailWithDisplayText("Bitte erst ein Gericht auswählen!\n Please select a meal first!");
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

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {           
            DataTransferManager.ShowShareUI();
        }
    }
}
