using Avalonia.Controls;
using FRESHLyricMaker.ViewModels;
using System;

namespace FRESHLyricMaker.Views
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel => (DataContext as MainWindowViewModel) ?? throw new InvalidOperationException();

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
