using ASS.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASS.Server.Web.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ByondController : ControllerBase
    {

        ByondService byondService;

        public ByondController(ByondService bs)
        {
            byondService = bs;
        }

        [HttpPost("install/{major}.{minor}")]
        public async Task<int> InstallVersionAsync(int major, int minor)
        {
            await byondService.SwitchToVersion(new API.ByondVersion() { Major = major, Minor = minor });
            return 0;
        }

    }
}
