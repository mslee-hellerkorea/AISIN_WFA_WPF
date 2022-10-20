using AISIN_WFA.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public class OcxWrapper
    {
        #region [Member]

        private AxHELLERCOMMLib.AxHellerComm ocx = null;
        private ReaderWriterLockSlim lockOcx = new ReaderWriterLockSlim();
        internal bool IsTakenControl { get; set; } = false;
        private bool IsIntialSPUpdateFinished = false;
        private object variant = new object();
        public string Lane1Barcode { get; set; } = string.Empty;
        public string Lane2Barcode { get; set; } = string.Empty;
        #endregion

        #region [Enum]
        private enum eOCXEventID : int
        {
            ALARM_ID = 20,
            ALARM_MESSAGE = 21,
            ALARM_ACK = 22,
            LIGHT_TOWER_CHANGE = 30,
            JOB_CHANGE = 40,
            HEAT_SP_CHANGE = 51,
            BOARD_ENTERED = 60,
            BOARD_EXITED = 61,
            LANE1_SMEMA_ENTRY_BA = 71,  //
            LANE2_SMEMA_ENTRY_BA = 72,  //
            LANE3_SMEMA_ENTRY_BA = 73,  //
            LANE4_SMEMA_ENTRY_BA = 74,  //
            BELT0_OPENLOOP = 81,        //belt speed setpoint changed
            BELT0_CLOSEDLOOP = 82,
            BELT1_OPENLOOP = 83,
            BELT1_CLOSEDLOOP = 84,
            LANE1_BOARD_ENTRY = 100,
            LANE2_BOARD_ENTRY = 101,
            LANE3_BOARD_ENTRY = 102,
            LANE4_BOARD_ENTRY = 103,
            LANE1_BOARD_EXIT = 104,
            LANE2_BOARD_EXIT = 105,
            LANE3_BOARD_EXIT = 106,
            LANE4_BOARD_EXIT = 107,
        }

        private enum eLightTowerState
        {
            LIGHT_TOWER_OFF = 0,
            LIGHT_TOWER_RED = 1,
            LIGHT_TOWER_AMBER = 2,
            LIGHT_TOWER_GREEN = 4
        }
        #endregion

        #region [Connection]
        public bool InitWrapper()
        {
            if (!CheckOvenAlive())
            {
                HLog.log(HLog.eLog.ERROR, "Go To Shutdown. Cannot find HC2 Oven Operating Program!");
                return false;
            }

            //Initialize OCX.
            bool isSuccess = true;

            //bool bTakeLock = false;
            try
            {
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

                ocx = new AxHELLERCOMMLib.AxHellerComm();
                ocx.CreateControl();
                isSuccess = ocx.StartCommunicating(1);
                if (isSuccess) isSuccess = ocx.StartOven();
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("InitWrapper -- Message: {0}", ex.Message));
            }

            if (!isSuccess) return false;

            ocx.NotificationEvent += OCXWrapper_NotificationEvent;
            ocx.UpdateChannelParam += OCXWrapper_UpdateChannelParam;

            return true;
        }

        public bool CheckOvenAlive()
        {
            int handle = HandleUtil.FindWindow("CHellerMainFrame", null);
            return handle != 0;
        }
        #endregion

        #region [Get Set Function]
        public void TakeControl(bool bTake)
        {
            lockOcx.EnterWriteLock();
            try
            {
                if (bTake) ocx.TakeControl(2);
                else ocx.ReleaseControl(2);

                IsTakenControl = bTake;
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("TakeControl -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public int LoadRecipe(string fullRecipeName)
        {
            //this will not check the existance of board.
            int result = -1;
            lockOcx.EnterWriteLock();
            try
            {
                if (fullRecipeName.ToUpper().Contains("COOLDOWN"))
                    ocx.LoadCooldown();
                else
                    result = ocx.LoadRecipe(ref fullRecipeName);
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }

            return result;
        }

        public string GetRecipePath()
        {
            string recipe = string.Empty;
            lockOcx.EnterWriteLock();
            try
            {
                int result = ocx.GetRecipePath(ref recipe);
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("GetRecipeName -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
            return recipe;
        }

        public void SetRailWidth(int railIdx, float width)
        {
            lockOcx.EnterWriteLock();
            try
            {
                ocx.TakeControl(2);
                ocx.SetRailWidth((short)railIdx, width);
                ocx.ReleaseControl(2);
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("SetRailWidth -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public float GetRailWidth(int railIdx)
        {
            float RailWidth = 0;
            lockOcx.EnterWriteLock();
            try
            {
                variant = ocx.GetRailWidth((short)railIdx);
                if (variant == null)
                    RailWidth = -1;
                else
                    RailWidth = (float)variant;
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("GetRailWidth -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }

            return RailWidth;
        }

        public void SetBeltSpeed(int beltIdx, float speed)
        {
            lockOcx.EnterWriteLock();
            try
            {
                ocx.TakeControl(2);
                ocx.SetFurnaceBeltSpeed(speed, (short)beltIdx);
                ocx.ReleaseControl(2);
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("SetBeltSpeed -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public float GetBeltSpeed(int channel, bool isSetPoint)
        {
            float speed = 0;
            lockOcx.EnterWriteLock();
            try
            {
                if (isSetPoint)
                    speed = ocx.GetFurnaceBeltSetPoint((short)channel) * 0.1F; // TODO : 확인 필요
                else
                    speed = Convert.ToSingle(ocx.GetFurnaceBeltSpeed((short)channel)) / 100F;
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("SetBeltSpeed -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
            return speed;
        }

        public int GetFurnaceBeltSetPoint(int channel)
        {
            int speed = 0;
            lockOcx.EnterWriteLock();
            try
            {
                speed = ocx.GetFurnaceBeltSetPoint((short)channel);
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("GetFurnaceBeltSpeed -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
            return speed;
        }

        public object GetFurnaceBeltSpeed(int channel)
        {
            object speed = 0;
            lockOcx.EnterWriteLock();
            try
            {
                speed = Convert.ToSingle(ocx.GetFurnaceBeltSpeed((short)channel)) / 100F; //TODO : Need to check the Acutal value
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("GetFurnaceBeltSpeed -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
            return speed;
        }

        public int GetFurnaceOxygenPPM()
        {
            int ppm = 0;
            lockOcx.EnterWriteLock();
            try
            {
                ppm = ocx.getFurnaceOxygenPPM();
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("getFurnaceOxygenPPM -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
            return ppm;
        }

        public void SetSmema(int laneID, bool bOn)
        {
            lockOcx.EnterWriteLock();
            try
            {
                //Hold 1 is off, 0 is on                
                if (ocx != null && !ocx.IsDisposed)
                    ocx.SMEMA_SetLaneHold(Convert.ToUInt32(laneID - 1), bOn ? 0 : 1);
                else
                    HLog.log(HLog.eLog.ERROR, "Oven is not connected!");
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("SetBeSetSmemaltSpeed -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public void SetSmema(int laneID, int bhold)
        {
            lockOcx.EnterWriteLock();
            try
            {
                //Hold 1 is off, 0 is on                
                if (ocx != null && !ocx.IsDisposed)
                    ocx.SMEMA_SetLaneHold((uint)laneID, bhold);
                else
                    HLog.log(HLog.eLog.ERROR, "Oven is not connected!");
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("SetBeSetSmemaltSpeed -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public int GetBoardInCount(int laneNo)
        {
            lockOcx.EnterWriteLock();
            try
            {
                if (laneNo == 0)
                {
                    int ret = 0;
                    for (short i = 0; i < ChannelInfo.LaneChannelList.Count; i++)
                    {
                        ret += ocx.GetBoardsInOvenCount(i);
                    }

                    return ret;
                }
                else
                    return ocx.GetBoardsInOvenCount((short)(laneNo - 1));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public void SetTemperatureSP(bool isCoolZone, bool isTop, int zoneID, float temperature)
        {
            lockOcx.EnterWriteLock();
            try
            {
                ocx.TakeControl(2);
                if (isCoolZone)
                {
                    //channelparam is used to set the SP according the recipe information's sequence.
                    //the column #2 is setpoint.
                    ocx.SetChannelParam((short)ChannelInfo.CoolZoneChannelList[zoneID], 2, temperature, 0);
                }
                else
                {
                    int heaterIndex = 1;
                    if (isTop) heaterIndex = zoneID * 2 - 1;
                    else heaterIndex = zoneID * 2;
                    ocx.SetFurnaceSetpoints_Float(temperature, (short)heaterIndex);
                }
                ocx.ReleaseControl(2);
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", String.Format("SetTemperatureSP -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public float GetFurnaceSetpointsLong(int channel, bool isSetPoint)
        {
            float temp = 0;
            lockOcx.EnterWriteLock();
            try
            {
                if (isSetPoint)
                    temp = ocx.GetFurnaceSetpointsLong((short)channel) / 10F; // TODO : 확인 필요
                else
                    temp = Convert.ToSingle(ocx.GetFurnaceTCValueLong((short)channel)) / 10F;
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("SetBeltSpeed -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
            return temp;
        }

        public void SetDigitalOutput(short numDO, bool bOn)
        {
            if (numDO < 8) numDO += 48;
            else if (numDO < 16) numDO += 0;
            else if (numDO < 24) numDO += 8;
            else if (numDO < 32) numDO += 16;

            lockOcx.EnterWriteLock();
            try
            {
                if (ocx != null && !ocx.IsDisposed)
                    ocx.SetDigitalOutput(numDO, bOn ? 1 : 0);
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, String.Format("SetDigitalOutput -- Message: {0}", ex.Message));
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public bool GetDigitalOutput(short numDO)
        {
            if (numDO < 8) numDO += 48;
            else if (numDO < 16) numDO += 0;
            else if (numDO < 24) numDO += 8;
            else if (numDO < 32) numDO += 16;

            lockOcx.EnterWriteLock();
            try
            {
                return ocx.GetDigitalInput(numDO) != 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                lockOcx.ExitWriteLock();
            }
        }

        public string GetCurrentLightTowerColor()
        {
            string state = string.Empty;
            try
            {
                lockOcx.EnterWriteLock();
                short lightTowerColor = ocx.GetCurrentLightTowerColor();
                lockOcx.ExitWriteLock();
                switch ((eLightTowerState)lightTowerColor)
                {
                    case eLightTowerState.LIGHT_TOWER_OFF:
                        state = "Off";
                        break;
                    case eLightTowerState.LIGHT_TOWER_RED:
                        state = "Red";
                        break;
                    case eLightTowerState.LIGHT_TOWER_AMBER:
                        state = "Amber";
                        break;
                    case eLightTowerState.LIGHT_TOWER_GREEN:
                        state = "Green";
                        break;
                    default:
                        state = lightTowerColor.ToString("0");
                        break;
                }
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, $"GetCurrentLightTowerColor - {ex.Message}");
            }

            return state;
        }

        public float GetChannel(int channel)
        {
            float value = 0;
            try
            {
                lockOcx.EnterWriteLock();
                value = ocx.GetChannel((short)channel);
                lockOcx.ExitWriteLock();
            }
            catch (Exception ex)
            {
                HLog.log(HLog.eLog.EXCEPTION, $"GetChannel - {ex.Message}");
            }

            return value;
        }
        #endregion

        #region [Local Function]
        private string[] ConvertToTraceData()
        {
            string[] valuse = new string[20];
            try
            {
                // Recipe Name
                valuse[0] = AisinCollections.Instance.OvenState.RecipeName;

                // Top Zone SP
                StringBuilder setTopZones = new StringBuilder();
                foreach (KeyValuePair<int,float> zone in AisinCollections.Instance.OvenState.TopZoneTemperatureSP)
                {
                    if (zone.Value != 0)
                    {
                        setTopZones.Append(zone.ToString());
                        setTopZones.Append(",");
                    }
                }
                setTopZones.Remove(setTopZones.Length - 1, 1);
                valuse[1] = setTopZones.ToString();

                // Bottom Zone SP
                StringBuilder setBottomZones = new StringBuilder();
                foreach (KeyValuePair<int, float> zone in AisinCollections.Instance.OvenState.BottomZoneTemperatureSP)
                {
                    if (zone.Value != 0)
                    {
                        setBottomZones.Append(zone.ToString());
                        setBottomZones.Append(",");
                    }
                }
                setBottomZones.Remove(setBottomZones.Length - 1, 1);
                valuse[2] = setBottomZones.ToString();

                // Conveyor1 SP
                valuse[3] = AisinCollections.Instance.OvenState.BeltSpeedSP[0].ToString();

                // Top Zone PV
                StringBuilder processTopZones = new StringBuilder();
                foreach (KeyValuePair<int, float> zone in AisinCollections.Instance.OvenState.TopZoneTemperaturePV)
                {
                    if (zone.Value != 0)
                    {
                        processTopZones.Append(zone.ToString());
                        processTopZones.Append(",");
                    }
                }
                processTopZones.Remove(processTopZones.Length - 1, 1);
                valuse[4] = processTopZones.ToString();

                // Bottom Zone PV
                StringBuilder processBottomZones = new StringBuilder();
                foreach (KeyValuePair<int, float> zone in AisinCollections.Instance.OvenState.BottomZoneTemperaturePV)
                {
                    if (zone.Value != 0)
                    {
                        processBottomZones.Append(zone.ToString());
                        processBottomZones.Append(",");
                    }
                }
                processBottomZones.Remove(processBottomZones.Length - 1, 1);
                valuse[5] = processBottomZones.ToString();

                // Conveyor1 PV
                valuse[6] = AisinCollections.Instance.OvenState.BeltSpeedPV[0].ToString();

                // O2 PPM
                valuse[7] = AisinCollections.Instance.OvenState.OxgenPPM[0].ToString();

            }
            catch (Exception ex)
            {
                HLog.log("ERROR", String.Format("ConvertToTraceData -- Message: {0}", ex.Message));
            }
            return valuse;
        }
        #endregion

        #region [Event]
        private void OCXWrapper_NotificationEvent(object sender, AxHELLERCOMMLib._DHellerCommEvents_NotificationEventEvent e)
        {
            switch ((eOCXEventID)e.lEventID)
            {
                case eOCXEventID.ALARM_ID:
                    break;
                case eOCXEventID.ALARM_MESSAGE:
                    break;
                case eOCXEventID.ALARM_ACK:
                    break;
                case eOCXEventID.LIGHT_TOWER_CHANGE:
                    break;
                case eOCXEventID.JOB_CHANGE:
                    break;
                case eOCXEventID.HEAT_SP_CHANGE:
                    if (e.lEventData == 64)
                    {
                        IsIntialSPUpdateFinished = true;
                        // Ignore..
                        // OnInitialized?.BeginInvoke(ChannelInfo.LaneChannelList.Count, null, null);
                    }
                    break;
            }

            if (IsIntialSPUpdateFinished)
            {
                switch ((eOCXEventID)e.lEventID)
                {
                    case eOCXEventID.BOARD_ENTERED:
                    case eOCXEventID.BOARD_EXITED:
                        //not use
                        break;
                    case eOCXEventID.LANE1_SMEMA_ENTRY_BA:
                        HLog.log(HLog.eLog.EVENT, $"SMEMA1 {e.lEventData}");
                        //not use
                        break;
                    case eOCXEventID.LANE2_SMEMA_ENTRY_BA:
                        HLog.log(HLog.eLog.EVENT, $"SMEMA2 {e.lEventData}");
                        //not use
                        break;
                    case eOCXEventID.LANE3_SMEMA_ENTRY_BA:
                        HLog.log(HLog.eLog.EVENT, $"SMEMA3 {e.lEventData}");
                        //not use
                        break;
                    case eOCXEventID.LANE4_SMEMA_ENTRY_BA:
                        HLog.log(HLog.eLog.EVENT, $"SMEMA4 {e.lEventData}");
                        //not use
                        break;
                    case eOCXEventID.LANE1_BOARD_ENTRY:
                        SetSmema(0, 1);
                        HLog.logTrace(Lane1Barcode, ConvertToTraceData());
                        //UseLog.Log(UseLog.LogCategory.Error, $"TEST Board{e.lEventData} Enter");
                        BoardEntered(1, e.lEventData);  //lEventData is serial number of baord 
                        break;
                    case eOCXEventID.LANE1_BOARD_EXIT:  //include board drop
                        //UseLog.Log(UseLog.LogCategory.Error, $"TEST Board{e.lEventData} Exit");
                        BoardExited(1, e.lEventData);
                        break;
                    case eOCXEventID.LANE2_BOARD_ENTRY:
                        SetSmema(1, 1);
                        HLog.logTrace(Lane2Barcode, ConvertToTraceData());
                        BoardEntered(2, e.lEventData);
                        break;
                    case eOCXEventID.LANE2_BOARD_EXIT:
                        BoardExited(2, e.lEventData);
                        break;
                    case eOCXEventID.LANE3_BOARD_ENTRY:
                        BoardEntered(3, e.lEventData);
                        break;
                    case eOCXEventID.LANE3_BOARD_EXIT:
                        BoardExited(3, e.lEventData);
                        break;
                    case eOCXEventID.LANE4_BOARD_ENTRY:
                        BoardEntered(4, e.lEventData);
                        break;
                    case eOCXEventID.LANE4_BOARD_EXIT:
                        BoardExited(4, e.lEventData);
                        break;
                    default:
                        break;
                }
            }

        }

        private void BoardEntered(int laneID, int serialID)
        {
            // TODO
        }

        private void BoardExited(int laneID, int serialID)
        {
            // TODO
        }

        private void OCXWrapper_UpdateChannelParam(object sender, AxHELLERCOMMLib._DHellerCommEvents_UpdateChannelParamEvent e)
        {
            HLog.log(HLog.eLog.EXCEPTION, $"EnumChannel : {e.enumChannel}, ChannelParam : {e.enumChannellParam}, Value : {e.fValue}");
            //throw new NotImplementedException();
        }
        #endregion
    }
}
