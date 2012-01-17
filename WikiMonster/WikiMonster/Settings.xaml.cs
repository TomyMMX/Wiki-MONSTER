using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using TileScheduler;
using WikiParser;
using System.Threading;

namespace WikiMonster
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
            try
            {
                LangToggle.IsChecked = (bool)userSettings["lang"];
            }
            catch {
                LangToggle.IsChecked = false;
            }
        }

        private IsolatedStorageSettings userSettings =
                    IsolatedStorageSettings.ApplicationSettings;

        private void ToggleSwitch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            HelperMetjods.UpdateOrAdd(userSettings, "lang", true);
            ResetArticles();
        }

        private void ToggleSwitch_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            HelperMetjods.UpdateOrAdd(userSettings, "lang", false);
            ResetArticles();
        }

        private void ResetArticles()
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    List<Article> articles = new List<Article>();
                    try
                    {
                        articles = (List<Article>)userSettings["Articles"];
                    }
                    catch { }

                    articles.Clear();

                    HelperMetjods.UpdateOrAdd(userSettings, "Articles", articles);

                    HelperMetjods.getFutureArticles();
                }
                catch { }
            });
            t.Start();
        }


    }
}
