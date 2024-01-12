using System.Data.SQLite;
using System.Data;
using novibet.models.dto;
using novibet.Web;
using static novibet.Controllers.noviAPI;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Xml.Linq;
using Org.BouncyCastle.Ocsp;

namespace novibet.Datavase
{
    
    public class update_db
    {
        private string _connectionString = @"Data Source=..\ipdb.db;Version=3;";

        public int GetNewCId()
        {
            int id = 0;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"SELECT MAX(Id)+1 FROM Countries;";
                    id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return id;
        }
        public int get_max_ips()
        {
            int ret = 0;
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"SELECT COUNT(IP) FROM IPAddresses ;";
                    ret = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        
    
            return ret;

        }

        public List<string> GetIPsByPage(int pageNumber)
        {
            var returnList = new List<string>();
            int batchSize = 100; // Number of IPs per page
            int offset = (pageNumber - 1) * batchSize;

            string connectionString = @"Data Source=..\ipdb.db;Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT IP FROM IPAddresses LIMIT @batchSize OFFSET @offset";
                    command.Parameters.AddWithValue("@batchSize", batchSize);
                    command.Parameters.AddWithValue("@offset", offset);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string ipAddress = reader.GetString(0);
                            returnList.Add(ipAddress);
                        }
                    }
                }
            }

            return returnList;
        }

        public int Country_exists(string twolettercode)
        {
            
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT  Id FROM Countries WHERE Countries.TwoLetterCode=@twolettercode";
                    command.Parameters.AddWithValue("@twolettercode", twolettercode);
                    return Convert.ToInt32(command.ExecuteScalar());

                }
            }
        }
    
        public DataDTO IP2C(string ip)
        {
            DataDTO ip2c = new DataDTO();
            //Ip2cService ip2cService = new Ip2cService(new HttpClient());
            //var (two, three, name) = ip2cService.GetCountryCodesByIpAsync(ip);

            // search on ip2c.org 
            WebClient wc = new WebClient();
            var response = wc.DownloadString("http://ip2c.org/" + ip);
            var output = response.Split(';');

            ip2c.TwoLetterCode = output[1];
            ip2c.ThreeLetterCode = output[2];
            ip2c.CountryName = output[3];

            return ip2c;
        }
        public void Update(string ipAddress, DataDTO ip2c_response)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {            
                connection.Open();
                int getcountry_id = 0;

                using (var command = new SQLiteCommand(connection))
                {
                    getcountry_id = Country_exists(ip2c_response.TwoLetterCode);
                    if (getcountry_id == 0)
                    {
                    //Case:Create Country
                        getcountry_id = GetNewCId();
                        command.CommandType = CommandType.Text;
                        command.CommandText = "INSERT INTO Countries (Id, Name, TwoLetterCode, ThreeLetterCode, CreatedAt) VALUES (@Id, @Name, @TwoLetterCode, @ThreeLetterCode, @CreatedAt)";
                        command.Parameters.AddWithValue("@Id", getcountry_id);
                        command.Parameters.AddWithValue("@Name", ip2c_response.CountryName);
                        command.Parameters.AddWithValue("@TwoLetterCode", ip2c_response.TwoLetterCode);
                        command.Parameters.AddWithValue("@ThreeLetterCode", ip2c_response.ThreeLetterCode);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    
                    //in anyway change country_id of IPAddresses table
                    command.CommandType = CommandType.Text;
                    command.CommandText = "UPDATE IPAddresses SET CountryId=@countryid, UpdatedAt = @updatedat WHERE IP =@ip; ";
                    //command.CommandText = "UPDATE IPAddresses SET CountryId=(SELECT Id FROM Countries WHERE TwoLetterCode=@twolettercode) WHERE IP =@Ip; ";
                    command.Parameters.AddWithValue("@ip", ipAddress);
                    command.Parameters.AddWithValue("@countryid", getcountry_id);
                    command.Parameters.AddWithValue("@updatedat", DateTime.Now);
                    //command.Parameters.AddWithValue("@twolettercode", ip2c_response.TwoLetterCode);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                connection.Close();
            }
        }
    }
}

