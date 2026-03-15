namespace B3ly.DAL.Models
{
    public class Address
    {
        public int AddressId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        public AppUser User { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
