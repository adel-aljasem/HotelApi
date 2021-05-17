using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayPal;
using PayPal.Api;
using Models;

namespace Hotel_Api.Controllers
{
    [Route("api/[controller]/[action]")]
    public class PaypalPaymentController : Controller
    {
        private readonly IConfiguration configuration;

        // Get a reference to the config
        static Dictionary<string, string> config = new Dictionary<string, string> {
            {"clientId", "Abv1o4lKxU7YPDD9T1lKxlQJMVsaEKtK8eKBXGokhdEK8IKsr4OjKwkE3W0tsk-dHNEJGE58lSpW5xBp" },
            {"clientSecret","EO9t-bP5OP06ill1LS7EdutEKVRwnjkisYBeo8zH2w5ak6VdSlkpQinJqmEYQrAn_GrT3D2MZivXLSN0" }
        };

        // Use OAuthTok
        static string accessToken = new OAuthTokenCredential(config).GetAccessToken();

        // Use OAuthTokenCredential to request an access token from PayPal

        APIContext apiContext;

        public PaypalPaymentController(IConfiguration configuration)
        {
            this.configuration = configuration;
            APIContext apiContext = new APIContext(accessToken);

            // Initialize the apiContext's configuration with the default configuration for this application.
            apiContext.Config = ConfigManager.Instance.GetProperties();

            
            // Define any custom configuration settings for calls that will use this object.
            apiContext.Config["connectionTimeout"] = "100000"; // Quick timeout for testing purposes

            // Define any HTTP headers to be used in HTTP requests made with this APIContext object
            if (apiContext.HTTPHeaders == null)
            {
                apiContext.HTTPHeaders = new Dictionary<string, string>();
            }
            apiContext.HTTPHeaders["Accept"] = "application/json";
            this.apiContext = apiContext;


        }



        

        [HttpGet]
        public IActionResult Payment(string redirectUrl, string cancel_url, string price)
        {
            try
            {

             

    

                var redirect = new RedirectUrls { return_url = redirectUrl, cancel_url = cancel_url };

                var transactionList = new List<Transaction>();

              

                transactionList.Add(new Transaction
                {
                    description = "test",
                    amount = new PayPal.Api.Amount
                    {
                        currency="USD",
                        total = price
                    }

                });



                Payment pay = new Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal"},
                    transactions = transactionList,
                    redirect_urls = redirect,
                }.Create(apiContext);

                var links = pay.links.GetEnumerator();


                string paypalRedirectUrl = string.Empty;

                while (links.MoveNext())
                {
                    Links link = links.Current;
                    if (link.rel.ToLower().Trim().Equals("approval_url"))
                    {
                        paypalRedirectUrl = link.href;
                    }
                }


                PayPalDTO payPalDTO = new PayPalDTO
                {
                    Link = paypalRedirectUrl,
                    total = price
                    
                };


                return Ok(payPalDTO);
            }
            catch (PayPalException e)
            { 
                return BadRequest(e.Message);
            }



        }
    }
}
