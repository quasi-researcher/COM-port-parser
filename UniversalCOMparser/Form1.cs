using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Globalization;

namespace UniversalCOMparser
{
    public partial class Form1 : Form
    {
        //COM port def
        SerialPort port = new SerialPort();
        Parity parityBit;
        StopBits stopBit;

        //variables to split string
        bool VertHor=true;  //data order: is value in every new string or in alternate strings
        bool[] maskMain = new bool[20]; //mask for values of interest from COM port
        bool SplitEnabled = false; //split the incoming string
        bool LogEnabled = false;  //log data from COM to txt file
        int CycleSizeMain = 1;  //parameter for alternate strings (repetition period)
        int CycleSizeFirst = 0; //parameter for alternate strings (fisrt cycle)
        char[] sepMain = new char[5]; //separators to plit the string

        //double[] maxarr = new double[30] ;
        //double[] arr = new double[10];
        
        int x = 0; //counter of incoming messages from COM
        int plotInd = 0; //what to plot

        delegate void delegateUpdateForm(string txt);

        public Form1()
        {
           
            InitializeComponent();
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            //default COM port settings
            comboBox2.SelectedIndex = 3;  //9600
            comboBox3.SelectedIndex = 3;  //8 data bits
            comboBox4.SelectedIndex = 0;  //Parity - NONE
            comboBox5.SelectedIndex = 1;  //1 stop bit
            // default data log name
            textBox1.Text = DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00")+DateTime.Now.Second.ToString("00") + "_" + DateTime.Now.Day.ToString("00") + DateTime.Now.Month.ToString("00") + DateTime.Now.Year.ToString() + "_comport_log.txt";
            saveFileDialog1.FileName = textBox1.Text;
            button5.Enabled = false;  // button stop data logs
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;

            File.AppendAllText("app_log.txt", Environment.NewLine + DateTime.Now.ToString() + " app started");

        }

        private void button1_Click(object sender, EventArgs e)  //refresh serial ports names
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
        }

        private void button3_Click(object sender, EventArgs e)  //browse log file directory
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog1.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e) //start logging data
        {
            LogEnabled = true;
            button4.Enabled = false;
            button5.Enabled = true;
            button3.Enabled = false;
            checkBox1.Enabled = false;
        }

        private void button5_Click(object sender, EventArgs e) //stop logging data
        {
            LogEnabled = false;
            button5.Enabled = false;
            button4.Enabled = true;
            button3.Enabled = true;
            checkBox1.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)  // open COM port
        {
            try
            {
                port.PortName = comboBox1.SelectedItem.ToString();
                port.BaudRate = Convert.ToInt32(comboBox2.SelectedItem.ToString());
                port.DataBits= Convert.ToInt16(comboBox3.SelectedItem.ToString());
                
                switch (comboBox4.SelectedIndex)
                {
                    case 0:
                        parityBit = Parity.None;
                        break;
                    case 1:
                        parityBit = Parity.Odd;
                        break;
                    case 2:
                        parityBit = Parity.Even;
                        break;
                    case 3:
                        parityBit = Parity.Mark;
                        break;
                    case 4:
                        parityBit = Parity.Space;
                        break;
                }
                port.Parity = parityBit;

                switch (comboBox5.SelectedIndex)
                {
                    case 0:
                        stopBit = StopBits.None;
                    break;
                    case 1:
                        stopBit = StopBits.One;
                        break;
                    case 2:
                        stopBit = StopBits.Two;
                    break;
                    case 3:
                        stopBit = StopBits.OnePointFive;
                        break;
                    
                }
                port.StopBits = stopBit;
                port.DtrEnable = true;
                port.Open();
                port.DataReceived += Port_DataReceived; //method which is called when data arrived 

                //disable controls after successful connection
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox4.Enabled = false;
                comboBox5.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button6.Enabled = true;
                button7.Enabled = true;
                button8.Enabled = true;
                numericUpDown1.Maximum = 0;
            }
            catch (Exception ex)
            {
                File.AppendAllText("app_log.txt", Environment.NewLine + DateTime.Now.ToString() + " " + ex.ToString());
                MessageBox.Show("Can't open selected port" + Environment.NewLine + "Error source: " + ex.ToString(), "COM port parser", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
         
        //method that is called when data arrived
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e) 
        {
            string txt;
            string txtToLog="";
            txt = port.ReadLine();
            if (LogEnabled)
            {
                if (checkBox1.Checked)
                {
                    txtToLog = DateTime.Now.ToString() + " " + txt;
                }
                else
                {
                    txtToLog = txt;
                }
                File.AppendAllText(textBox1.Text, Environment.NewLine+txtToLog);
            }

            //update chart and text boxes
            UpdateForm(txt);
          
        }

        //update chart and text boxes
        private void UpdateForm(string txt)
        {
            string[] splitted = new string[50];
            double y=0;
            if (InvokeRequired)
            {
                Invoke(new delegateUpdateForm(UpdateForm), txt);
            }
            else
            {
                CycleSizeFirst = Convert.ToInt16(numericUpDown2.Value);
                textBox2.AppendText(txt.Trim() + Environment.NewLine);
                if (plotInd==0) //try to plot raw incoming string if possible
                {
                    Double.TryParse(txt, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                    if (x > 1000) { chart1.ChartAreas[0].AxisX.ScaleView.Zoom(x - 1000, x); }
                    chart1.Series[0].Points.AddY(y);
                }
                if (SplitEnabled) //split incoming string to separate values 
                {
                    splitted = SplitString(txt);
                    if (plotInd != 0)
                    {
                        if ((x - CycleSizeFirst) % CycleSizeMain == 0)
                        {
                            Double.TryParse(splitted[plotInd - 1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                            if (x > 1000) { chart1.ChartAreas[0].AxisX.ScaleView.Zoom(x - 1000, x); }
                            chart1.Series[0].Points.AddY(y);
                        }
                    }
                }
                numericUpDown2.Maximum = x;
                x++;
            }
        }

        private void button6_Click(object sender, EventArgs e) //data snapshot button
        {
            try
            {
                textBox3.Text = textBox2.Lines[textBox2.Lines.Count() - 2];
                
            }
            catch (Exception ex)
            {
                File.AppendAllText("app_log.txt", Environment.NewLine + DateTime.Now.ToString() + " " + ex.ToString());
                MessageBox.Show("Error" + Environment.NewLine + "Error source: " + ex.ToString(), "COM port parser", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button7_Click(object sender, EventArgs e) //open a form to adjust split settings
        {
            Form2 form2 = new Form2(textBox3.Text);
            form2.AdviseParent += new Form2.AdviseParentEventHandler(SetFromForm2);
            form2.Show();
            
        }

        //pass setings to the main window
        public void SetFromForm2(char[] sep, bool VertOrHor, int CycleSize, bool[] mask)
        {
            int vartoplot = 0; //number of variables of interest
            for (int i=0; i<maskMain.Length-1; i++)
            { maskMain[i] = false; } //reset the mask
            for (int i=0; i<sepMain.Length-1; i++)
            { sepMain[i] = '\0'; } //reset the separator list

            SplitEnabled = true;
            sepMain = sep;
            maskMain = mask;
            VertHor = VertOrHor;
            CycleSizeMain = CycleSize;

            for (int i=0; i< maskMain.Length; i++)
            {
                if (maskMain[i])
                { vartoplot++;  }
            }
            numericUpDown1.Maximum = vartoplot;
        }

        //method to split the incoming string
        private string[] SplitString(string txt)
        {
            string[] splitted = new string[20];
            string[] splittedMask = new string[20];
            string OnlyMarked = "";
            int k = 0;
            if (txt.IndexOfAny(sepMain) != -1)
            {
                try
                {
                    splitted = txt.Split(sepMain);
                }
                catch (Exception ex)
                {
                    File.AppendAllText("app_log.txt", Environment.NewLine + DateTime.Now.ToString() + " " + ex.ToString());
                }
            }
            if (VertHor)
            {
                for (int i=0; i<=splitted.Length-1; i++)
                {
                    if (maskMain[i])
                    {
                        OnlyMarked += " " + splitted[i];
                        splittedMask[k] = splitted[i];
                        k++;
                    }
                }
                textBox4.AppendText(OnlyMarked + Environment.NewLine);
                
            }
            else
            {
                if ((x - CycleSizeFirst) % CycleSizeMain == 0)
                {
                    for (int i = 0; i <= splitted.Length - 1; i++)
                    {
                        if (maskMain[i])
                        {
                            OnlyMarked += " " + splitted[i];
                            splittedMask[k] = splitted[i];
                            k++;
                        }
                    }
                    textBox4.AppendText(OnlyMarked + Environment.NewLine);
                }
                
            }
            
            return splittedMask;
        }

        private void button8_Click(object sender, EventArgs e) //plot splitted variable
        {
            chart1.Series[0].Points.Clear(); //reset the chart
            plotInd = Convert.ToInt16(numericUpDown1.Value); //select which variable to plot
        }

        
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
