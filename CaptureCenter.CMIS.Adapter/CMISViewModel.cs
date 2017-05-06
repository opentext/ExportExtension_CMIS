using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    public class CMISViewModel : SIEEViewModel
    {
        #region Construction
        public CMISSettings CMISSettings { get; set; }
        public ICMISClient CMISClient { get; set; }

        public CMISViewModel_CT CT { get; set; }
        public CMISViewModel_FT FT { get; set; }
        public CMISViewModel_DT DT { get; set; }
        public CMISViewModel_TT TT { get; set; }
        public CMISViewModel_ST ST { get; set; }

        public CMISViewModel(SIEESettings settings, ICMISClient cmisClient) : base() 
        {
            CMISSettings = settings as CMISSettings;
            CMISClient = cmisClient;
            CT = new CMISViewModel_CT(this);
            FT = new CMISViewModel_FT(this);
            DT = new CMISViewModel_DT(this);
            TT = new CMISViewModel_TT(this);
            ST = new CMISViewModel_ST(this);

            SelectedTab = 0;
            IsRunning = false;
            DataLoaded = false;

            if (this.CMISSettings.LoadRepositoriesPossible)
            {
                LoadRepositoriesButtonHandler();
                if (this.CMISSettings.ConnectPossible) ConnectdButtonHandler();
            }

            CT.PropertyChanged += (s, e) => {
                if (CT.IsConnectionRelevant(e.PropertyName))
                {
                    DataLoaded = false;
                    if (e.PropertyName != CT.SelectedRepository_name)
                    {
                        RepositoriesLoaded = false;
                        CMISSettings.LoadRepositoriesPossible = false;
                    }
                    CMISSettings.ConnectPossible = false;
                }
            };
        }

        public override void Initialize(UserControl control)
        {
            CT.Initialize(control);
            FT.Initialize(control);
            DT.Initialize(control);
            TT.Initialize(control);
            ST.Initialize(control);
            initializeTabnames(control);
        }

        public override SIEESettings Settings
        {
            get { return CMISSettings; }
        }

        public override void OpenTabs(object sender, ExecutedRoutedEventArgs e)
        {
            DebugMode = true;
            RaisePropertyChanged(DataLoaded_name);
        }
        #endregion

        #region Properties
        public bool DebugMode { get; set; } = false;

        private int _selectedTab;
        public int SelectedTab
        {
            get { return _selectedTab; }
            set { SetField(ref _selectedTab, value); }
        }
        private string DataLoaded_name = "DataLoaded";
        private bool _dataLoaded;
        public bool DataLoaded
        {
            get { return _dataLoaded || DebugMode; }
            set { SetField(ref _dataLoaded, value); }
        }
        private bool _repositoriesLoaded;
        public bool RepositoriesLoaded
        {
            get { return _repositoriesLoaded; }
            set { SetField(ref _repositoriesLoaded, value); }
        }
        #endregion

        #region Functions
        // Common handler for the events
        private delegate void handler();
        private bool callHandler(handler h, string message)
        {
            bool result = false;
            IsRunning = true;
            try { h(); result = true; }
            catch (Exception e) { SIEEMessageBox.Show(e.Message, message, MessageBoxImage.Error); }
            finally { IsRunning = false; }
            return result;
        }

        public void LoadRepositoriesButtonHandler()
        {
            if (callHandler(CT.LoadRepositories, "Couldn't connect (login)"))
            {
                RepositoriesLoaded = true;
                CMISSettings.LoadRepositoriesPossible = true;
            }
        }

        public void ConnectdButtonHandler()
        {
            if (callHandler(CT.Connect, "Couldn't load data"))
            {
                CMISSettings.ConnectPossible = true;
                TabNamesReset();
                DataLoaded = true;
                if (callHandler(callFTActivateTab, "Couldn't load folders"))
                    SelectedTab = 1;
            }
        }

        private void callFTActivateTab() {  FT.ActivateTab(); }

        public void LoadPropertiesHandler()
        {
            callHandler(TT.LoadProperties, "Couldn't load Properties");
        }
        #endregion

        #region Tab activation
        // Call tab actication function only once as long as the connections settings have not changed
        // This dictionary stores whether the tab has alreay been activated.
        public Dictionary<string, bool> Tabnames;

        // Retrieve tabitem names from user control
        private void initializeTabnames(UserControl control)
        {
            Tabnames = new Dictionary<string, bool>();
            TabControl tc = (TabControl)LogicalTreeHelper.FindLogicalNode(control, "mainTabControl");
            foreach (TabItem tabItem in LogicalTreeHelper.GetChildren(tc)) Tabnames[tabItem.Name] = false;
        }
        private void TabNamesReset()
        {
            if (Tabnames != null) 
                foreach (string tn in Tabnames.Keys.ToList()) Tabnames[tn] = false;
        }

        public void ActivateTab(string tabName)
        {
            if (Tabnames[tabName]) return;
            IsRunning = true;
            try
            {
                switch (tabName)
                {
                    case "connectionTab":       { Tabnames[tabName] = CT.ActivateTab(); break; }
                    case "folderTab":           { Tabnames[tabName] = FT.ActivateTab(); break; }
                    case "typesTab":            { Tabnames[tabName] = TT.ActivateTab(); break; }
                    case "secondaryTypesTab":   { Tabnames[tabName] = ST.ActivateTab(); break; }
                    case "documentTab":         { Tabnames[tabName] = DT.ActivateTab(CMISSettings.CreateSchema()); break; }
                }
            }
            catch (Exception e)
            {
                SIEEMessageBox.Show(e.Message, "Error in " + tabName, MessageBoxImage.Error);
                DataLoaded = false;
                SelectedTab = 0;
                TabNamesReset();
            }
            finally { IsRunning = false; }
        }
        #endregion

    }
}
