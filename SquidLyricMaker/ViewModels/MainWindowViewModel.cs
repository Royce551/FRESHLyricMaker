using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FRESHMusicPlayer;
using System.Globalization;
using System.Text;
using ReactiveUI;
using System.Timers;
using SquidLyricMaker.Models;

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

        // Player

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
            IsReady = false;
            progressTimer.Stop();
        }

        private void Player_SongChanged(object? sender, EventArgs e)
        {
            IsReady = true;
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

        private bool isReady = false;
        public bool IsReady
        {
            get => isReady;
            set => this.RaiseAndSetIfChanged(ref isReady, value);
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

        public ObservableCollection<LyricLine> Song { get; set; } = new();

        private int selectedLineIndex;
        public int SelectedLineIndex
        {
            get => selectedLineIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedLineIndex, value);
            }
        }

        public LyricLine? SelectedLine
        { 
            get
            {
                if (Song.Count >= SelectedLineIndex) return Song[SelectedLineIndex];
                else return null;
            }
        }

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
                this.RaisePropertyChanged(nameof(Song));
            }
        }

        public ObservableCollection<Translation> Translations { get; set; } = new();

        private bool wordByWordMode = false;
        public bool WordByWordMode
        {
            get => wordByWordMode;
            set => this.RaiseAndSetIfChanged(ref wordByWordMode, value);
        }

        public void AddTranslationCommand()
        {
            Translations.Add(new Translation(this));
        }

        // Synchronizer

        public void TimestampLineCommand()
        {
            if (SelectedLine != null && Player.FileLoaded) SelectedLine.TimeStamp = Player.CurrentTime;
            NextLineCommand();
        }
        public void TimestampWordCommand()
        {
            if (WordByWordMode && SelectedLine != null && Player.FileLoaded)
            {
                if (SelectedLine.TimeStamp == TimeSpan.Zero) SelectedLine.TimeStamp = Player.CurrentTime;

                SelectedLine.Words[SelectedLine.SelectedWordIndex].TimeStamp = Player.CurrentTime;
                NextWordCommand();
            }
        }
        public void PreviousLineCommand()
        {
            if (SelectedLineIndex - 1 > 0)
                SelectedLineIndex--;
        }
        public void NextLineCommand()
        {
            if (SelectedLineIndex + 1 <= Song.Count)
                SelectedLineIndex++;
        }
        public void PreviousWordCommand()
        {
            if (WordByWordMode && SelectedLine != null)
            {
                if (SelectedLine.SelectedWordIndex - 1 > 0)
                    SelectedLine.SelectedWordIndex--;
            }
        }
        public void NextWordCommand()
        {
            if (WordByWordMode && SelectedLine != null)
            {
                
                if (SelectedLine.SelectedWordIndex + 1 < SelectedLine.Words.Count) SelectedLine.SelectedWordIndex++;
                else if (SelectedLine.SelectedWordIndex + 1 >= SelectedLine.Words.Count) NextLineCommand();
            }
        }

        // Export
        private bool exportPlainLRC;
        public bool ExportPlainLRC
        {
            get => exportPlainLRC;
            set
            {
                this.RaiseAndSetIfChanged(ref exportPlainLRC, value);
                this.RaisePropertyChanged(nameof(LrcFilePreview));
            }
        }
        private bool exportEnhancedLRC;
        public bool ExportEnhancedLRC
        {
            get => exportEnhancedLRC;
            set
            {
                this.RaiseAndSetIfChanged(ref exportEnhancedLRC, value);
                this.RaisePropertyChanged(nameof(LrcFilePreview));
            }
        }
        private bool exportWithTranslations;
        public bool ExportWithTranslations
        {
            get => exportWithTranslations;
            set
            {
                this.RaiseAndSetIfChanged(ref exportWithTranslations, value);
                this.RaisePropertyChanged(nameof(LrcFilePreview));
            }
        }

        private bool exportWithoutMetadata;
        public bool ExportWithoutMetadata
        {
            get => exportWithoutMetadata;
            set
            {
                this.RaiseAndSetIfChanged(ref exportWithoutMetadata, value);
                this.RaisePropertyChanged(nameof(LrcFilePreview));
            }
        }
        private bool exportWithTwoDigitPrecision;
        public bool ExportWithTwoDigitPrecision
        {
            get => exportWithTwoDigitPrecision;
            set
            {
                this.RaiseAndSetIfChanged(ref exportWithTwoDigitPrecision, value);
                this.RaisePropertyChanged(nameof(LrcFilePreview));
            }
        }

        public string LrcFilePreview
        {
            get
            {
                if (ExportPlainLRC)
                {
                    return new PlainLRCWriter
                    {
                        ExportWithTwoDigitPrecision = ExportWithTwoDigitPrecision,
                        ExportWithoutMetadata = ExportWithoutMetadata,
                        ExportWithTranslations = ExportWithTranslations,
                        Metadata = Player.Metadata
                    }.Write(Song, Translations);
                }
                else return "Enhanced LRC is not supported yet :(";
            }
        }

        public void ExportCommand()
        {

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
        private TimeSpan timeStamp;
        public TimeSpan TimeStamp
        {
            get => timeStamp;
            set => this.RaiseAndSetIfChanged(ref timeStamp, value);
        }

        public ObservableCollection<LyricWord> Words { get; set; } = new();

        private int selectedWordIndex = -1;
        public int SelectedWordIndex
        {
            get => selectedWordIndex;
            set
            {
                if (!mainWindow.WordByWordMode)
                {
                    selectedWordIndex = -1;
                    return;
                }
                this.RaiseAndSetIfChanged(ref selectedWordIndex, value);
            }
        }

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
            if (mainWindow.SelectedLine != this) SelectedWordIndex = -1;
            if (mainWindow.SelectedLine == this && SelectedWordIndex == -1) SelectedWordIndex = 0;
        }
    }

    public class LyricWord : ViewModelBase
    {
        private TimeSpan timeStamp;
        public TimeSpan TimeStamp
        {
            get => timeStamp;
            set => this.RaiseAndSetIfChanged(ref timeStamp, value);
        }

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
