using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Globalization;
using System.Windows;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    public class CMISViewModel_CT : ModelBase
    {
        #region Constructor
        private CMISViewModel vm;
        private CMISSettings settings;

        public CMISViewModel_CT(CMISViewModel vm)
        {
            this.vm = vm;
            settings = vm.CMISSettings;
            Repositories = new List<CMISRepository>();
            Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(n => n.DisplayName).ToList();
            if (settings.Binding == TypeOfBinding.Atom)
                AtomBinding = true;
            else
                BrowserBinding = true;
            Warning = "Login not yet done";
        }

        public void Initialize(UserControl control)
        {
            findPasswordBox(control);
        }

        public bool ActivateTab() { return true; }
        #endregion

        #region Properties: for settings object
        private string ServerURL_name = "ServerURL";
        public string ServerURL
        {
            get { return settings.ServerURL;  }
            set  { settings.ServerURL = value; SendPropertyChanged(); }
        }

        private string AtomBinding_name = "AtomBinding";
        public bool AtomBinding
        {
            get { return settings.Binding == TypeOfBinding.Atom; }
            set {
                if (value)
                {
                    settings.Binding = TypeOfBinding.Atom;
                    BrowserBinding = WebServiceBinding = false;
                }
                SendPropertyChanged();
            }
        }
        public TypeOfBinding TypeOfBinding { get { return settings.Binding; } }

        private string BrowserBinding_name = "BrowserBinding";
        public bool BrowserBinding
        {
            get { return settings.Binding == TypeOfBinding.Browser; }
            set {
                if (value)
                {
                    settings.Binding = TypeOfBinding.Browser;
                    AtomBinding = WebServiceBinding = false;
                }
                SendPropertyChanged();
            }
        }

        private string WebServiceBinding_name = "WebServiceBinding";
        public bool WebServiceBinding
        {
            get { return settings.Binding == TypeOfBinding.WebService; }
            set
            {
                if (value)
                {
                    settings.Binding = TypeOfBinding.WebService;
                    AtomBinding = BrowserBinding = false;
                }
                SendPropertyChanged();
            }
        }

        private string Username_name = "Username";
        public string Username
        {
            get { return settings.Username; }
            set { settings.Username = value; SendPropertyChanged(); }
        }

        public string SelectedRepository_name = "SelectedRepository";
        public CMISRepository SelectedRepository
        {
            get { return settings.SelectedRepository; }
            set { settings.SelectedRepository = value; SendPropertyChanged(); }
        }

        public bool SecondaryTypesSupported
        {
            get { return settings.SecondaryTypesSupported; }
            set { settings.SecondaryTypesSupported = value; SendPropertyChanged(); }
        }
        #endregion

        #region Properties: for view model
        private List<CultureInfo> _cultures;
        public List<CultureInfo> Cultures
        {
            get { return _cultures; }
            set { SetField(ref _cultures, value); }
        }
        private List<CMISRepository> _repositories;
        public List<CMISRepository> Repositories
        {
            get { return _repositories; }
            set { SetField(ref _repositories, value); }
        }
        private string _warning;
        public string Warning
        {
            get { return _warning; }
            set { SetField(ref _warning, value); }
        }
        #endregion

        #region Password
        private string Password_name = "Password";
        public string Password
        {
            get
            {
                if (settings.Password == null) return string.Empty;
                return PasswordEncryption.Decrypt(settings.Password);
            }
            set
            {
                settings.Password = PasswordEncryption.Encrypt(value);
                SendPropertyChanged("Password");
            }
        }

        private PasswordBox passwordBox;
        private void findPasswordBox(UserControl control)
        {
            passwordBox = (PasswordBox)LogicalTreeHelper.FindLogicalNode(control, "passwordBox");
        }
        public void PasswordChangedHandler()
        {
            Password = SIEEUtils.GetUsecuredString(passwordBox.SecurePassword);
        }
        #endregion

        #region Functions
        public ICMISClient GetCMISClient()
        {
            return vm.CMISClient;
        }
        public bool IsConnectionRelevant(string property)
        {
            return
                property == ServerURL_name ||
                property == AtomBinding_name ||
                property == BrowserBinding_name ||
                property == WebServiceBinding_name ||
                property == Username_name ||
                property == Password_name ||
                property == SelectedRepository_name;
        }

        // Called from button click handler
        public void LoadRepositories()
        {
            settings.InitializeCMISClient(vm.CMISClient);
            Repositories = vm.CMISClient.LoadRepositories();

            // if the "SelectedRepository" is not in the list of repositories then select the first one
            if (Repositories != null)
            {
                string selectedRepositoryId = SelectedRepository == null? null : SelectedRepository.Id;
                List<CMISRepository> lrp = Repositories.Where(n => n.Id == selectedRepositoryId).ToList();
                SelectedRepository = (lrp.Count == 1)? lrp.First() : Repositories[0];
            }
         }

        // Called from button click handler
        public void Connect()
        {
            vm.CMISClient.SelectRepository(SelectedRepository.Id);
            SecondaryTypesSupported = vm.CMISClient.GetSupportedVersion() != "1.0";
        }
        #endregion

        #region Connection test handler
        private ConnectionTestResultDialog connectionTestResultDialog;
        private ConnectionTestHandler ConnectionTestHandler;

        // Set up objects, start tests (running in the backgroud) and launch the dialog
        public void CheckConnectionHandler()
        {
            settings.InitializeCMISClient(vm.CMISClient);

            VmTestResultDialog vmConnectionTestResultDialog = new VmTestResultDialog();
            ConnectionTestHandler = new eDocsConnectionTestHandler(vmConnectionTestResultDialog);
            ConnectionTestHandler.CallingViewModel = this;

            connectionTestResultDialog = new ConnectionTestResultDialog(ConnectionTestHandler);
            connectionTestResultDialog.DataContext = vmConnectionTestResultDialog;

            ConnectionTestHandler.LaunchTests();
            SIEEUtilsWPF.ShowDialog(connectionTestResultDialog);
        }
        #endregion
    }
}
