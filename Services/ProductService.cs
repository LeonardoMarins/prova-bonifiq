using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
	public class ProductService
	{
		TestDbContext _ctx;
		PaginatorService _paginator;

		public ProductService(TestDbContext ctx, PaginatorService paginator)
		{
			_ctx = ctx;
			_paginator = paginator;
		}

		public Task<PaginatedResult<Product>>  ListProducts(int page)
		{
			var query = _ctx.Products.OrderBy(p => p.Id).AsQueryable();
			return _paginator.PaginateAsync(query, page, 10);
		}

	}
}
