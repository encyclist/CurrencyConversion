using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Wox.Infrastructure.Storage;

namespace Exchange
{
    /// <summary>
    /// SettingsControl.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private PluginJsonStorage<Settings> storage;
        private Settings settings;

        public SettingsControl()
        {
            InitializeComponent();

            storage = new PluginJsonStorage<Settings>();
            settings = storage.Load();

            apiKeyText.Text = settings.apiKey;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
            e.Handled = true;
        }

        private void onClickSave(object sender, RoutedEventArgs e)
        {
            settings.apiKey = apiKeyText.Text;
            storage.Save();
        }
    }
}
