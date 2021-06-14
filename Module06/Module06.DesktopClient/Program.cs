using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using EasyConsoleCore;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Directory = System.IO.Directory;
using File = Microsoft.Graph.File;

namespace Module06.DesktopClient
{
	public class Secrets
	{
		public string ClientSecret { get; set; }
	}

	class Program
	{
		private const string tenantId = "";
		private const string clientId = "";

		// optional, you can also set a user secret "ClientSecret" to the client secret (you can create one under 'certificates and secrets' in the Azure Portal page for your App registration.
		private const string clientCertificatePath = "";

		private const string storageAccountName = "";
		private const string storageAccountUrl = "https://" + storageAccountName + ".blob.core.windows.net/";

		static void Main(string[] args)
		{
            var menu = new Menu();
			menu.Add("Request user token with MSAL", () => GetAccessToken().Wait());
			menu.Add("Request user data with Graph", () => GetUserData().Wait());
			menu.Add("Create confidential client", () => DemoConfidentialClient().Wait());
			menu.Add("Create storage data", () => CreateStorageData().Wait());

			menu.Display();
		}

		private static IPublicClientApplication BuildPublicClient()
		{
			var publicApp = PublicClientApplicationBuilder
				.Create(clientId)
				.WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
				.WithRedirectUri("http://localhost")
				.Build();

			return publicApp;
		}

		private static string GetClientSecret()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddUserSecrets<Secrets>();
			var config = builder.Build();

			var secret = config["ClientSecret"];
			if (string.IsNullOrWhiteSpace(secret))
			{
				throw new InvalidOperationException("Missing client secret");
			}

			return secret;
		}

		private static IConfidentialClientApplication BuildConfidentialClient()
		{
			var secret = GetClientSecret();

			var confidentialApp = ConfidentialClientApplicationBuilder
				.Create(clientId)
				.WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
				.WithClientSecret(secret)
				.Build();

			return confidentialApp;
		}

		private static async Task GetAccessToken()
		{
			var app = BuildPublicClient();

			List<string> scopes = new List<string>
			{
				"user.read"
			};

			var result = await app
				.AcquireTokenInteractive(scopes)
				.ExecuteAsync();

			Console.WriteLine($"Token:\t{result.AccessToken}");
		}

		private static async Task GetUserData()
		{
			var app = BuildPublicClient();

			List<string> scopes = new List<string>
			{
				"user.read"
			};

			DeviceCodeProvider provider = new DeviceCodeProvider(app, scopes);
			GraphServiceClient client = new GraphServiceClient(provider);

			User myProfile = await client.Me
				.Request()
				.GetAsync();
			
			Console.WriteLine($"Name:\t{myProfile.DisplayName}");
			Console.WriteLine($"AAD Id:\t{myProfile.Id}");
		}

		private static async Task CreateStorageData()
		{
			TokenCredential tokenCredential;

			if (!string.IsNullOrEmpty(clientCertificatePath))
			{
				tokenCredential = new ClientCertificateCredential(tenantId, clientId, clientCertificatePath);
			}
			else
			{

				try
				{
					var clientSecret = GetClientSecret();
					tokenCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
				}
				catch (InvalidOperationException e) when (e.Message.Contains("Missing client secret"))
				{
					Console.WriteLine("The app's configuration is missing an app registration client secret.");
					Console.WriteLine("Please add it, preferably by using Visual Studio's user secrets. You can also use the appsettings.json file.");
					Console.WriteLine("For a production app, you should use a certificate instead.");
					return;
				}
			}


			var account = new BlobServiceClient(new Uri(storageAccountUrl), tokenCredential);


			await foreach (var container in account.GetBlobContainersAsync())
			{
				Console.WriteLine("container: " + container.Name);
			}

			var containerClient = account.GetBlobContainerClient("demo");


			var result = await containerClient.CreateIfNotExistsAsync();

			
		}

		private static async Task DemoConfidentialClient()
		{
			var app = BuildConfidentialClient();

			var result = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();

			Console.WriteLine($"Token:\t{result.AccessToken}");

			return;
		}
	}
}
