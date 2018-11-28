// Copyright MyScript. All right reserved.

using MyScript.IInk.UIReferenceImplementation.UserControls;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;

namespace MyScript.IInk.GetStarted
{
    public sealed partial class MainPage : Page
    {
        private static readonly HttpClient client = new HttpClient();
        // Defines the type of content (possible values are: "Text Document", "Text", "Diagram", "Math", and "Drawing")
        private const string PartType = "Math";

        private Engine _engine;
        private Engine _engine2;
        private Editor Editor => UcEditor.Editor;

        public object WebClient { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            Loaded += UcEditor.UserControl_Loaded;
            Loaded += Page_Loaded;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _engine = App.Engine;
            _engine2 = App.Engine;
            // Folders "conf" and "resources" are currently parts of the layout
            // (for each conf/res file of the project => properties => "Build Action = content")
            var confDirs = new string[1];
            confDirs[0] = "conf";
            _engine.Configuration.SetStringArray("configuration-manager.search-path", confDirs);
            _engine2.Configuration.SetStringArray("configuration-manager.search-path", confDirs);

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            var tempFolder = System.IO.Path.Combine(localFolder, "tmp");
            _engine.Configuration.SetString("content-package.temp-folder", tempFolder);
            _engine2.Configuration.SetString("content-package.temp-folder", tempFolder);

            // Initialize the editor with the engine
            UcEditor.Engine = _engine;
            // Force pointer to be a pen, for an automatic detection, set InputMode to AUTO
            SetInputMode(InputMode.PEN);
            Editor.ToString();
            NewFile();
            Loaded -= Page_Loaded;
        }

        private void AppBar_UndoButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.Undo();
        }

        private void AppBar_RedoButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.Redo();
        }

        private void AppBar_ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ImageSource.Source = null;
            Editor.Clear();
        }

        private async void AppBar_ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var supportedStates = Editor.GetSupportedTargetConversionStates(null);
                if ( (supportedStates != null) && (supportedStates.Count() > 0))
                {
                    Editor.Convert(null, supportedStates[0]);
                }
                string latex = Editor.Export_(Editor.GetRootBlock(), MimeType.LATEX);
                string latex2 = latex;
                cleanURLAsync(latex2);
            }
            catch (Exception ex)
            {
                var msgDialog = new Windows.UI.Popups.MessageDialog(ex.ToString());
                await msgDialog.ShowAsync();
            }
        }
        private async void cleanURLAsync(string latextString)
        {
            string ApiKey = "4Q2EQ8-UEQGLYJ4GL";
            latextString = latextString.Replace("+", "%2B");
            string url = "https://api.wolframalpha.com/v1/simple?i=" + latextString +"&appid=" + ApiKey;
            HttpClient wc = new HttpClient();
            Stream stream = await wc.GetStreamAsync(url);
            MemoryStream myStream = new MemoryStream();
            stream.CopyTo(myStream);
            myStream.Position = 0;
            BitmapImage myBitmap = new BitmapImage();
            myBitmap.SetSource(myStream.AsRandomAccessStream());
            ImageSource.Source = myBitmap;
            ImageSource.Stretch = Windows.UI.Xaml.Media.Stretch.Fill;

        }
        private void SetInputMode(InputMode inputMode)
        {
            UcEditor.InputMode = inputMode;
            autoToggleButton.IsChecked = (inputMode == InputMode.AUTO);
            touchPointerToggleButton.IsChecked = (inputMode == InputMode.TOUCH);
            editToggleButton.IsChecked = (inputMode == InputMode.PEN);
        }

        private void AppBar_TouchPointerButton_Click(object sender, RoutedEventArgs e)
        {
            var isChecked = ((ToggleButton)(sender)).IsChecked;
            if (isChecked != null && (bool)isChecked)
            {
                SetInputMode(InputMode.TOUCH);
            }
        }

        private void AppBar_EditButton_Click(object sender, RoutedEventArgs e)
        {
            var isChecked = ((ToggleButton)(sender)).IsChecked;
            if (isChecked != null && (bool)isChecked)
            {
                SetInputMode(InputMode.PEN);
            }
        }

        private void AppBar_AutoButton_Click(object sender, RoutedEventArgs e)
        {
            var isChecked = ((ToggleButton)(sender)).IsChecked;
            if (isChecked != null && (bool)isChecked)
            {
                SetInputMode(InputMode.AUTO);
            }
        }

        private void NewFile()
        {
            // Create package and part
            var packageName = MakeUntitledFilename();
            var package = _engine.CreatePackage(packageName);
            var part = package.CreatePart(PartType);
            Editor.Part = part;
            Title.Text = "Type: " + PartType;
            var packageName2 = MakeUntitledFilename2();
            var package2 = _engine2.CreatePackage(packageName2);
            var part2 = package2.CreatePart(PartType);
        }
        private static string MakeUntitledFilename2()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            var num = 0;
            string name;

            do
            {
                var baseName = "File" + (++num) + "hello2" + ".iink";
                name = System.IO.Path.Combine(localFolder, baseName);
            }
            while (System.IO.File.Exists(name));

            return name;
        }

        private static string MakeUntitledFilename()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            var num = 0;
            string name;

            do
            {
                var baseName = "File" + (++num) + ".iink";
                name = System.IO.Path.Combine(localFolder, baseName);
            }
            while (System.IO.File.Exists(name));

            return name;
        }

        private void NavigateToSecondPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TextPage));
        }
    }
}
