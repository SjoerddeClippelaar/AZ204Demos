using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Module02.DurableFunctions
{
    public static class Monitor
    {
        [FunctionName("Monitor")]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            int jobId = context.GetInput<int>();
            int pollingInterval = 30;
            var machineId = "Demo";
            DateTime expiryTime = DateTime.Now + new TimeSpan(0, 30, 0);

            // loop until the decided expiry time, and poll the status every 30 seconds
            while (context.CurrentUtcDateTime < expiryTime)
            {
                var jobStatus = await context.CallActivityAsync<string>("Monitor_GetJobStatus", jobId);
                if (jobStatus == "Completed")
                {
                    // Perform an action when a condition is met.
                    await context.CallActivityAsync("SendAlert", machineId);
                    break;
                }
                // Orchestration sleeps until this time.
                var nextCheck = context.CurrentUtcDateTime.AddSeconds(pollingInterval);
                await context.CreateTimer(nextCheck, CancellationToken.None);
            }
            // Perform more work here, or let the orchestration end.
        }

        [FunctionName("Monitor_GetJobStatus")]
        public static string GetJobStatus([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Checking Status");

            return "Working";
        }

        [FunctionName("Monitor_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Monitor", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}