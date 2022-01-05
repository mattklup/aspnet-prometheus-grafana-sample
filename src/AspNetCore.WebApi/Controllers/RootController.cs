using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Controllers
{
    [ApiController]
    [Route("/test")]
    public class RootController : ControllerBase
    {
        private readonly ILoadTestSimulator simulator;

        public RootController(ILoadTestSimulator simulator)
        {
            this.simulator = simulator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return new ObjectResult(new
            {
                Test = "load",
                User = await this.simulator.SimulateLoadTestAsync(),
            });
        }
    }
}