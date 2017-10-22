using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

namespace MensaApp2
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Extractor god;
        private Uri today = new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/?print=1");
        private Uri tomorrow = new Uri("https://www.studentenwerk-dresden.de/mensen/speiseplan/morgen.html?print=1");
        private Mensa selectedMensa;
        private Meal selectedMeal;

        private object header;

        public MainPage()
        {
            god = new Extractor(today);
            this.InitializeComponent();
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
            header = NavView.MenuItems.ElementAt(0);
            DateButton.Background = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]);
            DateTextBlock.Foreground = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]);
            ShareButton.IsEnabled = false;
            DateTextBlock.Text = DateTime.Today.ToString("D");
            Download();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // set the initial SelectedItem 
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "reload")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            string mensaName = args.InvokedItem as string;
            SelectMensa(mensaName);
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem item = args.SelectedItem as NavigationViewItem;
            if(item != null)
            {
                SelectMensa(item.Tag.ToString());
            }
        }

        private async void Download()
        {
            try
            {
                ReloadButton.IsEnabled = false;
                DateButton.IsEnabled = false;
                ShareButton.IsEnabled = false;
                NavView.MenuItems.Clear();
                ListViewMeal.Items.Clear();
                await god.DownloadPage();
                god.Extract();
                NavView.MenuItems.Add(header);
                FillNavigation();
            }
            catch (Exception)
            {
                TextBlockMealTitle.Text = "Connection Error!";
                ShareButton.IsEnabled = false;
            }
            ShareButton.IsEnabled = true;
            ReloadButton.IsEnabled = true;
            DateButton.IsEnabled = true;
        }

        private void FillNavigation()
        {

            foreach (Mensa mensa in god.Mensen)
            {
                NavView.MenuItems.Add(new NavigationViewItem()
                { Content = mensa.Name, Icon = new SymbolIcon(Symbol.ZoomIn), Tag = mensa.Name });
            }

            try
            {
                SelectMensa(god.Mensen.First().Name);
            }
            catch (Exception)
            {
                return;
            }
        }

        private void SelectMensa(string name)
        {
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == name)
                {
                    NavView.SelectedItem = item;
                    ListViewMeal.Items.Clear();
                    selectedMensa = god.FindMensaByName(name);
                    if(selectedMensa != null)
                    {
                        TextBlockMensaTitle.Text = selectedMensa.Name;
                        foreach(Meal meal in selectedMensa.Meals)
                        {
                            ListViewMeal.Items.Add(meal.Name);
                        }
                        if(ListViewMeal.Items.Count > 0)
                        {
                            SelectMeal(0);
                        }
                    }
                    break;
                }
            }
        }

        private void SelectMeal(int index)
        {
            if(index == -1)
            {
                index = 0;
            }

            selectedMeal = selectedMensa.Meals.ElementAt(index);

            TextBlockMealTitle.Text = selectedMeal.Name;

            TextBlockMealTitle.Foreground = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]);
            if (selectedMeal.Vital)
            {
                TextBlockMealTitle.Foreground = new SolidColorBrush(Colors.Green);
            }

            if (selectedMeal.Evening)
            {
                TextBlockMealTitle.Foreground = new SolidColorBrush(Colors.DarkRed);
            }
            TextBlockMealPrice.Text = selectedMeal.Price;

            TextBlockMealLabels.Text = String.Empty;
            foreach (string label in selectedMeal.Labels)
            {
                TextBlockMealLabels.Text = TextBlockMealLabels.Text + "\n" + label;
            }

            try
            {
                ImageMeal.Source = new BitmapImage(selectedMeal.PictureURI);
            }
            catch (Exception)
            {

                ImageMeal.Source = new BitmapImage(new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg"));
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            Download();
        }

        private void DateButton_Click(object sender, RoutedEventArgs e)
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

            DateButton.Background = new SolidColorBrush(accent);
            DateTextBlock.Foreground = new SolidColorBrush(accent);
            DateTextBlock.Text = day.ToString("D");
            Download();
        }

        private void ListViewMeal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectMeal(ListViewMeal.SelectedIndex);
        }

        private void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            String ContentText;

            if (selectedMeal != null)
            {
                ContentText = selectedMeal.Name + "\n" + selectedMeal.Price + "\n" + selectedMensa.Name + "\n" + DateTextBlock.Text + "\n==================\n TU MENSA for WINDOWS";

                args.Request.Data.SetText(ContentText);
                args.Request.Data.Properties.Title = "Essensvorschlag!";
                args.Request.Data.SetWebLink(selectedMeal.MealURI);

                if (selectedMeal.PictureURI != null && selectedMeal.PictureURI.ToString() != "")
                {
                    args.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(selectedMeal.PictureURI));
                }
            }
            else
            {
                args.Request.FailWithDisplayText("Bitte erst ein Gericht auswählen!\n Please select a meal first!");
            }
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
    }
}
