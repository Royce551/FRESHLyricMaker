using FRESHMusicPlayer.Backends;
using SquidLyricMaker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquidLyricMaker.Models
{
    public class PlainLRCWriter : ILyricWriter
    {
        public bool ExportWithTranslations { get; init; }

        public bool ExportWithoutMetadata { get; init; }

        public bool ExportWithTwoDigitPrecision { get; init; }

        public IMetadataProvider? Metadata { get; init; }

        public string Write(ICollection<LyricLine> song, ICollection<Translation> translations)
        {
            var builder = new StringBuilder();
            if (!ExportWithoutMetadata)
            {
                if (Metadata is null) throw new InvalidOperationException();

                if (!string.IsNullOrEmpty(Metadata.Title)) builder.AppendLine($"[ti: {Metadata.Title}]");
                if (Metadata.Artists.Length != 0) builder.AppendLine($"[ar: {string.Join(", ", Metadata.Artists)}]");
                if (!string.IsNullOrEmpty(Metadata.Album)) builder.AppendLine($"[al: {Metadata.Album}]");

                builder.AppendLine($"[length: {TimeSpan.FromSeconds(Metadata.Length):mm\\:ss\\:fff}]");
                builder.AppendLine($"[re: SquidLyricMaker  https://github.com/royce551/squidlyricmaker]");

                builder.AppendLine();
            }

            int i = 0;
            foreach (var line in song)
            {
                if (!ExportWithTwoDigitPrecision) builder.AppendLine($"[{line.TimeStamp:mm\\:ss\\:fff}] {string.Join(' ', line.Words.Select(x => x.Word))}");
                else builder.AppendLine($"[{line.TimeStamp:mm\\:ss\\:ff}] {string.Join(' ', line.Words.Select(x => x.Word))}");

                if (ExportWithTranslations)
                {
                    foreach (var translation in translations)
                    {
                        var translationLines = translation.Text.Split('\n');
                        if (translationLines.Length < song.Count) return $"The {translation.LanguageName} translation doesn't line up with the source";

                        builder.AppendLine($"[{translation.LanguageCode}] {translationLines[i].Trim('\r')}");
                    }
                }
                i++;
            }

            return builder.ToString();
        }
    }
}
