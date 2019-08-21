using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using System.Management;
using System.Security.Cryptography;
using System.Windows.Interop;

namespace SoftwareLocker
{
    public class WpfWindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WpfWindowWrapper(System.Windows.Window wpfWindow)
        {
            Handle = new System.Windows.Interop.WindowInteropHelper(wpfWindow).Handle;
        }

        public IntPtr Handle { get; private set; }
    }
    static class SystemInfo
    {
        #region -> Private Variables

        public static bool UseProcessorID;
        public static bool UseBaseBoardProduct;
        public static bool UseBaseBoardManufacturer;
        public static bool UseDiskDriveSignature;
        public static bool UseVideoControllerCaption;
        public static bool UsePhysicalMediaSerialNumber;
        public static bool UseBiosVersion;
        public static bool UseBiosManufacturer;
        public static bool UseWindowsSerialNumber;

        #endregion

        public static string GetSystemInfo(string SoftwareName)
        {
            if (UseProcessorID == true)
                SoftwareName += RunQuery("Processor", "ProcessorId");

            if (UseBaseBoardProduct == true)
                SoftwareName += RunQuery("BaseBoard", "Product");

            if (UseBaseBoardManufacturer == true)
                SoftwareName += RunQuery("BaseBoard", "Manufacturer");

            if (UseDiskDriveSignature == true)
                SoftwareName += RunQuery("DiskDrive", "Signature");

            if (UseVideoControllerCaption == true)
                SoftwareName += RunQuery("VideoController", "Caption");

            if (UsePhysicalMediaSerialNumber == true)
                SoftwareName += RunQuery("PhysicalMedia", "SerialNumber");

            if (UseBiosVersion == true)
                SoftwareName += RunQuery("BIOS", "Version");

            if (UseWindowsSerialNumber == true)
                SoftwareName += RunQuery("OperatingSystem", "SerialNumber");

            SoftwareName = RemoveUseLess(SoftwareName);

            if (SoftwareName.Length < 25)
                return GetSystemInfo(SoftwareName);

            return SoftwareName.Substring(0, 25).ToUpper();
        }

        private static string RemoveUseLess(string st)
        {
            char ch;
            for (int i = st.Length - 1; i >= 0; i--)
            {
                ch = char.ToUpper(st[i]);

                if ((ch < 'A' || ch > 'Z') &&
                    (ch < '0' || ch > '9'))
                {
                    st = st.Remove(i, 1);
                }
            }
            return st;
        }

        private static string RunQuery(string TableName, string MethodName)
        {
            ManagementObjectSearcher MOS = new ManagementObjectSearcher("Select * from Win32_" + TableName);
            foreach (ManagementObject MO in MOS.Get())
            {
                try
                {
                    return MO[MethodName].ToString();
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
            }
            return "";
        }
    }

    class FileReadWrite
    {
        // Key for TripleDES encryption
        public static byte[] key = { 21, 10, 64, 10, 100, 40, 200, 4,
                    21, 54, 65, 246, 5, 62, 1, 54,
                    54, 6, 8, 9, 65, 4, 65, 9};

        private static byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0 };

        public static string ReadFile(string FilePath)
        {
            FileInfo fi = new FileInfo(FilePath);
            if (fi.Exists == false)
                return string.Empty;

            FileStream fin = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            TripleDES tdes = new TripleDESCryptoServiceProvider();
            CryptoStream cs = new CryptoStream(fin, tdes.CreateDecryptor(key, iv), CryptoStreamMode.Read);

            StringBuilder SB = new StringBuilder();
            int ch;
            for (int i = 0; i < fin.Length; i++)
            {
                ch = cs.ReadByte();
                if (ch == 0)
                    break;
                SB.Append(Convert.ToChar(ch));
            }

            cs.Close();
            fin.Close();
            return SB.ToString();
        }

        public static void WriteFile(string FilePath, string Data)
        {
            FileStream fout = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write);
            TripleDES tdes = new TripleDESCryptoServiceProvider();
            CryptoStream cs = new CryptoStream(fout, tdes.CreateEncryptor(key, iv), CryptoStreamMode.Write);

            byte[] d = Encoding.ASCII.GetBytes(Data);
            cs.Write(d, 0, d.Length);
            cs.WriteByte(0);

            cs.Close();
            fout.Close();
        }
    }

    static class Encryption
    {
        static public string InverseByBase(string st, int MoveBase)
        {
            StringBuilder SB = new StringBuilder();
            //st = ConvertToLetterDigit(st);
            int c;
            for (int i = 0; i < st.Length; i += MoveBase)
            {
                if (i + MoveBase > st.Length - 1)
                    c = st.Length - i;
                else
                    c = MoveBase;
                SB.Append(InverseString(st.Substring(i, c)));
            }
            return SB.ToString();
        }

        static public string InverseString(string st)
        {
            StringBuilder SB = new StringBuilder();
            for (int i = st.Length - 1; i >= 0; i--)
            {
                SB.Append(st[i]);
            }
            return SB.ToString();
        }

        static public string ConvertToLetterDigit(string st)
        {
            StringBuilder SB = new StringBuilder();
            foreach (char ch in st)
            {
                if (char.IsLetterOrDigit(ch) == false)
                    SB.Append(Convert.ToInt16(ch).ToString());
                else
                    SB.Append(ch);
            }
            return SB.ToString();
        }

        /// <summary>
        /// moving all characters in string insert then into new index
        /// </summary>
        /// <param name="st">string to moving characters</param>
        /// <returns>moved characters string</returns>
        static public string Boring(string st)
        {
            int NewPlace;
            char ch;
            for (int i = 0; i < st.Length; i++)
            {
                NewPlace = i * Convert.ToUInt16(st[i]);
                NewPlace = NewPlace % st.Length;
                ch = st[i];
                st = st.Remove(i, 1);
                st = st.Insert(NewPlace, ch.ToString());
            }
            return st;
        }

        static public string MakePassword(string st, string Identifier)
        {
            if (Identifier.Length != 3)
                throw new ArgumentException("Identifier must be 3 character length");

            int[] num = new int[3];
            num[0] = Convert.ToInt32(Identifier[0].ToString(), 10);
            num[1] = Convert.ToInt32(Identifier[1].ToString(), 10);
            num[2] = Convert.ToInt32(Identifier[2].ToString(), 10);
            st = Boring(st);
            st = InverseByBase(st, num[0]);
            st = InverseByBase(st, num[1]);
            st = InverseByBase(st, num[2]);

            StringBuilder SB = new StringBuilder();
            foreach (char ch in st)
            {
                SB.Append(ChangeChar(ch, num));
            }
            return SB.ToString();
        }

        static private char ChangeChar(char ch, int[] EnCode)
        {
            ch = char.ToUpper(ch);
            if (ch >= 'A' && ch <= 'H')
                return Convert.ToChar(Convert.ToInt16(ch) + 2 * EnCode[0]);
            else if (ch >= 'I' && ch <= 'P')
                return Convert.ToChar(Convert.ToInt16(ch) - EnCode[2]);
            else if (ch >= 'Q' && ch <= 'Z')
                return Convert.ToChar(Convert.ToInt16(ch) - EnCode[1]);
            else if (ch >= '0' && ch <= '4')
                return Convert.ToChar(Convert.ToInt16(ch) + 5);
            else if (ch >= '5' && ch <= '9')
                return Convert.ToChar(Convert.ToInt16(ch) - 5);
            else
                return '0';
        }
    }

    // Activate Property
    public class TrialMaker
    {
        #region -> Private Variables 

        private string _BaseString;
        private string _Password;
        private string _SoftName;
        private string _RegFilePath;
        private string _HideFilePath;
        private int _DefDays;
        private int _Runed;
        private string _Text;
        private string _Identifier;
        public static bool calledOnce = false;
        #endregion

        #region -> Constructor 

        /// <summary>
        /// Make new TrialMaker class to make software trial
        /// </summary>
        /// <param name="SoftwareName">Name of software to make trial</param>
        /// <param name="RegFilePath">File path to save password(enrypted)</param>
        /// <param name="HideFilePath">file path for saving hidden information</param>
        /// <param name="Text">A text for contacting to you</param>
        /// <param name="TrialDays">Default period days</param>
        /// <param name="TrialRunTimes">How many times user can run as trial</param>
        /// <param name="Identifier">3 Digit string as your identifier to make password</param>
        public TrialMaker(string SoftwareName,
            string RegFilePath, string HideFilePath,
            string Text, int TrialDays, int TrialRunTimes,
            string Identifier)
        {
            _SoftName = SoftwareName;
            _Identifier = Identifier;

            SetDefaults();

            _DefDays = TrialDays;
            _Runed = TrialRunTimes;

            _RegFilePath = RegFilePath;
            _HideFilePath = HideFilePath;
            _Text = Text;
        }

        private void SetDefaults()
        {
            SystemInfo.UseBaseBoardManufacturer = false;
            SystemInfo.UseBaseBoardProduct = true;
            SystemInfo.UseBiosManufacturer = false;
            SystemInfo.UseBiosVersion = true;
            SystemInfo.UseDiskDriveSignature = true;
            SystemInfo.UsePhysicalMediaSerialNumber = false;
            SystemInfo.UseProcessorID = true;
            SystemInfo.UseVideoControllerCaption = false;
            SystemInfo.UseWindowsSerialNumber = false;

            MakeBaseString();
            MakePassword();
        }

        #endregion

        // Make base string (Computer ID)
        private void MakeBaseString()
        {
            _BaseString = Encryption.Boring(Encryption.InverseByBase(SystemInfo.GetSystemInfo(_SoftName), 10));
        }

        private void MakePassword()
        {
            _Password = Encryption.MakePassword(_BaseString, _Identifier);
        }

        /// <summary>
        /// Show registering dialog to user
        /// </summary>
        /// <returns>Type of running</returns>
        public RunTypes ShowDialog(System.Windows.Window parent=null)
        {
            // check if registered before
            if (CheckRegister() == true)
            {
                DevelopWorkspace.Base.license.IsTrialLicense = true;
                return RunTypes.Full;

            }

            DevelopWorkspace.Base.license.IsTrialLicense = true;
            int dayLeft = DaysToEnd();
            DevelopWorkspace.Base.license.DaysToEnd = _DefDays.ToString();
            DevelopWorkspace.Base.license.Runed = _Runed.ToString();

            frmDialog PassDialog = new frmDialog(_BaseString, _Password, _DefDays, _Runed, _Text);
            
            MakeHideFile();


            //当在这个期限范围内提醒用户这是试用版
            if (parent == null && (_DefDays < 30 || _Runed < 50))
            {
                if(calledOnce == false) {
                    calledOnce = true;
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                }

                //为了在WPF的mainwindow之上显示
                DialogResult DR;
                DR = PassDialog.ShowDialog(new WpfWindowWrapper(System.Windows.Application.Current.MainWindow));
                if (DR == System.Windows.Forms.DialogResult.OK)
                {
                    MakeRegFile();
                    return RunTypes.Full;
                }
                else if (DR == DialogResult.Retry)
                    return RunTypes.Trial;
                else
                    return RunTypes.Expired;
            }
            else if (parent != null) {

                if (calledOnce == false)
                {
                    calledOnce = true;
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                }
                //为了在WPF的mainwindow之上显示
                DialogResult DR;
                DR = PassDialog.ShowDialog(new WpfWindowWrapper(parent));

                if (DR == System.Windows.Forms.DialogResult.OK)
                {
                    MakeRegFile();
                    return RunTypes.Full;
                }
                else if (DR == DialogResult.Retry)
                    return RunTypes.Trial;
                else
                    return RunTypes.Expired;
            }
            else
            {
                return RunTypes.Trial;
            }
        }

        // save password to Registration file for next time usage
        private void MakeRegFile()
        {
            FileReadWrite.WriteFile(_RegFilePath, _Password);
        }

        // Control Registeration file for password
        // if password saved correctly return true else false
        private bool CheckRegister()
        { 
            string Password = FileReadWrite.ReadFile(_RegFilePath);

            if (_Password == Password)
                return true;
            else
                return false;
        }

        // from hidden file
        // indicate how many days can user use program
        // if the file does not exists, make it
        private int DaysToEnd()
        {
            FileInfo hf = new FileInfo(_HideFilePath);
            if (hf.Exists == false)
            {
                MakeHideFile();
                return _DefDays;
            }
            return CheckHideFile();
        }

        // store hidden information to hidden file
        // Date,DaysToEnd,HowManyTimesRuned,BaseString(ComputerID)
        private void MakeHideFile()
        {
            string HideInfo;
            HideInfo = DateTime.Now.Ticks + ";";
            HideInfo += _DefDays + ";" + _Runed + ";" + _BaseString;
            FileReadWrite.WriteFile(_HideFilePath, HideInfo);
        }

        // Get Data from hidden file if exists
        private int CheckHideFile()
        {
            string[] HideInfo;
            HideInfo = FileReadWrite.ReadFile(_HideFilePath).Split(';');
            long DiffDays;
            int DaysToEnd;

            if (_BaseString == HideInfo[3])
            {
                DaysToEnd = Convert.ToInt32(HideInfo[1]);
                if (DaysToEnd <= 0)
                {
                    _Runed = 0;
                    _DefDays = 0;
                    return 0;
                }
                DateTime dt = new DateTime(Convert.ToInt64(HideInfo[0]));
                DiffDays = DateAndTime.DateDiff(DateInterval.Day,
                    dt.Date, DateTime.Now.Date,
                    FirstDayOfWeek.Saturday,
                    FirstWeekOfYear.FirstFullWeek);
                
                DaysToEnd = Convert.ToInt32(HideInfo[1]);
                _Runed = Convert.ToInt32(HideInfo[2]);
                _Runed -= 1;

                DiffDays = Math.Abs(DiffDays);

                _DefDays = DaysToEnd - Convert.ToInt32(DiffDays);
            }
            return _DefDays;
        }

        public enum RunTypes
        { 
            Trial = 0,
            Full,
            Expired,
            UnKnown
        }

        #region -> Properties 

        /// <summary>
        /// Indicate File path for storing password
        /// </summary>
        public string RegFilePath
        {
            get
            {
                return _RegFilePath;
            }
            set
            {
                _RegFilePath = value;
            }
        }

        /// <summary>
        /// Indicate file path for storing hidden information
        /// </summary>
        public string HideFilePath
        {
            get
            {
                return _HideFilePath;
            }
            set
            {
                _HideFilePath = value;
            }
        }

        /// <summary>
        /// Get default number of days for trial period
        /// </summary>
        public int TrialPeriodDays
        {
            get
            {
                return _DefDays;
            }
        }

        /// <summary>
        /// Get or Set TripleDES key for encrypting files to save
        /// </summary>
        public byte[] TripleDESKey
        {
            get
            {
                return FileReadWrite.key;
            }
            set
            {
                FileReadWrite.key = value;
            }
        }

        #endregion

        #region -> Usage Properties 

        public bool UseProcessorID
        {
            get
            {
                return SystemInfo.UseProcessorID;
            }
            set
            {
                SystemInfo.UseProcessorID = value;
            }
        }

        public bool UseBaseBoardProduct
        {
            get
            {
                return SystemInfo.UseBaseBoardProduct;
            }
            set
            {
                SystemInfo.UseBaseBoardProduct = value;
            }
        }

        public bool UseBaseBoardManufacturer
        {
            get
            {
                return SystemInfo.UseBiosManufacturer;
            }
            set
            {
                SystemInfo.UseBiosManufacturer = value;
            }
        }

        public bool UseDiskDriveSignature
        {
            get
            {
                return SystemInfo.UseDiskDriveSignature;
            }
            set
            {
                SystemInfo.UseDiskDriveSignature = value;
            }
        }

        public bool UseVideoControllerCaption
        {
            get
            {
                return SystemInfo.UseVideoControllerCaption;
            }
            set
            {
                SystemInfo.UseVideoControllerCaption = value;
            }
        }

        public bool UsePhysicalMediaSerialNumber
        {
            get
            {
                return SystemInfo.UsePhysicalMediaSerialNumber;
            }
            set
            {
                SystemInfo.UsePhysicalMediaSerialNumber = value;
            }
        }

        public bool UseBiosVersion
        {
            get
            {
                return SystemInfo.UseBiosVersion;
            }
            set
            {
                SystemInfo.UseBiosVersion = value;
            }
        }

        public bool UseBiosManufacturer
        {
            get
            {
                return SystemInfo.UseBiosManufacturer;
            }
            set
            {
                SystemInfo.UseBiosManufacturer = value;
            }
        }

        public bool UseWindowsSerialNumber
        {
            get
            {
                return SystemInfo.UseWindowsSerialNumber;
            }
            set
            {
                SystemInfo.UseWindowsSerialNumber = value;
            }
        }

        #endregion
    }
}