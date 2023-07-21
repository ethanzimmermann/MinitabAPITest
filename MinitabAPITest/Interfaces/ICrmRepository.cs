using MinitabAPITest.Models;

namespace MinitabAPITest.Interfaces
{
    public interface ICrmRepository
    {
        Task UpsertCustomer(CustomerInformation customer);
    }
}
