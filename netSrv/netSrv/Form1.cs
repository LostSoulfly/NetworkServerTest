using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Network;
using Network.Enums;
using Network.Packets;
using Packets;

namespace netSrv
{
    public partial class Form1 : Form
    {

        private ServerConnectionContainer secureServer;
        private ClientConnectionContainer secureClient;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string privateKey;
            string publicKey;
            int port = 1234;

            try
            {
                privateKey = File.ReadAllText("privateKey.xml");
                publicKey = File.ReadAllText("publicKey.xml");
            } catch
            {
                MessageBox.Show("Problem loading keys..");
                return;
            }

            secureServer = ConnectionFactory.CreateSecureServerConnectionContainer(port, publicKey, privateKey, start: false);

            secureServer.ConnectionLost += serverConnectionLost;
            secureServer.ConnectionEstablished += serverConnectionEstablished;

            secureServer.AllowBluetoothConnections = false;

            secureServer.Start();
            AddText(textBoxServer, $"Server started on port {port}" + System.Environment.NewLine);

        }

        private void serverConnectionEstablished(Connection connection, ConnectionType connectionType)
        {
            AddText(textBoxServer, $"Connection established from {connection.IPRemoteEndPoint}" + System.Environment.NewLine);

            connection.RegisterPacketHandler<Packets.TestPacket>(serverPacketReceived, connection);
            connection.RegisterRawDataHandler("String", serverStringHandler);

        }

        private void serverStringHandler(RawData packet, Connection connection)
        {
            AddText(textBoxServer, packet.ToString());
        }

        private void clientStringHandler(RawData packet, Connection connection)
        {
            AddText(textBoxClient, packet.ToString());
        }

        private void serverPacketReceived(TestPacket packet, Connection connection)
        {
            AddText(textBoxServer, packet.test + System.Environment.NewLine);
        }

        private void clientPacketReceived(TestPacket packet, Connection connection)
        {
            AddText(textBoxClient, packet.test + System.Environment.NewLine);
        }

        private void serverConnectionLost(Connection connection, ConnectionType connectionType, Network.Enums.CloseReason closeReason)
        {
            AddText(textBoxServer, $"Connection lost from {connection.IPRemoteEndPoint}: {closeReason.ToString()}" + System.Environment.NewLine);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
                File.WriteAllText("publicKey.xml", rsa.ToXmlString(false));
                File.WriteAllText("privateKey.xml", rsa.ToXmlString(true));
                textBoxClient.Text += "New keys generated.." + System.Environment.NewLine;
                textBoxServer.Text += "New keys generated.." + System.Environment.NewLine;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating/saving keys: " + ex);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (secureServer.IsTCPOnline)
                secureServer.Stop();

            if (secureClient.IsAlive)
                secureClient.Shutdown(Network.Enums.CloseReason.ClientClosed);

            secureServer = null;
            secureClient = null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string privateKey;
            string publicKey;
            int port = 1234;
            string ipAddress = "127.0.0.1";

            try
            {
                privateKey = File.ReadAllText("privateKey.xml");
                publicKey = File.ReadAllText("publicKey.xml");
            }
            catch
            {
                MessageBox.Show("Problem loading keys..");
                return;
            }

            secureClient = ConnectionFactory.CreateSecureClientConnectionContainer(ipAddress, port, publicKey, privateKey);

            secureClient.ConnectionEstablished += clientConnectionEstablished;
            secureClient.ConnectionLost += clientConnectionLost;

            secureClient.AutoReconnect = false;


        }

        private void AddText(TextBox textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new MethodInvoker(() => textBox.Text += text + Environment.NewLine));
            }
            else
            {
                textBox.Text += text + Environment.NewLine;
            }
        }

        private void clientConnectionLost(Connection connection, ConnectionType arg2, Network.Enums.CloseReason closeReason)
        {
            AddText(textBoxClient, $"Connection lost to {connection.IPRemoteEndPoint}: {closeReason.ToString()}" + System.Environment.NewLine);
        }

        private void clientConnectionEstablished(Connection connection, ConnectionType arg2)
        {
            AddText(textBoxClient, $"Connection established to {connection.IPRemoteEndPoint}" + System.Environment.NewLine);
            connection.RegisterPacketHandler<Packets.TestPacket>(clientPacketReceived, connection);
            connection.RegisterRawDataHandler("String", clientStringHandler);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                secureClient.Send(new TestPacket(textBox2.Text));
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                foreach (Connection conn in secureServer.TCP_Connections)
                {
                    if (conn.IsAlive)
                        conn.Send(new TestPacket(textBox3.Text));
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
