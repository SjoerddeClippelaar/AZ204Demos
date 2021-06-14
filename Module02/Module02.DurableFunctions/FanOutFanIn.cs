using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Module02.DurableFunctions
{
    public static class FanOutFanIn
    {
        // durable client to start the process
        [FunctionName("FanOutFanIn_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FanOutFanIn", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        // special orchestrator function
        // kicks off the worker functions and waits for them to complete
        [FunctionName("FanOutFanIn")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            
            var parallelTasks = new List<Task<int>>();

            // Get a list of N work items to process in parallel.

            log.LogInformation($"Orchestrator calling prepare batch");
            var workBatch = await context.CallActivityAsync<List<string>>("FanOutFanIn_Prepare", null);

            for (int i = 0; i < workBatch.Count; i++)
            {
                Task<int> task = context.CallActivityAsync<int>("FanOutFanIn_DoWork", workBatch[i]);
                parallelTasks.Add(task);

            }
            var workerResults = await Task.WhenAll(parallelTasks);


            var sum = await context.CallActivityAsync<int>("FanOutFanIn_ProcessResults", workerResults);
            log.LogInformation($"Sum of string lengths: {sum}");

            return;
        }

        [FunctionName("FanOutFanIn_Prepare")]
        public static List<string> PrepareWork([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Preparing FanOut workload.");
            return new List<string>
                {
                    "Tokyo",
                    "Seattle",
                    "Amsterdam",
                };
        }

        [FunctionName("FanOutFanIn_DoWork")]
        public static int DoWork([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Doing work in city: {name}.");
            return name.Length;
        }

        [FunctionName("FanOutFanIn_ProcessResults")]
        public static int ProcessResults([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            log.LogInformation($"Combining/post-processing batch data.");

            var data = context.GetInput<int[]>();
            var result = data.Sum();

            return result;
        }

    }
}