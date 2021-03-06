using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FRESHLyricMaker.Views
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
