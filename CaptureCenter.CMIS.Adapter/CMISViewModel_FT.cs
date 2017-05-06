using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using System.Windows.Controls;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    public class CMISViewModel_FT : ModelBase
    {
        #region Construction
        private CMISViewModel vm;
        private CMISSettings settings;

        private string noFolderWarining = "No folder selected yet";
        private string noVersioningTypeWarning = "The selected type does not support versioning";

        public CMISViewModel_FT(CMISViewModel vm)
        {
            this.vm = vm;
            settings = vm.CMISSettings;

            ConflictHandlingMethods = new List<ConflictHandlingMethod>() {
                new ConflictHandlingMethod() { Name = "None", Value = CMISSettings.ConflictHandling.None },
                new ConflictHandlingMethod() { Name = "Replace file", Value = CMISSettings.ConflictHandling.Replace },
                new ConflictHandlingMethod() { Name = "Add consecutive number", Value = CMISSettings.ConflictHandling.AddNumber },
                new ConflictHandlingMethod() { Name = "Add raondom string", Value = CMISSettings.ConflictHandling.AddBlurb },
                new ConflictHandlingMethod() { Name = "Create new version", Value = CMISSettings.ConflictHandling.AddVersion },
            };
        }

        public void Initialize(UserControl control)
        {
            vm.TT.PropertyChanged += (s, e) => {
                RaisePropertyChanged(Warning_name);
                RaisePropertyChanged(WarningVisible_name);
            };
        }

        public bool ActivateTab()
        {
            if (!vm.DebugMode) initializeFolderTree();
            return true;
        }
        #endregion

        #region Properties: proxy for settings
        public string SelectedFolderPath
        {
            get { return settings.SelectedFolderPath; }
            set { settings.SelectedFolderPath = value; SendPropertyChanged(); }
        }

        public List<string> SerializedFolderPath
        {
            get { return settings.SerializedFolderPath; }
            set { settings.SerializedFolderPath = value; RaisePropertyChanged(WarningVisible_name); }
        }

        public bool UseSubFolderField
        {
            get { return settings.UseSubFolderField; }
            set { settings.UseSubFolderField = value; SendPropertyChanged(); }
        }
        public string SubFolderField
        {
            get { return settings.SubFolderField; }
            set { settings.SubFolderField = value; SendPropertyChanged(); }
        }

        public bool UseSubFolderType
        {
            get { return settings.UseSubFolderType; }
            set { settings.UseSubFolderType = value; SendPropertyChanged(); }
        }
        public bool SubFolderTypeFixed
        {
            get { return settings.SubFolderTypeFixed; }
            set {
                settings.SubFolderTypeFixed = value;
                if (value) SubFolderTypeFromField = false;
                SendPropertyChanged();
            }
        }
        public string SubFolderType
        {
            get { return settings.SubFolderType; }
            set { settings.SubFolderType = value; SendPropertyChanged(); }
        }

        public bool SubFolderTypeFromField
        {
            get { return settings.SubFolderTypeFromField; }
            set {
                settings.SubFolderTypeFromField = value;
                if (value) SubFolderTypeFixed = false;
                SendPropertyChanged();
            }
        }
        public string SubFolderTypeField
        {
            get { return settings.SubFolderTypeField; }
            set { settings.SubFolderTypeField = value; SendPropertyChanged(); }
        }

        public CMISSettings.ConflictHandling SelectedConflictHandling
        {
            get { return settings.SelectedConflictHandling; }
            set
            {
                settings.SelectedConflictHandling = value;
                SendPropertyChanged();
                RaisePropertyChanged(NumberOfDigitsVisible_name);
                RaisePropertyChanged(VersioningVisible_name);
                RaisePropertyChanged(Warning_name);
                RaisePropertyChanged(WarningVisible_name);
            }
        }

        public int NumberOfDigits
        {
            get { return settings.NumberOfDigits; }
            set { settings.NumberOfDigits = value; SendPropertyChanged(); }
        }

        public bool Major
        {
            get { return settings.Major; }
            set { settings.Major = value; SendPropertyChanged(); }
        }
        #endregion

        #region Properties: view model
        private string WarningVisible_name = "WarningVisible";
        public bool WarningVisible { get {
                return settings.SerializedFolderPath == null || versionConflict();
            } }

        private SIEETreeView _folders;
        public SIEETreeView Folders
        {
            get { return _folders; }
            set { SetField(ref _folders, value); }
        }

        public class ConflictHandlingMethod
        {
            public CMISSettings.ConflictHandling Value { get; set; }
            public string Name { get; set; }
        }
        private List<ConflictHandlingMethod> _conflictHandlingMethods;
        public List<ConflictHandlingMethod> ConflictHandlingMethods
        {
            get { return _conflictHandlingMethods; }
            set { SetField(ref _conflictHandlingMethods, value); }
        }

        private string NumberOfDigitsVisible_name = "NumberOfDigitsVisible";
        public string NumberOfDigitsVisible
        {
            get { return SelectedConflictHandling == CMISSettings.ConflictHandling.AddNumber ? "Visible" : "Collapsed"; }
        }
        private string VersioningVisible_name = "VersioningVisible";
        public string VersioningVisible
        {
            get { return SelectedConflictHandling == CMISSettings.ConflictHandling.AddVersion ? "Visible" : "Collapsed"; }
        }

        private string Warning_name = "Warning";
        public string Warning
        {
            get { return versionConflict()? noVersioningTypeWarning :  noFolderWarining; }
        }
        #endregion

        #region Event handler
        // called from user clicking somewhere in the tree
        public void SetSelectedFolderHandler(TVIViewModel folderNode)
        {
            SelectedFolderPath = folderNode.GetDisplayNamePath();
            SerializedFolderPath = folderNode.GetSerializedPath();
        }
        #endregion

        #region Functions
        private void initializeFolderTree()
        {
            Folders = new SIEETreeView(vm);
            Folders.AddItem(new TVIViewModel(new CMISFolderNode(null, vm.CMISClient.GetRootFolder()), null, true));
            Folders.InitializeTree(SerializedFolderPath, typeof(CMISFolderNode));
        }

        private bool versionConflict()
        {
            return 
                vm.TT.LoadedType != null && vm.TT.LoadedType.Versionable == false && 
                SelectedConflictHandling == CMISSettings.ConflictHandling.AddVersion;
        }
    }
    #endregion
}
