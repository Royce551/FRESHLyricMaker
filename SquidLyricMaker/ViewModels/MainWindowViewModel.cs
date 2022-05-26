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
        public  Player Player { get; private set; } = new();
        private Timer progressTimer = new(100);

        public MainWindowViewModel()
        {
            Player.SongChanged += Player_SongChanged;
            Player.SongStopped += Player_SongStopped;
            progressTimer.Elapsed += ProgressTimer_Elapsed;
        }

        private void ProgressTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ProgressTick();
        }

        public async void LoadFileCommand()
        {
            await Player.PlayAsync(SourceLanguageText);
            Player.Volume = 0.5f;
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
            if (!Player.FileLoaded)
            {
                LoadFileCommand();
                return;
            }
            if (Player.Paused) Player.Resume();
            else Player.Pause();
        }

        public void BackFiveSeconds()
        {
            if (Player.FileLoaded && Player.CurrentTime.TotalSeconds > 5) Player.CurrentTime -= TimeSpan.FromSeconds(5);
        }
        public void ForwardFiveSeconds()
        {
            if (Player.FileLoaded && Player.TotalTime.TotalSeconds - Player.CurrentTime.TotalSeconds > 5) Player.CurrentTime += TimeSpan.FromSeconds(5);
        }

        private TimeSpan currentTime;
        public TimeSpan CurrentTime
        {
            get
            {
                if (Player.FileLoaded)
                    return Player.CurrentTime;
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
                if (Player.FileLoaded)
                    return Player.CurrentTime.TotalSeconds;
                else return 0;

            }
            set
            {
                if (TimeSpan.FromSeconds(value) >= TotalTime) return;
                Player.CurrentTime = TimeSpan.FromSeconds(value);
                ProgressTick();
                this.RaiseAndSetIfChanged(ref currentTimeSeconds, value);
            }
        }
        private TimeSpan totalTime;
        public TimeSpan TotalTime
        {
            get
            {
                if (Player.FileLoaded)
                    return Player.TotalTime;
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
                if (Player.FileLoaded)
                    return Player.TotalTime.TotalSeconds;
                else return 0;
            }
            set => this.RaiseAndSetIfChanged(ref totalTimeSeconds, value);
        }

        public void ProgressTick()
        {
            this.RaisePropertyChanged(nameof(CurrentTime));
            this.RaisePropertyChanged(nameof(CurrentTimeSeconds));
            foreach (var line in Song) line.Update();
        }

        public List<LyricLine> Song = new();

        private string sourceLanguageText = default!;
        public string SourceLanguageText
        {
            get => sourceLanguageText;
            set
            {
                this.RaiseAndSetIfChanged(ref sourceLanguageText, value);
                Song.Clear();
                foreach (var line in sourceLanguageText.Split('\n'))
                {
                    var lyricLine = new LyricLine(this, TimeSpan.Zero);
                    foreach (var word in line.Split(' ', '/'))
                    {
                        lyricLine.Words.Add(new(this, TimeSpan.Zero, word));
                    }
                    Song.Add(lyricLine);
                }
            }
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

    public class LyricLine : ViewModelBase
    {
        public TimeSpan TimeStamp { get; set; }

        public List<LyricWord> Words { get; set; } = new();

        public bool IsCurrentlySungLine => mainWindow.Player.CurrentTime > TimeStamp;

        private MainWindowViewModel mainWindow;

        public LyricLine(MainWindowViewModel mainWindow, TimeSpan timeStamp)
        {
            this.mainWindow = mainWindow;
            TimeStamp = timeStamp;
        }

        public void Update()
        {
            foreach (var word in Words) word.Update();
        }
    }

    public class LyricWord : ViewModelBase
    {
        public TimeSpan TimeStamp { get; set; }

        public string Word { get; set; }

        public bool IsCurrentlySungWord => mainWindow.Player.CurrentTime > TimeStamp;

        private MainWindowViewModel mainWindow;

        public LyricWord(MainWindowViewModel mainWindow, TimeSpan timeStamp, string word)
        {
            this.mainWindow = mainWindow;
            TimeStamp = timeStamp;
            Word = word;
        }

        public void Update()
        {
            this.RaisePropertyChanged(nameof(IsCurrentlySungWord));
        }
    }
}
