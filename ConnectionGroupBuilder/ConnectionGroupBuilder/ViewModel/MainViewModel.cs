using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ConnectionGroupBuilder.Core.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System.Resources;
using System.Reflection;
using ConnectionGroupBuilder.Message;
using ConnectionGroupBuilder.View;
using GalaSoft.MvvmLight.Messaging;

namespace ConnectionGroupBuilder.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        #region private fields
        private ObservableCollection<string> _packagesCollection;
        private AppVPackageService _appVPackageService;
        private BackgroundWorker _bworker;
        private RelayCommand _pickFileCommand;
        private RelayCommand _generateXmlCommand;
        private RelayCommand _changeVersionCommand;
        private ResourceManager _resourceManager;
        private string _connectionGroupVersion;
        private string _connectionGroupDisplayName;
        private string _pathToFile;
        private bool _progressBarVisibility;
        private int _connectionGroupPriority;
        #endregion

        public MainViewModel()
        {
            _resourceManager = new ResourceManager("ConnectionGroupBuilder.Strings.Resources", Assembly.GetExecutingAssembly());

            ConnectionGroupVersion = Guid.NewGuid().ToString();
            ConnectionGroupDisplayName = "ConnectionGroup" + Guid.NewGuid().ToString();
            ConnectionGroupPriority = 0;

            PackagesCollection = new ObservableCollection<string>();
            _appVPackageService = new AppVPackageService();
            _bworker = new BackgroundWorker();
            _bworker.DoWork += this.DoWork;
            _bworker.RunWorkerCompleted += this.RunWorkerCompleted;
        }

        #region getters/setters
        public ObservableCollection<string> PackagesCollection
        {
            get
            {
                return _packagesCollection;
            }
            set
            {
                if (_packagesCollection != value)
                {
                    _packagesCollection = value;
                    RaisePropertyChanged("PackagesCollection");
                }
            }
        }


        public bool ProgressBarVisibility
        {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                if (_progressBarVisibility != value)
                {
                    _progressBarVisibility = value;
                    RaisePropertyChanged("ProgressBarVisibility");
                }
            }
        }


        public string PathToFile
        {
            get
            {
                return _pathToFile;
            }
            set
            {
                if (_pathToFile != value)
                {
                    _pathToFile = value;
                    RaisePropertyChanged("PathToFile");
                }
            }
        }

        public string ConnectionGroupVersion
        {
            get
            {
                return _connectionGroupVersion;
            }
            set
            {
                if (_connectionGroupVersion != value) 
                {
                    _connectionGroupVersion = value;
                    RaisePropertyChanged("ConnectionGroupVersion");
                }
            }
        }

        public string ConnectionGroupDisplayName
        {
            get
            {
                return _connectionGroupDisplayName;
            }
            set
            {
                if (_connectionGroupDisplayName != value)
                {
                    _connectionGroupDisplayName = value;
                    RaisePropertyChanged("ConnectionGroupDisplayName");
                }
            }
        }

        public int ConnectionGroupPriority
        {
            get
            {
                return _connectionGroupPriority;
            }
            set
            {
                if (_connectionGroupPriority != value)
                {
                    _connectionGroupPriority = value;
                    RaisePropertyChanged("ConnectionGroupPriority");
                }
            }
        }
        #endregion

        #region buttons getters 
        public RelayCommand PickFileCommand
        {
            get
            {
                return _pickFileCommand ?? (_pickFileCommand = new RelayCommand(PickFile));
            }
        }

        public RelayCommand GenerateXmlCommand
        {
            get
            {
                return _generateXmlCommand ?? (_generateXmlCommand = new RelayCommand(GenerateXml));
            }
        }

        public RelayCommand ChangeVersionCommand
        {
            get
            {
                return _changeVersionCommand ?? (_changeVersionCommand = new RelayCommand(ChangeVersion));
            }
        }
        #endregion


        #region Methods/Actions
        private void GenerateXml()
        {
            try
            {
                string location = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                location = System.IO.Path.Combine(location, ConnectionGroupDisplayName + ".xml");
                SaveFileDialog saveDlg = new SaveFileDialog();
                saveDlg.FileName = ConnectionGroupDisplayName;
                saveDlg.DefaultExt = ".xml";
                saveDlg.Filter = "XML files (.xml)|*.xml";
                bool? result = saveDlg.ShowDialog();
                if (result == true)
                    location = saveDlg.FileName;

                if (PackagesCollection == null || PackagesCollection.Count < 2)
                {
                    MessageBox.Show(_resourceManager.GetString("ToLessPackagesError"), _resourceManager.GetString("Error"));
                    return;
                }
                if (ConnectionGroupVersion != String.Empty && ConnectionGroupDisplayName != String.Empty && ConnectionGroupPriority is int)
                {
                    _appVPackageService.GenerateXmlConnectionGroup(ConnectionGroupVersion, ConnectionGroupDisplayName, Convert.ToString(ConnectionGroupPriority),location);
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString("BadPropertiesError"), _resourceManager.GetString("Error"));
                }
                OpenScriptWindow();
                ClearCommands();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenScriptWindow()
        {
            if (_appVPackageService != null && _appVPackageService.PowerShellScript != String.Empty)
            {
                PowerShellScriptWindow window = new PowerShellScriptWindow();
                Messenger.Default.Send<ScriptContentMessage>(
                    new ScriptContentMessage(_appVPackageService.PowerShellScript));

                window.Show();
            }
        }

        private void PickFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.Filter = "APPV files (.appv)|*.appv";
            if (dlg.ShowDialog() == true)
            {
                PathToFile = dlg.FileName;
            }
            if (!_bworker.IsBusy)
                _bworker.RunWorkerAsync();
            

        }

        private void ChangeVersion()
        {
            ConnectionGroupVersion = Guid.NewGuid().ToString();
        }

        private void ClearCommands()
        {
            ConnectionGroupVersion = Guid.NewGuid().ToString();
            ConnectionGroupDisplayName = "ConnectionGroup" + Guid.NewGuid().ToString();
            ConnectionGroupPriority = 0;
            PackagesCollection.Clear();
            PathToFile = String.Empty;
        }

        #endregion

        #region BackgroundWorker
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressBarVisibility = true;
            if (!String.IsNullOrEmpty(PathToFile))
            {
                try
                {
                    _appVPackageService.ExtractDataFromManifest(PathToFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }


        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PackagesCollection = new ObservableCollection<string>();
            _appVPackageService.PackagesList.ForEach(x=>PackagesCollection.Add(String.Format("{0} - {1}",x.DisplayName, x.PackageId)));
            ProgressBarVisibility = false;
        }

        #endregion
    }
}