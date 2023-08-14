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
        string m_price;
        string m_percentageFees;
        string m_fixedFees;
        string m_decimals;

        int decimalPlacesToUse;
        
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        
        try
        {
            m_price = req.Query["price"];
            m_percentageFees = req.Query["percentage_fees"];
            m_fixedFees = req.Query["fixed_fees"];
            m_decimals = req.Query["decimal_places"];
            
            m_price = m_price ?? data?.goalPrice;
            m_percentageFees = m_percentageFees ?? data?.percentageFees;
            m_fixedFees = m_fixedFees ?? data?.fixedFees;
            m_decimals = m_decimals ?? data?.decimalPlacesToUse;
        }
        catch
        {
            return new BadRequestObjectResult("Please pass the specified parameters on the query string.");
        }

        decimal price = decimal.Parse(m_price);
        decimal percentageFees = decimal.Parse(m_percentageFees);
        decimal fixedFees = decimal.Parse(m_fixedFees);
        
        #region  Set Default Decimal value if null
        try
        {
            decimalPlacesToUse = int.Parse(m_decimals);
        }
        catch
        {
            decimalPlacesToUse = 2;
        }
        #endregion
        
        #region Format percentages which are not multipliers
        if (percentageFees > decimal.Parse("1.99"))
        {
            percentageFees = decimal.Subtract(decimal.One, percentageFees);
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