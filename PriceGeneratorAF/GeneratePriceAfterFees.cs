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
            mDecimals = req.Query["decimal_places"];
        }
        catch
        {
            return new BadRequestObjectResult("Please pass the specified parameters on the query string.");
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
        
        #region Format percentages which are not multipliers
        if (percentageFees > decimal.Parse("1.99"))
        {
            percentageFees = decimal.Divide(percentageFees, decimal.Parse("100.0"));
        }
        #endregion
        
        var result = decimal.Divide(decimal.Add(goalPrice, fixedFees), percentageFees);
        
        result = Math.Round(result, decimalPlacesToUse, MidpointRounding.AwayFromZero);
        
        return (ActionResult)new OkObjectResult($"{result}");
        
    }
}