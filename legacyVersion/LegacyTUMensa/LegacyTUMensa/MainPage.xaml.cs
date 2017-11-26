using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace LegacyTUMensa
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Extractor god = null;
        private Uri today = new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/?print=1");
        private Uri tomorrow = new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/morgen.html?print=1");
        private Uri[] w0 = {    new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d1.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d2.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d3.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d4.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d5.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d6.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w0-d7.html?print=1")};

        private Uri[] w1= {    new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d1.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d2.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d3.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d4.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d5.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d6.html?print=1"),
                                new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/w1-d7.html?print=1")};

        private Mensa selectedMensa;
        private Meal selectedMeal;

        public MainPage()
        {
            god = new Extractor(today);
            this.InitializeComponent();
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;

            ShareButton.IsEnabled = false;
            DateTextBlock.Text = DateTime.Today.ToString("D");
            Download();
            MySplitView.IsPaneOpen = true;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                MySplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
            }

        }

        private async void Download()
        {
            try
            {
                ReloadButton.IsEnabled = false;
                DateChangerButton.IsEnabled = false;
                ShareButton.IsEnabled = false;

                MensaListView.Items.Clear();
                await god.DownloadPage();
                god.Extract();
                MealDateTextBlock.Text = god.Title;
                FillNavigation();

                ReloadButton.IsEnabled = true;
                DateChangerButton.IsEnabled = true;
            }
            catch(Exception e)
            {
                DateTextBlock.Text = "ERROR";
                ShareButton.IsEnabled = false;  
            }
        }

        private void FillNavigation()
        {
            foreach(Mensa mensa in god.Mensen)
            {
                MensaListView.Items.Add(mensa.Name);
            }

            try
            {
                MensaListView.SelectedIndex = 0;
                MySplitView.IsPaneOpen = true;
            }

            catch(Exception e)
            {
                DateTextBlock.Text = "no offerings today";
            }
        }

        private void SelectMensa(int index)
        {
            MealListView.Items.Clear();
            selectedMeal = null;
            selectedMensa = null;

            if ((index == -1) || (index >= god.Mensen.Count))
            {
                MensaTitleTextBlock.Text = "Angebot des Studentenwerks Dresden";
                return;
            }
            else
            {

                selectedMensa = god.Mensen.ElementAt(index);
                MensaTitleTextBlock.Text = selectedMensa.Name;
                foreach(Meal meal in selectedMensa.Meals)
                {
                    MealListView.Items.Add(meal.Name);
                }

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    MySplitView.IsPaneOpen = false;
                }
            }
        }

        private void SelectMeal(int index)
        {
            MealTitleTextBlock.Text = "";
            MealPriceTextBlock.Text = "";
            MealLabelTextBlock.Text = "";
            ShareButton.IsEnabled = false;
            MealImage.Source = new BitmapImage(new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg"));

            if (selectedMensa == null)
            {
                return;
            }

            if(index < MealListView.Items.Count && index >= 0)
            {
                selectedMeal = selectedMensa.Meals.ElementAt(index);
                MealTitleTextBlock.Text = selectedMeal.Name;
                MealPriceTextBlock.Text = selectedMeal.Price;
                MealLabelTextBlock.Text = "";
                foreach(string label in selectedMeal.Labels)
                {
                    MealLabelTextBlock.Text += "\n" + label;
                }

                ShareButton.IsEnabled = true;

                try
                {
                    MealImage.Source = new BitmapImage(selectedMeal.PictureURI);
                }
                catch (Exception)
                {
                    MealImage.Source = new BitmapImage(new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg"));
                }
            }
        }

        private void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            String ContentText = "";

            if(selectedMeal == null)
            {
                args.Request.FailWithDisplayText("no Meal has been selected to share!");
                return;
            }

            ContentText = selectedMeal.Name + "\n" + selectedMeal.Price + "\n" + selectedMensa.Name + "\n" + DateTextBlock.Text + "\n TU MENSA for Windows 10 (legacy version)";
            args.Request.Data.SetText(ContentText);
            args.Request.Data.Properties.Title = "Essensvorschlag!";
            args.Request.Data.SetWebLink(selectedMeal.MealURI);

            if(selectedMeal.PictureURI != null && !selectedMeal.PictureURI.ToString().Equals(String.Empty))
            {
                args.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(selectedMeal.PictureURI));
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            Download();
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void MensaListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectMensa(MensaListView.SelectedIndex);
        }

        private void DateChangerButton_Click(object sender, RoutedEventArgs e)
        {
            Color accent = (Color)this.Resources["SystemAccentColor"];
            DateTime day = DateTime.Today;

            if (god.URL.Equals(today))
            {
                god = new Extractor(tomorrow);
                accent.B = (byte)(255 - accent.B);
                accent.R = (byte)(255 - accent.R);
                accent.G = (byte)(255 - accent.G);
                day = day.AddDays(1);
            }
            else
            {
                god = new Extractor(today);
            }
            SolidColorBrush brush = new SolidColorBrush(accent);
            DateChangerButton.Background = brush;
            MealDateTextBlock.Foreground = brush;
            DateTextBlock.Foreground = brush;

            DateTextBlock.Text = day.ToString("D");
            Download();
            MySplitView.IsPaneOpen = true;
        }

        private void MealListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectMeal(MealListView.SelectedIndex);
        }
    }
}
