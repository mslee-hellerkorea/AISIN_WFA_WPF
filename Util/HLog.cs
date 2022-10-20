using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AISIN_WFA.Util
{
    public static class HLog
    {
        #region [Mutex]
        private static Mutex logMutex = new Mutex();
        private static Mutex logTraceMutex = new Mutex();
        #endregion

        #region [PREFIX/SUFIX]
        public static string PREFIX = "Log.";
        public static string SUFFIX = ".txt";
        public static string PREFIX_TRACE = "";
        public static string SUFFIX_TRACE = ".log";
        #endregion

        #region [FileStream / StreamWriter]
        private static FileStream logFileStream = null;
        private static StreamWriter logStreamWrite = null;
        private static FileStream logTraceFileStream = null;
        private static StreamWriter logTraceStreamWrite = null;
        #endregion

        public static string StartupPath = @"C:\TEST\";
        public static string LogTracePath = @"C:\TRACE\Reflow\";
        private const int mutexWait = 100; // milliseconds to wait for access to file

        public enum eLog
        {
            EVENT,
            ERROR,
            DEBUG,
            EXCEPTION
        }

        public static void log(
            string msgType,
            string logMsg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (logMutex.WaitOne(mutexWait))
            {
                try
                {
                    StringBuilder sb = new StringBuilder(DateTime.Now.ToString("hh:mm:ss:fff"));
                    sb.Append("    " + msgType.ToString() + ":   ");
                    sb.Append(logMsg);
                    sb.Append(" (" + Path.GetFileName(sourceFilePath) + ": ").Append(sourceLineNumber + ")");
                    if (!Directory.Exists(StartupPath))
                    {
                        Directory.CreateDirectory(StartupPath);
                    }

                    string path = StartupPath + PREFIX + DateTime.Now.ToString("yyyyMMdd") + SUFFIX;
                    bool exists = File.Exists(path);
                    if (logFileStream == null)
                    {
                        logFileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                    }
                    else
                    {
                        if (logFileStream.Name != path)
                        {
                            if (logStreamWrite != null)
                            {
                                logStreamWrite.Close();
                                logStreamWrite = null;
                            }
                            logFileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                        }
                    }
                    if (logStreamWrite == null)
                    {
                        logStreamWrite = new StreamWriter(logFileStream);
                        if (!exists)
                        {
                            logStreamWrite.WriteLine("Start new log file");
                        }
                    }
                    logStreamWrite.WriteLine(sb.ToString());
                    logStreamWrite.Flush();
                    logFileStream.Flush();
                }
                catch (Exception ex)
                {
                    WriteApplicationEventLog(string.Format("HLog.log exception={0} msg={1}", ex.Message, string.Format("{0} {1} {2}", logMsg, Path.GetFileName(sourceFilePath), sourceLineNumber)));
                }
                logMutex.ReleaseMutex();
            }
            else
            {
                WriteApplicationEventLog(string.Format("HLog.log mutex timeout msg={0}", string.Format("{0} {1} {2}", logMsg, Path.GetFileName(sourceFilePath), sourceLineNumber)));
            }
        }

        public static void log(
            eLog msgType, 
            string logMsg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (logMutex.WaitOne(mutexWait))
            {
                try
                {
                    StringBuilder sb = new StringBuilder(DateTime.Now.ToString("hh:mm:ss:fff"));
                    sb.Append("    " + msgType.ToString() + ":   ");
                    sb.Append(logMsg);
                    sb.Append(" (" + Path.GetFileName(sourceFilePath) + ": ").Append(sourceLineNumber + ")");
                    if (!Directory.Exists(StartupPath))
                    {
                        Directory.CreateDirectory(StartupPath);
                    }

                    string path = StartupPath + PREFIX + DateTime.Now.ToString("yyyyMMdd") + SUFFIX;
                    bool exists = File.Exists(path);
                    if (logFileStream == null)
                    {
                        logFileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                    }
                    else
                    {
                        if (logFileStream.Name != path)
                        {
                            if (logStreamWrite != null)
                            {
                                logStreamWrite.Close();
                                logStreamWrite = null;
                            }
                            logFileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                        }
                    }
                    if (logStreamWrite == null)
                    {
                        logStreamWrite = new StreamWriter(logFileStream);
                        if (!exists)
                        {
                            logStreamWrite.WriteLine("Start new log file");
                        }
                    }
                    logStreamWrite.WriteLine(sb.ToString());
                    logStreamWrite.Flush();
                    logFileStream.Flush();
                }
                catch (Exception ex)
                {
                    WriteApplicationEventLog(string.Format("HLog.log exception={0} msg={1}", ex.Message, string.Format("{0} {1} {2}", logMsg, Path.GetFileName(sourceFilePath), sourceLineNumber)));
                }
                logMutex.ReleaseMutex();
            }
            else
            {
                WriteApplicationEventLog(string.Format("HLog.log mutex timeout msg={0}", string.Format("{0} {1} {2}", logMsg, Path.GetFileName(sourceFilePath), sourceLineNumber)));
            }
        }

        public static void logTrace(string bcr, string[] logMsg)
        {
            if (logTraceMutex.WaitOne(mutexWait))
            {
                try
                {
                    string barcodeID = "BarcodeID=" + bcr;
                    string dateTime = "DateTime=" + DateTime.Now.ToString("yyyy/MM/dd") + ", " + DateTime.Now.ToString("HH:mm:ss");
                    string recipeName = "RecipeName=" + logMsg[0];
                    string setTopZones = "TopZones=" + logMsg[1];
                    string setBottomZones = "BottomZones=" + logMsg[2];
                    string setConveyor1 = "Conveyor1=" + logMsg[3];
                    string processTopZones = "TopZones=" + logMsg[4];
                    string processBottomZones = "BottomZones=" + logMsg[5];
                    string processConveyor1 = "Conveyor1=" + logMsg[6];
                    string o2PPM = "O2PPM=" + logMsg[7];
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(barcodeID);
                    sb.AppendLine(dateTime);
                    sb.AppendLine(recipeName);
                    sb.AppendLine("[Set Values]");
                    sb.AppendLine(setTopZones);
                    sb.AppendLine(setBottomZones);
                    sb.AppendLine(setConveyor1);
                    sb.AppendLine("[Process Values]");
                    sb.AppendLine(processTopZones);
                    sb.AppendLine(processBottomZones);
                    sb.AppendLine(processConveyor1);
                    sb.AppendLine(o2PPM);

                    if (!Directory.Exists(LogTracePath))
                    {
                        Directory.CreateDirectory(LogTracePath);
                    }

                    // 20220917_104628_70902209173727011.log
                    string path = LogTracePath + PREFIX_TRACE + DateTime.Now.ToString("yyyyMMdd") + "_" +
                        DateTime.Now.ToString("hhmmss") + "_" + bcr + SUFFIX_TRACE;

                    bool exists = File.Exists(path);
                    if (logTraceFileStream == null)
                    {
                        logTraceFileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                    }
                    else
                    {
                        if (logTraceFileStream.Name != path)
                        {
                            if (logTraceStreamWrite != null)
                            {
                                logTraceStreamWrite.Close();
                                logTraceStreamWrite = null;
                            }
                            logTraceFileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                        }
                    }
                    if (logTraceStreamWrite == null)
                    {
                        logTraceStreamWrite = new StreamWriter(logTraceFileStream);
                    }
                    logTraceStreamWrite.WriteLine(sb.ToString());
                    logTraceStreamWrite.Flush();
                    logTraceFileStream.Flush();
                }
                catch (Exception ex)
                {
                    WriteApplicationEventLog(string.Format("HLog.logTrace exception={0} msg={1}", ex.Message, string.Format("{0}", logMsg)));
                }
                logTraceMutex.ReleaseMutex();
            }
            else
            {
                WriteApplicationEventLog(string.Format("HLog.logTrace mutex timeout msg={0}", string.Format("{0}", logMsg)));
            }
        }

        private static void WriteApplicationEventLog(string msg)
        {
            try
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry(msg, EventLogEntryType.Error);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("{0} {1}", msg, e.Message));
            }
        }
    }
}
