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
using System.Text;
using Venetasoft.WP.Net;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Venetasoft;
using System.Net.NetworkInformation;
using System.Device.Location;
using Microsoft.Advertising.Mobile.UI;
using System.Diagnostics;

namespace MailMessageDemo
{


    public partial class MainPage : PhoneApplicationPage,IDisposable
    {
        MailMessage mailMessage = null;
        EmailAddressChooserTask address;
        private GeoCoordinateWatcher gcw = null;
        private const string APPLICATION_ID = "";
        private const string AD_UNIT_ID = "";
        public MainPage()
        {
            InitializeComponent();
            address = new EmailAddressChooserTask();
            address.Completed += new EventHandler<EmailResult>(address_Completed);
          
            this.gcw = new GeoCoordinateWatcher();
            this.gcw.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
            this.gcw.Start();
            Application.Current.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(Current_UnhandledException);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
           
               
            if (mailMessage == null)
            {
                //create MailMessage object
                mailMessage = new MailMessage();

                //License needed from Venetasoft (Purchase it from here:http://www.geekchamp.com/marketplace/)
                mailMessage.LicenceKey = "";

                //set message event handlers
                mailMessage.Error += new EventHandler<Venetasoft.WP7.ErrorEventArgs>(mailMessage_Error);
                mailMessage.MailSent += new EventHandler<Venetasoft.WP7.ValueEventArgs<bool>>(mailMessage_MailSent);
                mailMessage.Progress += new EventHandler<Venetasoft.WP7.ValueEventArgs<int>>(mailMessage_Progress);

            }

            if (this.textBoxMailTo.Text == ";")
            {
                this.textBoxMailTo.Text = "";
            }
           
            //debug info: show attachments that will be added to mail (from resource) => see code in buttonSend_Click()
           
        }

        void address_Completed(object sender, EmailResult e)
        {
            try
            {
               
                if (this.textBoxMailTo.Text == ";")
                {
                    this.textBoxMailTo.Text = "";
                }

                if (e.TaskResult == TaskResult.OK)
                {
                    this.textBoxMailTo.Text = e.Email + ";" + textBoxMailTo.Text;
                }
                if (e.TaskResult == TaskResult.Cancel)
                {
                    this.textBoxMailTo.Text = textBoxMailTo.Text;
                }
                    if (this.textBoxMailTo.Text == ";")
                    {
                        this.textBoxMailTo.Text = "";
                    }
                
                   /* string s = textBoxMailTo.Text;

                    if (s.Length > 1)
                    {
                        s = s.Substring(0, s.Length - 1);
                    }
                   

                    textBoxMailTo.Text = s;*/
                
                   
                
            }
            catch
            {
            }
        }

        void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            this.gcw.Stop();

            adControl1.Latitude = e.Position.Location.Latitude;
            adControl1.Longitude = e.Position.Location.Longitude;
            adControl2.Latitude = e.Position.Location.Latitude;
            adControl2.Longitude = e.Position.Location.Latitude;

            Debug.WriteLine("Device lat/long: " + e.Position.Location.Latitude + ", " + e.Position.Location.Longitude);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(APPLICATION_ID) && !string.IsNullOrEmpty(AD_UNIT_ID))
                {
                    adControl1 = new Microsoft.Advertising.Mobile.UI.AdControl(APPLICATION_ID, AD_UNIT_ID, true);
                    adControl2 = new Microsoft.Advertising.Mobile.UI.AdControl(APPLICATION_ID, AD_UNIT_ID, true); 
                }
                else
                {
                    adControl1.Visibility = Visibility.Collapsed;
                    adControl2.Visibility = Visibility.Collapsed;

                }
                textBoxMailTo.Text = "";
                if (this.textBoxMailTo.Text == ";")
                {
                    this.textBoxMailTo.Text = "";
                }
                try
                {
                    IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                    IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile("myFile.txt", FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        this.textBoxUserName.Text = reader.ReadLine();
                    }
                }
                catch
                {
                }
                try
                {
                    IsolatedStorageFile myIsolatedStorag = IsolatedStorageFile.GetUserStoreForApplication();
                    IsolatedStorageFileStream fileStrea = myIsolatedStorag.OpenFile("mypass.txt", FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStrea))
                    {
                        this.textBoxPassword.Password = reader.ReadLine();
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

        void Current_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        void SendMail()
        {
            try
            {
                #region validation checks
                if (NetworkInterface.GetIsNetworkAvailable() == false)
                {
                    MessageBox.Show("Network is unavailable.");
                    return;
                }
                if (mailMessage != null && mailMessage.Busy == true)
                {
                    MessageBox.Show("Pending operation in progress, please wait..");
                    return;
                }
                if (String.IsNullOrEmpty(textBoxUserName.Text.Trim()))
                {
                    MessageBox.Show("Please insert a valid value in the 'UserName' field (i.e username@gmail.com).");
                    return;
                }
                if (String.IsNullOrEmpty(textBoxPassword.Password))
                {
                    MessageBox.Show("Please insert a valid value in the 'Password' field.");
                    return;
                }
                string[] recipients = textBoxMailTo.Text.Trim().Replace(',', ';').Split(';');
                foreach (var sto in recipients)
                {
                    if (MailMessage.IsValidEmailAddress(sto) == false)
                    {
                        MessageBox.Show("Please insert a valid email address in the 'To' field. \n->Seperate emails by adding ';' symbol. For example, in the 'To' field add emails like trail@gmail.com;trail2@outlook.com ");
                        return;
                    }
                }

               /* if (String.IsNullOrEmpty(textBoxSubject.Text.Trim()))
                {
                    MessageBox.Show("Please insert a valid value in the 'Subject' field.");
                    return;
                }*/
                #endregion

                //in case of large attachments/slow connection, disable phone auto-lock or wifi will be dropped and email sending  will be aborted
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

                mailMessage.UserName = textBoxUserName.Text; //i.e:  username@hotmail.com 
                mailMessage.Password = textBoxPassword.Password;

                mailMessage.AccountType = MailMessage.accountType.Unknown;
                if (textBoxUserName.Text.ToLower().Contains("@gmail"))
                    mailMessage.AccountType = MailMessage.accountType.Gmail;
               
                //else               
                //  mailMessage.SetCustomSMTPServer("smtp.mySmtpMailServer.com", 25, false);  //custom smtp server <=============
              
                if (MailMessage.IsValidEmailAddress(mailMessage.UserName) == true)
                    mailMessage.From = mailMessage.UserName;
                else
                    mailMessage.From = "foo@foo.com";
                

                mailMessage.To = textBoxMailTo.Text; //you can add multiple recipients separated by ';'
                
                    //mailMessage.Cc =  you can add multiple recipients separated by ';'
                //mailMessage.Bcc = you can add multiple recipients separated by ';'
                mailMessage.Subject = textBoxSubject.Text;
                mailMessage.Body = textBoxBody.Text; //text or HTML (body must start with <html> or <!DOCTYPE HTML>)
                //mailMessage.CharSet = "gb2312";  //set for international charset ("gb2312", "big5", etc), default is "utf-8"

               

                mailMessage.Send(); //Asyncronous call

                buttonSend.Content = "ABORT";
                buttonAddAttachment.IsEnabled = false;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                buttonSend.Content = "SEND";
                buttonAddAttachment.IsEnabled = true;
            }
        }

        #region UI handlers
        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mailMessage.Busy == false)
                {
                    
                    string t = this.textBoxMailTo.Text;
                    string lastFour = t.Substring(t.Length - 1, 1);
                    if (lastFour == ";")
                    {
                        string s = textBoxMailTo.Text.Substring(0, textBoxMailTo.Text.Length - 1);
                        textBoxMailTo.Text = s;
                        SendMail();
                    }
                    else
                        SendMail();
                    
                }
                else
                {
                    mailMessage.Abort();
                    listBoxAttachments.Items.Clear();
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }
        
        private void chkHTMLBody_Click(object sender, RoutedEventArgs e)
        {
            if (chkHTMLBody.IsChecked == false)
                this.textBoxBody.Text = "My App can send email with every kind of attachment now!";
            else
                this.textBoxBody.Text = "<html><body>My App can send email <b>with every kind of attachment</b> now!</body></html>";
        }


       

        void ResetUI()
        {
            textBlockProgress.Text = "";
            rectangleProgress.Width = 0;
            buttonAddAttachment.IsEnabled = true;
            buttonSend.Content = "SEND";
            listBoxAttachments.Items.Clear();
            
        }
        #endregion

        #region photo chooser
        private void buttonAddAttachment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask objPhotoChooser = new PhotoChooserTask();
                objPhotoChooser.Completed += new EventHandler<PhotoResult>(PhotoChooseCall);
                objPhotoChooser.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        void PhotoChooseCall(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                #region Copy picture from Library to isolated storage.


                string fileName = "smtp_tmp\\" + System.IO.Path.GetFileName(e.OriginalFileName);
                
                BinaryReader objReader = new BinaryReader(e.ChosenPhoto);
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isStore.DirectoryExists("smtp_tmp") == false) isStore.CreateDirectory("smtp_tmp");

                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages. 
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the image to isolated storage, 4K chunks at a time 
                        while ((bytesRead = e.ChosenPhoto.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }
                #endregion

                mailMessage.AddAttachment(fileName);

                this.listBoxAttachments.Items.Add(System.IO.Path.GetFileName(fileName) + "\r" + " - " + FormatBytes(MailMessage.FileSize(fileName))); //debug
                
            }
        }
        #endregion

        #region mail handlers
        void mailMessage_Progress(object sender, Venetasoft.WP7.ValueEventArgs<int> e)
        {
            try
            {
                this.rectangleProgress.Width = (int)((421 * e.Value) / 100);

                if (e.Value == 0)
                    textBlockProgress.Text = "Connecting...";
                else
                    textBlockProgress.Text = "Sending " + e.Value.ToString() + "%";
            }
            catch (Exception)
            {
            }
        }

        void mailMessage_MailSent(object sender, Venetasoft.WP7.ValueEventArgs<bool> e)
        {
            try
            {
                if (e.Value == false)   //mail not sent         
                {
                    string errMsg = mailMessage.LastError;
                    if (errMsg.Contains("Connection lost") == false && errMsg.Contains("Aborted by user") == false)
                        errMsg += "\r\nLast server response: " + mailMessage.LastServerResponse;

                    MessageBox.Show("Error sending mail: " + errMsg);
                    listBoxAttachments.Items.Clear();

                }
                else
                {
                    MessageBox.Show("Email successfully sent.");

                }

                ResetUI();
                textBoxMailTo.Text = "";
            }
            catch
            {
            }
        }

        void mailMessage_Error(object sender, Venetasoft.WP7.ErrorEventArgs e)
        {
            MessageBox.Show(e.Value, "Error sending mail", MessageBoxButton.OK);
            ResetUI();
        }
        #endregion

        #region page event handlers
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (mailMessage != null && mailMessage.Busy == true)
                {
                    if (MessageBox.Show("Sending in progress, abort and exit ?", "", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (mailMessage != null && mailMessage.Busy == true)
                        mailMessage.Abort();
                }

                base.OnBackKeyPress(e);
            }
            catch
            {
            }
        }


        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {

            if (mailMessage != null && mailMessage.Busy == true)
                mailMessage.Abort();

            base.OnNavigatedFrom(e);
        }
        #endregion

        #region utils
        private static string FormatBytes(long bytes, bool noFloat = false)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);
            decimal ret = 0;

            foreach (string order in orders)
            {
                if (bytes > max)
                {
                    ret = decimal.Divide(bytes, max);
                    if (noFloat == true)
                        ret = (int)ret;

                    return string.Format("{0:##.##} {1}", ret, order);
                }

                max /= scale;
            }
            return "0 Bytes";
        }
        #endregion
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

        private void adControl1_AdRefreshed(object sender, EventArgs e)
        {
            Debug.WriteLine("AdControl new ad received");
        }

        private void adControl1_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Debug.WriteLine("AdControl error: " + e.Error.Message);
        }

        private void adControl2_AdRefreshed(object sender, EventArgs e)
        {
            Debug.WriteLine("AdControl new ad received");
        }

        private void adControl2_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Debug.WriteLine("AdControl error: " + e.Error.Message);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    if (this.textBoxMailTo.Text == ";")
                    {
                        this.textBoxMailTo.Text = "";
                    }
                }
                catch
                {
                }
                try
                {
                    address.Show();
                }
                catch
                {
                }
                try
                {
                    if (this.textBoxMailTo.Text == ";")
                    {
                        this.textBoxMailTo.Text = "";
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

       

        private void user_focus(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                textBoxUserName.BorderBrush = new SolidColorBrush(Colors.White);
            }
            catch
            {
            }
        }

        private void password_focus(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            try
            {
                textBoxPassword.BorderBrush= new SolidColorBrush(Colors.White);
            }
            catch
            {
            }
        }

        private void listbox_focus(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			try
            {
                listBoxAttachments.BorderBrush = new SolidColorBrush(Colors.White);
            }
            catch
            {
            }
        }

        private void mailto_focus(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            try
            {
                textBoxMailTo.BorderBrush = new SolidColorBrush(Colors.White);
            }
            catch
            {
            }
        }

        private void subject_Focus(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            try
            {
                textBoxSubject.BorderBrush = new SolidColorBrush(Colors.White);
            }
            catch
            {
            }
        }

        private void body_Focus(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            try
            {
                textBoxBody.BorderBrush = new SolidColorBrush(Colors.White);
            }
            catch
            {
            }
        }

        private void textBoxUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                using (StreamWriter writeFile = new StreamWriter(new IsolatedStorageFileStream("myFile.txt", FileMode.Create, FileAccess.Write, myIsolatedStorage)))
                {
                    string someTextData = textBoxUserName.Text;
                    writeFile.WriteLine(someTextData);
                    writeFile.Close();
                }
            }
            catch
            {
            }
        }

        private void textBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

                //create new file
                using (StreamWriter writeFile = new StreamWriter(new IsolatedStorageFileStream("mypass.txt", FileMode.Create, FileAccess.Write, myIsolatedStorage)))
                {
                    string someTextData = textBoxPassword.Password;
                    writeFile.WriteLine(someTextData);
                    writeFile.Close();
                }
            }
            catch
            {
            }
        }

        private void roundButton2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.textBoxUserName.Text = "";
            }
            catch
            {
            }
        }

        private void roundButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.textBoxPassword.Password = "";
            }
            catch
            {
            }
        }

        private void textusername_focus(object sender, System.Windows.RoutedEventArgs e)
        {
            textBoxUserName.Background = new SolidColorBrush(Colors.White);
            textBoxUserName.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }

        private void textpassword_focus(object sender, System.Windows.RoutedEventArgs e)
        {
            textBoxPassword.Background = new SolidColorBrush(Colors.White);
            textBoxPassword.BorderBrush = new SolidColorBrush(Colors.Transparent);

        }

        private void textBoxMailTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
               /* */
               
               
            }
            catch
            {
            }
        }

        private void clear_Click(object sender, System.EventArgs e)
        {
        	 try
            {

                listBoxAttachments.Items.Clear();
            }
            catch
            {
                MessageBox.Show("No Attachments are done to clear attachments");
            }
        }

       

        private void about_click(object sender, System.EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/Page4.xaml", UriKind.RelativeOrAbsolute));
            }
            catch
            {
            }
        }

        private void signup_Click(object sender, System.EventArgs e)
        {
            try
            {

                if (ink.Visibility == Visibility.Visible)
                {
                    ink.Visibility = Visibility.Collapsed;
                }
                if (ink.Visibility == Visibility.Collapsed)
                {
                    ink.Visibility = Visibility.Visible;
                }
               
			
				  
            }
            catch
            {
            }
        }

        private void close_click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {

                ink.Visibility = Visibility.Collapsed;

            }
            catch
            {
            }
        }

        private void closefocus_focus(object sender, System.Windows.RoutedEventArgs e)
        {
        	try
            {

                ink.Visibility = Visibility.Collapsed;

            }
            catch
            {
            }
        }

        private void roundButton4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textBoxMailTo.Text = "";
            }
            catch
            {
            }
        }

       

       

        private void listBoxAttachments_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                listBoxAttachments.Items.Remove(listBoxAttachments.SelectedItem);
            }
            catch
            {
            }
        }

        
      

        
        

    }
}