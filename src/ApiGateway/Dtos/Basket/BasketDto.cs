using System.Collections.Generic;

namespace ApiGateway.API.Dtos.Basket
{
    public class BasketDto
    {
        public BasketDto()
        {
            CartItems = new List<BasketItemDto>();
        }

        public string BasketId { get; set; }
        public List<BasketItemDto> CartItems { get; set; }
        public decimal CartTotal { get; set; }
        public int ItemCount { get; set; }
    }
}