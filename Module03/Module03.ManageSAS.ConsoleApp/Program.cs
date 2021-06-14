using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using EasyConsoleCore;

namespace Module03.ManageSAS.ConsoleApp
{
    class Program
    {
        private const string AccountName = "";
        private const string AccountKey = "";
        private static readonly string StorageConnString = $"DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName={AccountName};AccountKey={AccountKey}";

        private const string ContainerName = "test";
        private const string BlobName = "sampleblob";



        static void Main(string[] args)
        {
            CreateTestBlobs().Wait();

        }

        private static async Task CreateTestBlobs()
        {
            // =============================
            // Create resources to test with
            // =============================

            // first create clients for the storage account, container and blob
            var accountClient = new BlobServiceClient(StorageConnString);

            var result = await accountClient.GetAccountInfoAsync();
            
            var containerClient = accountClient.GetBlobContainerClient(ContainerName);
            await containerClient.DeleteIfExistsAsync();
            await containerClient.CreateAsync();

            var blobClient = containerClient.GetBlobClient(BlobName);
            await blobClient.DeleteIfExistsAsync();

            var file = File.OpenRead("images/Microsoft_Azure-Logo.png");
            await blobClient.UploadAsync(file);



            // ====================
            // generate sas tokens
            // ====================

            var credential = new StorageSharedKeyCredential(AccountName, AccountKey);
            var sasExpiratonTime = DateTime.Now.AddMinutes(30);

            // generate SAS token for account
            var accountSasBuilder = new AccountSasBuilder(
                AccountSasPermissions.Read,
                sasExpiratonTime,
                AccountSasServices.Files | AccountSasServices.Blobs,
                AccountSasResourceTypes.Service);
            var accountSas = accountSasBuilder.ToSasQueryParameters(credential).ToString();
            Console.WriteLine("SAS token for the storage account is: {0}", accountSas);


            // for container
            var containerSasBuilder = new BlobSasBuilder(
                BlobContainerSasPermissions.Read,
                sasExpiratonTime)
            {
                BlobContainerName = ContainerName,
                Resource = "c",
            };
            var containerSas = containerClient.GenerateSasUri(containerSasBuilder);
            Console.WriteLine("SAS token for the container is: {0}", containerSas);


            // for blob
            var blobSasBuilder = new BlobSasBuilder(
                BlobContainerSasPermissions.Read,
                sasExpiratonTime)
            {
                BlobContainerName = ContainerName,
                BlobName = BlobName,
                Resource = "b"
            };
            var blobSas = blobClient.GenerateSasUri(blobSasBuilder);
            Console.WriteLine("SAS token for the blob is: {0}", blobSas);

            // note: it's not possible to retrieve the SAS tokens after you create them
            // meaning you should probably store the keys you make and/or keep the expiration time short
        }
    }
}
