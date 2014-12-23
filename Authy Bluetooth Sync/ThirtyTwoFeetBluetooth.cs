using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Windows.Forms;
using System.Windows.Forms;
using InTheHand.Net;

namespace Authy_Bluetooth_Sync
{
    class ThirtyTwoFeetBluetooth : BluetoothInterface
    {
        private Guid serviceGuid;
        private BluetoothClient client;
        private BluetoothAddress address;

        /*public ThirtyTwoFeetBluetooth(Guid serviceGuid, BluetoothDeviceInfo device)
        {
            this.serviceGuid = serviceGuid;
            this.address = device.DeviceAddress;
            this.client = new BluetoothClient();
        }*/

        public ThirtyTwoFeetBluetooth(Guid serviceGuid, BluetoothAddress address)
        {
            this.serviceGuid = serviceGuid;
            this.address = address;
            this.client = new BluetoothClient();
        }

        public BluetoothDeviceInfo DiscoverDevices(Form form)
        {
            var dlg = new SelectBluetoothDeviceDialog();
            DialogResult result = dlg.ShowDialog(form);
            if (result != DialogResult.OK)
            {
                return null;
            }

            return dlg.SelectedDevice;
        }

        public bool PairDevice(BluetoothDeviceInfo device)
        {
            using (BluetoothWin32Authentication win = new BluetoothWin32Authentication(authhandle))
            {
                if (BluetoothSecurity.PairRequest(device.DeviceAddress, ""))
                {
                    this.address = device.DeviceAddress;
                    return true;
                }
            }

            return false;
        }

        public bool PairDevice(BluetoothAddress address)
        {
            using (BluetoothWin32Authentication win = new BluetoothWin32Authentication(authhandle))
            {
                if (BluetoothSecurity.PairRequest(address, ""))
                {
                    this.address = address;
                    return true;
                }
            }

            return false;
        }

        private void authhandle(Object e, BluetoothWin32AuthenticationEventArgs args)
        {
            args.Confirm = true;
        }

        public void Connect()
        {
            BluetoothEndPoint endpoint = new BluetoothEndPoint(this.address, this.serviceGuid);
            client.Encrypt = true;
            client.Connect(endpoint);
        }

        public Stream GetStream()
        {
            return client.GetStream();
        }
    }
}
