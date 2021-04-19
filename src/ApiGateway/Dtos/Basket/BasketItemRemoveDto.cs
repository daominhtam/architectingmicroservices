using System.Runtime.Serialization;

namespace ApiGateway.API.Dtos.Basket
{
    [DataContract]
    public class BasketItemRemoveDto
    {
        [DataMember] public string Message { get; set; }

        [DataMember] public decimal CartTotal { get; set; }

        [DataMember] public int CartCount { get; set; }

        [DataMember] public int ItemCount { get; set; }

        [DataMember] public int DeleteId { get; set; }
    }
}