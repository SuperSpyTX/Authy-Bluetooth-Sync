using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Authy_Bluetooth_Sync
{
    [JsonObject(MemberSerialization.OptIn)]
    class Config
    {
        [JsonProperty]
        public String BluetoothAddress { get; set; }

        [JsonProperty]
        public String DeviceType { get; set; }

        [JsonProperty]
        public String Hash { get; set; }

        [JsonProperty]
        public List<AuthyToken> Tokens { get; set; }
    }
}
