using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Linq;
using System;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Net.NetworkInformation;

namespace TileScheduler
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private WikiParser.WikiArticleParser wap;
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            if (task is PeriodicTask)
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    NotifyComplete();
                    return;
                }         

                wap = new WikiParser.WikiArticleParser(HelperMetjods.GetLanguage(), false);
                wap.Changedevent+=new WikiParser.WikiArticleParser.ArticleDownHanlder(wap_Changedevent);
            }           
        }
        private IsolatedStorageSettings userSettings =
                   IsolatedStorageSettings.ApplicationSettings;
        void wap_Changedevent()
        {
            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault();
            Uri bg = null;
            try
            {
                bg = new Uri(wap.ImageLinks[0].Replace("150px", "250px"), UriKind.Absolute);
            }
            catch
            {
                bg = new Uri("http://dl.dropbox.com/u/109923/wiki.png", UriKind.Absolute);
            }

            if (wap.ArticleName.Length > 15)
                wap.ArticleName = wap.ArticleName.Substring(0, 15) + "...";

            HelperMetjods.UpdateOrAdd(userSettings, "ArticleBody", wap.MainContent);
            HelperMetjods.UpdateOrAdd(userSettings, "ArticleName", wap.ArticleName);
            HelperMetjods.UpdateOrAdd(userSettings, "ArticleLink", wap.ArticleLink);
            HelperMetjods.UpdateOrAdd(userSettings, "ArticleImages", wap.ImageLinks);

            //test if Tile was created
            if (TileToFind != null)
            {
                TileToFind.Update(new StandardTileData
                {
                    Title = wap.ArticleName,
                    BackgroundImage = bg,
                    Count = 0,
                    BackTitle = wap.ArticleName,
                    BackBackgroundImage = new Uri("http://dl.dropbox.com/u/109923/wiki.png", UriKind.Absolute)
                });
            }

            NotifyComplete();
        }
    }
}