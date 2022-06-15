using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHLyricMaker.Models
{
    public class ConfigurationFile
    {
        public float Volume { get; set; } = 1f;

        public Theme Theme { get; set; } = Theme.Dark;

        public string Language { get; set; } = "automatic";

        public int WindowLeft { get; set; }

        public int WindowTop { get; set; }

        public double WindowWidth { get; set; } = 750;

        public double WindowHeight { get; set; } = 500;

        public int SelectedTabIndex { get; set; }
    }
    public enum Theme
    {
        Light,
        Dark
    }
}
