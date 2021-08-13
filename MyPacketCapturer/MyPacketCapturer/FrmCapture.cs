using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PacketDotNet;
using SharpPcap;

namespace MyPacketCapturer
{
    public partial class FrmCapture : Form
    {

        CaptureDeviceList devices; // List of devices for this computer
        public static ICaptureDevice device; // The device we will be using
        public static string stringPackets = ""; //Data that is captured
        static int numPackets = 0;
        FrmSend fSend; //This wiil be our send from
        ProtocolRead readP; //protocol from

        public FrmCapture()
        {
            InitializeComponent();
            //get list of devices
            devices = CaptureDeviceList.Instance;

            //makes sure there is at least 1 device
            if (devices.Count < 1)
            {
                MessageBox.Show("No capture devices found!?!");
                Application.Exit();
            }

            //add the devices to the combo box
            foreach(ICaptureDevice dev in devices)
            {
                cmbDevices.Items.Add(dev.Description);
            }

            //get the second device and display in the combo box
            device = devices[1];
            cmbDevices.Text = device.Description;


            //register our handler function to the "packet arrival' event
            device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);

            //open the device for capturing
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

        }

        private static void device_OnPacketArrival(object sender, CaptureEventArgs packet)
        {
            /*//Increment the num of packets
            //numPackets++;            

            //array to store our data
            byte[] data = packet.Packet.Data;

            if (data[23].ToString("X2") == "06" || data[23].ToString("X2") == "11")
            {
                //numPackets++;
                stringPackets += "Packet Number: " + Convert.ToString(numPackets);
                stringPackets += Environment.NewLine;

                //keep track of the number of bytes displayed per line
                int byteCounter = 0;
                string source = "0x";
                string dest = "0x";
                int x;

                //if(data[23].ToString("X2") == "06")
                //stringPackets += data[23].ToString("X2") + Environment.NewLine;
                foreach (byte b in data)
                {

                    switch (byteCounter)
                    {
                        case 34:
                            stringPackets += "Source Port: ";
                            source += b.ToString("X2");
                            break;
                        case 35:
                            source += b.ToString("X2");
                            //source = stringPackets;
                            x = Convert.ToInt32(source, 16);
                            //s = x.ToString() + Environment.NewLine;
                            //addSource(x.ToString());
                            stringPackets += x.ToString();
                            stringPackets += Environment.NewLine;
                            break;
                        case 36:
                            stringPackets += "Destination Port: ";
                            dest += b.ToString("X2");
                            break;
                        case 37:
                            dest += b.ToString("X2");
                            x = Convert.ToInt32(dest, 16);
                            //d = x.ToString();
                            //destTotal++;
                            //addDest(x.ToString());
                            stringPackets += x.ToString();
                            stringPackets += Environment.NewLine;
                            break;

                    }

                    byteCounter++;
                    //stringPackets += Environment.NewLine;
                }
                stringPackets += Environment.NewLine;
                stringPackets += Environment.NewLine;
                numPackets++;*/
                //Increment the num of packets
                numPackets++;

                //put the packet num in the capture window
                stringPackets += "Packet Number: " + Convert.ToString(numPackets);
                stringPackets += Environment.NewLine;

                //array to store our data
                byte[] data = packet.Packet.Data;

                //keep track of the number of bytes displayed per line
                int byteCounter = 0;

                stringPackets += "Destination MAC Address: ";
                //parsing the packets 
                foreach (byte b in data)
                {
                    //add the byte to our string (in hex)
                    if(byteCounter<=13) stringPackets += b.ToString("X2") + " ";
                    byteCounter++;

                   switch (byteCounter)
                    {
                        case 6: stringPackets += Environment.NewLine;
                            stringPackets += "Source MAC Address: ";
                            break;
                        case 12: stringPackets += Environment.NewLine;
                            stringPackets += "EtherType: ";
                            break;
                        case 14: if(data[12] == 8)
                            {
                                if (data[13] == 0) stringPackets += "(IP)";
                                if (data[13] == 6) stringPackets += "(ARP)";
                            }
                            break;
                    }
                }

                stringPackets +=Environment.NewLine + Environment.NewLine;
                byteCounter = 0;
                stringPackets += "Raw Data" + Environment.NewLine;

                //process each byte in our captured packet
                foreach (byte b in data)
                {
                    //add the byte to our string (in hex)
                    stringPackets += b.ToString("X2") + " ";
                    byteCounter++;

                    if(byteCounter == 16)
                    {
                        byteCounter = 0;
                        stringPackets += Environment.NewLine;
                    }
                }
                stringPackets += Environment.NewLine;
                stringPackets += Environment.NewLine;
            
            }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if(btnStartStop.Text == "Start")
                {
                    device.StartCapture();
                    timer1.Enabled = true;
                    btnStartStop.Text = "Stop";
                }
                else
                {
                    device.StopCapture();
                    timer1.Enabled = false;
                    btnStartStop.Text = "Start";
                }
            }
            catch(Exception exp)
            {

            }
        }

        //dumps the packet data from string to the text box
        private void timer1_Tick(object sender, EventArgs e)
        {
            txtCapturedData.AppendText(stringPackets);
            stringPackets = "";
            txtNumPackets.Text = Convert.ToString(numPackets);
        }

        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            device = devices[cmbDevices.SelectedIndex];
            cmbDevices.Text = device.Description;
            txtGUID.Text = device.Name;

            //register our handler function to the "packet arrival' event
            device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);

            //open the device for capturing
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            saveFileDialog1.Title = "Save the Captured Packets";
            saveFileDialog1.ShowDialog();

            //check to see if a filename was given
            if(saveFileDialog1.FileName != "")
            {
                System.IO.File.WriteAllText(saveFileDialog1.FileName, txtCapturedData.Text);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            openFileDialog1.Title = "Save the Captured Packets";
            openFileDialog1.ShowDialog();

            //check to see if a filename was given
            if (openFileDialog1.FileName != "")
            {
                txtCapturedData.Text = System.IO.File.ReadAllText(openFileDialog1.FileName);
            }
        }

        private void sendWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FrmSend.instantiations == 0)
            {
                fSend = new FrmSend(); //Creates a new frmSend
                fSend.Show();
            }
        }

        private void readProtocolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProtocolRead.instantiations == 0)
            {
                readP = new ProtocolRead(); //Creates a new readp
                readP.Show();
            }
        }
    }
}
