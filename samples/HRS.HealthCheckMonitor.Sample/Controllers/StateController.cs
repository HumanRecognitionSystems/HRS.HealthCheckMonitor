using HRS.HealthCheckMonitor.Sample.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HRS.HealthCheckMonitor.Sample.Controllers
{
    [Route("api/state")]
    public class StateController : ControllerBase
    {
        private readonly HealthCheckData _data;

        public StateController(HealthCheckData data)
        {
            _data = data;
        }

        [HttpPost("{check}/{state}")]
        public IActionResult SetState(string check, HealthStatus state)
        {
            if(_data.Checks.TryGetValue(check, out var checkState))
            {
                checkState.Status = state;
            }
            return Ok();
        }
    }
}
