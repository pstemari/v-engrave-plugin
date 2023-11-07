using System;
using System.Windows.Forms;
using CamBam.Util;

namespace VEngraveForCamBam
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PopulateForm();
        }

        private void PopulateForm()
        {
            LogCheck.Checked = Params.isMessages ? true : false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Params.isMessages = LogCheck.Checked ? true : false;
        }

        private double Text2Double(string value)
        {
            double result;
            try
            {
                result = Convert.ToDouble(value);
                return result;
            }
            catch (FormatException)
            {
                if (value == "") return 0;
                MessageBox.Show(TextTranslation.Translate("Not a valid number format"));
                return 0;
            }
            catch (OverflowException)
            {
                MessageBox.Show(TextTranslation.Translate("Not a valid number format"));
                return 0;
            }
        }
    }
}
