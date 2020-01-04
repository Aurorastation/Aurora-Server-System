using ASS.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASS.Server.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {

        IServiceProvider serviceProvider;

        public StatusController(IServiceProvider sp)
        {
            serviceProvider = sp;
        }

        [Route("")]
        [Route("int")]
        [HttpGet]
        public IEnumerable<int> GetInt()
        {
            var rng = new Random();
            var grpc = serviceProvider.GetRequiredService<GrpcService>();
            if (!grpc.IsInitilized)
                grpc.Initialize();
            return new int[] { rng.Next() };
        }
    }
}
