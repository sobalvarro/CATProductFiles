using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CATProductFiles
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSearchProduct_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter= "CATProduct Files (*.CATProduct)|*.CATProduct";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txbPath.Text = fileDialog.FileName;
                List<string> productFiles = GetComponents(fileDialog.FileName);
                libList.Items.Clear();
                foreach (string file in productFiles)
                {
                    libList.Items.Add(file);
                }
            }
        }

        public static List<string> GetComponents(string productFullFileName)
        {

            //Declare the result list
            List<string> result = new List<string>();

            //Read the product file and store as an UTF8 string
            string productUTFText = File.ReadAllText(productFullFileName, Encoding.UTF8);

            //Find the beginning of the CATOctetArray, that means the start of the portion where the components are listed
            int octetArrayStart = productUTFText.IndexOf("CATOctetArray");

            //Find the end of the CATOctet Array
            int octetArrayEnd = productUTFText.IndexOf("\u0008FINJPL", octetArrayStart);

            //Calculate the length of the Octet Array
            int octetArrayLenght = octetArrayEnd - octetArrayStart;

            //Extract the portion of the OctetArray from the full text
            string octetArray = productUTFText.Substring(octetArrayStart, octetArrayLenght);

            //Clean up the Octet Array by eliminating the SOH Char
            octetArray = octetArray.Replace(";\u0001", "");

            //Create a raw list by splitting the octet array on "[SCH][EOT]File Keyword
            string[] docIdseparator = { "\u0001\u0004File" };

            List<string> rawFileList = octetArray.Split(docIdseparator, StringSplitOptions.None).ToList();

            //Loop the list for further cleaning
            foreach (string rawFile in rawFileList)
            {
                //If start with CATOctetArray ignore
                if (rawFile.StartsWith("CATOctetArray")) { continue; }

                //If the chars from 1 to 4 = feat then ignore.
                if (rawFile.Substring(1).StartsWith("feat")) { continue; }

                //Find the null char, then the char ", this is the beginning and the end of the raw file
                int rawFileStartIndex = rawFile.IndexOf("\u0000");
                int rawFileEndIndex = rawFile.IndexOf("\u0022");
                int rawFileLenght = rawFileEndIndex - rawFileStartIndex;
                string cleanFile = rawFile.Substring(rawFileStartIndex, rawFileLenght);

                //Clean up by finding the last backslash and retrieving the string after it
                int fileStartIndex = cleanFile.LastIndexOf("\u005c") + 1;
                cleanFile = cleanFile.Substring(fileStartIndex);

                //Clean up by eliminating null characters
                cleanFile = cleanFile.Replace("\u0000", "");

                //Clean up by eliminating last character
                cleanFile = cleanFile.Substring(0, cleanFile.Length - 1);

                result.Add(cleanFile);

            }

            return result;
        }
    }
}
