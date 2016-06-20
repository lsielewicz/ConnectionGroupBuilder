using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using ConnectionGroupBuilder.Message;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ConnectionGroupBuilder.ViewModel
{
    public class PowerShellScriptViewModel : ViewModelBase
    {
        private bool _progressBarVisibility;
        private bool _isExecutionEnabled;
        private string _scriptContent;
        private RelayCommand _saveScriptCommand;
        private RelayCommand _executeScriptCommand;
        private BackgroundWorker _executionBackgroundWorker;


        public PowerShellScriptViewModel()
        {
            IsExecutionEnabled = true;
            Messenger.Default.Register<ScriptContentMessage>(this,x=>HandleScriptMessage(x.PowerShellScript));

            _executionBackgroundWorker = new BackgroundWorker();
            _executionBackgroundWorker.DoWork += DoExecutionWork;
            _executionBackgroundWorker.RunWorkerCompleted += RunExecutionWorkCompleted;
        }



        public bool IsExecutionEnabled
        {
            get
            {
                return _isExecutionEnabled;
            }
            set
            {
                if (_isExecutionEnabled != value)
                {
                    _isExecutionEnabled = value;
                    RaisePropertyChanged("IsExecutionEnabled");
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
        public string ScriptContent
        {
            get
            {
                return _scriptContent;
            }
            set
            {
                if (_scriptContent != value)
                {
                    _scriptContent = value;
                    RaisePropertyChanged("ScriptContent");
                }
            }
        }

        public RelayCommand SaveScriptCommand
        {
            get
            {
                return _saveScriptCommand ?? (_saveScriptCommand = new RelayCommand(SaveScript));
            }
        }

        public RelayCommand ExecuteScriptCommand
        {
            get
            {
                return _executeScriptCommand ?? (_executeScriptCommand = new RelayCommand(ExecuteScript));
            }
        }


        private void SaveScript()
        {
            try
            {
                string location = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                location = System.IO.Path.Combine(location, "psscript" + ".ps1");
                SaveFileDialog saveDlg = new SaveFileDialog();
                saveDlg.FileName = "psscript";
                saveDlg.DefaultExt = ".ps1";
                saveDlg.Filter = "PowerShell files (.ps1)|*.ps1";
                bool? result = saveDlg.ShowDialog();
                if (result == true)
                    location = saveDlg.FileName;

                File.WriteAllText(location, ScriptContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExecuteScript()
        {
            if(!_executionBackgroundWorker.IsBusy)
                _executionBackgroundWorker.RunWorkerAsync();
        }

        private void HandleScriptMessage(string script)
        {
            ScriptContent = script;
            IsExecutionEnabled = true;
        }

        #region Backgroundworker
        private void DoExecutionWork(object sender, DoWorkEventArgs e)
        {
            ProgressBarVisibility = true;
            try
            {
                using (PowerShell powerShellInstance = PowerShell.Create())
                {
                    powerShellInstance.AddScript(ScriptContent);
                    Collection<PSObject> psOutput = powerShellInstance.Invoke();

                    foreach (PSObject outline in psOutput)
                    {
                        if (outline != null)
                        {
                            //TODO something 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RunExecutionWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBarVisibility = false;
            IsExecutionEnabled = false;
        }
        #endregion



    }
}
