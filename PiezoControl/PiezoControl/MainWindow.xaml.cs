using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathWorks.xPCTarget.FrameWork;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;


namespace PiezoControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        xPCTargetPC tar_pc = new xPCTargetPC();
        DispatcherTimer trialtimer = new System.Windows.Threading.DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();
        int trialnumber;
        int secondselapsed;
        int drivertype = 0;
        
        public MainWindow()
        {
            InitializeComponent();
            trialtimer.IsEnabled = false;
            trialtimer.Tick += new EventHandler(trialtimer_Tick);
            trialtimer.Interval = TimeSpan.FromSeconds(.1);
            //folderDB.SelectionChanged += new SelectionChangedEventHandler(folderDB_Change);
            trialduration.IsEnabled = false;

            //Create and populate an initial listing of folders containing experiment file sets
            List<string> foldernames = new List<string>();
            folderDB.Items.Add(@"No Folder");
            string[] folders = System.IO.Directory.GetDirectories(System.IO.Path.GetFullPath(@".\StimProtocols"));
            foreach (string foldername in folders)
            {
                folderDB.Items.Add(System.IO.Path.GetFileName(foldername));
            }
            folderDB.SelectedIndex = 0;
        }

        public void folderDB_closed(object sender, EventArgs e)
        {
            //Pre-clear the existing populated lists and then repopulate the blank items for High (5V) and Low (0V)
            A0.Items.Clear();
            A1.Items.Clear();
            A2.Items.Clear();
            A3.Items.Clear();
            trig.Items.Clear();
            shutter.Items.Clear();
            dio1.Items.Clear();
            dio2.Items.Clear();
            dio3.Items.Clear();
            dio4.Items.Clear();
            dio5.Items.Clear();
            dio6.Items.Clear();
            A0.Items.Add("Blank High"); A0.Items.Add("Blank Low");
            A0.SelectedIndex = 1;
            A1.Items.Add("Blank High"); A1.Items.Add("Blank Low");
            A1.SelectedIndex = 1;
            A2.Items.Add("Blank High"); A2.Items.Add("Blank Low");
            A2.SelectedIndex = 1;
            A3.Items.Add("Blank High"); A3.Items.Add("Blank Low");
            A3.SelectedIndex = 1;
            trig.Items.Add("Blank High"); trig.Items.Add("Blank Low");
            trig.SelectedIndex = 1;
            shutter.Items.Add("Blank High"); shutter.Items.Add("Blank Low");
            shutter.SelectedIndex = 1;
            dio1.Items.Add("Blank High"); dio1.Items.Add("Blank Low");
            dio1.SelectedIndex = 1;
            dio2.Items.Add("Blank High"); dio2.Items.Add("Blank Low");
            dio2.SelectedIndex = 1;
            dio3.Items.Add("Blank High"); dio3.Items.Add("Blank Low");
            dio3.SelectedIndex = 1;
            dio4.Items.Add("Blank High"); dio4.Items.Add("Blank Low");
            dio4.SelectedIndex = 1;
            dio5.Items.Add("Blank High"); dio5.Items.Add("Blank Low");
            dio5.SelectedIndex = 1;
            dio6.Items.Add("Blank High"); dio6.Items.Add("Blank Low");
            dio6.SelectedIndex = 1;

            //Unless the current selection is No Folder, populate the files that correspond to the appropriate experiment group folder (plus defaults)
            string current_folder = folderDB.SelectedValue.ToString();
            if (current_folder != "No Folder")
            {
                List<string> filenames = new List<string>();
                bool stim_exist = System.IO.Directory.Exists(System.IO.Path.GetFullPath(@".\StimProtocols" + "\\" + folderDB.SelectedValue));
                bool trig_exist = System.IO.Directory.Exists(System.IO.Path.GetFullPath(@".\TrigProtocols" + "\\" + folderDB.SelectedValue));
                bool dio_exist = System.IO.Directory.Exists(System.IO.Path.GetFullPath(@".\DIOProtocols" + "\\" + folderDB.SelectedValue));

                if (!stim_exist || !trig_exist || !dio_exist)
                {
                    MessageBox.Show("Must have identically named experiment in Stim, Trig, and DIO folders.");
                    folderDB.SelectedIndex = 0;
                    return;
                }

                string[] stim_files = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(@".\StimProtocols" + "\\" + folderDB.SelectedValue));
                string[] trig_files = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(@".\TrigProtocols" + "\\" + folderDB.SelectedValue));
                string[] dio_files = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(@".\DIOProtocols" + "\\" + folderDB.SelectedValue));

                foreach (string file in stim_files)
                {
                    A0.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                    A1.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                    A2.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                    A3.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                }

                foreach (string file in trig_files)
                {
                    trig.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                }

                
            }
            else
            {
                populateAllFiles();
            }
        }

        public void connectBTN_Click(object sender, RoutedEventArgs e)   {
            tar_pc.TcpIpTargetAddress = ipTB.Text;
            tar_pc.TcpIpTargetPort = portTB.Text;

            try
            {
                tar_pc.Connect();
            }
            catch (Exception xpcerr)
            {
                string errmsg = xpcerr.ToString();
                MessageBox.Show(errmsg, "CONN Error", MessageBoxButton.OK);
            }
            connectBTN.IsEnabled = false;
            dcBTN.IsEnabled = true;
            origBTN.IsEnabled = true;
            galvoBTN.IsEnabled = true;
            estimBTN.IsEnabled = true;

            mnameTB.Text = tar_pc.Application.Name;
        }

        public void populateAllFiles()
        {
            //Load Stimulus Protocol files
            List<string> filenames = new List<string>();
            //OpenFileDialog ofd = new OpenFileDialog();
            
            string[] filesindir = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(@".\StimProtocols"));

            foreach (string file in filesindir)
            {
                A0.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                A1.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                A2.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                A3.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }

            //Load Trigger Data File
            filesindir = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(@".\TrigProtocols"));

            foreach (string file in filesindir)
            {
                trig.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }

            //Load Shutter Protocol Files
            filesindir = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(@".\DIOProtocols"));

            foreach (string file in filesindir)
            {
                shutter.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }

            //Load Digital Protocol Files
            filesindir = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath(@".\DIOProtocols"));

            foreach (string file in filesindir)
            {
                dio1.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                dio2.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                dio3.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                dio4.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                dio5.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                dio6.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }
        }
    
        public void LoadProcedure() 
        {
            startBTN.IsEnabled = true;
            stopBTN.IsEnabled = false;
            origBTN.IsEnabled = false;
            galvoBTN.IsEnabled = false;
            estimBTN.IsEnabled = false;
            unloadBTN.IsEnabled = true;
            kernelBTN.IsEnabled = true;
            shutter.IsEnabled = true;

            populateAllFiles();
            
            mnameTB.Text = tar_pc.Application.Name;
        }

        private void stopBTN_Click(object sender, RoutedEventArgs e)
        {
            //Stop timer first so there is no race condition where app stops right before timer ticks
            trialtimer.Stop();
            tar_pc.Application.Stop();
            stopwatch.Start();
            stopwatch.Reset();
            //'Since the application has started, disable the Start button and enable the Stop button
            startBTN.IsEnabled = true;
            stopBTN.IsEnabled = false;
            trialtimer.IsEnabled = false;
            kernelBTN.IsEnabled = true;
            unloadBTN.IsEnabled = true;
            onoffCB.IsEnabled = false;
        }

        public void trialtimer_Tick(object sender, EventArgs e)
        {
            IList<double> trial_value = new List<double>{0};
            //try
            //{
            //    IList<xPCSignal> trial_sig;
                
            //    string[] signal_name = new string[1];
            //    signal_name[0] = "Memory1";

            //    trial_sig = tar_pc.Application.Signals.GetSignals(signal_name);
            //    trial_value = tar_pc.Application.Signals.GetSignalsValue(trial_sig);
                               
            //}
            //catch (xPCException err)
            //{
                //if (secondselapsed + 1 > Convert.ToInt32(Convert.ToDouble(trialduration.Text) / 1000))
                //{
                //    trialnumber = trialnumber + 1;
                //    secondselapsed = 0;
                //}
                //else
                //{
                //    secondselapsed = secondselapsed + 1;
                //}
                trial_value[0] = Math.Floor(Convert.ToDouble(stopwatch.ElapsedMilliseconds)/Convert.ToDouble(trialduration.Text));
            //}
            //'TrialCount = TrialCount + 1

            trialnum.Content = trial_value[0].ToString();
        }

        private void startBTN_Click(object sender, RoutedEventArgs e)
        {
            //'Here we start the application. 
            trialnumber = 0;
            secondselapsed = 0;
            tar_pc.Application.Start();
            stopwatch.Start();
            trialnum.Content = @"0";
            //'Since we start the application we disable the start button, enable stop.
            startBTN.IsEnabled = false;
            stopBTN.IsEnabled = true;
            trialtimer.IsEnabled = true;
            kernelBTN.IsEnabled = false;
            unloadBTN.IsEnabled = false;
            //if (mnameTB.Text == "mstimulator_trigger") { }
            //else{
            //    onoffCB.IsEnabled = true;
            //}
            //Start timer after being relatively sure the application has started
            //Thread.Sleep(5000);

            trialtimer.Start();
        }

        private void unloadBTN_Click(object sender, RoutedEventArgs e)
        {
            tar_pc.Unload();

            startBTN.IsEnabled = false;
            stopBTN.IsEnabled = false;
            origBTN.IsEnabled = true;
            galvoBTN.IsEnabled = true;
            estimBTN.IsEnabled = true;
            unloadBTN.IsEnabled = false;
            if (trialtimer.IsEnabled)
            {
                trialtimer.Stop();
            }
            trialtimer.IsEnabled = false;
            dcBTN.IsEnabled = true;
            onoffCB.IsEnabled = false;
            kernelBTN.IsEnabled = false;

            mnameTB.Text = tar_pc.Application.Name;
        }

        private void dcBTN_Click(object sender, RoutedEventArgs e)
        {
            tar_pc.Disconnect();
            //'Enable components required to Reconnect to Target
            connectBTN.IsEnabled = true;
            //'Disable all the components required when not connected to the target PC.
            if (trialtimer.IsEnabled)
            {
                trialtimer.Stop();
            }
            trialtimer.IsEnabled = false;
            dcBTN.IsEnabled = false;
            startBTN.IsEnabled = false;
            stopBTN.IsEnabled = false;
            origBTN.IsEnabled = false;
            galvoBTN.IsEnabled = false;
            estimBTN.IsEnabled = false;
            unloadBTN.IsEnabled = false;
        }

        private void origBTN_Click(object sender, RoutedEventArgs e)
        {
            tar_pc.Load(@"mstimulator_new.dlm");
            LoadProcedure();
            drivertype = 0;
        }

        private void galvoBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tar_pc.Load(@".\mstimulator_galvo.dlm");
                LoadProcedure();
                drivertype = 1;
            }
            catch (xPCException gvapperr)
            {
                MessageBox.Show(gvapperr.Message, "App Load Error", MessageBoxButton.OK);
            }
        }

        private void onoffCB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            xPCParameter mstim_switch_index;
            double[] mstim_value = new double[1];

            mstim_switch_index = tar_pc.Application.Parameters["Subsystem/Mstim Switch","Value"];
            if ((bool)onoffCB.IsChecked)
            {
                mstim_value[0] = 1;
                mstim_switch_index.Value = mstim_value;
            }
            else
            {
                mstim_value[0] = 0;
                mstim_switch_index.Value = mstim_value;
            }

            //trialtimer.IsEnabled = true;
        }

        private void kernelBTN_Click(object sender, RoutedEventArgs e)
        {
            string current_folder = folderDB.SelectedValue.ToString();
            List<string> expfiles = new List<string> {};
            if (current_folder != "No Folder")
            {
                expfiles.Add(@".\StimProtocols\" + @"\" + current_folder + @"\" + A0.Text + ".txt");
                expfiles.Add(@".\StimProtocols\" + @"\" + current_folder + @"\" + A1.Text + ".txt");
                expfiles.Add(@".\StimProtocols\" + @"\" + current_folder + @"\" + A2.Text + ".txt");
                expfiles.Add(@".\StimProtocols\" + @"\" + current_folder + @"\" + A3.Text + ".txt"); 
                expfiles.Add(@".\TrigProtocols\" + @"\" + current_folder + @"\" + trig.Text + ".txt"); 
                expfiles.Add(@".\DIOProtocols\" + @"\" + current_folder + @"\" + shutter.Text + ".txt"); 
                expfiles.Add(@".\DIOProtocols\" + @"\" + current_folder + @"\" + dio1.Text + ".txt"); 
                expfiles.Add(@".\DIOProtocols\" + @"\" + current_folder + @"\" + dio2.Text + ".txt");
                expfiles.Add(@".\DIOProtocols\" + @"\" + current_folder + @"\" + dio3.Text + ".txt"); 
                expfiles.Add(@".\DIOProtocols\" + @"\" + current_folder + @"\" + dio4.Text + ".txt");
                expfiles.Add(@".\DIOProtocols\" + @"\" + current_folder + @"\" + dio5.Text + ".txt"); 
                expfiles.Add(@".\DIOProtocols\" + @"\" + current_folder + @"\" + dio6.Text + ".txt");
            }
            else
            {
                expfiles.Add(@".\StimProtocols\" + A0.Text + ".txt"); 
                expfiles.Add(@".\StimProtocols\" + A1.Text + ".txt"); 
                expfiles.Add(@".\StimProtocols\" + A2.Text + ".txt");
                expfiles.Add(@".\StimProtocols\" + A3.Text + ".txt"); 
                expfiles.Add(@".\TrigProtocols\" + trig.Text + ".txt"); 
                expfiles.Add(@".\DIOProtocols\" + shutter.Text + ".txt"); 
                expfiles.Add(@".\DIOProtocols\" + dio1.Text + ".txt");
                expfiles.Add(@".\DIOProtocols\" + dio2.Text + ".txt");
                expfiles.Add(@".\DIOProtocols\" + dio3.Text + ".txt");
                expfiles.Add(@".\DIOProtocols\" + dio4.Text + ".txt");
                expfiles.Add(@".\DIOProtocols\" + dio5.Text + ".txt");
                expfiles.Add(@".\DIOProtocols\" + dio6.Text + ".txt");
            }

            List<string> subsystems = new List<string> {"Subsystem/kernel1", "Subsystem/kernel2", "Subsystem/kernel3", "Subsystem/kernel4", "Subsystem/D1", "Subsystem/D2",
                "Subsystem/D3", "Subsystem/D4", "Subsystem/D5", "Subsystem/D6", "Subsystem/D7", "Subsystem/D8"}; 

            if (tar_pc.IsConnected)
            {
                for (int i = 0; i < subsystems.Count; i++)
                {
                    double IC = 0;
                    // Reset the initial condition if we are in Piezo mode AND setting A0 or A1
                    if (drivertype == 0 && (i == 0 || i == 1)) {IC = 5;}
                    double file_length = OpenTXT(expfiles[i], subsystems[i], IC);
                    if (i == 0) //Base the length of the stimulation on the length of the longest file (A0)
                    {
                        double[] threshold_value = {file_length};
                        xPCParameter threshold_index = tar_pc.Application.Parameters["Subsystem/threshold", "Value"];
                        threshold_index.Value = threshold_value;
                        trialduration.Text = file_length.ToString();
                    }
                }
            }
            else
            {
                MessageBox.Show("Connect to Target PC and Load Application First", "No Conn/App", MessageBoxButton.OK);
            }

        }

        private double OpenTXT(string datafile,string subsys, double IC)
        {
            double[] arrValues = new double[600000];
            double file_length;

            int locBH = datafile.IndexOf("Blank High");
            int locBL = datafile.IndexOf("Blank Low");

            if (locBH > -1)
            { 
                for (int i = 0; i < 600000; i++) arrValues[i] = 5; 
                file_length = 1; 
            }
            else if (locBL > -1)
            {
                for (int i = 0; i < 600000; i++) arrValues[i] = 0;
                file_length = 1;
            }
            else
            {
                string[] readText = System.IO.File.ReadAllLines(System.IO.Path.GetFullPath(datafile));
                int i = 1;
                arrValues[0] = IC;

                for (i = 0; i < readText.Length; i++)
                {
                    arrValues[i + 1] = Convert.ToDouble(readText[i]);
                }
                file_length = i;

                int j = 0;
                for (j = readText.Length; j <= 599998; j++)
                {
                    arrValues[j] = IC;
                }
            }

            xPCParameter kernel_index = tar_pc.Application.Parameters[subsys, "Value"];
            kernel_index.Value = arrValues;
            return file_length;
        }

    } 
        
}
