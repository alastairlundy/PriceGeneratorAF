using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Invoiceron.PriceGeneratorAF;

public static class CalculateRevenueAfterFees
{
    [FunctionName("CalculateRevenueAfterFees")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
    {
        //log.LogInformation("C# HTTP trigger function processed a request.");

        decimal price;
        decimal percentageFees;
        decimal fixedFees;

        int decimalPlacesToUse;
        
        try
        {
            price = decimal.Parse(req.Query["price"]);
            percentageFees = decimal.Parse(req.Query["percentage_fees"]);
            fixedFees = decimal.Parse(req.Query["fixed_fees"]);
        }
        catch
        {
            return new BadRequestObjectResult("Please pass the specified parameters on the query string.");
        }

        #region  Set Default Decimal value if null
        try
        {
            decimalPlacesToUse = int.Parse(req.Query["decimal_places"]);
        }
        catch
        {
            decimalPlacesToUse = 2;
        }
        #endregion
        
        if(percentageFees > decimal.Parse("2.0"))
        {
            percentageFees = decimal.Divide(percentageFees, decimal.Parse("100.0"));
        }

        //   string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
     
        // ReSharper disable once CommentTypo
        //Use BIDMAS as order of operations
        var result = decimal.Subtract(decimal.Multiply(price, (decimal.One - percentageFees)), fixedFees);
        
        result = Math.Round(result, decimalPlacesToUse, MidpointRounding.AwayFromZero);
        
        return (ActionResult)new OkObjectResult($"{result}");
    }
}