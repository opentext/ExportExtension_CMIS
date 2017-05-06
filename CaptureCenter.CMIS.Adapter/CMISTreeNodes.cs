using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    #region CMIS folder node
    public sealed class CMISFolderNode : TVIModel
    {
        #region Construction
        public CMISFolderNode() { } // for xml serializer

        public CMISFolderNode(CMISFolderNode parent, CMISFolder folder)
        {
            CMISFolder = folder;
            FolderPath = parent == null ? folder.DisplayName : parent.FolderPath + GetPathConcatenationString() + folder.DisplayName;
        }
        #endregion

        #region Properties
        private CMISFolder cmisFolder;
        [XmlIgnore]
        public CMISFolder CMISFolder
        {
            get { return cmisFolder; }
            set { cmisFolder = value; Id = cmisFolder.Id; DisplayName = cmisFolder.DisplayName; }
        }
        public string FolderPath { get; set; }
        #endregion

        #region Functions
        public override List<TVIModel> GetChildren()
        {
            List<TVIModel> result = new List<TVIModel>();
            try
            {
                foreach (CMISFolder f in CMISFolder.GetAllSubFolders())
                    result.Add((TVIModel)new CMISFolderNode(this, f));
            }
            catch (Exception e)
            {
                SIEEMessageBox.Show(
                    "Error loading subfolders\n" + e.Message,
                    "CMIS Connector",
                    System.Windows.MessageBoxImage.Error);
            }
            return result;
        }

        public override string GetPathConcatenationString() { return "/"; }
        public override string GetTypeName() { return "Folder"; }

        public override TVIModel Clone()
        {
            return this.MemberwiseClone() as CMISFolderNode;
        }

        public override string GetPath(List<TVIModel> path, Pathtype pt)
        {
            string result = string.Empty;
            for (int i = 0; i < path.Count; i++)
                result += GetPathConcatenationString() + (pt == Pathtype.Id ? path[i].Id : path[i].DisplayName);
            return result;
        }
        #endregion
    }
    #endregion

    #region CMIS type node
    public sealed class CMISTypeNode : TVIModel
    {
        #region Construction
        public CMISTypeNode() { } // for xml serializer

        public CMISTypeNode(CMISTypeNode parent, CMISType iType)
        {
            CMISType = iType;
            TypePath = parent == null ? iType.DisplayName : parent.TypePath + GetPathConcatenationString() + iType.DisplayName;
        }
        #endregion

        #region Properties
        private CMISType cmisType;
        [XmlIgnore]
        public CMISType CMISType
        {
            get { return cmisType; }
            set { cmisType = value; Id = cmisType.Id; DisplayName = cmisType.DisplayName; }
        }
        public string TypePath { get; set; }

        public override string Icon { get { return "Default"; } }
        #endregion

        #region Functions
        public override List<TVIModel> GetChildren()
        {
            List<TVIModel> result = new List<TVIModel>();
            foreach (CMISType co in CMISType.GetAllSubTypes())
                result.Add((TVIModel)new CMISTypeNode(this, co));
            return result;
        }

        public override string GetPathConcatenationString() { return "/"; }
        public override string GetTypeName() { return "Type"; }

        public override TVIModel Clone()
        {
            return this.MemberwiseClone() as CMISTypeNode;
        }

        public override string GetPath(List<TVIModel> path, Pathtype pt)
        {
            string result = string.Empty;
            for (int i = 0; i != path.Count; i++)
                result += GetPathConcatenationString() + (pt == Pathtype.Id? path[i].Id : path[i].DisplayName);
            return result;
        }
        #endregion
    }
    #endregion

}
