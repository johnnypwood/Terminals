using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Terminals.Configuration;
using Terminals.Data;

namespace Terminals.CaptureManager
{
    internal static class CaptureManager
    {
        private static readonly Settings settings = Settings.Instance;

        public static string CaptureRoot
        {
            get
            {
                return settings.CaptureRoot;
            }
        }

        public static Captures LoadCaptures(string path)
        {
            var captures = new Captures();
            var dir = new DirectoryInfo(path);

            foreach (FileInfo cap in GetFiles(dir))
            {
                var newCap = new Capture(cap.FullName);
                captures.Add(newCap);
            }

            return captures;
        }

        private static FileInfo[] GetFiles(DirectoryInfo dir)
        {
            try
            {
                return dir.GetFiles("*.png");
            }
            catch (Exception)
            {
                return new FileInfo[0];
            }
        }

        internal static void PerformScreenCapture(Control control, IFavorite favorite)
        {
            string name = ResolveFileName(favorite);
            string formatedDate = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            string filename = String.Format("{0}{1}.png", name, formatedDate);
            string tempFile = Path.Combine(CaptureRoot, filename);
            var sc = new ScreenCapture();
            Bitmap bmp = sc.CaptureControl(control, tempFile, ImageFormatTypes.imgPNG);

            if (settings.EnableCaptureToClipboard)
                Clipboard.SetImage(bmp);
        }

        private static string ResolveFileName(IFavorite favorite)
        {
            if (favorite != null && !string.IsNullOrEmpty(favorite.Name))
                return favorite.Name + "-";

            return string.Empty;
        }

        public static List<DirectoryInfo> LoadCaptureFolder(string path)
        {
            if (!Directory.Exists(path))
                return new List<DirectoryInfo>();

            DirectoryInfo dir = new DirectoryInfo(path);
            return new List<DirectoryInfo>(dir.GetDirectories());
        }

        internal static void EnsureRoot()
        {
            try
            {
                TryEnsureRoot();
            }
            catch (Exception exception)
            {
                string logMessage = String.Format("Capture root could not be created, set it to the default value : {0}", CaptureRoot);
                Logging.Error(logMessage, exception);
            }
        }

        private static void TryEnsureRoot()
        {
            string root = CaptureRoot;
            if (Directory.Exists(root))
                return;

            Logging.Info(String.Format("Capture root folder does not exist:{0}. Lets try to create it now.", root));
            Directory.CreateDirectory(root);
        }
    }
}
