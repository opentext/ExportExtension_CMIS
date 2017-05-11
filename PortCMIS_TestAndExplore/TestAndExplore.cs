using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;

using PortCMIS;
using PortCMIS.Binding;
using PortCMIS.Client;
using PortCMIS.Const;
using PortCMIS.Data;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using PortCMIS.Utils;
using PortCMIS.Client.Impl;
using PortCMIS.Data.Extensions;

namespace PortCMIS_TestAndExplore
{
    [TestClass]
    public class PortCMIS_TestAndExplore
    {
        private static ISession getInMemorySession()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters[SessionParameter.BindingType] = PortCMIS.BindingType.AtomPub; 
            parameters[SessionParameter.AtomPubUrl] = "http://knocc-demo3:8080/chemistry-opencmis-server-inmemory-0.14.0/atom";
            parameters[SessionParameter.User] = "test";
            parameters[SessionParameter.Password] = "";

            SessionFactory factory = SessionFactory.NewInstance();
            ISession session = factory.GetRepositories(parameters)[0].CreateSession();
            return session;
        }

        private static ISession getAlfrescoSession()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters[SessionParameter.BindingType] = PortCMIS.BindingType.AtomPub;
            parameters[SessionParameter.AtomPubUrl] = "https://cmis.alfresco.com/alfresco/api/-default-/public/cmis/versions/1.1/atom";
            parameters[SessionParameter.User] = "admin";
            parameters[SessionParameter.Password] = "admin";

            SessionFactory factory = SessionFactory.NewInstance();
            ISession session = factory.GetRepositories(parameters)[0].CreateSession();
            return session;
        }

        private static ISession getContentServerSession()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters[SessionParameter.BindingType] = PortCMIS.BindingType.Browser;
            //parameters[SessionParameter.AtomPubUrl] = "http://egovint02.eng-muc.opentext.net:8080/xecm-cmis/atom";
            parameters[SessionParameter.BrowserUrl] = "http://egovint02.eng-muc.opentext.net:8080/xecm-cmis/browser";
            parameters[SessionParameter.User] = "otadmin@otds.admin";
            parameters[SessionParameter.Password] = "opentext";
            //parameters[SessionParameter.User] = "haralds";
            //parameters[SessionParameter.Password] = "opentext";

            SessionFactory factory = SessionFactory.NewInstance();
            IList<IRepository> sessions = factory.GetRepositories(parameters);
            ISession session = sessions.Where(n => n.Id == "198").First().CreateSession();
            return session;
        }

        [TestMethod]
        [TestCategory("Test and explore")]
        public void Login()
        {
            getContentServerSession();
        }

        [TestMethod] [Ignore]
        [TestCategory("Test and explore")]
        public void CS_Folder()
        {
            ISession session = getContentServerSession(); // 1.5 sec
            IFolder root = session.GetRootFolder(); // 1.0 sec
            var x = root.GetChildren().ToList(); // ~30.0 sec
            IFolder myFolder = x.Where(n => n.Name == "OCC Test" && n is IFolder).First() as IFolder;
            Dictionary<string, object> props = new Dictionary<string, object>()
            {
                {"cmis:objectTypeId", "cmis:folder" },
                {"cmis:name", "SomeFolder" + Guid.NewGuid() },
            };
            myFolder.CreateFolder(props);
        }

        [TestMethod] [Ignore]
        [TestCategory("Test and explore")]
        public void CS_Document()
        {
            string documentName = "Document.pdf";
            string documentFile = @"c:\temp\" + documentName;
            IContentStream contentStream;
            IFolder folder;
            Dictionary<string, object> props;
            IDocument document;
            string description;

            ISession session = getContentServerSession();
            FileInfo file = new FileInfo(documentFile);
            contentStream = session.ObjectFactory.CreateContentStream(
                documentName, file.Length, "application/pdf",
                new FileStream(documentFile, FileMode.Open));

            folder = session.GetObjectByPath("/OCC Test") as IFolder;
            props = new Dictionary<string, object>() {
                { PropertyIds.Name, documentName },
                { PropertyIds.ObjectTypeId, "cmis:document" },
                { "cmis:description", "Hello World" },
            };
            folder.CreateDocument(props, contentStream, VersioningState.Major);
            document = session.GetObjectByPath("/OCC Test/" + documentName) as IDocument;
            description = document.GetPropertyValue("cmis:description") as string;
        }

        [TestMethod] [Ignore]
        [TestCategory("Test and explore")]
        public void CMIS_986()
        // https://issues.apache.org/jira/browse/CMIS-986
        {
            string documentName = "Document.pdf";
            string documentFile = @"c:\temp\" + documentName;
            IContentStream contentStream;
            IFolder folder;
            Dictionary<string, object> props;
            IDocument document;
            string versionLabel;

            ISession session = getInMemorySession();
            FileInfo file = new FileInfo(documentFile);
            contentStream = session.ObjectFactory.CreateContentStream(
                documentName, file.Length, "application/pdf",
                new FileStream(documentFile, FileMode.Open));

            folder = session.GetObjectByPath("/My_Folder-0-0") as IFolder;
            props = new Dictionary<string, object>() {
                { PropertyIds.Name, documentName },
                { PropertyIds.ObjectTypeId, "VersionableType" },
            };
            folder.CreateDocument(props, contentStream, VersioningState.Major);
            document = session.GetObjectByPath("/My_Folder-0-0/" + documentName) as IDocument;
            versionLabel = document.GetPropertyValue("cmis:versionLabel") as string;
            contentStream.Stream.Dispose();

            props.Remove(PropertyIds.Name);
            contentStream = session.ObjectFactory.CreateContentStream(
                documentName, file.Length, "application/pdf",
                new FileStream(documentFile, FileMode.Open));
            var pwcId = document.CheckOut();
            Document pwc = (Document)session.GetObject(pwcId);
            pwc.CheckIn(true, props, contentStream, "Some comment");
            document = session.GetObjectByPath("/My_Folder-0-0/" + documentName) as IDocument;
            document = document.GetObjectOfLatestVersion(false);
            versionLabel = document.GetPropertyValue("cmis:versionLabel") as string;
        }

        [TestMethod] [Ignore]
        [TestCategory("Test and explore")]
        public void CMIS_1018()
        // https://issues.apache.org/jira/browse/CMIS-1018
        {
            System.Numerics.BigInteger value = 0;
            string xx = value.ToString("#", System.Globalization.CultureInfo.InvariantCulture);

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters[SessionParameter.BindingType] = PortCMIS.BindingType.AtomPub;
            parameters[SessionParameter.AtomPubUrl] = "http://107.189.71.11:7000/emc-cmis/resources";
            parameters[SessionParameter.User] = "dmadmin";
            parameters[SessionParameter.Password] = "demo.demo";

            SessionFactory factory = SessionFactory.NewInstance();
            ISession session = factory.GetRepositories(parameters)[0].CreateSession();
            IFolder folder = session.GetRootFolder();
            foreach (ICmisObject co in folder.GetChildren())
            {
            }
        }

        class MyDocument : IDocumentTypeDefinition
        {
            public string Id { get { return "occDocument"; } }
            public string LocalName { get { return "occDocument"; } }
            public string LocalNamespace { get { return "http://opentext.com"; } }
            public string DisplayName { get { return "OCC Document"; } }
            public string QueryName { get { return "occDocument"; } }
            public string Description { get { return "Special type to test all properties"; } }
            public BaseTypeId BaseTypeId { get { return BaseTypeId.CmisDocument; ; } }
            public string ParentTypeId { get { return "cmis:document"; } }
            public bool? IsCreatable { get { return true; } }
            public bool? IsFileable { get { return true; } }
            public bool? IsQueryable { get { return true; } }
            public bool? IsFulltextIndexed { get { return false; } }
            public bool? IsIncludedInSupertypeQuery { get { return true; } }
            public bool? IsControllablePolicy { get { return true; } }
            public bool? IsControllableAcl { get { return true; } }
            public IPropertyDefinition this[string propertyId] { get { return null; } }
            public IList<IPropertyDefinition> PropertyDefinitions { get; }
            public ITypeMutability TypeMutability { get; }
            public bool? IsVersionable { get { return true; } }
            public ContentStreamAllowed? ContentStreamAllowed { get { return PortCMIS.Enums.ContentStreamAllowed.Allowed; } }
            public IList<ICmisExtensionElement> Extensions { get { return null; } set {; } }
        }
    }
}
