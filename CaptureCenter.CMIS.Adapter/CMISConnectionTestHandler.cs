using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    public class eDocsConnectionTestHandler : ConnectionTestHandler
    {
        // Define the tests
        public eDocsConnectionTestHandler(VmTestResultDialog vmTestResultDialog) : base(vmTestResultDialog)
        {
            TestList.Add(new TestFunctionDefinition()
            { Name = "Try to reach CMIS server (ping)", Function = TestFunction_Ping, ContinueOnError = true });
            TestList.Add(new TestFunctionDefinition()
            { Name = "Try to get repositories", Function = TestFunction_Login });
            TestList.Add(new TestFunctionDefinition()
            { Name = "Try to read root folders", Function = TestFunction_Read });
        }

        #region The test fucntions
        // Implement the tests
        private CMISClient cmisClient;

        private bool TestFunction_Ping(ref string errorMsg)
        {
            Ping pinger = new Ping();
            string serverURL =(CallingViewModel as CMISViewModel_CT).ServerURL;
            if (serverURL == null || serverURL == "")
            {
                errorMsg = "No server specified";
                return false;
            }
            try
            {
                string servername = (new Uri(serverURL)).Host;
                PingReply reply = pinger.Send(servername);
                if (reply.Status != IPStatus.Success)
                {
                    errorMsg = "Return status = " + reply.Status.ToString();
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                if (e.InnerException != null) errorMsg += "\n" + e.InnerException.Message;
                return false;
            }
        }

        private bool TestFunction_Login(ref string errorMsg)
        {
            CMISViewModel_CT viewModel = ((CMISViewModel_CT)CallingViewModel);
            try
            {
                cmisClient = viewModel.GetCMISClient() as CMISClient;
                List<CMISRepository> repositories = cmisClient.LoadRepositories();
                cmisClient.SelectRepository(repositories[0].Id);
            }
            catch (Exception e)
            {
                errorMsg = "Could not log in. \n" + e.Message;
                if (e.InnerException != null)
                    errorMsg += "\n" + e.InnerException.Message;
                return false;
            }
            return true;
        }

        private bool TestFunction_Read(ref string errorMsg)
        {
            try
            {
                cmisClient.GetRootFolder();
                return true;
            }
            catch (Exception e)
            {
                errorMsg = "Could not read solutions\n" + e.Message;
                if (e.InnerException != null)
                    errorMsg += "\n" + e.InnerException.Message;
                return false;
            }
        }
        #endregion
    }
}
