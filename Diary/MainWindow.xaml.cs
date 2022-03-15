using Diary.Logic;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Diary.Logic.Native;


namespace Diary
{
	// Interaction logic for MainWindow.xaml
	public partial class MainWindow : Window 
    {
        private ActiveDatabase Adb { get; set; }

        public MainWindow() 
        {
            InitializeComponent();
            Adb = new ActiveDatabase();
            LblVersion.Content = AssemblyVersion;
            TxtUsername.Focus();
        }

        #region TabPanel

        private void CollapseAllPages() 
        {
            MainPage.Visibility = Visibility.Collapsed;
            SearchPage.Visibility = Visibility.Collapsed;
            MorePage.Visibility = Visibility.Collapsed;
            InfoPage.Visibility = Visibility.Collapsed;
            BtnMain.IsEnabled = true;
            BtnSearch.IsEnabled = true;
            BtnMore.IsEnabled = true;
            BtnInfo.IsEnabled = true;
        }

        private void btnMain_Click(object sender, RoutedEventArgs e) 
        {
            CollapseAllPages();
            BtnMain.IsEnabled = false;
            MainPage.Visibility = Visibility.Visible;
            TxtTitle.Focus();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) 
        {
            CollapseAllPages();
            BtnSearch.IsEnabled = false;
            SearchPage.Visibility = Visibility.Visible;
            TxtSearch.Focus();
        }

        private void btnMore_Click(object sender, RoutedEventArgs e) 
        {
            CollapseAllPages();
            BtnMore.IsEnabled = false;
            MorePage.Visibility = Visibility.Visible;
            TxtRemoveEntree.Focus();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e) 
        {
            CollapseAllPages();
            BtnInfo.IsEnabled = false;
            InfoPage.Visibility = Visibility.Visible;
        }

        #endregion TabPanel

        #region MainPage

        private string BildURL = ""; 

        private async void btnSave_Click(object sender, RoutedEventArgs e) 
        {
            var title = TxtTitle.Text;
            var body = TxtBody.Text;
            var tags = TxtTags.Text;
            var PicURI = BildURL;
            LblMainStatus.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(body) || string.IsNullOrEmpty(tags)) 
            {
                LblMainStatus.Content = "Titel, Tags und oder Text dürfen nicht leer sein";
            } else 
            {
                var add = await Task.Run(() => Adb.AddEntry(title, body, tags, PicURI));
                if (add) 
                {
                    LblMainStatus.Content = "Eintrag erfolgreich erstellt!";
                    TxtTitle.Text = "";
                    TxtBody.Text = "";
                    TxtTags.Text = "";
                    BildURL = "";
                    PictureViewer.Source = null;

                } else 
                {
                    LblMainStatus.Content = "Eintrag konnte nicht erstellt werden (Duplikat)";
                }
            }

        }


        private void txtBody_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) 
        {
            LblMainStatus.Visibility = Visibility.Collapsed;
        }

        private void txtBody_TextChanged(object sender, TextChangedEventArgs e) 
        {
            if (LblMainStatus.Visibility == Visibility.Visible) 
            {
                LblMainStatus.Visibility = Visibility.Collapsed;
            }
            var body = TxtBody.Text;
            var lines = body.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length;
            var words = body.Replace(Environment.NewLine, " ").Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;
            var metrics = $"{TxtBody.Text.Length} Buchstaben, {words} Wörter, {lines} Zeilen.";
            LblMetrics.Content = metrics;
        }


        #endregion MainPage

        #region SearchPage

        private async void btnSearchQuery_Click(object sender, RoutedEventArgs e) {
            var query = TxtSearch.Text;
            LstSearchResults.ItemsSource = null;
            LblSearchStatus.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(query)) {
                LblSearchStatus.Content = "Cannot search with empty text!";
            } else {
                var results = await Task.Run(() => Adb.Search(query));
                if (results == null || results.Count == 0) {
                    LblSearchStatus.Content = "Found no results :(";
                } else {
                    LblSearchStatus.Content = $"Results found: {results.Count}";
                    LstSearchResults.ItemsSource = results;
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) 
        {
            if (LblSearchStatus.Visibility == Visibility.Visible) 
            {
                LblSearchStatus.Visibility = Visibility.Collapsed;
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e) 
        {
            if (e.Key == Key.Enter) 
            {
                btnSearchQuery_Click(sender, e);
            }
        }

        #endregion SearchPage

        #region MorePage

        private async void btnRemoveEntree_Click(object sender, RoutedEventArgs e) 
        {
            var id = TxtRemoveEntree.Text;
            LblRemoveStatus.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(id)) 
            {
                LblRemoveStatus.Content = "Bitte die ID Nummer eingeben";
            } else 
            {
                var status = int.TryParse(id, out var convertedId);
                if (!status) 
                {
                    LblRemoveStatus.Content = "Nur Zahlen verwenden!";
                } else 
                {
                    var removeStatus = await Task.Run(() => Adb.RemoveEntry(convertedId));
                    if (!removeStatus) 
                    {
                        LblRemoveStatus.Content = "ID existiert nicht";
                    } else 
                    {
                        LblRemoveStatus.Content = "Eintrag erfolgreich entfernt!";
                    }
                }
            }
        }

        private async void btnExportDiary_Click(object sender, RoutedEventArgs e) 
        {
            LblExportStatus.Visibility = Visibility.Visible;
            var exportResults = await Task.Run(() => Adb.ExportEntries());
            if (!exportResults) 
            {
                LblExportStatus.Content = "Fehler, Tagebuch wurde nicht exportiert!";
            } else 
            {
                LblExportStatus.Content = "Das Tagebuch wurde erfolgreich exportiert";
            }
        }

        private async void btnRemoveUser_Click(object sender, RoutedEventArgs e) 
        {
            var password = PswBoxRemove.Password;
            LblRemoveUserStatus.Visibility = Visibility.Visible;
            if (password != Adb.CurrentUser.Password) 
            {
                LblRemoveUserStatus.Content = "Falsches Passwort!";
            } else 
            {
                var removeStatus = await Task.Run(() => Adb.RemoveCurrentUser());
                if (!removeStatus) 
                {
                    LblRemoveUserStatus.Content = "Benutzer konnte nicht gelöscht werden";
                } else 
                {
                    LblRemoveUserStatus.Content = "Benutzer erfolgreich gelöscht";
                }
            }
        }

        private void pswBoxRemove_KeyDown(object sender, KeyEventArgs e) 
        {
            if (e.Key == Key.Enter) 
            {
                btnRemoveUser_Click(sender, e);
            }
        }

        private void txtRemoveEntree_KeyDown(object sender, KeyEventArgs e) 
        {
            if (e.Key == Key.Enter) 
            {
                btnRemoveEntree_Click(sender, e);
            }
        }

        #endregion MorePage

        #region LoginPage

        private async void btnCreate_Click(object sender, RoutedEventArgs e) 
        {
            var username = TxtUsername.Text;
            var password = PswBox.Password;
            LblLoginStatus.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) 
            {
                LblLoginStatus.Content = "Username und Passwort dürfen nicht leer sein!";
            } else 
            {
                var createResult = await Task.Run(() => Adb.AddUser(new User(username, password)));
                if (!createResult) 
                {
                    LblLoginStatus.Content = "Benutzer bereits vorhanden!";
                } else 
                {
                    LoginPage.Visibility = Visibility.Collapsed;
                    GeneralPage.Visibility = Visibility.Visible;
                }
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e) 
        {
            var username = TxtUsername.Text;
            var password = PswBox.Password;
            LblLoginStatus.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) 
            {
                LblLoginStatus.Content = "Username und Passwort dürfen nicht leer sein!";
            } else 
            {
                var loginResult = await Task.Run(() => Adb.Login(username, password));
                if (!loginResult) 
                {
                    LblLoginStatus.Content = "Falsches Passwort";
                } else 
                {
                    LoginPage.Visibility = Visibility.Collapsed;
                    GeneralPage.Visibility = Visibility.Visible;
                }
            }
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e) 
        {
            if (e.Key == Key.Enter) 
            {
                PswBox.Focus();
            }
        }

        private void pswBox_KeyDown(object sender, KeyEventArgs e) 
        {
            if (e.Key == Key.Enter) 
            {
                btnLogin_Click(sender, e);
            }
        }

        #endregion LoginPage

        private void btnSavePicture(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    BildURL = filePath;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.EndInit();
                PictureViewer.Source = bitmap;
            }
                
        }
    }
}
