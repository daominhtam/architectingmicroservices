using System;
using System.Threading.Tasks;
using Basket.API.Contracts;
using Basket.API.Domain;
using CommandBus.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Basket.API.Commands
{
    public class AddProductCommandHandler : ICommandHandler
    {
        private const string ProductPartitionKey = "MusicProduct";
        private readonly ILogger<AddProductCommandHandler> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IBasketBusinessServices _basketDomain;

        public AddProductCommandHandler(ILogger<AddProductCommandHandler> logger,
            IServiceProvider serviceProvider,
            IBasketBusinessServices basketDomain)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _basketDomain = basketDomain;
        }

        public async Task HandleAsync(Command command)
        {
            string correlationToken = null;

            try
            {
                // Downcast to concrete type from abstract type
                var product = command as AddProductCommand;

                correlationToken = command.CorrelationToken;
                
                await _basketDomain.BuildReadModel(product, correlationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Exception unpacking EmptyBasket Event in Eventhandler : {ex.Message} with correlation Token: {correlationToken}");
            }
        }
    }
}