using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Xml¤W©ß.Tests
{
    [TestClass]
    public class Form1Tests
    {
        [TestMethod]
        public void UploadFile_WithValidParameters_UploadsFile()
        {
            // Arrange
            string uploadPath = "ftp://example.com/uploads";
            string subfolder = "subfolder";
            string xmlfilename = "test.xml";
            string xmlfile = "C:\\path\\to\\test.xml";
            string userName = "username";
            string password = "password";

            Form1 form = new Form1();

            // Act
            form.UploadFile(uploadPath, subfolder, xmlfilename, xmlfile, userName, password);

            // Assert
            // Check if the file was uploaded successfully
            string expectedUploadFilePath = Path.Combine(uploadPath, subfolder, xmlfilename);
            bool fileExists = CheckIfFileExists(expectedUploadFilePath, userName, password);
            Assert.IsTrue(fileExists);
        }

        private bool CheckIfFileExists(string filePath, string userName, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(filePath);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.Credentials = new NetworkCredential(userName, password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return true;
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
