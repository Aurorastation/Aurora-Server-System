using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ASS.API;
using Grpc.Core;

namespace ASS.Server.Services
{
    class InstanceService : Instance.InstanceBase
    {
        public async override Task<InstanceStatus> GetStatus(EmptyRequest request, ServerCallContext context)
        {
            return new InstanceStatus
            {
                Message = $"YOU:{request.Auth.Token}:WE:{DateTime.Now.ToString()}"
            };
        }
    }
}
