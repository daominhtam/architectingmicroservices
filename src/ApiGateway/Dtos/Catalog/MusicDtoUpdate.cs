using System.Runtime.Serialization;

namespace ApiGateway.API.Dtos.Catalog
{
    [DataContract]
    public class MusicDtoUpdate
    {
        [DataMember] public int Id { get; set; }

        [DataMember] public string Title { get; set; }

        [DataMember] public string ProductArtUrl { get; set; }

        [DataMember] public bool ParentalCaution { get; set; }

        [DataMember] public string Upc { get; set; }

        [DataMember] public decimal Price { get; set; }

        [DataMember] public int Available { get; set; }

        [DataMember] public bool? Cutout { get; set; }

        [DataMember] public int ArtistId { get; set; }

        [DataMember] public int GenreId { get; set; }
    }
}