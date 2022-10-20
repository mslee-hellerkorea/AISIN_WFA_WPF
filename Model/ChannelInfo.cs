using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using AISIN_WFA.Util;

namespace AISIN_WFA.Model
{
    public class ChannelInfo
    {
        public static string filePath = @"C:\Oven\System Files\OVEN.xml";
        public static Dictionary<int, bool> UsageChannelList = new Dictionary<int, bool>();
        public static Dictionary<int, string> NameChannelList = new Dictionary<int, string>();
        public static Dictionary<int, int> HeatZoneChannelList = new Dictionary<int, int>(); // 1 ~ zone*2
        public static Dictionary<int, int> TopZoneChannelList = new Dictionary<int, int>();
        public static Dictionary<int, int> BottomZoneChannelList = new Dictionary<int, int>();
        public static Dictionary<int, int> CoolZoneChannelList = new Dictionary<int, int>();
        public static Dictionary<int, int> CoolBlowerChannelList = new Dictionary<int, int>();
        public static Dictionary<int, int> BeltSpeedChannelList = new Dictionary<int, int>();
        public static Dictionary<int, int> RailWidthChannelList = new Dictionary<int, int>();
        public static Dictionary<int, int> LaneChannelList = new Dictionary<int, int>();
        public static Dictionary<int, int> TopBlowerChannelList = new Dictionary<int, int>();
        public static Dictionary<int, int> BottomBlowerChannelList = new Dictionary<int, int>();
        public static Dictionary<string, int> OtherChannelList = new Dictionary<string, int>();
        public static Dictionary<int, int> ExhaustChannelList = new Dictionary<int, int>();
        public static Dictionary<int, float> PredefinedBoardLength = new Dictionary<int, float>();
        public static Dictionary<int, float> SensorDistance = new Dictionary<int, float>();
        public static Dictionary<int, int> SingleOxygenList = new Dictionary<int, int>();   //0 for Flush
        public static float GlobalHiProcess = 0;
        public static string OvenHC2Version = "";

        public static bool IsUnitInch = false;
        public static bool IsUnitMM = false;

        public static void GetChannelInfo()
        {
            GlobalHiProcess = Convert.ToSingle(UseXMLConfig.GetAttrXPath("//table[@name='ADD_SYSTEM_PARAMS_TABLE']/record/field[@name='Global_Hi_Process']", "value", filePath));
            OvenHC2Version = UseXMLConfig.GetAttrXPath("//table[@name='ADD_SYSTEM_PARAMS_TABLE']/record/field[@name='VERSION']", "value", filePath);

            XPathDocument xPathDoc = new XPathDocument(filePath);
            XPathNavigator xPN = xPathDoc.CreateNavigator();

            XPathNodeIterator xpi = xPN.Select("//table[@name='CHANNELS_TABLE']/record");
            int iChannelCount = xpi.Count;

            for (int recordIdx = 0; recordIdx < iChannelCount; recordIdx++)
            {
                xpi.MoveNext();
                XPathNavigator tmpRecord = xpi.Current;
                bool bSuccess = false;
                bool isEnabled = tmpRecord.SelectSingleNode("./field[@name='Is Enabled']").GetAttribute("value", "").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                int channelID = -1;
                bSuccess = int.TryParse(tmpRecord.SelectSingleNode("./field[@name='CHANNEL_ID']").GetAttribute("value", ""), out channelID);
                string channelName = tmpRecord.SelectSingleNode("./field[@name='Channel Name']").GetAttribute("value", "");

                if (bSuccess)
                {
                    if (isEnabled)
                    {
                        string[] tmpSp = channelName.Split(' ');
                        if (tmpSp.Length < 2) tmpSp = channelName.Split('_');  //for LGIT
                        int numIdx = 0;
                        bool bSubSuccess = false;
                        if (tmpSp.Count() >= 2)
                        {
                            try
                            {
                                bSubSuccess = int.TryParse(tmpSp[1], out numIdx);

                                if (bSubSuccess)
                                {
                                    if (tmpSp[0].Equals("Top", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        int blowerChannelID = -1;
                                        int.TryParse(tmpRecord.SelectSingleNode("./field[@name='Blower RPM Channel Number']").GetAttribute("value", ""), out blowerChannelID);

                                        TopZoneChannelList.Add(numIdx, channelID);
                                        HeatZoneChannelList.Add(numIdx * 2 - 1, channelID);
                                        if (blowerChannelID > 0)
                                            TopBlowerChannelList.Add(numIdx, blowerChannelID);
                                    }
                                    else if (tmpSp[0].Equals("Bottom", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        int blowerChannelID = -1;
                                        int.TryParse(tmpRecord.SelectSingleNode("./field[@name='Blower RPM Channel Number']").GetAttribute("value", ""), out blowerChannelID);

                                        BottomZoneChannelList.Add(numIdx, channelID);
                                        HeatZoneChannelList.Add(numIdx * 2, channelID);
                                        if (blowerChannelID > 0)
                                            BottomBlowerChannelList.Add(numIdx, blowerChannelID);
                                    }
                                    else if (tmpSp[0].Equals("Cool", StringComparison.CurrentCultureIgnoreCase) && tmpSp[2].Equals("Flux", StringComparison.CurrentCultureIgnoreCase) && tmpSp[3].Equals("Heater", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        CoolZoneChannelList.Add(numIdx, channelID);

                                        int blowerChannelID = -1;
                                        int.TryParse(tmpRecord.SelectSingleNode("./field[@name='Blower RPM Channel Number']").GetAttribute("value", ""), out blowerChannelID);
                                        //In case of blower
                                        if (blowerChannelID > 0)
                                            CoolBlowerChannelList.Add(numIdx, blowerChannelID);
                                    }
                                    else if (tmpSp[0].Equals("Belt", StringComparison.CurrentCultureIgnoreCase) && tmpSp[2].Equals("Speed", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        BeltSpeedChannelList.Add(numIdx, channelID);
                                    }
                                    else if (tmpSp[0].Equals("Rail", StringComparison.CurrentCultureIgnoreCase) && tmpSp[2].Equals("Adjustment", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        RailWidthChannelList.Add(numIdx, channelID);
                                    }
                                    else
                                    {
                                        OtherChannelList.Add(channelName + " Other " + channelID, channelID);
                                    }
                                }
                                else
                                {
                                    if (tmpSp[1].Equals("Boards", StringComparison.CurrentCultureIgnoreCase) && tmpSp[2].Equals("on", StringComparison.CurrentCultureIgnoreCase) && tmpSp[3].Equals("Belt", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        int.TryParse(tmpSp[4], out numIdx);
                                        LaneChannelList.Add(numIdx, channelID);
                                    }
                                    else if (tmpSp[0].Equals("Exhaust") && tmpSp[1].StartsWith("PV"))
                                    {
                                        numIdx = Convert.ToInt32(Regex.Match(tmpSp[1], @"\d+").Value);
                                        ExhaustChannelList.Add(numIdx, channelID);
                                    }
                                    else if (channelID >= (int)ChannelIDEnums.OXY_FLUSH && channelID <= (int)ChannelIDEnums.OXY_SAMPLE_17)
                                    {
                                        //over 3 channel oxygen channel name.
                                        int channelOutput = -1;
                                        if (int.TryParse(tmpRecord.SelectSingleNode("./field[@name='Oxygen Output']").GetAttribute("value", ""), out channelOutput))
                                            SingleOxygenList.Add(channelID - (int)ChannelIDEnums.OXY_FLUSH, channelOutput);
                                    }
                                    else
                                        OtherChannelList.Add(channelName + " Other " + channelID, channelID);
                                }
                            }
                            catch
                            { }
                        }
                        else
                        {
                            OtherChannelList.Add(channelName + " Other " + channelID, channelID);
                        }

                        UsageChannelList.Add(channelID, true);
                    }
                    else
                    {
                        UsageChannelList.Add(channelID, false);
                    }

                    NameChannelList.Add(channelID, channelName);
                }
            }

            //Predefined board length
            for (int i = 0; i < LaneChannelList.Count; i++)
            {
                string xPath = $"//table[@name='CHANNELS_TABLE']/record[@id='{i + (int)ChannelIDEnums.LANE1_BOARD_LENGTH}']/field[@name='Lane Board Length']";
                float boardLength = Convert.ToSingle(xPN.SelectSingleNode(xPath).GetAttribute("value", ""));
                PredefinedBoardLength.Add(i + 1, boardLength);
            }

            //Sensor Distance
            for (int i = 0; i < LaneChannelList.Count; i++)
            {
                string xPath = $"//table[@name='SYSTEM_PARAMS_TABLE']/record/field[@name='Sensor Distance {i + 1}']";
                float sensorDist = Convert.ToSingle(xPN.SelectSingleNode(xPath).GetAttribute("value", ""));
                SensorDistance.Add(i + 1, sensorDist);
            }

            //Length Unit
            {
                string xPath = $"//table[@name='SYSTEM_PARAMS_TABLE']/record/field[@name='LENGTHUNITS_ENUM']";
                int unitEnum = Convert.ToInt32(xPN.SelectSingleNode(xPath).GetAttribute("value", ""));
                if (unitEnum == 0) IsUnitInch = true;
                else if (unitEnum == 2) IsUnitMM = true;
            }
        }

        //public static void GetCodeList()
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        HLog.log(HLog.eLog.EXCEPTION, $"GetCodeList - {ex.Message}");
        //    }
        //}
    }

    public enum ChannelIDEnums : short
    {
        //Channel Name Based 1
        //Channel Value Based 0
        //HEAT_CHAN_1 = 0,
        //HEAT_CHAN_2 = 1,
        //HEAT_CHAN_3 = 2,
        //HEAT_CHAN_4 = 3,
        //HEAT_CHAN_5 = 4,
        //HEAT_CHAN_6 = 5,
        //HEAT_CHAN_7 = 6,
        //HEAT_CHAN_8 = 7,
        //HEAT_CHAN_9 = 8,
        //HEAT_CHAN_10 = 9,
        //HEAT_CHAN_11 = 10,
        //HEAT_CHAN_12 = 11,
        //EXHAUST_FLUX_HEATER_CHAN = 12,
        //BELT_CHAN_1 = 13,
        //HEAT_CHAN_13 = 14,
        //HEAT_CHAN_14 = 15,
        //HEAT_CHAN_15 = 16,
        //HEAT_CHAN_16 = 17,
        //HEAT_CHAN_17 = 18,
        //HEAT_CHAN_18 = 19,
        //HEAT_CHAN_19 = 20,
        //HEAT_CHAN_20 = 21,
        //HEAT_CHAN_21 = 22,
        //HEAT_CHAN_22 = 23,
        //HEAT_CHAN_23 = 24,
        //HEAT_CHAN_24 = 25,
        PROFILE_OUTPUT_1 = 26,
        PROFILE_OUTPUT_2 = 27,
        PROFILE_OUTPUT_3 = 28,
        PROFILE_OUTPUT_4 = 29,
        PROFILE_OUTPUT_5 = 30,
        //BELT_CHAN_2 = 31,
        FREE_CHAN_1 = 32,
        DANSENSOR_CHAN = 33,
        GLOBAL_BLOWER_CONTROL_CHAN = 34,
        ANALOG_FAN_CHAN = 35,
        NITROGEN_CHAN = 36,
        CBS_BELT_CHAN_1 = 37,
        LIGHT_TOWER_CHAN = 38,
        HEAT_ZONE_BLOWER_1 = 39,
        HEAT_ZONE_BLOWER_2 = 40,
        HEAT_ZONE_BLOWER_3 = 41,
        CLOSED_LOOP_RAIL_CHAN_1 = 42,
        CLOSED_LOOP_RAIL_CHAN_2 = 43,
        CLOSED_LOOP_RAIL_CHAN_3 = 44,
        CLOSED_LOOP_RAIL_CHAN_4 = 45,
        BOARDS_IN_OVEN_BELT_CHAN_1 = 46,
        BOARDS_IN_OVEN_BELT_CHAN_2 = 47,
        BOARDS_IN_OVEN_BELT_CHAN_3 = 48,
        BOARDS_PROCESSED_BELT_CHAN_1 = 49,
        BOARDS_PROCESSED_BELT_CHAN_2 = 50,
        BOARDS_PROCESSED_BELT_CHAN_3 = 51,
        CBS_BELT_CHAN_2 = 52,
        BOARDS_IN_OVEN_BELT_CHAN_4 = 53,
        BOARDS_PROCESSED_BELT_CHAN_4 = 54,
        EXHAUST_FLUX_HEATER_CHAN_1 = 55,
        EXHAUST_FLUX_HEATER_CHAN_2 = 56,
        DANSENSOR_PPM = 57,
        EXTERNAL_BUTTON = 58,
        FLUX_CONDENS = 59,
        BARCODE_BUTTON = 60,
        FANA_0100 = 61,
        FANB_0100 = 62,
        FANC_0100 = 63,
        FAND_LMH = 64,
        FANE_LMH = 65,
        HEAT_CHAN_25 = 66,
        HEAT_CHAN_26 = 67,
        HEAT_CHAN_27 = 68,
        HEAT_CHAN_28 = 69,
        EXHAUST_FLUX_HEATER_CHAN_0_1913 = 70,
        EXHAUST_FLUX_HEATER_CHAN_1_1913 = 71,
        LOT_ID = 72,
        DIGITAL_SWITCH = 73,
        LOT_BUTTON = 74,
        OXYMASTER_CHAN = 75,
        CAPACITY_BOARDS = 76,
        //HEAT_CHAN_29 = 77,
        //HEAT_CHAN_30 = 78,
        //HEAT_CHAN_31 = 79,
        //HEAT_CHAN_32 = 80,
        //HEAT_CHAN_33 = 81,
        //HEAT_CHAN_34 = 82,
        //HEAT_CHAN_35 = 83,
        //HEAT_CHAN_36 = 84,
        //HEAT_CHAN_37 = 85,
        //HEAT_CHAN_38 = 86,
        //HEAT_CHAN_39 = 87,
        //HEAT_CHAN_40 = 88,
        //HEAT_CHAN_41 = 89,
        //HEAT_CHAN_42 = 90,
        //HEAT_CHAN_43 = 91,
        //HEAT_CHAN_44 = 92,
        //HEAT_CHAN_45 = 93,
        //HEAT_CHAN_46 = 94,
        //HEAT_CHAN_47 = 95,
        //HEAT_CHAN_48 = 96,
        //HEAT_CHAN_49 = 97,
        //HEAT_CHAN_50 = 98,
        //HEAT_CHAN_51 = 99,
        //HEAT_CHAN_52 = 100,
        //HEAT_CHAN_53 = 101,
        //HEAT_CHAN_54 = 102,
        //HEAT_CHAN_55 = 103,
        //HEAT_CHAN_56 = 104,
        //HEAT_CHAN_57 = 105,
        //HEAT_CHAN_58 = 106,
        //HEAT_CHAN_59 = 107,
        //HEAT_CHAN_60 = 108,
        //HEAT_CHAN_61 = 109,
        //HEAT_CHAN_62 = 110,
        EXHAUST_FLUX_NONHEATER_CHAN_01 = 111,
        EXHAUST_FLUX_NONHEATER_CHAN_02 = 112,
        EXHAUST_FLUX_NONHEATER_CHAN_03 = 113,
        EXHAUST_FLUX_NONHEATER_CHAN_04 = 114,
        EXHAUST_FLUX_NONHEATER_CHAN_05 = 115,
        EXHAUST_FLUX_NONHEATER_CHAN_06 = 116,
        EXHAUST_FLUX_NONHEATER_CHAN_07 = 117,
        EXHAUST_FLUX_NONHEATER_CHAN_08 = 118,
        EXHAUST_FLUX_NONHEATER_CHAN_09 = 119,
        EXHAUST_FLUX_NONHEATER_CHAN_10 = 120,
        LIGHT_TOWER2_CHAN = 121,
        DUALBARCODE_CHAN = 122,
        BARCODE_SCANNER = 123,
        MASS_FLOW1 = 124,
        MASS_FLOW2 = 125,
        MASS_FLOW3 = 126,
        MASS_FLOW4 = 127,
        MASS_FLOW5 = 128,
        MASS_FLOW1_UNIT = 129,
        MASS_FLOW2_UNIT = 130,
        MASS_FLOW3_UNIT = 131,
        MASS_FLOW4_UNIT = 132,
        MASS_FLOW5_UNIT = 133,

        OXY_FLUSH = 143,
        OXY_SAMPLE_1 = 144,
        OXY_SAMPLE_2 = 145,
        OXY_SAMPLE_3 = 146,
        OXY_SAMPLE_4 = 147,
        OXY_SAMPLE_5 = 148,
        OXY_SAMPLE_6 = 149,
        OXY_SAMPLE_7 = 150,
        OXY_SAMPLE_8 = 151,
        OXY_SAMPLE_9 = 152,
        OXY_SAMPLE_10 = 153,
        OXY_SAMPLE_11 = 154,
        OXY_SAMPLE_12 = 155,
        OXY_SAMPLE_13 = 156,
        OXY_SAMPLE_14 = 157,
        OXY_SAMPLE_15 = 158,
        OXY_SAMPLE_16 = 159,
        OXY_SAMPLE_17 = 160,

        LANE1_BOARD_LENGTH = 165,
        LANE2_BOARD_LENGTH = 166,
        LANE3_BOARD_LENGTH = 167,
        LANE4_BOARD_LENGTH = 168,

    }
}
