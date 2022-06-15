using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using FRESHLyricMaker.Models;
using FRESHLyricMaker.ViewModels;
using FRESHLyricMaker.Views;
using System;

namespace FRESHLyricMaker
{
    public partial class App : Application
    {
        public static ConfigurationFile Config { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async override void OnFrameworkInitializationCompleted()
        {
            Config = await JsonService.ReadAsync<ConfigurationFile>("config.json");

            if (Config.Theme == Theme.Light)
            {
                var lightSIADLTheme = new StyleInclude(new Uri("avares://FRESHLyricMaker"))
                {
                    Source = new Uri("avares://SIADL.Avalonia/LightTheme.axaml")
                };
                var lightFluentTheme = new FluentTheme(new Uri("avares://FRESHLyricMaker"))
                {
                    Mode = FluentThemeMode.Light
                };
#pragma warning disable CS8602 // this is 99% not going to be null
                Avalonia.Application.Current.Styles[0] = lightFluentTheme;
#pragma warning restore CS8602
                Avalonia.Application.Current.Styles[1] = lightSIADLTheme;
                // this is all a bit hacky; hopefully can be improved soon!
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                    Width = Config.WindowWidth,
                    Height = Config.WindowHeight
                };
                var mainWindow = desktop.MainWindow as MainWindow;
                mainWindow.ViewModel.Player.Volume = Config.Volume;
                mainWindow.ViewModel.SelectedTabIndex = Config.SelectedTabIndex;
                mainWindow.Show();
                desktop.Exit += async (s, e) =>
                {
                    Config.WindowLeft = mainWindow.Position.X;
                    Config.WindowTop = mainWindow.Position.Y;
                    Config.WindowWidth = mainWindow.Width;
                    Config.WindowHeight = mainWindow.Height;
                    Config.SelectedTabIndex = mainWindow.ViewModel.SelectedTabIndex;
                    Config.Volume = mainWindow.ViewModel.Player.Volume;
                    await JsonService.WriteAsync("config.json", Config);
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
