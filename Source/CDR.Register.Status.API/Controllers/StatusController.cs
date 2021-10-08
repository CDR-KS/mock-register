using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Status.API.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Threading.Tasks;

namespace CDR.Register.Status.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    [CheckIndustry]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _statusService;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IStatusService StatusService, ILogger<StatusController> logger)
        {
            _statusService = StatusService;
            _logger = logger;
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        public async Task<IStatusCodeActionResult> GetDataRecipientsStatus(string industry)
        {
            using (LogContext.PushProperty("ControllerName", ControllerContext.RouteData.Values["controller"].ToString()))
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation("Request received");
            }
            return Ok(await _statusService.GetDataRecipientStatusesAsync(industry.ToIndustry()));
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/software-products/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        public async Task<IStatusCodeActionResult> GetSoftwareProductStatus(string industry)
        {
            using (LogContext.PushProperty("ControllerName", ControllerContext.RouteData.Values["controller"].ToString()))
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation("Request received");
            }
            return Ok(await _statusService.GetSoftwareProductStatusesAsync(industry.ToIndustry()));
        }
    }
}