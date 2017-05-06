using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    [TestClass]
    public class TestCMISAdapter
    {
        public TestCMISAdapter() { SIEEMessageBox.Suppress = true; }

        #region Connection tab
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T01_ConnectionTab()
        {
            // Verify default values
            CMISViewModel vm = new CMISViewModel(new CMISSettings(), new CMISClientMock());
            Assert.IsFalse(vm.IsRunning);
            Assert.IsFalse(vm.DataLoaded);
            Assert.IsFalse(vm.RepositoriesLoaded);
            Assert.AreEqual(0, vm.SelectedTab);
            Assert.AreEqual("http://<Server URL>", vm.CT.ServerURL);
            Assert.AreEqual(TypeOfBinding.Atom, vm.CT.TypeOfBinding);
            Assert.AreEqual(null, vm.CT.Username);
            Assert.AreEqual(string.Empty, vm.CT.Password);
            Assert.IsNull(vm.CT.SelectedRepository.Id);
            Assert.IsNull(vm.CT.SelectedRepository.Description);
            Assert.IsFalse(vm.CT.SecondaryTypesSupported);

            // Verify tab order
            CMIS_WPFControl wpfControl = new CMIS_WPFControl();
            wpfControl.DataContext = vm;
            Dictionary<int, string> tabs = new Dictionary<int, string>() {
                { 0, "connectionTab" },
                { 1, "folderTab" },
                { 2, "typesTab" },
                { 3, "documentTab" },
                { 4, "secondaryTypesTab" },
            };
            TabControl tc = (TabControl)LogicalTreeHelper.FindLogicalNode(wpfControl, "mainTabControl");
            for (int i = 0; i < tabs.Count; i++)
            {
                tc.SelectedIndex = i;
                TabItem ti = tc.SelectedItem as TabItem;
                Assert.AreEqual(tabs[i], ti.Name);
            }

            // Load repositories
            vm = getViewModel();
            Assert.IsFalse(vm.RepositoriesLoaded);
            Assert.IsFalse(vm.DataLoaded);
            Assert.IsFalse(vm.CMISSettings.LoadRepositoriesPossible);
            Assert.IsFalse(vm.CMISSettings.ConnectPossible);

            vm = new CMISViewModel(vm.Settings, new CMISClientMock());
            Assert.IsFalse(vm.RepositoriesLoaded); // no change
            Assert.IsFalse(vm.DataLoaded); // no change
            Assert.IsFalse(vm.CMISSettings.LoadRepositoriesPossible); // no change
            Assert.IsFalse(vm.CMISSettings.ConnectPossible); // no change

            vm.LoadRepositoriesButtonHandler();
            Assert.AreEqual(3, vm.CT.Repositories.Count);
            Assert.IsTrue(vm.RepositoriesLoaded); // change
            Assert.IsFalse(vm.DataLoaded); // no change
            Assert.IsTrue(vm.CMISSettings.LoadRepositoriesPossible); // change
            Assert.IsFalse(vm.CMISSettings.ConnectPossible); // no change

            vm = new CMISViewModel(vm.Settings, new CMISClientMock());
            Assert.AreEqual(3, vm.CT.Repositories.Count);
            Assert.IsTrue(vm.RepositoriesLoaded); // change
            Assert.IsFalse(vm.DataLoaded); // no change
            Assert.IsTrue(vm.CMISSettings.LoadRepositoriesPossible); // change
            Assert.IsFalse(vm.CMISSettings.ConnectPossible); // no change

            vm.CT.Username += "-1"; // invalidate connection settings
            Assert.IsFalse(vm.RepositoriesLoaded); // change
            Assert.IsFalse(vm.DataLoaded); // no change
            Assert.IsFalse(vm.CMISSettings.LoadRepositoriesPossible); // change
            Assert.IsFalse(vm.CMISSettings.ConnectPossible); // no change
            vm.LoadRepositoriesButtonHandler();

            // Connect
            Assert.AreEqual(vm.CT.SelectedRepository, vm.CT.Repositories.First());
            vm.ConnectdButtonHandler();
            Assert.IsNull(SIEEMessageBox.LastMessage);
            Assert.IsTrue(vm.RepositoriesLoaded); // no change
            Assert.IsTrue(vm.DataLoaded); // change
            Assert.IsTrue(vm.CMISSettings.LoadRepositoriesPossible); // no change
            Assert.IsTrue(vm.CMISSettings.ConnectPossible); // change

            vm.CT.SelectedRepository = vm.CT.Repositories.Last(); // reselect
            Assert.IsTrue(vm.RepositoriesLoaded); // no change
            Assert.IsFalse(vm.DataLoaded); // change
            Assert.IsTrue(vm.CMISSettings.LoadRepositoriesPossible); // no change
            Assert.IsFalse(vm.CMISSettings.ConnectPossible); // change

            vm.ConnectdButtonHandler();
            Assert.IsNull(SIEEMessageBox.LastMessage);
            vm = new CMISViewModel(vm.Settings, new CMISClientMock());
            Assert.IsTrue(vm.RepositoriesLoaded); // no change
            Assert.IsTrue(vm.DataLoaded); // no change
            Assert.IsTrue(vm.CMISSettings.LoadRepositoriesPossible); // no change
            Assert.IsTrue(vm.CMISSettings.ConnectPossible); // no change

            // Binding type
            vm.CT.BrowserBinding = true;
            Assert.AreEqual(TypeOfBinding.Browser, vm.CMISSettings.Binding);
            Assert.IsFalse(vm.CT.AtomBinding);
            Assert.IsTrue(vm.CT.BrowserBinding);
            Assert.IsFalse(vm.CT.WebServiceBinding);
            vm.CT.WebServiceBinding = true;
            Assert.AreEqual(TypeOfBinding.WebService, vm.CMISSettings.Binding);
            Assert.IsFalse(vm.CT.AtomBinding);
            Assert.IsFalse(vm.CT.BrowserBinding);
            Assert.IsTrue(vm.CT.WebServiceBinding);
            vm.CT.AtomBinding = true;
            Assert.AreEqual(TypeOfBinding.Atom, vm.CMISSettings.Binding);
            Assert.IsTrue(vm.CT.AtomBinding);
            Assert.IsFalse(vm.CT.BrowserBinding);
            Assert.IsFalse(vm.CT.WebServiceBinding);
        }

        private CMISViewModel getViewModel()
        {
            SIEEMessageBox.LastMessage = null;
            CMISViewModel vm = new CMISViewModel(new CMISSettings(), new CMISClientMock());
            vm.CT.ServerURL = "http://MyServer";
            vm.CT.Username = "johannes";
            vm.CT.Password = "schacht";
            vm.Initialize(new CMIS_WPFControl());
            return vm;
        }

        private CMISViewModel getInitializedViewModel()
        {
            CMISViewModel vm = getViewModel();

            Assert.IsFalse(vm.DataLoaded);
            Assert.IsFalse(vm.TT.PropertiesLoaded);
            Assert.IsTrue(vm.FT.WarningVisible);
            Assert.AreEqual(0, vm.SelectedTab);

            vm.LoadRepositoriesButtonHandler();
            Assert.IsFalse(vm.DataLoaded);

            vm.ConnectdButtonHandler();
            Assert.IsTrue(vm.DataLoaded);
            Assert.IsFalse(vm.TT.PropertiesLoaded);
            Assert.IsTrue(vm.FT.WarningVisible);
            Assert.AreEqual(1, vm.SelectedTab);

            vm.ActivateTab("typesTab");
            vm.TT.SetSelectTypeHandler(vm.TT.Types[0]);
            Assert.IsFalse(vm.TT.PropertiesLoaded);
            Assert.AreEqual(vm.TT.SelectedCulture, CultureInfo.CurrentCulture);

            vm.FT.SetSelectedFolderHandler(vm.FT.Folders[0]);
            Assert.IsFalse(vm.FT.WarningVisible);

            Assert.IsNull(SIEEMessageBox.LastMessage);
            return vm;
        }
        #endregion

        #region Basic export
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T02_BasicExport()
        {
            CMISViewModel vm = getInitializedViewModel();
            vm.CMISSettings.SelectedCultureInfoName = "en-US";
            StoreDocumentResult expectedtResult = StoreDocumentResult.DefaultResult();

            CMISClientMock cmisClient = new CMISClientMock();
            CMISExport export = new CMISExport(cmisClient);
            SIEEBatch batch = createBatch(expectedtResult.Filename, expectedtResult.DocName);
            SIEEFieldlist fieldlist = batch[0].Fieldlist;

            // Default document export
            vm.LoadPropertiesHandler();
            Assert.IsTrue(vm.TT.PropertiesLoaded);
            Assert.AreEqual(6, vm.TT.Properties.Count);
            export.Init(vm.Settings);
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedtResult.Compare(cmisClient.SDR);

            foreach (CMISProperty p in vm.TT.Properties)
            {
                p.Selected = true;
                object objectValue;
                string stringValue;
                switch (p.Type)
                {
                    case CMISClientType.Boolean:
                        stringValue = "true";
                        objectValue = true;
                        break;
                    case CMISClientType.Integer:
                        stringValue = "4711";
                        objectValue = 4711;
                        break;
                    case CMISClientType.Decimal:
                        stringValue = "1.8";
                        objectValue = 1.8M;
                        break;
                    case CMISClientType.DateTime:
                        stringValue = "11.03.2017";
                        objectValue = DateTime.Parse(stringValue, new CultureInfo("en-US"));
                        break;
                    default: objectValue = stringValue = "Some string"; break;
                }
                fieldlist.Add(new SIEEField(p.Id, p.Id, stringValue));
                expectedtResult.Properties[p.Id] = objectValue;
                export.Init(vm.Settings);
                export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
                expectedtResult.Compare(cmisClient.SDR);
                if (p.Type != CMISClientType.String)
                {
                    fieldlist.Where(n => n.ExternalId == p.Id).First().Value = "Illegal value";
                    export.Init(vm.Settings);
                    export.ExportBatch(vm.Settings, batch); Assert.IsFalse(batch[0].Succeeded);
                }
                expectedtResult.Compare(cmisClient.SDR);
                fieldlist.Remove(fieldlist.Where(n => n.ExternalId == p.Id).First());
                expectedtResult.Properties.Remove(p.Id);
            }

            CMISProperty pror = vm.TT.Properties.Where(n => n.Type == CMISClientType.Decimal).First();
            pror.Selected = true;
            fieldlist.Add(new SIEEField(pror.Id, pror.Id, "1,8"));
            expectedtResult.Properties[pror.Id] = 1.8M;
            vm.CMISSettings.SelectedCultureInfoName = "de-DE";
            export.Init(vm.Settings);
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedtResult.Compare(cmisClient.SDR);
        }

        private SIEEBatch createBatch(string filename, string docName)
        {
            SIEEField field = new SIEEField() { Name = "SomeField", ExternalId = "SomeField", Value = "SomeValue" };
            SIEEFieldlist fieldlist = new SIEEFieldlist();
            fieldlist.Add(field);
            SIEEDocument document = new SIEEDocument();
            document.PDFFileName = filename;
            document.InputFileName = docName;
            document.BatchId = "4711";
            document.DocumentId = "0";
            document.Fieldlist = fieldlist;
            SIEEBatch batch = new SIEEBatch();
            batch.Add(document);
            return batch;
        }
        #endregion

        #region Folder tab
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T03_FolderTab()
        {
            CMISViewModel vm = getInitializedViewModel();
            StoreDocumentResult expectedResult = StoreDocumentResult.DefaultResult();

            CMISClientMock cmisClient = new CMISClientMock();
            CMISExport export = new CMISExport(cmisClient);

            // Default values
            Assert.AreEqual("/" + cmisClient.GetRootFolder().DisplayName, vm.FT.SelectedFolderPath);
            Assert.IsFalse(vm.FT.UseSubFolderField);
            Assert.AreEqual("FolderField", vm.FT.SubFolderField);
            Assert.IsFalse(vm.FT.UseSubFolderType);
            Assert.IsTrue(vm.FT.SubFolderTypeFixed);
            Assert.AreEqual("cmis:folder", vm.FT.SubFolderType);
            Assert.IsFalse(vm.FT.SubFolderTypeFromField);
            Assert.AreEqual("FolderTypeField", vm.FT.SubFolderTypeField);
            Assert.AreEqual(CMISSettings.ConflictHandling.None, vm.FT.SelectedConflictHandling);
            Assert.AreEqual(4, vm.FT.NumberOfDigits);
            Assert.AreEqual("Collapsed", vm.FT.NumberOfDigitsVisible);
            Assert.IsTrue(vm.FT.Major);
            Assert.AreEqual("Collapsed", vm.FT.VersioningVisible);

            // Visibility checks and logic
            vm.FT.SubFolderTypeFromField = true;
            Assert.IsFalse(vm.FT.SubFolderTypeFixed);
            vm.FT.SubFolderTypeFixed = true;
            Assert.IsFalse(vm.FT.SubFolderTypeFromField);
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.Replace;
            Assert.AreEqual("Collapsed", vm.FT.NumberOfDigitsVisible);
            Assert.AreEqual("Collapsed", vm.FT.VersioningVisible);
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.AddBlurb;
            Assert.AreEqual("Collapsed", vm.FT.NumberOfDigitsVisible);
            Assert.AreEqual("Collapsed", vm.FT.VersioningVisible);
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.AddNumber;
            Assert.AreEqual("Visible", vm.FT.NumberOfDigitsVisible);
            Assert.AreEqual("Collapsed", vm.FT.VersioningVisible);
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.AddVersion;
            Assert.AreEqual("Collapsed", vm.FT.NumberOfDigitsVisible);
            Assert.AreEqual("Visible", vm.FT.VersioningVisible);
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.None;

            // Default document export
            SIEEBatch batch = createBatch(expectedResult.Filename, expectedResult.DocName);
            SIEEFieldlist fieldlist = batch[0].Fieldlist;
            vm.LoadPropertiesHandler();
            export.Init(vm.Settings);
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedResult.Compare(cmisClient.SDR);

            // SubFolderField
            vm.FT.UseSubFolderField = true;
            fieldlist.Add(new SIEEField()
            { Name = "FolderField", Value = "subFolder", ExternalId = "FolderField" });
            expectedResult.FinalFolder += "/subFolder";
            export.Init(vm.Settings);
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedResult.UsedFolderType = vm.FT.SubFolderType; // Default is what export implements
            expectedResult.Compare(cmisClient.SDR);

            // SubFolderType
            vm.FT.UseSubFolderType = true;
            vm.FT.SubFolderType = "cmis:myFolderType";
            export.Init(vm.Settings);
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedResult.UsedFolderType = "cmis:myFolderType";
            expectedResult.Compare(cmisClient.SDR);

            // SubFolderFieldType
            vm.FT.SubFolderTypeFromField = true;
            fieldlist.Add(new SIEEField()
            {
                Name = vm.FT.SubFolderTypeField,
                Value = "cmis:anotherFolderType",
                ExternalId = vm.FT.SubFolderTypeField,
            });
            export.Init(vm.Settings);
            export.ExportBatch(vm.Settings, batch);
            expectedResult.UsedFolderType = "cmis:anotherFolderType";
            expectedResult.Compare(cmisClient.SDR);

            // Conflict Handling : Replace
            cmisClient.ExistingDocument = expectedResult.DocName + ".pdf";
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.Replace;
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedResult.Compare(cmisClient.SDR);

            // Conflict Handling : None
            cmisClient.ExistingDocument = expectedResult.DocName + ".pdf";
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.None;
            export.ExportBatch(vm.Settings, batch);
            expectedResult.OverwrittenExistingDocument = true;
            expectedResult.Compare(cmisClient.SDR);
            expectedResult.OverwrittenExistingDocument = false;
            cmisClient.SDR.OverwrittenExistingDocument = false;

            // Conflict Handling : AddBlurb
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.AddBlurb;
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedResult.ExpectBlurb = true;
            expectedResult.Compare(cmisClient.SDR);
            expectedResult.ExpectBlurb = false;

            // Conflict Handling : AddNumber
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.AddNumber;
            vm.FT.NumberOfDigits = 2;
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            string save = expectedResult.DocName;
            expectedResult.DocName += "_01";
            expectedResult.Compare(cmisClient.SDR);
            expectedResult.DocName = save;

            // Conflict Handling : AddVersion
            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.AddVersion;
            vm.FT.Major = true;
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedResult.Updated = true;
            expectedResult.Major = true;
            expectedResult.CheckInComment = "OCC created version";
            expectedResult.Compare(cmisClient.SDR);
            vm.FT.Major = false;
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedResult.Major = false;
            expectedResult.Compare(cmisClient.SDR);
        }

        public class StoreDocumentResult
        {
            public string Password { get; set; }
            public string FinalFolder { get; set; }
            public string Filename { get; set; }
            public string DocName { get; set; }
            public Dictionary<string, object> Properties { get; set; }
            public bool Updated { get; set; }
            public bool Major { get; set; }
            public string CheckInComment { get; set; }
            public CMISType Type { get; set; }
            public string UsedFolderType { get; set; }

            public bool OverwrittenExistingDocument { get; set; }
            public bool ExpectBlurb { get; set; }

            public static StoreDocumentResult DefaultResult()
            {
                return new StoreDocumentResult()
                {
                    Password = "schacht",
                    FinalFolder = "Root folder",
                    Filename = "Document.pdf",
                    DocName = "4711_0",
                    Properties = new Dictionary<string, object>() {
                        { "cmis:objectTypeId" , "type-1"},
                        { "SomeField", "SomeValue" },
                    },
                    Updated = false,
                    Major = false,
                    CheckInComment = null,
                    Type = new CMISType("2", "document/lyrics"),
                    UsedFolderType = null,
                    OverwrittenExistingDocument = false,
                    ExpectBlurb = false,
                };
            }
            public void Compare(StoreDocumentResult result)
            {
                Assert.AreEqual(Password, result.Password);
                Assert.AreEqual(FinalFolder, result.FinalFolder);
                Assert.AreEqual(Filename, result.Filename);
                if (!ExpectBlurb)
                    Assert.AreEqual(DocName + ".pdf", result.DocName);
                else
                    Assert.AreEqual(0, result.DocName.IndexOf(DocName + "_"));
                foreach (string key in Properties.Keys)
                    Assert.AreEqual(Properties[key], result.Properties[key]);
                Assert.AreEqual(Updated, result.Updated);
                Assert.AreEqual(Major, result.Major);
                Assert.AreEqual(CheckInComment, result.CheckInComment);
                Assert.AreEqual(UsedFolderType, result.UsedFolderType);
                Assert.AreEqual(OverwrittenExistingDocument, result.OverwrittenExistingDocument);
            }
        }
        #endregion

        #region Types tab
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T04_TypesTab()
        {
            CMISViewModel vm = getInitializedViewModel();
            StoreDocumentResult expectedResult = StoreDocumentResult.DefaultResult();

            // Default setting
            Assert.AreEqual(0, vm.TT.Properties.Count);

            // Select, deselect all
            vm.LoadPropertiesHandler();
            Assert.IsNull(SIEEMessageBox.LastMessage);
            Assert.AreEqual(6, vm.TT.Properties.Count);
            foreach (CMISProperty p in vm.TT.Properties) Assert.IsFalse(p.Selected);
            vm.TT.SelectAllHandler();
            foreach (CMISProperty p in vm.TT.Properties) Assert.IsTrue(p.Selected);
            vm.TT.DeselectAllHandler();
            foreach (CMISProperty p in vm.TT.Properties) Assert.IsFalse(p.Selected);

            // Create schema
            SIEEFieldlist fl = vm.Settings.CreateSchema();
            Assert.AreEqual(0, fl.Count);
            vm.TT.Properties[0].Selected = true;
            fl = vm.Settings.CreateSchema();
            Assert.AreEqual(1, fl.Count);
            Assert.AreEqual(vm.TT.Properties[0].Id, fl[0].Name);
        }
        #endregion

        #region Multi-tab logic
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T05_MultiTabLogic()
        {
            CMISViewModel vm = getInitializedViewModel();
            Assert.IsFalse(vm.TT.PropertiesLoaded);
            Assert.IsFalse(vm.FT.WarningVisible);
            Assert.IsTrue(vm.TT.Warning.IndexOf("properties") > 0);
            Assert.IsTrue(vm.FT.Warning.IndexOf("folder") > 0);

            CMISClientMock cmisClient = vm.CMISClient as CMISClientMock;
            cmisClient.VersionableType = true;
            vm.TT.SetSelectTypeHandler(vm.TT.Types[0]);
            vm.LoadPropertiesHandler();
            Assert.IsTrue(vm.TT.PropertiesLoaded);
            Assert.IsFalse(vm.FT.WarningVisible);

            vm.FT.SelectedConflictHandling = CMISSettings.ConflictHandling.AddVersion;
            Assert.IsFalse(vm.FT.WarningVisible);

            cmisClient.VersionableType = false;
            vm.TT.SetSelectTypeHandler(vm.TT.Types[0]);
            vm.LoadPropertiesHandler();
            Assert.IsTrue(vm.FT.WarningVisible);
            Assert.IsTrue(vm.FT.Warning.IndexOf("version") > 0);
        }
        #endregion

        #region Serialization
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T06_Serialization()
        {
            CMISViewModel vm = getInitializedViewModel();
            vm.CMISSettings.SerializedFolderPath = new List<string>() { "SerializedFolderPath" };
            vm.CMISSettings.SerializedTypePath = new List<string>() { "SerializedTypePath" };
            vm.CMISSettings.Properties = new ObservableCollection<CMISProperty>() {
                new CMISProperty() {
                    DisplayName = "one",
                    Id = "1",
                    Selected = true,
                    //TypeName = "String",
                    Type = CMISClientType.String,
                } };

            // Serialize -> deserialize -> serialize again -> compare strings
            string xmlString = Serializer.SerializeToXmlString(vm.Settings, System.Text.Encoding.Unicode);
            CMISSettings cmisSettings = (CMISSettings)Serializer.DeserializeFromXmlString(xmlString, typeof(CMISSettings), System.Text.Encoding.Unicode);
            string xmlString1 = Serializer.SerializeToXmlString(cmisSettings, System.Text.Encoding.Unicode);
            Assert.AreEqual(xmlString, xmlString1);

            // Deserialize some older string
            //File.WriteAllText(@"c:\temp\CMISSettings-Serialized.xml", xmlString);
            xmlString = Properties.Resources.CMISSettings_Serialized;
            cmisSettings = (CMISSettings)Serializer.DeserializeFromXmlString(xmlString, typeof(CMISSettings), System.Text.Encoding.Unicode);
        }
        #endregion

        #region Select repository
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T07_SelectReposiory()
        {
            CMISViewModel vm = getViewModel();
            vm.LoadRepositoriesButtonHandler();
            Assert.AreEqual("Repository 1", vm.CT.SelectedRepository.Id);
            Assert.AreEqual(vm.CT.Repositories[0], vm.CT.SelectedRepository);
            vm.CT.SelectedRepository = vm.CT.Repositories[1];
            SIEESettings save = vm.Settings;
            vm = new CMISViewModel(vm.Settings, vm.CMISClient);
            Assert.AreEqual("Repository 2", vm.CT.SelectedRepository.Id);
            Assert.AreEqual(vm.CT.Repositories[1].Description, vm.CT.SelectedRepository.Description);
        }
        #endregion

        #region Password
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T08_Password()
        {
            CMISViewModel vm = getInitializedViewModel();

            // Test one: Direct Password set and get on the view model
            string pw = "Hello World!";
            vm.CT.Password = pw;
            Assert.AreEqual(pw, vm.CT.Password);
            string encryptedPW = vm.CMISSettings.Password;
            Assert.AreNotEqual(pw, encryptedPW);

            // Verify password in export
            StoreDocumentResult expectedtResult = StoreDocumentResult.DefaultResult();
            expectedtResult.Password = pw;

            CMISClientMock cmisClient = new CMISClientMock();
            CMISExport export = new CMISExport(cmisClient);
            SIEEBatch batch = createBatch(expectedtResult.Filename, expectedtResult.DocName);
            SIEEFieldlist fieldlist = batch[0].Fieldlist;

            // Default document export
            vm.LoadPropertiesHandler();
            Assert.IsTrue(vm.TT.PropertiesLoaded);
            Assert.AreEqual(6, vm.TT.Properties.Count);
            export.Init(vm.Settings);
            export.ExportBatch(vm.Settings, batch); Assert.IsTrue(batch[0].Succeeded);
            expectedtResult.Compare(cmisClient.SDR);
        }
        #endregion

        #region Property converter
        [TestMethod]
        [TestCategory("-> CMIS Adapter")]
        public void T09_PropertyConverter()
        {
            CultureInfo ci = new CultureInfo("en-US");
            object o;
            CMISProperty prop = new CMISProperty();
            prop.Type = CMISClientType.Integer;
            SIEEField field = new SIEEField();
            field.Value = "42";
            List<string> valueList = new List<string>() { "2", "", "3", null };
            field.ValueList = valueList;

            // Default, take value
            o = prop.ConvertValue(field, ci);
            Assert.IsTrue(o is Int32);
            Assert.AreEqual(42, o);

            // IsMulti alone, no list
            prop.IsMulti = true;
            field.ValueList = new List<string>();
            o = prop.ConvertValue(field, ci);
            Assert.IsTrue(o is List<Int32>);
            Assert.AreEqual(1, (o as List<Int32>).Count);
            Assert.AreEqual(42, (o as List<Int32>)[0]);

            // IsMulti with list
            field.ValueList = valueList;
            o = prop.ConvertValue(field, ci);
            Assert.IsTrue(o is List<Int32>);
            Assert.AreEqual(1, (o as List<Int32>).Count);
            Assert.AreEqual(42, (o as List<Int32>)[0]);

            // Now create list
            field.Cardinality = -1;
            o = prop.ConvertValue(field, ci);
            Assert.IsTrue(o is List<Int32>);
            Assert.AreEqual(4, (o as List<Int32>).Count);
            Assert.AreEqual(2, (o as List<Int32>)[0]);
            Assert.AreEqual(0, (o as List<Int32>)[1]);
            Assert.AreEqual(3, (o as List<Int32>)[2]);
            Assert.AreEqual(0, (o as List<Int32>)[3]);

            // Check Boolean
            prop.IsMulti = false;
            prop.Type = CMISClientType.Boolean;
            field.Value = "true";
            o = prop.ConvertValue(field, ci);
            Assert.AreEqual(true, o);
            field.Value = null;
            o = prop.ConvertValue(field, ci);
            Assert.AreEqual(false, o);

            // Check DateTime
            prop.IsMulti = false;
            prop.Type = CMISClientType.DateTime;
            DateTime dt = DateTime.Now;
            field.Value = dt.ToString(ci);
            o = prop.ConvertValue(field, ci);
            Assert.AreEqual(dt.ToString(), o.ToString());
            field.Value = null;
            o = prop.ConvertValue(field, ci);
            Assert.AreEqual(dt.ToString(), o.ToString());

            // Check Decimal
            prop.IsMulti = false;
            prop.Type = CMISClientType.Decimal;
            field.Value ="5.6";
            o = prop.ConvertValue(field, ci);
            Assert.AreEqual((Decimal)5.6, o);
            field.Value = null;
            o = prop.ConvertValue(field, ci);
            Assert.AreEqual((Decimal)0.0, o);
        }
        #endregion
    }

    public class CMISClientMock : ICMISClient
    {
        // Influence behavior
        public string ExistingDocument { get; set; } = null;
        public bool VersionableType { get; set; } = false;

        #region Connection and Repositories
        public string ServerURL { get; set; }
        public TypeOfBinding TypeOfBinding { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string GetSupportedVersion() { return "1,1"; }
        #endregion

        #region Repositories
        public List<CMISRepository> LoadRepositories()
        {
            return new List<CMISRepository>() {
                { new CMISRepository() { Id =  "Repository 1", Description = "Repository 1\nName 1\nDescription 1" } },
                { new CMISRepository() { Id =  "Repository 2", Description = "Repository 2\nName 1\nDescription 2" } },
                { new CMISRepository() { Id =  "Repository 3", Description = "Repository 3\nName 1\nDescription 3" } },
            };
        }
        public void SelectRepository(string repositoryId)
        {
            Assert.IsTrue(LoadRepositories().Where(n => n.Id == repositoryId).Count() > 0);
        }
        #endregion

        #region Folder
        public CMISFolder GetRootFolder()
        {
            return new CMISFolder("1", "Root folder");
        }
        public CMISFolder GetFolderFromId(string folderId)
        {
            if (folderId == "1") return GetRootFolder();
            return new CMISFolder("2", folderId);
        }
        public CMISFolder GetFolderFromPath(string path)
        {
            return new CMISFolder("3", path);
        }
        public CMISFolder GetSubfolder(CMISFolder baseFolder, string pathExtension, string folderType)
        {
            SDR.UsedFolderType = folderType;
            return new CMISFolder("4", baseFolder.DisplayName + "/" + pathExtension);
        }
        public void DeleteFolder(CMISFolder folder) { }
        #endregion

        #region Types
        public CMISType GetRootType()
        {
            CMISType result = new CMISType("1", "Root type");
            result.Versionable = VersionableType;
            return result;
        }
        public CMISType GetTypeFromId(string typeId)
        {
            CMISType result = new CMISType("type-1", "Root type");
            result.Versionable = VersionableType;
            return result;
        }
        public CMISType GetTypeByPath(string path)
        {
            CMISType result = new CMISType("3", path);
            result.Versionable = VersionableType;
            return result;
        }
        public List<CMISProperty> GetPropertyDefinitions(CMISType type)
        {
            return new List<CMISProperty>() {
                 new CMISProperty() { // Should be ignored by LoadProperties()
                    Id = "cmis:name", DisplayName = "Name", Type = CMISClientType.String, },
                new CMISProperty() {
                    Id = "Field-String",  DisplayName = "Prop 1", Type = CMISClientType.String, },
                new CMISProperty() {
                    Id = "Field-Integer", DisplayName = "Prop 2", Type = CMISClientType.Integer, },
                new CMISProperty() {
                    Id = "Field-Boolean", DisplayName = "Prop 3", Type = CMISClientType.Boolean, },
                new CMISProperty() {
                    Id = "Field-Decimal", DisplayName = "Prop 4", Type = CMISClientType.Decimal, },
                new CMISProperty() {
                    Id = "Field-DateTime", DisplayName = "Prop 5", Type = CMISClientType.DateTime, },
                new CMISProperty() {
                    Id = "Field-DateTime", DisplayName = "Prop 5", Type = CMISClientType.DateTime, IsMulti = true, },
            };
        }
        #endregion

        #region Documents
        public CMISDocument GetDocument(CMISFolder folder, string documentName)
        {
            if (ExistingDocument == documentName)
                return new CMISDocument("1", "Document 1");
            else
                return null;
        }
        public void DeleteDocument(CMISDocument document) { ExistingDocument = null; }

        public TestCMISAdapter.StoreDocumentResult SDR = new TestCMISAdapter.StoreDocumentResult();

        public void StoreDocument(CMISFolder folder, string document, string docName,
            Dictionary<string, object> props, bool? major)
        {
            SDR.Password = Password;
            SDR.FinalFolder = folder.DisplayName;
            SDR.Filename = document;
            SDR.DocName = docName;
            SDR.Properties = props;
            if (ExistingDocument == docName)
                SDR.OverwrittenExistingDocument = true;
        }

        public void UpdateDocument(CMISFolder folder, string document, string docName,
            Dictionary<string, object> props, bool major, string checkInComment)
        {
            SDR.FinalFolder = folder.DisplayName;
            SDR.Filename = document;
            SDR.DocName = docName;
            SDR.Properties = props;
            SDR.Updated = true;
            SDR.Major = major;
            SDR.CheckInComment = checkInComment;
        }

        public object GetPropertyValue(CMISDocument document, string propertyName)
        {
            return document.Name + ":" + propertyName;
        }

        public string GetCheckinComent(CMISDocument document) { return "Some comment"; }

        public CMISDocument GetObjectOfLatestVersion(CMISDocument document, bool major) { return document;}
        #endregion
    }
}
