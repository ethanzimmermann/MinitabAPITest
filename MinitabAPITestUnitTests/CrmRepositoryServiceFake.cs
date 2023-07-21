using MinitabAPITest.Interfaces;
using MinitabAPITest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinitabAPITestUnitTests
{
    internal class CrmRepositoryServiceFake : ICrmRepository
    {
        public Task UpsertCustomer(CustomerInformation customer)
        {
            return Task.CompletedTask;
        }
    }
}
