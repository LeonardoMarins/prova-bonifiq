﻿using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Provider;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public class CustomerService
    {
        TestDbContext _ctx;
        PaginatorService _paginator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CustomerService(TestDbContext ctx, PaginatorService paginator, IDateTimeProvider dateTimeProvider)
        {
            _ctx = ctx;
            _paginator = paginator;
            _dateTimeProvider = dateTimeProvider;
        }

        public Task<PaginatedResult<Customer>> ListCustomers(int page)
        {
            var query = _ctx.Customers.OrderBy(p => p.Id).AsQueryable();
            return _paginator.PaginateAsync(query, page, 10);
        }

        public async Task<bool> CanPurchase(int customerId, decimal purchaseValue)
        {
            if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId));

            if (purchaseValue <= 0) throw new ArgumentOutOfRangeException(nameof(purchaseValue));

            //Business Rule: Non registered Customers cannot purchase
            var customer = await _ctx.Customers.FindAsync(customerId);
            if (customer == null) throw new InvalidOperationException($"Customer Id {customerId} does not exists");

            //Business Rule: A customer can purchase only a single time per month
            var baseDate = _dateTimeProvider.UtcNow.AddMonths(-1);
            var ordersInThisMonth = await _ctx.Orders.CountAsync(s => s.CustomerId == customerId && s.OrderDate >= baseDate);
            if (ordersInThisMonth > 0)
                return false;

            //Business Rule: A customer that never bought before can make a first purchase of maximum 100,00
            var haveBoughtBefore = await _ctx.Customers.CountAsync(s => s.Id == customerId && s.Orders.Any());
            if (haveBoughtBefore == 0 && purchaseValue > 100)
                return false;

            //Business Rule: A customer can purchases only during business hours and working days
            var now = _dateTimeProvider.UtcNow;
            if (now.Hour < 8 || now.Hour > 18 || now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                return false;


            return true;
        }

    }
}
