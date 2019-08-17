using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ASS.Server.Services
{
    class GrpcService : Grpc.Core.Server
    {
        IServiceProvider _sp;
        ILogger logger;
        IConfiguration config;

        public GrpcService(IServiceProvider sp) : this(sp.GetRequiredService<IConfiguration>())
        {
            _sp = sp;
            logger = _sp.GetRequiredService<ILogger<GrpcService>>();
        }

        public GrpcService(IConfiguration _config) : base()
        {
            config = _config;
            Ports.Add(config["GRPC:Host"], int.Parse(config["GRPC:Port"]), ServerCredentials.Insecure);
        }

        public void Initialize()
        {
            Services.Add(API.Instance.BindService(_sp.GetRequiredService<InstanceService>()));
            Start();
            logger.LogInformation($"gRPC server listening on {config["GRPC:Host"]}:{config["GRPC:Port"]}");
        }
    }
}
