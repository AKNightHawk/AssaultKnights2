using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using AngryWasp.DirectoryScanner;
using Engine.FileSystem;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ModPackager.Properties;

namespace ModPackager
{
    public partial class Form1 : Form
    {
        private XmlDocument patchFile = new XmlDocument();
        private string patchFileName;
        private XmlDocument referenceFile = new XmlDocument();

        public delegate void StringHandlerDelegate(string message);

        public Form1()
        {
            InitializeComponent();
        }

        private void WriteMessage(string message)
        {
            messageText.Text = message;
        }

        private bool MakeDirectories()
        {
            bool directoriesCreated = false;
            //directory for saving patch config files
            {
                string dir = VirtualFileSystem.GetRealPathByVirtual("user:Patches\\Config");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    directoriesCreated = true;
                }
            }

            //directory for saving patch output files
            {
                string dir = VirtualFileSystem.GetRealPathByVirtual("user:Patches\\Output");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    directoriesCreated = true;
                }
            }

            return directoriesCreated;
        }

        private void PreparePatchFile()
        {
            patchFile = new XmlDocument();
            patchFile.CreateXmlDeclaration("1.0", null, null);
            XmlNode rootNode = patchFile.CreateElement("patch");
            patchFile.AppendChild(rootNode);
        }

        private void LoadReferenceFile()
        {
            if (referenceFiles.SelectedIndex == -1)
            {
                MessageBox.Show("A reference file must be selected", "Mod Packager",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string p = VirtualFileSystem.GetRealPathByVirtual(
                "user:Patches\\Config\\" + referenceFiles.SelectedItem.ToString());

            referenceFile.Load(p);
        }

        private string FormatDate(DateTime date)
        {
            return string.Format("{0}.{1}.{2}_{3}.{4}.{5}", date.Day,
                date.Month, date.Year, date.Hour, date.Minute, date.Second);
        }

        private void WritePatchFileNode(FileInfo info)
        {
            XmlNode fileNode = patchFile.CreateElement("file");
            XmlAttribute path = patchFile.CreateAttribute("path");
            XmlAttribute lastModified = patchFile.CreateAttribute("lastModified");

            string relativePath = info.FullName.Replace(Application.StartupPath, "").Trim('\\');
            path.Value = relativePath;
            lastModified.Value = FormatDate(info.LastWriteTime);

            fileNode.Attributes.Append(path);
            fileNode.Attributes.Append(lastModified);

            patchFile.DocumentElement.AppendChild(fileNode);
        }

        private void firstRun_FileEvent(object sender, FileEventArgs e)
        {
            if (e.Info.FullName.Contains("NativeDlls") ||
                e.Info.FullName.Contains("UserSettings") ||
                e.Info.FullName.Contains("Patches"))
                return;

            WritePatchFileNode(e.Info);

            Invoke(new StringHandlerDelegate(WriteMessage), "Scanned: " + e.Info.Name);
        }

        private void firstRunPatchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PreparePatchFile();
            DirectoryScanner scanner = new DirectoryScanner();
            scanner.SearchPattern = "*.*;";
            scanner.FileEvent += new DirectoryScanner.FileEventHandler(firstRun_FileEvent);
            scanner.WalkDirectory(Application.StartupPath);
        }

        private void firstRunPatchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string p = VirtualFileSystem.GetRealPathByVirtual("user:Patches\\Config\\Reference.xml");
            patchFile.Save(p);

            Settings.Default.LastPatch = p;
            Settings.Default.Save();

            LoadPreviousFiles();

            MessageBox.Show("Reference file created. You can now use the software", "First Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //directories have needed to be made, we run the reference builder
            if (MakeDirectories())
            {
                if (DialogResult.Yes == MessageBox.Show("This is the first time the patch maker has been run.\r\nBefore you can use it you must create a reference build.\r\nDo you want to do that now?", "First Run", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    firstRunPatchWorker.RunWorkerAsync();
                }
            }
            else
            {
                LoadPreviousFiles();
            }
        }

        private void LoadPreviousFiles()
        {
            referenceFiles.Items.Clear();

            string dir = VirtualFileSystem.GetRealPathByVirtual("user:Patches\\Config");
            string[] patchFiles = Directory.GetFiles(dir);

            foreach (string info in patchFiles)
                referenceFiles.Items.Add(info.Remove(0, info.LastIndexOf("\\") + 1));

            if (patchFiles.Length > 0)
            {
                if (!string.IsNullOrEmpty(Settings.Default.LastPatch))
                {
                    for (int i = 0; i < patchFiles.Length; i++)
                    {
                        if (patchFiles[i].Contains(Settings.Default.LastPatch))
                            referenceFiles.SelectedIndex = i;
                    }
                }
                else
                    referenceFiles.SelectedIndex = 0;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            diffFiles.Items.Clear();
            LoadReferenceFile();
            patchWorker.RunWorkerAsync();
        }

        private void patch_FileEvent(object sender, FileEventArgs e)
        {
            if (e.Info.FullName.Contains("NativeDlls") ||
                e.Info.FullName.Contains("UserSettings") ||
                e.Info.FullName.Contains("Patches"))
                return;

            string relativePath = e.Info.FullName.Replace(Application.StartupPath, "").Trim('\\');
            string query = string.Format("//patch/file[@path = '{0}']", relativePath);
            XmlNode n = referenceFile.SelectSingleNode(query);

            if (n == null) //file does not exist
            {
                Invoke(new StringHandlerDelegate(AddDiffFile), relativePath);
            }
            else
            {
                string lastModified = FormatDate(e.Info.LastWriteTime);
                if (n.Attributes["lastModified"].Value != lastModified) //modified dates are different
                {
                    Invoke(new StringHandlerDelegate(AddDiffFile), relativePath);
                }
            }

            WritePatchFileNode(e.Info);

            Invoke(new StringHandlerDelegate(WriteMessage), "Scanned: " + e.Info.Name);
        }

        private void AddDiffFile(string fileName)
        {
            diffFiles.Items.Add(fileName);
            diffFiles.SetItemChecked(diffFiles.Items.Count - 1, true);
        }

        private void AddDelFile(string fileName)
        {
            delFiles.Items.Add(fileName);
            delFiles.SetItemChecked(delFiles.Items.Count - 1, true);
        }

        private void patchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PreparePatchFile();
            DirectoryScanner scanner = new DirectoryScanner();
            if (string.IsNullOrEmpty(txtExtensions.Text))
                scanner.SearchPattern = "*.*;";
            else
                scanner.SearchPattern = txtExtensions.Text;
            scanner.FileEvent += new DirectoryScanner.FileEventHandler(patch_FileEvent);
            scanner.WalkDirectory(Application.StartupPath);

            foreach (XmlNode node in referenceFile.DocumentElement.ChildNodes)
            {
                string filePath = node.Attributes[0].Value;
                if (!File.Exists(filePath))
                {
                    string relativePath = filePath.Replace(Application.StartupPath, "").Trim('\\');
                    Invoke(new StringHandlerDelegate(AddDelFile), relativePath);
                }
            }
        }

        private void patchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (diffFiles.Items.Count == 0)
            {
                MessageBox.Show("Done. Nothing to patch", "Mod Packager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                Application.Exit();
                return;
            }

            patchFileName = FormatDate(DateTime.Now);
            string p = VirtualFileSystem.GetRealPathByVirtual("user:Patches\\Config\\" + patchFileName + ".xml");
            patchFile.Save(p);

            Settings.Default.LastPatch = patchFileName;
            Settings.Default.Save();

            LoadPreviousFiles();

            MessageBox.Show("Done", "First Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < diffFiles.Items.Count; i++)
            {
                diffFiles.SetItemChecked(i, true);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < diffFiles.Items.Count; i++)
            {
                diffFiles.SetItemChecked(i, false);
            }
        }

        private void makePatch_Click(object sender, EventArgs e)
        {
            //create directory for patch

            backupWorker.RunWorkerAsync();
        }

        private void backupWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string dir = VirtualFileSystem.GetRealPathByVirtual("user:Patches\\Output\\" + patchFileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            for (int i = 0; i < diffFiles.Items.Count; i++)
            {
                //ignore unchecked files
                if (!diffFiles.GetItemChecked(i))
                    continue;

                string file = diffFiles.Items[i].ToString();

                string sourcePath = Application.StartupPath + "\\" + file;
                string destinationPath = dir + "\\" + file;
                string destinationFolderPath = destinationPath.Remove(destinationPath.LastIndexOf('\\'));
                if (!Directory.Exists(destinationFolderPath))
                    Directory.CreateDirectory(destinationFolderPath);

                File.Copy(sourcePath, destinationPath);

                Invoke(new StringHandlerDelegate(WriteMessage), "Copying: " + file);
            }

            string zipFileName = dir + ".zip";

            FileStream outFileStream = File.Create(zipFileName);

            string[] fileNames = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

            using (ZipOutputStream zipStream = new ZipOutputStream(outFileStream))
            {
                zipStream.SetLevel(9); // 0 - store only to 9 - means best compression

                byte[] buffer = new byte[4096];
                DateTime dateTimeNow = DateTime.Now;

                foreach (string fileName in fileNames)
                {
                    string ef = fileName.Replace(dir + "\\", "");
                    ZipEntry entry = new ZipEntry(ef);

                    using (FileStream fileStream = File.OpenRead(fileName))
                    {
                        entry.Size = fileStream.Length;
                        entry.DateTime = dateTimeNow;
                        zipStream.PutNextEntry(entry);
                        StreamUtils.Copy(fileStream, zipStream, buffer);
                    }
                }

                zipStream.Finish();
                zipStream.Close();
            }

            Directory.Delete(dir, true);
        }

        private void backupWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Done", "Mod Packager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //create delete.bat
            File.Create("Delete.bat").Close();
            StreamWriter sw = new StreamWriter("Delete.bat");
            foreach (object item in delFiles.Items)
            {
                string file = item.ToString();
                sw.WriteLine("del \"C:\\Program Files D\\NeoAxis\\NeoAxis Engine Indie SDK 0.852\\Game\\Bin\\" + file + "\"");
            }

            sw.Close();
        }
    }
}