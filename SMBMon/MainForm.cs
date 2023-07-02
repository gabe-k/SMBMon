using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using SMBLibrary;
using SMBLibrary.Server;
using SMBLibrary.Authentication.GSSAPI;
using SMBLibrary.Authentication.NTLM;

namespace SMBMon
{
    public partial class MainForm : Form
    {
        private SMBServer m_server;

        public MainForm()
        {
            InitializeComponent();
        }

        // dummy function for the auth provider
        public string GetUserPassword(string accountName)
        {
            if (accountName == "Guest")
            {
                return String.Empty;
            }
            return null;
        }

        private void startServerButton_Click(object sender, EventArgs e)
        {
            SMBShareCollection shares = new SMBShareCollection();
            Program.FilteredFileSystem = new NTFilteredFileSystem("C:\\test");
            FileSystemShare share = new FileSystemShare("test", Program.FilteredFileSystem);
            shares.Add(share);

            NTLMAuthenticationProviderBase authenticationMechanism = new IndependentNTLMAuthenticationProvider(GetUserPassword);
            GSSProvider securityProvider = new GSSProvider(authenticationMechanism);
            m_server = new SMBServer(shares, securityProvider);
            m_server.Start(IPAddress.Any, SMBTransportType.DirectTCPTransport, false, true);
        }

        public void AddLogEntry(SMBLogEntry entry)
        {
            string[] columns = { entry.Time.ToString(), entry.Handle.ToString("X"), entry.Operation.ToString(), entry.Path.ToString(), entry.Result.ToString("X"), entry.Detail };
            eventsListView.Invoke((MethodInvoker)delegate
            {
                eventsListView.Items.Add(new ListViewItem(columns));
                eventsListView.Items[eventsListView.Items.Count - 1].EnsureVisible();
            });
        }

        private void addFilterButton_Click(object sender, EventArgs e)
        {
            SMBFilterClause clause = new SMBFilterClause(FilterField.Path, FilterOperand.Contains, "test");
            SMBFilter filter = new SMBFilter(NTFileOperation.CreateFile, FilterAction.Log);
            filter.AddClause(clause);
            Program.FilteredFileSystem.AddFilter(filter);
        }
    }
}
