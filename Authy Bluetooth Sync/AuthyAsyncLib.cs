using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Authy_Bluetooth_Sync
{
    class AuthyAsyncLib
    {
        public static Guid AUTHY_ANDROID_GUID = new Guid("A027AEB0-B2CA-C028-6D38-C8DE060A9A18");

        private Object TASK_LOCK = new Object();
        private Object PACKET_LOCK = new Object();
        private BlockingCollection<Task> taskQueue = new BlockingCollection<Task>();
        private Thread taskThreadObj;
        private Stopwatch stopwatch;
        private long lastPacket = 0;
        private String stream;
        private BluetoothInterface client;
        private StreamWriter writer;

        public AuthyAsyncLib(BluetoothInterface client)
        {
            this.client = client;
        }

        public void Initialize()
        {
            this.client.Connect();
            this.stream = "";
            writer = new StreamWriter(this.client.GetStream(), Encoding.UTF8);
            writer.AutoFlush = true;
            Thread mt = new Thread(new ThreadStart(monitorThread));
            Thread rt = new Thread(new ThreadStart(readThread));
            stopwatch = new Stopwatch();
            stopwatch.Start();
            mt.Start();
            rt.Start();
        }

        public void SendToDevice(String data, Action<String> callback)
        {
           // Task t = Task.Factory.StartNew (() => ServerSentPacket());
           // t.Wait();
            Action streamCallback = delegate()
            {
                try
                {
                    this.writer.WriteLine(data);
                }
                catch (Exception)
                {
                    MessageBox.Show("The application has lost connection with the device", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }

                lastPacket = stopwatch.ElapsedMilliseconds;

                /*while (true)
                {
                    lock (TASK_LOCK)
                    {
                        Monitor.Wait(TASK_LOCK);
                    }
                    try
                    {
                        JObject.Parse(stream);

                        Task t = Task.Factory.StartNew(() => callback(GetResponseFromDevice()));
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }*/
                lock (TASK_LOCK)
                {
                    Monitor.Wait(TASK_LOCK);
                }
                Task t = Task.Factory.StartNew(() => callback(GetResponseFromDevice()));
                //break;
            };
            Task t6 = new Task(streamCallback);
            taskQueue.Add(t6);

            if (taskThreadObj == null || taskThreadObj.IsAlive == false)
            {
                taskThreadObj = new Thread(new ThreadStart(taskThread));
                taskThreadObj.Start();
            }
        }

        private void taskThread()
        {
            while (taskQueue.Count > 0)
            {
                foreach (Task task in taskQueue)
                {
                    task.Start();
                    task.Wait();
                    taskQueue.Take();
                }
            }
        }

        public void FetchHash(Action<String> callback)
        {
            SendToDevice(new JObject(new JProperty("t", "sc")).ToString(), delegate(String result)
            {
                JObject joj = JObject.Parse(result);
                callback(joj["s"].ToString());
            });
        }

        public void FetchAllTokens(Action<JObject> callback)
        {
            SendToDevice(new JObject(new JProperty("t", "fts")).ToString(), delegate(String result)
            {
                JObject joj = JObject.Parse(result);
                callback(joj);
            });
        }

        public void FetchToken(AuthyToken token, Action<String> callback)
        {
            SendToDevice(new JObject(new JProperty("t", "ft"),new JProperty("id", token.GetId())).ToString(), delegate(String result)
            {
                JObject joj = JObject.Parse(result);
                //callback(joj);
                callback(joj["v"].ToString());
            });
        }

        public String GetResponseFromDevice()
        {
            String receive = stream;
            this.stream = "";
            return receive;
        }

        public bool IsWaiting()
        {
            return stopwatch.ElapsedMilliseconds - lastPacket > 1000 && stream.Length > 0;
        }

        private void monitorThread()
        {
            while (true)
            {
                lock (PACKET_LOCK)
                {
                    // If we sit here, it's an OBAMA GRIDLOCK!
                    Monitor.Pulse(PACKET_LOCK);
                }
                if (IsWaiting())
                {
                    lock (TASK_LOCK)
                    {
                        Monitor.Pulse(TASK_LOCK);
                    }
                }
            }
        }


        private void readThread()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            lastPacket = stopwatch.ElapsedMilliseconds;

            using (StreamReader reader = new StreamReader(this.client.GetStream()))
            {
                char[] buffer = new char[1536];
                int i = 0;
                while ((i = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    lastPacket = stopwatch.ElapsedMilliseconds;
                    for (int i2 = 0; i2 < 1; i2++)
                    {
                        String newStr = new String(buffer).Trim(new char[] { '\0','\n','\r','\t' }).Trim();
                        stream += newStr;
                    }
                    buffer = new char[1536];

                    lock (PACKET_LOCK)
                    {
                        Monitor.Pulse(PACKET_LOCK);
                    }
                }
            }
        }
    }
}
