using novibet.models;
using System.Data;
using System.Data.SQLite;

namespace novibet.Datavase
{
    public class retrieve_ip_num
    {
        private string _connectionString;

        public retrieve_ip_num(string connectionString)
        {
            _connectionString = @"Data Source=..\ipdb.db;Version=3;";
        }


        public DateTime return_last_update(string twolettercode)
        {
            DateTime _last=DateTime.Now;
            _connectionString = @"Data Source=..\ipdb.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"SELECT MAX(UpdatedAt) FROM IPAddresses a, Countries b WHERE a.CountryId= (SELECT b.Id WHERE TwoLetterCode = @twolettercode);";
                    command.Parameters.AddWithValue("@twolettercode", twolettercode);
                    _last = Convert.ToDateTime(command.ExecuteScalar());




                }
                return _last;
            }
        
        }
        


        public Data Return_IPs(string twolettercode)
        {

            Data myData = new Data();
            myData.Country = twolettercode;

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"SELECT DISTINCT COUNT(IP) FROM IPAddresses a, Countries b WHERE a.CountryId= (SELECT b.Id WHERE TwoLetterCode = @twolettercode);";
                    command.Parameters.AddWithValue("@twolettercode", twolettercode);
                    int ips_num = Convert.ToInt32(command.ExecuteScalar());
                    myData.Ips_found = ips_num;

                    if (ips_num == 0)
                    {
                        myData.Ips_List.Add("Has no Ip registered");
                        myData.last_upadte= new  DateTime(2000, 1, 1, 12, 0, 0);
                    }
                    else
                    {

                        command.CommandType = CommandType.Text;
                        command.CommandText = $"SELECT IP FROM IPAddresses WHERE CountryId =  (SELECT Id FROM Countries WHERE TwoLetterCode = @twolettercode)";
                        command.Parameters.AddWithValue("@twolettercode", twolettercode);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string Ip = reader["IP"].ToString();
                                myData.Ips_List.Add(Ip);
                            }
                        }
                        myData.last_upadte = return_last_update(twolettercode);
                    }
                }
            }

            return myData;
        }
    }
}
