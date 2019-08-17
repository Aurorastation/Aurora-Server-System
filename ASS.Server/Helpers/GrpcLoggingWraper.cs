using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ASS.Server.Helpers
{
    class GrpcLoggingWraper : Grpc.Core.Logging.ILogger
    {
        private ILogger _logger;
        public GrpcLoggingWraper(ILogger logger)
        {
            _logger = logger;
        }

        public Grpc.Core.Logging.ILogger ForType<T>() => this;
        public void Debug(string message) => _logger.LogDebug(message);
        public void Debug(string format, params object[] formatArgs) => _logger.LogDebug(format, args: formatArgs);
        public void Error(string message) => _logger.LogError(message);
        public void Error(string format, params object[] formatArgs) => _logger.LogError(format, args: formatArgs);
        public void Error(Exception exception, string message) => _logger.LogError(exception, message);
        public void Info(string message) => _logger.LogInformation(message);
        public void Info(string format, params object[] formatArgs) => _logger.LogInformation(format, args: formatArgs);
        public void Warning(string message) => _logger.LogWarning(message);
        public void Warning(string format, params object[] formatArgs) => _logger.LogWarning(format, args: formatArgs);
        public void Warning(Exception exception, string message) => _logger.LogWarning(exception, message);
    }
}
