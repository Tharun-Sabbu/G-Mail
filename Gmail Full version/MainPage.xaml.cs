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
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;
using System.Device.Location;
using Microsoft.Advertising.Mobile.UI;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using System.Windows.Threading;
using System.Threading;
using Gmail_Full_version.Framework;
using Gmail_Full_version.ViewModels;
using WaitSpinner;



namespace Gmail_Full_version
{
    public partial class MainPage : PhoneApplicationPage, IDisposable
    {
        // Constructor
        private double _dragDistanceToOpen = 75.0;
        private double _dragDistanceToClose = 305.0;
        private double _dragDistanceNegative = -75.0;
        private bool _isSettingsOpen = false;
        private FrameworkElement _feContainer;
        private const string APPLICATION_ID = "";
        private const string AD_UNIT_ID = "";
        private AppSettings settings = new AppSettings();
        private GeoCoordinateWatcher gcw = null;
        DispatcherTimer dt = new DispatcherTimer();
        ProgressIndicator test;
        private const string _key = "url";
        public MainPage()
        {
            
            InitializeComponent();
            App.Current.UnhandledException+=new EventHandler<ApplicationUnhandledExceptionEventArgs>(Current_UnhandledException);
            DataContext = App.ViewModel;

            _feContainer = this.Container as FrameworkElement;
            dt.Interval = TimeSpan.FromSeconds(1.0);
            dt.Tick += delegate(object s, EventArgs e)
            {
                dt.Stop();
            };
            this.gcw = new GeoCoordinateWatcher();
            this.gcw.PositionChanged +=new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
            this.gcw.Start();
           
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            if (!IsWirelessEnabled() && !IsCellularEnabled())
            {
                MessageBox.Show("Network unavailable");
                progress1.Visibility = Visibility.Collapsed;
            }

            webBrowser1.Navigating += new EventHandler<NavigatingEventArgs>(webBrowser1_Navigating);
            webBrowser1.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(webBrowser1_Navigated);
            webBrowser1.Navigate(new Uri("http://accounts.google.com/ServiceLogin?service=mail&passive=true&continue=http://mail.google.com/mail/?ui%3Dmobile%26zyp%3Dl&&scc=1&ltmpl=ecobx&nui=5&btmpl=mobile"));

        }
        
        private void GestureListener_OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            try
            {
                if (settingstextbox.Text == "on")
                {
                    if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange > 0 && !_isSettingsOpen)
                    {
                        double offset = _feContainer.GetHorizontalOffset().Value + e.HorizontalChange;
                        if (offset > _dragDistanceToOpen)
                            this.OpenSettings();
                        else
                            _feContainer.SetHorizontalOffset(offset);
                    }

                    if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange < 0 && _isSettingsOpen)
                    {
                        double offsetContainer = _feContainer.GetHorizontalOffset().Value + e.HorizontalChange;
                        if (offsetContainer < _dragDistanceToClose)
                            this.CloseSettings();
                        else
                            _feContainer.SetHorizontalOffset(offsetContainer);
                    }
                }
            }
            catch
            {
            }
        }
        private void SettingsStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            try
            {
                ResetLayoutRoot();
            }
            catch
            {
            }
        }

        private void ResetLayoutRoot()
        {
            try
            {
                if (!_isSettingsOpen)
                    _feContainer.SetHorizontalOffset(0.0);
                else
                    _feContainer.SetHorizontalOffset(300.0);
            }
            catch
            {
            }
        }
        private void GestureListener_OnDragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            try
            {
                if (settingstextbox.Text == "on")
                {
                    if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange > 0 && !_isSettingsOpen)
                    {
                        if (e.HorizontalChange < _dragDistanceToOpen)
                            this.ResetLayoutRoot();
                        else
                            this.OpenSettings();
                    }

                    if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange < 0 && _isSettingsOpen)
                    {
                        if (e.HorizontalChange > _dragDistanceNegative)
                            this.ResetLayoutRoot();
                        else
                            this.CloseSettings();
                    }
                }
            }
            catch
            {
            }

        }
        private void CloseSettings()
        {
            try
            {
                var trans = _feContainer.GetHorizontalOffset().Transform;
                trans.Animate(trans.X, 0, TranslateTransform.XProperty, 300, 0, new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                });

                _isSettingsOpen = false;
            }
            catch
            {
            }
        }

        private void OpenSettings()
        {
            try
            {
                var trans = _feContainer.GetHorizontalOffset().Transform;
                trans.Animate(trans.X, 333, TranslateTransform.XProperty, 333, 0, new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                });

                _isSettingsOpen = true;
            }
            catch
            {
            }
        }
        void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            try
            {
                this.gcw.Stop();

                adControl1.Latitude = e.Position.Location.Latitude;
                adControl1.Longitude = e.Position.Location.Longitude;

                Debug.WriteLine("Device lat/long: " + e.Position.Location.Latitude + ", " + e.Position.Location.Longitude);
            }
            catch
            {
            }
        }



        void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            progress1.Visibility = Visibility.Collapsed;
            
        }

        void webBrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            try
            {
                string site;
                site = textbox1.Text;
                textbox1.Text = e.Uri.ToString();

                progress1.Visibility = Visibility.Visible;
                if (e.Uri.ToString().Contains("mail-attachment"))
                {
                    /* e.Cancel = true;
                     string url = e.Uri.ToString();
                
                     if (url.Contains("?"))
                     {
                         url = url + "&!!!";
                   
                     }
                     else
                     {
                         url = url + "?!!!";
                    
                     }
                     webBrowser1.Navigate(new Uri(url), null, "User-Agent:Mozilla/5.0(compatible;MSIE 9.0; Windows NT 6.1; Trident/5.0; XBLWP7; ZuneWP7");
                   /*  NavigationService.Navigate(new Uri("/agent.xaml?text=" + textbox1.Text, UriKind.RelativeOrAbsolute));
                   /*  webBrowser1.Navigate(new Uri(textbox1.Text), null, "User-Agent:Mozilla.5.0(Linux; U; Android 2.2.1;en-us;Nexus One Build/FRG83) AppleWebKit/533.1 (KHTML,like Gecko) Version/4.0 Mobile Safari/533.1");
               
                     Dispatcher.BeginInvoke(() =>
                      {
                      Thread.Sleep(100);
                      });  */
                    try
                    {
                        WebBrowserTask webBrowserTask = new WebBrowserTask();
                        webBrowserTask.Uri = new Uri(textbox1.Text, UriKind.RelativeOrAbsolute);
                        webBrowserTask.Show();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }


        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedFrom(e);
                App app = Application.Current as App;
                app.storeValue = textbox1.Text;
                if (!App.ViewModel.IsDataLoaded)
                    App.ViewModel.LoadData();
            }
            catch
            {
            }
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
           
          
                
           
            settings.save();
        }    
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    IsolatedStorageFile myIsolatedStoragesetting = IsolatedStorageFile.GetUserStoreForApplication();
                    IsolatedStorageFileStream fileStream = myIsolatedStoragesetting.OpenFile("mysett.txt", FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        this.settingstextbox.Text = reader.ReadLine();
                    }
                }
                catch
                {
                }
               
                if (!string.IsNullOrEmpty(APPLICATION_ID) && !string.IsNullOrEmpty(AD_UNIT_ID))
                {
                    adControl1 = new Microsoft.Advertising.Mobile.UI.AdControl(APPLICATION_ID, AD_UNIT_ID, true);
                }
                else
                {
                    adControl1.Visibility = Visibility.Collapsed;

                }
                test = Microsoft.Phone.Shell.SystemTray.ProgressIndicator;
                
              
            }
            catch
            {

            }
        }

        void Current_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void back_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {
                webBrowser1.InvokeScript("eval", "history.go(-1)");

                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void inbox_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {


                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/yluztu4ffmlx-/?&", UriKind.RelativeOrAbsolute));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void forward_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {
                webBrowser1.InvokeScript("eval", "history.go(1)");
                progress1.Visibility = Visibility.Visible;

            }
            catch
            {
            }
        }

        private void attach_Click(object sender, System.EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/Page1.xaml", UriKind.RelativeOrAbsolute));
            }
            catch
            {
            }
        }

        private void refresh_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {
                webBrowser1.InvokeScript("eval", "history.go()");
                progress1.Visibility = Visibility.Visible;

            }
            catch
            {
            }
        }

        private void downattach_Click(object sender, System.EventArgs e)
        {
            try
            {
                string site = textbox1.Text;
                string url = HttpUtility.UrlEncode(site);
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = new Uri(site, UriKind.RelativeOrAbsolute);
                webBrowserTask.Show();
            }
            catch
            {
            }
        }

        private void drafts_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/k4fpna5y13lj-/?&s=d"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void spam_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/d48r1557gdk4-/?&s=m"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void trash_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/1jt3s5jq0hmvs-/?&s=t"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void starred_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/1tzwn14forkdb-/?&s=r"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void allmail_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/1tzwn14forkdb-/?&s=a"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void contacts_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/sysgaeisz3qo-/?&v=cl"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void personal_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/ike4rwjpdu75-/?&s=l&l=Personal"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void receipts_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/1il33voa45bef-/?&s=l&l=Receipts"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void travel_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/1g86gizn8xghu-/?&s=l&l=Travel"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void work_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/129tma7l3zl3zktv-/?&s=l&l=Work"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void signout_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://accounts.google.com/Logout?service=mail&continue=https://accounts.google.com/ServiceLogin?service%3Dmail%26passive%3Dtrue%26continue%3Dhttps://mail.google.com/mail/x/lz3jfwljpcta/?ui%253Dmobile%2526zyp%253Dl%26scc%3d1%26ltmpl%3Decobx%26hl%3Den%26nui%3D5%26btmpl%3Dmobile&hl=en"));
                progress1.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void orientationon_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
            }
            catch
            {
            }
        }

        private void oreintationoff_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                SupportedOrientations = SupportedPageOrientation.Portrait;
            }
            catch
            {
            }
        }

        private void rate_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {
                MarketplaceReviewTask market = new MarketplaceReviewTask();
                market.Show();
            }
            catch
            {
            }
        }

        private void about_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {
                NavigationService.Navigate(new Uri("/Page4.xaml", UriKind.RelativeOrAbsolute));
            }
            catch
            {
            }
        }

        private void myapps_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {
                MarketplaceSearchTask market = new MarketplaceSearchTask();
                market.SearchTerms = "Aravind Yandrapu";
                market.Show();
            }
            catch
            {
            }
        }

        private void compose_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/43ye1zcrpay-/?&v=b&eot=1&pv=tl&cs=b"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            try
            {

                /*MessageBoxResult res = MessageBox.Show("", "Are you sure you want to exit?", MessageBoxButton.OKCancel);
                if (res == MessageBoxResult.OK)
                {

                    if (NavigationService.CanGoBack)
                    {
                        while (NavigationService.RemoveBackEntry() != null)
                        {
                            NavigationService.RemoveBackEntry();
                        }
                    }
                }
                else
                    if (res == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }*/
                e.Cancel = true;
                if (_isSettingsOpen)
                {
                    this.CloseSettings();
                }
                else
                {
                    if (!dt.IsEnabled)
                    {
                        dt.Start();
                        webBrowser1.InvokeScript("eval", "history.go(-1)");

                    }
                    else
                    {

                        new Microsoft.Xna.Framework.Game().Exit();
                    }
                }
            }
            catch
            {
            }
        }
        private void sent_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {

                webBrowser1.Navigate(new Uri("http://mail.google.com/mail/x/xdb0foggv46a-/?&s=s"));
                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void tips_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            try
            {
                NavigationService.Navigate(new Uri("/Page3.xaml", UriKind.RelativeOrAbsolute));
            }
            catch
            {
            }
        }

        private void adControl1_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Debug.WriteLine("AdControl error: " + e.Error.Message);
        }

        private void adControl1_AdRefreshed(object sender, EventArgs e)
        {
            Debug.WriteLine("AdControl new ad received");
        }
        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.gcw != null)
                {
                    this.gcw.Dispose();
                    this.gcw = null;
                }
            }
        }

        #endregion
        private bool IsWirelessEnabled()
        {
            return DeviceNetworkInformation.IsWiFiEnabled;
        }

        private bool IsCellularEnabled()
        {
            return DeviceNetworkInformation.IsCellularDataEnabled;
        }

       

       

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/navigatetrysetting.xaml", UriKind.RelativeOrAbsolute));
            }
            catch
            {
            }
        }

        private void CheckBoxSetting1_Checked_1(object sender, RoutedEventArgs e)
        {
            try
            {

                SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
                settings.save();
            }
            catch
            {
            }
        }

        private void CheckBoxSetting1_Unchecked_1(object sender, RoutedEventArgs e)
        {
            try
            {

                SupportedOrientations = SupportedPageOrientation.Portrait;
                settings.save();
            }
            catch
            {
            }
        }

        private void setting_Click(object sender, System.EventArgs e)
        {
            try
            {
                ink1.Visibility = Visibility.Visible;
                PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (frame != null)
                {
                    PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
                    if (page != null)
                    {
                        SystemTray.SetIsVisible(page, false);
                    }
                }
                ApplicationBar.IsVisible = false;
            }
            catch
            {
            }

        }

        private void ToggleButton1_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
            }
            catch
            {
            }
        }

        private void ToggleButton1_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                SupportedOrientations = SupportedPageOrientation.Portrait;
            }
            catch
            {
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (frame != null)
                {
                    PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
                    if (page != null)
                    {
                        SystemTray.SetIsVisible(page, true);
                    }
                }
            }
            catch
            {
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (frame != null)
                {
                    PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
                    if (page != null)
                    {
                        SystemTray.SetIsVisible(page, false);
                    }
                }
            }
            catch
            {
            }
        }

        private void roundButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ink1.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void roundButton2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ink1.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

      
        private void textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            string site;
            site = textbox1.Text;
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            using (StreamWriter writeFile = new StreamWriter(new IsolatedStorageFileStream("mymailto.txt", FileMode.Create, FileAccess.Write, myIsolatedStorage)))
            {
                string someTextDat = textbox1.Text;
                writeFile.WriteLine(someTextDat);
                writeFile.Close();
            }
        }
        private void pinlink_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (textbox1.Text != string.Empty)
                {

                    string site = textbox1.Text;

                    ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("url=" + site));


                    // Create the Tile if we didn't find that it already exists.
                    if (TileToFind == null)
                    {
                        StandardTileData NewTileData = new StandardTileData
                        {
                            BackgroundImage = new Uri("gmail.png", UriKind.Relative),

                            Title = "G-Mail Express",
                            BackContent = (string)site,
                        };

                        // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                        ShellTile.Create(new Uri("/MainPage.xaml?" + _key + "=" + site, UriKind.Relative), NewTileData);


                    }



                    else
                    {
                        MessageBox.Show("Tile already exists");
                    }

                }
                else
                {
                    MessageBox.Show("No link to pin");
                }
            }
            catch
            {
                MessageBox.Show("No link to pin");
            }
        }

        private void refr_Click(object sender, System.EventArgs e)
        {
            try
            {
                webBrowser1.InvokeScript("eval", "history.go()");

                progress1.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void exitapp_Click(object sender, System.EventArgs e)
        {
            try
            {
                new Microsoft.Xna.Framework.Game().Exit();
            }
            catch
            {
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/desktop.xaml?id={0}",Guid.NewGuid().ToString()), UriKind.RelativeOrAbsolute));
        }

        private void pc_Click(object sender, System.EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/desktop.xaml", UriKind.RelativeOrAbsolute));
            }
            catch
            {
            }
            }

       

        private void newcompose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {

                this.CloseSettings();
                attchmentink.Visibility = Visibility.Visible;
                
            }
            catch
            {
            }
        }

        private void allow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/Page1.xaml", UriKind.RelativeOrAbsolute));
                attchmentink.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/o856e9jedbxi-/?&v=b&eot=1&pv=tl&cs=b", UriKind.RelativeOrAbsolute));
                attchmentink.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void search_focus(object sender, System.Windows.RoutedEventArgs e)
        {
            search.Foreground = new SolidColorBrush(Colors.White);
            search.Background = new SolidColorBrush(Colors.Gray);
            search.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }

        private void searchbutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/1o60gcyh6u7sr-/?ie=UTF-8&s=q&q="+search.Text+"&nvp_site_mail=Search", UriKind.RelativeOrAbsolute));
                this.CloseSettings();
            }
            catch
            {
            }
        }

       

        private void inboxbutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (logandinbink.Visibility == Visibility.Visible)
                {
                    logandinbink.Visibility = Visibility.Collapsed;
                }
                else
                    logandinbink.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }

        private void refreshbutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                webBrowser1.InvokeScript("eval", "history.go()");
                progress1.Visibility = Visibility.Visible;

            }
            catch
            {
            }
        }

               private void menubutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isSettingsOpen)
                {
                    CloseSettings();
                }
                else
                {
                    OpenSettings();
                }
            }
            catch
            {
            }
        }

               private void inboxshow_Click(object sender, RoutedEventArgs e)
               {
                   try
                   {

                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/jkxy634quwsa-/?&", UriKind.RelativeOrAbsolute));
                       logandinbink.Visibility = Visibility.Collapsed;
                   }
                   catch
                   {
                   }
               }

               private void logout_Click(object sender, RoutedEventArgs e)
               {
                   try
                   {

                       webBrowser1.Navigate(new Uri("http://accounts.google.com/Logout?service=mail&continue=https://accounts.google.com/ServiceLogin?service%3Dmail%26passive%3Dtrue%26continue%3Dhttps://mail.google.com/mail/x/lz3jfwljpcta/?ui%253Dmobile%2526zyp%253Dl%26scc%3d1%26ltmpl%3Decobx%26hl%3Den%26nui%3D5%26btmpl%3Dmobile&hl=en"));
                       logandinbink.Visibility = Visibility.Collapsed;
                   }
                   catch
                   {
                   }
               }

               private void settingstextbox_TextChanged(object sender, TextChangedEventArgs e)
               {
                   try
                   {
                       IsolatedStorageFile myIsolatedStoragesetting = IsolatedStorageFile.GetUserStoreForApplication();
                       using (StreamWriter settingfile = new StreamWriter(new IsolatedStorageFileStream("mysett.txt", FileMode.Create, FileAccess.Write, myIsolatedStoragesetting)))
                       {
                           string setting = settingstextbox.Text;
                           settingfile.WriteLine(setting);
                           settingfile.Close();
                       }
                   }
                   catch
                   {
                   }
               }

               private void slideon_Click(object sender, System.EventArgs e)
               {
                   try
                   {
                       settingstextbox.Text = "on";
                       IsolatedStorageFile myIsolatedStoragesetting = IsolatedStorageFile.GetUserStoreForApplication();
                       using (StreamWriter settingfile = new StreamWriter(new IsolatedStorageFileStream("mysett.txt", FileMode.Create, FileAccess.Write, myIsolatedStoragesetting)))
                       {
                           string setting = settingstextbox.Text;
                           settingfile.WriteLine(setting);
                           settingfile.Close();
                       }
                   }
                   catch
                   {
                   }
               }

               private void slideoff_Click(object sender, System.EventArgs e)
               {
                   try
                   {

                       settingstextbox.Text = "off";
                       IsolatedStorageFile myIsolatedStoragesetting = IsolatedStorageFile.GetUserStoreForApplication();
                       using (StreamWriter settingfile = new StreamWriter(new IsolatedStorageFileStream("mysett.txt", FileMode.Create, FileAccess.Write, myIsolatedStoragesetting)))
                       {
                           string setting = settingstextbox.Text;
                           settingfile.WriteLine(setting);
                           settingfile.Close();
                       }
                   }
                   catch
                   {
                   }
               }

               private void inboxtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/jkxy634quwsa-/?&", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void Starredtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/fd0odl6wn96f-/?&s=r", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void senttext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/12aby7tye1wiv-/?&s=s", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void drafttext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/13jg28hgxx33m-/?&s=d", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void allmailtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/al9vjaeecvji-/?&s=a", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void spamtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/rn1tkz64aq2x-/?&s=m", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void trashtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       
                           webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/xktdqqvpzxhe-/?&s=t", UriKind.RelativeOrAbsolute));
                           this.CloseSettings();
                       
                   }
                   catch
                   {
                   }
               }

               private void Receipts_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/1lpvli7ix816k-/?&s=l&l=Receipts", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void personaltext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/mn5wvqzncog2-/?&s=l&l=Personal", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void Work_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/1rvb5560yxapq-/?&s=l&l=Work", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void Travel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
               {
                   try
                   {
                       webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/x/11rh5lb8iz1ao-/?&s=l&l=Travel", UriKind.RelativeOrAbsolute));
                       this.CloseSettings();
                   }
                   catch
                   {
                   }
               }

               private void tips_CLick(object sender, System.EventArgs e)
               {
                   try
                   {
                       MessageBox.Show("★ Press backkey double times to exit application \n★ When attaching pictures to mail make sure your phone is not connected to Zune Software. \n★ Download attachments by just a click on 'scan and download attachment' in desktop version page.");
                   }
                   catch
                   {
                   }
               }
       
    }
}