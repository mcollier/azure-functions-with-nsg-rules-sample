using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web.Http;

namespace Company.Function
{
    public static class HttpTriggerCSharp1
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("HttpTriggerCSharp1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("HttpOutbound")]
        public static async Task<IActionResult> GetExternalWebSiteData([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest request, ILogger log)
        {
            IActionResult result = new OkResult();
            string targetUrl = Environment.GetEnvironmentVariable("TargetUrl");

            try
            {
                log.LogInformation($"Attempting to invoke '{targetUrl}'.");

                var httpResponse = await httpClient.GetAsync(targetUrl);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    log.LogError("Failed to get the requested site. {0}", httpResponse.ReasonPhrase);
                    result = new BadRequestObjectResult($"Failed to retrieve requested site ('{targetUrl}'). {httpResponse.ReasonPhrase}");
                }
                else
                {
                    log.LogInformation($"Successfully called to {targetUrl}!");
                    result = new OkObjectResult($"Successfully called to {targetUrl}!");
                }
            }
            catch (Exception e)
            {
                log.LogError("Exception while attempting to call URL! {0} - {1}", e.ToString(), e.Message);
                result = new ExceptionResult(e, true);
            }

            return result;
        }
    }
}
