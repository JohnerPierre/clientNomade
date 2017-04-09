using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;

namespace clientNomade
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public RestClient Client { get; set; }
        private string currentID;
        private As3910TagListener l;
        Random rnd = new Random();
        int previousRand = 5;

        private void App_Load(object sender, EventArgs e)
        {
            InitView();

            //Enable shortcut
            KeyPreview = true;

            //Create REST client 
            string restServer = "http://192.168.137.1:5000";
            this.Client = new RestClient(restServer);
            tb_port.Text = "5000";
            tb_rest.Text = "http://192.168.137.1";

            // Init Tag Listener
            if (cb_serie.Items.Count>0)
            {
                l = new As3910TagListener(cb_serie.SelectedItem.ToString());
                l.NewTagEvent += Login;
            } 
        }

        private void InitView() {
            // Fix Some Display 
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;


            foreach (string s in SerialPort.GetPortNames())
            {
                cb_serie.Items.Add(s);
            }

            for (int i = 1; i < 10; i++)
                cb_idM.Items.Add(i);

            cb_idM.SelectedIndex = 1;
            cb_serie.SelectedIndex = 0;
            
        }

        private void Login(object sender, NewTagEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate { this.LoginMain(e.Tag);});
        }

        private void LoginMain(string id) {
            if (tabControl1.SelectedIndex == 0) {
                tabControl1.SelectedIndex = 1;
                this.currentID = id;
                string idMachine = cb_idM.SelectedItem.ToString();
                var json = Client.MakeRequest("/db/get_history2/"+id+":"+idMachine);
                List<string> items = JsonDecode(json);
                UpdateView(items);
            }      
        }

        private List<string> JsonDecode(string json)
        {
            List<string> v = new List<string>();
            json = Regex.Replace(json, "[@,\\.\"'\\\\]", string.Empty);
            string[] words = Regex.Split(json, "\n");
            foreach (string i in words)
            {
                if (i.Contains(':'))
                {
                    string[] s = i.Split(':');
                    v.Add(s[1]);
                }      
            }
            return v;
        }

        private void UpdateView(List<string> items) {
            label2.Text = items[2];
            label3.Text = items[1];
            label4.Text = items[0];
            Series s = new Series("Actual");
            s.ChartType = SeriesChartType.Line;
            chart1.Series.Add(s);
            List<int> av = new List<int>();
            List<int> count = new List<int>();
            List<double> average = new List<double>();
            foreach (string index in items)
            {

                if (index.Contains(';')) {
                    string[] split = index.Split(';');
                    for(int i = 0; i< split.Length-1; i++)
                    {
                        if (av.Count <= i) { 
                            av.Add(int.Parse(split[i]));
                            count.Add(1);
                        }
                        else {
                            av[i] += int.Parse(split[i]);
                            count[i] += 1;
                        }
                           
                    }
                }
            }

            for (int i = 0; i< av.Count; i++)
            {
                average.Add( (double)(av[i]) / count[i]);
            }

            AverageChart(average);
        }

        private void AverageChart(List<double> av)
        {
            Series a = new Series("Average");
            a.ChartType = SeriesChartType.Line;;
            foreach (double i in av)
            {
                    a.Points.Add(i);
            }
            chart1.Series.Add(a);
        }

        private void TickSimu(object sender,EventArgs e)
        {
            int newPoint = Math.Max(previousRand + rnd.Next(-3, 3), 0);
            Series s = chart1.Series.FindByName("Actual");
            s.Points.Add(newPoint);
            previousRand = newPoint;
        }

        private void BtnSimulate_Click(object sender, EventArgs e)
        {
            timerSimu.Enabled = !timerSimu.Enabled;
        }

        private void EndSession(object sender, EventArgs e)
        {
            timerSimu.Enabled = false;
            tabControl1.SelectedIndex = 2;
            string values = "";
            foreach (DataPoint p in chart1.Series.FindByName("Actual").Points)
            {
                values += p.YValues[0]+";";
            }

            string idMachine = cb_idM.SelectedItem.ToString();
            var json = Client.MakeRequest("/db/create_history/"+ this.currentID + ":"+idMachine + ":"+values);
            List <string> items = JsonDecode(json);

            label5.Text = items.ElementAt(0);

            chart1.Series.Clear();
            timerEnd.Tag = 30;
            btnENd.Text = "Logout(-)";
            timerEnd.Enabled = true;
        }

        private void TickEnd(object sender, EventArgs e)
        {
            int timeLeft = int.Parse(timerEnd.Tag.ToString()); 

            timerEnd.Tag = timeLeft - 1;
            btnENd.Text = "Logout(" + timeLeft + ")";

            if (timeLeft == 0)
                BtnENd_Click(sender, e);
        }

        private void BtnENd_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            timerEnd.Enabled = false;
        }

        private void OptionsShortcut(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'd' && tabControl1.SelectedIndex == 0)
                tabControl1.SelectedIndex = 3;
            else if (e.KeyChar == 'd' && tabControl1.SelectedIndex == 3)
                tabControl1.SelectedIndex = 0;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.l.CloseThread();
        }

        private void rest_TextChanged(object sender, EventArgs e)
        {
            this.Client.EndPoint = tb_rest.Text + ":"+ tb_port.Text;
        }

        private void cb_serie_TabIndexChanged(object sender, EventArgs e)
        {
            this.l.CloseThread();
            this.l = new As3910TagListener(cb_serie.SelectedItem.ToString());
            this.l.NewTagEvent += Login;
        }
    }
}
