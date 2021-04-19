using System;
using System.Runtime.Serialization;

namespace ApiGateway.API.Dtos.Catalog
{
    public class Product
    {
        [DataMember] public int Id { get; set; }

        [DataMember] public string Title { get; set; }

        [DataMember] public string AlbumArtUrl { get; set; }

        [DataMember] public bool ParentalCaution { get; set; }

        [DataMember] public string Upc { get; set; }

        [DataMember] public DateTime? ReleaseDate { get; set; }

        [DataMember] public decimal Price { get; set; }

        [DataMember] public int AvailableStock { get; set; }

        [DataMember] public string ArtistName { get; set; }

        [DataMember] public string GenreName { get; set; }
    }
}