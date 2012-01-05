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
using Microsoft.Phone.Tasks;

namespace WikiMonster
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        private void goToLuknja(object sender, System.Windows.RoutedEventArgs e)
        {
        	var wbt = new WebBrowserTask();
                wbt.Uri = new Uri("http://www.luknja.com");
                wbt.Show();
        }

        private void showPrivacy(object sender, System.Windows.RoutedEventArgs e)
        {
        	Uri i = new Uri("/Privacy.xaml", UriKind.Relative);
            NavigationService.Navigate(i);
        }

        private void reviewApp_Click(object sender, RoutedEventArgs e)
        {
            var task = new MarketplaceReviewTask();
            task.Show();
        }
    }
}
