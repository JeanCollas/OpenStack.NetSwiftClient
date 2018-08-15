using NetSwiftClient.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetSwiftClient.Demo
{
    public partial class DemoContext : INotifyPropertyChanged
    {
        /// Default constructed in file DemoContext.NotIncluded.cs
        /// You may include there your own variables init to make tests easier to run

        const int MAX_LOG = 500;
        private string _AuthUrl;
        public string AuthUrl
        {
            get { return _AuthUrl; }
            set { bool changed = _AuthUrl != value; _AuthUrl = value; if (changed) NotifyPropertyChanged(nameof(AuthUrl)); }
        }

        private string _AuthName;
        public string AuthName
        {
            get { return _AuthName; }
            set { bool changed = _AuthName != value; _AuthName = value; if (changed) NotifyPropertyChanged(nameof(AuthName)); }
        }

        private string _AuthPassword;
        public string AuthPassword
        {
            get { return _AuthPassword; }
            set { bool changed = _AuthPassword != value; _AuthPassword = value; if (changed) NotifyPropertyChanged(nameof(AuthPassword)); }
        }

        private string _AuthDomain = "Default";
        public string AuthDomain
        {
            get { return _AuthDomain; }
            set { bool changed = _AuthDomain != value; _AuthDomain = value; if (changed) NotifyPropertyChanged(nameof(AuthDomain)); }
        }

        private string _Token;
        public string Token
        {
            get { return _Token; }
            set { bool changed = _Token != value; _Token = value; if (changed) NotifyPropertyChanged(nameof(Token)); }
        }



        private string _ObjectStoreUrl;
        public string ObjectStoreUrl
        {
            get { return _ObjectStoreUrl; }
            set { bool changed = _ObjectStoreUrl != value; _ObjectStoreUrl = value; if (changed) NotifyPropertyChanged(nameof(ObjectStoreUrl)); }
        }

        private string _Container;
        public string Container
        {
            get { return _Container; }
            set { bool changed = _Container != value; _Container = value; if (changed) NotifyPropertyChanged(nameof(Container)); }
        }

        private string _NewFileSource;
        public string NewFileSource
        {
            get { return _NewFileSource; }
            set { bool changed = _NewFileSource != value; _NewFileSource = value; if (changed) NotifyPropertyChanged(nameof(NewFileSource)); }
        }

        private string _NewFileName;
        public string NewFileName
        {
            get { return _NewFileName; }
            set { bool changed = _NewFileName != value; _NewFileName = value; if (changed) NotifyPropertyChanged(nameof(NewFileName)); }
        }


        public ObservableCollection<SwiftContainerInfoResponse.ContainerFileObject> Files { get; set; } = new ObservableCollection<SwiftContainerInfoResponse.ContainerFileObject>();

        private SwiftContainerInfoResponse.ContainerFileObject _SelectedFile;
        public SwiftContainerInfoResponse.ContainerFileObject SelectedFile
        {
            get { return _SelectedFile; }
            set { bool changed = _SelectedFile != value; _SelectedFile = value; if (changed) NotifyPropertyChanged(nameof(SelectedFile)); }
        }

        public ObservableCollection<string> LogCollection { get; set; } = new ObservableCollection<string>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public void Log(string line)
        {
            LogCollection.Insert(0, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + line);
            //LogCollection.Add(line);
            if (LogCollection.Count > MAX_LOG + 30)
                for (var i = LogCollection.Count - 1; i > MAX_LOG; i--)
                    LogCollection.RemoveAt(i);
        }

        public ICommand AuthenticateCommand =>
            new Command(async (p) => await AuthenticateAsync());

        public async Task AuthenticateAsync()
        {
            var c = new SwiftClient() { };
            var resp = await c.AuthenticateAsync(AuthUrl, AuthName, AuthPassword, AuthDomain);
            Log($"Authentication {(resp.IsSuccess ? "" : " not ")} successful");
            if (resp.IsSuccess)
            {
                Log($"Token: {resp.Token}");
                Token = resp.Token;
            }
        }

        public ICommand ListContainersCommand =>
            new Command(async (p) => await ListContainersAsync());

        public async Task ListContainersAsync()
        {
            var c = new SwiftClient();

            if(!await AuthenticateIfNeededAsync())
            {
                Log($"Could not authenticated: could not retreive a valid token");
                return;
            }
            c.InitToken(Token);
            var resp = await c.AccountListContainersAsync(ObjectStoreUrl);

            Log($"Containers listing {(resp.IsSuccess ? "" : "not ")}successful");
            if (resp.IsSuccess)
            {
                Log($"{resp.ContentObject.Count} containers");
                foreach (var cont in resp.ContentObject)
                {
                    Log($"Container: {cont.Name} ({cont.Bytes} bytes, last modified {cont.Last_modified})");
                }
                if (resp.ContentObject.Count > 0)
                    Container = resp.ContentObject.First().Name;
            }
        }

        public ICommand ContainerInfoCommand =>
    new Command(async (p) => await ContainerInfoAsync());

        public async Task ContainerInfoAsync()
        {
            var c = new SwiftClient();

            if(!await AuthenticateIfNeededAsync())
            {
                Log($"Could not authenticated: could not retreive a valid token");
                return;
            }
            c.InitToken(Token);
            var resp = await c.ContainerGetAsync(ObjectStoreUrl, Container);

            Log($"Container '{Container}' infos received {(resp.IsSuccess ? "" : "not ")}successfully");
            if (resp.IsSuccess)
            {
                Log($"{resp.ContentObject.Count} file(s)");
                Files.Clear();

                foreach (var file in resp.ContentObject)
                {
                    Log($"file: {file.Name} ({file.Bytes} bytes, last modified {file.Last_modified}, content type: '{file.Content_type}')");
                    Files.Add(file);
                }
            }
        }

        public ICommand SendFileCommand =>
    new Command(async (p) => await SendFileAsync());

        public async Task SendFileAsync()
        {
            var c = new SwiftClient();

            if(!await AuthenticateIfNeededAsync())
            {
                Log($"Could not authenticated: could not retreive a valid token");
                return;
            }
            c.InitToken(Token);

            var file = System.IO.File.OpenRead(NewFileSource);

            string contentType = "application/octet-stream";

            contentType = MimeTypeMap.GetMimeType(System.IO.Path.GetExtension(NewFileSource));

            var resp = await c.ObjectPutAsync(ObjectStoreUrl, Container, NewFileName, file, contentType);

            Log($"File '{NewFileName}' {(resp.IsSuccess ? "" : "not ")}sent successfully");
            //if (resp.IsSuccess)
            //{
            //    Log($"{resp.ContentObject.Count} file(s)");
            //    Files.Clear();

            //    foreach (var file in resp.ContentObject)
            //    {
            //        Log($"file: {file.Name} ({file.Bytes} bytes, last modified {file.LastModified}, content type: '{file.Content_type}')");
            //        Files.Add(file);
            //    }
            //    if (resp.ContentObject.Count > 0)
            //        Container = resp.ContentObject.First().Name;
            //}
            await ContainerInfoAsync();

        }

        public ICommand DeleteFileCommand =>
    new Command(async (p) => await DeleteFileAsync());

        public async Task DeleteFileAsync()
        {

            var file = SelectedFile;
            if(file==null)
            {
                Log($"No file selected");
                return;
            }

            var c = new SwiftClient();

            if (!await AuthenticateIfNeededAsync())
            {
                Log($"Could not authenticated: could not retreive a valid token");
                return;
            }
            c.InitToken(Token);


            var resp = await c.ObjectDeleteAsync(ObjectStoreUrl, Container, file.Name);

            Log($"File '{file.Name}' {(resp.IsSuccess ? "" : "not ")}deleted successfully");

            await ContainerInfoAsync();
        }


        private async Task<bool> AuthenticateIfNeededAsync()
        {
            if (!Token.IsNullOrEmpty())
                Log($"Already authenticated");
            else
                await AuthenticateAsync();
            return !Token.IsNullOrEmpty();
        }
    }

    public class Command : ICommand
    {
        private Action<object> _Action;

        public Command(Action<object> action)
        {
            _Action = action;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _Action != null;
        }

        public void Execute(object parameter)
        {
            _Action.Invoke(parameter);
        }
    }
}
