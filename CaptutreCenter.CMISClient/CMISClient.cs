using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

using PortCMIS;
using PortCMIS.Binding;
using PortCMIS.Client;
//using PortCMIS.Const;
using PortCMIS.Data;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using PortCMIS.Utils;
using PortCMIS.Client.Impl;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    #region Interface
    public enum TypeOfBinding { Atom = 0, Browser = 1, WebService = 2 };

    public interface ICMISClient
    {
        // Connection
        string ServerURL { get; set; }
        TypeOfBinding TypeOfBinding { get; set; }
        string Username { get; set; }
        string Password { get; set; }

        string GetSupportedVersion();

        // Repositories
        List<CMISRepository> LoadRepositories();
        void SelectRepository(string repositoryId);

        // Folder
        CMISFolder GetRootFolder();
        CMISFolder GetFolderFromId(string folderId);
        CMISFolder GetFolderFromPath(string path);
        CMISFolder GetSubfolder(CMISFolder baseFolder, string pathExtension, string folderType);
        void DeleteFolder(CMISFolder folder);

        // Types
        CMISType GetRootType();
        CMISType GetTypeFromId(string typeId);
        List<CMISProperty> GetPropertyDefinitions(CMISType type);

        // Documents
        CMISDocument GetDocument(CMISFolder folder, string documentName);
        void DeleteDocument(CMISDocument document);
        void StoreDocument(CMISFolder folder, string document, string docName,
            Dictionary<string, object> props, bool? major);
        void UpdateDocument(CMISFolder folder, string document, string docName,
            Dictionary<string, object> props, bool major, string checkInComment);
        object GetPropertyValue(CMISDocument document, string propertyName);
        string GetCheckinComent(CMISDocument document);
        CMISDocument GetObjectOfLatestVersion(CMISDocument document, bool major);
    }
    #endregion

    #region Interface objects
    [Serializable]
    public class CMISRepository
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
    public class CMISFolder
    {
        private ISession session;
        public CMISFolder(IFolder folder, ISession session)
        {
            Id = folder.Id;
            DisplayName = folder.Name;
            this.session = session;
        }
        public CMISFolder(string id, string name) { Id = id;  DisplayName = name; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public IFolder IFolder(ISession session) { return session.GetObject(Id) as Folder; }
        public List<CMISFolder> GetAllSubFolders()
        {
            List<CMISFolder> result = new List<CMISFolder>();
            try
            {
                foreach (ICmisObject co in IFolder(session).GetChildren())
                    if (co is IFolder) result.Add(new CMISFolder(co as IFolder, session));
            }
            catch (Exception e) { CMISClient.CMISClientException(e); }
            return result;
        }
    }
    public class CMISType
    {
        public CMISType(ITree<IObjectType> node)
        {
            this.node = node;
            Type = this.node.Item;
            DisplayName = node.Item.DisplayName;
            Id = node.Item.Id;
            Versionable = (Type as IDocumentType).IsVersionable;
        }
        public CMISType(string id, string name) { Id = id; DisplayName = name; }
        private ITree<IObjectType> node;
        public IObjectType Type { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public bool? Versionable { get; set; }
        public List<CMISType> GetAllSubTypes()
        {
            if (node.Item.Id != Id) throw new Exception("Can't use this CMISType");
            List<CMISType> result = new List<CMISType>();
            if (node.Children == null) return result;
            foreach (ITree<IObjectType> node in node.Children)
                result.Add(new CMISType(node));
            return result;
        }
    }
    public class CMISDocument
    {
        public CMISDocument(IDocument document) { Id = document.Id; Name = document.Name; }
        public CMISDocument(string id, string name) { Id = id; Name = name; }
        public string Id { get; set; }
        public string Name { get; set; }
        public IDocument IDocument(ISession session) { return session.GetObject(Id) as IDocument; }
    }
    public enum CMISClientType { Boolean, DateTime, Decimal, Integer, String }

    #endregion

    #region CMIS Property
    [Serializable]
    public class CMISProperty : ModelBase
    {
        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetField(ref _id, value); }
        }
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetField(ref _displayName, value); }
        }
        private CMISClientType _type;
        public CMISClientType Type
        {
            get { return _type; }
            set { SetField(ref _type, value); }
        }
        private bool _isMulti;
        public bool IsMulti
        {
            get { return _isMulti; }
            set { SetField(ref _isMulti, value); }
        }
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { SetField(ref _selected, value); }
        }

        public object ConvertValue(string value, CultureInfo cultureInfo)
        {
            return ConvertValue(new SIEEField() { Value = value, Cardinality = 0 }, cultureInfo);
        }
        public object ConvertValue(SIEEField field, CultureInfo cultureInfo)
        {
            switch (Type)
            {
                case CMISClientType.Boolean:    { return convert<bool>(convertBool, field, cultureInfo); }
                case CMISClientType.DateTime:   { return convert<DateTime>(convertDateTime, field, cultureInfo); }
                case CMISClientType.Decimal:    { return convert<Decimal>(convertDecimal, field, cultureInfo); }
                case CMISClientType.Integer:    { return convert<Int32>(convertInteger, field, cultureInfo); }
                default:                        { return convert<string>(convertString, field, cultureInfo); }
            }
        }

        private object convert<T>(converType converter, SIEEField field, CultureInfo cultureInfo)
        {
            List<T> resultList = new List<T>();

            if (!IsMulti) return converter(field.Value, cultureInfo);

            if (field.Cardinality == 0 || field.ValueList.Count == 0)
                resultList.Add((T)converter(field.Value, cultureInfo));
            else
                foreach (string s in field.ValueList)
                    resultList.Add((T)converter(s, cultureInfo));
            return resultList;
        }

        delegate object converType(string value, CultureInfo cultureInfo);

        private object convertBool(string value, CultureInfo cultureInfo)
        {
            if (value == null || value == string.Empty) value = "false";
            return bool.Parse(value);
        }
        private object convertDateTime(string value, CultureInfo cultureInfo)
        {
            if (value == null || value == string.Empty) value = DateTime.Now.ToString(cultureInfo);
            return DateTime.Parse(value, cultureInfo);
        }
        private object convertDecimal(string value, CultureInfo cultureInfo)
        {
            if (value == null || value == string.Empty) value ="0";
            return Decimal.Parse(value, cultureInfo);
        }
        private object convertInteger(string value, CultureInfo cultureInfo)
        {
            if (value == null || value == string.Empty) value = "0";
            return Int32.Parse(value, cultureInfo);
        }
        private object convertString(string value, CultureInfo cultureInfo)
        {
            if (value == null) value = string.Empty;
            return value;
        }
    }
    #endregion

    #region Implementation
    public class CMISClient : ICMISClient
    {
        public static void CMISClientException(Exception e)
        {
            string myMessage = e.Message;
            if (e is CmisBaseException)
            {
                CmisBaseException ce = e as CmisBaseException;
                if (ce.ErrorContent != null)
                    myMessage += "-->" + (e as CmisBaseException).ErrorContent;
            }
            throw new Exception(myMessage);
        }
        #region Connection
        private ISession session { get; set; }
        public string ServerURL { get; set; }
        public TypeOfBinding TypeOfBinding { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string GetSupportedVersion()
        {
            return session.RepositoryInfo.CmisVersionSupported;
        }
        #endregion

        #region Repositories
        IList<IRepository> cmisRepositories = null;

        public List<CMISRepository> LoadRepositories()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            switch(TypeOfBinding)
            {
                case TypeOfBinding.Atom:
                    {
                        parameters[SessionParameter.BindingType] = BindingType.AtomPub;
                        parameters[SessionParameter.AtomPubUrl] = ServerURL;
                        break;
                    }
                case TypeOfBinding.Browser:
                    {
                        parameters[SessionParameter.BindingType] = BindingType.Browser;
                        parameters[SessionParameter.BrowserUrl] = ServerURL;
                        break;
                    }
                case TypeOfBinding.WebService:
                    {
                        parameters[SessionParameter.BindingType] = BindingType.AtomPub;
                        parameters[SessionParameter.AtomPubUrl] = ServerURL;
                        break; }
            }
            parameters[SessionParameter.User] = Username;
            parameters[SessionParameter.Password] = Password;

            SessionFactory factory = SessionFactory.NewInstance();
            cmisRepositories = factory.GetRepositories(parameters);
            List<CMISRepository> result = new List<CMISRepository>();
            foreach (IRepository r in cmisRepositories)
                result.Add(new CMISRepository()
                {
                    Id = r.Id,
                    Description = r.Id + "\n" + r.Name + "\n" + r.Description,
                });
            return result;
        }

        public void SelectRepository(string repositoryId)
        {
            if (cmisRepositories == null || cmisRepositories.Count == 0) throw (new Exception("No repositories loaded"));
            session = cmisRepositories.Where(n => n.Id == repositoryId).First().CreateSession();
        }
        #endregion

        #region Folder
        public CMISFolder GetRootFolder()
        {
            return new CMISFolder(session.GetRootFolder(), session);
        }

        public CMISFolder GetFolderFromId(string folderId)
        {
            return new CMISFolder(session.GetObject(folderId) as IFolder, session);
        }

        public CMISFolder GetFolderFromPath(string path)
        {
            try { return new CMISFolder(session.GetObjectByPath(path) as IFolder, session); }
            catch { } // OpenText Domea fails on this
            IFolder curr = session.GetRootFolder();
            foreach (string pathElement in path.Split('/'))
            {
                if (pathElement == string.Empty) continue;
                curr = curr.GetChildren().Where(n => n is IFolder && n.Name == pathElement).First() as IFolder;
            }
            return new CMISFolder(curr, session);
        }

        public CMISFolder GetSubfolder(CMISFolder baseFolder, string pathExtension, string folderType)
        {
            IDictionary<string, object> props = new Dictionary<string, object>();
            props["cmis:objectTypeId"] = folderType;

            IFolder currFolder = baseFolder.IFolder(session);
            foreach (string element in pathExtension.Split('/'))
            {
                if (element == string.Empty) continue; // ignore leadiong "/"
                List<ICmisObject> childs = currFolder.GetChildren().Where(n => n.Name == element && n is IFolder).ToList();
                if (childs.Count > 0)
                {
                    // if subfolder already exists -> use it
                    currFolder = (IFolder)childs[0];
                    continue;
                }
                // else create it
                props[PropertyIds.Name] = element;
                currFolder = currFolder.CreateFolder(props);
            }
            return new CMISFolder(currFolder, session);
        }

        public void DeleteFolder(CMISFolder folder)
        {
            session.Delete(folder.IFolder(session));
        }
        #endregion

        #region Types
        public CMISType GetRootType()
        {
            ITree<IObjectType> node = session.GetTypeDescendants(null, 1, true).Where(n => n.Item is DocumentType).First();
            return new CMISType(node);
        }

        public CMISType GetTypeFromId(string typeId)
        {
            CMISType result = GetRootType();
            result.Type = session.GetTypeDefinition(typeId) as IObjectType;
            result.Id = result.Type.Id;
            result.DisplayName = result.Type.DisplayName;
            result.Versionable = (result.Type as IDocumentType).IsVersionable;
            return result;
        }

        public List<CMISProperty> GetPropertyDefinitions(CMISType type)
        {
            List<CMISProperty> result = new List<CMISProperty>();
            foreach (IPropertyDefinition p in type.Type.PropertyDefinitions)
            {
                if (p.Updatability == Updatability.ReadWrite && typeMap.Keys.Contains(p.PropertyType))
                    result.Add(new CMISProperty()
                    {
                        Id = p.Id,
                        DisplayName = p.DisplayName,
                        Type = typeMap[p.PropertyType],
                        IsMulti = p.Cardinality == Cardinality.Multi,
                    });
            }
            return result;
        }

        private Dictionary<PropertyType, CMISClientType> typeMap = new Dictionary<PropertyType, CMISClientType>() {
            { PropertyType.Boolean, CMISClientType.Boolean },
            { PropertyType.DateTime, CMISClientType.DateTime },
            { PropertyType.Decimal, CMISClientType.Decimal },
            { PropertyType.Integer, CMISClientType.Integer },
            { PropertyType.String, CMISClientType.String },
        };
        #endregion

        #region Documents
        public CMISDocument GetDocument(CMISFolder folder, string documentName)
        {
            List<ICmisObject> found = folder.IFolder(session).GetChildren().Where(n => n.Name == documentName && n is IDocument).ToList();
            if (found == null || found.Count == 0) return null;
            return new CMISDocument(found.First() as IDocument);
        }
        public void DeleteDocument(CMISDocument document) { session.Delete(document.IDocument(session)); }

        public void StoreDocument(
            CMISFolder folder,                  // where to put the document
            string document,                    // file name of the document
            string docName,                     // name of the new document
            Dictionary<string, object> props,   // document properties
            bool? major)                         // use versioning and which one
        {
            IContentStream contentStream = getContentStream(document, docName);
            try
            {
                IFolder iFolder = folder.IFolder(session);
                VersioningState? vs = null;
                if (major != null) vs = major == true? VersioningState.Major : VersioningState.Minor;
                props[PropertyIds.Name] = docName;
                iFolder.CreateDocument(props, contentStream, vs);
                props.Remove(PropertyIds.Name);
            }
            catch (CmisBaseException e) { throw new Exception(e.Message + "\n" + e.ErrorContent); }
            finally { contentStream.Stream.Dispose(); }
        }

        public void UpdateDocument(
            CMISFolder folder,
            string document,
            string docName,
            Dictionary<string, object> props,
            bool major,
            string checkInComment)
        {
            IContentStream contentStream = getContentStream(document, docName);
            try
            {
                IDocument d = GetDocument(folder, docName).IDocument(session);
                d.UpdateProperties(props);
                if (d.AllowableActions == null ||
                    d.AllowableActions.Actions.Contains(PortCMIS.Enums.Action.CanCheckOut))
                {
                    var pwcId = d.CheckOut();
                    Document pwc = (Document)session.GetObject(pwcId);
                    pwc.CheckIn(major, props, contentStream, checkInComment);
                }
                else
                    d.CheckIn(major, props, contentStream, checkInComment);
            }
            catch (CmisBaseException e) { throw new Exception(e.ErrorContent != null? e.ErrorContent : e.Message); }
            finally { contentStream.Stream.Dispose(); }
        }

        private IContentStream getContentStream(string document, string docName)
        {
            FileInfo file = new FileInfo(document);
            return session.ObjectFactory.CreateContentStream(
                docName, file.Length, "application/pdf", 
                new FileStream(document, FileMode.Open));
        }

        public object GetPropertyValue(CMISDocument document, string propertyName)
        {
            return document.IDocument(session).GetPropertyValue(propertyName);
        }

        public string GetCheckinComent(CMISDocument document)
        {
            IDocument docId = document.IDocument(session);
            IDocument latest = session.GetLatestDocumentVersion(docId);
            return latest.GetPropertyValue("cmis:checkinComment") as string;
        }

        public CMISDocument GetObjectOfLatestVersion(CMISDocument document, bool major)
        {
            return new CMISDocument(document.IDocument(session).GetObjectOfLatestVersion(major));
        }
        #endregion

    }
    #endregion
}
