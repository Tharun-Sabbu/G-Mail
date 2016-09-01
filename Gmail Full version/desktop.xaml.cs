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
using System.Threading;
using Microsoft.Phone.Tasks;
using Gmail_Full_version.Framework;
using Gmail_Full_version.ViewModels;
using System.IO.IsolatedStorage;
using System.IO;
using WaitSpinner;
using Gmail_Full_version;
using System.Device.Location;
using System.Diagnostics;



namespace Gmail_Full_version
{
    public partial class desktop : PhoneApplicationPage
    {
        private double _dragDistanceToOpen = 75.0;
        private double _dragDistanceToClose = 305.0;
        private double _dragDistanceNegative = -75.0;
        private bool _isSettingsOpen = false;
        private FrameworkElement _feContainer;
        private const string APPLICATION_ID = "";
            private const string AD_UNIT_ID = "";
        private GeoCoordinateWatcher gcw = null;
        public desktop()
        {
            InitializeComponent();
            DataContext = App.ViewModel;

            _feContainer = this.Container as FrameworkElement;
            this.Loaded += new RoutedEventHandler(desktop_Loaded);
            webBrowser1.Navigate(new Uri("https://mail.google.com/mail/?ui=html&zy=h"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
            this.gcw = new GeoCoordinateWatcher();
            this.gcw.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
            this.gcw.Start();
        }
        void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            this.gcw.Stop();

            adControl1.Latitude = e.Position.Location.Latitude;
            adControl1.Longitude = e.Position.Location.Longitude;
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
                App.ViewModel.LoadData();
        }
        private void CloseSettings()
        {
            var trans = _feContainer.GetHorizontalOffset().Transform;
            trans.Animate(trans.X, 0, TranslateTransform.XProperty, 300, 0, new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            });

            _isSettingsOpen = false;
        }

        private void OpenSettings()
        {
            var trans = _feContainer.GetHorizontalOffset().Transform;
            trans.Animate(trans.X, 300, TranslateTransform.XProperty, 300, 0, new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            });

            _isSettingsOpen = true;
            
                
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

        private void SettingsStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            ResetLayoutRoot();
        }

        private void ResetLayoutRoot()
        {
            if (!_isSettingsOpen)
                _feContainer.SetHorizontalOffset(0.0);
            else
                _feContainer.SetHorizontalOffset(300.0);
        }
        void desktop_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(APPLICATION_ID) && !string.IsNullOrEmpty(AD_UNIT_ID))
                {
                    adControl1 = new Microsoft.Advertising.Mobile.UI.AdControl(APPLICATION_ID, AD_UNIT_ID, true);

                }
                else
                {
                    adControl1.Visibility = Visibility.Collapsed;

                }
                try
                {
                    IsolatedStorageFile myIsolatedStorageink = IsolatedStorageFile.GetUserStoreForApplication();
                    IsolatedStorageFileStream inkfileStream = myIsolatedStorageink.OpenFile("myink.txt", FileMode.Open, FileAccess.Read);
                    using (StreamReader inkreader = new StreamReader(inkfileStream))
                    {
                        this.inkpresentertextbox.Text = inkreader.ReadLine();
                    }
                }
                catch
                {
                }
                try
                {
                    IsolatedStorageFile myIsolatedStoragesetting = IsolatedStorageFile.GetUserStoreForApplication();
                    IsolatedStorageFileStream fileStream = myIsolatedStoragesetting.OpenFile("mysett2.txt", FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        this.settingstextbox.Text = reader.ReadLine();
                    }
                }
                catch
                {
                }

               
               
              
            }
            catch
            {
            }
            }

        private void weBrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            try
            {
                progress1.Visibility = Visibility.Visible;

                textBox1.Text = e.Uri.ToString();

                if (e.Uri.ToString().Contains("mail-attachment"))
                {
                    WebBrowserTask webBrowserTask = new WebBrowserTask();
                    webBrowserTask.Uri = new Uri(textBox1.Text, UriKind.RelativeOrAbsolute);
                    webBrowserTask.Show();
                }
                if (!e.Uri.ToString().Contains("==="))
                {

                    textBox1.Text = e.Uri.ToString();
                    string url = e.Uri.ToString();
                    if (e.Uri.ToString().Contains("mail.google.com/mail"))
                    {

                        if (url.Contains("?"))
                            url = url + "&===";
                        else
                            url = url + "?===";


                        webBrowser1.Navigate(new Uri(url), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");


                    }





                }
            }
            catch
            {
            }
        }

        private void weBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            progress1.Visibility = Visibility.Collapsed;
           
            if (inkpresentertextbox.Text == "firstrun")
            {
                ink1.Visibility = Visibility.Visible;
                ink2.Visibility = Visibility.Visible;
                ApplicationBar.IsVisible = false;
            }
            if (ink2.Visibility == Visibility.Visible)
            {
                ApplicationBar.IsVisible = false;
            }
            if (ink2.Visibility == Visibility.Collapsed)
            {
                ApplicationBar.IsMenuEnabled = true;
            }
            if (ink1.Visibility == Visibility.Visible)
            {
                ink2.Visibility = Visibility.Visible;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                inkpresentertextbox.Text = "endrun";
                IsolatedStorageFile myIsolatedStorageink = IsolatedStorageFile.GetUserStoreForApplication();
                using (StreamWriter inkfile = new StreamWriter(new IsolatedStorageFileStream("myink.txt", FileMode.Create, FileAccess.Write, myIsolatedStorageink)))
                {
                    string ink = inkpresentertextbox.Text;
                    inkfile.WriteLine(ink);
                    inkfile.Close();
                }
            }
            catch
            {
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                ink2.Visibility = Visibility.Collapsed;
                ApplicationBar.IsVisible = true;
                inkpresentertextbox.Text = "endrun";
                IsolatedStorageFile myIsolatedStorageink = IsolatedStorageFile.GetUserStoreForApplication();
                using (StreamWriter inkfile = new StreamWriter(new IsolatedStorageFileStream("myink.txt", FileMode.Create, FileAccess.Write, myIsolatedStorageink)))
                {
                    string ink = inkpresentertextbox.Text;
                    inkfile.WriteLine(ink);
                    inkfile.Close();
                }
            }
            catch
            {
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                ink1.Visibility = Visibility.Collapsed;
                ink2.Visibility = Visibility.Collapsed;
                ApplicationBar.IsVisible = true;
                inkpresentertextbox.Text = "endrun";
                IsolatedStorageFile myIsolatedStorageink = IsolatedStorageFile.GetUserStoreForApplication();
                using (StreamWriter inkfile = new StreamWriter(new IsolatedStorageFileStream("myink.txt", FileMode.Create, FileAccess.Write, myIsolatedStorageink)))
                {
                    string ink = inkpresentertextbox.Text;
                    inkfile.WriteLine(ink);
                    inkfile.Close();
                }
            }
            catch
            {
            }
        }
        

        private void exit_Click(object sender, System.EventArgs e)
        {
            try
            {
                new Microsoft.Xna.Framework.Game().Exit();
            }
            catch
            {
            }
        }

        private void back_Click(object sender, System.EventArgs e)
        {
            try
            {
                webBrowser1.InvokeScript("eval", "history.go(-1)");
                progress1.Visibility = Visibility.Visible;

            }
            catch
            {
            }
        }

        private void forward_Click(object sender, System.EventArgs e)
        {
            try
            {
                webBrowser1.InvokeScript("eval", "history.go(1)");
                progress1.Visibility = Visibility.Visible;

            }
            catch
            {
            }
        }

        private void rate_Click(object sender, System.EventArgs e)
        {
            try
            {
                // TODO: Add event handler implementation here.
                MarketplaceReviewTask market = new MarketplaceReviewTask();
                market.Show();
            }
            catch
            {
            }
        }

        private void mob_Click(object sender, System.EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }
            catch
            {
            }
            }

        private void compose_Click(object sender, RoutedEventArgs e)
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

        private void inkpresentertextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                IsolatedStorageFile myIsolatedStorageink = IsolatedStorageFile.GetUserStoreForApplication();
                using (StreamWriter inkfile = new StreamWriter(new IsolatedStorageFileStream("myink.txt", FileMode.Create, FileAccess.Write, myIsolatedStorageink)))
                {
                    string ink = inkpresentertextbox.Text;
                    inkfile.WriteLine(ink);
                    inkfile.Close();
                }
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
                using (StreamWriter settingfile = new StreamWriter(new IsolatedStorageFileStream("mysett2.txt", FileMode.Create, FileAccess.Write, myIsolatedStoragesetting)))
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
                using (StreamWriter settingfile = new StreamWriter(new IsolatedStorageFileStream("mysett2.txt", FileMode.Create, FileAccess.Write, myIsolatedStoragesetting)))
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
                using (StreamWriter settingfile = new StreamWriter(new IsolatedStorageFileStream("mysett2.txt", FileMode.Create, FileAccess.Write, myIsolatedStoragesetting)))
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

                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/o4ktsvq7f0xf/?&v=b&s=q&q=he&pv=tl&cs=b"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                    
                
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

        private void searchbutton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/14zntbxpgboht/?s=q&q=" + search.Text + "&nvp_site_mail=Search+Mail"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
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

        private void inboxtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/xv23knbxs7db/?&"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void Starredtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/d9oqvouj1q80/?&s=r"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void senttext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/t1cm1wxxuzi/?&s=s"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void drafttext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/fdmp0gsy5397/?&s=d"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void allmailtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/1tuyjrdp91ktm/?&s=a"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void spamtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/qqp3wzbynesl/?&s=m"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void trashtext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/ohr401hs0apk/?&s=t"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void Receipts_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/19i1nm1ef7zqw/?&s=l&l=Receipts"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void personaltext_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/mwn3i29f9jpq/?&s=l&l=Personal"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void Contacts_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/9rq1m75s5dc5/?&v=cl"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void Work_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/6egztyt5lp63/?&s=l&l=Work"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

        private void Travel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/15n44qjse5lmi/?&s=l&l=Travel"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
            }
            catch
            {
            }
        }

       

        private void inboxshow_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                webBrowser1.Navigate(new Uri("https://mail.google.com/mail/u/0/h/xv23knbxs7db/?&"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
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
                webBrowser1.Navigate(new Uri("https://accounts.youtube.com/accounts/Logout2?hl=en&service=mail&ilo=1&ils=s.youtube%2Cs.IN&ilc=0&continue=https%3A%2F%2Faccounts.google.com%2FServiceLogin%3Fservice%3Dmail%26passive%3Dtrue%26rm%3Dfalse%26continue%3Dhttps%3A%2F%2Fmail.google.com%2Fmail%2F%26ss%3D1%26scc%3D1%26ltmpl%3Ddefault%26ltmplcache%3D2%26hl%3Den%26emr%3D1&zx=-343110144"), null, "User-Agent:Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; ARM; Touch; WPDesktop)");
                CloseSettings();
                logandinbink.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void adControl1_AdRefreshed(object sender, EventArgs e)
        {
            Debug.WriteLine("AdControl new ad received");
        }

        private void adControl1_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Debug.WriteLine("AdControl error: " + e.Error.Message);
        }
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
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
           
                
                if (_isSettingsOpen)
                {
                    this.CloseSettings();
                   e.Cancel = true; 
                }
                
        }
        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/Page4.xaml", UriKind.RelativeOrAbsolute));
            }
            catch
            {
            }
        }

        private void tips_Click(object sender, System.EventArgs e)
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

//Opera/9.80 (Windows NT 6.0) Presto/2.12.388 Version/12.14