using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Util
{
    public static class Constant
    {
        public static readonly string SOFTWARE_NAME = "AISIN line communication";
        public static readonly string REVISION = "v1.0.0_Beta1";
        public static readonly int BARCODE_MAX = 24;
    }
    public class AisinEnums
    {
        public enum ePlcType
        {
            OMRON,
            MITSUBISHI
        }

        public enum ePlcConnectTo
        {
            UpstreamLane1,
            UpstreamLane2,
            DownstreamLane1,
            DownstreamLane2
        }

        public enum eLaneRail
        {
            Rail1,
            Rail2,
            Rail3,
            Rail4,
            Unused
        }
    }
}
