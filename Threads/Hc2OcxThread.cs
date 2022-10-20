using AISIN_WFA.Model;
using AISIN_WFA.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AISIN_WFA.Threads
{
    public class Hc2OcxThread
    {
        private bool isInit = false;

        public void Run()
        {
            while (AisinParameters.Instance.RunFlag)
            {
                try
                {
                    if (!isInit)
                    {
                        initialization();
                    }
                    else if (isInit && AisinCollections.Instance.OcxWrapper.CheckOvenAlive())
                    {
                        // Get Light Tower State
                        AisinCollections.Instance.OvenState.LighTower = AisinCollections.Instance.OcxWrapper.GetCurrentLightTowerColor();

                        // Get Recipe Name
                        AisinCollections.Instance.OvenState.RecipeName = AisinCollections.Instance.OcxWrapper.GetRecipePath();

                        // Get Rail Width PV
                        for (int i = 1; i < 5; i++)
                        {
                            AisinCollections.Instance.OvenState.RailWidthPV[i - 1] = AisinCollections.Instance.OcxWrapper.GetRailWidth(i);
                        }

                        // Get Rail Width SP
                        for (int i = 101; i < 105; i++)
                        {
                            AisinCollections.Instance.OvenState.RailWidthSP[i - 101] = AisinCollections.Instance.OcxWrapper.GetRailWidth(i);
                        }

                        // Processed Count
                        for (int i = 49; i < 52; i++)
                        {
                            AisinCollections.Instance.OvenState.ProcessedCount[i - 49] = AisinCollections.Instance.OcxWrapper.GetChannel(i);
                        }
                        AisinCollections.Instance.OvenState.ProcessedCount[3] = AisinCollections.Instance.OcxWrapper.GetChannel(53);

                        // InOven Count
                        for (int i = 46; i < 49; i++)
                        {
                            AisinCollections.Instance.OvenState.InOvenCount[i - 46] = AisinCollections.Instance.OcxWrapper.GetChannel(i);
                        }
                        AisinCollections.Instance.OvenState.InOvenCount[3] = AisinCollections.Instance.OcxWrapper.GetChannel(54);

                        // Top zone SP
                        foreach (var TopZone in ChannelInfo.TopZoneChannelList)
                        {
                            int channel = 0;
                            if (TopZone.Value >= 13)
                                channel = TopZone.Value - 2;
                            else
                                channel = TopZone.Value;

                            channel += 1;

                            AisinCollections.Instance.OvenState.TopZoneTemperatureSP[TopZone.Key] = AisinCollections.Instance.OcxWrapper.GetFurnaceSetpointsLong(channel, true);
                        }

                        // Bottom zone SP
                        foreach (var BottomZone in ChannelInfo.BottomZoneChannelList)
                        {
                            int channel = 0;
                            if (BottomZone.Value >= 13)
                                channel = BottomZone.Value - 2;
                            else
                                channel = BottomZone.Value;

                            channel += 1;

                            AisinCollections.Instance.OvenState.BottomZoneTemperatureSP[BottomZone.Key] = AisinCollections.Instance.OcxWrapper.GetFurnaceSetpointsLong(channel, true);
                        }

                        // Top zone PV
                        foreach (var TopZone in ChannelInfo.TopZoneChannelList)
                        {
                            int channel = 0;
                            if (TopZone.Value >= 13)
                                channel = TopZone.Value - 2;
                            else
                                channel = TopZone.Value;

                            channel += 1;

                            AisinCollections.Instance.OvenState.TopZoneTemperaturePV[TopZone.Key] = AisinCollections.Instance.OcxWrapper.GetChannel(channel);
                        }

                        // Bottom zone PV
                        foreach (var BottomZone in ChannelInfo.BottomZoneChannelList)
                        {
                            int channel = 0;
                            if (BottomZone.Value >= 13)
                                channel = BottomZone.Value - 2;
                            else
                                channel = BottomZone.Value;

                            channel += 1;

                            AisinCollections.Instance.OvenState.BottomZoneTemperaturePV[BottomZone.Key] = AisinCollections.Instance.OcxWrapper.GetChannel(channel);
                        }

                        // Bottom zone PV
                        foreach (var belt in ChannelInfo.BeltSpeedChannelList)

                            // Belt Speed SP
                            for (int i = 1; i < ChannelInfo.BeltSpeedChannelList.Count + 1; i++)
                            {
                                AisinCollections.Instance.OvenState.BeltSpeedSP[i - 1] = AisinCollections.Instance.OcxWrapper.GetFurnaceBeltSetPoint(i) * 0.1F;
                            }

                        // Belt Speed PV
                        for (int i = 1; i < ChannelInfo.BeltSpeedChannelList.Count + 1; i++)
                        {
                            AisinCollections.Instance.OvenState.BeltSpeedPV[i - 1] = (float)AisinCollections.Instance.OcxWrapper.GetFurnaceBeltSpeed(i);
                        }

                        // Oxygen
                        AisinCollections.Instance.OvenState.OxgenPPM[0] = AisinCollections.Instance.OcxWrapper.GetFurnaceOxygenPPM();
                    }

                    if (isInit && AisinCollections.Instance.OcxWrapper.CheckOvenAlive())
                        AisinCollections.Instance.OvenState.IsHc2Alive = true;
                    else
                        AisinCollections.Instance.OvenState.IsHc2Alive = false;


                }
                catch (Exception ex)
                {
                    HLog.log(HLog.eLog.EXCEPTION, $"Run - {ex.Message}");
                }

                Thread.Sleep(AisinCollections.Instance.AisinSetup.GeneralSetup.OcxUpdatePeriod);
            }
        }

        public bool initialization()
        {
            bool result = false;
            {
                App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    try
                    {
                        if (AisinCollections.Instance.OcxWrapper == null)
                        {
                            AisinCollections.Instance.OcxWrapper = new OcxWrapper();
                            result = AisinCollections.Instance.OcxWrapper.InitWrapper();

                            if (result)
                                isInit = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        HLog.log(HLog.eLog.EXCEPTION, $"initialization - {ex.Message}");
                    }

                });
            }
            return result;
        }
    }
}
