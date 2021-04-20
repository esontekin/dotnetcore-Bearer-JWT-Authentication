using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace SecureClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Making the call, phone ringing....");
            CallApiAsync().GetAwaiter().GetResult();
        }

        private static async Task CallApiAsync()
        {
            var config = AuthConfig.ReadJsonFromFile("appsettings.json");
            var app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();
            var resourceIds = new string[] {config.ResourceId};
            AuthenticationResult result = null;

            try
            {
                result = await app.AcquireTokenForClient(resourceIds).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token Aquired \n");
                Console.WriteLine(result.AccessToken);
                Console.ResetColor();
            }
            catch (MsalClientException exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception.Message);
                Console.ResetColor();
            }

            if (!string.IsNullOrEmpty(result.AccessToken))
            {
                var httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if (defaultRequestHeaders.Accept == null || defaultRequestHeaders.Accept
                    .Any(m=>m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }

                defaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("bearer", result.AccessToken);

                try
                {
                    var response = await httpClient.GetAsync(config.BaseAddress);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        var json = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"\n {json}");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed to call API: {response.StatusCode}");
                        var content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content);
                    }
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to call API: {ex.Message}");
                    Console.ResetColor();
                }
                
            }
        }
    }
}
