using System;
using RightDocs.Common;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    [CustomExportDestinationDescription("CMISWriter", "ExportExtensionInterface", "SIEE based Writer for CMIS compliant Export", "OpenText")]
    public class CMISWriter : EECExportDestination
    {
        public CMISWriter() : base()
        {
            Initialize(new CMISFactory());
        }
    }
}