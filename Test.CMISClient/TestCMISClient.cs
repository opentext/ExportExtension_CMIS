using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;


namespace CaptureCenter.CMIS
{
    [TestClass]
    public class TestCMISClient
    {
        /// Overall organisation
        /// 
        /// The CMIS client librabry is tested against various target systems. For each target system there
        /// is one CMISTestSystem instance defined that contains all relevant settings for the target.

        private string testonly = "InMemory";

        #region Constructor
        // Turn on/off performance tests
        private bool runPerformaceTest = false;
        
        // We need to have one docuemnt to work with
        private string sampleDocument;

        // We need a CMIS client object to work with
        private CMISClient cmisClient;

        private string sieeSubPath = "SIEEsub1";
        private string documentName = "Sample document";

        public TestCMISClient()
        {
            testsystems = ExportExtensionCommon.SIEEUtils.GetLocalTestDefinintions(testsystems);
            sampleDocument = Path.Combine(Path.GetTempPath(), "Document.pdf");
            File.WriteAllBytes(sampleDocument, SIEE_CMIS_Test.Properties.Resources.Document);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            cmisClient = new CMISClient();
        }
        #endregion

        #region CMIS test systems
        /// This class provides some description values for a CMIS target and expected results
        public class CMISTestSystem
        {
            public string TestSystemName { get; set; }  // used to identify the system
            public string Description { get; set; }     // used for error messages in Assert
            public bool IsActive { get; set; }

            // Connection parameter
            public string ServerURL { get; set; }
            public TypeOfBinding TypeOfBinding { get; set; } = TypeOfBinding.Atom;
            public string Username { get; set; }
            public string Password { get; set; }

            // Repository
            public string SupportedVersion { get; set; }
            public int NumberOfRepositories { get; set; }
            public string Repository { get; set; }

            // Folder
            public string RootFolder { get; set; }
            public string SubFolder { get; set; }
            public string FolderType { get; set; } = "cmis:folder";

            // Types
            public string RootType { get; set; } = "cmis:document";
            public string TypesType { get; set; }
            public int NumberOfProperties { get; set; }
            public List<CMISProperty> ExpectedProperties { get; set; }

            // Documents
            public string StoreType { get; set; }
            public string VersioningType { get; set; } = null;
            public List<CMISTestProperty> Properties { get; set; } = null;

            // Different target systems use different display values for the version
            public string V1_Major { get; set; }    // e.g. 1.0 --> First document version (Major versioning)
            public string V2_Major { get; set; }    // e.g. 2.0 --> Second document version (Major versioning)
            public bool SupportsMinorVersions { get; set; }
            public string V1_Minor { get; set; }    // e.g. 1.0 --> First document version (Minor versioning)
            public string V2_Minor { get; set; }    // e.g. 1.1 --> Second document version (Minor versioning)

            public List<string> Excepctions { get; set; } = new List<string>();
            public bool VersionableTest { get; set; } = false;
         }

        public class CMISTestProperty
        {
            public CMISProperty Property { get; set; }
            public string TestValue { get; set; }
        }

        private List<CMISTestSystem> testsystems = new List<CMISTestSystem>()
        {
            new CMISTestSystem()
            {
                // Connection
                TestSystemName = "Alfresco",
                Description = "Alfresco (public server)",
                ServerURL = "https://cmis.alfresco.com/alfresco/api/-default-/public/cmis/versions/1.1/atom",
                Username = "admin",
                Password = "admin",

                // Repository
                SupportedVersion = "1.1",
                NumberOfRepositories = 1,
                Repository = "-default-",

                // Folder
                RootFolder = "Company Home",
                SubFolder = "/temp",
                FolderType = "cmis:folder",

                // Types
                TypesType = "D:cmiscustom:document",
                NumberOfProperties = 5,
                ExpectedProperties = new List<CMISProperty>() {
                    new CMISProperty() {
                        DisplayName = "Custom Document Property (multi-valued boolean)",
                        Id = "cmiscustom:docprop_boolean_multi",
                        Type = CMISClientType.Boolean,
                    },
                    new CMISProperty() {
                        DisplayName = "Custom Document Property (datetime)",
                        Id = "cmiscustom:docprop_datetime",
                        Type = CMISClientType.DateTime,
                    },
                    new CMISProperty() {
                        DisplayName = "Custom Document Property (string)",
                        Id = "cmiscustom:docprop_string",
                        Type = CMISClientType.String,
                    },
                },
                StoreType = "D:cmiscustom:document",
                Properties = new List<CMISTestProperty>() {
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "cmiscustom:docprop_string", Type = CMISClientType.String},
                        TestValue = "Johannes Schacht"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "cmiscustom:docprop_boolean_multi", Type = CMISClientType.Boolean, IsMulti = true},
                        TestValue = "false"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "cmiscustom:docprop_datetime", Type = CMISClientType.DateTime},
                        TestValue = "2016-06-28 4:33"
                    },
                },
                VersioningType = "D:cmiscustom:document",
                V1_Major = "1.0",
                V2_Major = "2.0",
                V1_Minor = "0.1",
                V2_Minor = "0.2",
                SupportsMinorVersions = true,
            },

            new CMISTestSystem()
            {
                // Connection
                TestSystemName = "InMemory",
                Description = "InMemory CMIS on some host",
                ServerURL = "http://somhost:8080/chemistry-opencmis-server-inmemory-0.14.0/atom",
                TypeOfBinding = TypeOfBinding.Atom,
                Username = "test",
                Password = "",

                // Repository
                SupportedVersion = "1.0",
                NumberOfRepositories = 1,
                Repository = "A1",

                // Folder
                RootFolder = "RootFolder",
                SubFolder = "/My_Folder-0-0",

                // Types
                TypesType = "officeDocument",
                NumberOfProperties = 29,
                ExpectedProperties = new List<CMISProperty>() {
                    new CMISProperty() {
                        DisplayName = "Category",
                        Id = "category",
                        Type = CMISClientType.String,
                    },
                    new CMISProperty() {
                        DisplayName = "Character Count",
                        Id = "characterCount",
                        Type = CMISClientType.Integer,
                    },
                },

                // Documents
                StoreType = "exifImage",
                Properties = new List<CMISTestProperty>() {
                    new CMISTestProperty() {
                       Property = new CMISProperty() { Id = "copyright", Type = CMISClientType.String},
                       TestValue = "Johannes Schacht"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "colorSpace", Type = CMISClientType.Integer},
                        TestValue = "2"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "fNumber", Type = CMISClientType.Decimal},
                        TestValue = "2.8"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "createDate", Type = CMISClientType.DateTime},
                        TestValue = "2016-06-28 4:33"
                    },
                },
                VersioningType = "VersionableType",
                V1_Major = "1.0",
                V2_Major = "2.0",
                V1_Minor = "0.1",
                V2_Minor = "0.2",
                SupportsMinorVersions = true,
                VersionableTest = true,
            },

            new CMISTestSystem()
            {
                // Connection
                TestSystemName = "ArchiveCenter",
                Description = "OpenText Archive Center on some host",
                ServerURL = "http://somehost:8080/as_cmis/atom",
                Username = "",
                Password = "",

                // Repository
                SupportedVersion = "1.1",
                NumberOfRepositories = 1,
                Repository = "IAFML-1",

                // Folder
                RootFolder = "IAFML-1",
                SubFolder = "/OCC Test",
                FolderType = "ot:fsa_folder",

                // Types
                TypesType = "ot:fsa_document",
                NumberOfProperties = 14,
                ExpectedProperties = new List<CMISProperty>() {
                    new CMISProperty() {
                        DisplayName = "File Size",
                        Id = "ot:fsa_fileSize",
                        Type = CMISClientType.Integer,
                    },
                    new CMISProperty() {
                        DisplayName = "Owner",
                        Id = "ot:fsa_owner",
                        Type = CMISClientType.String,
                    },
                },

                // Documents
                StoreType = "ot:fsa_document",
                Properties = new List<CMISTestProperty>() {
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "ot:fsa_owner", Type = CMISClientType.String},
                        TestValue ="Johannes Schacht"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "ot:fsa_fileSize", Type = CMISClientType.Integer},
                        TestValue = "200"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "ot:fsa_archiveFlag", Type = CMISClientType.Boolean},
                        TestValue = "false"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "ot:fsa_creationTime", Type = CMISClientType.DateTime},
                        TestValue = "2016-06-28 4:33"
                    },
                },
                VersioningType = "ot:fsa_document",
                V1_Major = "1",
                V2_Major = "2",
                SupportsMinorVersions = false,
            },

            new CMISTestSystem()
            {
                // Connection
                TestSystemName = "ContentServer",
                Description = "OpenText Content Server on some host",
                ServerURL = "http://somehost:8080/xecm-cmis/browser",
                TypeOfBinding = TypeOfBinding.Browser,
                Username = "",
                Password = "",

                // Repository
                SupportedVersion = "1.1",
                NumberOfRepositories = 5,
                Repository = "141",

                // Folder
                RootFolder = "Amt",
                SubFolder = "/OCC Test",
                FolderType = "cmis:folder",

                // Types
                TypesType = "cmis:unsupported_document",
                NumberOfProperties = 2,
                ExpectedProperties = new List<CMISProperty>() {
                    new CMISProperty() {
                        DisplayName = "Description",
                        Id = "cmis:description",
                        Type = CMISClientType.String,
                    },
                },

                StoreType = "cmis:document",
                Properties = new List<CMISTestProperty>(),
                VersioningType = "cmis:document",

                V1_Major = "1",
                V2_Major = "2",
                SupportsMinorVersions = false,
            },

            new CMISTestSystem()
            {
                // Connection
                TestSystemName = "Domea",
                Description = "OpenText Domea on some host",
                ServerURL = "http://somehost:8080/domea-cmis/atom",
                Username = "",
                Password = "",

                // Repository
                SupportedVersion = "1.0",
                NumberOfRepositories = 1,
                Repository = "domea",

                // Folder
                RootFolder = "localhost:4666",
                SubFolder = "DMS/OCC Test",
                FolderType = "ccd:container0",

                // Types
                TypesType = "ccd:document0",
                NumberOfProperties = 38,
                ExpectedProperties = new List<CMISProperty>() {
                    new CMISProperty() {
                        DisplayName = "Betreff",
                        Id = "{Comment}",
                        Type = CMISClientType.String,
                    },
                    new CMISProperty() {
                        DisplayName = "Externes Datum",
                        Id = "{ExternalDate}",
                        Type = CMISClientType.DateTime,
                    },
                    new CMISProperty() {
                        DisplayName = "Verantw. OE",
                        Id = "{Udi2}",
                        Type = CMISClientType.Integer,
                    },
                },
                StoreType = "ccd:document0",
                Properties = new List<CMISTestProperty>() {
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "{Comment}", Type = CMISClientType.String},
                        TestValue = "Johannes Schacht"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "{ExternalDate}", Type = CMISClientType.DateTime},
                        TestValue = "2016-06-28"
                    },
                    new CMISTestProperty() {
                        Property = new CMISProperty() { Id = "CI1", Type = CMISClientType.Integer},
                        TestValue = "4711"
                    },
                },
                VersioningType = null,
                V1_Major = "ccd:currentVersion",
                V2_Major = "ccd:currentVersion",
                V2_Minor = "ccd:currentVersion",
                SupportsMinorVersions = true,
            },

            new CMISTestSystem()
            {
                // Connection
                TestSystemName = "Documentum",
                Description = "OpenText Documentum on some system",
                ServerURL = "http://somehost:7000/emc-cmis/resources",
                TypeOfBinding = TypeOfBinding.Atom,
                Username = "",
                Password = "",

                // Repository
                SupportedVersion = "1.0",
                NumberOfRepositories = 1,
                Repository = "corp",

                // Folder
                RootFolder = "Root Folder",
                SubFolder = "/dmadmin/OCC",

                // Types
                TypesType = "ccms_generic_content",
                NumberOfProperties = 35,
                ExpectedProperties = new List<CMISProperty>() {
                    new CMISProperty() {
                        DisplayName = "Authors",
                        Id = "authors",
                        Type = CMISClientType.String,
                    },
                    new CMISProperty() {
                        DisplayName = "Full Text Indexed",
                        Id = "a_full_text",
                        Type = CMISClientType.Boolean,
                    },
                },

                // Documents
                StoreType = "ccms_generic_content",
                Properties = new List<CMISTestProperty>() {
                    //new CMISTestProperty() {
                    //    Property = new CMISProperty() { Id = "owner_name", Type = CMISClientType.String},
                    //    TestValue = "dmadmin"
                    //},
                    //new CMISTestProperty() {
                    //    Property = new CMISProperty() { Id = "cmis:creationDate", Type = CMISClientType.DateTime},
                    //    TestValue ="2017-03-09 10:33"
                    //},
                },
                VersioningType = "ccms_generic_content",
                V1_Major = "1.0",
                V2_Major = "2.0",
                V1_Minor = "1.1",
                V2_Minor = "1.2",
                SupportsMinorVersions = true,
            },
        };

        private List<CMISTestSystem> getTestSystem()
        {
            if (!string.IsNullOrEmpty(testonly))
                return testsystems.Where(n => n.TestSystemName == testonly).ToList();

            return testsystems.Where(n => n.IsActive).ToList();
        }
        #endregion

        #region Test Repositories
        [TestMethod]
        [TestCategory("CMIS Client")]
        public void t01_Repositories() {foreach (CMISTestSystem cts in getTestSystem()) t01_Repositories(cts); }
        private void t01_Repositories(CMISTestSystem con)
        {
            List<CMISRepository> repositories = loadRepositories(con);
            Assert.AreEqual(con.NumberOfRepositories, repositories.Count());
            Assert.IsTrue(repositories.Where(n => n.Id == con.Repository).Count() == 1);
            cmisClient.SelectRepository(con.Repository);
            Assert.AreEqual(con.SupportedVersion, cmisClient.GetSupportedVersion());
        }

        private List<CMISRepository> loadRepositories(CMISTestSystem con)
        {
            cmisClient.ServerURL = con.ServerURL;
            cmisClient.TypeOfBinding = con.TypeOfBinding;
            cmisClient.Username = con.Username;
            cmisClient.Password = con.Password;
            return cmisClient.LoadRepositories();
        }

        private void initializeClient(CMISTestSystem con)
        {
            loadRepositories(con);
            cmisClient.SelectRepository(con.Repository);
        }
        #endregion

        #region Test Folder
        [TestMethod]
        [TestCategory("CMIS Client")]
        public void t02_Folder() { foreach (CMISTestSystem cts in getTestSystem()) t02_Folder(cts); }
        private void t02_Folder(CMISTestSystem con)
        {
            initializeClient(con);
            CMISFolder root = cmisClient.GetRootFolder();
            Assert.AreEqual(con.RootFolder, root.DisplayName);
            cmisClient.GetRootFolder().GetAllSubFolders().Count(); 

            CMISFolder sub = cmisClient.GetFolderFromPath(con.SubFolder);
            Assert.AreEqual(con.SubFolder.Split('/').Last(), sub.DisplayName);

            CMISFolder sub1 = cmisClient.GetSubfolder(sub, sieeSubPath, con.FolderType);
            CMISFolder sub3 = cmisClient.GetSubfolder(sub, sieeSubPath + "/sub2/sub3", con.FolderType);
            CMISFolder sub4 = cmisClient.GetSubfolder(sub, sieeSubPath + "/sub2/sub4", con.FolderType);
            CMISFolder sub2 = cmisClient.GetFolderFromPath(con.SubFolder + "/" + sieeSubPath + "/" + "sub2");
            Assert.AreEqual(2, sub2.GetAllSubFolders().Count());
            
            cmisClient.DeleteFolder(sub4);
            cmisClient.DeleteFolder(sub3);
            cmisClient.DeleteFolder(sub2);
            deleteDocument(sub1, documentName);
            cmisClient.DeleteFolder(sub1);
        }
        #endregion

        #region Test Types
        [TestMethod]
        [TestCategory("CMIS Client")]
        public void t03_Types() { foreach (CMISTestSystem cts in getTestSystem()) t03_Types(cts); }
        private void t03_Types(CMISTestSystem con)
        {
            initializeClient(con);
            CMISType root = cmisClient.GetRootType();
            Assert.AreEqual(con.RootType, root.Id);

            CMISType type = cmisClient.GetTypeFromId(con.TypesType);
            List<CMISProperty> props = cmisClient.GetPropertyDefinitions(type);
            Assert.AreEqual(con.NumberOfProperties, props.Count);
            foreach (CMISProperty p in con.ExpectedProperties)
            {
                CMISProperty p1 = props.Where(n => n.Id == p.Id).First();
                Assert.AreEqual(p.DisplayName, p1.DisplayName);
                Assert.AreEqual(p.Type, p1.Type);
            }
        }
        #endregion

        #region Test Documents
        [TestMethod]
        [TestCategory("CMIS Client")]
        public void t04_Documents() { foreach (CMISTestSystem cts in getTestSystem()) t04_Documents(cts); }
        private void t04_Documents(CMISTestSystem con)
        {
            initializeClient(con);
            string checkinComment = "Test check-in comment";

            CMISFolder folder = cmisClient.GetFolderFromPath(con.SubFolder);
            folder = cmisClient.GetSubfolder(folder, sieeSubPath, con.FolderType);

            CMISType type = cmisClient.GetTypeFromId(con.StoreType);
            deleteDocument(folder, documentName);
            storeAndVerifyDocument(folder, documentName, type, con.Properties);

            if (con.VersioningType != null)
            {
                CMISDocument cmisDocument;
                deleteDocument(folder, documentName);
                Dictionary<string, object> props = new Dictionary<string, object>();
                props["cmis:objectTypeId"] = cmisClient.GetTypeFromId(con.VersioningType).Id;

                cmisClient.StoreDocument(folder, sampleDocument, documentName, props, true);
                cmisDocument = cmisClient.GetDocument(folder, documentName);
                Assert.AreEqual(con.V1_Major, getVersionString(cmisDocument));

                cmisClient.UpdateDocument(folder, sampleDocument, documentName, props, true, checkinComment);
                cmisDocument = cmisClient.GetObjectOfLatestVersion(cmisDocument, major:true);
                Assert.AreEqual(con.V2_Major, getVersionString(cmisDocument));
                Assert.AreEqual(checkinComment, cmisClient.GetCheckinComent(cmisDocument));

                if (con.SupportsMinorVersions)
                {
                    deleteDocument(folder, documentName);
                    cmisClient.StoreDocument(folder, sampleDocument, documentName, props, false);
                    cmisDocument = cmisClient.GetDocument(folder, documentName);
                    Assert.AreEqual(con.V1_Minor, getVersionString(cmisDocument));

                    cmisClient.UpdateDocument(folder, sampleDocument, documentName, props, false, checkinComment);
                    cmisDocument = cmisClient.GetObjectOfLatestVersion(cmisDocument, major:false);
                    Assert.AreEqual(con.V2_Minor, getVersionString(cmisDocument));
                    Assert.AreEqual(checkinComment, cmisClient.GetCheckinComent(cmisDocument));
                }
            }

            deleteDocument(folder, documentName);
            cmisClient.DeleteFolder(folder);
        }

        private string getVersionString(CMISDocument cmisDocument)
        {
            return cmisClient.GetPropertyValue(cmisDocument, "cmis:versionLabel") as string;
        }

        private void deleteDocument(CMISFolder folder, string documentName)
        {
            CMISDocument cmisDocument = cmisClient.GetDocument(folder, documentName);
            if (cmisDocument != null) cmisClient.DeleteDocument(cmisDocument);
        }

        private void storeAndVerifyDocument(
            CMISFolder folder, string documentName,
            CMISType type, List<CMISTestProperty> properties)
        {
            CultureInfo ci = new CultureInfo("en-US");
            Dictionary<string, object> props = new Dictionary<string, object>();
            foreach (CMISTestProperty ctp in properties)
                props[ctp.Property.Id] = ctp.Property.ConvertValue(ctp.TestValue, ci);

            bool description = type.Type.PropertyDefinitions.Where(n => n.Id == "cmis:description").Count() > 0;
            if (description) props["cmis:description"] = "Hello World";
            props["cmis:objectTypeId"] = type.Id;
            cmisClient.StoreDocument(folder, sampleDocument, documentName, props, null);

            CMISDocument cmisDocument = cmisClient.GetDocument(folder, documentName);
            if (description) Assert.AreEqual("Hello World", cmisClient.GetPropertyValue(cmisDocument, "cmis:description"));
            //foreach (CMISProperty p in properties.Keys)
            foreach (CMISTestProperty ctp in properties)
            {
                object expected = ctp.TestValue;
                object result = cmisClient.GetPropertyValue(cmisDocument, ctp.Property.Id);
                Assert.AreEqual(expected, result); break;
            }
        }
        #endregion

        #region Versionable types
        [TestMethod]
        [TestCategory("CMIS Client")]
        public void t05_Versionable() { foreach (CMISTestSystem cts in getTestSystem()) t05_Versionable(cts); }
        private void t05_Versionable(CMISTestSystem con)
        {
            if (!con.VersionableTest) return;

            initializeClient(con);
            CMISType type = cmisClient.GetTypeFromId(con.TypesType);
            Assert.IsTrue(type.Versionable != true);
            type = cmisClient.GetTypeFromId(con.VersioningType);
            Assert.IsTrue(type.Versionable == true);
        }
        #endregion

        #region Test Performance
        // ++++++++++++++++ Test performance and memory use
        // some figure I figured out
        // InMemory          600 ms
        // Archive Center   1200 ms
        // Domea            1500 ms
        // Alfresco         3500 ms
        // Documentum       2250 ms
        [TestMethod]
        [TestCategory("CMIS Client")]
        public void t06_Performance() { foreach (CMISTestSystem cts in getTestSystem()) t06_Performance(cts); }
        private void t06_Performance(CMISTestSystem con)
        {
            if (!runPerformaceTest) return;

            initializeClient(con);
            string documentName = "Sample document";
            string folderName = con.SubFolder + "/" + sieeSubPath;

            CMISFolder folder = cmisClient.GetSubfolder(cmisClient.GetRootFolder(), folderName, con.FolderType);
            CMISType type = cmisClient.GetTypeFromId(con.StoreType);
            deleteDocument(folder, documentName);

            // pre-upload a few documents to warm up the system ...
            for (int i = 0; i != 5; i++)
                doOneDocument(folder, documentName, type, con);

            // do the real test
            Process p = Process.GetCurrentProcess();
            long memBefore = p.PeakWorkingSet64 / 1000;
            int nofCycles = 100;
            int[] times = new int[nofCycles];
            for (int i = 0; i != nofCycles; i++)
            {
                DateTime start = DateTime.Now;
                doOneDocument(folder, documentName, type, con);
                times[i] = (int)(DateTime.Now - start).TotalMilliseconds;
                Console.WriteLine(i + " = " + times[i]);
            }
            long memAfter = p.PeakWorkingSet64 / 1000;
            Assert.AreEqual(memBefore, memAfter);
            Assert.AreEqual(true, nofCycles > 20);
            double avgStart = average(times, 0, 9);
            double avgEnd = average(times, nofCycles - 10, nofCycles - 1);
            double deviation = Math.Abs(avgEnd - avgStart) / avgStart;
            Assert.AreEqual(true, deviation <= .25, "deviates (avgStart=" + avgStart + " ,avgEnd=" + avgEnd + ")");
        }
        private double average(int[] times, int start, int end)
        {
            double sum = 0;
            for (int i = start; i <= end; i++) sum += times[i];
            return sum / (end - start + 1);
        }

        private void doOneDocument(CMISFolder folder, string documentName, CMISType type, CMISTestSystem con)
        {
            storeAndVerifyDocument(folder, documentName, type, con.Properties);
            deleteDocument(folder, documentName);
        }
        #endregion
    }
}
