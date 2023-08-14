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
            mDecimals = req.Query["decimal_places"];
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
            decimalPlacesToUse = int.Parse(mDecimals);
        }
        catch
        {
            decimalPlacesToUse = 2;
        }
        #endregion
        
        #region Format percentages which are not multipliers
        if (percentageFees > decimal.Parse("1.99"))
        {
            percentageFees = decimal.Divide(percentageFees, decimal.Parse($"100.0"));
        }
        #endregion
        
        //   string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
     
        // ReSharper disable once CommentTypo
        //Use BIDMAS as order of operations
        var result = decimal.Subtract(decimal.Multiply(price, (decimal.One - percentageFees)), fixedFees);
        
        result = Math.Round(result, decimalPlacesToUse, MidpointRounding.AwayFromZero);
        
        return (ActionResult)new OkObjectResult($"{result}");
    }
}