using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MinitabAPI.Models;
using MinitabAPITest.Controllers;
using MinitabAPITest.Interfaces;
using MinitabAPITest.Models;

namespace MinitabAPITestUnitTests
{
    public class CustomerControllerTest
    {
        private readonly CustomerController _customerController;
        private readonly ICrmRepository _repository;

        public CustomerControllerTest()
        {
            _repository = new CrmRepositoryServiceFake();
            ConfigurationManager mock = new ConfigurationManager();
            mock.AddJsonFile("testsettings.json");
            _customerController = new CustomerController(mock, _repository);
        }

        [Fact]
        public void Test_Valid_Customer()
        {
            //Arrange
            CustomerInformation mockInfo = new CustomerInformation()
            {
                CustomerName = "Fred Flintstone",
                CustomerEmail = "fred.flintstone@bedrock-llc.com"
            };
            mockInfo.Address = new CustomerAddress()
            {
                Line1 = "10 South LaSalle St.",
                City = "Chicago",
                State = "IL",
                PostalCode = "60603",
                Country = "US"
            };

            //Act
            var response = _customerController.Post(mockInfo) as OkObjectResult;
            CustomerResponse? result = (response != null && response.Value != null ? 
                response.Value as CustomerResponse : 
                new CustomerResponse() { Status = "None"});
            
            //Assert
            Assert.NotNull(result);
            Assert.Equal("success", result.Status);
        }

        [Fact]
        public void Test_Invalid_Address()
        {
            //Arrange
            CustomerInformation mockInfo = new CustomerInformation()
            {
                CustomerName = "Fred Flintstone",
                CustomerEmail = "fred.flintstone@bedrock-llc.com"
            };
            mockInfo.Address = new CustomerAddress()
            {
                Line1 = "bad",
                City = "bad",
                State = "bad",
                PostalCode = "bad",
                Country = "bad"
            };

            //Act
            var response = _customerController.Post(mockInfo) as OkObjectResult;
            CustomerResponse? result = (response != null && response.Value != null ? 
                response.Value as CustomerResponse : 
                new CustomerResponse() { Status = "None" });

            //Assert
            Assert.NotNull(result);
            Assert.Equal("partial success", result.Status);
            Assert.Equal("Invalid Address", result.Details);
        }

        [Fact]
        public void Test_Invalid_Customer()
        {
            CustomerInformation mockInfo = new CustomerInformation()
            {
                CustomerName = "Fred Flintstone"
            };
            mockInfo.Address = new CustomerAddress()
            {
                Line1 = "bad",
                City = "bad",
                State = "bad",
                PostalCode = "bad",
                Country = "bad"
            };

            var response = _customerController.Post(mockInfo) as OkObjectResult;
            CustomerResponse? result = (response != null && response.Value != null ? 
                response.Value as CustomerResponse : 
                new CustomerResponse() { Status = "None" });

            Assert.NotNull(result);
            Assert.Equal("failed", result.Status);
            Assert.Equal("Customer Name and Email are required", result.Details);
        }
    }
}