using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using ShakeGestures;
using WikiParser;
using System.Windows.Markup;
using System.Threading;
using TileScheduler;
using Microsoft.Phone.Net.NetworkInformation;

namespace WikiMonster
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            if (ApplicationBar.MenuItems.Count == 2 && System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en")
                this.ApplicationBar.MenuItems.RemoveAt(0);

            ShakeGesturesHelper.Instance.ShakeGesture += new EventHandler<ShakeGestureEventArgs>(Instance_ShakeGesture);
            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            App.ViewModel.LoadImagesEvent += new MainViewModel.ArticleDownHanlder(ViewModel_LoadImagesEvent);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            this.DoubleTap += new EventHandler<System.Windows.Input.GestureEventArgs>(MainPage_DoubleTap);
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 5;
            ShakeGesturesHelper.Instance.ShakeMagnitudeWithoutGravitationThreshold = 0.25;

            ShakeGesturesHelper.Instance.Active = true;
        }

        void MainPage_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
           // throw new NotImplementedException();
        }

        void Instance_ShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            try
            {
                List<Article> articles = new List<Article>();
                Article art = new Article();
                try
                {
                    articles = (List<Article>)userSettings["Articles"];
                    art = articles[0];
                    articles.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        offlineGrid.Visibility = Visibility.Visible;
                    }
                    return;
                }

                HelperMetjods.UpdateOrAdd(userSettings, "Articles", articles);

                var w = new WikiParser.WikiArticleParser(art.ArticleName, art.MainContent, art.ArticleLink, art.ImageLinks);

                HelperMetjods.UpdateCurrentArticleStorage(w);

                App.ViewModel.wap = w;
                App.ViewModel.LoadData(); 
            }
            catch (Exception ex)
            {

            }
        }               

        private ShellTileSchedule SampleTileSchedule = new ShellTileSchedule();
        
        private IsolatedStorageSettings userSettings = 
                    IsolatedStorageSettings.ApplicationSettings;

        private bool loaded = false;

        void ViewModel_LoadImagesEvent(List<string> imageLinks)
        {
            this.Dispatcher.BeginInvoke(
            () =>
            {
                loaded = true;
                firstBootgrid.Visibility = Visibility.Collapsed;

                App.ViewModel.ArticleName = App.ViewModel.wap.ArticleName;
                App.ViewModel.ArticleBody = App.ViewModel.wap.MainContent;
                App.ViewModel.ArticleLink = App.ViewModel.wap.ArticleLink;

                try
                {
                    if (ContentStack.Children.Count == 3)
                    {
                        ContentStack.Children.RemoveAt(0);
                    }

                    TextBlock textBlock = (XamlReader.Load(App.ViewModel.ArticleBody) as TextBlock);
                    ContentStack.Children.Insert(0, textBlock);
                    
                }
                catch { }

                ImageStack.Children.Clear();
                int i = 0;
                foreach (var link in imageLinks)
                {
                    Image image1 = new Image();

                    image1.Stretch = Stretch.Uniform;
                    image1.Width = 150;
                    Uri uri = new Uri(link, UriKind.Absolute);
                    image1.Source = new BitmapImage(uri);

                    ImageStack.Children.Add(image1);
                    i++;

                    if (i == 10)
                        break;
                }
            });

            List<Article> articles = new List<Article>();
            try
            {
                articles = (List<Article>)userSettings["Articles"];
            }
            catch
            {

            }

            int maks = 15;
            if (articles.Count < maks)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    WikiParser.WikiArticleParser wep = new WikiArticleParser(HelperMetjods.GetLanguage(), true);
                }
            }
        }

        private void wep_Changedevent()
        {
            if(!loaded)
            {
                lock (IsolatedStorageSettings.ApplicationSettings)
                {
                    var articles = new List<Article>();
                    var art = new Article();
                    try
                    {
                        articles = (List<Article>)userSettings["Articles"];
                        art = articles[0];
                        articles.RemoveAt(0);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    loaded = true;
                    HelperMetjods.UpdateOrAdd(userSettings, "Articles", articles);

                    WikiParser.WikiArticleParser w = new WikiParser.WikiArticleParser(art.ArticleName, art.MainContent, art.ArticleLink, art.ImageLinks);
                    HelperMetjods.UpdateCurrentArticleStorage(w);

                    userSettings.Save();
                    App.ViewModel.wap = w;
                }
                

                loadData();
            }            
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        

            loadData();

            Thread t = new Thread(() => {
                while(!loaded)
                {
                   
                    Thread.Sleep(5000);
                    wep_Changedevent();
                }
            });
            t.Start();

            HelperMetjods.getFutureArticles();
        }

       

        private void loadData()
        {
            this.Dispatcher.BeginInvoke(
            () =>
            {
            try
            {
                App.ViewModel.ArticleBody = (string)userSettings["ArticleBody"];
                App.ViewModel.ArticleName = (string)userSettings["ArticleName"];
                App.ViewModel.ArticleLink = (string)userSettings["ArticleLink"];
                App.ViewModel.setWap(App.ViewModel.ArticleName, App.ViewModel.ArticleBody, App.ViewModel.ArticleLink, (List<string>)userSettings["ArticleImages"]);
            }
            catch (Exception ex)
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    offlineGrid.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    offlineGrid.Visibility = Visibility.Collapsed;
                    firstBootgrid.Visibility = Visibility.Visible;
                }
            }

            offlineGrid.Visibility = Visibility.Collapsed;

            App.ViewModel.LoadData();
            });
        }

        private void ShowArticleInBrowser(object sender, System.Windows.RoutedEventArgs e)
        {
        	    var wbt = new WebBrowserTask();
                wbt.Uri = new Uri(App.ViewModel.wap.ArticleLink);
                wbt.Show();
        }

        private void openAbout(object sender, System.EventArgs e)
        {
        	Uri i = new Uri("/About.xaml", UriKind.Relative);
            NavigationService.Navigate(i);
        }
		
		 private void openSettings(object sender, System.EventArgs e)
        {
        	Uri i = new Uri("/Settings.xaml", UriKind.Relative);
            NavigationService.Navigate(i);
        }

       
    }
}