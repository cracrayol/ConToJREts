using HidLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ConToJREts {
    delegate void MyDelegate();
    public class Form1 : Form {
        private bool IsDisposing = true;
        private int port;
        private SerialPort myPort;
        private string lastWord = string.Empty;
        private int notchnow;
        private int notchbefor;
        private int amount;
        private int reb = 1;
        private int rebbefor;
        private bool bottonA;
        private bool bottonB;
        private bool bottonC;
        private bool bottonS;
        private bool bottonAbefor;
        private bool bottonBbefor;
        private bool bottonCbefor;
        private bool bottonSbefor;
        private int VendorId = 4660;
        private int ProductId = 4660;
        private HidDevice _device;
        private byte[] Zuiki_readBuffer = new byte[8];
        private byte B9;
        private byte B8 = 5;
        private byte B7 = 19;
        private byte B6 = 32;
        private byte B5 = 46;
        private byte B4 = 60;
        private byte B3 = 73;
        private byte B2 = 87;
        private byte B1 = 101;
        private byte P0 = 128;
        private byte P1 = 159;
        private byte P2 = 183;
        private byte P3 = 206;
        private byte P4 = 230;
        private byte P5 = byte.MaxValue;
        private byte Zuiki_rebbefor;
        private int bottonnum1befor;
        private int bottonnum10befor;
        private int bottonnum100befor;
        private int bottonnum1000befor;
        private int bottonnum10000befor;
        private int bottonnum100000befor;
        private int bottonnum1000000befor;
        private int bottonnum10000000befor;
        private int bottonnum2_1befor;
        private int bottonnum2_10befor;
        private int bottonnum2_100befor;
        private int bottonnum2_1000befor;
        private Button connectBtn;
        private Button disconnectBtn;
        private Button close;
        private Label label1;
        private Label label2;
        private Label label3;
        private ComboBox portList;
        private ComboBox controllerlist;
        private CheckBox buttoncheck;
        private CheckBox arrowcheck;
        private Dictionary<string, string> keyMapping;
        private IniFile iniFile;

        public Form1() => this.InitializeComponent();

        private void Form1_Load(object sender, EventArgs e) {
            this.label1.Text = "Disconnected";
            for (int index = 1; index <= 256; ++index)
                this.portList.Items.Add((object)("COM" + index.ToString()));
            this.controllerlist.Items.Add((object)"Not selected");
            this.controllerlist.Items.Add((object)"TS MasCon 2");
            this.controllerlist.Items.Add((object)"ZKNS-001");
            this.controllerlist.SelectedIndex = 0;
            this.controllerlist.Enabled = true;
            this.portList.Visible = false;
            this.iniFile = new IniFile("key.ini");
        }

        private void controllerlist_SelectedIndexChanged(object sender, EventArgs e) {
            if (this.controllerlist.SelectedIndex == 0) {
                this.label2.Text = "";
                this.portList.Visible = false;
                this.buttoncheck.Visible = false;
                this.arrowcheck.Visible = false;
            }
            if (this.controllerlist.SelectedIndex == 1) {
                this.label2.Text = "Port : ";
                this.portList.Visible = true;
                this.buttoncheck.Visible = false;
                this.arrowcheck.Visible = false;
            }
            if (this.controllerlist.SelectedIndex == 2) {
                this.label2.Text = "";
                this.portList.Visible = false;
                this.buttoncheck.Visible = true;
                this.arrowcheck.Visible = true;
                this.arrowcheck.Checked = this.iniFile.Read("DISABLE_DPAD", "ZUIKI") == "1";
                this.buttoncheck.Checked = this.iniFile.Read("DISABLE_BUTTONS", "ZUIKI") == "1";
            }
        }

        private void connectBtn_Click(object sender, EventArgs e) {
            if (this.controllerlist.SelectedIndex == 1)
                this.TSmc2_openPort();
            if (this.controllerlist.SelectedIndex == 2)
                this.Zuiki_open();
        }

        private void disconnectBtn_Click(object sender, EventArgs e) {
            this.TSmc2_closePort();
            this.Zuiki_close();
            this.disconnectBtn.Enabled = false;
            this.connectBtn.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            this.TSmc2_closePort();
            this.Zuiki_close();
            this.Dispose();
        }

        private void close_Click(object sender, EventArgs e) {
            this.TSmc2_closePort();
            this.Zuiki_close();
            this.Dispose();
        }

        private void TSmc2_openPort() {
            if (this.portList.SelectedIndex >= 0)
                this.port = this.portList.SelectedIndex + 1;
            try {
                this.myPort = new SerialPort("COM" + this.port.ToString(), 19200, Parity.None, 8, StopBits.One);
                this.myPort.Open();
            } catch (Exception ex) {
                int num = (int)MessageBox.Show(ex.Message, "Universal Contact Interface", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.TSmc2_closePort();
            }
            if (this.myPort == null)
                return;

            this.keyMapping = new Dictionary<string, string>();
            this.keyMapping.Add("A", this.iniFile.Read("A", "MASCON2"));
            this.keyMapping.Add("B", this.iniFile.Read("B", "MASCON2"));
            this.keyMapping.Add("C", this.iniFile.Read("C", "MASCON2"));
            this.keyMapping.Add("S", this.iniFile.Read("S", "MASCON2"));

            this.label1.Text = "Connected";
            this.IsDisposing = false;
            this.controllerlist.Enabled = false;
            this.portList.Enabled = false;
            this.disconnectBtn.Enabled = true;
            this.connectBtn.Enabled = false;
            new Thread((ThreadStart)(() => {
                while (!this.IsDisposing)
                    this.TSmc2Loop();
            })).Start();
        }

        private void TSmc2_closePort() {
            if (this.myPort == null)
                return;
            this.IsDisposing = true;
            try {
                this.myPort.Close();
            } catch {
            }
            try {
                this.myPort.Dispose();
            } catch {
            }
            this.myPort = (SerialPort)null;
            this.label1.Text = "Disconnected";
            this.controllerlist.Enabled = true;
            this.portList.Enabled = true;
        }

        private void TSmc2Loop() {
            if (this.myPort == null)
                return;

            string str;
            try {
                str = this.myPort.ReadExisting();
            } catch (Exception ex) {
                int num = (int)MessageBox.Show(ex.Message, "Universal Contact Interface", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.TSmc2_closePort();
                return;
            }

            if (str.Length == 0)
                return;

            string[] strArray = (this.lastWord + str).Split('\r');
            for (int index = 0; index < strArray.Length - 1; ++index) {
                if (strArray[index] == "TSB20")
                    this.notchnow = -9;
                if (strArray[index] == "TSB30")
                    this.notchnow = -8;
                if (strArray[index] == "TSB40")
                    this.notchnow = -7;
                if (strArray[index] == "TSE99")
                    this.notchnow = -6;
                if (strArray[index] == "TSA05")
                    this.notchnow = -5;
                if (strArray[index] == "TSA15")
                    this.notchnow = -4;
                if (strArray[index] == "TSA25")
                    this.notchnow = -3;
                if (strArray[index] == "TSA35")
                    this.notchnow = -2;
                if (strArray[index] == "TSA45")
                    this.notchnow = -1;
                if (strArray[index] == "TSA50")
                    this.notchnow = 0;
                if (strArray[index] == "TSA55")
                    this.notchnow = 1;
                if (strArray[index] == "TSA65")
                    this.notchnow = 2;
                if (strArray[index] == "TSA75")
                    this.notchnow = 3;
                if (strArray[index] == "TSA85")
                    this.notchnow = 4;
                if (strArray[index] == "TSA95")
                    this.notchnow = 5;
                if (this.notchnow != this.notchbefor) {
                    this.amount = this.notchnow == -9 || this.notchnow == 5 ? (this.notchbefor - this.notchnow) * 20 : this.notchbefor - this.notchnow;
                    INPUT pInputs = new INPUT() {
                        type = 0,
                        ui = new INPUT_UNION() {
                            mouse = new MOUSEINPUT() {
                                dwFlags = 2048,
                                mouseData = this.amount * 120,
                                dwExtraInfo = IntPtr.Zero,
                                time = 0
                            }
                        }
                    };
                    Form1.NativeMethods.SendInput(1, ref pInputs, Marshal.SizeOf<INPUT>(pInputs));
                }
                this.notchbefor = this.notchnow;
                if (strArray[index] == "TSG99")
                    this.reb = 1;
                if (strArray[index] == "TSG50")
                    this.reb = 0;
                if (strArray[index] == "TSG00")
                    this.reb = -1;
                if (this.reb > this.rebbefor)
                    SendKeys.SendWait("{UP}");
                if (this.reb < this.rebbefor)
                    SendKeys.SendWait("{DOWN}");
                this.rebbefor = this.reb;
                if (strArray[index] == "TSK99")
                    this.bottonS = true;
                if (strArray[index] == "TSK00")
                    this.bottonS = false;
                if (this.bottonSbefor != this.bottonS) {
                    if (this.bottonS)
                        SendKeys.SendWait(this.keyMapping["S"]);
                    this.bottonSbefor = this.bottonS;
                }
                if (strArray[index] == "TSX99")
                    this.bottonA = true;
                if (strArray[index] == "TSX00")
                    this.bottonA = false;
                if (this.bottonAbefor != this.bottonA) {
                    if (this.bottonA)
                        SendKeys.SendWait(this.keyMapping["A"]);
                    this.bottonAbefor = this.bottonA;
                }
                if (strArray[index] == "TSY99")
                    this.bottonB = true;
                if (strArray[index] == "TSY00")
                    this.bottonB = false;
                if (this.bottonBbefor != this.bottonB) {
                    if (this.bottonB)
                        SendKeys.SendWait(this.keyMapping["B"]);
                    this.bottonBbefor = this.bottonB;
                }
                if (strArray[index] == "TSZ99")
                    this.bottonC = true;
                if (strArray[index] == "TSZ00")
                    this.bottonC = false;
                if (this.bottonCbefor != this.bottonC) {
                    if (this.bottonC)
                        SendKeys.SendWait(this.keyMapping["C"]);
                    this.bottonCbefor = this.bottonC;
                }
            }
            this.lastWord = strArray[strArray.Length - 1];
        }

        private void Zuiki_open() {
            if (this._device == null) {
                this.VendorId = 3853;
                this.ProductId = 193;
                this._device = HidDevices.Enumerate(this.VendorId, this.ProductId).FirstOrDefault<HidDevice>();
            }
            if (this._device == null) {
                this.VendorId = 13277;
                this.ProductId = 1;
                this._device = HidDevices.Enumerate(this.VendorId, this.ProductId).FirstOrDefault<HidDevice>();
            }
            if (this._device == null) {
                this.VendorId = 13277;
                this.ProductId = 2;
                this._device = HidDevices.Enumerate(this.VendorId, this.ProductId).FirstOrDefault<HidDevice>();
            }
            if (this._device != null) {
                this._device.OpenDevice();
                this._device.MonitorDeviceEvents = true;
                this.IsDisposing = false;
                this._device.ReadReport(new ReadReportCallback(this.OnReport));
                this.label1.Text = "Connected";
                this.controllerlist.Enabled = false;
                this.portList.Enabled = false;
                this.disconnectBtn.Enabled = true;
                this.connectBtn.Enabled = false;

                this.keyMapping = new Dictionary<string, string>();
                this.keyMapping.Add("UP", this.iniFile.Read("UP", "ZUIKI"));
                this.keyMapping.Add("DOWN", this.iniFile.Read("DOWN", "ZUIKI"));
                this.keyMapping.Add("LEFT", this.iniFile.Read("LEFT", "ZUIKI"));
                this.keyMapping.Add("RIGHT", this.iniFile.Read("RIGHT", "ZUIKI"));
                this.keyMapping.Add("A", this.iniFile.Read("A", "ZUIKI"));
                this.keyMapping.Add("B", this.iniFile.Read("B", "ZUIKI"));
                this.keyMapping.Add("X", this.iniFile.Read("X", "ZUIKI"));
                this.keyMapping.Add("Y", this.iniFile.Read("Y", "ZUIKI"));
                this.keyMapping.Add("L", this.iniFile.Read("L", "ZUIKI"));
                this.keyMapping.Add("R", this.iniFile.Read("R", "ZUIKI"));
                this.keyMapping.Add("ZL", this.iniFile.Read("ZL", "ZUIKI"));
                this.keyMapping.Add("ZR", this.iniFile.Read("ZR", "ZUIKI"));
                this.keyMapping.Add("PLUS", this.iniFile.Read("PLUS", "ZUIKI"));
                this.keyMapping.Add("MINUS", this.iniFile.Read("MINUS", "ZUIKI"));
                this.keyMapping.Add("HOME", this.iniFile.Read("HOME", "ZUIKI"));
                this.keyMapping.Add("CAPTURE", this.iniFile.Read("CAPTURE", "ZUIKI"));
            } else {
                int num = (int)MessageBox.Show("Device Not Found.");
            }
        }

        private void OnReport(HidReport report) {
            if (!this.IsDisposing) {
                MyDelegate myDelegate = delegate {
                    this.Zuiki_readBuffer = report.Data;
                    this.Zuiki_send();
                };
                this.Invoke(myDelegate);
            }
            if (this._device == null)
                return;
            this._device.ReadReport(new ReadReportCallback(this.OnReport));
        }

        private void Zuiki_close() {
            if (this._device == null)
                return;
            this.IsDisposing = true;
            this._device.CloseDevice();
            this._device.Dispose();
            this._device.MonitorDeviceEvents = false;
            this._device = (HidDevice)null;
            this.label1.Text = "Disconnected";
            this.controllerlist.Enabled = true;
            this.portList.Enabled = true;
        }

        private void Zuiki_send() {
            if ((int)this.Zuiki_readBuffer[4] <= (int)this.B9 - ((int)this.B9 - (int)this.B8) / 2)
                this.notchnow = -9;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B9 - ((int)this.B9 - (int)this.B8) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.B8 - ((int)this.B8 - (int)this.B7) / 2)
                this.notchnow = -8;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B8 - ((int)this.B8 - (int)this.B7) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.B7 - ((int)this.B7 - (int)this.B6) / 2)
                this.notchnow = -7;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B7 - ((int)this.B7 - (int)this.B6) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.B6 - ((int)this.B6 - (int)this.B5) / 2)
                this.notchnow = -6;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B6 - ((int)this.B6 - (int)this.B5) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.B5 - ((int)this.B5 - (int)this.B4) / 2)
                this.notchnow = -5;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B5 - ((int)this.B5 - (int)this.B4) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.B4 - ((int)this.B4 - (int)this.B3) / 2)
                this.notchnow = -4;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B4 - ((int)this.B4 - (int)this.B3) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.B3 - ((int)this.B3 - (int)this.B2) / 2)
                this.notchnow = -3;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B3 - ((int)this.B3 - (int)this.B2) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.B2 - ((int)this.B2 - (int)this.B1) / 2)
                this.notchnow = -2;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B2 - ((int)this.B2 - (int)this.B1) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.B1 - ((int)this.B1 - (int)this.P0) / 2)
                this.notchnow = -1;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.B1 - ((int)this.B1 - (int)this.P0) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.P0 - ((int)this.P0 - (int)this.P1) / 2)
                this.notchnow = 0;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.P0 - ((int)this.P0 - (int)this.P1) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.P1 - ((int)this.P1 - (int)this.P2) / 2)
                this.notchnow = 1;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.P1 - ((int)this.P1 - (int)this.P2) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.P2 - ((int)this.P2 - (int)this.P3) / 2)
                this.notchnow = 2;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.P2 - ((int)this.P2 - (int)this.P3) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.P3 - ((int)this.P3 - (int)this.P4) / 2)
                this.notchnow = 3;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.P3 - ((int)this.P3 - (int)this.P4) / 2 && (int)this.Zuiki_readBuffer[4] <= (int)this.P4 - ((int)this.P4 - (int)this.P5) / 2)
                this.notchnow = 4;
            if ((int)this.Zuiki_readBuffer[4] > (int)this.P4 - ((int)this.P4 - (int)this.P5) / 2)
                this.notchnow = 5;
            if (this.notchnow != this.notchbefor) {
                this.amount = this.notchnow == -9 || this.notchnow == 5 ? (this.notchbefor - this.notchnow) * 20 : this.notchbefor - this.notchnow;
                INPUT pInputs = new INPUT() {
                    type = 0,
                    ui = new INPUT_UNION() {
                        mouse = new MOUSEINPUT() {
                            dwFlags = 2048,
                            mouseData = this.amount * 120,
                            dwExtraInfo = IntPtr.Zero,
                            time = 0
                        }
                    }
                };
                Form1.NativeMethods.SendInput(1, ref pInputs, Marshal.SizeOf<INPUT>(pInputs));
            }
            this.notchbefor = this.notchnow;
            if (!this.arrowcheck.Checked && (int)this.Zuiki_rebbefor != (int)this.Zuiki_readBuffer[2]) {
                if (this.Zuiki_readBuffer[2] == (byte)0)
                    SendKeys.SendWait(this.keyMapping["UP"]);
                if (this.Zuiki_readBuffer[2] == (byte)4)
                    SendKeys.SendWait(this.keyMapping["DOWN"]);
                if (this.Zuiki_readBuffer[2] == (byte)2)
                    SendKeys.SendWait(this.keyMapping["LEFT"]);
                if (this.Zuiki_readBuffer[2] == (byte)6)
                    SendKeys.SendWait(this.keyMapping["RIGHT"]);
            }
            this.Zuiki_rebbefor = this.Zuiki_readBuffer[2];
            if (this.buttoncheck.Checked)
                return;
            int int32_1 = Convert.ToInt32(Convert.ToString(this.Zuiki_readBuffer[0], 2));
            // Y button
            int num1 = int32_1 - int32_1 / 10 * 10;
            if (num1 != this.bottonnum1befor) {
                if (num1 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["Y"]);
                this.bottonnum1befor = num1;
            }
            // B button
            int num2 = (int32_1 - int32_1 / 100 * 100) / 10;
            if (num2 != this.bottonnum10befor) {
                if (num2 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["B"]);
                this.bottonnum10befor = num2;
            }
            // A button
            int num3 = (int32_1 - int32_1 / 1000 * 1000) / 100;
            if (num3 != this.bottonnum100befor) {
                if (num3 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["A"]);
                this.bottonnum100befor = num3;
            }
            // X button
            int num4 = (int32_1 - int32_1 / 10000 * 10000) / 1000;
            if (num4 != this.bottonnum1000befor) {
                if (num4 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["X"]);
                this.bottonnum1000befor = num4;
            }
            // L button
            int num5 = (int32_1 - int32_1 / 100000 * 100000) / 10000;
            if (num5 != this.bottonnum10000befor) {
                if (num5 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["L"]);
                this.bottonnum10000befor = num5;
            }
            // R button
            int num6 = (int32_1 - int32_1 / 1000000 * 1000000) / 100000;
            if (num6 != this.bottonnum100000befor) {
                if (num6 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["R"]);
                this.bottonnum100000befor = num6;
            }
            // ZL Button
            int num7 = (int32_1 - int32_1 / 10000000 * 10000000) / 1000000;
            if (num7 != this.bottonnum1000000befor) {
                if (num7 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["ZL"]);
                this.bottonnum1000000befor = num7;
            }
            // ZR Button
            int num9 = (int32_1 - int32_1 / 100000000 * 100000000) / 10000000;
            if (num9 != this.bottonnum10000000befor) {
                if (num9 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["ZR"]);
                this.bottonnum10000000befor = num9;
            }
            int int32_2 = Convert.ToInt32(Convert.ToString(this.Zuiki_readBuffer[1], 2));
            // - button
            int num10 = int32_2 - int32_2 / 10 * 10;
            if (num10 != this.bottonnum2_1befor) {
                if (num10 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["MINUS"]);
                this.bottonnum2_1befor = num10;
            }
            // + button
            int num11 = (int32_2 - int32_2 / 100 * 100) / 10;
            if (num11 != this.bottonnum2_10befor) {
                if (num11 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["PLUS"]);
                this.bottonnum2_10befor = num11;
            }
            // Home button
            int num12 = (int32_2 - int32_2 / 100000 * 100000) / 10000;
            if (num12 != this.bottonnum2_100befor) {
                if (num12 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["HOME"]);
                this.bottonnum2_100befor = num12;
            }
            // Capture button
            int num13 = (int32_2 - int32_2 / 1000000 * 1000000) / 100000;
            if (num13 != this.bottonnum2_1000befor) {
                if (num13 % 2 == 1)
                    SendKeys.SendWait(this.keyMapping["CAPTURE"]);
                this.bottonnum2_1000befor = num13;
            }
        }

        protected override void Dispose(bool disposing) => base.Dispose(disposing);

        private void InitializeComponent() {
            this.connectBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.disconnectBtn = new System.Windows.Forms.Button();
            this.portList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.controllerlist = new System.Windows.Forms.ComboBox();
            this.close = new System.Windows.Forms.Button();
            this.buttoncheck = new System.Windows.Forms.CheckBox();
            this.arrowcheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(12, 13);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(75, 25);
            this.connectBtn.TabIndex = 0;
            this.connectBtn.Text = "Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(93, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 2;
            // 
            // disconnectBtn
            // 
            this.disconnectBtn.Enabled = false;
            this.disconnectBtn.Location = new System.Drawing.Point(12, 44);
            this.disconnectBtn.Name = "disconnectBtn";
            this.disconnectBtn.Size = new System.Drawing.Size(75, 25);
            this.disconnectBtn.TabIndex = 7;
            this.disconnectBtn.Text = "Disconnect";
            this.disconnectBtn.UseVisualStyleBackColor = true;
            this.disconnectBtn.Click += new System.EventHandler(this.disconnectBtn_Click);
            // 
            // portList
            // 
            this.portList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.portList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.portList.FormattingEnabled = true;
            this.portList.Location = new System.Drawing.Point(250, 44);
            this.portList.Name = "portList";
            this.portList.Size = new System.Drawing.Size(80, 21);
            this.portList.TabIndex = 46;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 47;
            this.label2.Text = "Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(178, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 49;
            this.label3.Text = "controller:";
            // 
            // controllerlist
            // 
            this.controllerlist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.controllerlist.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.controllerlist.FormattingEnabled = true;
            this.controllerlist.Location = new System.Drawing.Point(250, 16);
            this.controllerlist.Name = "controllerlist";
            this.controllerlist.Size = new System.Drawing.Size(160, 21);
            this.controllerlist.TabIndex = 48;
            this.controllerlist.SelectedIndexChanged += new System.EventHandler(this.controllerlist_SelectedIndexChanged);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(140, 115);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(140, 25);
            this.close.TabIndex = 50;
            this.close.Text = "Close the program";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // buttoncheck
            // 
            this.buttoncheck.AutoSize = true;
            this.buttoncheck.Location = new System.Drawing.Point(213, 81);
            this.buttoncheck.Name = "buttoncheck";
            this.buttoncheck.Size = new System.Drawing.Size(99, 17);
            this.buttoncheck.TabIndex = 51;
            this.buttoncheck.Text = "Disable buttons";
            this.buttoncheck.UseVisualStyleBackColor = true;
            this.buttoncheck.CheckedChanged += new System.EventHandler((object sender, EventArgs e) => {
                this.iniFile.Write("DISABLE_BUTTONS", this.buttoncheck.Checked ? "1" : "0", "ZUIKI");
            });
            // 
            // arrowcheck
            // 
            this.arrowcheck.AutoSize = true;
            this.arrowcheck.Location = new System.Drawing.Point(113, 81);
            this.arrowcheck.Name = "arrowcheck";
            this.arrowcheck.Size = new System.Drawing.Size(94, 17);
            this.arrowcheck.TabIndex = 52;
            this.arrowcheck.Text = "Disable D-Pad";
            this.arrowcheck.UseVisualStyleBackColor = true;
            this.arrowcheck.CheckedChanged += new System.EventHandler((object sender, EventArgs e) => {
                this.iniFile.Write("DISABLE_DPAD", this.arrowcheck.Checked ? "1" : "0", "ZUIKI");
            });
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 159);
            this.ControlBox = false;
            this.Controls.Add(this.buttoncheck);
            this.Controls.Add(this.arrowcheck);
            this.Controls.Add(this.close);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.controllerlist);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.portList);
            this.Controls.Add(this.disconnectBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connectBtn);
            this.Name = "Form1";
            this.Text = "ConToJREts";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public static class NativeMethods {
            public const int INPUT_MOUSE = 0;
            public const int INPUT_KEYBOARD = 1;
            public const int INPUT_HARDWARE = 2;
            public const int MOUSEEVENTF_MOVE = 1;
            public const int MOUSEEVENTF_ABSOLUTE = 32768;
            public const int MOUSEEVENTF_LEFTDOWN = 2;
            public const int MOUSEEVENTF_LEFTUP = 4;
            public const int MOUSEEVENTF_RIGHTDOWN = 8;
            public const int MOUSEEVENTF_RIGHTUP = 16;
            public const int MOUSEEVENTF_MIDDLEDOWN = 32;
            public const int MOUSEEVENTF_MIDDLEUP = 64;
            public const int MOUSEEVENTF_WHEEL = 2048;
            public const int WHEEL_DELTA = 120;
            public const int KEYEVENTF_KEYDOWN = 0;
            public const int KEYEVENTF_KEYUP = 2;
            public const int KEYEVENTF_EXTENDEDKEY = 1;

            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(ref Win32Point pt);

            [DllImport("user32.dll")]
            public static extern void SendInput(int nInputs, ref INPUT pInputs, int cbsize);
        }
    }
}
