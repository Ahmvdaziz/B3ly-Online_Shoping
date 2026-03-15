using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Data;
using B3ly.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace B3ly.BLL.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext _db;
        public AddressRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<AddressVM>> GetUserAddressesAsync(string userId) =>
            await _db.Addresses.Where(a => a.UserId == userId)
                .Select(a => new AddressVM
                {
                    AddressId = a.AddressId,
                    Country   = a.Country,
                    City      = a.City,
                    Street    = a.Street,
                    Zip       = a.Zip,
                    IsDefault = a.IsDefault
                }).ToListAsync();

        public async Task<Address?> GetByIdAsync(int id, string userId) =>
            await _db.Addresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);

        public async Task AddAsync(Address address)
        {
            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();
        }
    }
}
