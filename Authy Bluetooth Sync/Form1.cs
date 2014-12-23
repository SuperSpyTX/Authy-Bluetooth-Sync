using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using InTheHand.Windows.Forms;
using InTheHand.Net.Bluetooth;
using System.Threading;
using System.IO;

namespace Authy_Bluetooth_Sync
{
    public partial class Form1 : Form
    {
        private BluetoothDeviceInfo device;
        private StreamWriter writer;
        private BluetoothClient cli = new BluetoothClient();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //= ... select one of peer()...
           // BluetoothAddress addr = device.DeviceAddress;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           /* BluetoothDeviceInfo[] peers = cli.DiscoverDevices();
            foreach (BluetoothDeviceInfo device2 in peers) {
                if (device2.DeviceName.Equals("Big 6"))
                {
                    this.device = device2;
                    break;
                }
                //MessageBox.Show(device.DeviceName + " - " + device.DeviceAddress);
            }*/
            //BluetoothDeviceInfo device = null;
            var dlg = new SelectBluetoothDeviceDialog();
            DialogResult result = dlg.ShowDialog(this);
            if (result != DialogResult.OK)
            {
                return;
            }
            device = dlg.SelectedDevice;
            //BluetoothAddress addr = device.DeviceAddress;
           // MessageBox.Show(addr.ToString());
            using (BluetoothWin32Authentication win = new BluetoothWin32Authentication(authhandle))
            {
                MessageBox.Show("Pairing");

                if (BluetoothSecurity.PairRequest(device.DeviceAddress, ""))
                {
                    //MessageBox.Show(BluetoothSecurity.GetPinRequest().ToString());
                    //BluetoothEndPoint endpoint = new BluetoothEndPoint(this.device.DeviceAddress, Guid.Parse("A027AEB0-B2CA-C028-6D38-C8DE060A9A18"));
                    //cli.Encrypt = true;
                    //cli.BeginConnect(endpoint, new AsyncCallback(callback), this.device);
                    MessageBox.Show("WIN!");
                    MessageBox.Show(cli.GetRemoteMachineName(device.DeviceAddress) + " - :-)");
                    ServiceRecord[] wat = device.GetServiceRecords(new Guid("A027AEB0-B2CA-C028-6D38-C8DE060A9A18")); // A027AEB0-B2CA-C028-6D38-C8DE060A9A18
                    foreach (ServiceRecord w in wat)
                    {
                        MessageBox.Show(ServiceRecordUtilities.Dump(w));
                    }//__cstring:0000000100030AE7 00000025 C 58FA956A-6FEE-47F3-AE89-C8D75FA687B0
                    BluetoothEndPoint endpoint = new BluetoothEndPoint(this.device.DeviceAddress, new Guid("A027AEB0-B2CA-C028-6D38-C8DE060A9A18"));
                    cli.Encrypt = true;
                    cli.Connect(endpoint);
                    button1.Text = "Connected to " + device.DeviceName;
                    button1.Enabled = false;
                    //Thread wt = new Thread(new ThreadStart(writeThread));
                    Thread rt = new Thread(new ThreadStart(readThread));
                    //wt.Start();

                    if (cli.GetStream().CanWrite)
                    {
                        writer = new StreamWriter(cli.GetStream(), Encoding.UTF8);
                        //{

                            //writer.WriteLine("{\"t\": \"fts\"}");
                            /*for (int i = 0; i < 500; i++)
                            {
                                writer.WriteLine("{\"t\": \"\"}");
                            
                            }*/
                        writer.AutoFlush = true;
                        //writer.WriteLine("{\"t\": \"ft\",\"id\": \"4771790\"}");
                        //writer.WriteLine("{\"t\": \"ft\",\"id\": \"1369618224\"}");
                            rt.Start();
                            //writer.WriteLine("{\"t\": \"fts\"}");
                            /*Thread.Sleep(5000);
                            for (int i = 0; i < 1000; i++)
                            {
                                writer.WriteLine("{\"t\": \"ft\",\"id\": \"1369618224\"}");
                            }*/
                            
                            //textBox1.Text = responseStream;
                        //}
                    }



                    /* //cli.GetStream().Write(bytes, 0, bytes.Length);
                     StreamWriter writer = new StreamWriter(cli.GetStream(), Encoding.UTF8);
                    
                     if (cli.GetStream().CanWrite)
                     {
                         //while (true)
                        // {
                         //for (int i = 0; i < 1000; i++)
                         //{
                            // writer.WriteLine("{\"t\": \"fts\"}");
                             //writer.WriteLine("{\"t\": \"sc\"}");
                            // writer.WriteLine("{\"t\": \"ft\"}");
                         //}
                        // }
                         for (int i = 0; i < 1000; i++)
                         {
                             writer.WriteLine("{\"t\": \"fts\"}");
                             //writer.WriteLine("{\"t\": \"sc\"}");
                         }
                         //cli.GetStream().Flush();
                         //cli.GetStream().Write(bytes, 0, bytes.Length);
                         //cli.GetStream().Flush();
                         MessageBox.Show("Written!");
                     }
                     //cli.GetStream().Flush();
                     String response = "";
                     using (StreamReader reader = new StreamReader(cli.GetStream(), Encoding.UTF8))
                     {
                         //MessageBox.Show("Reading...");

                         //writer.WriteLine("{\"t\": \"fts\"}");
                         char[] buffer = new char[1024];
                         int i = 0;
                         while ((i = reader.Read(buffer, 0, 1024)) != 1)
                         {
                             MessageBox.Show(i.ToString());
                             response += new String(buffer);
                         }


                     }
                     MessageBox.Show(response);
                     //MessageBox.Show("WIN!");
                 }*/
                    /*MessageBox.Show(Guid.Parse("A027AEB0-B2CA-C028-6D38-C8DE060A9A18").ToString());
                    ServiceRecord[] rcs = device.GetServiceRecords(Guid.Parse("A027AEB0-B2CA-C028-6D38-C8DE060A9A18"));
                    foreach (ServiceRecord r in rcs)
                    {
                        MessageBox.Show(ServiceRecordUtilities.Dump(r));
                    }
                    MessageBox.Show(cli.GetRemoteMachineName(device.DeviceAddress) + " - :-)");
                    byte[] bytes = GetBytes("{\"t\": \"ft\",\"id\":\"2\"}");

                    BluetoothEndPoint endpoint = new BluetoothEndPoint(this.device.DeviceAddress, Guid.Parse("A027AEB0-B2CA-C028-6D38-C8DE060A9A18"));
                    cli.Encrypt = true;
                    cli.Connect(endpoint);
                     cli.GetStream().Write(bytes, 0, bytes.Length);
                     cli.GetStream().Flush();
                     String response = "";
                     using (StreamReader reader = new StreamReader(cli.GetStream(), Encoding.UTF8))
                     {
                         response = reader.ReadToEnd();
                     }
                     MessageBox.Show(response);
                } else {
                    MessageBox.Show("Awww...");
                }*/
                }
            }
            /*bool isPaired = BluetoothSecurity.PairRequest(device.DeviceAddress, null);
            if (isPaired)
            {
                // now it is paired
                MessageBox.Show("Paired Successfully!");
            }
            else
            {
                // pairing failed
                MessageBox.Show("Pairing Failed!");
            }*/
            //cli.BeginConnect(endpoint, new AsyncCallback(callback), this.device);
            //MessageBox.Show("WIN!");
            //MessageBox.Show(cli.GetRemoteMachineName(device.DeviceAddress) + " - :-)");
            //byte[] bytes = GetBytes("{\"t\": \"ft\",\"id\":\"2\"}");
           //cli.Connect(endpoint);
           // cli.GetStream().Write(bytes, 0, bytes.Length);
           // String response = "";
           // using (StreamReader reader = new StreamReader(cli.GetStream(), Encoding.UTF8))
           // {
           //     response = reader.ReadToEnd();
           // }
           // MessageBox.Show(response);
            //device.GetVersions();
            //ServiceRecord[] gg = device.GetServiceRecords(new Guid("{A027AEB0-B2CA-C028-6D38-C8DE060A9A18}"));
            //MessageBox.Show(gg.Length.ToString());
        }

        private void authhandle(Object e, BluetoothWin32AuthenticationEventArgs args)
        {
            MessageBox.Show("Event Fired!");
            args.Confirm = true;
        }

        private void readThread()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            using (StreamReader reader = new StreamReader(cli.GetStream(), Encoding.UTF8))
            {
                //MessageBox.Show("Reading...");

                //writer.WriteLine("{\"t\": \"fts\"}");
                char[] buffer = new char[1536];
                int i = 0;
                while ((i = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //MessageBox.Show(i.ToString());
                    //responseStream += new String(buffer);
                    textBox1.AppendText(new String(buffer));
                    //MessageBox.Show(responseStream);
                    //buffer = new char[1];
                    buffer = new char[1536];
                }
            }
        }

        
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            writer.WriteLine(textBox2.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
