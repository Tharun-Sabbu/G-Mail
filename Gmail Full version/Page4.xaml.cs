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

namespace Handling_Facebook
{
    public partial class Page1 : PhoneApplicationPage
    {
        public Page1()
        {
            InitializeComponent();
            Application.Current.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(Current_UnhandledException);
        }

        void Current_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }
      



        

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {

                MarketplaceReviewTask market = new MarketplaceReviewTask();
                market.Show();
            }
            catch
            {
            }
        }

        private void TextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {

                EmailComposeTask emailcomposer = new EmailComposeTask();
                emailcomposer.To = "mailto:";
                emailcomposer.Show();
            }
            catch
            {
            }
        }
    }
}