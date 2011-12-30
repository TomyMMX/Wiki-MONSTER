using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using WikiParser;
using TileScheduler;


namespace WikiMonster
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ItemViewModel> Items { get; private set; }

        public WikiArticleParser wap;

        public delegate void ArticleDownHanlder(List<string> imageLinks);
        public event ArticleDownHanlder LoadImagesEvent;

        private string _ArticleBody = "";

        public string ArticleBody
        {
            get
            {
                return _ArticleBody;
            }
            set
            {
                if (value != _ArticleBody)
                {
                    _ArticleBody = value;
                    NotifyPropertyChanged("ArticleBody");
                }
            }
        }

        private string _ArticleName = "";

        public string ArticleName
        {
            get
            {
                return _ArticleName;
            }
            set
            {
                if (value != _ArticleName)
                {
                    _ArticleName = value;
                    NotifyPropertyChanged("ArticleName");
                }
            }
        }

        private string _ArticleLink = "";

        public string ArticleLink
        {
            get
            {
                return _ArticleLink;
            }
            set
            {
                if (value != _ArticleLink)
                {
                    _ArticleLink = value;
                    NotifyPropertyChanged("ArticleLink");
                }
            }
        }

        public bool IsDataLoaded
        {
            get;
            set;
        }

        public void setWap(string name, string body, string link, List<string> images)
        {
            wap = new WikiArticleParser(name, body, link, images);
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {           
                if (wap!=null)
                    wap_Changedevent();
                else
                {
                    wap = new WikiArticleParser(HelperMetjods.GetLanguage(), false);
                    wap.Changedevent += new WikiArticleParser.ArticleDownHanlder(wap_Changedevent);
                }

            this.IsDataLoaded = true;
        }

        void wap_Changedevent()
        {        
                LoadImagesEventP();            
        }

        private void LoadImagesEventP()
        {
            if (LoadImagesEvent != null)
                LoadImagesEvent(wap.ImageLinks);
        }		
						
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}