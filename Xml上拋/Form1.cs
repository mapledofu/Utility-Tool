using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
namespace Xml上拋
{
    public partial class Form1 : Form
    {
        public Form1() { InitializeComponent(); }
        XmlDocument xmlDocument = new XmlDocument();
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Xml上拋";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    xmlDocument.Load(file);
                    var itemsNode = xmlDocument.SelectSingleNode("BARCODE_SYSTEM");

                    //RadioButton radioBtn = this.groupBox1.Controls.OfType<RadioButton>()
                    //                   .Where(x => x.Checked).FirstOrDefault();

                    if (textBox6 != null)
                    {
                        if (itemsNode.SelectSingleNode("Test_mode") == null)
                        {
                            XmlElement Test_mode = xmlDocument.CreateElement(Name = "Test_mode");
                            Test_mode.InnerText = textBox6.Text;
                            var OpNO = itemsNode.SelectSingleNode("OpNO");
                            itemsNode.InsertAfter(Test_mode, OpNO);
                        }
                        else
                        {
                            itemsNode.SelectSingleNode("Test_mode").InnerText = textBox6.Text;
                        }
                            
                        xmlDocument.Save(file);
                    }

                    string xmlfile = file;
                    string xmlfilename = Path.GetFileName(file);
                    label2.Text = "Uploading ...";
                    UploadFile(textBox_j750_summary.Text, textBox1.Text, xmlfilename, xmlfile, textBox2.Text, textBox4.Text);
                    UploadFile(textBox_barcode_backup.Text, textBox1.Text, xmlfilename, xmlfile, textBox3.Text, textBox5.Text);
                }
            }
            label2.Text = "finish";
        }

        /// <summary>
        /// Uploads a file to the specified upload path using FTP.
        /// </summary>
        /// <param name="uploadPath">The FTP upload path.</param>
        /// <param name="subfolder">The subfolder within the upload path.</param>
        /// <param name="xmlfilename">The name of the XML file.</param>
        /// <param name="xmlfile">The path of the XML file.</param>
        /// <param name="userName">The FTP username.</param>
        /// <param name="password">The FTP password.</param>
        private void UploadFile(string uploadPath, string subfolder, string xmlfilename, string xmlfile, string userName, string password)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(userName, password);
                    string uploadFilePath = Path.Combine(uploadPath, subfolder, xmlfilename);
                    webClient.UploadFile(uploadFilePath, "STOR", xmlfile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string rootDirectory = Environment.CurrentDirectory;
            Process.Start(rootDirectory);
        }
    }
}
