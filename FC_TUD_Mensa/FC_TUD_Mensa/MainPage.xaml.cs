using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MensaExtractor;
using Windows.Foundation.Metadata;
using System.Globalization;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace FC_TUD_Mensa
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Plan speiseplan;
        private MensaDay selectedDay;
        private Mensa selectedMensa;
        private MensaMeal selectedMeal;
        private CultureInfo cultInfo = new CultureInfo(Windows.System.UserProfile.GlobalizationPreferences.Languages[0].ToString());
        //SortedList<DateTime, DateWrapper> DayKeys = new SortedList<DateTime, DateWrapper>();

        public MainPage()
        {
            this.InitializeComponent();
            
            if (cultInfo.Parent.Name.Equals("de"))
            {
                PriceTitleTextBlock.Text = "Preis:";
            }
            else
            {
                PriceTitleTextBlock.Text = "Price:";
            }
            this.DataContext = this;
            this.Download();
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;

            var currentHeight = Window.Current.Bounds.Height;
            var currentWidth = Window.Current.Bounds.Width;

            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")&&currentWidth>=1080)
            {
                MainSplitView.IsPaneOpen = true;
                HamburgerButton.IsEnabled = false;
                HamburgerButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainSplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
                MainSplitView.IsPaneOpen = true;
            }
        }

        private void Download()
        {
            speiseplan = new Plan();
            DateTextBlock.Text = "";
            MensaDateTextBlock.Text = "";
            ShareButton.IsEnabled = false;
            ReloadButton.IsEnabled = false;
            try
            {
                speiseplan.Download(false, true, true, true);

                if (speiseplan.Days.TryGetValue(DateTime.Today, out selectedDay) && selectedDay.Mensen.Count > 0)
                {
                    selectedMensa = selectedDay.Mensen.First();
                    DateTextBlock.Text = selectedDay.Day.ToString("D");
                    MensaDateTextBlock.Text = selectedDay.Day.ToString("D");
                }
                else
                {
                    DateTime min = speiseplan.Days.Keys.Min();
                    bool hasDay = speiseplan.Days.TryGetValue(min, out selectedDay);

                    if (!hasDay)
                    {
                        //ShowConnectionError();
                        throw new Exception();
                    }

                    selectedMensa = selectedDay.Mensen.First();
                    DateTextBlock.Text = selectedDay.Day.ToString("D");
                    MensaDateTextBlock.Text = selectedDay.Day.ToString("D");
                }
                if (selectedMensa.Meals.Count > 0)
                {
                    selectedMeal = selectedMensa.Meals.First();
                    ShareButton.IsEnabled = true;
                }

                MensaTitleTextBlock.DataContext = selectedMensa;
                MealListView.ItemsSource = selectedMensa.Meals;
                MensaListView.ItemsSource = selectedDay.Mensen;

                Calendar.DataContext = speiseplan.Days.Keys;
                Calendar.MinDate = speiseplan.Days.Keys.Min().AddDays(-1);
                Calendar.MaxDate = speiseplan.Days.Keys.Max().AddDays(6);

                if (MensaListView.Items.Count > 0)
                {
                    MensaListView.SelectedIndex = 0;
                }
            }
            catch (Exception e)
            {
                if (cultInfo.Parent.Name.Equals("de"))
                {
                    MensaTitleTextBlock.Text = "FEHLER";
                    MealNameTextBlock.Text = "Keine Verbindung";
                }
                else
                {
                    MensaTitleTextBlock.Text = "ERROR";
                    MealNameTextBlock.Text = "No Connection";
                }
                ShowConnectionError();
            }
            MainSplitView.IsPaneOpen = true;
            ReloadButton.IsEnabled = true;
        }

        private async void ShowConnectionError()
        {
            var dialog = new MessageDialog("Connection error! We are unable to establish a connection, we are sorry!");
            if (cultInfo.Parent.Name.Equals("de"))
            {
                dialog = new MessageDialog("Verbindungsprobleme! Wir konnten keine Verbindung zum Studentenwerk Dresden herstellen, um die Speisepläne abzurufen");
            }
            await dialog.ShowAsync();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            var currentWidth = Window.Current.Bounds.Width;
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")&&currentWidth >=1080)
            {
                MainSplitView.IsPaneOpen = true;
                return;
            }
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void Calendar_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs e)
        {
            e.Item.IsBlackout = !speiseplan.Days.Keys.Contains(e.Item.Date.Date);
        }

        private void Calendar_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate.HasValue)
            {
                int mensaPos = MensaListView.SelectedIndex;
                ShareButton.IsEnabled = false;
                DateTime targetTime = args.NewDate.Value.DateTime;
                speiseplan.Days.TryGetValue(targetTime, out selectedDay);

                MensaListView.ItemsSource = selectedDay.Mensen;
                selectedMensa = selectedDay.Mensen.First();
                MensaListView.SelectedIndex = 0;

                if (selectedMensa.IsOpen)
                {
                    selectedMeal = selectedMensa.Meals.First();
                    MealListView.ItemsSource = selectedMensa.Meals;
                    MealListView.SelectedIndex = 0;
                    ShareButton.IsEnabled = true;
                }
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    MainSplitView.IsPaneOpen = true;
                }

                DateTextBlock.Text = selectedDay.Day.ToString("D");
                MensaDateTextBlock.Text = selectedDay.Day.ToString("D");

                if(MensaListView.Items.Count > mensaPos - 1)
                {
                    MensaListView.SelectedIndex = mensaPos;
                }
            }
            
        }

        private void MensaListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(MensaListView.SelectedItems.Count > 0)
            {
                ShareButton.IsEnabled = false;
                IsOpenTextBlock.Text = String.Empty;
                selectedMensa = (Mensa)MensaListView.SelectedItem;
                MensaTitleTextBlock.DataContext = selectedMensa;
                MealListView.ItemsSource = selectedMensa.Meals;
                if (!selectedMensa.IsOpen)
                {
                    IsOpenTextBlock.Text = "No Offerings today";
                    if (cultInfo.Parent.Name.Equals("de"))
                    {
                        IsOpenTextBlock.Text = "Heute keine Angebote";
                    }
                }
                else
                {
                    selectedMeal = selectedMensa.Meals.First<MensaMeal>();
                    MealListView.SelectedIndex = 0;
                    ShareButton.IsEnabled = true;
                }

                var currentWidth = Window.Current.Bounds.Width;
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")||currentWidth <=720)
                {
                    MainSplitView.IsPaneOpen = false;
                }
            }
        }

        private void MealListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MealImage.Source = new BitmapImage(new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg"));
            ShareButton.IsEnabled = false;
            if (MealListView.SelectedItems.Count > 0)
            {
                selectedMeal = (MensaMeal)MealListView.SelectedItem;

                MealNameTextBlock.Text = selectedMeal.Name;

                MealStudentPriceTextBlock.Text = "Student: " + selectedMeal.StudentPrice;
                MealOtherPriceTextBlock.Text = "Other: " + selectedMeal.OtherPrice;

                if (cultInfo.Parent.Name.Equals("de"))
                {
                    MealStudentPriceTextBlock.Text = "Student: " + selectedMeal.StudentPrice;
                    MealOtherPriceTextBlock.Text = "Andere: " + selectedMeal.OtherPrice;
                }
                MealLabelsTextBlock.Text = "";
                foreach (string label in selectedMeal.Labels)
                {
                    MealLabelsTextBlock.Text = MealLabelsTextBlock.Text + "\n" + label;
                }
                MealFlagsTextBlock.Text = "";
                if (selectedMeal.Vital)
                {
                    MealFlagsTextBlock.Text += "FIT&VITAL ";
                }
                if (selectedMeal.Evening)
                {
                    if (cultInfo.Parent.Name.Equals("de"))
                    {
                        MealFlagsTextBlock.Text += "ABENDANGEBOT";
                    }
                    else
                    {
                        MealFlagsTextBlock.Text += "Only available at evening";
                    }
                }
                if(selectedMeal.PictureRetrieved)
                {
                    MealImage.Source = new BitmapImage(selectedMeal.Picture);
                }
                else
                {
                    SetPicture();
                }
                ShareButton.IsEnabled = true;
            }
            else
            {
                MealNameTextBlock.Text = "";
                MealStudentPriceTextBlock.Text = "";
                MealOtherPriceTextBlock.Text = "";
                MealLabelsTextBlock.Text = "";
                MealFlagsTextBlock.Text = "";
            }
        }

        private async void SetPicture()
        {
            bool done = await selectedMeal.GetPictureUriAsync();

            if(!selectedMeal.Picture.Equals(new Uri("https://bilderspeiseplan.studentenwerk-dresden.de/lieber_mensen_gehen_gross.jpg")))
            {
                MealImage.Source = new BitmapImage(selectedMeal.Picture);
            }
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            int mensaPosition = MensaListView.SelectedIndex;
            Download();
            MensaListView.SelectedIndex = mensaPosition;
        }

        private void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            String ContentText = "";

            if (selectedMeal == null)
            {
                args.Request.FailWithDisplayText("no Meal has been selected to share!");
                return;
            }

            ContentText = selectedMeal.Name + "\n" + selectedMeal.StudentPrice +"/"+ selectedMeal.OtherPrice + "\n" + selectedMensa.Name + "\n" + selectedDay.Day.ToString("D") + "\n TU MENSA for Windows 10";
            args.Request.Data.SetText(ContentText);
            if (cultInfo.Parent.Name.Equals("de"))
            {
                args.Request.Data.Properties.Title = "Essensvorschlag!";
            }
            else
            {
                args.Request.Data.Properties.Title = "Food \"suggestion\"!";
            }
            args.Request.Data.SetWebLink(selectedMeal.Link);

            if (selectedMeal.Picture != null && !selectedMeal.Picture.ToString().Equals(String.Empty))
            {
                args.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(selectedMeal.Picture));
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var currentWidth = Window.Current.Bounds.Width;
            if(currentWidth <= 1080)
            {
                MainSplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                HamburgerButton.Visibility = Visibility.Visible;
                HamburgerButton.IsEnabled = true;
                if (currentWidth <= 800)
                {
                    MainSplitView.IsPaneOpen = false;
                    MealImage.MinWidth = 30;
                    MealImage.MaxWidth = 300;

                    UpButton.IsEnabled = true;
                    UpButton.Visibility = Visibility.Visible;

                    DownButton.IsEnabled = true;
                    DownButton.Visibility = Visibility.Visible;
                }
                else
                {
                    MainSplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
                    MainSplitView.IsPaneOpen = true;

                    UpButton.IsEnabled = false;
                    UpButton.Visibility = Visibility.Collapsed;

                    DownButton.IsEnabled = false;
                    DownButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MainSplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
                MainSplitView.IsPaneOpen = true;
                HamburgerButton.Visibility = Visibility.Collapsed;
                HamburgerButton.IsEnabled = false;
                MealImage.MinWidth = 400;
                MealImage.MaxWidth = 800;

                UpButton.IsEnabled = false;
                UpButton.Visibility = Visibility.Collapsed;

                DownButton.IsEnabled = false;
                DownButton.Visibility = Visibility.Collapsed;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if(MensaListView.SelectedIndex < MensaListView.Items.Count-1)
            {
                MensaListView.SelectedIndex += 1;
            }
            else
            {
                MensaListView.SelectedIndex = 0;
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (MensaListView.SelectedIndex > 0)
            {
                MensaListView.SelectedIndex -= 1;
            }
            else
            {
                MensaListView.SelectedIndex = MensaListView.Items.Count - 1;
            }
        }
    }
}
