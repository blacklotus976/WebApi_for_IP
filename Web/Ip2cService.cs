using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace novibet.Web
{
    public class Ip2cService
    {
        private readonly HttpClient _httpClient;

        public Ip2cService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(string TwoLetterCode, string ThreeLetterCode, string name)> GetCountryCodesByIpAsync(string ipAddress)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://ip2c.org/{ipAddress}");
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                // Split by ';' 
                var parts = responseContent.Split(';');
                if (parts.Length >= 2 && parts[0] == "1")
                {
                    return (parts[1], parts[2], parts[3]);
                }
                else
                {
                    Console.WriteLine($"Unexpected response: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return (null, null,null);
        }
    }
}
