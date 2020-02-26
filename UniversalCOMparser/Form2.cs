using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace UniversalCOMparser
{
    public partial class Form2 : Form
    {
        public delegate void AdviseParentEventHandler(char[] sep, bool VertOrHor, int CycleSize, bool[] mask);
        public event AdviseParentEventHandler AdviseParent;

        public Form2(string txt)
        {
            InitializeComponent();
            radioButton1.Checked = true; //data order default: horizontal
            label3.Enabled = false; //for default horizontal order cycle period is 1
            numericUpDown1.Enabled = false;
            textBox1.Text = txt; //COM port message snapshot

        }

        //pass values to the main window
        public void SetValuesOnMainForm(char[] sep, bool VertOrHor, int CycleSize, bool[] mask)
        {
            if (AdviseParent != null)
                AdviseParent(sep, VertOrHor, CycleSize, mask);
        }

        private void button1_Click(object sender, EventArgs e) // button Apply
        {
            bool[] mask = new bool[20];
            char[] sep;
            bool VertOrHor; //true - horizontal, false - veritcal
            int CycleSize = 1; //default for horizontal
            if (radioButton1.Checked)
            { VertOrHor = true; }
            else { VertOrHor = false; }

            sep = textBox2.Text.ToCharArray();
            CycleSize = Convert.ToInt32(numericUpDown1.Value);

            //read the mask
            for (int i=0; i<dataGridView1.ColumnCount-1; i++)
            {
                if (dataGridView1.Rows[1].Cells[i].Value.ToString()=="1")
                { mask[i] = true; }
                else { mask[i] = false; }
            }
            SetValuesOnMainForm(sep, VertOrHor, CycleSize, mask);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e) //split button
        {
            char[] sep;
            string[] splitted=new string[50];
            int n = 0;
            int i;
            sep = textBox2.Text.ToCharArray();
            dataGridView1.Columns.Clear();
            if (textBox1.Text.IndexOfAny(sep) != -1)
            {
                try
                {
                    splitted = textBox1.Text.Split(sep);
                    n = splitted.Length - 1;
                    //display the splitted values
                    dataGridView1.Columns.Add("V0", "V0");
                    dataGridView1.Rows.Add(2);
                    dataGridView1.Rows[0].HeaderCell.Value = "Value";
                    dataGridView1.Rows[1].HeaderCell.Value = "Mask";
                    i = 0;
                    foreach (string s in splitted)
                    {
                        
                        dataGridView1.Rows[0].Cells[i].Value = splitted[i];
                        dataGridView1.Rows[1].Cells[i].Value = 0;
                        i++;
                        dataGridView1.Columns.Add("V" + i.ToString(), "V" + i.ToString());
                        
                    }
                    
                }
                catch (Exception ex)
                {
                    File.AppendAllText("app_log.txt", Environment.NewLine + DateTime.Now.ToString() + " " + ex.ToString());
                    MessageBox.Show("Can't split the initial string." + Environment.NewLine + "Source: " + ex.ToString(), "COM port parser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Can't find separator in string", "COM port parser", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e) //change vertical / horizontal order
        {
            if (radioButton2.Checked) // vertical order
            {
                label3.Enabled = true;
                numericUpDown1.Enabled = true;
            }
            else //horizontal order
            {
                label3.Enabled =false;
                numericUpDown1.Enabled =false;
            }
        }

        private void button3_Click(object sender, EventArgs e) //cancel button
        {
            this.Hide();
        }
    }
}
