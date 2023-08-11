using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PriceGeneratorAF;

public static class CalculateRevenueAfterFees
{
    [FunctionName("CalculateRevenueAfterFees")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string price = req.Query["price"];
        string percentageFees = req.Query["percentage_fees"];
        string fixedFees = req.Query["fixed_fees"];

        string decimalPlacesToUse = req.Query["decimal_place_usage"];

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        
        price = price ?? data?.price;
        percentageFees = percentageFees ?? data?.percentageFees;
        fixedFees = fixedFees ?? data?.fixedFees;
        decimalPlacesToUse = decimalPlacesToUse ?? data?.decimalPlacesToUse;

        if (fixedFees == null || percentageFees == null || price == null)
        {
            return new BadRequestObjectResult("Please pass a parameter on the query string for each required parameter or in the request body");
        }
        
        // ReSharper disable once CommentTypo
        //Use BIDMAS as order of operations
        var result = (decimal.Parse(price) * (decimal.One - decimal.Parse(percentageFees)) - decimal.Parse(fixedFees));

        int decimals = int.Parse(decimalPlacesToUse) != null ? int.Parse(decimalPlacesToUse) : 2;
        
        result = Math.Round(result, decimals, MidpointRounding.AwayFromZero);
        
        return result != null
            ? (ActionResult)new OkObjectResult($"{result}")
            : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        
    }
}