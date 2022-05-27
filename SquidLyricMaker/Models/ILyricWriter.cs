using SquidLyricMaker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquidLyricMaker.Models
{
    public interface ILyricWriter
    {
        public string Write(ICollection<LyricLine> song, ICollection<Translation> translations);
    }
}
