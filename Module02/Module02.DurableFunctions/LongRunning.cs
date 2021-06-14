using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Module02.DurableFunctions
{
    public static class LongRunning
    {
        [FunctionName("LongRunning")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger logger)
        {
            logger.LogInformation("Starting long running operation (timer)");

            // normally you'd start some external operation here
            await context.CreateTimer(DateTime.Now + new TimeSpan(0, 1, 0), CancellationToken.None);

            logger.LogInformation("Long running operation complete.");

            return;
        }


        [FunctionName("LongRunning_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("LongRunning", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            var response = starter.CreateCheckStatusResponse(req, instanceId);
            var content = await (response.Content as StringContent)?.ReadAsStringAsync();
            
            return response;
        }


        [FunctionName("LongRunning_CheckStatus")]
        public static async Task<HttpResponseMessage> CheckStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            // Function input comes from the request content.

            //log.LogInformation($"Checking Status = '{instanceId}'.");
            var instanceId = req.RequestUri.ParseQueryString()["id"];
            var response = starter.CreateCheckStatusResponse(req, instanceId);
            var content = await (response.Content as StringContent)?.ReadAsStringAsync();

            return response;
            
        }
    }
}