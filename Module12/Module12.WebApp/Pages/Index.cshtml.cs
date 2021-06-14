using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace Module12.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        TelemetryClient telemetryClient;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger; 
            
            var config = TelemetryConfiguration.CreateDefault();
            telemetryClient = new TelemetryClient(config);
            
        }

        public void OnGet()
        {
            telemetryClient.TrackEvent("User opened index page");

            var rnd = new Random();
            telemetryClient.GetMetric("some test metric").TrackValue(rnd.Next(1000));

            //_logger.LogInformation("");
            //_logger.LogDebug("This is a test trace at 'Debug' level");

            Trace.TraceInformation("This is a test trace at 'Information' level");
            Trace.TraceError("This is a test trace at 'Debug' level");

            telemetryClient.Flush();
        }
    }
}
