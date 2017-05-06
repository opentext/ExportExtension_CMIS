using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Globalization;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    public class CMISViewModel_TT : ModelBase
    {
        #region Constructor
        private CMISViewModel vm;
        private CMISSettings settings;

        public CMISViewModel_TT(CMISViewModel vm)
        {
            this.vm = vm;
            this.settings = vm.CMISSettings;
            Warning = "No properties loaded yet";
            Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(n => n.DisplayName).ToList();
        }

        public void Initialize(UserControl control)  {   }

        public bool ActivateTab()
        {
            if (!vm.DebugMode) initializeTypeTree();
            return true;
        }
        #endregion

        #region Properties
        private string TypeNodeSelected_name = "TypeNodeSelected";
        public bool TypeNodeSelected { get { return settings.SerializedTypePath != null; } }

        private string PropertiesLoadedd_name = "PropertiesLoaded";
        public bool PropertiesLoaded { get { return settings.SelectedType != null; } }

        private SIEETreeView _types;
        public SIEETreeView Types
        {
            get { return _types; }
            set { SetField(ref _types, value); }
        }
        public string SelectedTypePath
        {
            get { return settings.SelectedTypePath; }
            set { settings.SelectedTypePath = value; SendPropertyChanged(); }
        }

        public ObservableCollection<CMISProperty> Properties
        {
            get { return settings.Properties; }
            set { settings.Properties = value; SendPropertyChanged(); }
        }

        private CMISType _loadedType;
        public CMISType LoadedType
        {
            get { return _loadedType; }
            set { SetField(ref _loadedType, value); }
        }

        private string _warning;
        public string Warning
        {
            get { return _warning; }
            set { SetField(ref _warning, value); }
        }

        private List<CultureInfo> cultures;
        public List<CultureInfo> Cultures
        {
            get { return cultures; }
            set { SetField(ref cultures, value); }
        }
        public CultureInfo SelectedCulture
        {
            get { return new CultureInfo(settings.SelectedCultureInfoName); }
            set
            {
                settings.SelectedCultureInfoName = value.Name;
                SendPropertyChanged();
            }
        }
        #endregion

        #region Event handler
        public void SetSelectTypeHandler(TVIViewModel typeNode)
        {
            SelectedTypePath = typeNode.GetDisplayNamePath();
            settings.SerializedTypePath = typeNode.GetSerializedPath();
            RaisePropertyChanged(TypeNodeSelected_name);
        }

        public void SelectAllHandler()
        {
            foreach (CMISProperty p in Properties) { p.Selected = true; }
        }

        public void DeselectAllHandler()
        {
            foreach (CMISProperty p in Properties) { p.Selected = false; }
        }
        #endregion
        
        #region Functions
        private void initializeTypeTree()
        {
            Types = new SIEETreeView(vm);
            Types.AddItem(new TVIViewModel(new CMISTypeNode(null, vm.CMISClient.GetRootType()), null, true));
            TVIViewModel tvivm = Types.InitializeTree(settings.SerializedTypePath, typeof(CMISTypeNode));
        }

        public void LoadProperties()
        {
            Properties.Clear();
            try
            {
                CMISTypeNode tn = (TVIViewModel.deSerialize(settings.SerializedTypePath.Last(), typeof(CMISTypeNode)) as CMISTypeNode);
                CMISType type = vm.CMISClient.GetTypeFromId(tn.Id);
                foreach (CMISProperty pd in vm.CMISClient.GetPropertyDefinitions(type))
                    if (pd.Id !="cmis:name") Properties.Add(pd);
                settings.SelectedType = type.Id;
                RaisePropertyChanged(PropertiesLoadedd_name);
                LoadedType = type;
            }
            catch (Exception e) {throw new Exception(
                "Could not load " + SelectedTypePath + 
                ". Reason:\n" + e.Message); }
        }
        #endregion
    }
}
