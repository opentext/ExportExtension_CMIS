using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Serialization;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    [Serializable]
    public class CMISSettings : SIEESettings
    {
        #region Construction
        public enum ConflictHandling { None, Replace, AddVersion, AddNumber, AddBlurb };

        public CMISSettings() : base() 
        {
            // Connection tab
            ServerURL = "http://<Server URL>";
            Binding = TypeOfBinding.Atom;
            SelectedRepository = new CMISRepository();
            LoadRepositoriesPossible = false;
            connectPossible = false;

            // Folder tab
            UseSubFolderField = false;
            SubFolderField = "FolderField";
            UseSubFolderType = false;
            SubFolderType = "cmis:folder";
            SubFolderTypeFixed = true;
            SubFolderTypeFromField = false;
            SubFolderTypeField = "FolderTypeField";
            SelectedConflictHandling = ConflictHandling.None;
            NumberOfDigits = 4;
            Major = true;

            // Types tab
            Properties = new ObservableCollection<CMISProperty>();
            SelectedCultureInfoName = CultureInfo.CurrentCulture.Name;

            // Document tab
            UseSpecification = true;
            Specification = "<BATCHID>_<DOCUMENTNUMBER>";
        }
        #endregion

        #region Properties (Connection)
        private string _serverURL;
        public string ServerURL
        {
            get { return _serverURL; }
            set { SetField(ref _serverURL, value); }
        }

        private TypeOfBinding _binding;
        public TypeOfBinding Binding
        {
            get { return _binding; }
            set { SetField(ref _binding, value); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetField(ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetField(ref _password, value); }
        }

        private CMISRepository _selectedRepository;
        public CMISRepository SelectedRepository
        {
            get { return _selectedRepository; }
            set { SetField(ref _selectedRepository, value); }
        }

        private bool _secondaryTypesSupported;
        public bool SecondaryTypesSupported
        {
            get { return _secondaryTypesSupported; }
            set { SetField(ref _secondaryTypesSupported, value); }
        }

        private bool lLoadRepositoriesPossible;
        public bool LoadRepositoriesPossible
        {
            get { return lLoadRepositoriesPossible; }
            set { SetField(ref lLoadRepositoriesPossible, value); }
        }
        private bool connectPossible;
        public bool ConnectPossible
        {
            get { return connectPossible; }
            set { SetField(ref connectPossible, value); }
        }
        #endregion

        #region Properties (Folder)
        // Tree view
        private string _selectedFolderPath;
        public string SelectedFolderPath
        {
            get { return _selectedFolderPath; }
            set { SetField(ref _selectedFolderPath, value); }
        }
        private List<string> serializedFolderPath;
        public List<string> SerializedFolderPath
        {
            get { return serializedFolderPath; }
            set { SetField(ref serializedFolderPath, value); }
        }
        
        // Folder field
        private bool _useSubFolderField;
        public bool UseSubFolderField
        {
            get { return _useSubFolderField; }
            set { SetField(ref _useSubFolderField, value); }
        }
        private string _subFolderField;
        public string SubFolderField
        {
            get { return _subFolderField; }
            set { SetField(ref _subFolderField, value); }
        }
        
        // Folder type
        private bool _useSubFolderType;
        public bool UseSubFolderType
        {
            get { return _useSubFolderType; }
            set { SetField(ref _useSubFolderType, value); }
        }
        private bool _subFolderTypeFixed;
        public bool SubFolderTypeFixed
        {
            get { return _subFolderTypeFixed; }
            set { SetField(ref _subFolderTypeFixed, value); }
        }
        private string _subFolderType;
        public string SubFolderType
        {
            get { return _subFolderType; }
            set { SetField(ref _subFolderType, value); }
        }
        private bool _subFolderTypeFromField;
        public bool SubFolderTypeFromField
        {
            get { return _subFolderTypeFromField; }
            set { SetField(ref _subFolderTypeFromField, value); }
        }
        private string _subFolderTypeField;
        public string SubFolderTypeField
        {
            get { return _subFolderTypeField; }
            set { SetField(ref _subFolderTypeField, value); }
        }
        
        // Collision handling
        private ConflictHandling _selectedConflictHandling;
        public ConflictHandling SelectedConflictHandling
        {
            get { return _selectedConflictHandling; }
            set { SetField(ref _selectedConflictHandling, value); }
        }
        private int _numberOfDigits;
        public int NumberOfDigits
        {
            get { return _numberOfDigits; }
            set { SetField(ref _numberOfDigits, value); }
        }
        private bool _major;
        public bool Major
        {
            get { return _major; }
            set { SetField(ref _major, value); }
        }
        #endregion

        #region Properties (Types)
        private string _selectedType;
        public string SelectedType
        {
            get { return _selectedType; }
            set { SetField(ref _selectedType, value); }
        }
        private string _selectedTypePath;
        public string SelectedTypePath
        {
            get { return _selectedTypePath; }
            set { SetField(ref _selectedTypePath, value); }
        }
        private List<string> serializedTypePath;
        public List<string> SerializedTypePath
        {
            get { return serializedTypePath; }
            set { SetField(ref serializedTypePath, value); }
        }
        private ObservableCollection<CMISProperty> _properties;
        public ObservableCollection<CMISProperty> Properties
        {
            get { return _properties; }
            set { SetField(ref _properties, value); }
        }
        private string selectedCultureInfoName;
        public string SelectedCultureInfoName
        {
            get { return selectedCultureInfoName; }
            set { SetField(ref selectedCultureInfoName, value); }
        }
        #endregion

        #region Properties (Document)
        private bool useInputFileName;
        public bool UseInputFileName
        {
            get { return useInputFileName; }
            set { SetField(ref useInputFileName, value); }
        }

        private bool useSpecification;
        public bool UseSpecification
        {
            get { return useSpecification; }
            set { SetField(ref useSpecification, value); RaisePropertyChanged(specification_Name); }
        }

        private string specification_Name = "Specification";
        private string specification;
        public string Specification
        {
            get { return useSpecification ? specification : null; }
            set { SetField(ref specification, value); }
        }
        #endregion

        #region Properties (Secondary Types)
        #endregion

        #region Functions
        public void InitializeCMISClient(ICMISClient client)
        {
            client.ServerURL = ServerURL;
            client.Username = Username;
            client.Password = PasswordEncryption.Decrypt(Password);
            client.TypeOfBinding = Binding;
        }
        public string GetLocation()
        {
            return SelectedFolderPath + "@" + ServerURL;
        }

        public override string GetDocumentNameSpec() { return Specification; }

        // Add all selected properties to the schema and add the field named for subfolder and subfolder type
        public override SIEEFieldlist CreateSchema()
        {
            SIEEFieldlist schema = new SIEEFieldlist();
            foreach (CMISProperty p in Properties)
                if (p.Selected)
                    schema.Add(new SIEEField()
                    {
                        Name = p.Id.Replace(':', '_'),
                        ExternalId = p.Id,
                        Cardinality = p.IsMulti ? -1 : 0,
                    });
            if (UseSubFolderField && SubFolderTypeFixed)
                schema.Add(new SIEEField() { Name = SubFolderField, ExternalId = SubFolderField });
            if (UseSubFolderField && SubFolderTypeFromField)
                schema.Add(new SIEEField() { Name = SubFolderTypeField, ExternalId = SubFolderTypeField });
            return schema;
        }
        #endregion

        public override object Clone()
        {
            return this.MemberwiseClone() as CMISSettings;
        }
    }

   
}
