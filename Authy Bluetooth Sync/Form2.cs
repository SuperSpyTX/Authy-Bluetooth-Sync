using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using InTheHand.Net;

namespace Authy_Bluetooth_Sync
{
    public partial class Form2 : Form
    {
        private AuthyAsyncLib asynclib;
        private ThirtyTwoFeetBluetooth btImpl;

        private bool newDevice = true;
        private Dictionary<string, AuthyToken> tokens = new Dictionary<string, AuthyToken>();

        private JObject manifest;

        System.Windows.Forms.ContextMenuStrip contextMenu1 = new System.Windows.Forms.ContextMenuStrip();

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            label2.Hide();
            try
            {
                btImpl = new ThirtyTwoFeetBluetooth(AuthyAsyncLib.AUTHY_ANDROID_GUID, null);
            } 
            catch (Exception)
            {
                MessageBox.Show("Bluetooth is either not supported on your computer or it is turned off.", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(-1);
            }
            Task t = Task.Factory.StartNew(delegate()
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        manifest = JObject.Parse(wc.DownloadString("https://api.authy.com/assets/android/low"));
                    }
                    catch (Exception)
                    {
                        manifest = null;
                    }
                }
            });

            if (File.Exists(Environment.CurrentDirectory + "\\authy_sync.config"))
            {
                newDevice = false;
                button1_Click(sender, e);
            }
        }

        private void exitClick(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void updateTokens(String updateHash, List<object> tokens, BluetoothAddress address, bool performAllUpdates)
        {
            foreach (Object tok in tokens)
            {
                AuthyToken atoken = JsonConvert.DeserializeObject<AuthyToken>(tok.ToString());
                this.tokens.Add(atoken.GetName(), atoken);

                Action<object, EventArgs> click = delegate(object el, EventArgs f)
                {
                    //MessageBox.Show(el.ToString());
                    AuthyToken token = this.tokens[el.ToString()];
                    asynclib.FetchToken(token, delegate(String code)
                    {
                        //MessageBox.Show("That code is: " + code + " - which has been co);
                        Action set = delegate()
                        {
                            try
                            {
                                Clipboard.SetText(code);
                                notifyIcon1.ShowBalloonTip(5000, code + ": Copied to clipboard!", "The one-time code has been copied to your clipboard!", ToolTipIcon.None);

                                Thread.Sleep(20000);
                                if (Clipboard.ContainsText() && Clipboard.GetText().Equals(code))
                                {
                                    Clipboard.Clear();
                                    notifyIcon1.ShowBalloonTip(5000, "Your one-time code has expired!", "The one-time code was sitting in your clipboard for too long and has been erased.", ToolTipIcon.None);
                                }
                            }
                            catch (Exception)
                            {
                                notifyIcon1.ShowBalloonTip(5000, "Error! Failed to copy to clipboard!", "The one-time code has failed to copy to your clipboard!", ToolTipIcon.Error);
                            }
                        };
                        Thread nt = new Thread(new ThreadStart(set));
                        nt.TrySetApartmentState(ApartmentState.STA);
                        nt.Start();


                    });
                };

                // Get OTP icons (idfk)

                Image img = null;

                if (manifest != null && manifest["urls"][atoken.GetIcon()] == null)
                {
                    atoken.InvalidateIcon();
                }

                if (!Directory.Exists(Environment.CurrentDirectory + "\\assets"))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\assets");
                }

                if (System.IO.File.Exists(Environment.CurrentDirectory + "\\assets\\" + atoken.GetIcon() + ".png"))
                {
                    if (manifest != null)
                    {
                        String offlineHash = "", onlineHash = "";

                        // Stack overflow - http://stackoverflow.com/questions/10520048/calculate-md5-checksum-for-a-file
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(Environment.CurrentDirectory + "\\assets\\" + atoken.GetIcon() + ".png"))
                            {
                                offlineHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                            }
                        }

                        onlineHash = manifest["urls"][atoken.GetIcon()]["menu_item_url"].ToString();

                        if (onlineHash != null && !onlineHash.Equals(offlineHash))
                        {
                            using (WebClient wc = new WebClient())
                            {
                                if (manifest != null)
                                {
                                    byte[] data = wc.DownloadData(manifest["urls"][atoken.GetIcon()]["menu_item_url"].ToString());
                                    File.WriteAllBytes(Environment.CurrentDirectory + "\\assets\\" + atoken.GetIcon() + ".png", data);
                                    using (var ms = new MemoryStream(data))
                                    {
                                        img = Image.FromStream(ms);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        img = Image.FromFile(Environment.CurrentDirectory + "\\assets\\" + atoken.GetIcon() + ".png");
                    }
                }
                else
                {
                    using (WebClient wc = new WebClient())
                    {
                        if (manifest != null)
                        {
                            byte[] data = wc.DownloadData(manifest["urls"][atoken.GetIcon()]["menu_item_url"].ToString());
                            File.WriteAllBytes(Environment.CurrentDirectory + "\\assets\\" + atoken.GetIcon() + ".png", data);
                            using (var ms = new MemoryStream(data))
                            {
                                img = Image.FromStream(ms);
                            }
                        }
                    }
                }

                this.contextMenu1.Items.Add(atoken.GetName(), img, new EventHandler(click));
            }

            contextMenu1.Items.Add(new ToolStripSeparator());
            contextMenu1.Items.Add("Exit", null, new System.EventHandler(exitClick));
            this.notifyIcon1.ContextMenuStrip = contextMenu1;
            this.Update();

            if (performAllUpdates)
            {
                Action configSaver = delegate()
                {
                    Config config = new Config();
                    config.DeviceType = "0"; // TODO: if iOS Support is added, this is 1.
                    config.BluetoothAddress = address.ToString();
                    config.Hash = updateHash;
                    config.Tokens = this.tokens.Values.ToList();

                    String configSave = JsonConvert.SerializeObject(config, Formatting.Indented);
                    System.IO.File.WriteAllText(Environment.CurrentDirectory + "\\" + "authy_sync.config", configSave);
                };

                Task t = Task.Factory.StartNew(configSaver);
            }

            this.Hide();
            notifyIcon1.ShowBalloonTip(5000, "Connected!", "You may now start using the application!", ToolTipIcon.None);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (newDevice)
            {
                BluetoothDeviceInfo device = btImpl.DiscoverDevices(this);

                if (device != null)
                {
                    label1.Hide();
                    button1.Hide();
                    label2.Show();
                    label2.Text = "Pairing device....";
                    this.Update();
                    if (btImpl.PairDevice(device))
                    {
                        label2.Text = "Connecting....";
                        this.Update();

                        asynclib = new AuthyAsyncLib(btImpl);
                        asynclib.Initialize();

                        asynclib.FetchAllTokens(delegate(JObject joj)
                        {
                            List<JToken> obj = joj["tks"].ToList();
                            List<object> obj2 = new List<object>();

                            foreach (JToken tok in obj)
                            {
                                obj2.Add(tok);
                            }

                            updateTokens(joj["s"].ToString(), obj2, device.DeviceAddress, true);
                        });


                        label2.Text = "Performing async operations...";
                        this.Update();
                    }
                    else
                    {
                        MessageBox.Show("Pairing failed!");
                        label1.Show();
                        button1.Show();
                        label2.Hide();
                    }

                }
            }
            else
            {
                try
                {
                    label1.Hide();
                    button1.Hide();
                    label2.Show();
                    label2.Text = "Loading configuration....";
                    this.Update();

                    Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Environment.CurrentDirectory + "\\authy_sync.config"));
                    BluetoothAddress addr = BluetoothAddress.Parse(config.BluetoothAddress);

                    if (config.DeviceType.Equals("0"))
                    {
                        btImpl = new ThirtyTwoFeetBluetooth(AuthyAsyncLib.AUTHY_ANDROID_GUID, addr);
                        asynclib = new AuthyAsyncLib(btImpl);
                    }
                    else
                    {
                        throw new Exception("Invalid device type!");
                    }


                    if (!btImpl.PairDevice(addr))
                    {
                        MessageBox.Show("Could not connect to remote device", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1);
                    }

                    //try
                    {
                        asynclib.Initialize();
                    }
                    /*catch (Exception)
                    {
                        MessageBox.Show("The remote device isn't running Authy for Android.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1);
                    }*/

                    asynclib.FetchHash(delegate(String hash)
                    {
                        if (config.Hash.Equals(hash))
                        {
                            List<AuthyToken> obj = config.Tokens;
                            List<object> obj2 = new List<object>();

                            foreach (AuthyToken tok in obj)
                            {
                                obj2.Add(tok.ToJson());
                            }

                            updateTokens(hash, obj2, addr, false);
                        }
                        else
                        {
                            asynclib.FetchAllTokens(delegate(JObject joj)
                            {
                                List<JToken> obj = joj["tks"].ToList();
                                List<object> obj2 = new List<object>();

                                foreach (JToken tok in obj)
                                {
                                    obj2.Add(tok);
                                }

                                updateTokens(joj["s"].ToString(), obj2, addr, true);
                            });
                        }
                    });

                    label2.Text = "Performing async operations...";
                    this.Update();
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid configuration file.  Please delete authy_sync.config in the same directory as this application.");
                    Environment.Exit(0);
                }
            }
        }
    }
}
