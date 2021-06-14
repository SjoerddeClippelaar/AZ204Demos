using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Module02.AzureFunctions1
{
    public static class BlobFunctionBasic
    {
        public const string StorageConnString = "AzureWebJobsStorage";

        [FunctionName("BlobFunctionBasic")]
        public static void Run(
            [BlobTrigger("basicblobfunction-inputs/{name}")]
            [StorageAccount(StorageConnString)]
            Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
