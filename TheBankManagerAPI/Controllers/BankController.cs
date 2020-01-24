using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TheBankManagerAPI.Models;
using TheBankManagerAPI.Service;

namespace TheBankManagerAPI.Controllers
{
    public class BankController : ApiController
    {
        [HttpPost]
        [Route("api/BankSMS")]

        public IHttpActionResult BankSMS(BankMessage bankMessage)
        {
            BankService bnkService = new BankService();
            try
            {
                List<Transaction> allBankSms = bnkService.saveTransactKeyValues(bankMessage);

                var transactionDetails = bnkService.addTransactions(allBankSms);
                return Ok(new
                {
                    IsSuccessful = true,
                    Result = "Transaction Details Added Successfully"
                });
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(new
                {
                    IsSuccessful = false,
                    ErrorMessage = "Error encountered while inserting transactions",
                    Error = e
                }));

            }


        }

        [HttpGet]
        [Route("api/BankSMS")]

        public IHttpActionResult Transactions(string username)
        {
            BankService bnkService = new BankService();
            try
            {


                var transactionDetails = bnkService.GetDashboard(username);
                return Ok(new
                {
                    IsSuccessful = true,
                    Result = "Transaction Details Added Successfully",
                    AllTransactions = transactionDetails
                });
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(new
                {
                    IsSuccessful = false,
                    ErrorMessage = "Error encountered while inserting transactions",
                    Error = e
                }));

            }
        }
    }
}
