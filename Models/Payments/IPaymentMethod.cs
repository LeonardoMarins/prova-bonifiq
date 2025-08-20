namespace ProvaPub.Models;

public interface IPaymentMethod
{
    void Pay(decimal amount);
}