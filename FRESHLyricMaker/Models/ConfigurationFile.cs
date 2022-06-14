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
    }
    public enum Theme
    {
        Light,
        Dark
    }
}
