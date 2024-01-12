using Microsoft.AspNetCore.Mvc;
using novibet.models;
using novibet.Datavase;
using novibet.Web;
using System.Threading.Tasks;
using novibet.models.dto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace novibet.Controllers
{
    [Route("api/noviApi")]
    [ApiController]
    public class noviAPI : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> getData([FromQuery] string ipAddress)
        {
            // Your connection string for the database+++ change if you are not me
           // string connectionString = "Data Source=C:\\Users\\james\\Desktop\\novibet\\ipdb.db;Version=3;";
            string connectionString = @"Data Source=..\ipdb.db;Version=3;";

            DbRead dbReader = new DbRead(connectionString);

            // Check if IP exists in the database
            DataDTO data = dbReader.ReadDataFromDatabase(ipAddress);

            if (data != null)
            {               
                return Ok(data);
            }
            else
            {
               
                Ip2cService ip2cService = new Ip2cService(new HttpClient());
                var (twoLetterCode, threeLetterCode, name) = await ip2cService.GetCountryCodesByIpAsync(ipAddress);

                if (!string.IsNullOrEmpty(twoLetterCode) && !string.IsNullOrEmpty(threeLetterCode) && !string.IsNullOrEmpty(name) )
                {
                    // Create a new Data object with the retrieved data
                    var newData = new DataDTO
                    {
                        CountryName = name,
                        TwoLetterCode = twoLetterCode,
                        ThreeLetterCode = threeLetterCode
                    };

                    // Data retrieved from ip2c, update the database using DbWrite
                    DbWrite dbWriter = new DbWrite(connectionString);
                    dbWriter.InsertData(ipAddress, twoLetterCode, threeLetterCode, name);
                    return Ok(newData);
                }
                else
                {
                    // Not found in both database and ip2c
                    return NotFound("Data not found in db or ip2c!");
                }
            }
        }


        
        public class twolettercodes
        {
            public List<string> twolettercodesfilter { get; set; }
        }
        [HttpGet("getAnIPAddresses/raw_input")]
        public ActionResult<Data> getIps([FromQuery]twolettercodes raw_input)
        {
            string connectionString = @"Data Source=..\ipdb.db;Version=3;";


            retrieve_ip_num Retriever = new retrieve_ip_num(connectionString);
            List<Data> dataList = new List<Data>();

            List<Data_correct_response> data_correct_response = new List<Data_correct_response>();

            foreach (string twolettercode in raw_input.twolettercodesfilter)
            {
                Data data = Retriever.Return_IPs(twolettercode);
                if (data != null)
                {
                    dataList.Add(data);
                    Data_correct_response response = new Data_correct_response();
                    response.Country=data.Country;
                    response.Ips_found = data.Ips_found;
                    response.last_upadte= data.last_upadte;
                    data_correct_response.Add(response);
                }
            }
            if (dataList.Count > 0)

            {




                Array ToReturn = data_correct_response.ToArray();

               // return Ok(dataList);
                //return Ok(data_correct_response);
                return Ok(ToReturn);
            }
            else
            {
                return NotFound();
            }


        }

        [HttpPut("UpdateIpsBatch")]
        public IActionResult UpdateIpsBatch()
        {
            try
            {
                // Calculate how many IPs
                update_db Update = new update_db();
                int total_ips = Update.get_max_ips();
                int batches = total_ips / 100 + 1;

                for (int i = 1; i <= batches; i++)
                {
                    List<string> Ips = new List<string>();
                    Ips = Update.GetIPsByPage(i);
                    foreach (string ip in Ips)
                    {
                        DataDTO ip2c_response = Update.IP2C(ip);
                        Update.Update(ip, ip2c_response);

                        Thread.Sleep(1000); // Sleep for 1 second-
                    }
                }

                return Ok("Batch update completed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred during the batch update.");
            }
        }
    }
}
