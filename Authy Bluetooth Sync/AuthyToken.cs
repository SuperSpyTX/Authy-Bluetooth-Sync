using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Authy_Bluetooth_Sync
{
    class AuthyToken
    {
        [JsonProperty]
        public String n { get; set; }

        [JsonProperty]
        public String at { get; set; }

        [JsonProperty]
        public String aid { get; set; }

        /*public AuthyToken(String name, String id, String icon)
        {
            this.name = name;
            this.id = id;
            this.icon = icon;
        }

        public AuthyToken(JToken token)
        {
            this.name = token["n"].ToString();
            this.id = token["aid"].ToString();
            this.icon = token["at"].ToString();
        }*/

        public String GetName()
        {
            return this.n;
        }

        public String GetId()
        {
            return this.aid;
        }

        public String GetIcon()
        {
            return this.at;
        }

        public void InvalidateIcon()
        {
            this.at = "authenticator";
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
