using novibet.models;
using novibet.models.dto;
using System;
using System.Data;
using System.Data.SQLite;

namespace novibet.Datavase
{
    public class DbRead
    {
        private string _connectionString;

        public DbRead(string connectionString)
        {
            //_connectionString = "Data Source=C:\\Users\\james\\Desktop\\novibet\\ipdb.db;Version=3;";//input string here!!!sos may differ from compurt to computer
            _connectionString = @"Data Source=..\ipdb.db;Version=3;";
        }

        public DataDTO ReadDataFromDatabase(string ipAddress)
        {
            DataDTO myData = new DataDTO();

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Create an SQL command to retrieve data from the database
                using (SQLiteCommand command = new SQLiteCommand("SELECT IP, TwoletterCode, ThreeletterCode, Name\r\nFROM Countries b\r\nJOIN IPAddresses a ON a.CountryId = b.Id\r\nWHERE a.IP = @ipAddress;", connection))
                {
                    command.Parameters.AddWithValue("@ipAddress", ipAddress);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Retrieve values from the database 
                            myData.CountryName = reader["Name"].ToString();
                            myData.TwoLetterCode = reader["TwoLetterCode"].ToString();
                            myData.ThreeLetterCode = reader["ThreeLetterCode"].ToString();
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            return myData;
        }
    }
}
