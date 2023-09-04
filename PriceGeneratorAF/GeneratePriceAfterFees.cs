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

public static class GeneratePriceAfterFees
{
    [FunctionName("GeneratePriceAfterFees")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GeneratePriceAfterFees")] HttpRequest req, ILogger log)
    {
        string mGoalPrice;
        string mPercentageFees;
        string mFixedFees;
        string mDecimals;

        int decimalPlacesToUse;
        
        try
        {
            mGoalPrice = req.Query["goal_price"];
            mPercentageFees = req.Query["percentage_fees"];
            mFixedFees = req.Query["fixed_fees"];
        }
        catch
        {
            return new BadRequestObjectResult("Please pass the specified parameters on the query string.");
        }

        try
        {
            mDecimals = req.Query["decimal_places"];
        }
        catch
        {
            mDecimals = decimal.Parse("2").ToString();
        }
        
        decimal goalPrice = decimal.Parse(mGoalPrice);
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
        
        var result = decimal.Add(goalPrice, fixedFees) / (decimal.One - (percentageFees / 100));
        
        result = Math.Round(result, decimalPlacesToUse, MidpointRounding.AwayFromZero);
        
        return (ActionResult)new OkObjectResult($"{result}");
        
    }
}