using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Text.RegularExpressions;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

namespace WikiParser
{
    public class Article
    {
        public string ArticleName { set; get; }
        public string MainContent { set; get; }
        public string ArticleLink { set; get; }
        public List<string> ImageLinks { set; get; }    
    }

    public class WikiArticleParser
    {
       public delegate void ArticleDownHanlder();
       public event ArticleDownHanlder Changedevent;

       private string language;
       public string ArticleName {set;get;}

       public string artlinkname{set;get;}

       public string MainContent{set;get;}
       public string ArticleLink { set; get; }
       public List<string> ImageLinks { set; get; }
       public bool isReady = false;
       private bool addArticle = true; 

       public WikiArticleParser(string name, string body, string link, List<string> images)
       {
           ArticleName = name;
           ArticleLink = link;
           MainContent = body;
           ImageLinks = images;
           isReady = true;
       }

        public WikiArticleParser(string language, bool addToQueue)
        {
            ImageLinks = new List<string>();
            this.language = language;
            GetNewArticle(language);
            addArticle = addToQueue;
        }

        public void GetNewArticle(string lang)
        {
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            client.DownloadStringAsync(new Uri(string.Format("http://{0}.wikipedia.org/wiki/Special:Random", lang)));
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            
            string page = "";
            try
            {
                page=e.Result;
            }
            catch {
                GetNewArticle(language);
                return;
            }
            page = HttpUtility.HtmlDecode(page);
            // "wgPageName": "Velika_nagrada_Nîmesa_1933", "wgTitle":
            Regex guidRegex = new Regex("wgPageName\":\\s?\"[\\w-_%()–\\S—]*\",");
            Match m = guidRegex.Match(page);           

            string articleName = "";

            if (m.Value != "")
            {
                articleName = m.Value;
            }
            try
            {
                int where1 = GetNthIndex(articleName, '\"', 2);
                int where2 = GetNthIndex(articleName, '\"', 3);

                int len = articleName.Length - where1  - (articleName.Length - where2 + 1);

                articleName = articleName.Substring(where1 + 1, len);
            }
            catch {
                GetNewArticle(language);
                return;
            }
            ArticleLink = string.Format("http://{0}.wikipedia.org/wiki/{1}", language, articleName);
            Regex Regex2 = new Regex("\"wgTitle\":\\s?\"[\\w-_,%()\\s\\.–(\\w:)'(/(?<!\\)(?>\\\\)*/)°—™+]+\",");
            Match m2 = Regex2.Match(page);
            
            string article2Name = "";

            if (m2.Value != "")
            {
                article2Name = m2.Value;
            }

            if (string.IsNullOrEmpty(article2Name))
            {
                GetNewArticle(language);
                return;
            }
            int where = GetNthIndex(article2Name, '\"', 3);

            article2Name = article2Name.Substring(where + 1, article2Name.Length - where - 1 - (article2Name.Length - article2Name.LastIndexOf("\",")));

            ArticleName = article2Name;

            artlinkname = articleName;

            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted22);
            client.DownloadStringAsync(new Uri(string.Format("http://{0}.wikipedia.org/w/api.php?action=query&prop=revisions&titles={1}&rvprop=content&redirects&format=xml", language, artlinkname)));
        }

        public int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }


        void client_DownloadStringCompleted22(object sender, DownloadStringCompletedEventArgs e)
        {   
            string page="";
            try
            {
                page = e.Result;
            }
            catch {
                return;
            }

            //our articles have to have pictures in them
            Regex Regex123 = new Regex("\\W[\\w]+:[\\w\\s_-]+(.jpg|.png|.svg)", RegexOptions.IgnoreCase);
            Match m = Regex123.Match(page);

            if (!m.Success)
            {
                GetNewArticle(language);
                return;
            }

            Regex Regex2 = new Regex("[\\w^,]+");
            Match m2 = Regex2.Match(ArticleName);

            string firstWord = "";

            if (m2.Value != "")
            {
                firstWord = m2.Value;
            }
            

            Regex t = new Regex("[\\w\\s']*'''" + firstWord, RegexOptions.IgnoreCase);
            string bla = "";
            foreach (var s in t.Matches(page))
            {
                bla = s.ToString();
                break;
            }

            if (bla == "")
            {
                GetNewArticle(language);
                return;
            }

            //gets the main description for the article
            int StartAt = page.LastIndexOf(bla);

            if (StartAt == -1)
            {
                GetNewArticle(language);
                return;
            }

            page = page.Substring(StartAt);

            int EndAt = page.IndexOf("<ref>");
            if (EndAt == -1)
                EndAt = page.IndexOf("==");
            if (EndAt == -1)
                EndAt = page.IndexOf("{{");

            if (EndAt == -1)
            {
                GetNewArticle(language);
                return;
            }

            string mainContent = page.Substring(0, EndAt);

            if (mainContent.Length < 250)
            {
                GetNewArticle(language);
                return;
            }

            MainContent = mainContent;
            MainContent = HttpUtility.HtmlDecode(MainContent);
            try
            {
                Regex regex = new Regex(@"<[^>]+>[^<]*</[^>]+>");
                while (regex.IsMatch(MainContent))
                {
                    MainContent = regex.Replace(MainContent, "");
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

            try
            {
                Regex regex = new Regex(@"<[^<]+/>");
                while (regex.IsMatch(MainContent))
                {
                    MainContent = regex.Replace(MainContent, "");
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

            if (MainContent == null)
                MainContent = mainContent;            

            GetFileLinks(e.Result);
        }

        private IsolatedStorageSettings userSettings =
                   IsolatedStorageSettings.ApplicationSettings;

        private void Changed()
        {
            isReady = true;

            parseMainContent();
            lock (IsolatedStorageSettings.ApplicationSettings)
            {
                try
                {
                    if (addArticle)
                    {
                        Article art = new Article();
                        art.ArticleLink = ArticleLink;
                        art.ArticleName = ArticleName;
                        art.MainContent = MainContent;
                        art.ImageLinks = ImageLinks;

                        List<Article> articles = new List<Article>();
                        try
                        {
                            articles = (List<Article>)userSettings["Articles"];
                        }
                        catch { }

                        articles.Add(art);

                        UpdateOrAdd(userSettings, "Articles", articles);
                    }
                }
                catch { }

                //userSettings.Save();
            }
            if (Changedevent != null)            
                Changedevent();
        }

        private void parseMainContent()
        {
            //[\\w\\s-_|^[]+]]
            Regex t = new Regex("(\\[\\[[^\\[]+\\]\\])|('''[\\w\\s,\\.]+''')", RegexOptions.IgnoreCase);

            List<string> used = new List<string>();
            MainContent = MainContent.Replace("'", "");
            MainContent = HttpUtility.HtmlEncode(MainContent);
            MainContent = MainContent.Replace("&amp;quot;", "\'");
            MainContent = MainContent.Replace("&amp;nbsp;", "");
            MainContent = MainContent.Replace("&amp;ndash;", "-");
            MainContent = MainContent.Replace("&quot;", "\'");
            MainContent = MainContent.Replace("&nbsp;", "");
            MainContent = MainContent.Replace("&ndash;", "-");
            //MainContent = HttpUtility.HtmlDecode(MainContent);

            try
            {
                Regex regex = new Regex(@"(<[^<]+>)|({{[^{]+}})|(<!--[^<]+-->)");
                while (regex.IsMatch(MainContent))
                {
                    MainContent = regex.Replace(MainContent, "");
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }
           

            foreach (var s in t.Matches(MainContent))
            {
                bool doReplace = true;

                foreach (var b in used)
                {
                    if (b.Equals(s.ToString()))
                        doReplace = false;
                }

                if (doReplace)
                {
                    used.Add(s.ToString());

                    string ss=s.ToString();
                    
                    string[] split = ss.Substring(2, ss.Length - 4).Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length == 1)
                        MainContent = MainContent.Replace(s.ToString(), "<Run FontWeight=\"Bold\" Text=\"" + split[0] + "\"/>");
                    else
                        MainContent = MainContent.Replace(s.ToString(), "<Run FontWeight=\"Bold\" Text=\"" + split[1] + "\"/>");
                    
                }
            }

            // Build text with highlights

            string textBlockStr = "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" FontSize=\"25\" TextWrapping=\"Wrap\" VerticalAlignment=\"Top\">";

            textBlockStr += MainContent;

            textBlockStr += "</TextBlock>";
           // textBlockStr = textBlockStr.Replace("&nbsp;", "");

            MainContent = textBlockStr;
        }

        public void UpdateOrAdd(IsolatedStorageSettings obj, string key, object value)
        {
            if (obj.Contains(key))
            {
                obj[key] = value;
            }
            else
            {
                obj.Add(key, value);
            }
        }

        int howmany = 0;

        private void GetFileLinks(string page)
        {
            Regex Regex2 = new Regex("\\W[\\w]+:[\\w\\s_-]+(.jpg|.png|.svg)", RegexOptions.IgnoreCase); 
            Match m = Regex2.Match(page);

            if (!m.Success)
            {
                GetNewArticle(language);
                return;
            }

            while (m.Success)
            {
                howmany++;
                int where = m.Value.LastIndexOf(":");
                string what = m.Value.Substring(0, where);

                string val = m.Value.Replace(what, "File");
                string file = HttpUtility.HtmlEncode(val.Replace(" ", "_"));
                WebClient client = new WebClient();
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted3);
                client.DownloadStringAsync(new Uri(string.Format("http://commons.wikimedia.org/w/api.php?action=query&titles={0}&prop=imageinfo&iiprop=url&format=xml", file)));
                m = m.NextMatch();

                if (howmany == 10)
                    break;
            }
            areAlldownloaded();
        }
        private void areAlldownloaded()
        {
            if(ImageLinks.Count==howmany)
                Changed();
        }

        void client_DownloadStringCompleted3(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Regex Regex23 = new Regex("url=\"http://upload.wikimedia.org/wikipedia/commons/[\\w\\W]+\" de");
                Match mcc = Regex23.Match(e.Result);

                string link = "";
                if (mcc.Value != null)
                {
                    link = mcc.Value.Substring(5, mcc.Value.Length - 5 - 4);
                    link = link.Replace("commons/", "commons/thumb/");

                    Regex Regex233 = new Regex("/[\\w-_%]+(.jpg|.png|.svg)", RegexOptions.IgnoreCase);
                    Match mccc = Regex233.Match(HttpUtility.HtmlDecode(link));

                    link += "/150px-" + mccc.Value.Substring(1);

                    ImageLinks.Add(link);
                }
            }
            catch { }
            areAlldownloaded();
        }
    }
}