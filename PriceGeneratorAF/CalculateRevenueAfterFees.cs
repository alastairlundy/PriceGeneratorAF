using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AlastairLundy.PriceGeneratorAF;

public static class CalculateRevenueAfterFees
{
    [FunctionName("CalculateRevenueAfterFees")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CalculateRevenueAfterFees")] HttpRequest req, ILogger log)
    {
        string mPrice;
        string mPercentageFees;
        string mFixedFees;
        string mDecimals;

        int decimalPlacesToUse = 0;
        
        try
        {
            mPrice = req.Query["price"];
            mPercentageFees = req.Query["percentage_fees"];
            mFixedFees = req.Query["fixed_fees"]; 
        }
        catch
        {
            return new BadRequestObjectResult("Please pass the specified parameters on the query string.");
        }
        
        

        decimal price = decimal.Parse(mPrice);
        decimal percentageFees = decimal.Parse(mPercentageFees);
        decimal fixedFees = decimal.Parse(mFixedFees);
        
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
        
        //   string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
     
        // ReSharper disable once CommentTypo
        //Use BIDMAS as order of operations
        var result = price * (decimal.One - (percentageFees / 100));

        result = result - fixedFees;
        
        result = Math.Round(result, decimalPlacesToUse, MidpointRounding.AwayFromZero);
        
        return (ActionResult)new OkObjectResult($"{result}");
    }
}