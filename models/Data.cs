namespace novibet.models
{
    public class Data
    {
        public string Country { get; set; }
        public int Ips_found { get; set; }// 2lettercode

        public List<string> Ips_List { get; set; } = new List<string>();

        public DateTime last_upadte { get; set; }

    }



    public class Data_correct_response
    {
        public string Country { get; set; }
        public int Ips_found { get; set; }// 2lettercode

        //public List<string> Ips_List { get; set; } = new List<string>();

        public DateTime last_upadte { get; set; }

    }
}



