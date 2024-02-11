using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;


namespace WPF.Template
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // For gathering the version number.
        }

        public static string PackageVersion
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                if (assembly != null)
                {
                    var version = assembly.GetName().Version;
                    if (version != null)
                    {
                        return version.ToString();
                    }
                }
                return "Error getting version number.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private async void CheckforUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Version currentVersion = new(PackageVersion);

            // GitHub API URL for the latest release.
            string latestReleaseUrl = "https://api.github.com/repos/Jestzer/WPF.Template/releases/latest";

            // Use HttpClient to fetch the latest release data.
            using (HttpClient client = new())
            {
                // GitHub API requires a user-agent. I'm adding the extra headers to reduce HTTP error 403s.
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("WPF.Template", PackageVersion));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    try
                    {
                        // Make the latest release a JSON string.
                        string jsonString = await client.GetStringAsync(latestReleaseUrl);

                        // Parse the JSON to get the tag_name (version number).
                        using JsonDocument doc = JsonDocument.Parse(jsonString);
                        JsonElement root = doc.RootElement;
                        string latestVersionString = root.GetProperty("tag_name").GetString()!;

                        // Remove 'v' prefix if present in the tag name.
                        latestVersionString = latestVersionString.TrimStart('v');

                        // Parse the version string.
                        Version latestVersion = new Version(latestVersionString);

                        // Compare the current version with the latest version.
                        if (currentVersion.CompareTo(latestVersion) < 0)
                        {
                            // A newer version is available!
                            ErrorWindow errorWindow = new();
                            errorWindow.Owner = this;
                            errorWindow.ErrorTextBlock.Text = "";
                            errorWindow.URLTextBlock.Visibility = Visibility.Visible;
                            errorWindow.Title = "Check for updates";
                            errorWindow.ShowDialog();
                        }
                        else
                        {
                            // The current version is up-to-date.
                            ErrorWindow errorWindow = new();
                            errorWindow.Owner = this;
                            errorWindow.Title = "Check for updates";
                            errorWindow.ErrorTextBlock.Text = "You are using the latest release available.";
                            errorWindow.ShowDialog();
                        }
                    }
                    catch (JsonException ex)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Owner = this;
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "The Json code in this program didn't work. Here's the automatic error message it made: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Owner = this;
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "HTTP error 403: GitHub is saying you're sending them too many requests, so... slow down, I guess? " +
                            "Here's the automatic error message: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                    catch (HttpRequestException ex)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Owner = this;
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "HTTP error. Here's the automatic error message: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.Owner = this;
                    errorWindow.Title = "Check for updates";
                    errorWindow.ErrorTextBlock.Text = "Oh dear, it looks this program had a hard time making the needed connection to GitHub. Make sure you're connected to the internet " +
                        "and your lousy firewall/VPN isn't blocking the connection. Here's the automated error message: \"" + ex.Message + "\"";
                    errorWindow.ShowDialog();
                }
            }
        }

        private void ShowErrorWindow(string errorMessage)
        {
            ErrorWindow errorWindow = new ErrorWindow();
            errorWindow.ErrorTextBlock.Text = errorMessage;
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }

        private void MoneyButton_Click(object sender, RoutedEventArgs e)
        {
            ShowErrorWindow("Money texblock parsing error.");
        }

        private void ProfitButton_Click(object sender, RoutedEventArgs e)
        {
            ShowErrorWindow("Profit texblock parsing error.");
        }
    }
}