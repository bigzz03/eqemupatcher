﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Shell;

namespace EQEmu_Patcher
{
    
    public partial class MainForm : Form
    {

        private Dictionary<VersionTypes, ClientVersion> clientVersions = new Dictionary<VersionTypes, ClientVersion>();

        VersionTypes currentVersion;

       // TaskbarItemInfo tii = new TaskbarItemInfo();
        public MainForm()
        {
            InitializeComponent();
        }           

        private void MainForm_Load(object sender, EventArgs e)
        {
            buildClientVersions();
            IniLibrary.Load();

            if (IniLibrary.instance.ClientVersion == VersionTypes.Unknown)
            {
                detectClientVersion();
                if (currentVersion == VersionTypes.Unknown)
                {
                    this.Close();
                }
                detectCleanCopy();
                IniLibrary.instance.ClientVersion = currentVersion;
                IniLibrary.Save();
            }
            
           
        }

        System.Diagnostics.Process process;
      

        System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();

        Dictionary<string, string> WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            var fileMap = new Dictionary<string, string>();
            try
            {
                 files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                txtList.Text += e.Message +"\n";
                return fileMap;
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                txtList.Text += e.Message + "\r\n";
                return fileMap;
            }

            if (files != null)
            {
                
                foreach (System.IO.FileInfo fi in files)
                {
                    if (fi.Name.Contains(".ini"))
                    { //Skip INI files
                        progressBar.Value++;
                        continue;
                    }
                    if (fi.Name == System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                    { //Skip self EXE
                        progressBar.Value++;
                        continue;
                    }

                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    var md5 = UtilityLibrary.GetMD5(fi.FullName);
                    txtList.Text += fi.Name + ": " + md5 + "\r\n";
                    if (progressBar.Maximum > progressBar.Value) {
                        progressBar.Value++;
                    }
                    fileMap[fi.Name] = md5;
                    txtList.Refresh();
                    updateTaskbarProgress();
                    Application.DoEvents();
                    
                }
                //One final update of data
                if (progressBar.Maximum > progressBar.Value)
                {
                    progressBar.Value++;
                }
                txtList.Refresh();
                updateTaskbarProgress();
                Application.DoEvents();
            }
            return fileMap;
        }

        private void detectCleanCopy()
        {
            //I use eqspells to detect clean copies, since it's the most common file to be edited.
            if (MessageBox.Show("This directory appears to be a clean copy of " + clientVersions[currentVersion].FullName + ". Should we set this as the default?", "Clean Copy?", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
            {
                IniLibrary.instance.IsCleanCopy = true;
            }
        }

        private void detectClientVersion()
        {

            try
            {

                var hash = UtilityLibrary.GetEverquestExecutableHash(AppDomain.CurrentDomain.BaseDirectory);
                if (hash == "")
                {
                    MessageBox.Show("Please run this patcher in your Everquest directory.");
                    this.Close();
                    return;
                }
                switch (hash)
                {
                    case "85218FC053D8B367F2B704BAC5E30ACC":
                        currentVersion = VersionTypes.Secrets_Of_Feydwer;
                        break;
                    case "859E89987AA636D36B1007F11C2CD6E0":
                        currentVersion = VersionTypes.Underfoot;
                        break;
                    case "BB42BC3870F59B6424A56FED3289C6D4":
                        currentVersion = VersionTypes.Titanium;
                        break;
                    case "368BB9F425C8A55030A63E606D184445":
                        currentVersion = VersionTypes.Rain_Of_Fear;
                        break;
                    case "240C80800112ADA825C146D7349CE85B":
                        currentVersion = VersionTypes.Rain_Of_Fear_2;
                        break;
                    default:
                        currentVersion = VersionTypes.Unknown;
                        break;

                }
                if (currentVersion == VersionTypes.Unknown)
                {
                    if (MessageBox.Show("Unable to recognize the Everquest client in this directory, open a web page to report to devs?", "Visit", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("https://github.com/Xackery/eqemupatcher/issues/new?title=A+New+EQClient+Found&body=Hi+I+Found+A+New+Client!+Hash:+" + hash);
                    }
                    txtList.Text = "Unable to recognize the Everquest client in this directory, send to developers: " + hash;
                }
                else
                {
                    txtList.Text = "You seem to have put me in a " + clientVersions[currentVersion].FullName + " client directory";
                }


                txtList.Text += "\r\n\r\nIf you wish to help out, press the scan button on the bottom left and wait for it to complete, then copy paste this data as an Issue on github!";
            }
            catch (UnauthorizedAccessException err)
            {
                MessageBox.Show("You need to run this program with Administrative Privileges" + err.Message);
                return;
            }
        }

        //Build out all client version's dictionary
        private void buildClientVersions()
        {
            clientVersions.Clear();
            clientVersions.Add(VersionTypes.Titanium, new ClientVersion("Titanium", "titanium"));
            clientVersions.Add(VersionTypes.Secrets_Of_Feydwer, new ClientVersion("Secrets Of Feydwer", "sof"));
            clientVersions.Add(VersionTypes.Seeds_Of_Destruction, new ClientVersion("Seeds of Destruction", "sod"));
            clientVersions.Add(VersionTypes.Rain_Of_Fear, new ClientVersion("Rain of Fear", "rof"));
            clientVersions.Add(VersionTypes.Rain_Of_Fear_2, new ClientVersion("Rain of Fear 2", "rof2"));
            clientVersions.Add(VersionTypes.Underfoot, new ClientVersion("Underfoot", "underfoot"));

        }

        private int getFileCount(System.IO.DirectoryInfo root) {
            int count = 0;
                           
            System.IO.FileInfo[] files = null;
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                txtList.Text += e.Message + "\n";
                return 0;
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                txtList.Text += e.Message + "\r\n";
                return 0;
            }

            if (files != null)
            {
              return files.Length;
            }
            return count;
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            txtList.Text = "";
            progressBar.Maximum = getFileCount(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
            progressBar.Maximum += getFileCount(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Resources"));
            progressBar.Maximum += getFileCount(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\sounds"));
            progressBar.Maximum += getFileCount(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\SpellEffects"));
            progressBar.Maximum += getFileCount(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\storyline"));
          //  progressBar.Maximum += getFileCount(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\uifiles"));
          //  progressBar.Maximum += getFileCount(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\atlas"));
            txtList.Text = "Max:" + progressBar.Maximum;
            PatchVersion pv = new PatchVersion();
            pv.ClientVersion = clientVersions[currentVersion].ShortName;
            //Root
            var fileMap = WalkDirectoryTree(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
            pv.RootFiles = fileMap;
            //Resources
            fileMap = WalkDirectoryTree(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Resources"));
            pv.ResourceFiles = fileMap;
            //Sounds
            fileMap = WalkDirectoryTree(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\sounds"));
            pv.SoundFiles = fileMap;
            //SpellEffects
            fileMap = WalkDirectoryTree(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\SpellEffects"));
            pv.SpellEffectFiles = fileMap;
            //Storyline
            fileMap = WalkDirectoryTree(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\storyline"));
            pv.StorylineFiles = fileMap;
           /*
            //UIFiles
            fileMap = WalkDirectoryTree(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\uifiles"));
            pv.UIFiles = fileMap;
            //Atlas
            fileMap = WalkDirectoryTree(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\atlas"));
            pv.AtlasFiles = fileMap;
            */
            txtList.Text = JsonConvert.SerializeObject(pv);
        }

        private void updateTaskbarProgress()
        {
            
            if (Environment.OSVersion.Version.Major < 6)
            { //Only works on 6 or greater
                return;
            }
            
            
           // tii.ProgressState = TaskbarItemProgressState.Normal;            
           // tii.ProgressValue = (double)progressBar.Value / progressBar.Maximum;            
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try {
                process = UtilityLibrary.StartEverquest();
                if (process != null)
                {
                    this.Close();
                }
            }
            catch  (Exception err) {
                MessageBox.Show("An error occured: "   + err.Message);
            }
        }
    }
}


