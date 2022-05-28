using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SquidLyricMaker.ViewModels
{
    public class AboutDialogViewModel : ViewModelBase
    {
        public string AppVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public string FrameworkVersion => RuntimeInformation.FrameworkDescription;

        public string OSVersion => Environment.OSVersion.VersionString;

        private void OpenURL(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }

        public void OpenProjectWebsite() => OpenURL("https://royce551.github.io/SquidLyricMaker");
        public void ReportIssue() => OpenURL("https://github.com/Royce551/SquidLyricMaker/issues/new");
        public void OpenSourceCode() => OpenURL("https://github.com/Royce551/SquidLyricMaker");
    }
}
