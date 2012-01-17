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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

namespace WikiMonster
{
    public partial class ImageShow : PhoneApplicationPage
    {
        private IsolatedStorageSettings userSettings =
                   IsolatedStorageSettings.ApplicationSettings;
        public ImageShow()
        {
            InitializeComponent();
            
            Uri uri = new Uri((String)userSettings["BigImageLink"], UriKind.Absolute);
            BigImage.Source = new BitmapImage(uri);
        }
    }
}