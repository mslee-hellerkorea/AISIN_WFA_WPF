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
    public class AisinManager
    {
        private static readonly Lazy<AisinManager> lazy = new Lazy<AisinManager>(() => new AisinManager());
        public static AisinManager Instance { get { return lazy.Value; } }

        private List<Thread> threads;

        public void init()
        {
            AisinCollections.Instance.SetupControl.ReadSetupFromFile();
            AisinCollections.Instance.BarcodeControl.ReadBarcodeTableFromFile();
            AisinParameters.Instance.RunFlag = true;

            threads = new List<Thread>();

            threads.Add(new Thread(new ThreadStart(new Hc2OcxThread().Run)));

            threads.Add(new Thread(new ThreadStart(new MxPlcThread().Run)));

            // start all threads
            foreach (Thread t in threads)
            {
                t.Start();
            }
        }

        public void Shutdown()
        {
            AisinParameters.Instance.RunFlag = false;

            if (threads != null)
            {
                foreach (Thread t in threads)
                {
                    t.Abort();
                    t.Join();
                }
                threads.Clear();
            }
            HLog.log("INFO", "Threads closed at shutdown...");
        }
    }
}
