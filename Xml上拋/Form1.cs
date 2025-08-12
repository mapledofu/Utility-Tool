using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
namespace Xml上拋
{
    public partial class Form1 : Form
    {
        public Form1() { InitializeComponent(); }
        XmlDocument xmlDocument = new XmlDocument();
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "工具小程式";
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

                    if(LBC_checkBox.Checked == true)
                    {
                        if (itemsNode.SelectSingleNode("LBC1_DATA") == null)
                        {
                            XmlElement LBC1_DATA = xmlDocument.CreateElement(Name = "LBC1_DATA");
                            LBC1_DATA.InnerText = "1";
                            itemsNode.AppendChild(LBC1_DATA);
                            XmlElement LBC2_DATA = xmlDocument.CreateElement(Name = "LBC2_DATA");
                            LBC2_DATA.InnerText = "1";
                            itemsNode.AppendChild(LBC2_DATA);
                        }
                        else
                        {
                            itemsNode.SelectSingleNode("LBC1_DATA").InnerText = "1";
                            itemsNode.SelectSingleNode("LBC2_DATA").InnerText = "1";
                        }          
                    }
                    if (RRT_checkBox.Checked == true)
                    {
                        var RRT_flag = itemsNode.SelectSingleNode("RRT_flag");
                        RRT_flag.InnerText = "TRUE";
                    }
                    if (RTBin_checkBox.Checked == true)
                    {
                        var RT_Bin = itemsNode.SelectSingleNode("RT_Bin");
                        RT_Bin.InnerText = RTBin_textBox.Text;
                        if (itemsNode.SelectSingleNode("ANF_RT_Bin") != null)
                        {
                            var ANF_RT_Bin = itemsNode.SelectSingleNode("ANF_RT_Bin");
                            ANF_RT_Bin.InnerText = RTBin_textBox.Text;
                        }    
                    }
                    if (RT_Frequency_checkBox.Checked == true)
                    {
                        var RT_Frequency = itemsNode.SelectSingleNode("RT_Frequency");
                        RT_Frequency.InnerText = RT_Frequency_textBox.Text;
                    }

                    if (textBox6 != null)//F278
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
                    if(checkBox_FTP.Checked == true)
                    {
                        UploadFile(textBox_j750_summary_FTP.Text, textBox1.Text, xmlfilename, xmlfile, textBox2.Text, textBox4.Text);
                        UploadFile(textBox_barcode_backup_FTP.Text, textBox1.Text, xmlfilename, xmlfile, textBox3.Text, textBox5.Text);
                    }
                    else
                    {
                        UploadFile(textBox_j750_summary.Text, textBox1.Text, xmlfilename, xmlfile, textBox2.Text, textBox4.Text);
                        UploadFile(textBox_barcode_backup.Text, textBox1.Text, xmlfilename, xmlfile, textBox3.Text, textBox5.Text);
                    }
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
                    //string uploadFilePath = Path.Combine($"ftp:{uploadPath}{subfolder}/{xmlfilename}").Replace("\\", "/");
                    webClient.UploadFile(uploadFilePath, xmlfile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void DownloadFile(string ftpPath, string localPath, string fileName, string userName, string password)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(userName, password);
                    string ftpFilePath = Path.Combine(ftpPath, fileName).Replace("\\", "/"); ;
                    string localFilePath = Path.Combine(localPath, fileName);

                    webClient.DownloadFile(ftpFilePath, localFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Download failed: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string rootDirectory = Environment.CurrentDirectory;
            Process.Start(rootDirectory);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }
        void ftp(string ftpaddress, string RunCard)
        {
            try
            {
                string FTP_Data_Download_Address = "";
                string FtpAccount = "j750";
                string FtpPassword = "j750";
                string zipfilename = "";
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpaddress);
                ftpRequest.Credentials = new NetworkCredential(FtpAccount, FtpPassword);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());

                List<string> directories = new List<string>();//FTP紀錄需要下載的檔案名稱

                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    if (line.Contains(RunCard) && line.Contains(FT_Station_comboBox.Text) && !line.Contains("CORR") && !line.Contains("HW"))
                    {
                        directories.Add(line);
                        var temp = line.Split('_');
                        zipfilename = $"{temp[0]}_{temp[1]}_{FT_Station_comboBox.Text}_Summary.zip";
                    }
                    line = streamReader.ReadLine();
                }
                streamReader.Close();
                string LocalAdress = System.Environment.CurrentDirectory + "\\" + RN_textBox.Text + "\\";
                if (directories.Count() != 0)
                {
                    int i = 0;
                    string userName = "j750";
                    string password = "j750";
                    
                    CreateFolder(LocalAdress);
                    FTP_Data_Download_Address = LocalAdress+"\\";

                    foreach (var e in directories)
                    {
                        //FTP下載檔案
                        DownloadFile(ftpaddress, LocalAdress, e, userName, password);
                        i++;
                    }  
                }
                // 壓縮成一個 zip 檔案
                string zipPath = Path.Combine(System.Environment.CurrentDirectory + "\\", zipfilename);
                if (File.Exists(zipPath)) File.Delete(zipPath); // 若已存在則刪除
                ZipFile.CreateFromDirectory(LocalAdress, zipPath);
                //MessageBox.Show("壓縮完成：" + zipPath);
                Directory.Delete(LocalAdress, true);

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }
        void CreateFolder(string address)
        {
            try
            {
                Directory.CreateDirectory(address+ "\\");
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string rootDirectory = Environment.CurrentDirectory;
            Process.Start(rootDirectory);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string CustomID;
            string RunCard;

            CustomID = RN_textBox.Text.Substring(1, 4) + "/";
            RunCard = RN_textBox.Text;
            ftp(@"ftp://172.17.6.212/SUM/" + CustomID, RunCard);
        }
    }
}
