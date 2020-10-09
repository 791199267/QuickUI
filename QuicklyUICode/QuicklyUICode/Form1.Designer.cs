using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QuicklyUICode
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public void From1()
        {

        }

        private void CreatCode(object sender, EventArgs e)
        {
            string createPath = textBox1.Text + "\\" + textBox2.Text + ".cs";
            string packName = textBox3.Text.Split('/')[0];
            string uiName = textBox3.Text.Split('/')[1];
            if (createPath == "" || packName == "" || uiName == "")
            {
                MessageBox.Show("参数不全");
                return;
            }
            string basePath = System.AppDomain.CurrentDomain.BaseDirectory;
            var nameSpaceStr = Regex.Split(createPath, "src", RegexOptions.IgnoreCase)[1];
            string[] strArray = nameSpaceStr.Split('\\');
            nameSpaceStr = "Hotfix";
            for (int i = 0; i < strArray.Length - 1; i++)
            {
                if (i != strArray.Length - 2)
                {
                    nameSpaceStr += (strArray[i] + ".");
                }
                else
                {
                    nameSpaceStr += strArray[i];
                }
            }
            string className = strArray[strArray.Length - 1].Split('.')[0];
            //代码编译单元
            CodeCompileUnit codeUnit = new CodeCompileUnit();
            //命名空间
            CodeNamespace codeNamespace = new CodeNamespace(nameSpaceStr);
            //导入引用
            codeNamespace.Imports.Add(new CodeNamespaceImport("FairyGUI"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("Hotfix.Framework"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            //codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            //codeNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            //codeNamespace.Imports.Add(new CodeNamespaceImport("System.Text"));
            //定义类
            CodeTypeDeclaration classUnit = new CodeTypeDeclaration(className);
            classUnit.IsClass = true;
            classUnit.TypeAttributes = TypeAttributes.Public;
            var parentClass = new CodeTypeReference();
            if (checkBox2.Checked)
            {
                classUnit.BaseTypes.Add(new CodeTypeReference("DialogBase"));
            }
            else
            {
                classUnit.BaseTypes.Add(new CodeTypeReference("FairyUIBase"));
            }

            //类，命名空间，编译单元相关联
            codeNamespace.Types.Add(classUnit);
            codeUnit.Namespaces.Add(codeNamespace);

            //字段
            CodeMemberField codeMemberField = new CodeMemberField();
            codeMemberField.Name = "_root";
            codeMemberField.Attributes = MemberAttributes.Private;
            codeMemberField.Type = new CodeTypeReference(packName + "." + uiName);
            classUnit.Members.Add(codeMemberField);

            //方法
            //构造方法
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            string statementStr = string.Format(@"setPackName(""{0}"", ""{1}"");", packName, uiName);
            CodeSnippetStatement ass = new CodeSnippetStatement(statementStr);
            constructor.Statements.Add(ass);
            classUnit.Members.Add(constructor);

            statementStr = string.Format(@"_root = {0}.{1}.CreateInstanceEx(_view_ex);", packName, uiName);
            var method2 = creatCodeMethod("onCreateView", statementStr, MemberAttributes.Public | MemberAttributes.Override);
            classUnit.Members.Add(method2);

            statementStr = string.Format(@"base.onShow();");
            var method3 = creatCodeMethod("onShow", statementStr, MemberAttributes.Public | MemberAttributes.Override);
            classUnit.Members.Add(method3);

            statementStr = string.Format(@"base.onHide();");
            var method4 = creatCodeMethod("onHide", statementStr, MemberAttributes.Public | MemberAttributes.Override);
            classUnit.Members.Add(method4);

            List<string> onOffEventStrList = getOnOffEventStrList();
            bool autoEvent = checkBox1.Checked;

            //事件注册 反注册 方法
            statementStr = string.Format(@"base.onEvent();");
            if (autoEvent && onOffEventStrList[0] != "") statementStr += "\r\n" + onOffEventStrList[0];
            var method5 = creatCodeMethod("onEvent", statementStr, MemberAttributes.Public | MemberAttributes.Override);
            classUnit.Members.Add(method5);

            statementStr = string.Format(@"base.offEvent();");
            if (autoEvent && onOffEventStrList[1] != "") statementStr += "\r\n" + onOffEventStrList[1];
            var method6 = creatCodeMethod("offEvent", statementStr, MemberAttributes.Public | MemberAttributes.Override);
            classUnit.Members.Add(method6);

            var btnNameList = getBtnNameList();
            if (btnNameList.Count > 0 && autoEvent)
            {
                statementStr = "";
                statementStr += "GObject sender = (GObject)context.sender;";
                statementStr += "\r\n";
                for (int i = 0; i < btnNameList.Count; i++)
                {
                    if (i == 0)
                    {
                        statementStr += "if (sender == _root." + btnNameList[i] + ")";
                    }
                    else
                    {
                        statementStr += "else if (sender ==  _root." + btnNameList[i] + ")";
                    }
                    statementStr += "\r\n{\r\n}\r\n";
                }
                var method7 = creatCodeMethod("onClickbtn", statementStr, MemberAttributes.Public);
                CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression("EventContext", "context");
                method7.Parameters.Add(expression);
                classUnit.Members.Add(method7);
            }

            if (File.Exists(createPath))
            {
                if (MessageBox.Show("文件已经存在是否覆盖?", "此操作不可恢复", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CreatFile(createPath, codeUnit);
                }
            }
            else
            {
                CreatFile(createPath, codeUnit);
            }
        }


        private CodeMemberMethod creatCodeMethod(string methodName, string statementStr, MemberAttributes memberAttributes = MemberAttributes.Public | MemberAttributes.Final)
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodName;
            method.Attributes = memberAttributes;
            CodeSnippetStatement ass = new CodeSnippetStatement(statementStr);
            method.Statements.Add(ass);
            return method;
        }

        private List<string> getBtnNameList()
        {
            List<string> btnComponentList = new List<string>();
            for (int i = 0; i < ComponentList.Count; i++)
            {
                if (ComponentList[i].Length > 3)
                {
                    var prefixStr = ComponentList[i][0].ToString() + ComponentList[i][1].ToString() + ComponentList[i][2].ToString();
                    if (prefixStr == "btn")
                    {
                        btnComponentList.Add(ComponentList[i]);
                    }
                }
            }
            return btnComponentList;
        }

        private List<String> getOnOffEventStrList()
        {
            List<string> resList = new List<string>();
            var btnComponentList = getBtnNameList();
            string onEvent = "";
            string offEvent = "";
            for (int i = 0; i < btnComponentList.Count; i++)
            {
                if (i != btnComponentList.Count - 1)
                {
                    onEvent += "_root." + btnComponentList[i] + ".onClick.Add(onClickbtn);\r\n";
                    offEvent += "_root." + btnComponentList[i] + ".onClick.Remove(onClickbtn);\r\n";
                }
                else
                {
                    onEvent += "_root." + btnComponentList[i] + ".onClick.Add(onClickbtn);";
                    offEvent += "_root." + btnComponentList[i] + ".onClick.Remove(onClickbtn);";
                }
            }
            resList.Add(onEvent);
            resList.Add(offEvent);
            return resList;
        }

        private void CreatFile(string createPath, CodeCompileUnit unit)
        {
            FileStream fs = new FileStream(createPath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            options.BlankLinesBetweenMembers = true;
            provider.GenerateCodeFromCompileUnit(unit, sw, options);

            sw.Close();
            fs.Close();
            MessageBox.Show("生成成功!");
        }
        public static string RedTxtStr(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("文件路径不存在:" + path);
                return "";
            }

            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.Default);
            string res = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            return res;
        }

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.选择代码生成路径ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.选择文件生成路径ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(64, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "生成代码路径";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(147, 44);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(218, 21);
            this.textBox1.TabIndex = 2;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(147, 105);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(218, 21);
            this.textBox2.TabIndex = 4;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(64, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "类名";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(147, 145);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(218, 21);
            this.textBox3.TabIndex = 6;
            this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(64, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "包/组件名字";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(147, 191);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 36);
            this.button1.TabIndex = 7;
            this.button1.Text = "生成代码";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.SelectedPath = "D:\\alienbrainWork\\works\\AFK\\Developer\\Client\\xgame_project\\src\\xGame\\UI\\modules";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.选择代码生成路径ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(434, 25);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // 选择代码生成路径ToolStripMenuItem
            // 
            this.选择代码生成路径ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.选择文件生成路径ToolStripMenuItem,
            this.toolStripTextBox1});
            this.选择代码生成路径ToolStripMenuItem.Name = "选择代码生成路径ToolStripMenuItem";
            this.选择代码生成路径ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.选择代码生成路径ToolStripMenuItem.Text = "编辑";
            this.选择代码生成路径ToolStripMenuItem.Click += new System.EventHandler(this.选择代码生成路径ToolStripMenuItem_Click);
            // 
            // 选择文件生成路径ToolStripMenuItem
            // 
            this.选择文件生成路径ToolStripMenuItem.Name = "选择文件生成路径ToolStripMenuItem";
            this.选择文件生成路径ToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.选择文件生成路径ToolStripMenuItem.Text = "选择文件生成路径";
            this.选择文件生成路径ToolStripMenuItem.Click += new System.EventHandler(this.选择文件生成路径ToolStripMenuItem_Click);
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(172, 22);
            this.toolStripTextBox1.Text = "选择组件源代码";
            this.toolStripTextBox1.Click += new System.EventHandler(this.toolStripTextBox1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(314, 223);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(120, 16);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "组件自动绑定方法";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(314, 191);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(84, 16);
            this.checkBox2.TabIndex = 10;
            this.checkBox2.Text = "DialogBase";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 261);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UI代码快速生成";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private FolderBrowserDialog folderBrowserDialog1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem 选择代码生成路径ToolStripMenuItem;
        private ToolStripMenuItem 选择文件生成路径ToolStripMenuItem;
        private CheckBox checkBox1;
        private ToolStripMenuItem toolStripTextBox1;
        private CheckBox checkBox2;
    }
}

