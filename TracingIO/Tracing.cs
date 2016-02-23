using TracingIO.Objects.Enum;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;

namespace TracingIO
{
    public static class Tracing
    {
        private static object _writeLockObj = new object();

        /// <summary>
        /// Writes out a line with the given message if the trace level is set to Verbose.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteVerboseLine(string message)
        {
            if (ConfigUtil.TraceLevel >= TraceLevel.Verbose)
            {
                WriteLine(message, TraceLevel.Verbose);
            }
        }

        /// <summary>
        /// Writes out a line with the given message if the trace level is set to Verbose, Info, or Warning.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteWarningLine(string message)
        {
            if (ConfigUtil.TraceLevel >= TraceLevel.Warning)
            {
                WriteLine(message, TraceLevel.Warning);
            }
        }

        /// <summary>
        /// Writes out a line with the given message if the trace level is set to Info or Verbose.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInfoLine(string message)
        {
            if (ConfigUtil.TraceLevel >= TraceLevel.Info)
            {
                WriteLine(message, TraceLevel.Info);
            }
        }

        /// <summary>
        /// Writes out a line with the given message if the trace level is set to Error, Warning, Info, or Verbose.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteErrorLine(string message)
        {
            if (ConfigUtil.TraceLevel >= TraceLevel.Error)
            {
                WriteLine(message, TraceLevel.Error);
            }
        }

        /// <summary>
        /// Writes out a line with the given message if the trace level is set to Error, Warning, Info, or Verbose.  Also, if the emailErrors flag is true in the web.config, an email with this message will be sent to the emailErrorsTo address.
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteException(Exception ex)
        {
            WriteException(ex, string.Empty);
        }

        /// <summary>
        /// This writes out the exception as an error status if the Agility.Config trace switch is set to output error messages.
        /// </summary>
        /// <param name="appendMessage"></param>
        /// <param name="ex"></param>
        public static void WriteException(Exception ex, string appendMessage)
        {
            TraceLevel traceLevel = TraceLevel.Error;

            //write the error to the log
            StringBuilder sb = new StringBuilder();

            if (HttpContext.Current != null)
            {
                sb.Append("Request Details:");

                sb.Append(Environment.NewLine);
                sb.Append("URL: ").Append(HttpContext.Current.Request.Url);

                try
                {
                    if (HttpContext.Current.Request.UrlReferrer != null)
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append("Referrer: ").Append(HttpContext.Current.Request.UrlReferrer);
                    }
                }
                catch { }

                sb.Append(Environment.NewLine);
                sb.Append("User Agent: ").Append(HttpContext.Current.Request.UserAgent);

                sb.Append(Environment.NewLine);
                sb.Append("Host Address: ").Append(HttpContext.Current.Request.UserHostAddress);

                if (!string.IsNullOrEmpty(HttpContext.Current.Request.UserHostName))
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("User Host Name: ").Append(HttpContext.Current.Request.UserHostName);
                }

                //login name
                if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("Username: ");
                    sb.Append(HttpContext.Current.User.Identity.Name);
                }

            }

            if (System.Threading.Thread.CurrentPrincipal != null
                && System.Threading.Thread.CurrentPrincipal.Identity != null
                && !string.IsNullOrEmpty(System.Threading.Thread.CurrentPrincipal.Identity.Name))
            {
                sb.Append(Environment.NewLine);
                sb.Append("Identity: ").Append(System.Threading.Thread.CurrentPrincipal.Identity.Name);

            }

            if (ex.InnerException is SqlException)
            {
                sb.Append(Environment.NewLine);
                sb.Append("SQL Details:").Append(Environment.NewLine);
                SqlException sqlEx = (SqlException)ex.InnerException;

                foreach (SqlError error in sqlEx.Errors)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(" - ").Append(error.Message).Append(" - in Proc: ").Append(error.Procedure).Append(" line ").Append(error.LineNumber);

                }

            }
            else if (ex is SqlException)
            {
                sb.Append(Environment.NewLine);
                sb.Append("SQL Details:").Append(Environment.NewLine);
                SqlException sqlEx = (SqlException)ex;
                foreach (SqlError error in sqlEx.Errors)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(" - ").Append(error.Message).Append(" - in Proc: ").Append(error.Procedure).Append(" line ").Append(error.LineNumber);

                }
            }
            else
            {
                sb.Append(Environment.NewLine);
                sb.Append(ex.ToString());

            }
            if (!string.IsNullOrEmpty(appendMessage))
            {
                sb.Append(Environment.NewLine);
                sb.Append("Additional Message: ").Append(appendMessage);

            }

            string message = sb.ToString();

            //actually write the message to the log base on the tracelevel defined for this message.
            switch (traceLevel)
            {
                case TraceLevel.Error:
                    WriteErrorLine(message);
                    break;
                case TraceLevel.Warning:
                    WriteWarningLine(message);
                    break;
                case TraceLevel.Info:
                    WriteInfoLine(message);
                    break;
                case TraceLevel.Verbose:
                    WriteVerboseLine(message);
                    break;

            }
        }

        private static void WriteLine(string message, TraceLevel level)
        {

            //build the output
            StringBuilder output = new StringBuilder();
            output.AppendFormat("*** {0} *** {1} *** {2:yyyy-MM-dd HH:mm:ss}",
                level.ToString().PadRight(8, ' '),
                System.Environment.MachineName,
                DateTime.Now);
            output.Append(Environment.NewLine);
            output.Append(message);
            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);


            WriteLineToFile(output.ToString());
        }

        private static void WriteLineToFile(string message)
        {

            try
            {

                string logFile = GetLogFilePath();

                if (!string.IsNullOrEmpty(logFile))
                {

                    FileInfo fo = new FileInfo(logFile);

                    if (fo.Exists && fo.IsReadOnly)
                    {
                        return;
                    }

                    lock (_writeLockObj)
                    {
                        string folder = Path.GetDirectoryName(logFile);
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        
                        File.AppendAllText(logFile, message);
                    }

                }
            }
            catch { }


        }

        internal static string GetLogFilePath()
        {
            return GetLogFilePath(DateTime.Now);
        }

        internal static string GetLogFilePath(DateTime logFileDate)
        {
            string logFile = ConfigUtil.TraceLogFilePath;

            //only if we have a file to write to...
            if (!string.IsNullOrEmpty(logFile))
            {
                //append date information to the log file
                if (Path.HasExtension(logFile))
                {
                    logFile = Path.GetDirectoryName(logFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(logFile);
                }

                logFile = string.Format("{0}{1:_yyyy_MM_dd}.log", logFile, logFileDate);
            }

            return logFile;
        }
    }
}
