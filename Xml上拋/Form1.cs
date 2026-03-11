using FluentFTP;
using FluentFTP.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
namespace Xml上拋
{
    public partial class Form1 : Form
    {
        public Form1() { InitializeComponent(); }
        XmlDocument xmlDocument = new XmlDocument();

        public class IniFile
        {
            public string Path;

            public IniFile(string path)
            {
                Path = path;
            }

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            private static extern int GetPrivateProfileString(
                string section, string key, string defaultValue,
                StringBuilder retVal, int size, string filePath);

            public string Read(string section, string key, string defaultValue = "")
            {
                var sb = new StringBuilder(1024);
                GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, Path);
                return sb.ToString();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "工具小程式";

            string iniPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "config.ini"
                );

            IniFile ini = new IniFile(iniPath);
            this.textBox_j750_summary_FTP.Text = ini.Read("MTK_auto_load_E750", "Ftp", "");
            this.textBox2.Text = ini.Read("MTK_auto_load_E750", "Account", "");
            this.textBox4.Text = ini.Read("MTK_auto_load_E750", "Password", "");

            this.textBox_barcode_backup_FTP.Text = ini.Read("barcode_backup", "Ftp", "");
            this.textBox3.Text = ini.Read("barcode_backup", "Account", "");
            this.textBox5.Text = ini.Read("barcode_backup", "Password", "");
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
                        if (itemsNode.SelectSingleNode("RT_Frequency") == null)
                        {
                            XmlElement RT_F = xmlDocument.CreateElement(Name = "RT_Frequency");
                            RT_F.InnerText = RT_Frequency_textBox.Text;

                            itemsNode.AppendChild(RT_F);
                        }
                        else
                        {
                            var RT = itemsNode.SelectSingleNode("RT_Frequency");
                            RT.InnerText = RT_Frequency_textBox.Text;
                        }
                    }

                    xmlDocument.Save(file);
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
            PGM_Loot();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }


        public void DownloadAndZipWithFluentFtp(
                string ftpAddress,
                string runCard,
                string rnFolderName,       // 對應 RN_textBox.Text
                string ftpAccount,
                string ftpPassword,
                bool useFtps = false,      // 若要 FTPS，改 true
                FtpDataConnectionType dataConnType = FtpDataConnectionType.AutoPassive,
                int connectTimeoutMs = 15000,
                int readWriteTimeoutMs = 30000,
                Encoder serverEncoding = null // 若伺服器用特定編碼(如 BIG5/GBK)，可傳入
            )
        {
            try
            {
                // 建立暫存資料夾：<CurrentDir>\<RN_textBox.Text>\
                var baseDir = Environment.CurrentDirectory;
                var localWorkDir = Path.Combine(baseDir, rnFolderName);
                Directory.CreateDirectory(localWorkDir);

                // zip 檔名：從第一個符合的檔名取得 prefix0、prefix1
                string zipFilename = string.Empty;

                // 設定 FTP Client
                var creds = new NetworkCredential(ftpAccount, ftpPassword);

                var config = new FtpConfig
                {
                    DataConnectionType = dataConnType,
                    ConnectTimeout = connectTimeoutMs,
                    ReadTimeout = readWriteTimeoutMs,
                    RetryAttempts = 3,
                    //Encoding = UTF8, // 依需要調整
                    ValidateAnyCertificate = true // 若內網/自簽；若正式站建議改為嚴格驗證
                };

                var client = new FtpClient(new Uri(ftpAddress).Host, creds)
                {
                    Config = config
                };

                //if (useFtps)
                //{
                //    client.EncryptionMode = FtpEncryptionMode.Explicit; // 或 Implicit 視伺服器而定
                //    client.SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                //                         | System.Security.Authentication.SslProtocols.Tls13;
                //}

                // 連線
                client.Connect();

                // 列目錄（根目錄或指定子路徑）
                string remotePath = new Uri(ftpAddress).AbsolutePath; // e.g. "/" or "/subdir"
                if (string.IsNullOrWhiteSpace(remotePath)) remotePath = "/";
                var items = client.GetListing(remotePath);

                // 篩選檔案名稱 (RunCard + station 且不含 CORR/HW)
                var filtered = new List<FtpListItem>();
                foreach (var it in items)
                {
                    if (it.Type == FtpObjectType.File)
                    {
                        var name = it.Name ?? string.Empty;
                        if (name.Contains(runCard)
                            && name.Contains(FT_Station_comboBox.Text)
                            && !name.Contains("CORR")
                            && !name.Contains("HW"))
                        {
                            filtered.Add(it);

                            // 建立 zip 檔名：取第一個符合的檔案
                            if (string.IsNullOrEmpty(zipFilename))
                            {
                                var parts = name.Split('_');
                                if (parts.Length >= 2)
                                {
                                    zipFilename = $"{parts[0]}_{parts[1]}_{FT_Station_comboBox.Text}_Summary.zip";
                                }
                                else
                                {
                                    // 後備：無法拆出前兩段就直接用 station
                                    zipFilename = $"{runCard}_{FT_Station_comboBox.Text}_Summary.zip";
                                }
                            }
                        }
                    }
                }

                if (filtered.Count == 0)
                {
                    // 沒有符合就直接結束
                    MessageBox.Show("未找到符合條件的檔案。");
                    return;
                }

                // 逐一下載到暫存資料夾
                foreach (var fi in filtered)
                {
                    var localPath = Path.Combine(localWorkDir, fi.Name);
                    client.DownloadFile(localPath, fi.FullName, FtpLocalExists.Overwrite, FtpVerify.Retry);
                }

                // 壓成 zip
                if (string.IsNullOrEmpty(zipFilename))
                {
                    zipFilename = $"{runCard}_{FT_Station_comboBox.Text}_Summary.zip";
                }
                var zipPath = Path.Combine(baseDir, zipFilename);
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(localWorkDir, zipPath);

                // 刪除暫存資料夾
                Directory.Delete(localWorkDir, true);

                MessageBox.Show($"壓縮完成：{zipPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        void ftp(string ftpaddress, string RunCard, string FtpAccount, string FtpPassword)
        {
            try
            {
                string FTP_Data_Download_Address = "";
                string zipfilename = "";

                //var baseUrl = "ftp://172.17.3.28/%2Fusr/local/home/vol4/D10/prod/autoResult/summary/";
                //var baseUri = new Uri(baseUrl);
                //var basePath = Uri.UnescapeDataString(baseUri.AbsolutePath);    // "/usr/local/.../summary/"
                //if (!basePath.EndsWith("/")) basePath += "/";
                //var fullPath = basePath + "L176/";                              // "/usr/local/.../summary/L176/"
                //var newUri = new Uri($"ftp://{baseUri.Host}{(baseUri.IsDefaultPort ? "" : ":" + baseUri.Port)}{fullPath}");

                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpaddress);
                ftpRequest.Credentials = new NetworkCredential(FtpAccount, FtpPassword);

                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory; // NLST，比 LIST 更簡單
                ftpRequest.UsePassive = true;
                ftpRequest.UseBinary = true;
                ftpRequest.KeepAlive = false;
                ftpRequest.Timeout = 8000;
                ftpRequest.ReadWriteTimeout = 20000;

                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());

                List<string> directories = new List<string>();//FTP紀錄需要下載的檔案名稱

                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var lineSplit = line.Split('_');
                    if (lineSplit[0] == RunCard && line.Contains(FT_Station_comboBox.Text) && !line.Contains("CORR") && !line.Contains("HW"))
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
                    
                    CreateFolder(LocalAdress);
                    FTP_Data_Download_Address = LocalAdress+"\\";

                    foreach (var e in directories)
                    {
                        //FTP下載檔案
                        DownloadFile(ftpaddress, LocalAdress, e, FtpAccount, FtpPassword);
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
            PGM_Loot();
        }

        void PGM_Loot()
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

            switch (CustomID)
            {
                case "L401/":
                    ftp(@"ftp://172.17.6.212/SUM/" + CustomID, RunCard,"j750", "j750");
                    break;

                case "L176/":
                    ftp(@"ftp://172.17.3.28/%2Fusr/local/home/vol4/D10/prod/autoResult/summary/" + CustomID, RunCard, "eng", "eng1812");

                    //DownloadAndZipWithFluentFtp(@"ftp://172.17.3.28/%2Fusr/local/home/vol4/D10/prod/autoResult/summary/" + CustomID, RunCard, RunCard, "eng", "eng1812");
                    break;

                default:
                    break;
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            string gzfile = "";
            string xmlfile = "";
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories);
            XmlDocument xmlDocument = new XmlDocument();
            foreach (string file in files)
            {
                if (file.Contains(".gz"))
                {
                    gzfile = Path.GetFileName(file);
                }
                //if (file.Contains(".XML") || file.Contains(".xml"))
                //{
                //    xmlfile = Path.GetFileName(file);
                //    xmlDocument.Load(file);
                //}
            }
            string SubConLot = "";
            string ProgFile_DeviceVariant_TestMode = "";
            string DialogLot = "";
            string LotDateCode = "", WaferNo = "";
            string Tester = "";
            string Date = "";
            string time = "";
            var itemsNode = xmlDocument.SelectSingleNode("BARCODE_SYSTEM");

            if (radioButtonFT.Checked)
            {
                SubConLot = gzfile.Split('_')[5];
                ProgFile_DeviceVariant_TestMode = gzfile.Split('_')[6].Split('.')[0];
                DialogLot = gzfile.Split('_')[1];
                //LotDateCode = itemsNode.SelectSingleNode("DateCode").InnerText;
                //Tester = itemsNode.SelectSingleNode("TesterID").InnerText;
                LotDateCode = DateCode_textBox.Text;
                Tester = Tester_textBox.Text;
                Date = gzfile.Split('_')[0].Substring(6, 2) + "-" + gzfile.Split('_')[0].Substring(4, 2) + "-" + gzfile.Split('_')[0].Substring(2, 2);
                time = gzfile.Split('_')[0].Substring(8, 2) + "-" + gzfile.Split('_')[0].Substring(10, 2);

                FT_DecompressFile();
                FT_CompressFile();
                File.Delete(Environment.CurrentDirectory + "\\" + SubConLot + "#" + ProgFile_DeviceVariant_TestMode
                          + "#" + DialogLot + "#" + LotDateCode + "#" + Tester + "#" + Date + "#" + time + ".std");
                label1.Text = "Done";
            }
            if (radioButtonCP.Checked)
            {
                SubConLot = gzfile.Split('_')[4];
                ProgFile_DeviceVariant_TestMode = gzfile.Split('_')[5].Split('.')[0];
                DialogLot = gzfile.Split('_')[1];
                WaferNo = gzfile.Split('_')[3].PadLeft(2, '0');
                //Tester = itemsNode.SelectSingleNode("TesterID").InnerText;
                Tester = Tester_textBox.Text;
                Date = gzfile.Split('_')[0].Substring(6, 2) + "-" + gzfile.Split('_')[0].Substring(4, 2) + "-" + gzfile.Split('_')[0].Substring(2, 2);
                time = gzfile.Split('_')[0].Substring(8, 2) + "-" + gzfile.Split('_')[0].Substring(10, 2);

                CP_DecompressFile();
                CP_CompressFile();

                File.Delete(Environment.CurrentDirectory + "\\" + SubConLot + "#" + ProgFile_DeviceVariant_TestMode
                          + "#" + DialogLot + "#" + WaferNo + "#" + Tester + "#" + Date + "#" + time + ".std");
                label1.Text = "Done";
            }


            void FT_CompressFile()
            {
                string RenameFile = "";

                RenameFile = Environment.CurrentDirectory + "\\" + SubConLot + "#" + ProgFile_DeviceVariant_TestMode
                          + "#" + DialogLot + "#" + LotDateCode + "#" + Tester + "#" + Date + "#" + time + ".std";
                using (FileStream originalFileStream = File.OpenRead(RenameFile))
                using (FileStream compressedFile = File.Create(SubConLot + "#" + ProgFile_DeviceVariant_TestMode
                          + "#" + DialogLot + "#" + LotDateCode + "#" + Tester + "#" + Date + "#" + time + ".std.gz"))
                using (var compressor = new GZipStream(compressedFile, CompressionMode.Compress))
                {
                    originalFileStream.CopyTo(compressor);
                }
            }

            void FT_DecompressFile()
            {
                FileStream compressedFileStream = File.Open(Environment.CurrentDirectory + "\\" + gzfile, FileMode.Open);
                FileStream outputFileStream = File.Create(Environment.CurrentDirectory + "\\" + SubConLot + "#" + ProgFile_DeviceVariant_TestMode
                                  + "#" + DialogLot + "#" + LotDateCode + "#" + Tester + "#" + Date + "#" + time + ".std");
                var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
                decompressor.CopyTo(outputFileStream);

                compressedFileStream.Close();
                outputFileStream.Close();
                decompressor.Close();
            }

            void CP_CompressFile()
            {
                string RenameFile = "";

                RenameFile = Environment.CurrentDirectory + "\\" + SubConLot + "#" + ProgFile_DeviceVariant_TestMode
                          + "#" + DialogLot + "#" + WaferNo + "#" + Tester + "#" + Date + "#" + time + ".std";
                using (FileStream originalFileStream = File.OpenRead(RenameFile))
                using (FileStream compressedFile = File.Create(SubConLot + "#" + ProgFile_DeviceVariant_TestMode
                          + "#" + DialogLot + "#" + WaferNo + "#" + Tester + "#" + Date + "#" + time + ".std.gz"))
                using (var compressor = new GZipStream(compressedFile, CompressionMode.Compress))
                {
                    originalFileStream.CopyTo(compressor);
                }
            }

            void CP_DecompressFile()
            {
                FileStream compressedFileStream = File.Open(Environment.CurrentDirectory + "\\" + gzfile, FileMode.Open);
                FileStream outputFileStream = File.Create(Environment.CurrentDirectory + "\\" + SubConLot + "#" + ProgFile_DeviceVariant_TestMode
                                  + "#" + DialogLot + "#" + WaferNo + "#" + Tester + "#" + Date + "#" + time + ".std");
                var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
                decompressor.CopyTo(outputFileStream);

                outputFileStream.Close();
                compressedFileStream.Close();
                decompressor.Close();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            PGM_Loot();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if(textBox6 != null)
            {
                MessageBox.Show("textBox6 != null");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            PGM_Loot();
        }

        public static class AngleBracketEscaper
        {
            /// <summary>
            /// 讀取 XML（自動偵測 UTF-16 LE/BE、UTF-8（含/不含 BOM）、Big5/CP950），
            /// 只將「內文/屬性值」中的 '<'、'>' 轉成實體 &lt;、&gt;，避免被誤判為標籤。
            /// - 不會改動真正的標籤（例如 <Tag ...>）
            /// - 會略過註解/CDATA/處理指令，不做轉換
            /// - 轉出為 UTF-8（無 BOM）
            /// </summary>
            public static void ConvertAnglesInTextAndAttr(string inputPath, string outputPath)
            {
                // 若有機會讀 Big5/CP950，.NET 6+ 需要註冊 CodePages
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var text = ReadAsUnicodeAuto(inputPath);
                var fixedText = EscapeAngles(text);

                File.WriteAllText(outputPath, fixedText, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }

            /// <summary>
            /// 依 BOM 自動判斷 UTF-16 LE/BE、UTF-8 BOM；無 BOM 則先試 UTF-8（嚴格），失敗再 Big5/CP950（嚴格）。
            /// </summary>
            private static string ReadAsUnicodeAuto(string path)
            {
                byte[] data = File.ReadAllBytes(path);

                // --- 1) 依 BOM 判斷 ---
                if (data.Length >= 2)
                {
                    // UTF-16 LE BOM: FF FE
                    if (data[0] == 0xFF && data[1] == 0xFE)
                        return Encoding.Unicode.GetString(data); // UTF-16 LE

                    // UTF-16 BE BOM: FE FF
                    if (data[0] == 0xFE && data[1] == 0xFF)
                        return Encoding.BigEndianUnicode.GetString(data); // UTF-16 BE
                }
                if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                {
                    // UTF-8 with BOM
                    return Encoding.UTF8.GetString(data);
                }

                // --- 2) 無 BOM：先試嚴格 UTF-8，失敗再 Big5/CP950（嚴格） ---
                var utf8Strict = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                try
                {
                    return utf8Strict.GetString(data);
                }
                catch
                {
                    // 台灣常見 Big5/CP950
                    var big5Strict = Encoding.GetEncoding(950, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
                    return big5Strict.GetString(data);
                }
            }

            /// <summary>
            /// 逐字掃描文字，僅在「非標籤內容」與「屬性值（引號內）」轉義 <、>。
            /// 保留標籤、註解、CDATA、處理指令的原貌。
            /// </summary>
            private static string EscapeAngles(string input)
            {
                var sb = new StringBuilder(input.Length + input.Length / 20);

                bool inTag = false;        // 是否在 <...> 之間
                bool inAttr = false;       // 標籤中且在屬性值（引號內）
                char attrQuote = '\0';     // 屬性值的引號 ' 或 "
                bool inComment = false;    // <!-- ... -->
                bool inCData = false;      // <![CDATA[ ... ]]>
                bool inPI = false;         // <? ... ?>

                int i = 0;
                int n = input.Length;

                while (i < n)
                {
                    // 小工具：檢查後續是否為指定字串
                    bool Ahead(string s)
                    {
                        if (i + s.Length > n) return false;
                        for (int k = 0; k < s.Length; k++)
                        {
                            if (input[i + k] != s[k]) return false;
                        }
                        return true;
                    }

                    // 1) 位於特殊區塊（Comment / CDATA / PI）
                    if (inComment)
                    {
                        if (Ahead("-->")) { sb.Append("-->"); i += 3; inComment = false; }
                        else { sb.Append(input[i++]); }
                        continue;
                    }
                    if (inCData)
                    {
                        if (Ahead("]]>")) { sb.Append("]]>"); i += 3; inCData = false; }
                        else { sb.Append(input[i++]); }
                        continue;
                    }
                    if (inPI)
                    {
                        if (Ahead("?>")) { sb.Append("?>"); i += 2; inPI = false; }
                        else { sb.Append(input[i++]); }
                        continue;
                    }

                    // 2) 標籤內（<tag ...> 或 </tag>）
                    if (inTag)
                    {
                        char c = input[i];

                        if (!inAttr)
                        {
                            if (c == '"' || c == '\'')
                            {
                                inAttr = true; attrQuote = c;
                                sb.Append(c); i++;
                                continue;
                            }
                            if (c == '>')
                            {
                                inTag = false;
                                sb.Append('>'); i++;
                                continue;
                            }
                            sb.Append(c); i++;
                            continue;
                        }
                        else // 在屬性值（引號內）
                        {
                            if (c == '<')
                            {
                                sb.Append("&lt;"); i++;
                                continue;
                            }
                            else if (c == '>')
                            {
                                sb.Append("&gt;"); i++;
                                continue;
                            }
                            else if (c == attrQuote)
                            {
                                inAttr = false; attrQuote = '\0';
                                sb.Append(c); i++;
                                continue;
                            }
                            else
                            {
                                sb.Append(c); i++;
                                continue;
                            }
                        }
                    }

                    // 3) 純文字內容區（不在標籤、也不在特殊區塊）
                    if (Ahead("<!--")) { sb.Append("<!--"); i += 4; inComment = true; continue; }
                    if (Ahead("<![CDATA[")) { sb.Append("<![CDATA["); i += 9; inCData = true; continue; }
                    if (Ahead("<?")) { sb.Append("<?"); i += 2; inPI = true; continue; }
                    if (Ahead("</")) { sb.Append("</"); i += 2; inTag = true; continue; }

                    if (input[i] == '<')
                    {
                        // 判斷是否為真正的標籤起點
                        if (i + 1 < n && IsNameStartChar(input[i + 1]))
                        {
                            sb.Append('<'); i++; inTag = true; continue;
                        }
                        else
                        {
                            // 內文中誤用 '<' → 轉義
                            sb.Append("&lt;"); i++; continue;
                        }
                    }
                    if (input[i] == '>')
                    {
                        // 內文中的 '>'（非必須，但保守轉義）
                        sb.Append("&gt;"); i++; continue;
                    }

                    // 對已存在的常見實體（&lt;、&gt;、&amp;、&quot;、&apos;）保持原樣
                    if (input[i] == '&')
                    {
                        if (Ahead("&lt;") || Ahead("&gt;") || Ahead("&amp;") || Ahead("&quot;") || Ahead("&apos;"))
                        {
                            int semi = input.IndexOf(';', i + 1);
                            if (semi > i)
                            {
                                sb.Append(input, i, semi - i + 1);
                                i = semi + 1;
                                continue;
                            }
                        }
                        // 不是已知實體 → 原樣保留
                        sb.Append('&'); i++;
                        continue;
                    }

                    // 其他一般字元
                    sb.Append(input[i]); i++;
                }

                return sb.ToString();
            }

            private static bool IsNameStartChar(char c)
            {
                // XML 名稱起始字元（簡化版）：字母 / '_' / ':'（簡化即可用於區分常見情況）
                return char.IsLetter(c) || c == '_' || c == ':';
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string root = Environment.CurrentDirectory;

            using (var dlg = new OpenFileDialog())
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Title = "選擇資料夾";
                    dialog.CheckFileExists = false;
                    dialog.CheckPathExists = true;
                    dialog.ValidateNames = false;
                    dialog.InitialDirectory = root;
                    dialog.FileName = "選擇此資料夾"; // 必須填，否則當成選檔案

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        root = Path.GetDirectoryName(dialog.FileName);
                    }
                }
            }

            string[] files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string file_ext = Path.GetExtension(file);
                if (!file_ext.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    continue;
                try
                {
                    AngleBracketEscaper.ConvertAnglesInTextAndAttr(file, file);

                    string xmlfile = File.ReadAllText(file, Encoding.GetEncoding("utf-8"));
                    if (Xml_SECN_radioButton.Checked)
                    {
                        xmlfile = File.ReadAllText(file, Encoding.GetEncoding("big5"));
                    }

                    xmlDocument.LoadXml(xmlfile);
                    ReadXML_WritetoClass(xmlfile, file.GetFtpFileName());
                }
                catch (System.Xml.XmlException ex)
                {
                    // Log the error or handle it accordingly
                    using (StreamWriter se = File.AppendText(Environment.CurrentDirectory + "\\" + "Error loading XML file.txt"))
                    {
                        se.WriteLine($"{file}': {ex.Message}");
                    }
                }
            }
            //out put to csv

            if (Xml_SECN_radioButton.Checked == true)
            {
                string fileName = "SECN_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                WriteAllProductsToCsv($"{root}\\{fileName}", true);
            }
            else
            {
                string fileName = "OutTest_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                WriteAllProductsToCsv($"{root}\\{fileName}", true);
            }
                
            MessageBox.Show("Done");
        }

        private static string GetInnerTextOrDefault(XmlNode parent, string xpath, string defaultValue = "")
        {
            var node = parent?.SelectSingleNode(xpath);
            return node?.InnerText ?? defaultValue;
        }

        public sealed class SecnDocument
        {
            public string Product { get; set; } = "";      // ← 你要作為 key 的欄位
            public string DocNo { get; set; } = "";
            public string FRelevance { get; set; } = "";
            public string F_TestSec { get; set; } = "";
            public string Temperature { get; set; } = "";
            public string BinRecycle { get; set; } = "";
            public string EQAText { get; set; } = "";
            public string Location { get; set; } = "";
        }

        public sealed class OutTestDocument
        {
            public string F_RTNo { get; set; } = "";      // ← 你要作為 key 的欄位
            public string FileName { get; set; } = "";
            public string F_Item5_T { get; set; } = "";
            public string F_LVM { get; set; } = "";
        }

        private readonly Dictionary<string, List<SecnDocument>> _byProduct =
                new Dictionary<string, List<SecnDocument>>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, List<OutTestDocument>> _OutTestList =
                new Dictionary<string, List<OutTestDocument>>(StringComparer.OrdinalIgnoreCase);

        private static string NormalizeKey(string s)
        {
            if (s == null) return "";
            return s.Trim();
        }

        private static int GetFtThirdDigitFromLocation(string location)
        {
            if (string.IsNullOrEmpty(location)) return int.MaxValue;

            string loc = location.Trim();
            int comma = loc.IndexOf(',');
            string first = comma >= 0 ? loc.Substring(0, comma).Trim() : loc;

            if (first.Length >= 3 && char.IsDigit(first[2]))
                return (int)(first[2] - '0');

            return int.MaxValue;
        }


        private readonly List<string> _productOrder = new List<string>();

        // 在你加入 _byProduct 的同時，維護順序（僅第一次出現時加入）
        private void EnsureProductOrder(string productKey)
        {
            for (int i = 0; i < _productOrder.Count; i++)
                if (string.Equals(_productOrder[i], productKey, StringComparison.OrdinalIgnoreCase))
                    return; // 已經存在，不重複加入

            _productOrder.Add(productKey);
        }

        void ReadXML_WritetoClass(string file, string filename)
        {
            if (Xml_SECN_radioButton.Checked == true)
            {
                var itemsNode = xmlDocument.SelectSingleNode("realtekSECN")
            ?? throw new InvalidOperationException("XML 缺少節點：realtekSECN");

                string DocNo = GetInnerTextOrDefault(itemsNode, "DocNo");
                string Product = GetInnerTextOrDefault(itemsNode, "Product");
                string FRelevance = GetInnerTextOrDefault(itemsNode, "FRelevance");
                string F_TestSec = GetInnerTextOrDefault(itemsNode, "F_TestSec");
                string Temperature = GetInnerTextOrDefault(itemsNode, "Temperature");
                string BinRecycle = GetInnerTextOrDefault(itemsNode, "BinRecycle");
                string EQAText = GetInnerTextOrDefault(itemsNode, "EQAText");
                string Location = GetInnerTextOrDefault(itemsNode, "Location");


                // 建議：對 key 做標準化（去空白、統一大小寫）
                string productKey = NormalizeKey(Product);

                var doc = new SecnDocument
                {
                    Product = Product,
                    DocNo = DocNo,
                    FRelevance = FRelevance,
                    F_TestSec = F_TestSec,
                    Temperature = Temperature,
                    BinRecycle = BinRecycle,
                    EQAText = EQAText,
                    Location = Location,
                };

                if (!_byProduct.TryGetValue(productKey, out var list))
                {
                    list = new List<SecnDocument>();
                    _byProduct[productKey] = list;
                    EnsureProductOrder(productKey);
                }

                // 可避免完全重覆（同 DocNo）：
                bool isDup = list.Any(x => x.DocNo == doc.DocNo);
                if (!isDup)
                    list.Add(doc);

                list.Sort((a, b) =>
                {
                    int kx = GetFtThirdDigitFromLocation(a.Location);
                    int ky = GetFtThirdDigitFromLocation(b.Location);
                    int cmp = kx.CompareTo(ky);
                    if (cmp != 0) return cmp;
                    // 次要排序避免不穩定
                    return string.Compare(a.DocNo, b.DocNo, StringComparison.OrdinalIgnoreCase);
                });
            }
            else//委測需求單
            {
                try
                {
                    var itemsNode = xmlDocument.SelectSingleNode("realtekOutTest")
                                ?? throw new InvalidOperationException("XML 缺少節點：realtekOutTest");

                    string F_RTNo = GetInnerTextOrDefault(itemsNode, "F_RTNo");
                    string F_Item5_T = "N/A";

                    //Find InnerText LVM 窮舉?---------------------------------------------------

                    string[,] Item = 
                        {
                            { "F_Item0_T", "F_Teradyne" },
                            { "F_Item1_T", "F_DSIO" },
                            { "F_Item2_T", "F_Patch" },
                            { "F_Item3_T", "F_LMF" },
                            { "F_Item4_T", "F_DSP" },
                            { "F_Item5_T", "F_LVM" },
                            { "F_Item6_T", "F_Pin" },
                            { "F_Item7_T", "F_CPU" }
                        };

                    string F_LVM = "N/A";

                    for (int i = 0; i < Item.GetLength(0); i++)
                    {
                        if(GetInnerTextOrDefault(itemsNode, Item[i,0]).Contains("LVM"))
                        {
                            F_Item5_T = GetInnerTextOrDefault(itemsNode, Item[i, 0]);
                            F_LVM = GetInnerTextOrDefault(itemsNode, Item[i, 1]);
                        }
                    }
                    //----------------------------------------------------------------------------
                    string FileName = filename;

                    // 建議：對 key 做標準化（去空白、統一大小寫）
                    string productKey = NormalizeKey(filename);

                    var doc = new OutTestDocument
                    {
                        FileName = FileName,
                        F_RTNo = F_RTNo,
                        F_Item5_T = F_Item5_T,
                        F_LVM = F_LVM,
                    };

                    if (!_OutTestList.TryGetValue(productKey, out var OutTestList))
                    {
                        OutTestList = new List<OutTestDocument>();
                        _OutTestList[productKey] = OutTestList;
                        EnsureProductOrder(productKey);
                    }

                    // 可避免完全重覆（同 DocNo）：
                    bool isDup = OutTestList.Any(x => x.FileName == doc.FileName);
                    if (!isDup)
                    {
                        OutTestList.Add(doc);
                    }
                    else
                    {
                        using (StreamWriter se = File.AppendText(Environment.CurrentDirectory + "\\" + "Error loading XML file.txt"))
                        {
                            se.WriteLine($"{doc.FileName}");
                        }
                    }
                }
                catch (Exception ex) 
                {
                
                }  
            }
        }

        private static string CsvEscape(string s)
        {
            if (s == null) return "";
            bool needQuote = s.IndexOf(',') >= 0 || s.IndexOf('"') >= 0 || s.IndexOf('\r') >= 0 || s.IndexOf('\n') >= 0;
            if (s.IndexOf('"') >= 0)
                s = s.Replace("\"", "\"\"");
            return needQuote ? ("\"" + s + "\"") : s;
        }

        private void WriteAllProductsToCsv(string outputFullPath, bool useBig5)
        {

            if (Xml_SECN_radioButton.Checked == true)
            {
                // 選擇輸出順序來源
                List<string> productOrder = null;
                if (_productOrder != null && _productOrder.Count > 0)
                {
                    productOrder = new List<string>(_productOrder);
                }
                else
                {
                    // 沒有維護順序就用目前的鍵列舉順序（注意不保證穩定）
                    productOrder = new List<string>(_byProduct.Keys);
                }

                // 選擇編碼
                Encoding enc = useBig5 ? Encoding.GetEncoding("big5") : new UTF8Encoding(true); // true = BOM

                using (var sw = new StreamWriter(outputFullPath, false, enc))
                {
                    // 標題列
                    sw.WriteLine("DocNo,Product,Location,Temperature,FRelevance,EQAText,F_TestSec,BinRecycle");

                    // 逐個 Product 輸出
                    for (int pi = 0; pi < productOrder.Count; pi++)
                    {
                        string productKey = productOrder[pi];
                        List<SecnDocument> list;
                        if (!_byProduct.TryGetValue(productKey, out list) || list == null || list.Count == 0)
                            continue;

                        // 注意：**照目前 list 的順序**輸出，不另行排序
                        for (int i = 0; i < list.Count; i++)
                        {
                            SecnDocument d = list[i];

                            // 清理欄位避免 CR/LF 或首尾空白影響 CSV
                            string product = d.Product != null ? d.Product.Replace("；", "").Trim() : "";
                            string docNo = d.DocNo != null ? d.DocNo.Trim() : "";
                            string fRel = d.FRelevance != null ? d.FRelevance.Trim() : "";
                            string fTestSec = d.F_TestSec != null ? d.F_TestSec.Trim() : "";
                            string temp = d.Temperature != null ? d.Temperature.Trim() : "";
                            string binRecycle = d.BinRecycle != null ? d.BinRecycle.Replace("\r", " ").Replace("\n", " ").Trim() : "";
                            string eqa = d.EQAText != null ? d.EQAText.Trim() : "";
                            string location = d.Location != null ? d.Location.Replace("；", "").Replace("\r", " ").Replace("\n", " ").Trim() : "";

                            // 寫一列（CSV 轉義）
                            sw.WriteLine(
                                CsvEscape(docNo) + "," +
                                CsvEscape(product) + "," +
                                CsvEscape(location) + "," +
                                CsvEscape(temp) + "," +
                                CsvEscape(fRel) + "," +
                                CsvEscape(eqa) + "," +
                                CsvEscape(fTestSec) + "," +
                                CsvEscape(binRecycle)
                            );
                        }
                    }
                }
            }
            else
            {
                // 選擇輸出順序來源
                List<string> productOrder = null;
                if (_productOrder != null && _productOrder.Count > 0)
                {
                    productOrder = new List<string>(_productOrder);
                }
                else
                {
                    // 沒有維護順序就用目前的鍵列舉順序（注意不保證穩定）
                    productOrder = new List<string>(_byProduct.Keys);
                }

                // 選擇編碼
                Encoding enc = useBig5 ? Encoding.GetEncoding("big5") : new UTF8Encoding(true); // true = BOM

                using (var sw = new StreamWriter(outputFullPath, false, enc))
                {
                    // 標題列
                    sw.WriteLine("FileName,Product,F_Item5_T,F_LVM");

                    // 逐個 Product 輸出
                    for (int pi = 0; pi < productOrder.Count; pi++)
                    {
                        string productKey = productOrder[pi];
                        List<OutTestDocument> list;
                        if (!_OutTestList.TryGetValue(productKey, out list) || list == null || list.Count == 0)
                            continue;

                        // 注意：**照目前 list 的順序**輸出，不另行排序
                        for (int i = 0; i < list.Count; i++)
                        {
                            OutTestDocument d = list[i];

                            // 清理欄位避免 CR/LF 或首尾空白影響 CSV
                            string F_RTNo = d.F_RTNo != null ? d.F_RTNo.Trim() : "";
                            string FileName = d.FileName != null ? d.FileName.Replace("\r", " ").Replace("\n", " ").Trim() : "";
                            string F_Item5_T = d.F_Item5_T != null ? d.F_Item5_T.Replace("\r", " ").Replace("\n", " ").Trim() : "";
                            string F_LVM = d.F_LVM != null ? d.F_LVM.Replace("\r", " ").Replace("\n", " ").Trim() : "";

                            // 寫一列（CSV 轉義）
                            sw.WriteLine(
                                CsvEscape(F_RTNo) + "," +
                                CsvEscape(FileName) + "," +
                                CsvEscape(F_Item5_T) + "," +
                                CsvEscape(F_LVM)
                            );
                        }
                    }
                }
            }
        }
    }
}
