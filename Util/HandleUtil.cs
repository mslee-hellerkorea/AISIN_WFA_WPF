using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AISIN_WFA.Util
{
    public class HandleUtil
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string lp1, string lp2);
    }
}
