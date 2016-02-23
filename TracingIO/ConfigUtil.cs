using System;
using System.Configuration;
using TracingIO.Objects.Enum;

namespace TracingIO
{
    public static class ConfigUtil
    {

        public static string TraceLogFilePath
        {
            get
            {
                string s = ConfigurationManager.AppSettings["TraceLogFilePath"];

                if(String.IsNullOrWhiteSpace(s))
                {
                    s = @"c:\TraceIO\";
                }

                return s;
            }
        }

        public static TraceLevel TraceLevel
        {
            get
            {
                string s = ConfigurationManager.AppSettings["TraceLevel"];

                if (String.IsNullOrWhiteSpace(s))
                {
                    return TraceLevel.Verbose;
                }

                switch(s.ToUpperInvariant())
                {
                    case "ERROR":
                        return TraceLevel.Error;
                    case "WARNING":
                        return TraceLevel.Warning;
                    case "INFO":
                        return TraceLevel.Info;
                    default:
                        return TraceLevel.Verbose;
                }

            }
        }
    }
}
