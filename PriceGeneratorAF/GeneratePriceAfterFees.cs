using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PriceGeneratorAF;

public static class GeneratePriceAfterFees
{
    [FunctionName("GeneratePriceAfterFees")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string goalPrice = req.Query["goal_price"];
        string percentageFees = req.Query["percentage_fees"];
        string fixedFees = req.Query["fixed_fees"];
        string decimalPlacesToUse = req.Query["decimal_place_usage"];
        
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        
        goalPrice = goalPrice ?? data?.goalPrice;
        percentageFees = percentageFees ?? data?.percentageFees;
        fixedFees = fixedFees ?? data?.fixedFees;
        decimalPlacesToUse = decimalPlacesToUse ?? data?.decimalPlacesToUse;
        
        decimal newPercentage;
        
        if (decimal.Parse(percentageFees) > 1)
        {
            newPercentage = decimal.One - decimal.Parse(percentageFees);
        }
        else
        {
            newPercentage = decimal.Parse(percentageFees);
        }
        
        int decimals = int.Parse(decimalPlacesToUse) != null ? int.Parse(decimalPlacesToUse) : 2;
        
        var result = decimal.Parse(goalPrice + fixedFees) / newPercentage;
        
        result = Math.Round(result, decimals, MidpointRounding.AwayFromZero);
        
        return result != null
            ? (ActionResult)new OkObjectResult($"{result}")
            : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        
    }
}