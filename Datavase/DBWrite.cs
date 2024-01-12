using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;

namespace novibet.Datavase
{
    
    public class DbWrite
    {
        private readonly string _connectionString;

        public int GetCountryId(string twoLetterCode)
        {
            int id = -1; 

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"SELECT Id FROM Countries WHERE TwoLetterCode = @twolettercode";
                    command.Parameters.AddWithValue("@TwoLetterCode", twoLetterCode);

                   
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        id = Convert.ToInt32(result);
                    }
                }
            }

            return id;
        }

        public int GetNewIpId()
        {
            int id= 0;
            using (var connection= new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using( var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"SELECT MAX(Id)+1 FROM IPAddresses;";
                    id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return id;

        }

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

        public int TwoLetterCodeExists(string twoLetterCode)
        {
            int country_id = 0;


            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"SELECT Id FROM Countries WHERE TwoLetterCode = @TwoLetterCode";
                    command.Parameters.AddWithValue("@TwoLetterCode", twoLetterCode);

                    country_id = Convert.ToInt32(command.ExecuteScalar());
            
                }
            }

            return country_id;
        }

        public DbWrite(string connectionString)
        {
            //_connectionString = "Data Source=C:\\Users\\james\\Desktop\\novibet\\ipdb.db;Version=3;";//input string here!!sos different connection for every computer
             _connectionString = @"Data Source=..\ipdb.db;Version=3;";
        }
        
        public void InsertData(string ipAddress,  string twolettercode, string threelettercode, string name)
        {
            int Id = TwoLetterCodeExists(twolettercode);
            if (Id > 0)//if country exists
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();


                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = "INSERT INTO IPAddresses (Id, CountryId, IP, CreatedAt, UpdatedAt) VALUES (@Id, @CountryId, @IpAddress, @CreatedAt, @UpdatedAt)";

                        command.Parameters.AddWithValue("@IpAddress", ipAddress);
                        command.Parameters.AddWithValue("@CountryId", Id);//TwoLetterCodeExists(twolettercode)==GetCountryId
                        command.Parameters.AddWithValue("@Id", GetNewIpId());
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                        command.ExecuteNonQuery();

                    }
                }
            }
            else//need to add country
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(connection))
                    {
                        int new_country_id = GetNewCId();
                        command.CommandType = CommandType.Text;
                        command.CommandText = "INSERT INTO IPAddresses (Id, CountryId, IP, CreatedAt, UpdatedAt) VALUES (@Id, @CountryId, @IpAddress, @CreatedAt, @UpdatedAt)";

                        command.Parameters.AddWithValue("@Id", GetNewIpId());
                        command.Parameters.AddWithValue("@CountryId", new_country_id);
                        command.Parameters.AddWithValue("@IpAddress", ipAddress);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                        command.ExecuteNonQuery();

                        command.CommandType = CommandType.Text;
                        command.CommandText = "INSERT INTO Countries (Id, Name, TwoLetterCode, ThreeLetterCode, CreatedAt) VALUES (@Id, @Name, @TwoLetterCode, @ThreeLetterCode, @CreatedAt)";
                        command.Parameters.AddWithValue("@Id", new_country_id);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@TwoLetterCode", twolettercode);
                        command.Parameters.AddWithValue("@ThreeLetterCode", threelettercode);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        command.ExecuteNonQuery();

                    }
                }
            }
        }

    }
}
