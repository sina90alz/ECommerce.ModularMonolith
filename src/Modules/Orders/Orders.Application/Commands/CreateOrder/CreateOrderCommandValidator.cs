using FluentValidation;
using Orders.Application.Commands.CreateOrder;
using Products.Contracts;

namespace Orders.Application.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator(IProductReadService productReadService)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");

        RuleFor(x => x)
            .MustAsync(async (command, cancellation) =>
            {
                var product = await productReadService.GetByIdAsync(command.ProductId);
                return product is not null;
            })
            .WithMessage("Product does not exist.");
    }
}
