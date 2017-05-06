using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    #region Secondary types
    public enum CMIS_Type { CMIS_String, CMIS_Integer };

    public class SecondaryType : ModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        private CMIS_Type _type;
        public CMIS_Type Type
        {
            get { return _type; }
            set { SetField(ref _type, value); }
        }
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { SetField(ref _selected, value); }
        }
    }
    #endregion

    public class CMISViewModel_ST : ModelBase
    {
        #region Constructor
        private CMISViewModel vm;
        private CMISSettings settings;

        public CMISViewModel_ST(CMISViewModel vm)
        {
            this.vm = vm;
            this.settings = vm.CMISSettings;
            SecondaryTypes = new ObservableCollection<SecondaryType>();
        }

        public void Initialize(UserControl control) { }

        public bool ActivateTab() { return true; }
        #endregion

        #region Properties
        private ObservableCollection<SecondaryType> _secondaryTypes;
        public ObservableCollection<SecondaryType> SecondaryTypes
        {
            get { return _secondaryTypes; }
            set { SetField(ref _secondaryTypes, value); }
        }
         #endregion
    }
}
