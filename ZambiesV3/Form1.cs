using AutoUpdaterDotNET;
using JRPC_Client;
using MetroFramework;
using MetroFramework.Controls;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using XBLAPI;
using XBLAPI.Responses;
using XDevkit;
using Timer = System.Windows.Forms.Timer;

namespace ZambiesV3
{
    public partial class Form1 : MetroForm
    {
        IXboxConsole Console;
        Boolean force = false;
        Boolean force1 = false;
        Boolean lag = false;
        Boolean recoil = false;
        Boolean recoil1 = false;
        Boolean crosshair = false;
        Boolean crosshair1 = false;
        Boolean uav = false;
        Boolean wallhack = false;
        Boolean wallhack1 = false;
        Boolean redb = false;
        Boolean redb1 = false;
        Boolean devkit = false;
        MetroThemeStyle theme = MetroThemeStyle.Dark;
        bool[] autofire = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        bool[] autofirezm = new bool[4] { false, false, false, false };
        Timer autofireTimer;
        Timer autofireTimer2;

        public Form1()
        {
            InitializeComponent();
            clientboxz.SelectedIndex = 0;
            clientboxmp.SelectedIndex = 0;
            log("Not Connected!");

            autofireTimer = new Timer();
            autofireTimer.Interval = 100;
            autofireTimer.Tick += autofire_tick;


            autofireTimer2 = new Timer();
            autofireTimer2.Interval = 100;
            autofireTimer2.Tick += autofire_tickzm;

        }

        private bool try_connect()
        {
            try
            {
                System.Console.WriteLine("B");
                if (Console.Connect(out Console))
                {
                    GetConsoleType();
                    autofireTimer.Start();
                    autofireTimer2.Start();
                    log("Connected - " + ((devkit) ? "Devkit" : "Retail"));
                    return true;
                }
            }
            catch { }
            log("Failed to Connect!");
            autofireTimer.Stop();
            autofireTimer2.Stop();
            return false;
        }

        private void GetConsoleType()
        {
            byte[] Mem = Console.GetMemory(0x8E038610, 0x4);
            Array.Reverse(Mem);
            uint KVData = BitConverter.ToUInt32(Mem, 0);
            if ((KVData & 0x8000) == 0x8000) devkit = false;
            else devkit = true;
        }

        private Boolean Connected()
        {
            try
            {
                System.Console.WriteLine("A");
                GetConsoleType();
                autofireTimer.Start();
                autofireTimer2.Start();
                return true;
            }
            catch
            {
                System.Console.WriteLine("C");
                return try_connect();
            }
        }

        public void log(string command)
        {
            String date = DateTime.Now.ToString("hh:mm:ss tt");
            String text = date + " - " + command + "\r\n";
            status.Text = text;
            System.Console.WriteLine(text);
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            try_connect();
        }

        public static byte[] nop = new byte[] { 0x60, 0x00, 00, 00 };
        public static byte[] mr_r16_neg1 = new byte[] { 0x3A, 0x00, 0xFF, 0xFF };

        //xam offsets
        public static uint g_freememory = 0x81aa2090;
        public static uint g_rguserinfo = 0x81aa2600;
        public static uint g_XamUserGetXUID = 0x816d7e78;
        public static uint g_XUserFindUserAddress = 0x81829018;

        // Multiplayer Offsets
        public static uint ps_mp = 0x84571620; //TU7
        public static uint cbuf_mp = 0x8263A6A0; //TU7
        public static uint sv_mp = 0x8266EEB0; //TU7
        public static uint dvar_prot1_mp = 0x826B818C; //TU7
        public static uint dvar_prot2_mp = 0x826B81D0; //TU7
        public static uint jump_mp = 0x8209C764; //TU7
        public static uint no_recoil_mp = 0x82279CB8; public static byte[] recoil_bytes_mp = new byte[] { 0x4B, 0xF7, 0x92, 0x39 }; //TU7
        public static uint crosshair_mp = 0x82099FA8; // TU7
        public static uint uav_mp = 0x8228BBB8; public static byte[] uav_bytes_mp = new byte[] { 0x41, 0x9A, 0x00, 0xB0 }; //TU7
        public static uint whack_mp = 0x824B0898; public static byte[] whack_bytes_mp = new byte[] { 0x7C, 0xF0, 0x3B, 0x78 }; //TU7
        public static uint redboxes1_mp = 0x82610920; public static byte[] redbox1_bytes_mp = new byte[] { 0x41, 0x82, 0x00, 0xDC }; //TU7
        public static uint redboxes2_mp = 0x82610948; public static byte[] redbox2_bytes_mp = new byte[] { 0x41, 0x82, 0x00, 0xB4 }; //TU7

        // Zombies Offsets
        public static uint ps_z = 0x845160D0; //TU7
        public static uint cbuf_z = 0x82631630; //TU7
        public static uint sv_z = 0x82665968; //TU7
        public static uint dvar_prot1_zm = 0x826AC4B4; //TU7
        public static uint dvar_prot2_zm = 0x826AC4F8; //TU7
        public static uint no_recoil_zm = 0x82278798; public static byte[] recoil_bytes_zm = new byte[] { 0x4B, 0xF7, 0xA4, 0x69 }; //TU7
        public static uint redboxes1_zm = 0x826078E0; public static byte[] redbox1_bytes_zm = new byte[] { 0x41, 0x82, 0x00, 0xDC }; //TU7
        public static uint redboxes2_zm = 0x82607908; public static byte[] redbox2_bytes_zm = new byte[] { 0x41, 0x82, 0x00, 0xB4 }; //TU7
        public static uint crosshair_zm = 0x82099EC8; //TU7
        public static uint whack_zm = 0x824AF890; public static byte[] whack_bytes_zm = new byte[] { 0x7C, 0xF0, 0x3B, 0x78 }; //TU7

        public uint getZPS(int Client)
        {
            return (ps_z + ((uint)(Client * 0x61B8)));
        }

        public uint getMPPS(int Client)
        {
            return (ps_mp + ((uint)(Client * 0x61D8)));
        }

        private void sendGun(byte[] gun, int Client, int i)
        {
            if (i == 1)
            {
                Console.SetMemory(getZPS(Client) + 0x30B, gun);
            }
            else
            {
                Console.SetMemory(getZPS(Client) + 0x35B, gun);
            }
        }

        public void money(int Client)
        {
            if (money1.Text == "")
            {
                return;
            }
            int t = int.Parse(money1.Text);
            if (t < 0)
            {
                t = 0;
            }
            if (t > 999999)
            {
                t = 999999;
            }
            byte[] bytes = BitConverter.GetBytes(t);
            Array.Reverse(bytes);
            Console.SetMemory(this.getZPS(Client) + 0x5E8C, bytes);
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { Client, 0, "< \"^7Money set to: " + t + "!\"" });
        }

        public void maxAmmo(int Client)
        {
            if (!Connected()) return;
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { Client, 0, "< \"^7Max Ammo Given!\"" });
            for (int i = 0; i < 24; i++)
            {
                int x = i * 4;
                Console.SetMemory((uint)((this.getZPS(Client) + 0x53C) + x), new byte[] { 0x00, 0xFF, 0xFF, 0xFF });
            }
            //Console.SetMemory(this.getZPS(Client) + 0x582, new byte[] { 0xff, 0x00 });
            //Console.SetMemory(this.getZPS(Client) + 0x57A, new byte[] { 0xff, 0x00 });
        }

        private void metroButton11_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            clientboxz.Items.Clear();
            clientboxz.Items.Add("All Clients");
            for (int i = 0; i < 4; i++)
            {
                if (GetGamertagZ(i) != "") clientboxz.Items.Add(i + ": " + GetGamertagZ(i));
                else clientboxz.Items.Add(i + ": ");
            }
            log("Refreshed Clients");
            clientboxz.SelectedIndex = 0;
        }

        public string GetGamertagZ(int cli)
        {
            if (!Connected()) return null;
            byte[] data = new byte[15];
            data = Console.GetMemory(this.getZPS(cli) + 0x5DB0, 15);
            return this.ByteToString(data);
        }

        public string GetGamertagMP(int cli)
        {
            if (!Connected()) return null;
            byte[] data = new byte[15];
            data = Console.GetMemory(this.getMPPS(cli) + 0x5DB0, 15);
            return this.ByteToString(data);
        }

        public string ByteToString(byte[] input)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            if (input.Length > 0) input = trimZeros(input);
            else return "";
            return encoding.GetString(input);
        }

        public byte[] trimZeros(byte[] input)
        {
            var i = input.Length;
            while (i-- > 0 && input[i] == 0) { }

            var temp = new byte[i + 1];
            Array.Copy(input, temp, i + 1);
            return temp;
        }

        private void cash_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (GetGamertagZ(i) != "") money(i);
                    }
                    log("Set Money for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    money(t);
                    log("Set Money for " + GetGamertagZ(t));
                }
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {

            if (!Connected()) return;
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (GetGamertagZ(i) != "") god(i);
                    }
                    log("Set God Mode for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    god(t);
                    log("Set God Mode for " + GetGamertagZ(t));
                }
            }
        }
        private void god(int ClientIndex)
        {
            if (!Connected()) return;
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { ClientIndex, 0, "< \"^7God Mode: ^2On!\"" });
            Console.SetMemory(this.getZPS(ClientIndex) + 0x23, new byte[] { 0x05 });
        }

        private void god1(int ClientIndex)
        {
            if (!Connected()) return;
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { ClientIndex, 0, "< \"^7God Mode: ^1Off!\"" });
            Console.SetMemory(this.getZPS(ClientIndex) + 0x23, new byte[] { 0x04 });
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (GetGamertagZ(i) != "") god1(i);
                    }
                    log("Turned off God Mode for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    god1(t);
                    log("Turned off God Mode for " + GetGamertagZ(t));
                }
            }
        }

        private void metroButton10_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (GetGamertagZ(i) != "") maxAmmo(i);
                    }
                    log("Set Max Ammo for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    maxAmmo(t);
                    log("Set Max Ammo for " + GetGamertagZ(t));
                }
            }
        }

        private static uint CanonicalHash(string data)
        {
            uint hash = 0;
            data = data.ToLower() + (char)0x0;
            hash = (data[0] ^ (uint)0x4B9ACE2F) * 0x1000193;
            for (int i = 1; i <= data.Length - 1; i++)
                hash = (data[i] ^ hash) * 0x1000193;
            return hash;
        }

        /*private void metroButton5_Click(object sender, EventArgs e)
        {
            if(!Connected()) return;
            if (!force)
            {
                fhost.Text = "Turn Off Force Host";
                cbufz("party_connectToOthers 0; partyMigrate_disabled 1; sv_endGameIfISuck 0; party_minplayers 1; allowAllNAT 1; party_mergingEnabled 0; xpartygo");
                log("Force Host On!");
            }
            else
            {
                fhost.Text = "Turn On Force Host";
                cbufz("reset party_connectToOthers; reset partyMigrate_disabled; reset sv_endGameIfISuck; reset party_minplayers; reset allowAllNAT; reset party_mergingEnabled");
                log("Force Host Off!");
            }
            force = !force;
        }*/

        private void cbufz(string s)
        {
            if (!Connected()) return;
            Console.CallVoid(JRPC.ThreadType.Title, cbuf_z, new object[] { 0, s.ToString() });
            log("Called Cbuf: " + s.ToString());
        }

        private void cbufmp(string s)
        {
            if (!Connected()) return;
            Console.CallVoid(JRPC.ThreadType.Title, cbuf_mp, new object[] { 0, s.ToString() });
            log("Called Cbuf: " + s.ToString());
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (cbufbox.Text != "")
            {
                cbufz(cbufbox.Text);
            }
        }

        private void metroButton8_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            clientboxmp.Items.Clear();
            clientboxmp.Items.Add("All Clients");
            for (int i = 0; i < 12; i++)
            {
                if (GetGamertagMP(i) != "") clientboxmp.Items.Add(i + ": " + GetGamertagMP(i) + GetIPMP(i));
                else clientboxmp.Items.Add(i + ": ");
            }
            log("Refreshed Clients");
            clientboxz.SelectedIndex = 0;
        }

        private string GetIPMP(int i)
        {
            uint num = (uint)(i * 0x100) - 0x3B6AEE18;
            string str = BitConverter.ToString(Console.GetMemory(num, 8)).Replace("-", "");
            byte[] ipbytes = Console.GetMemory(num + 0xe5, 4);
            Array.Reverse(ipbytes);
            return ((str.Contains("09")) ? " - " + ByteToIP(ipbytes) : " - 0.0.0.0");
        }

        private void metroButton5_Click_1(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (cbufbox_mp.Text != "")
            {
                cbufmp(cbufbox_mp.Text);
            }
        }

        /*private void metroButton7_Click(object sender, EventArgs e)
        {
            if(!Connected()) return;
            if (!force1)
            {
                fhost1.Text = "Turn Off Force Host";
                cbufmp("party_connectToOthers 0; partyMigrate_disabled 1; sv_endGameIfISuck 0; party_minplayers 1; allowAllNAT 1");
                log("Force Host On!");
            }
            else
            {
                fhost1.Text = "Turn On Force Host";
                cbufmp("reset party_connectToOthers; reset partyMigrate_disabled; reset sv_endGameIfISuck; reset party_minplayers; reset allowAllNAT");
                log("Force Host Off!");
            }
            force1 = !force1;
        }*/

        private void dvar_prot_zm(object sender, EventArgs e)
        {
            if (!Connected()) return;
            Console.SetMemory(dvar_prot1_zm, nop);
            Console.SetMemory(dvar_prot2_zm, nop);
            log("Removed Dvar Protections!");
        }

        private void dvar_prot_mp(object sender, EventArgs e)
        {
            if (!Connected()) return;
            Console.SetMemory(dvar_prot1_mp, nop);
            Console.SetMemory(dvar_prot2_mp, nop);
            log("Removed Dvar Protections!");
        }

        /*private void lagswitch_Click(object sender, EventArgs e)
        {
            if(!Connected()) return;
            if (!lag)
            {
                lagswitch.Text = "Lag Switch Off";
                if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { 0, 0, "< \"^7Lag Switch : ^2On!\"" });
                Console.SetMemory(0x845A2E67, new byte[1]);
                log("Lag Switch On!");
            }
            else
            {
                lagswitch.Text = "Lag Switch On";
                if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { 0, 0, "< \"^7Lag Switch : ^1Off!\"" });
                Console.SetMemory(0x845A2E67, new byte[1] { 0x02 });
                log("Lag Switch Off!");
            }
            lag = !lag;
        }*/

        private void metroButton11_Click_1(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    log("UFO not for supported for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    Console.SetMemory(getZPS(t) + 0x5D03, new byte[1] { 0x01 });
                    Console.SetMemory(getZPS(t) + 0x5DEA, new byte[1] { 0x00 });
                    if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { t, 0, "< \"^7UFO Mode : ^1Enabling!\"" });
                    System.Threading.Timer timer = null;
                    timer = new System.Threading.Timer((obj) =>
                    {
                        Console.SetMemory(getZPS(t) + 0x5D03, new byte[1] { 0x02 });
                        if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { t, 0, "< \"^7UFO Mode : ^1Enabled!\"" });
                        timer.Dispose();
                    }, null, 5000, System.Threading.Timeout.Infinite);
                    log("UFO set for " + GetGamertagZ(t));
                }
            }
        }

        private void metroButton12_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    log("UFO not for supported for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    Console.SetMemory(getZPS(t) + 0x5D03, new byte[1] { 0x01 });
                    Console.SetMemory(getZPS(t) + 0x5DEA, new byte[1] { 0x80 });
                    if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { t, 0, "< \"^7UFO Mode : ^1Disabling!\"" });
                    System.Threading.Timer timer = null;
                    timer = new System.Threading.Timer((obj) =>
                    {
                        Console.SetMemory(getZPS(t) + 0x5D03, new byte[1] { 0x00 });
                        if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { t, 0, "< \"^7UFO Mode : ^1Disabled!\"" });
                        timer.Dispose();
                    }, null, 1000, System.Threading.Timeout.Infinite);
                    log("UFO turned off for " + GetGamertagZ(t));
                }
            }
        }

        private void svz(String s)
        {
            Console.CallVoid(JRPC.ThreadType.Title, sv_z, new object[] { -1, 1, s });
        }

        private void svmp(String s)
        {
            Console.CallVoid(JRPC.ThreadType.Title, sv_mp, new object[] { -1, 1, s });
            log("Called SV: " + s.ToString());
        }

        private void metroButton14_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (svboxmp.Text != "")
            {
                svmp(svboxmp.Text);
            }
        }

        private void metroButton13_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (svboxz.Text != "")
            {
                svz(svboxz.Text);
            }
        }

        private void metroButton15_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (gunbox.Text == null || gunbox.Text == "" || gunbox.Text.Length < 2) return;
            byte[] result = gunbox.Text.Split(' ').Select(part => byte.Parse(part, System.Globalization.NumberStyles.HexNumber)).ToArray();
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (GetGamertagZ(i) != "") sendGun(result, i, 1);
                    }
                    log("Gave weapon to all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    sendGun(result, t, 1);
                    log("Gave weapon to " + GetGamertagZ(t));
                }
            }
        }

        private void metroButton16_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (gunbox.Text == null || gunbox.Text == "" || gunbox.Text.Length < 2) return;
            byte[] result = gunbox.Text.Split(' ').Select(part => byte.Parse(part, System.Globalization.NumberStyles.HexNumber)).ToArray();
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (GetGamertagZ(i) != "") sendGun(result, i, 2);
                    }
                    log("Gave weapon to all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    sendGun(result, t, 2);
                    log("Gave weapon to " + GetGamertagZ(t));
                }
            }
        }

        private void norecoil_mp(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!recoil)
            {
                recb.Style = MetroFramework.MetroColorStyle.Green;
                Console.SetMemory(no_recoil_mp, nop);
                log("No Recoil On!");
            }
            else
            {
                recb.Style = MetroFramework.MetroColorStyle.Red;
                Console.SetMemory(no_recoil_mp, recoil_bytes_mp);
                log("No Recoil Off!");
            }
            recoil = !recoil;
        }

        private void metroButton20_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxmp.SelectedIndex > -1)
            {
                int t = clientboxmp.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (GetGamertagMP(i) != "") godmp(i);
                    }
                    log("Set God Mode for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 11) t = 11;
                    godmp(t);
                    log("Set God Mode for " + GetGamertagMP(t));
                }
            }
        }

        private void godmp(int ClientIndex)
        {
            if (!Connected()) return;
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_mp, new object[] { ClientIndex, 0, "< \"^7God Mode: ^2On!\"" });
            Console.SetMemory(this.getMPPS(ClientIndex) + 0x23, new byte[] { 0x05 });
        }

        private void godmp1(int ClientIndex)
        {
            if (!Connected()) return;
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_mp, new object[] { ClientIndex, 0, "< \"^7God Mode: ^1Off!\"" });
            Console.SetMemory(this.getMPPS(ClientIndex) + 0x23, new byte[] { 0x04 });
        }

        private void metroButton19_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxmp.SelectedIndex > -1)
            {
                int t = clientboxmp.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (GetGamertagMP(i) != "") godmp1(i);
                    }
                    log("Turned off God Mode for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 11) t = 11;
                    godmp1(t);
                    log("Turned off God Mode for " + GetGamertagMP(t));
                }
            }
        }

        private void metroButton21_Click(object sender, EventArgs e)
        {
            prestige.Text = "11";
            xp.Text = "1500000";
            score.Text = "4135965";
            kills.Text = "63542";
            deaths.Text = "41421";
            wins.Text = "1562";
            losses.Text = "1139";
            timep.Text = "286522";
            headshots.Text = "8221";
        }

        private void metroButton18_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxmp.SelectedIndex > -1)
            {
                int t = clientboxmp.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (GetGamertagMP(i) != "") maxAmmoMP(i);
                    }
                    log("Set Max Ammo for all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 11) t = 11;
                    maxAmmoMP(t);
                    log("Set Max Ammo for " + GetGamertagMP(t));
                }
            }
        }

        public void maxAmmoMP(int Client)
        {
            if (!Connected()) return;
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_mp, new object[] { Client, 0, "< \"^7Max Ammo Given!\"" });
            Console.SetMemory(this.getMPPS(Client) + 0x53C, new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF });
        }

        private void metroButton22_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxmp.SelectedIndex > -1)
            {
                int t = clientboxmp.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (GetGamertagMP(i) != "") speedMP(i);
                    }
                    log("Gave speed to all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 11) t = 11;
                    speedMP(t);
                    log("Gave speed to " + GetGamertagMP(t));
                }
            }
        }

        private void speedMP(int Client)
        {
            if (!Connected()) return;
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_mp, new object[] { Client, 0, "< \"^7Super-Speed ^2On!\"" });
            Console.SetMemory(this.getMPPS(Client) + 0x5DE4, new byte[] { 0x40, 0x90 });

        }

        private void speedMP1(int Client)
        {
            if (!Connected()) return;
            if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_mp, new object[] { Client, 0, "< \"^7Super-Speed ^1Off!\"" });
            Console.SetMemory(this.getMPPS(Client) + 0x5DE4, new byte[] { 0x3f, 0x80 });

        }

        private void metroButton24_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxmp.SelectedIndex > -1)
            {
                int t = clientboxmp.SelectedIndex - 1;
                if (t == -1)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (GetGamertagMP(i) != "") speedMP1(i);
                    }
                    log("Took speed from all players!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 11) t = 11;
                    speedMP1(t);
                    log("Took speed from " + GetGamertagMP(t));
                }
            }
        }

        private void metroButton25_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (jump.Text != "" && jump.Text != null)
            {
                float jump1 = float.Parse(jump.Text);
                if (notify.Checked) Console.CallVoid(JRPC.ThreadType.Title, sv_mp, new object[] { -1, 0, "< \"^7Jump Height Set To: ^5" + jump1 + "!\"" });
                Console.WriteFloat(jump_mp, jump1);
            }
        }

        private void metroButton27_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (prest.Text != "" && prest.Text != null && rankk.Text != "" && rankk.Text != null)
            {
                if (clientboxmp.SelectedIndex > -1)
                {
                    int t = clientboxmp.SelectedIndex - 1;
                    if (t == -1)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            if (GetGamertagMP(i) != "") setRank(i, int.Parse(prest.Text), int.Parse(rankk.Text));
                        }
                        log("Sent rank for all players!");
                    }
                    else
                    {
                        if (t < 0) t = 0;
                        if (t > 11) t = 11;
                        setRank(t, int.Parse(prest.Text), int.Parse(rankk.Text));
                        log("Set rank for " + GetGamertagMP(t));
                    }
                }
            }
            else
            {
                log("Check your arguments!");
            }

        }

        private void setRank(int Client, int prestige, int rank)
        {
            if (!Connected()) return;
            if (prestige < 0) prestige = 0;
            if (prestige > 256) prestige = 256;
            if (rank < 0) rank = 0;
            if (rank > 256) rank = 256;
            Console.WriteInt32(this.getMPPS(Client) + 0x5E20, rank);
            Console.WriteInt32(this.getMPPS(Client) + 0x5E24, prestige);
        }

        private void changeText(String s)
        {
            if (gtbox.SelectedText.Length > 0)
            {
                gtbox.Text = gtbox.Text.Replace(gtbox.SelectedText, s);
            }
            else
            {
                this.gtbox.Text += s;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            changeText("^1");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            changeText("^2");
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            changeText("^3");
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            changeText("^4");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            changeText("^5");
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            changeText("^6");
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            changeText("^7");
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            changeText("^8");
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            changeText("^9");
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            changeText("^0");
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            changeText("^;");
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            changeText("^:");
        }

        private void metroButton26_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (gtbox.Text != null && gtbox.Text != "")
            {
                string s = gtbox.Text;
                if (clientboxmp.SelectedIndex > -1)
                {
                    int t = clientboxmp.SelectedIndex - 1;
                    if (t == -1)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            if (GetGamertagMP(i) != "") setGT(i, s);
                        }
                        log("Set gt for all players!");
                    }
                    else
                    {
                        if (t < 0) t = 0;
                        if (t > 11) t = 11;
                        setGT(t, s);
                        log("Set gt for " + GetGamertagMP(t));
                    }
                }
            }
            else
            {
                log("Can't set null gt!");
            }

        }

        private void setGT(int Client, string s)
        {
            if (!Connected()) return;
            if (s.Length > 15) s = s.Substring(0, 15);
            Console.WriteString(this.getMPPS(Client) + 0x5DFC, s);
        }

        private void setCT(int Client, string s)
        {
            if (!Connected()) return;
            if (s.Length > 4) s = s.Substring(0, 4);
            Console.WriteString(this.getMPPS(Client) + 0x5E74, s);
        }

        private void metroButton28_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (gtbox.Text != null && gtbox.Text != "")
            {
                string s = gtbox.Text;
                if (clientboxmp.SelectedIndex > -1)
                {
                    int t = clientboxmp.SelectedIndex - 1;
                    if (t == -1)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            if (GetGamertagMP(i) != "") setCT(i, s);
                        }
                        log("Set ct for all players!");
                    }
                    else
                    {
                        if (t < 0) t = 0;
                        if (t > 11) t = 11;
                        setCT(t, s);
                        log("Set ct for " + GetGamertagMP(t));
                    }
                }
            }
            else
            {
                log("Can't set null ct!");
            }
        }

        private void metroButton30_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (gtbox.Text != "" && gtbox.Text != null)
            {
                string s = gtbox.Text;
                if (s.Length > 15) s = s.Substring(0, 15);
                byte[] b = s.ToWCHAR();
                //Console.SetMemory(0x81AA261C, b);
                Array.Reverse(b);
                Console.SetMemory(0xc035261D, b);
                Array.Reverse(b);
                Console.SetMemory(0xc035261C, b);
                //Console.WriteString(0x847FD9A8, s);
                log("Set pre-game gt!");
            }
            else
            {
                log("Can't set null gt!");
            }

        }

        private void metroButton23_Click(object sender, EventArgs e)
        {
            if (prestige.Text != null && prestige.Text != "" && int.Parse(prestige.Text) > -1)
            {
                cbufmp("statsetbyname PLEVEL " + int.Parse(prestige.Text));
            }
            if (xp.Text != null && xp.Text != "" && int.Parse(xp.Text) > -1)
            {
                cbufmp("statsetbyname RANKXP " + int.Parse(xp.Text));
            }
            if (score.Text != null && score.Text != "" && int.Parse(score.Text) > -1)
            {
                cbufmp("statsetbyname SCORE " + int.Parse(score.Text));
            }
            if (kills.Text != null && kills.Text != "" && int.Parse(kills.Text) > -1)
            {
                cbufmp("statsetbyname KILLS " + int.Parse(kills.Text));
            }
            if (deaths.Text != null && deaths.Text != "" && int.Parse(deaths.Text) > -1)
            {
                cbufmp("statsetbyname DEATHS " + int.Parse(deaths.Text));
            }
            if (wins.Text != null && wins.Text != "" && int.Parse(wins.Text) > -1)
            {
                cbufmp("statsetbyname WINS " + int.Parse(wins.Text));
            }
            if (losses.Text != null && losses.Text != "" && int.Parse(losses.Text) > -1)
            {
                cbufmp("statsetbyname LOSSES " + int.Parse(losses.Text));
            }
            if (timep.Text != null && timep.Text != "" && int.Parse(timep.Text) > -1)
            {
                cbufmp("statsetbyname TIMEPLAYED " + int.Parse(timep.Text));
            }
            if (headshots.Text != null && headshots.Text != "" && int.Parse(headshots.Text) > -1)
            {
                cbufmp("statsetbyname HEADSHOTS " + int.Parse(headshots.Text));
            }
            cbufmp("updategamerprofile;uploadstats");
        }

        private void metroButton17_Click_1(object sender, EventArgs e)
        {
            if (prestige.Text != null && prestige.Text != "" && int.Parse(prestige.Text) > -1)
            {
                cbufz("statsetbyname PLEVEL " + int.Parse(prestige.Text));
            }
            if (rankz.Text != null && rankz.Text != "" && int.Parse(rankz.Text) > -1)
            {
                cbufz("statsetbyname RANK " + int.Parse(rankz.Text));
            }
            if (xp.Text != null && xp.Text != "" && int.Parse(xp.Text) > -1)
            {
                cbufz("statsetbyname RANKXP " + int.Parse(xp.Text));
            }
            if (score.Text != null && score.Text != "" && int.Parse(score.Text) > -1)
            {
                cbufz("statsetbyname SCORE " + int.Parse(score.Text));
            }
            if (kills.Text != null && kills.Text != "" && int.Parse(kills.Text) > -1)
            {
                cbufz("statsetbyname KILLS " + int.Parse(kills.Text));
            }
            if (deaths.Text != null && deaths.Text != "" && int.Parse(deaths.Text) > -1)
            {
                cbufz("statsetbyname DEATHS " + int.Parse(deaths.Text));
            }
            if (timep.Text != null && timep.Text != "" && int.Parse(timep.Text) > -1)
            {
                cbufz("statsetbyname TIME_PLAYED_TOTAL " + int.Parse(timep.Text));
            }
            if (headshots.Text != null && headshots.Text != "" && int.Parse(headshots.Text) > -1)
            {
                cbufz("statsetbyname HEADSHOTS " + int.Parse(headshots.Text));
            }
            if (roundsz.Text != null && roundsz.Text != "" && int.Parse(roundsz.Text) > -1)
            {
                cbufz("statsetbyname TOTAL_ROUNDS_SURVIVED " + int.Parse(roundsz.Text));
            }
            if (perksz.Text != null && perksz.Text != "" && int.Parse(perksz.Text) > -1)
            {
                cbufz("statsetbyname PERKS_DRANK " + int.Parse(perksz.Text));
            }
            cbufz("updategamerprofile;uploadstats");
        }

        private void norecoil_zm(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!recoil1)
            {
                rec2.Style = MetroFramework.MetroColorStyle.Green;
                Console.SetMemory(no_recoil_zm, nop);
                log("No Recoil On!");
            }
            else
            {
                rec2.Style = MetroFramework.MetroColorStyle.Red;
                Console.SetMemory(no_recoil_zm, recoil_bytes_zm);
                log("No Recoil Off!");
            }
            recoil1 = !recoil1;
        }

        private void smallcrosshairs_mp(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!crosshair)
            {
                crossb.Style = MetroFramework.MetroColorStyle.Green;
                Console.WriteFloat(crosshair_mp, 1);
                log("Small Crosshairs On!");
            }
            else
            {
                crossb.Style = MetroFramework.MetroColorStyle.Red;
                Console.WriteFloat(crosshair_mp, 240);
                log("Small Crosshairs Off!");
            }
            crosshair = !crosshair;
        }

        private void vsat_mp(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!uav)
            {
                auav.Style = MetroFramework.MetroColorStyle.Green;
                Console.SetMemory(uav_mp, nop);
                log("Advanced UAV On!");
            }
            else
            {
                auav.Style = MetroFramework.MetroColorStyle.Red;
                Console.SetMemory(uav_mp, uav_bytes_mp);
                log("Advanced UAV Off!");
            }
            uav = !uav;
        }

        private void wallhack_mp(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!wallhack)
            {
                wallb.Style = MetroFramework.MetroColorStyle.Green;
                Console.SetMemory(whack_mp, mr_r16_neg1);
                log("Wallhack On!");
            }
            else
            {
                wallb.Style = MetroFramework.MetroColorStyle.Red;
                Console.SetMemory(whack_mp, whack_bytes_mp);
                log("Wallhack Off!");
            }
            wallhack = !wallhack;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            someshittext.Text = "Zambies Updated to TU8!\r\n\r\nCheck out Tampered Live\r\n    @ http://Tampered.Live/ \r\n\r\nCheck Out NiNJA\r\n    @ https://xbls.ninja/ \r\n\r\n\r\n\r\nLove you guys!   - HaXzz";
            AutoUpdater.LetUserSelectRemindLater = false;
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
            AutoUpdater.RemindLaterAt = 5;
            AutoUpdater.Start("http://pastebin.com/raw/xtccstxP");
        }

        private void metroButton31_Click_2(object sender, EventArgs e)
        {
            AutoUpdater.DownloadUpdate();
        }

        private void redboxes_mp(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!redb)
            {
                redbb.Style = MetroFramework.MetroColorStyle.Green;
                Console.SetMemory(redboxes1_mp, nop);
                Console.SetMemory(redboxes2_mp, nop);
                log("Red Boxes On!");
            }
            else
            {
                redbb.Style = MetroFramework.MetroColorStyle.Red;
                Console.SetMemory(redboxes1_mp, redbox1_bytes_mp);
                Console.SetMemory(redboxes2_mp, redbox2_bytes_mp);
                log("Red Boxes Off!");
            }
            redb = !redb;
        }

        private void redboxes_zm(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!redb1)
            {
                redboxb.Style = MetroFramework.MetroColorStyle.Green;
                Console.SetMemory(redboxes1_zm, nop);
                Console.SetMemory(redboxes2_zm, nop);
                log("Red Boxes On!");
            }
            else
            {
                redboxb.Style = MetroFramework.MetroColorStyle.Red;
                Console.SetMemory(redboxes1_zm, redbox1_bytes_zm);
                Console.SetMemory(redboxes2_zm, redbox2_bytes_zm);
                log("Red Boxes Off!");
            }
            redb1 = !redb1;
        }

        private void smallcrosshairs_zm(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!crosshair1)
            {
                smallch.Style = MetroFramework.MetroColorStyle.Green;
                Console.WriteFloat(crosshair_zm, 1);
                log("Small Crosshairs On!");
            }
            else
            {
                smallch.Style = MetroFramework.MetroColorStyle.Red;
                Console.WriteFloat(crosshair_zm, 240);
                log("Small Crosshairs Off!");
            }
            crosshair1 = !crosshair1;
        }

        private void wallhack_zm(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (!wallhack1)
            {
                wallb1.Style = MetroFramework.MetroColorStyle.Green;
                Console.SetMemory(whack_zm, mr_r16_neg1);
                log("Wallhack On!");
            }
            else
            {
                wallb1.Style = MetroFramework.MetroColorStyle.Red;
                Console.SetMemory(whack_zm, whack_bytes_zm);
                log("Wallhack Off!");
            }
            wallhack1 = !wallhack1;
        }

        private void metroButton29_Click(object sender, EventArgs e)
        {
            log("Not Implemented!");
        }

        private void metroButton32_Click(object sender, EventArgs e)
        {
            if (theme == MetroFramework.MetroThemeStyle.Dark)
            {
                theme = MetroFramework.MetroThemeStyle.Light;
                clientboxz.BackColor = Color.White;
                clientboxmp.BackColor = Color.White;
                clientboxz.ForeColor = Color.Black;
                clientboxmp.ForeColor = Color.Black;
                metroButton32.Text = "Go Dark!";
            }
            else
            {
                theme = MetroFramework.MetroThemeStyle.Dark;
                clientboxz.BackColor = Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
                clientboxmp.BackColor = Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
                clientboxz.ForeColor = Color.White;
                clientboxmp.ForeColor = Color.White;
                metroButton32.Text = "Go Light!";
            }
            darkstyle.Theme = theme;
            this.Theme = theme;
            foreach (MetroButton c in GetAll(this, typeof(MetroButton)))
            {
                c.Theme = theme;
            }
            foreach (MetroComboBox c in GetAll(this, typeof(MetroComboBox)))
            {
                c.Theme = theme;
            }
            foreach (MetroCheckBox c in GetAll(this, typeof(MetroCheckBox)))
            {
                c.Theme = theme;
            }
            foreach (MetroForm c in GetAll(this, typeof(MetroForm)))
            {
                c.Theme = theme;
            }
            foreach (MetroLabel c in GetAll(this, typeof(MetroLabel)))
            {
                c.Theme = theme;
            }
            foreach (MetroLink c in GetAll(this, typeof(MetroLink)))
            {
                c.Theme = theme;
            }
            foreach (MetroMessageBox c in GetAll(this, typeof(MetroMessageBox)))
            {
                c.Theme = theme;
            }
            foreach (MetroPanel c in GetAll(this, typeof(MetroPanel)))
            {
                c.Theme = theme;
            }
            foreach (MetroProgressBar c in GetAll(this, typeof(MetroProgressBar)))
            {
                c.Theme = theme;
            }
            foreach (MetroProgressSpinner c in GetAll(this, typeof(MetroProgressSpinner)))
            {
                c.Theme = theme;
            }
            foreach (MetroRadioButton c in GetAll(this, typeof(MetroRadioButton)))
            {
                c.Theme = theme;
            }
            foreach (MetroScrollBar c in GetAll(this, typeof(MetroScrollBar)))
            {
                c.Theme = theme;
            }
            foreach (MetroTabControl c in GetAll(this, typeof(MetroTabControl)))
            {
                c.Theme = theme;
            }
            foreach (MetroTabPage c in GetAll(this, typeof(MetroTabPage)))
            {
                c.Theme = theme;
            }
            foreach (MetroTextBox c in GetAll(this, typeof(MetroTextBox)))
            {
                c.Theme = theme;
            }
            foreach (MetroTile c in GetAll(this, typeof(MetroTile)))
            {
                c.Theme = theme;
            }
            foreach (MetroToggle c in GetAll(this, typeof(MetroToggle)))
            {
                c.Theme = theme;
            }
            foreach (MetroTrackBar c in GetAll(this, typeof(MetroTrackBar)))
            {
                c.Theme = theme;
            }
            foreach (MetroUserControl c in GetAll(this, typeof(MetroUserControl)))
            {
                c.Theme = theme;
            }

            byte[] mac = { 0x00, 0x1D, 0xD8, 0x3F, 0xD9, 0xDE };
            SHA1Managed sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(mac, 0, 6);

            Array.Reverse(hash);

            uint upd = 0x10;
            uint tmp = BitConverter.ToUInt32(hash, 16);
            upd |= (uint)(tmp & ~0xFF);

            System.Console.WriteLine(upd.ToString("X"));

        }

        public IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        private void metroButton33_Click_1(object sender, EventArgs e)
        {
            prestige.Text = "11";
            xp.Text = "2200000";
            rankz.Text = "34";
            score.Text = "8735679";
            kills.Text = "785453";
            deaths.Text = "534";
            timep.Text = "286522";
            headshots.Text = "34291";
            roundsz.Text = "59513";
            perksz.Text = "2614";
        }

        private void sendPlayerCommand(int ClientIndex, uint offset, byte[] command)
        {
            if (!Connected()) return;
            Console.SetMemory(this.getMPPS(ClientIndex) + offset, command);
        }

        private void sendPlayerCommandZM(int ClientIndex, uint offset, byte[] command)
        {
            if (!Connected()) return;
            Console.SetMemory(this.getZPS(ClientIndex) + offset, command);
        }

        private void metroButton34_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxmp.SelectedIndex > -1)
            {
                int t = clientboxmp.SelectedIndex - 1;
                if (t == -1)
                {
                    log("Cannot autofire for all clients!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 11) t = 11;
                    autofire[t] = !autofire[t];
                    CheckAutofire();
                    log("Autofire enabled for " + GetGamertagMP(t));
                }
            }
        }

        private void CheckAutofire()
        {
            if (!Connected())
            {
                autofireTimer.Stop();
                autofireTimer2.Stop();
            }
            else
            {
                Boolean AutoZM = false;
                Boolean AutoMP = false;
                for (int i = 0; i < 12; i++)
                {
                    if (autofirezm[i]) AutoZM = true;
                    if (autofire[i]) AutoMP = true;
                }


                if (AutoMP) autofireTimer.Start();
                if (AutoZM) autofireTimer2.Start();
            }
        }



        private void autofire_tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 12; i++)
            {
                if (autofire[i])
                {
                    try
                    {
                        sendPlayerCommand(i, 0x53, new byte[] { 1 });
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex);
                    }
                }
            }
        }

        private void autofire_tickzm(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if (autofirezm[i])
                {
                    try
                    {
                        sendPlayerCommandZM(i, 0x53, new byte[] { 1 });
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex);
                    }
                }
            }
        }

        private void metroButton35_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (clientboxz.SelectedIndex > -1)
            {
                int t = clientboxz.SelectedIndex - 1;
                if (t == -1)
                {
                    log("Cannot autofire for all clients!");
                }
                else
                {
                    if (t < 0) t = 0;
                    if (t > 3) t = 3;
                    autofirezm[t] = !autofirezm[t];
                    CheckAutofire();
                    log("Autofire enabled for " + GetGamertagZ(t));
                }
            }
        }

        public static string[] stat_strings = new string[] { "multikill__objective_scorestreak_projectile", "multikill__rcbomb", "multikill__with_heroability", "multikill__with_heroweapon", "multikill__with_rcbomb", "multikill__zone_attackers", "multikill__attackers_ai_tank", "multikill__lmg_or_smg_hip_fire", "multikill__near_death", "multikill__remote_missile", "multikill__with_heroability", "multikill__with_heroweapon", "multikill__with_mgl", "multikill__attackers", "most_kills_least_deaths", "master_humiliation",
            "master_killer", "master_of_humiliation", "mastery_air_assault", "mastery_boot_camp", "mastery_career", "mastery_efficiency", "mastery_game_mode_heroics", "mastery_game_mode_victories", "mastery_ground_assault", "mastery_handling", "mastery_hero_weapon", "mastery_humiliation", "mastery_killer", "mastery_lethals", "mastery_perk_1", "mastery_perk_2",
            "mastery_perk_3", "mastery_return_fire", "mastery_specialist_abilitites", "mastery_specialist_killjoy", "mastery_support", "mastery_tactical", "mastery_wildcards", "medal_aitank_kill", "medal_annihilator_kill", "medal_annihilator_multikill", "medal_annihilator_multikill_2", "medal_armblades_kill", "medal_armblades_multikill", "medal_armblades_multikill_2", "medal_assisted_suicide", "medal_backstabber_kill",
            "medal_ball_capture_assist", "medal_ball_capture_carry", "medal_ball_capture_throw", "medal_ball_intercept", "medal_bomb_detonated", "medal_bounce_hatchet_kill", "medal_bowlauncher_kill", "medal_bowlauncher_multikill", "medal_bowlauncher_multikill_", "medal_c4_multikill", "medal_capture_enemy_crate", "medal_cleanse_kill", "medal_clear_2_attackers", "medal_combat_robot_kill", "medal_comeback_from_deathstreak", "medal_completed_match",
            "medal_dart_kill", "medal_death_machine_kill", "medal_defend_hq_last_alive", "medal_defused_bomb", "medal_defused_bomb_last_man_alive", "medal_destroyed_aitank", "medal_destroyed_combat_robot", "medal_destroyed_counteruav", "medal_destroyed_dart", "medal_destroyed_drone_strike", "medal_destroyed_emp", "medal_destroyed_heli_comlink", "medal_destroyed_heli_guard", "medal_destroyed_heli_gunner", "medal_destroyed_helicopter_agr_drop", "medal_destroyed_helicopter_giunit_drop",
            "medal_destroyed_high_altitude_vsat", "medal_destroyed_hover_rcxd", "medal_destroyed_microwave_turret", "medal_destroyed_plane_mortar", "medal_destroyed_raps_deployship", "medal_destroyed_raps_helicopter", "medal_destroyed_remote_missle", "medal_destroyed_rolling_thunder", "medal_destroyed_rolling_thunder_all_drones", "medal_destroyed_sentinel", "medal_destroyed_sentry_gun", "medal_destroyed_straferun", "medal_destroyed_supply_drop", "medal_destroyed_uav", "medal_destroyed_vtol_mothership", "medal_electrified",
            "medal_eliminate_oic", "medal_eliminate_sd", "medal_elimination_and_last_player_alive", "medal_end_enemy_armblades_attack", "medal_end_enemy_gravity_spike_attack", "medal_end_enemy_psychosis", "medal_end_enemy_specialist_weapon", "medal_escort_robot_disable", "medal_escort_robot_disable_near_goal", "medal_escort_robot_escort_goal", "medal_final_kill_elimination", "medal_first_kill", "medal_flag_capture", "medal_flag_carrier_kill_return_close", "medal_flamethrower_kill", "medal_flamethrower_multikill",
            "medal_flamethrower_multikill_2", "medal_flashback_kill", "medal_focus_earn_multiscorestreak", "medal_focus_earn_scorestreak", "medal_frag_multikill", "medal_gelgun_kill", "medal_gelgun_multikill", "medal_gelgun_multikill_2", "medal_gravityspikes_kill", "medal_gravityspikes_multikill", "medal_gravityspikes_multikill_2", "medal_hack__agrs_in_hack", "medal_hacked", "medal_hatchet_kill", "medal_headshot", "medal_heatwave_kill",
            "medal_heatwave_multikill_2", "medal_helicopter_comlink_kill", "medal_hover_rcxd_kill", "medal_hq_destroyed", "medal_hq_secure", "medal_humiliation_gun", "medal_kill_ball_carrier", "medal_kill_confirmed_multi", "medal_kill_enemies_one_bullet", "medal_kill_enemy_after_death", "medal_kill_enemy_grenade_throwback", "medal_kill_enemy_injuring_teammate", "medal_kill_enemy_one_bullet", "medal_kill_enemy_that_heatwaved_you", "medal_kill_enemy_that_is_in_air", "medal_kill_enemy_that_is_using_optic_camo",
            "medal_kill_enemy_that_is_wallrunning", "medal_kill_enemy_that_pulsed_you", "medal_kill_enemy_that_used_resurrect", "medal_kill_enemy_when_injured", "medal_kill_enemy_while_both_in_air", "medal_kill_enemy_while_capping", "medal_kill_enemy_while_capping_dom", "medal_kill_enemy_while_carrying_ball", "medal_kill_enemy_while_flashbanged", "medal_kill_enemy_while_in_air", "medal_kill_enemy_while_sliding", "medal_kill_enemy_while_stunned", "medal_kill_enemy_while_using_psychosis", "medal_kill_enemy_while_wallrunning", "medal_kill_enemy_who_has_flashbacked", "medal_kill_enemy_who_has_high_score",
            "medal_kill_enemy_who_has_powerarmor", "medal_kill_enemy_who_is_speedbursting", "medal_kill_enemy_who_is_using_focus", "medal_kill_enemy_who_killed_teammate", "medal_kill_enemy_with_care_package_crush", "medal_kill_enemy_with_fists", "medal_kill_enemy_with_gunbutt", "medal_kill_enemy_with_hacked_care_package", "medal_kill_enemy_with_more_ammo_oic", "medal_kill_enemy_with_their_weapon", "medal_kill_flag_carrier", "medal_kill_hacker_in_hack", "medal_kill_hacker_then_hack_in_hack", "medal_kill_in__seconds_gun", "medal_kill_underwater_enemy_explosive", "medal_kill_x_score_shrp",
            "medal_killed_annihilator_enemy", "medal_killed_armblades_enemy", "medal_killed_bomb_defuser", "medal_killed_bomb_planter", "medal_killed_bowlauncher_enemy", "medal_killed_enemy_while_carrying_flag", "medal_killed_flamethrower_enemy", "medal_killed_gelgun_enemy", "medal_killed_gravityspikes_enemy", "medal_killed_lightninggun_enemy", "medal_killed_minigun_enemy", "medal_killed_pineapple_enemy", "medal_killstreak_", "medal_killstreak_", "medal_killstreak_", "medal_killstreak_",
            "medal_killstreak_", "medal_killstreak_", "medal_killstreak_more_than_", "medal_knife_leader_gun", "medal_knife_with_ammo_oic", "medal_koth_secure", "medal_lightninggun_kill", "medal_lightninggun_multikill", "medal_lightninggun_multikill_2", "medal_longshot_kill", "medal_microwave_turret_kill", "medal_minigun_kill", "medal_minigun_multikill", "medal_minigun_multikill_2", "medal_most_points_shrp", "medal_multikill_2",
            "medal_multikill_3", "medal_multikill_4", "medal_multikill_5", "medal_multikill_6", "medal_multikill_7", "medal_multikill_8", "medal_multikill_more_than_8", "medal_multiple_grenade_launcher_kill", "medal_neutral_b_secured", "medal_optic_camo_capture_objective", "medal_optic_camo_kill", "medal_pineapple_kill", "medal_pineapple_multikill", "medal_pineapple_multikill_2", "medal_plane_mortar_kill", "medal_position_secure",
            "medal_power_armor_kill", "medal_quickly_secure_point", "medal_raps_kill", "medal_remote_missile_kill", "medal_resurrect_kill", "medal_retrieve_own_tags", "medal_revenge_kill", "medal_robot_disable_near_goal", "medal_rolling_thunder_kill", "medal_sentinel_kill", "medal_sentry_gun_kill", "medal_share_care_package", "medal_speed_burst_kill", "medal_stick_explosive_kill", "medal_stop_enemy_killstreak", "medal_straff_run_kill",
            "medal_teammate_confirm_kill", "medal_traversal_kill", "medal_uninterrupted_obit_feed_kills", "medal_vision_pulse_kill", "medal_vtol_mothership_kill", "medal_won_match", "medal_x2_score_shrp", "most_kills_least_deaths", "multikill_2_objective_scorestreak_projectile", "multikill_2_rcbomb", "multikill_2_with_heroability", "multikill_2_with_heroweapon", "multikill_2_with_rcbomb", "multikill_2_zone_attackers", "multikill_3_attackers_ai_tank", "multikill_3_lmg_or_smg_hip_fire",
            "multikill_3_near_death", "multikill_3_remote_missile", "multikill_3_with_heroability", "multikill_3_with_heroweapon", "multikill_3_with_mgl", "multikill_5_attackers", "objective_time", "offends", "oic_gamemode_mastery", "optic_camo_capture_objective", "penetration_shots", "score", "perk_bulletflinch_kills", "perk_earnmoremomentum_earn_streak", "perk_fastmantle_kills", "perk_fastweaponswitch_kill_after_swap",
            "perk_flak_survive", "perk_gpsjammer_immune_kills", "perk_hacker_destroy", "perk_immune_cuav_kills", "perk_longersprint", "perk_loudenemies_kills", "perk_movefaster_kills", "perk_noname_kills", "perk_nottargetedbyairsupport_destroy_aircraft", "perk_protection_stun_kills", "perk_quieter_kills", "perk_scavenger_kills_after_resupply", "pistolheadshot_10_onegame", "precision_master", "primary_mastery", "kills",
            "deaths", "plants", "protect_streak_with_trophy", "reload_then_kill_dualclip", "return_fire_killstreak_mastery", "returns", "revives", "sas_gamemode_mastery", "score_hc", "score_multiteam", "score_streaks_mastery", "sd_gamemode_mastery", "secondary_mastery", "shock_enemy_then_stab_them", "shoot_aircraft", "shoot_down_helicopter",
            "shoot_down_sentinel", "shotgun_mastery", "shrp_gamemode_mastery", "smg_mastery", "sniper_mastery", "specialist_transmissions", "stick_explosive_kill_5_onegame", "streaker", "stun_aitank_with_emp_grenade", "suicides", "support_killstreak_mastery", "survive_claymore_kill_planter_flak_jacket_equipped", "survive_with_flak", "tdm_gamemode_mastery", "teamkills", "teamkills_nostats",
            "throws", "ties", "time_played_alive", "time_played_allies", "time_played_axis", "time_played_opfor", "time_played_other", "time_played_team3", "time_played_total", "top3", "top3any", "top3any_hc", "top3any_multiteam", "top3team", "topplayer", "total_shots",
            "triple_kill_defenders_and_capture", "weapons_mastery", "weekly_timestamp", "wins", "wins_hc", "wins_multiteam", "mantle_then_kill", "longshot_3_onelife", "losses", "long_shot_longbarrel_suppressor_optic", "long_distance_hatchet_kill", "lmg_mastery", "lifetime_earnings", "lifetime_buyin", "last_escrow", "koth_gamemode_mastery",
            "killstreak_5_with_sentry_gun", "killstreak_5_with_death_machine", "killstreak_5_picked_up_weapon", "killstreak_5_dogs", "killstreak_30_no_scorestreaks", "killstreak_10_no_weapons_perks", "killsdenied", "killsconfirmed", "killsasflagcarrier", "kills_suppressor_ghost_hardwired_blastsuppressor", "kills_sprinting_dual_wield_and_gung_ho", "kills_pistol_lasersight_suppressor_longbarrel", "kills_one_life_fastmags_and_extclip", "kills_hipfire_rapidfire_lasersights_fasthands", "kills_first_throw_both_hatchets", "kills_extclip_grip_fastmag_quickdraw_stock",
            "kills_double_kill_3_lethal", "kills_counteruav_emp_hardline", "kills_auto_turret_5", "kills_after_reload_fastreload", "kills_after_jumping_or_sliding", "kills_ads_stock_and_cpu", "kills_ads_quickdraw_and_grip", "kills_3_resupplied_nade_one_life", "killed_raps_assist", "killed_raps", "killed_dog_close_to_teammate", "killed_dog", "kill_with_tossed_back_lethal", "kill_with_thermal_and_smoke_ads", "kill_with_specialist_overclock", "kill_with_resupplied_lethal_grenade",
            "kill_with_remote_control_sentry_gun", "kill_with_remote_control_ai_tank", "kill_with_pickup", "kill_with_loadout_weapon_with_3_attachments", "kill_with_hacked_claymore", "kill_with_hacked_carepackage", "kill_with_gunfighter", "kill_with_dual_lethal_grenades", "kill_with_cooked_grenade", "kill_with_controlled_sentinel", "kill_with_controlled_ai_tank", "kill_with_claymore", "kill_with_c4", "kill_with_both_primary_weapons", "kill_with_2_perks_same_category", "kill_while_wallrunning_2_walls",
            "kill_while_wallrunning", "kill_while_underwater", "kill_while_uav_active", "kill_while_sliding_from_doublejump", "kill_while_sliding", "kill_while_satellite_active", "kill_while_mantling", "kill_while_in_air", "kill_while_emp_active", "kill_while_damaging_with_microwave_turret", "kill_while_cuav_active", "kill_wallrunner_or_air_with_rcbomb", "kill_uav_enemy_with_ghost", "kill_trip_mine_shocked", "kill_tracker_sixthsense", "kill_stunned_tacmask",
            "kill_stun_lethal", "kill_sprint_stunned_gungho_tac", "kill_specialist_with_specialist", "kill_sixthsense_awareness", "kill_shocked_enemy", "kill_scavenger_tracker_resupply", "kill_prone_enemy", "kill_primary_and_secondary", "kill_overkill_gunfighter_5_attachments", "kill_overclock_afterburner_specialist_weapon_after_thrust", "kill_nemesis", "kill_optic_5_attachments", "kill_near_plant_engineer_hardwired", "kill_hip_gung_ho", "kill_hardwired_coldblooded", "kill_flashed_enemy",
            "kill_flak_tac_while_stunned", "kill_flak_tac_while_stunned", "kill_every_enemy", "kill_entire_team_with_specialist_weapon", "kill_enemy_withcar", "kill_enemy_with_tacknife", "kill_enemy_with_picked_up_weapon", "kill_enemy_with_fists", "kill_enemy_who_shocked_you", "kill_enemy_who_damaged_robot", "kill_enemy_while_prone", "kill_enemy_while_crouched", "kill_enemy_through_wall_with_fmj", "kill_enemy_through_wall", "kill_enemy_thats_wallrunning", "kill_enemy_that_in_air",
            "kill_enemy_that_blinded_you", "kill_enemy_survive_flak", "kill_enemy_sixth_sense", "kill_enemy_shoot_their_explosive", "kill_enemy_revealed_by_sensor", "kill_enemy_one_bullet_sniper", "kill_enemy_one_bullet_shotgun", "kill_enemy_locking_on_with_chopper_gunner", "kill_enemy_5_teammates_assists", "kill_doublejump_uav_engineer_hardwired", "kill_detect_tracker", "kill_dead_silence", "kill_crossbow_stackfire", "kill_concussed_enemy", "kill_close_deadsilence_awareness", "kill_close_blast_deadsilence",
            "kill_carrier", "kill_booby_trap_engineer", "kill_blindeye_ghost_aircraft", "kill_blast_doublejump", "kill_before_specialist_weapon_use", "kill_awareness", "kill_attacker_with_robot_or_tank", "kill_as_support_gunner", "kill_anteup_overclock_scorestreak_specialist", "kill_after_tac_insert", "kill_after_resupply", "kill_after_doublejump_out_of_water", "kill_2_wildcards", "kill_2_greed_2_perks_each", "kill_2_attackers_with_comlink", "kill_15_with_blade",
            "kill_10_enemy_one_bullet_sniper_onegame", "humiliate_victim", "kill_10_enemy_one_bullet_shotgun_onegame", "humiliate_attacker", "hq_gamemode_mastery", "holdingteamdefenderflag", "hits", "highlights_created", "hero_weapon_mastery", "hero_transmissions", "headshots", "headshot_fmj_highcaliber_longbarrel", "headshot_assault_5_onegame", "hatchet_kill_with_shield_equiped", "hasprestiged", "hasclantag",
            "hack_streak_with_blindeye_or_engineer", "hack_gamemode_mastery", "hack_enemy_target", "gun_gamemode_mastery", "ground_assault_killstreak_mastery", "gold_club", "get_final_kill", "game_modes_mastery", "flagcarrierkills", "films_shoutcasted", "field_specialist", "escorts", "end_enemy_specialist_ability_with_emp", "emblem_version", "earn_scorestreak_anteup", "earn_5_scorestreaks_anteup",
            "dr_lung", "double_kill_defenders", "double_kill_attackers", "dom_gamemode_mastery", "dm_gamemode_mastery", "disarm_hacked_carepackage", "disables", "diamond_club", "destructions", "destroyed_qrdrone_with_bullet", "destroyed_helicopter_with_bullet", "destroy_turret", "destroy_scorestreak_with_specialist", "destroy_scorestreak_with_dart", "destroy_scorestreak_rapidfire_fmj", "destroy_score_streak_with_qrdrone",
            "destroy_rcbomb_with_hatchet", "destroy_raps_before_drop", "destroy_qrdrone", "destroy_helicopter", "destroy_hcxd_with_hatchet", "destroy_explosive_with_trophy", "destroy_explosive", "destroy_equipment_with_emp_grenade", "destroy_equipment_with_emp_engineer", "destroy_equipment_engineer", "destroy_equipment", "destroy_combat_robot", "destroy_car", "destroy_aircraft_with_missile_drone", "destroy_aircraft_with_emp", "destroy_aircraft",
            "destroy_air_and_ground_blindeye_coldblooded", "destroy_ai_scorestreak_coldblooded", "destroy_ai_aircraft_using_blindeye", "destroy_5_tactical_inserts", "dem_gamemode_mastery", "defuses", "defused_bomb_last_man_alive", "defends", "defend_teammate_who_captured_package", "defend_hq_last_man_alive", "defend_carrier", "death_dodger", "currencyspent", "ctf_gamemode_mastery", "contracts_xp_earned", "contracts_purchased",
            "contracts_cp_spent", "contracts_cp_earned", "contracts_completed", "conf_gamemode_mastery", "completed_all_challenges", "complete_all_career_ops", "complete_all_career_combat_ops", "codpoints", "carries", "career_score_multiteam", "career_score_hc", "career_score", "captures", "capture_objective_tactician", "capture_objective_in_smoke", "capture_enemy_carepackage",
            "call_in_3_care_packages", "boot_camp_graduate", "assists", "assist_score_uav", "assist_score_satellite", "assist_score_microwave_turret", "assist_score_killstreak", "assist_score_emp", "assist_score_cuav", "assist_score", "assist", "assignments_mastery", "assault_rifle_mastery", "all_diamond_weapons", "air_assault_killstreak_mastery", "activate_cuav_while_enemy_satelite_active",
            "melee", "misc_bonus", "monthly_timestamp", "match_bonus", "maxxp"};

        private void UnlockAll()
        {
            log("Unlock Most Started...");
            Random random = new Random();

            cbufmp("statwriteddl playerstatslist 28 challengevalue " + random.Next(250, 999) +
                ";statwriteddl playerstatslist 29 challengevalue " + random.Next(250, 999));

            cbufmp("statwriteddl playerstatslist 30 challengevalue " + random.Next(250, 999) +
                ";statwriteddl playerstatslist 31 challengevalue " + random.Next(250, 999));

            for (int x = 0; x < 100; x++)
            {
                cbufmp("statwriteddl itemstats " + x + " stats headshots challengevalue " + random.Next(250, 999) +
                    ";statwriteddl itemstats " + x + " stats challenge1 challengevalue " + random.Next(250, 999) +
                    ";statwriteddl itemstats " + x + " stats challenge2 challengevalue " + random.Next(250, 999));

                cbufmp("statwriteddl itemstats " + x + " stats challenge3 challengevalue " + random.Next(250, 999) +
                    ";statwriteddl itemstats " + x + " stats challenge4 challengevalue " + random.Next(250, 999) +
                    ";statwriteddl itemstats " + x + " stats challenge5 challengevalue " + random.Next(250, 999));

                cbufmp("statwriteddl itemstats " + x + " stats challenge6 challengevalue " + random.Next(6 * (10 ^ 8), 11 * (10 ^ 8)) +
                    ";statwriteddl itemstats " + x + " stats kills challengevalue " + random.Next(6 * (10 ^ 8), 11 * (10 ^ 8)) +
                    ";statwriteddl itemstats " + x + " stats challenge7 challengevalue " + random.Next(6 * (10 ^ 8), 11 * (10 ^ 8)));

                cbufmp("statwriteddl itemstats " + x + " plevel 2" +
                    ";statwriteddl itemstats " + x + " xp 133337");

                cbufmp("statwriteddl itemstats " + x + " stats kills statvalue " + random.Next(6 * (10 ^ 8), 11 * (10 ^ 8)) +
                    ";statwriteddl itemstats " + x + " stats challenges challengevalue " + random.Next(250, 999));
            }

            cbufmp("statwriteddl playerstatslist 32 challengevalue " + random.Next(250, 999) +
                ";statwriteddl playerstatslist 33 challengevalue " + random.Next(250, 999) +
                ";statwriteddl playerstatslist 34 challengevalue " + random.Next(250, 999));

            cbufmp("statwriteddl playerstatslist 35 challengevalue " + random.Next(250, 999) +
                ";statwriteddl playerstatslist 36 challengevalue " + random.Next(250, 999) +
                ";statwriteddl playerstatslist 37 challengevalue " + random.Next(250, 999));

            for (int i = 0; i < 596; i += 2)
            {
                cbufmp("statwriteddl playerstatslist " + stat_strings[i] + " challengevalue " + random.Next(6 * (10 ^ 8), 11 * (10 ^ 8)) +
                    ";statwriteddl playerstatslist " + stat_strings[i + 1] + " challengevalue " + random.Next(6 * (10 ^ 8), 11 * (10 ^ 8)));
            }

            cbufmp("statwriteddl playerstatsbygametype tdm wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype hctdm wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype dm wins challengevalue " + random.Next(400, 501));

            cbufmp("statwriteddl playerstatsbygametype hcsd wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype sd wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype hcdem wins challengevalue " + random.Next(400, 501));

            cbufmp("statwriteddl playerstatsbygametype dom wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype hcdom wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype dem wins challengevalue " + random.Next(400, 501));

            cbufmp("statwriteddl playerstatsbygametype hckoth wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype koth wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype hcconf wins challengevalue " + random.Next(400, 501));

            cbufmp("statwriteddl playerstatsbygametype gun wins challengevalue " + random.Next(300, 501) +
                ";statwriteddl playerstatsbygametype ball wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype ctf wins challengevalue " + random.Next(300, 501));

            cbufmp("statwriteddl playerstatsbygametype sr wins challengevalue " + random.Next(400, 501) +
                ";statwriteddl playerstatsbygametype conf wins challengevalue " + random.Next(300, 510) +
                ";statwriteddl playerstatsbygametype fr wins challengevalue " + random.Next(400, 501));

            cbufmp("statwriteddl playerstatsbygametype oic wins challengevalue " + random.Next(300, 510) +
                ";statwriteddl playerstatsbygametype escort wins challengevalue " + random.Next(400, 501));

            log("Unlock Most Done!");
        }

        private void metroButton34_Click_1(object sender, EventArgs e)
        {
            if (!Connected()) return;
            UnlockAll();
        }

        private void metroButton35_Click_1(object sender, EventArgs e)
        {

        }

        private void metroButton37_Click(object sender, EventArgs e)
        {
            if (!Connected()) return;
            if (gtbox.Text == "") return;
            gtbox.Text = GetXUID(gtbox.Text);
        }

        public string GetXUID(string gt)
        {
            byte[] buffer = new byte[8];
            Console.SetMemory(g_freememory + 50, buffer);
            object[] objArray1 = new object[] { 0x9000006F93463L, 0, gt, 0x18, g_freememory + 50, 0 };
            Console.CallVoid(g_XUserFindUserAddress, objArray1);
            Thread.Sleep(1000);
            return BitConverter.ToString(Console.GetMemory(g_freememory + 50, 8)).Replace("-", "");
        }

        public string ByteToIP(byte[] data)
        {
            string str = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                decimal num2 = Convert.ToDecimal(data[i]);
                str = (i == 3) ? (str + Convert.ToString(num2)) : (str + Convert.ToString(num2) + ".");
            }
            return str;
        }

        public UInt16 ReverseShort(UInt16 input)
        {
            byte[] tmp = BitConverter.GetBytes(input);
            Array.Reverse(tmp);
            return BitConverter.ToUInt16(tmp, 0);
        }

        public UInt32 ReverseInt(UInt32 input)
        {
            byte[] tmp = BitConverter.GetBytes(input);
            Array.Reverse(tmp);
            return BitConverter.ToUInt32(tmp, 0);
        }

        public UInt64 ReverseLong(UInt64 input)
        {
            byte[] tmp = BitConverter.GetBytes(input);
            Array.Reverse(tmp);
            return BitConverter.ToUInt64(tmp, 0);
        }

        private static byte[] Reverse(byte[] array)
        {
            byte[] tmp = new byte[array.Length];
            int i = array.Length - 1;
            foreach (byte b in array)
            {
                tmp[i] = b;
                i--;
            }
            return tmp;
        }

        private static byte[] Reverse8(byte[] input)
        {
            byte[] buffer = new byte[input.Length];
            int num = input.Length - 8;
            int num2 = 0;
            for (int i = 0; i < (input.Length / 8); i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    buffer[num2 + j] = input[num + j];
                }
                num -= 8;
                num2 += 8;
            }
            return buffer;
        }

        private uint GetModuleHandle(string moduleName)
        {
            return Console.Call<uint>("xam.xex", 1102, new object[1] { moduleName });
        }

        private void metroButton35_Click_2(object sender, EventArgs e)
        {
            if (!Connected()) return;
            String Module = moduleTextBox.Text;
            if (Module.Length <= 4) return;
            uint Handle = GetModuleHandle(Module);
            if (Handle <= 0U)
            {
                Console.Call<uint>("xboxkrnl.exe", 409, new object[] { "Hdd:\\" + Module, 8, 0, 0 });
                Thread.Sleep(100);
                Handle = GetModuleHandle(Module);
                if (Handle <= 0U) log("Failed to load " + Module);
                else log("Module " + Module + " loaded.");
            }
            else
            {
                Console.WriteInt16(Handle + 64U, 1);
                Console.CallVoid("xboxkrnl.exe", 417, new object[] { Handle });
                Thread.Sleep(300);
                Handle = GetModuleHandle(Module);
                if (Handle > 0U) log("Failed to unload " + Module);
                else log("Module " + Module + " unloaded.");
                Thread.Sleep(100);
                Console.Call<uint>("xboxkrnl.exe", 409, new object[] { "Hdd:\\" + Module, 8, 0, 0 });
                if (Handle <= 0U) log("Failed to load " + Module);
                else log("Module " + Module + " loaded.");
            }
        }

        private void metroButton38_Click(object sender, EventArgs e)
        {
            byte[] In = StringToByteArray("64E60D2BB5B056AFDE235830F3697A72E233738C60042077BFB35A804C28785CDD483A949DDFE30840C9EA993631964F017AA05159ED24677031828352DA06E43639CE38042662D9023DFFE1AD73BD5BED758445EA03838A7E6AF7D78A5DCA777D2D98CDE090FECEA3B85CC50B23C5C48FCF3C2DC5B1B3DF699A492659190D");
            Array.Reverse(In);
            BigInteger Modulus = BigInteger.Parse("00E1322F1DE92AD64B494455CB05173F6671A964A415536E2B680C40F54FDA808F19B82CD0D7E964B2224C56DE03E2462F946F4FFFAD4588CF78CEED1CE5FD0F80533AE97043EAD1D12E39880C3CAEEBFDA5ACA3A69445E542EF269D5459952D252945B0169BEF788FB1EAE548AC1AC3C878899708DE24D1ED04D0555079199527", NumberStyles.HexNumber);
            BigInteger Exponent = BigInteger.Parse("10001", NumberStyles.HexNumber);
            BigInteger Input = new BigInteger(In);
            BigInteger Output = BigInteger.ModPow(Input, Exponent, Modulus);
            System.Console.WriteLine(BitConverter.ToString(Output.ToByteArray()).Replace("-", "").Substring(0, 256));
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

    }
}
