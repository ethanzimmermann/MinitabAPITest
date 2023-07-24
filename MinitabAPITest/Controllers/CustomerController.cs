using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using MinitabAPI.Models;
using MinitabAPITest.Interfaces;
using MinitabAPITest.Models;
using System.Net;
using static System.Net.WebRequestMethods;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MinitabAPITest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        //Url for validating addresses
        private const string USPSUrl = "http://production.shippingapis.com/ShippingAPI.dll";
        private const string USPSValidateURL = "?API=Verify&XML=<AddressValidateRequest USERID=\"{0}\"><Address ID=\"0\"><FirmName/><Address1/><Address2>{1}</Address2><City>{2}</City><State>{3}</State><Zip5>{4}</Zip5><Zip4/></Address></AddressValidateRequest>";
        
        private readonly IConfiguration _configuration;
        private readonly ICrmRepository _service;
        private HttpClient client;
        private string? _userId;

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="configuration">Local appsettings</param>
        /// <param name="service">Mock CRM Repository Service</param>
        public CustomerController(IConfiguration configuration, ICrmRepository service)
        {
            client = new HttpClient();
            _configuration = configuration;
            _userId = _configuration.GetValue<string>("USPSUserId");
            _service = service;
        }

        // POST <CustomerController>
        /// <summary>
        /// Accepts customer data, validates name, email, and address, and writes appropriately to the CRM
        /// </summary>
        /// <param name="value">JSON Customer Object</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] CustomerInformation value)
        {
            //Response to be returned
            CustomerResponse response = new CustomerResponse()
            {
                Status = "success"
            };

            //Return a failed response if customer name or customer email aren't present
            if(string.IsNullOrEmpty(value.CustomerName) || string.IsNullOrEmpty(value.CustomerEmail))
            {
                response.Status = "failed";
                response.Details = "Customer Name and Email are required";
                return Ok(response);
            }

            //Validate the provided address with the USPS web service and use parsed valid resonse
            //Remove address prior to saving if the address is invalid
            bool validAddress = false;
            if (value.Address != null)
            {
                CustomerAddress copy = value.Address;
                validAddress = validateAddress(ref copy);
                value.Address = copy;
            }
            if (!validAddress)
            {
                value.Address = null;
                response.Status = "partial success";
                response.Details = "Invalid Address";
            }
            _service.UpsertCustomer(value);
            return Ok(response);
        }

        /// <summary>
        /// Validates addresses against the USPS web service
        /// </summary>
        /// <param name="customerAddress">The address to validate</param>
        /// <returns>true if the address is valid, false otherwise</returns>
        private bool validateAddress(ref CustomerAddress customerAddress)
        {
            //Compile and format the query
            string validateURL = USPSUrl + USPSValidateURL;
            validateURL = String.Format(validateURL, _userId, customerAddress.Line1,
                customerAddress.City, customerAddress.State, customerAddress.PostalCode);
            
            //Check the response for an incoming error
            //If valid, parse valid resonse and return as ref
            string addressResponse = client.GetStringAsync(validateURL).Result;
            if (addressResponse.Contains("<Error>"))
            {
                return false;
            }
            else
            {
                customerAddress = parseAddressOutput(addressResponse);
                return true;
            }
        }

        /// <summary>
        /// Parses a valid address response into a CustomerAddress Object
        /// </summary>
        /// <param name="address">valid response address</param>
        /// <returns>CustomerAddress object containing values from valid response</returns>
        private CustomerAddress parseAddressOutput(string addressResponse)
        {
            CustomerAddress customerAddress = new CustomerAddress();
            int startIndex;
            int substringLength;

            getStartIndexAndLength(addressResponse, "Address2", out startIndex, out substringLength);
            customerAddress.Line1 = addressResponse.Substring(startIndex, substringLength);
            getStartIndexAndLength(addressResponse, "State", out startIndex, out substringLength);
            customerAddress.State = addressResponse.Substring(startIndex, substringLength);
            getStartIndexAndLength(addressResponse, "City", out startIndex, out substringLength);
            customerAddress.City = addressResponse.Substring(startIndex, substringLength);
            getStartIndexAndLength(addressResponse, "Zip5", out startIndex, out substringLength);
            customerAddress.PostalCode = addressResponse.Substring(startIndex, substringLength);
            customerAddress.Country = "US";
            return customerAddress;
        }

        /// <summary>
        /// Gets the appropriate start index and length of a CustomerAddress prop in a valid address response
        /// </summary>
        /// <param name="addressResponse">Valid address response as a string</param>
        /// <param name="marker">XML node containing the desired value</param>
        /// <param name="startIndex">Appropriate start index for the desired substring value</param>
        /// <param name="length">Appropriate length for the desired substring value</param>
        private void getStartIndexAndLength(string addressResponse, string marker, out int startIndex, out int length)
        {
            //Get the desired start index - after the desired XML attribute declaration
            startIndex = addressResponse.IndexOf("<"+marker+">") + ("<"+marker+">").Length;
            //Get the desired length
            length = addressResponse.IndexOf("</"+marker+">") - startIndex;
        }
    }
}
