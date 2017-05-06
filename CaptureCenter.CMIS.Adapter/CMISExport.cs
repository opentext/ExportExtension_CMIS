using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Globalization;
using ExportExtensionCommon;
using DOKuStar.Diagnostics.Tracing;

namespace CaptureCenter.CMIS
{
    public class CMISExport : SIEEExport
    {
        private static readonly ITrace trace = TraceManager.GetTracer(typeof(CMISExport));
        private CMISSettings mySettings;
        private ICMISClient cmisClient;
        private const string pdfExtension = ".pdf";
        private string selectedTypeId;
        private string selectedFolderId;
        private CultureInfo clientCulture;

        public CMISExport(ICMISClient cmisClient) : base()
        {
           this.cmisClient = cmisClient;
        }

        public override void Init(SIEESettings settings)
        {
            mySettings = (CMISSettings)settings;
            mySettings.InitializeCMISClient(cmisClient);
            clientCulture = new CultureInfo(mySettings.SelectedCultureInfoName);

            cmisClient.LoadRepositories();
            cmisClient.SelectRepository(mySettings.SelectedRepository.Id);
            selectedTypeId = mySettings.SelectedType;
            selectedFolderId = (TVIViewModel.deSerialize(mySettings.SerializedFolderPath.Last(), typeof(CMISFolderNode)) as CMISFolderNode).Id;
        }

        public override void ExportDocument(SIEESettings settings, SIEEDocument document, string name, SIEEFieldlist fieldlist)
        {
            CMISFolder folder = determineFolder(fieldlist);
            string docName = name;

            // Collision detection and handling (except for version handling)
            CMISDocument existingDocument = cmisClient.GetDocument(folder, docName + pdfExtension);
            if (existingDocument != null) switch (mySettings.SelectedConflictHandling)
            {
                case CMISSettings.ConflictHandling.Replace:
                        { cmisClient.DeleteDocument(existingDocument); break; }
                case CMISSettings.ConflictHandling.AddBlurb:
                        { docName = getDocumentNameWithBlurb(docName); break; }
                case CMISSettings.ConflictHandling.AddNumber:
                        { docName = getDocumentNameWithNumber(docName, folder); break; }
                default: break;
            }

            // Create property list
            string checkInComment = "OCC created version";
            Dictionary<string, object> props = new Dictionary<string, object>();
            props["cmis:objectTypeId"] = selectedTypeId;
 
            foreach (SIEEField f in fieldlist)
            {
                if (f.ExternalId == "cmis:checkinComment") { checkInComment = f.Value; continue; }
                if (mySettings.UseSubFolderField && 
                    mySettings.SubFolderTypeFixed &&
                    f.ExternalId == mySettings.SubFolderField) continue;
                if (mySettings.UseSubFolderField && 
                    mySettings.SubFolderTypeFromField && 
                    f.ExternalId == mySettings.SubFolderTypeField) continue;

                try { props[f.ExternalId] = convert(mySettings, f); }
                catch (Exception e) { throw new Exception(
                    "Error converting value for field '" + f.ExternalId + "' Value = '" + f.Value +
                    "' \nReason: " + e.Message
                ); }
            }

            try
            {
                string dn = docName + pdfExtension;
                if (cmisClient.GetDocument(folder, docName + pdfExtension) == null || !versionHandling())
                {
                    bool? v = null;
                    if (versionHandling()) v = mySettings.Major;
                    cmisClient.StoreDocument(folder, document.PDFFileName, dn, props, v);
                }
                else
                    cmisClient.UpdateDocument(folder, document.PDFFileName, dn, props, mySettings.Major, checkInComment);
            }
            catch (Exception e) { trace.WriteError(e.Message); throw; }
        }

        private object convert(CMISSettings settings, SIEEField field)
        {
            List<CMISProperty> props = settings.Properties.Where(n => n.Id == field.ExternalId).ToList();
            if (props == null || props.Count == 0) return field.Value;
            return props.First().ConvertValue(field, clientCulture);
        }

        private CMISFolder determineFolder(SIEEFieldlist fieldlist)
        {
            CMISFolder folder = cmisClient.GetFolderFromId(selectedFolderId);

            if (mySettings.UseSubFolderField)
            {
                // Get contents of subfolder field into pathExtension
                List<SIEEField> match = fieldlist.Where(n => n.Name == mySettings.SubFolderField).ToList();
                if (match.Count != 1) throw (new Exception("No field " + mySettings.SubFolderField + " in data"));
                string pathExtension = match.First().Value;

                // Get sub folder type
                string ftype = "cmis:folder";
                if (mySettings.UseSubFolderType && mySettings.SubFolderTypeFixed) ftype = mySettings.SubFolderType;
                if (mySettings.UseSubFolderType && mySettings.SubFolderTypeFromField)
                {
                    string newType = fieldlist.Where(n => n.ExternalId == mySettings.SubFolderTypeField).Select(n => n.Value).First();
                    if (newType != "") ftype = newType;
                }
                // Get the folder
                folder = cmisClient.GetSubfolder(folder, pathExtension, ftype);
            }
            return folder;
        }

        // Return true if versioning for conflict handling is active
        private bool versionHandling()
        {
            return mySettings.SelectedConflictHandling == CMISSettings.ConflictHandling.AddVersion;
        }

        // Return the document name for blurb collision handling
        private string getDocumentNameWithBlurb(string basename)
        {
            string s = Path.GetRandomFileName();
            return basename + "_" + s.Substring(0, 8) + s.Substring(9, 3);
        }
        
        // return the filename for addNumber collision handling
        private string getDocumentNameWithNumber(string basename, CMISFolder folder)
        {
            DocumentNameFindNumber dnfn = new DocumentNameFindNumber(DNFN_probe);
            currFolder = folder;
            int nextNumber = dnfn.GetNextFileName(basename);
            return fileNameWithNumber(basename, nextNumber);
        }

        private CMISFolder currFolder;
        private bool DNFN_probe(string filename, int number)
        {
            string fn = fileNameWithNumber(filename, number) + pdfExtension;
            return cmisClient.GetDocument(currFolder, fn) != null;
        }
        private string fileNameWithNumber(string basename, int number)
        {
            string format = "{0}_{1:D" + mySettings.NumberOfDigits + "}";
            return string.Format(format, basename, number);
        }
    }
}
