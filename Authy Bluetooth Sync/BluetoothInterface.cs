using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Authy_Bluetooth_Sync
{
    public interface BluetoothInterface
    {
        void Connect();

        Stream GetStream();
    }
}
