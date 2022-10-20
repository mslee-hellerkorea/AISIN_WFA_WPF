using AISIN_WFA.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public class MxWrapper
    {
        public int iErrorCode = 0;
        public string sErrorCode = "";

        private int stationNumber = 7;

        private Object thisLock = new Object();

        private ActUtlTypeLib.ActUtlType actUtlType = null;
        readonly object monObject = new object();

        private bool bConnected = false;
        public bool BConnected { get => bConnected; }
        public string SErrorCode { get => sErrorCode; }
        public int IErrorCode { get => iErrorCode; }

        System.Timers.Timer timer = new System.Timers.Timer();

        public delegate void OnDeviceStatus(string szDevice, int lData, int lReturnCode);
        public event OnDeviceStatus OnDeviceStatusEventHandler;

        public MxWrapper(int station)
        {
            try
            {
                stationNumber = station;
                actUtlType = new ActUtlTypeLib.ActUtlType();
                actUtlType.ActLogicalStationNumber = station;
                actUtlType.Open();
                actUtlType.OnDeviceStatus += new ActUtlTypeLib._IActUtlTypeEvents_OnDeviceStatusEventHandler(OnDeviceStatusEvent);

                timer.Interval = 1000;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elasped);
                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", String.Format("MxWrapper -- Message: {0}", ex.Message));
            }
        }

        private void OnDeviceStatusEvent(string szDevice, int lData, int lReturnCode)
        {
            try
            {
                if (lReturnCode == 0)
                {
                    return;
                }

                OnDeviceStatusEventHandler?.BeginInvoke(szDevice, lData, lReturnCode, null, null);

                sErrorCode = String.Format("0x{0:x8} [HEX]", lReturnCode);
                HLog.log("INFO", String.Format("OnDeviceStatusEvent -- PLC Error Code: {0}", sErrorCode));

                iErrorCode = lReturnCode;
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", String.Format("OnDeviceStatusEvent -- Message: {0}", ex.Message));
            }
        }

        void timer_Elasped(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Enabled = false;

            if (!IsOnline())
            {
                HLog.log("INFO", String.Format("Start retry open PLC..."));
                Open(stationNumber);
                HLog.log("INFO", String.Format("End retry open PLC..."));
            }

            timer.Enabled = true;
        }

        public bool Open(int iLogicalStationNumber)
        {
            int iRst = -1;

            this.stationNumber = iLogicalStationNumber;

            actUtlType.ActLogicalStationNumber = this.stationNumber;

            actUtlType.ActPassword = "";
            HLog.log("INFO", String.Format("retry open PLC..."));
            lock (thisLock)
            {
                try
                {
                    actUtlType.Close();
                    iRst = actUtlType.Open();
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("Open -- Message: {0}", ex.Message));
                }
            }

            if (iRst == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Close()
        {
            timer.Enabled = false;
            if (!bConnected) return true;

            int iRst = -1;
            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.Close();
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("Close -- Message: {0}", ex.Message));
                }
            }

            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }


        short[] sInt = new short[100];


        public int Initialization(int iLogicalStationNumber)
        {
            this.stationNumber = iLogicalStationNumber;

            return 0;
        }

        public bool IsOnline()
        {

            int iRst = -1;
            int iData = 0;

            lock (thisLock)
            {
                try
                {
                    // FX M8000 is always on / M8001 is always off
                    iRst = actUtlType.GetDevice("M8000", out iData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("IsOnline -- Message: {0}", ex.Message));
                }
            }

            if (iRst == 0 && iData == 1)
            {
                bConnected = true;
                return true;
            }

            bConnected = false;
            return false;
        }

        public bool EntryDeviceStatus(string szDeviceList, int lSize, int lMonitorCycle, ref int lplData)
        {
            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.EntryDeviceStatus(szDeviceList, lSize, lMonitorCycle, ref lplData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("EntryDeviceStatus -- Message: {0}", ex.Message));
                }
            }

            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public bool FreeDeviceStatus()
        {
            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.FreeDeviceStatus();
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("FreeDeviceStatus -- Message: {0}", ex.Message));
                }
            }

            if (iRst == 0) return true;

            SetError(iRst);
            return false;

        }


        #region Function : Read From PLC

        public bool ReadBuffer(int lStartIO, int lAddress, int lSize, out short lplData)
        {
            lplData = 0;

            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.ReadBuffer(lStartIO, lAddress, lSize, out lplData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("ReadBuffer -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public bool ReadDeviceBlock(string _sAdd, out string _str)
        {
            sInt = new short[10];
            Array.Clear(sInt, 0, sInt.Length);
            bool bCheck = ReadDeviceBlock2(_sAdd, sInt.Length, out sInt[0]);

            _str = "";
            if (bCheck)
            {
                for (int i = 0; i < sInt.Length; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(sInt[i]);
                    _str += Encoding.Default.GetString(bytes);
                }
            }

            return bCheck;
        }

        public bool ReadDeviceBlock(string szDevice, int iSize, out int lplData)
        {
            lplData = 0;

            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.ReadDeviceBlock(szDevice, iSize, out lplData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("ReadDeviceBlock -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public int ReadDeviceBlockInt(string szDevice, int iSize, out int lplData)
        {
            lplData = 0;

            if (!bConnected) return -1;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.ReadDeviceBlock(szDevice, iSize, out lplData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("ReadDeviceBlock -- Message: {0}", ex.Message));
                }
            }
            SetError(iRst);
            return iRst;
        }

        public bool ReadDeviceBlock2(string szDevice, int iSize, out short lplData)
        {
            lplData = 0;

            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.ReadDeviceBlock2(szDevice, iSize, out lplData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("ReadDeviceBlock2 -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public bool GetStringValue(string strStartAddr, int iAddrLength, ref string strGetting)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] readByte = new byte[iAddrLength * 2];
            byte[] ExceptFromNullByte = new byte[iAddrLength * 2];
            byte nullByte = 0x00;
            short[] readShort = new short[iAddrLength];
            bool IsRead = false;

            try
            {
                bool iRet = ReadDeviceBlock2(strStartAddr, iAddrLength, out readShort[0]);

                if (iRet == false)
                    return false;

                IsRead = true;

                for (int i = 0; i < iAddrLength; i++)
                {
                    byte[] tmpByte = BitConverter.GetBytes(readShort[i]);
                    readByte[i * 2] = tmpByte[0];
                    readByte[i * 2 + 1] = tmpByte[1];
                }

                for (int i = 0; i < iAddrLength * 2; i++)
                {
                    if (readByte[i].Equals(nullByte))
                    {
                        break;
                    }
                    else
                    {
                        ExceptFromNullByte[i] = readByte[i];
                    }
                }

                strGetting = CleanFileName(Encoding.ASCII.GetString(ExceptFromNullByte));

            }
            catch (Exception ex)
            {
                HLog.log("ERROR", String.Format("GetStringValue -- Message: {0}", ex.Message));
            }

            return IsRead;
        }

        public bool GetDevice(string sDevice, out int lplData)
        {
            lplData = 0;

            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.GetDevice(sDevice, out lplData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("GetDevice -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public bool GetDevice2(string szDevice, out short lplData)
        {
            lplData = 0;

            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.GetDevice2(szDevice, out lplData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("GetDevice2 -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public int GetDoubleDevice(string device)
        {
            int[] data = new int[2];
            int Yint = -100000;
            try
            {
                actUtlType.ReadDeviceBlock(device, 2, out data[0]);
                string value1 = Convert.ToString(data[0], 2);
                string value2 = Convert.ToString(data[1], 2);
                string bitString = value2 + value1;
                Yint = Convert.ToInt32(bitString, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Yint;
        }


        #endregion


        #region Function : Write to PLC
        public bool WriteBuffer(int lStartIO, int lAddress, int lSize, ref short lpsData)
        {
            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.WriteBuffer(lStartIO, lAddress, lSize, ref lpsData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("WriteBuffer -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;

        }

        public bool WriteDeviceBlock(string _sAdd, string _str)
        {
            if (_str == "")
            {
                sInt = new short[10];
                Array.Clear(sInt, 0, sInt.Length);
                bool bCheck1 = WriteDeviceBlock2(_sAdd, sInt.Length, ref sInt[0]);
                return bCheck1;
            }

            string str = _str;
            string[] str_temp;

            if (str.Length % 2 == 0)
            {
                str_temp = new string[str.Length / 2];
                sInt = new short[str_temp.Length];

                for (int i = 0; i < str.Length / 2; i++)
                {
                    str_temp[i] = str.Substring(i * 2, 2);
                }

                for (int i = 0; i < str_temp.Length; i++)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(str_temp[i]);
                    short sh = BitConverter.ToInt16(bytes, 0);
                    sInt[i] = sh;
                }
            }
            else
            {
                str_temp = new string[(str.Length / 2) + 1];
                sInt = new short[str_temp.Length];

                for (int i = 0; i < str.Length / 2 + 1; i++)
                {
                    if (i < (str.Length - 1) / 2)
                        str_temp[i] = str.Substring(i * 2, 2);
                    else
                        str_temp[i] = str.Substring(i * 2, 1);
                }

                for (int i = 0; i < str_temp.Length; i++)
                {
                    if (i < str_temp.Length - 1)
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(str_temp[i]);
                        short sh = BitConverter.ToInt16(bytes, 0);
                        sInt[i] = sh;
                    }
                    else
                    {
                        char data = Convert.ToChar(str_temp[i].Substring(0, 1));
                        sInt[i] = (short)data;
                    }
                }
            }

            bool bCheck2 = WriteDeviceBlock2(_sAdd, sInt.Length, ref sInt[0]);
            return bCheck2;

        }

        public bool WriteDeviceBlock(string szDevice, int iSize, ref int iData)
        {
            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.WriteDeviceBlock(szDevice, iSize, ref iData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("WriteDeviceBlock -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public int WriteDeviceBlockint(string szDevice, int iSize, ref int iData)
        {
            if (!bConnected) return -1;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.WriteDeviceBlock(szDevice, iSize, ref iData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("WriteDeviceBlock -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return iRst;

            SetError(iRst);
            return iRst;
        }

        public bool WriteDeviceBlock2(string szDevice, int iSize, ref short iData)
        {
            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.WriteDeviceBlock2(szDevice, iSize, ref iData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("WriteDeviceBlock2 -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public bool SetStringValue(string strStartAddr, int iAddrLength, string strSetting, ref string strErrorMsg)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] readyToGoBufferByte = encoding.GetBytes(strSetting);
            short[] readyToGoBufferShort = new short[iAddrLength];
            int iLengthofBuffer = Math.Min(iAddrLength * 2, readyToGoBufferByte.Length);
            bool IsSetValue = false;

            for (int i = 0; i <= iLengthofBuffer - 2; i += 2)
            {
                readyToGoBufferShort[i / 2] = BitConverter.ToInt16(readyToGoBufferByte, i);
            }
            if (iLengthofBuffer % 2 == 1) readyToGoBufferShort[iLengthofBuffer / 2] = readyToGoBufferByte[iLengthofBuffer - 1];

            try
            {
                bool iRet = WriteDeviceBlock2(strStartAddr, iAddrLength, ref readyToGoBufferShort[0]);
                if (iRet == false)
                    return false;

                IsSetValue = true;

            }
            catch (Exception ex)
            {
                HLog.log("ERROR", String.Format("SetStringValue -- Message: {0}", ex.Message));
            }

            return IsSetValue;
        }

        public bool SetCpuStatus(int iOperation)
        {
            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.SetCpuStatus(iOperation);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("SetCpuStatus -- Message: {0}", ex.Message));
                }
            }

            if (iRst == 0) return true;

            SetError(iRst);
            return false;

        }

        private void SetError(int _iErrorCode)
        {
            sErrorCode = String.Format("0x{0:x8} [HEX]", iErrorCode);
            iErrorCode = _iErrorCode;
            HLog.log("INFO", String.Format("MxComponent Error --- {0}", sErrorCode));
        }

        public bool SetDevice(string sDevice, int iData)
        {
            if (!bConnected) return false;

            int iRst = -1;

            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.SetDevice(sDevice, iData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("SetDevice -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public bool SetDevice2(string szDevice, short lData)
        {
            if (!bConnected) return false;

            int iRst = -1;
            lock (thisLock)
            {
                try
                {
                    iRst = actUtlType.SetDevice2(szDevice, lData);
                }
                catch (Exception ex)
                {
                    HLog.log("ERROR", String.Format("SetDevice2 -- Message: {0}", ex.Message));
                }
            }
            if (iRst == 0) return true;

            SetError(iRst);
            return false;
        }

        public bool SetDoubleDevice(string address, int value)
        {
            Int32 n = -2;
            string nt = Convert.ToString(n, 2);
            // Int32 n1 = Convert.ToInt32(nt, 2);
            UInt32 n2 = Convert.ToUInt32(nt, 2);

            int[] dataTemp = new int[2];
            long a = int.MaxValue;
            long b = int.MinValue;
            long c = a - b;
            long d = 0;
            if (value > 0)
            {

                dataTemp[0] = (int)(value & 0x0000ffff);
                dataTemp[1] = (int)(value >> 16);
            }
            else
            {
                d = c + value + 1;

                dataTemp[0] = (int)(d & 0x0000ffff);
                dataTemp[1] = (int)(d >> 16);
            }
            int result = actUtlType.WriteDeviceBlock(address, 2, ref dataTemp[0]);
            if (0 == result)
                return true;
            else
                return false;

        }

        #endregion


        #region Function : Converter

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public string StringToHex(string str)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public string HexToString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes);
        }

        public string IntToHex(int _int)
        {
            string sHex = _int.ToString("X");
            return sHex;
        }

        public int HexToInt(string _string)
        {
            int iInt = int.Parse(_string, System.Globalization.NumberStyles.HexNumber);
            return iInt;
        }

        public byte[] StrToByteArray(string str)
        {
            Dictionary<string, byte> hexindex = new Dictionary<string, byte>();
            for (int i = 0; i <= 15; i++)
                hexindex.Add(i.ToString("X2"), (byte)i);

            List<byte> hexres = new List<byte>();
            for (int i = 0; i < str.Length; i += 2)
                hexres.Add(hexindex[str.Substring(i, 2)]);

            return hexres.ToArray();
        }

        #endregion
    }
}
