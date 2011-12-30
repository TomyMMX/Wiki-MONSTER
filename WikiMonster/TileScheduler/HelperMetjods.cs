using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using WikiParser;
using Microsoft.Phone.Net.NetworkInformation;
using System.Collections.Generic;

namespace TileScheduler
{
    public static class HelperMetjods
    {
        private static IsolatedStorageSettings userSettings =
                 IsolatedStorageSettings.ApplicationSettings;
        public static void UpdateOrAdd(IsolatedStorageSettings obj, string key, object value)
        {
            if (obj.Contains(key))
            {
                obj[key] = value;
            }
            else
            {
                obj.Add(key, value);
            }

            obj.Save();
        }

        public static void UpdateCurrentArticleStorage(WikiParser.WikiArticleParser w)
        {
            HelperMetjods.UpdateOrAdd(userSettings, "ArticleBody", w.MainContent);
            HelperMetjods.UpdateOrAdd(userSettings, "ArticleName", w.ArticleName);
            HelperMetjods.UpdateOrAdd(userSettings, "ArticleLink", w.ArticleLink);
            HelperMetjods.UpdateOrAdd(userSettings, "ArticleImages", w.ImageLinks);
        }

        public static void getFutureArticles()
        {
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
                for (int i = articles.Count; i < maks; i++)
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        WikiParser.WikiArticleParser wep = new WikiArticleParser(HelperMetjods.GetLanguage(), true);
                    }
                }
            }
        }

        public static string GetLanguage()
        {

            bool isEnglihs=false;
            try
            {
                isEnglihs = (bool)userSettings["lang"];
            }
            catch { }

            if (isEnglihs)
                return "en";
            else
                return System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        }

    }
}
