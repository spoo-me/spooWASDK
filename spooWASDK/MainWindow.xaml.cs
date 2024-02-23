using H.NotifyIcon.Core;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;
using Windows.Win32;
using WinRT.Interop;
using WinUIEx.Messaging;
using Windows.Win32.Foundation;
using Microsoft.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.UI;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Input;
using Windows.Devices.Input;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using ZXing;
using System.Drawing;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using Windows.Storage.Streams;
using ZXing.Rendering;
using QRCoder;
using System.Text;
using Windows.Graphics.Imaging;
using spooWASDK.Classes;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace spooWASDK
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        public MainWindow(bool showStart = false)
        {
            this.InitializeComponent();
            this.MinWidth = 600;
            this.MinHeight = 365;
            this.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(625, 425));
            this.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            this.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            this.AppWindow.SetIcon($"{AppContext.BaseDirectory}/Assets/TaskbarIcon.ico");
            this.CenterOnScreen();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ShortenButton.IsEnabled = false;
            Ring.Visibility = Visibility.Visible;
            ShortenText.Visibility = Visibility.Collapsed;
            LoadingQR.Visibility = Visibility.Visible;

            var client = new HttpClient();
            var formData = new Dictionary<string, string>
            {
                { "url", URIBox.Text }
            };

            if (Alias.Text != string.Empty) formData.Add("alias", Alias.Text);
            if (InvalidPassword.IsOpen == false) formData.Add("password", Password.Password.ToString());
            if (MaxClicks.Value > 0) formData.Add("max-clicks", ((int)MaxClicks.Value).ToString());
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://spoo.me/"),
                Headers =
                {
                    { "Accept", "application/json" }
                },
                Content = new FormUrlEncodedContent(formData),
            };

            using (var response = await client.SendAsync(request))
            {
                //response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                if (body.Contains("Alias already exists"))
                {
                    await UsedAlias.ShowAsync();
                    Ring.Visibility = Visibility.Collapsed;
                    ShortenText.Visibility = Visibility.Visible;
                    ShortenButton.IsEnabled = true;
                    return;
                }
                else
                {
                    string result = body.Substring(14);
                    result = result.Replace("\"}", string.Empty);
                    ResultBox.Text = result;
                    await QR.EnsureCoreWebView2Async();
                    QR.Source = new Uri($"https://qr.spoo.me/gradient?text={result}&gradient1=(75,75,75)&gradient2=(15,15,15)");
                    var package = new DataPackage();
                    package.SetText(result);
                    Clipboard.SetContent(package);
                }
            }

            await WV.EnsureCoreWebView2Async();
            Ring.Visibility = Visibility.Collapsed;
            ShortenText.Visibility = Visibility.Visible;
            WV.Source = new Uri("https://confetti-animation.vercel.app/");

            RunLastLoad();

            await Task.Delay(500);

            await Result.ShowAsync();
        }

        private async Task RunLastLoad()
        {
            await Task.Delay(2500);

            LoadingQR.Visibility = Visibility.Collapsed;
        }

        bool validURL = false;
        bool validPassword = false;

        public void CheckButton()
        {
            if (InvalidURL.IsOpen == false && InvalidPassword.IsOpen == false)
            {
                ShortenButton.IsEnabled = true;
            }
            else ShortenButton.IsEnabled = false;
        }

        private void URIBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(URIBox.Text) == true)
            {
                validURL = false;
                InvalidURL.IsOpen = true;
            }
            else
            {
                Uri uriResult;
                bool result = Uri.TryCreate(URIBox.Text, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (result == true)
                {
                    if (URIBox.Text.Contains('.') != true)
                    {
                        validURL = false;
                        InvalidURL.IsOpen = true;
                    }
                    else
                    {
                        validURL = true;
                        InvalidURL.IsOpen = false;
                    }
                }
                else
                {
                    validURL = false;
                    InvalidURL.IsOpen = true;
                }
            }
            CheckButton();
        }

        public List<char> letters = new List<char>()
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N',
            'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        public List<char> numbers = new List<char>()
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Password.Password) == true)
            {
                validPassword = true;
                Password.Password = string.Empty;
                InvalidPassword.IsOpen = false;
                return;
            }
            else
            {
                bool hasLetter = false;
                bool hasNumber = false;
                bool hasSpecialChar = false;
                bool hasConsecutiveChars = false;
                foreach (char c in letters)
                {
                    if (Password.Password.Contains(c))
                    {
                        hasLetter = true;
                    }
                }
                foreach (char n in numbers)
                {
                    if (Password.Password.Contains(n))
                    {
                        hasNumber = true;
                    }
                }
                if (Password.Password.Contains('@') || Password.Password.Contains('.')) hasSpecialChar = true;
                for (int i = 0; i < Password.Password.Length - 1; i++)
                {
                    if (Password.Password[i] == Password.Password[i + 1])
                    {
                        hasConsecutiveChars = true;
                        break;
                    }
                }
                if (hasLetter == true && hasNumber == true && hasSpecialChar == true && hasConsecutiveChars == false && Password.Password.Length >= 8)
                {
                    validPassword = true;
                    InvalidPassword.IsOpen = false;
                }
                else
                {
                    validPassword = false;
                    InvalidPassword.IsOpen = true;
                }
            }
            CheckButton();
        }

        private void URIBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(URIBox.Text) == false)
            {
                if (URIBox.Text.StartsWith("https://") == false && URIBox.Text.StartsWith("http://") == false)
                {
                    URIBox.Text = "https://" + URIBox.Text;
                    return;
                }
            }
            CheckButton();
        }

        private void URIBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Alias.Focus(FocusState.Pointer);
            }
            CheckButton();
        }

        private void Alias_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string specialCharacters = " ~!@#$%^&*()-_=+{}[]|\\:;\"\'<>,.?/`";
                char replacementCharacter = '-';

                for (int i = 0; i < specialCharacters.Length; i++)
                {
                    Alias.Text = Alias.Text.Replace(specialCharacters[i], replacementCharacter);
                }
                if (Alias.Text.Length > 15) Alias.Text = Alias.Text.Remove(15);
                Password.Focus(FocusState.Pointer);
            }
            CheckButton();
        }

        private void Alias_LostFocus(object sender, RoutedEventArgs e)
        {
            string specialCharacters = " ~!@#$%^&*()-_=+{}[]|\\:;\"\'<>,.?/`";
            char replacementCharacter = '-';

            for (int i = 0; i < specialCharacters.Length; i++)
            {
                Alias.Text = Alias.Text.Replace(specialCharacters[i], replacementCharacter);
            }
            if (Alias.Text.Length > 15) Alias.Text = Alias.Text.Remove(15);
            CheckButton();
        }

        private void Result_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ShortenButton.IsEnabled = true;
            Ring.Visibility = Visibility.Collapsed;
            ShortenText.Visibility = Visibility.Visible;
        }
    }
}
