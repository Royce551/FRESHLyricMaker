using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FRESHMusicPlayer;
using System.Globalization;
using System.Text;
using ReactiveUI;
using System.Timers;

namespace SquidLyricMaker.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Player player = new();
        private Timer progressTimer = new(100);

        public MainWindowViewModel()
        {
            player.SongChanged += Player_SongChanged;
            player.SongStopped += Player_SongStopped;
            progressTimer.Elapsed += ProgressTimer_Elapsed;
        }

        private void ProgressTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ProgressTick();
        }

        public async void LoadFileCommand()
        {
            await player.PlayAsync(SourceLanguageText);
            player.Volume = 0.5f;
        }

        private void Player_SongStopped(object? sender, EventArgs e)
        {
            progressTimer.Stop();
        }

        private void Player_SongChanged(object? sender, EventArgs e)
        {
            progressTimer.Start();
            this.RaisePropertyChanged(nameof(TotalTime));
            this.RaisePropertyChanged(nameof(TotalTimeSeconds));
        }

        public void PlayPauseCommand()
        {
            if (!player.FileLoaded)
            {
                LoadFileCommand();
                return;
            }
            if (player.Paused) player.Resume();
            else player.Pause();
        }

        public void BackFiveSeconds()
        {
            if (player.FileLoaded && player.CurrentTime.TotalSeconds > 5) player.CurrentTime -= TimeSpan.FromSeconds(5);
        }
        public void ForwardFiveSeconds()
        {
            if (player.FileLoaded && player.TotalTime.TotalSeconds - player.CurrentTime.TotalSeconds > 5) player.CurrentTime += TimeSpan.FromSeconds(5);
        }

        private TimeSpan currentTime;
        public TimeSpan CurrentTime
        {
            get
            {
                if (player.FileLoaded)
                    return player.CurrentTime;
                else return TimeSpan.Zero;

            }
            set
            {
                this.RaiseAndSetIfChanged(ref currentTime, value);
            }
        }

        private double currentTimeSeconds;
        public double CurrentTimeSeconds
        {
            get
            {
                if (player.FileLoaded)
                    return player.CurrentTime.TotalSeconds;
                else return 0;

            }
            set
            {
                if (TimeSpan.FromSeconds(value) >= TotalTime) return;
                player.CurrentTime = TimeSpan.FromSeconds(value);
                ProgressTick();
                this.RaiseAndSetIfChanged(ref currentTimeSeconds, value);
            }
        }
        private TimeSpan totalTime;
        public TimeSpan TotalTime
        {
            get
            {
                if (player.FileLoaded)
                    return player.TotalTime;
                else return TimeSpan.Zero;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref totalTime, value);
            }
        }
        private double totalTimeSeconds;
        public double TotalTimeSeconds
        {
            get
            {
                if (player.FileLoaded)
                    return player.TotalTime.TotalSeconds;
                else return 0;
            }
            set => this.RaiseAndSetIfChanged(ref totalTimeSeconds, value);
        }

        public void ProgressTick()
        {
            this.RaisePropertyChanged(nameof(CurrentTime));
            this.RaisePropertyChanged(nameof(CurrentTimeSeconds));
        }

        private string? sourceLanguageText;
        public string? SourceLanguageText
        {
            get => sourceLanguageText;
            set => this.RaiseAndSetIfChanged(ref sourceLanguageText, value);
        }

        public ObservableCollection<Translation> Translations { get; set; } = new();

        public void AddTranslationCommand()
        {
            Translations.Add(new Translation(this));
        }
    }

    public class Translation : ViewModelBase
    {
        private string languageCode = "en-US";
        public string LanguageCode
        {
            get => languageCode;
            set
            {
                this.RaiseAndSetIfChanged(ref languageCode, value);
                this.RaisePropertyChanged(nameof(LanguageName));
            }
        }

        public string LanguageName
        {
            get
            {
                try
                {
                    return new CultureInfo(LanguageCode).DisplayName;
                }
                catch
                {
                    return "Invalid Language";
                }
            }
        }

        private string? text;
        public string? Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }


        private MainWindowViewModel mainWindow;
        public Translation(MainWindowViewModel mainWindow)
        {
            this.mainWindow = mainWindow;
        }
    }
}
