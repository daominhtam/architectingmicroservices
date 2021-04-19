using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ApiGateway.API.Dtos.Catalog
{
    [DataContract]
    public class MusicDto
    {
        [DataMember]
        [Required(ErrorMessage = "Id is required")]
        public int Id { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [DataMember] public string ProductArtUrl { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Parental Caution Flag is required")]
        public bool ParentalCaution { get; set; }

        [DataMember]
        [Required(ErrorMessage = "UPC is required")]
        public string Upc { get; set; }

        [DataMember] public DateTime? ReleaseDate { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Price is required")]
        [Range(0.1, 4.99, ErrorMessage = "Price Must be Greater than $0.10, but less than $4.99")]
        public decimal Price { get; set; }

        [DataMember] public int Available { get; set; }

        [DataMember] public bool? Cutout { get; set; }

        [DataMember] public string ArtistName { get; set; }

        [DataMember] public string GenreName { get; set; }

        [DataMember]
        [Required(ErrorMessage = "ArtistId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ArtistId must be greater than 0")]
        public int ArtistId { get; set; }

        [DataMember]
        [Required(ErrorMessage = "GenereId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be greater than 0")]
        public int GenreId { get; set; }
    }
}