using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
	public class OrderService
	{
        TestDbContext _ctx;

        public OrderService(TestDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Order> PayOrder(IPaymentMethod paymentMethod, decimal paymentValue, int customerId)
		{
			var order = new Order
			{
				Value = paymentValue,
				CustomerId = customerId,
				OrderDate = DateTime.UtcNow // salva em UTC
			};
			paymentMethod.Pay(paymentValue);

			return await InsertOrder(order); //Retorna o pedido para o controller
		}

		public async Task<Order> InsertOrder(Order order)
        {
			//Insere pedido no banco de dados
			return (await _ctx.Orders.AddAsync(order)).Entity;
        }
	}
}
