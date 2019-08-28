using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;

        public SeedDb(DataContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckPropertyTypesAsync();
            await CheckOwnersAsync();
            await CheckLesseesAsync();

        }

        private async  Task CheckLesseesAsync()
        {
            if (!_context.Lessees.Any())
            {
                _context.Lessees.Add(new Entities.Lessee
                {
                    Address = "Santa Ana",
                    CellPhone="12345678",
                    Document="12345678",
                    FirstName="First Name Lesse",
                    LastName="LastName Leasse",
                    
                });

                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckOwnersAsync()
        {
            if (!_context.Owners.Any())
            {
                _context.Owners.Add(new Entities.Owner
                {
                    Address = "Santa Ana",
                    CellPhone = "12345678",
                    Document = "01919707-6",
                    FirstName = "First Name Owern",
                    LastName = "LastName Owner",
                    FixedPhone= "12345678",
            
                });

                await _context.SaveChangesAsync();

            }
        }

        private async  Task CheckPropertyTypesAsync()
        {
            if (!_context.PropertyTypes.Any())
            {

                _context.PropertyTypes.Add(new Entities.PropertyType
                {
                    Name = "Casa",

                });

                await _context.SaveChangesAsync();
            
            }
        }
    }
}
