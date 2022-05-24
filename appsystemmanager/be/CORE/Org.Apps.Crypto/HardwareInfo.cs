using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Org.Apps.Crypto
{
    public static class HardwareInfo
    {
        private static string _fingerPrint = string.Empty;

        public static string GenerateUniqueId()
        {
            if (string.IsNullOrEmpty(_fingerPrint))
            {
                _fingerPrint = GetHash(CpuId +
                                       BiosId +
                                       MotherboardId +
                                       DiskId +
                                       VideoId +
                                       MacId);
            }
            return _fingerPrint;
        }

        private static string GetHash(string s)
        {
            using (MD5 sec = new MD5CryptoServiceProvider())
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] bt = enc.GetBytes(s);
                return GetHexString(sec.ComputeHash(bt));
            }
        }

        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        #region Original Device ID Getting Code

        //Return a hardware identifier
        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = string.Empty;
            using (ManagementClass managementClass = new ManagementClass(wmiClass))
            {
                ManagementObjectCollection managementObjCollection = managementClass.GetInstances();
                foreach (ManagementObject managementObj in managementObjCollection)
                {
                    if (managementObj[wmiMustBeTrue].ToString() == "True")
                    {
                        //Only get the first one
                        if (string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                result = managementObj[wmiProperty].ToString();
                                break;
                            }
                            catch
                            {
                                // TODO
                            }
                        }
                    }
                }
            }
            return result;
        }

        //Return a hardware identifier
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string result = string.Empty;
            using (ManagementClass managementClass = new ManagementClass(wmiClass))
            {
                ManagementObjectCollection managementObjCollection = managementClass.GetInstances();
                foreach (ManagementObject managementObj in managementObjCollection)
                {
                    //Only get the first one
                    if (string.IsNullOrEmpty(result))
                    {
                        try
                        {
                            result = managementObj[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                            // TODO
                        }
                    }
                }
            }
            return result;
        }

        #endregion Original Device ID Getting Code

        #region Properties

        private static string CpuId
        {
            get
            {
                //Uses first CPU identifier available in order of preference
                //Don't get all identifiers, as it is very time consuming
                string retVal = Identifier("Win32_Processor", "UniqueId");
                if (string.IsNullOrEmpty(retVal)) //If no UniqueID, use ProcessorID
                {
                    retVal = Identifier("Win32_Processor", "ProcessorId");
                    if (string.IsNullOrEmpty(retVal)) //If no ProcessorId, use Name
                    {
                        retVal = Identifier("Win32_Processor", "Name");
                        if (string.IsNullOrEmpty(retVal)) //If no Name, use Manufacturer
                        {
                            retVal = Identifier("Win32_Processor", "Manufacturer");
                        }
                        //Add clock speed for extra security
                        retVal += Identifier("Win32_Processor", "MaxClockSpeed");
                    }
                }
                return retVal;
            }
        }

        //BIOS Identifier
        private static string BiosId
        {
            get
            {
                return Identifier("Win32_BIOS", "Manufacturer") +
                       Identifier("Win32_BIOS", "SMBIOSBIOSVersion") +
                       Identifier("Win32_BIOS", "IdentificationCode") +
                       Identifier("Win32_BIOS", "SerialNumber") +
                       Identifier("Win32_BIOS", "ReleaseDate") +
                       Identifier("Win32_BIOS", "Version");
            }
        }

        //Main physical hard drive ID
        private static string DiskId
        {
            get
            {
                return Identifier("Win32_DiskDrive", "Model") +
                       Identifier("Win32_DiskDrive", "Manufacturer") +
                       Identifier("Win32_DiskDrive", "Signature") +
                       Identifier("Win32_DiskDrive", "TotalHeads");
            }
        }

        //Motherboard ID
        private static string MotherboardId
        {
            get
            {
                return Identifier("Win32_BaseBoard", "Model") +
                       Identifier("Win32_BaseBoard", "Manufacturer") +
                       Identifier("Win32_BaseBoard", "Name") +
                       Identifier("Win32_BaseBoard", "SerialNumber");
            }
        }

        //Primary video controller ID
        private static string VideoId
        {
            get
            {
                return Identifier("Win32_VideoController", "DriverVersion") +
                       Identifier("Win32_VideoController", "Name");
            }
        }

        //First enabled network card ID
        private static string MacId
        {
            get
            {
                return Identifier("Win32_NetworkAdapterConfiguration",
                                  "MACAddress", "IPEnabled");
            }
        }

        #endregion Properties
    }
}