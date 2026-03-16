namespace HolyWater.Server.Models
{
    public class userDTO
    {
        public string email { get; set; } 
        public string password { get; set; }
        public string username { get; set; }
        public int contactNumber { get; set; }
        public string dateOfBirth { get; set; }
        public string gender { get; set; }
        public AddressDTO address { get; set; }
    }

    public class AddressDTO
    {
        public string line1 { get; set; }
        public string? line2 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
    }
}
