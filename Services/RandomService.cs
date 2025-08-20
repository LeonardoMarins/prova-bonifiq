using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
	public class RandomService
	{
		int seed;
        TestDbContext _ctx;
		public RandomService()
        {
            var contextOptions = new DbContextOptionsBuilder<TestDbContext>()
    .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Teste;Trusted_Connection=True;")
    .Options;
            seed = Guid.NewGuid().GetHashCode();

            _ctx = new TestDbContext(contextOptions);
        }
        public async Task<int> GetRandom()
        {
	        int number;
	        bool exists;

	        do
	        {
		        number = Random.Shared.Next(100);
		        exists = await _ctx.Numbers.AnyAsync(x => x.Number == number);
	        }
	        while (exists);

	        _ctx.Numbers.Add(new RandomNumber { Number = number });
	        await _ctx.SaveChangesAsync();

	        return number;
		}

	}
}
