using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using static Modbus.Enums;


namespace Modbus
{
    public partial class Form1 : Form
    {

        private readonly Service service;
        private string text = "";

        public Form1()
        {
            service = new Service();

            InitializeComponent();
        }

        private List<int> timeLimit()
        {
            List < int > list = new List<int>();
            for (int i = 0; i <= 1000; i += 10)
            {
                list.Add(i);
            }

            return list;
        }

        private List<int> timeoutTrans()
        {
            List<int> list = new List<int>();
            for (int i = 0; i <= 10000; i += 100)
            {
                list.Add(i);
            }

            return list;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = new List<int> { 150, 300, 600, 1200, 2400, 4800, 9600 /*, 14400, 19200, 38400, 56000, 57600, 115200*/ }.Select(it => new KeyValuePair<int, int>(it, it)).ToList();
            comboBox2.DataSource = service.GetPortNames().Select(it => new KeyValuePair<string, string>(it, it)).ToList();
            comboBox3.DataSource = Enum.GetNames(typeof(Transmission)).Select(it => new KeyValuePair<Transmission, string>((Transmission)Enum.Parse(typeof(Transmission), it), it.Replace("T_", ""))).ToList();
            comboBox4.DataSource = timeLimit().Select(it => new KeyValuePair<int, int>(it, it)).ToList();
            comboBox5.DataSource = timeoutTrans().Select(it => new KeyValuePair<int, int>(it, it)).ToList();
            comboBox7.DataSource = Enumerable.Range(1, 247).Select(it => new KeyValuePair<int, int>(it, it)).ToList();
            comboBox6.DataSource = Enum.GetNames(typeof(Station)).Select(it => new KeyValuePair<Station, string>((Station)Enum.Parse(typeof(Station), it), it.Replace('_', '/'))).ToList();
            comboBox8.DataSource = Enumerable.Range(0, 6).Select(it => new KeyValuePair<int, int>(it, it)).ToList();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                         // Running on the UI thread
                        var message = service.ReceiveMessage();


                    if (message!=null)
                    {
                        try
                        {
                            this.textBox2.Invoke((MethodInvoker)delegate {
                                // Running on the UI thread
                                this.textBox2.AppendText(Environment.NewLine);
                                this.textBox2.AppendText(message.Item1);
                            });
                            var address = this.textBox4.Text;
                            var instruction = this.textBox5.Text;
                            if (message.Item2)
                                service.SendMessage("0", "2", this.textBox1.Text);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }

                }
            }).Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var portParameters = new PortParameters
            {
                Speed = Int32.Parse(this.comboBox1.Text),
                PortName = this.comboBox2.Text,
                readTimeout = Int32.Parse(this.comboBox5.Text)
            };

            service.SelfAddress = Int32.Parse(this.comboBox7.Text);

            this.textBox2.Text = "Konfiguracja portu...";

            var isOpen = service.OpenPort(portParameters, comboBox6.Text);
            SetAllEnabled(!isOpen);

            if (isOpen)
            {
                this.textBox2.AppendText(Environment.NewLine);
                this.textBox2.Text += "Konfiguracja przebiegła pomyślnie.";
            }
            else
            {
                this.textBox2.AppendText(Environment.NewLine);
                this.textBox2.Text += "Konfiguracja NIE przebiegła pomyślnie.";
            }
        }
        private void SetAllEnabled(bool enabled)
        {
            foreach (var control in tableLayoutPanel1.Controls)
            {
                if (control is Control)
                    ((Control)control).Enabled = enabled;
            }
            button1.Enabled = enabled;
            button2.Enabled = !enabled;
            textBox1.Enabled = !enabled;
            textBox1.Focus();
            if (enabled)
                RefreshStationType();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetAllEnabled(true);
            service.ClosePort();
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshStationType();
        }
        private void RefreshStationType()
        {
            Station station = (Station)comboBox6.SelectedValue;
            switch (station)
            {
                case Station.MASTER:
                    comboBox7.Enabled = false;
                    comboBox5.Enabled = true;
                    textBox4.Enabled = true;
                    textBox5.Enabled = true;
                    comboBox7.DropDownStyle = ComboBoxStyle.Simple;
                    comboBox7.Text = "0";
                    button3.Enabled = true;
                    break;
                case Station.SLAVE:
                    comboBox7.Enabled = true;
                    comboBox5.Enabled = false;
                    textBox4.Enabled = false;
                    textBox5.Enabled = false;
                    comboBox7.DropDownStyle = ComboBoxStyle.DropDownList;
                    comboBox7.SelectedIndex = 0;
                    button3.Enabled = false;
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var message = this.textBox1.Text;
            var address = this.textBox4.Text;
            var instruction = this.textBox5.Text;

            if (address.Equals("0") && instruction.Equals("2"))
                return;

            var hexFrame = service.SendMessage(address, instruction, message);
            this.textBox3.AppendText(Environment.NewLine);
            this.textBox3.AppendText($"[out]: {message}");
            this.textBox3.AppendText($" [hex frame]: {hexFrame}");
            
            this.textBox1.Text = string.Empty;
            this.textBox1.Focus();
            //wysyłamy wiadomość do slave'a
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            text = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
