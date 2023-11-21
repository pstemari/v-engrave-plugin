
using CamBam;


namespace VEngraveForCamBam
{
    internal interface Logger
    {
        void Log(string msg);
        void Log(int level, string msg);
        void Log(string format, params object[] vars);
        void Log(int level, string format, params object[] vars);
    }

    // All log messages disabled (to speed things up) - EdddyCurrent
    // [Serializable] removed for CamBAm v1.0 Aug2018
    internal class CamBamLogger : Logger
    {
        public void Log(string msg)
        {
            ThisApplication.AddLogMessage(msg);
        }
        public void Log(int level, string msg)
        {
            ThisApplication.AddLogMessage(level, msg);
        }
        public void Log(string format, params object[] vars)
        {
            ThisApplication.AddLogMessage(format, vars);
        }
        public void Log(int level, string format, params object[] vars)
        {
            // EddyCurrent - only show messages if enabled (isMessages == true)
            if (Params.isMessages) ThisApplication.AddLogMessage(level, format, vars);
        }
    }

    // [Serializable] removed for CamBAm v1.0 Aug2018 [Serializable]
    internal class NullLogger : Logger
    {
        public void Log(string msg) { }
        public void Log(int level, string msg) { }
        public void Log(string format, params object[] vars) { }
        public void Log(int level, string format, params object[] vars) { }
    }
}
