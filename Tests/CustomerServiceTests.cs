using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;
using ProvaPub.Services;
using NSubstitute;
using ProvaPub.Provider;

namespace ProvaPub.Tests
{
    public class CustomerServiceTests
    {
        private TestDbContext _context;
        private PaginatorService _paginator;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new TestDbContext(options);
            _paginator = new PaginatorService();
        }

        [Test]
        public void CanPurchase_InvalidCustomerId_Throws()
        {
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            var service = new CustomerService(_context, _paginator, dateTimeProvider);

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CanPurchase(0, 10));
        }

        [Test]
        public void CanPurchase_InvalidPurchaseValue_Throws()
        {
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            var service = new CustomerService(_context, _paginator, dateTimeProvider);

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CanPurchase(1, 0));
        }

        [Test]
        public void CanPurchase_NonRegisteredCustomer_Throws()
        {
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            var service = new CustomerService(_context, _paginator, dateTimeProvider);

            Assert.ThrowsAsync<InvalidOperationException>(() => service.CanPurchase(999, 10));
        }

        [Test]
        public async Task CanPurchase_AlreadyPurchasedThisMonth_ReturnsFalse()
        {
            var customer = new Customer { Id = 1, Name = "Cliente Teste" };
            _context.Customers.Add(customer);
            _context.Orders.Add(new Order { Id = 1, CustomerId = 1, OrderDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
            var service = new CustomerService(_context, _paginator, dateTimeProvider);

            var result = await service.CanPurchase(1, 50);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CanPurchase_FirstPurchaseAbove100_ReturnsFalse()
        {
            var customer = new Customer { Id = 1, Name = "Cliente Teste" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
            var service = new CustomerService(_context, _paginator, dateTimeProvider);

            var result = await service.CanPurchase(1, 150);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CanPurchase_OutsideBusinessHours_ReturnsFalse()
        {
            var customer = new Customer { Id = 1, Name = "Cliente Teste" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Mock do horário: domingo 10h
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.UtcNow.Returns(new DateTime(2025, 8, 24, 10, 0, 0)); // domingo

            var service = new CustomerService(_context, _paginator, dateTimeProvider);

            var result = await service.CanPurchase(1, 50);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CanPurchase_ValidPurchase_ReturnsTrue()
        {
            var customer = new Customer { Id = 1, Name = "Cliente Teste" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Mock do horário: terça-feira 10h (dentro do expediente)
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.UtcNow.Returns(new DateTime(2025, 8, 20, 10, 0, 0)); // terça

            var service = new CustomerService(_context, _paginator, dateTimeProvider);

            var result = await service.CanPurchase(1, 50);

            Assert.That(result, Is.True);
        }
    }
}
