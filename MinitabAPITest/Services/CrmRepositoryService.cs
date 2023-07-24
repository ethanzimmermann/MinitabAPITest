using MinitabAPITest.Interfaces;
using MinitabAPITest.Models;
using System.Text;
using System.Text.Json;

namespace MinitabAPI.Services
{
    public class CrmRepositoryService : ICrmRepository
    {
        /// <summary>
        /// Writes a customer's information to a file in AppData/Roaming, or the designated ApplicationData folder
        /// I decided to do something a little silly here that wouldn't actually require that I start up the Azure free trial
        /// </summary>
        /// <param name="customer">Customer Information</param>
        /// <returns>Task Complete</returns>
        public Task UpsertCustomer(CustomerInformation customer)
        {
            //Get serialized data
            string customerJson = JsonSerializer.Serialize(customer);

            //Define and create output directory
            var outputDirectory = String.Format(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "{0}"), "Minitab");
            Directory.CreateDirectory(outputDirectory);

            //Get the output file path and write to it
            var outputFile = String.Format(Path.Combine(outputDirectory, "{0}.json"), customer.CustomerName);

            File.WriteAllText(outputFile, customerJson);
            return Task.CompletedTask;
        }
    }
}
