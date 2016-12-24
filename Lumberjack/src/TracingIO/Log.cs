using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TracingIO
{
    public class Logger
    {
        string _workingDirectory;

        public Logger(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public async Task Info(string info)
        {
            await WriteFileAsync(GetFilePath(), BuildInfoLog(info));
        }

        public async Task Error(string error) {
            await WriteFileAsync(GetFilePath(), BuildErrorLog(error));
        }

        public async Task Error(Exception ex)
        {
            await WriteFileAsync(GetFilePath(), BuildErrorLog(ex));
        }
        
        public async Task Error(Exception ex, string message) {
            await WriteFileAsync(GetFilePath(), BuildErrorLog(ex, message));
        }

        async Task WriteFileAsync(string filePath, string textToWrite)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(textToWrite);

            using (FileStream sourceStream = new FileStream(filePath,
                   FileMode.Append, FileAccess.Write, FileShare.None,
                   bufferSize: 4096, useAsync: true)
                  )
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        string DetailedExceptionString(Exception exception)
        {
            Exception e = exception;
            StringBuilder s = new StringBuilder();
            while (e != null)
            {
                s.AppendLine("Exception type: " + e.GetType().FullName);
                s.AppendLine("Message       : " + e.Message);
                s.AppendLine("Stacktrace    :");
                s.AppendLine(e.StackTrace);
                s.AppendLine();
                e = e.InnerException;
            }
            return s.ToString();
        }

        private string BuildInfoLog(string info)
        {
            return BuildLog("INFO", info);
        }

        private string BuildErrorLog(Exception ex)
        {
            return BuildErrorLog(DetailedExceptionString(ex));
        }

        private string BuildErrorLog(Exception ex, string message)
        {
            var sb = new StringBuilder($"Additional Message: {message}");
            sb.AppendLine(DetailedExceptionString(ex));

            return BuildLog("ERROR", sb.ToString());
        }

        private string BuildErrorLog(string error) {
            return BuildLog("ERROR", error);
        }

        private string BuildLog(string type, string log)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"** {type}: {DateTimeOffset.Now} **");
            sb.AppendLine(log);
            sb.AppendLine();

            return sb.ToString();
        }
        
        private string GetFilename()
        {
            return $"{DateTime.Now.ToString("yyyy-MM-dd")}.log";
        }

        private string GetFilePath()
        {
            string separator = string.Equals(_workingDirectory.Substring(_workingDirectory.Length - 1), "\\") ? "" : "\\";
            return $"{_workingDirectory}{separator}{this.GetFilename()}";
        }
    }
}
