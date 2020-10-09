using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuicklyUICode
{
    public partial class Form1 : Form
    {
        public string componentCodeStr;
        public List<string> ComponentList = new List<string>();
        public Form1()
        {
            InitializeComponent();
            textBox2.Text = "";
            textBox3.Text = "";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreatCode(sender, e);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void 选择代码生成路径ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void 选择文件生成路径ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();     //显示选择文件对话框
            openFileDialog1.InitialDirectory = "D:\\alienbrainWork\\works\\AFK\\Developer\\Client\\xgame_project\\src\\xGame\\UI\\fairyui";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var path = openFileDialog1.FileName;
                componentCodeStr = RedTxtStr(path);

                //移除空格换行符等
                //componentCodeStr = componentCodeStr.Trim();
                //componentCodeStr.Replace(" ", "");
                string myString = "  this\n is\r a \ttest   ";
                componentCodeStr = Regex.Replace(componentCodeStr, @"\s", "");


                string[] strArray = new string[] { "namespace" };
                string[] strs = componentCodeStr.Split(strArray, StringSplitOptions.RemoveEmptyEntries);
                string packageName = strs[1].Split('{')[0].Trim();
                strArray = new string[] { "class" };
                strs = componentCodeStr.Split(strArray, StringSplitOptions.RemoveEmptyEntries);
                string componentName = strs[2].Split('{')[0].Trim();
                textBox3.Text = (packageName + "/" + componentName);
                string className = componentName[0].ToString().ToUpper() + componentName.Remove(0, 1) + "Panel";
                textBox2.Text = className;

                ComponentList.Clear();
                MidStrEx(componentCodeStr, "GComponent", ";", ComponentList);
            }
        }

        public static void MidStrEx(string sourse, string startstr, string endstr, List<string> resList)
        {
            if (sourse == null || !sourse.Contains(startstr) || !sourse.Contains(endstr))
            {
                return;
            }

            string result = string.Empty;
            int startindex = -1, endindex = -1;
            try
            {
                startindex = sourse.IndexOf(startstr);
                if (startindex == -1)
                    goto Finish;
                string tmpstr = sourse.Substring(startindex + startstr.Length);
                endindex = tmpstr.IndexOf(endstr);
                if (endindex == -1)
                    goto Finish;
                result = tmpstr.Remove(endindex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("MidStrEx Err:" + ex.Message);
            }

            Finish:
            {
                if (result != string.Empty)
                {
                    resList.Add(result);
                    var newSourse =  sourse.Remove(startindex, endindex + endstr.Length);
                    MidStrEx(newSourse, startstr, endstr, resList);
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
