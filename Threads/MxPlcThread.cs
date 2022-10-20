using AISIN_WFA.Model;
using AISIN_WFA.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AISIN_WFA.Util.AisinEnums;

namespace AISIN_WFA.Threads
{
    public class MxPlcThread
    {
        private bool IsInitUpstreamLane1 = false;
        private bool IsInitUpstreamLane2 = false;
        private bool IsInitDownstreamLane1 = false;
        private bool IsInitDownstreamLane2 = false;
        private int stationNumber = 0;
        private int barcodeAddrLength = 12;
        private const int TCP_MSG_MAX = 32;

        public void Run()
        {
            while (AisinParameters.Instance.RunFlag)
            {
                try
                {
                    if (!IsInitUpstreamLane1)
                        initialization(ePlcConnectTo.UpstreamLane1);
                    else
                    {
                        if (AisinCollections.Instance.MxWrapperUpstreamLane1.BConnected)
                            UpstreamLane1();
                    }

                    if (!IsInitUpstreamLane2)
                        initialization(ePlcConnectTo.UpstreamLane2);
                    else
                    {
                        if (AisinCollections.Instance.MxWrapperUpstreamLane2.BConnected)
                            UpstreamLane2();
                    }

                    if (!IsInitDownstreamLane1)
                        initialization(ePlcConnectTo.DownstreamLane1);
                    else
                    {
                        if (AisinCollections.Instance.MxWrapperDownstreamLane1.BConnected)
                            DownstreamLane1();
                    }

                    if (!IsInitDownstreamLane2)
                        initialization(ePlcConnectTo.DownstreamLane2);
                    else
                    {
                        if (AisinCollections.Instance.MxWrapperDownstreamLane2.BConnected)
                            DownstreamLane2();
                    }

                }
                catch (Exception ex)
                {
                    HLog.log(HLog.eLog.EXCEPTION, $"Run - {ex.Message}");
                }

                Thread.Sleep(AisinCollections.Instance.AisinSetup.GeneralSetup.PlcUpdatePeriod);
            }

        }

        private void UpstreamLane1()
        {
            try
            {
                // Read Board Available
                string BaAddr = AisinCollections.Instance.AisinSetup.PlcSetup.MxAddrBoardAvaiableLane1;
                int readBaData = 0;
                int iReturnCode = AisinCollections.Instance.MxWrapperUpstreamLane1.ReadDeviceBlockInt(BaAddr, 1, out readBaData);
                if (iReturnCode != 0)
                {
                    throw new Exception("Return code not 0, code: " + iReturnCode);
                }
                AisinCollections.Instance.UpstreamPlcData.BoadAvailableLane1 = readBaData;

                // Read Barcode data
                StringBuilder bcrSb = new StringBuilder();
                string BcrAddr = AisinCollections.Instance.AisinSetup.PlcSetup.MxAddrBarcodeLane1;
                int[] readBcrData = new int[barcodeAddrLength];
                byte[] BarcodeData = new byte[TCP_MSG_MAX];

                iReturnCode = AisinCollections.Instance.MxWrapperUpstreamLane1.ReadDeviceBlockInt(BcrAddr, barcodeAddrLength, out readBcrData[0]);
                if (iReturnCode != 0)
                {
                    throw new Exception("Return code not 0, code: " + iReturnCode);
                }

                UpdateBarcodeString();

                if (AisinCollections.Instance.BarcodeState.BASignalLane1 == false &&
                    AisinCollections.Instance.UpstreamPlcData.BoadAvailableLane1 != 0)
                {
                    // TODO :  LogWrite("Clear barcode array");
                    // Clear
                    Array.Clear(BarcodeData, 0, TCP_MSG_MAX);

                    // Line1 Barcode
                    for (int ndx = 0; ndx < Constant.BARCODE_MAX; ndx++)
                    {
                        byte barcodeDigit;

                        // Little endian
                        if ((ndx & 1) == 0)
                            barcodeDigit = (byte)(readBcrData[ndx / 2] & 0xFF);
                        else
                            barcodeDigit = (byte)(readBcrData[ndx / 2] >> 8 & 0xFF);
                        if (barcodeDigit == 1)
                            break;
                        BarcodeData[ndx] = barcodeDigit;
                        bcrSb.Append(barcodeDigit.ToString());
                        bcrSb.Append(" ");
                        AisinCollections.Instance.UpstreamPlcData.BarcodeDigitLane1 = barcodeDigit.ToString();
                    }
                    AisinCollections.Instance.UpstreamPlcData.BarcodeDigitLane1 = bcrSb.ToString();

                    if (BarcodeData[0] != 0)
                    {
                        string barcodeFromUpLane = string.Empty;
                        barcodeFromUpLane = System.Text.Encoding.Default.GetString(BarcodeData);
                        barcodeFromUpLane = Encoding.ASCII.GetString(BarcodeData, 0, BarcodeData.Length);
                        int offset = barcodeFromUpLane.IndexOf("\r\n");
                        if (offset < 0)
                            offset = barcodeFromUpLane.IndexOf('\r');
                        if (offset < 0)
                            offset = barcodeFromUpLane.IndexOf('\n');
                        if (offset > 0)
                            barcodeFromUpLane = barcodeFromUpLane.Substring(0, offset);

                        barcodeFromUpLane = barcodeFromUpLane.Trim().Replace(" ", "");
                        AisinCollections.Instance.UpstreamPlcData.BarcodeStringLane1 = barcodeFromUpLane;
                    }

                    // do nothing if hold smema until barcode scan is not enabled.
                    if (!AisinCollections.Instance.AisinSetup.BarcodeSetup.HoldSmemaUntilBarcode)
                        return;

                    if (!AisinCollections.Instance.AisinSetup.BarcodeSetup.AutoChangeRecipeWidthSpeed)
                    {
                        // change recipe,  width, speed not enabled, release smema when barcode is scanned.
                        if (BarcodeData[0] != 0)
                        {
                            //LogWrite("Auto load recipe not enabled, release smema for l.");
                            AisinCollections.Instance.OcxWrapper.SetSmema(0, 0);
                        }
                        return;
                    }

                    if (BarcodeData[0] != 0)
                    {
                        //LogWrite("Start to check barcode recipe for lane1");
                        AisinCollections.Instance.BarcodeControl.CheckBarcodeRecipe(0, AisinCollections.Instance.UpstreamPlcData.BarcodeStringLane1);
                    }

                    AisinCollections.Instance.BarcodeState.BASignalLane1 = true;
                }

                if (AisinCollections.Instance.BarcodeState.BASignalLane1 &&
                            AisinCollections.Instance.UpstreamPlcData.BoadAvailableLane1 == 0)
                {
                    AisinCollections.Instance.BarcodeState.BASignalLane1 = false;
                }
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, $"UpstreamLane1 - {ex.Message}");
            }
        }

        private void UpstreamLane2()
        {
            try
            {
                // Read Board Available
                string BaAddr = AisinCollections.Instance.AisinSetup.PlcSetup.MxAddrBoardAvaiableLane2;
                int readBaData = 0;
                int iReturnCode = AisinCollections.Instance.MxWrapperUpstreamLane2.ReadDeviceBlockInt(BaAddr, 1, out readBaData);
                if (iReturnCode != 0)
                {
                    throw new Exception("Return code not 0, code: " + iReturnCode);
                }
                AisinCollections.Instance.UpstreamPlcData.BoadAvailableLane2 = readBaData;

                // Read Barcode data
                StringBuilder bcrSb = new StringBuilder();
                string BcrAddr = AisinCollections.Instance.AisinSetup.PlcSetup.MxAddrBarcodeLane2;
                int[] readBcrData = new int[barcodeAddrLength];
                byte[] BarcodeData = new byte[TCP_MSG_MAX];

                iReturnCode = AisinCollections.Instance.MxWrapperUpstreamLane2.ReadDeviceBlockInt(BcrAddr, barcodeAddrLength, out readBcrData[0]);
                if (iReturnCode != 0)
                {
                    throw new Exception("Return code not 0, code: " + iReturnCode);
                }

                if (AisinCollections.Instance.BarcodeState.BASignalLane2 == false &&
                    AisinCollections.Instance.UpstreamPlcData.BoadAvailableLane2 != 0)
                {
                    // TODO :  LogWrite("Clear barcode array");
                    // Clear
                    Array.Clear(BarcodeData, 0, TCP_MSG_MAX);

                    // Line1 Barcode
                    for (int ndx = 0; ndx < Constant.BARCODE_MAX; ndx++)
                    {
                        byte barcodeDigit;

                        // Little endian
                        if ((ndx & 1) == 0)
                            barcodeDigit = (byte)(readBcrData[ndx / 2] & 0xFF);
                        else
                            barcodeDigit = (byte)(readBcrData[ndx / 2] >> 8 & 0xFF);
                        if (barcodeDigit == 1)
                            break;
                        BarcodeData[ndx] = barcodeDigit;
                        bcrSb.Append(barcodeDigit.ToString());
                        bcrSb.Append(" ");
                        AisinCollections.Instance.UpstreamPlcData.BarcodeDigitLane2 = barcodeDigit.ToString();
                    }
                    AisinCollections.Instance.UpstreamPlcData.BarcodeDigitLane2 = bcrSb.ToString();

                    if (BarcodeData[0] != 0)
                    {
                        string barcodeFromUpLane = string.Empty;
                        barcodeFromUpLane = System.Text.Encoding.Default.GetString(BarcodeData);
                        barcodeFromUpLane = Encoding.ASCII.GetString(BarcodeData, 0, BarcodeData.Length);
                        int offset = barcodeFromUpLane.IndexOf("\r\n");
                        if (offset < 0)
                            offset = barcodeFromUpLane.IndexOf('\r');
                        if (offset < 0)
                            offset = barcodeFromUpLane.IndexOf('\n');
                        if (offset > 0)
                            barcodeFromUpLane = barcodeFromUpLane.Substring(0, offset);

                        barcodeFromUpLane = barcodeFromUpLane.Trim().Replace(" ", "");
                        AisinCollections.Instance.UpstreamPlcData.BarcodeStringLane2 = barcodeFromUpLane;
                    }

                    // do nothing if hold smema until barcode scan is not enabled.
                    if (!AisinCollections.Instance.AisinSetup.BarcodeSetup.HoldSmemaUntilBarcode)
                        return;

                    if (!AisinCollections.Instance.AisinSetup.BarcodeSetup.AutoChangeRecipeWidthSpeed)
                    {
                        // change recipe,  width, speed not enabled, release smema when barcode is scanned.
                        if (BarcodeData[0] != 0)
                        {
                            //LogWrite("Auto load recipe not enabled, release smema for l.");
                            AisinCollections.Instance.OcxWrapper.SetSmema(1, 0);
                        }
                        return;
                    }

                    if (BarcodeData[0] != 0)
                    {
                        //LogWrite("Start to check barcode recipe for lane1");
                        AisinCollections.Instance.BarcodeControl.CheckBarcodeRecipe(1, AisinCollections.Instance.UpstreamPlcData.BarcodeStringLane2);
                    }

                    AisinCollections.Instance.BarcodeState.BASignalLane2 = true;
                }

                if (AisinCollections.Instance.BarcodeState.BASignalLane2 &&
                            AisinCollections.Instance.UpstreamPlcData.BoadAvailableLane2 == 0)
                {
                    AisinCollections.Instance.BarcodeState.BASignalLane2 = false;
                }
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, $"UpstreamLane2 - {ex.Message}");
            }
        }

        private void UpdateBarcodeString()
        {
            try
            {

            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, $"UpdateBarcodeString - {ex.Message}");
            }
        }


        private void DownstreamLane1()
        {
            try
            {
                float railWidth = 0;
                int data = 0;
                string addr = AisinCollections.Instance.AisinSetup.PlcSetup.MxAddrRailWidthLane1;

                switch (AisinCollections.Instance.AisinSetup.LaneRailSetup.Lane1Rail)
                {
                    case eLaneRail.Rail1:
                        railWidth = AisinCollections.Instance.OvenState.RailWidthSP[0];
                        break;
                    case eLaneRail.Rail2:
                        railWidth = AisinCollections.Instance.OvenState.RailWidthSP[1];
                        break;
                    case eLaneRail.Rail3:
                        railWidth = AisinCollections.Instance.OvenState.RailWidthSP[2];
                        break;
                    case eLaneRail.Rail4:
                        railWidth = AisinCollections.Instance.OvenState.RailWidthSP[3];
                        break;
                    case eLaneRail.Unused:
                        // No need to write value to plc.
                        return;
                    default:
                        break;
                }

                AisinCollections.Instance.DownstreamPlcData.Lane1Width = railWidth;
                data = FillPLCOutPut(railWidth);

                int returnCode = AisinCollections.Instance.MxWrapperDownstreamLane1.WriteDeviceBlockint(addr, 1, ref data);
                if (returnCode != 0)
                {
                    throw new Exception("Return code is not 0, return code: " + returnCode);
                }

            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, $"DownstreamLane1 - {ex.Message}");
            }
        }

        private void DownstreamLane2()
        {
            try
            {
                float railWidth = 0;
                int data = 0;
                string addr = AisinCollections.Instance.AisinSetup.PlcSetup.MxAddrRailWidthLane2;

                switch (AisinCollections.Instance.AisinSetup.LaneRailSetup.Lane2Rail)
                {
                    case eLaneRail.Rail1:
                        railWidth = AisinCollections.Instance.OvenState.RailWidthSP[0];
                        break;
                    case eLaneRail.Rail2:
                        railWidth = AisinCollections.Instance.OvenState.RailWidthSP[1];
                        break;
                    case eLaneRail.Rail3:
                        railWidth = AisinCollections.Instance.OvenState.RailWidthSP[2];
                        break;
                    case eLaneRail.Rail4:
                        railWidth = AisinCollections.Instance.OvenState.RailWidthSP[3];
                        break;
                    case eLaneRail.Unused:
                        // No need to write value to plc.
                        return;
                    default:
                        break;
                }

                AisinCollections.Instance.DownstreamPlcData.Lane1Width = railWidth;
                data = FillPLCOutPut(railWidth);

                int returnCode = AisinCollections.Instance.MxWrapperDownstreamLane1.WriteDeviceBlockint(addr, 1, ref data);
                if (returnCode != 0)
                {
                    throw new Exception("Return code is not 0, return code: " + returnCode);
                }
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, $"DownstreamLane2 - {ex.Message}");
            }
        }

        public int FillPLCOutPut(float railWidth)
        {
            int result = 0;
            try
            {
                // compute rail width in tenths of millimeter
                ushort temp = (ushort)((railWidth + 0.05) * 10);

                // break down into BCD digits
                ushort digit1 = (ushort)(temp / 1000);
                temp %= 1000;
                ushort digit2 = (ushort)(temp / 100);
                temp %= 100;
                ushort digit3 = (ushort)(temp / 10);
                ushort digit4 = (ushort)(temp % 10);


                // store rail width in BCD
                // Little endian
                result = digit1 << 12 | digit2 << 8 | digit3 << 4 | digit4;

            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, $"FillPLCOutPut - {ex.Message}");
            }

            return result;
        }

        public bool initialization(ePlcConnectTo connectTo)
        {
            bool result = false;
            {
                try
                {
                    switch (connectTo)
                    {
                        case ePlcConnectTo.UpstreamLane1:
                            {
                                stationNumber = AisinCollections.Instance.AisinSetup.PlcSetup.Lane1UpstreamStationNumber;
                                if (AisinCollections.Instance.MxWrapperUpstreamLane1 == null)
                                    AisinCollections.Instance.MxWrapperUpstreamLane1 = new MxWrapper(stationNumber);
                                IsInitUpstreamLane1 = true;
                            }
                            break;
                        case ePlcConnectTo.UpstreamLane2:
                            {
                                stationNumber = AisinCollections.Instance.AisinSetup.PlcSetup.Lane2UpstreamStationNumber;
                                if (AisinCollections.Instance.MxWrapperUpstreamLane2 == null)
                                    AisinCollections.Instance.MxWrapperUpstreamLane2 = new MxWrapper(stationNumber);
                                IsInitUpstreamLane2 = true;
                            }
                            break;
                        case ePlcConnectTo.DownstreamLane1:
                            {
                                stationNumber = AisinCollections.Instance.AisinSetup.PlcSetup.Lane1DownstreamStationNumber;
                                if (AisinCollections.Instance.MxWrapperDownstreamLane1 == null)
                                    AisinCollections.Instance.MxWrapperDownstreamLane1 = new MxWrapper(stationNumber);
                                IsInitDownstreamLane1 = true;
                            }
                            break;
                        case ePlcConnectTo.DownstreamLane2:
                            {
                                stationNumber = AisinCollections.Instance.AisinSetup.PlcSetup.Lane2DownstreamStationNumber;
                                if (AisinCollections.Instance.MxWrapperDownstreamLane2 == null)
                                    AisinCollections.Instance.MxWrapperDownstreamLane2 = new MxWrapper(stationNumber);
                                IsInitDownstreamLane2 = true;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    HLog.log(HLog.eLog.EXCEPTION, $"initialization - {ex.Message}");
                }
            }
            return result;
        }
    }
}
