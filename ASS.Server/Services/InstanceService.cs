using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ASS.API;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ASS.Server.Services
{
    class InstanceService : Instance.InstanceBase
    {
        IServiceProvider serviceProvider;
        public InstanceService(IServiceProvider sp) : base()
        {
            serviceProvider = sp;
        }

        public async override Task<InstanceStatus> GetStatus(EmptyRequest request, ServerCallContext context)
        {
            switch (request.Auth.Token)
            {
                case "IB":
                    var version = new ByondVersion() { Major = 512, Minor = 1469 };
                    var byond = serviceProvider.GetRequiredService<ByondService>();
                    await byond.SwitchToVersion(version);
                    return new InstanceStatus
                    {
                        Message = $"Installed {version}"
                    };
                default:
                    return new InstanceStatus
                    {
                        Message = $"YOU:{request.Auth.Token}:WE:{DateTime.Now.ToString()}"
                    };
            }
            
        }
    }
}
