using Avalonia.Controls;
using SquidLyricMaker.ViewModels;
using System;

namespace SquidLyricMaker.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (DataContext as MainWindowViewModel) ?? throw new InvalidOperationException();

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
