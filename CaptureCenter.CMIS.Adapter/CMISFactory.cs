using System;
using System.Drawing;
using System.Windows.Controls;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    public class CMISFactory : SIEEFactory
    {
        public override SIEESettings CreateSettings() { return new CMISSettings(); }
        public override SIEEUserControl CreateWpfControl() { return new CMIS_WPFControl(); }
        public override SIEEViewModel CreateViewModel(SIEESettings settings) { return new CMISViewModel(settings, new CMISClient()); }
        public override SIEEExport CreateExport() { return new CMISExport(new CMISClient()); }
        public override SIEEDescription CreateDescription() { return new CMISDescription(); }
    }

    class CMISDescription : SIEEDescription
    {
        public override string TypeName { get { return "CMIS"; } }
        public override string DefaultNewName { get { return "CMIS"; } }
        public override string GetLocation(SIEESettings s) { return ((CMISSettings)s).GetLocation(); }
        public override System.Drawing.Image Image { get { return Properties.Resources.CMISIcon; } }
    }
}
