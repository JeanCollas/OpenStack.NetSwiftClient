using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetSwiftClient.Demo.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static DemoContext StaticContext { get; set; }
        public DemoContext Context => StaticContext;

        const string SAVE_STATE_KEY = nameof(SAVE_STATE_KEY);

        public MainWindow()
        {
            if (Properties.Settings.Default.Properties[SAVE_STATE_KEY] == null)
            {
                Properties.Settings.Default.Properties.Add(new System.Configuration.SettingsProperty(SAVE_STATE_KEY, typeof(string), Properties.Settings.Default.Providers["LocalFileSettingsProvider"], false, "", SettingsSerializeAs.String, new SettingsAttributeDictionary(), false, false));
                Properties.Settings.Default.Save();
            }

            var val = Properties.Settings.Default.SettingsKey.Contains(SAVE_STATE_KEY) ? (string)Properties.Settings.Default[SAVE_STATE_KEY] : null;
            var loaded = val.IsNullOrEmpty() ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<DemoContext>(val);
            StaticContext = StaticContext ?? loaded ?? new DemoContext();

            //Properties.Settings.Default["SampleViewModel"] = Newtonsoft.Json.JsonConvert.SerializeObject(ctxt);

            DataContext = Context;

            Closing += MainWindow_Closing;

            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var ctxt = StaticContext;
            //            Properties.Settings.Default[SAVE_STATE_KEY] = Newtonsoft.Json.JsonConvert.SerializeObject(ctxt);
            //           Properties.Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "Images (.jpeg, .jpg, .png, .gif)|*.jpeg;*.png;*.jpg;*.gif";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                Context.NewFileSource = dlg.FileName;
                Context.NewFileName = System.IO.Path.GetFileName(dlg.FileName);

            }
        }

    }
}
