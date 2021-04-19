using System;
using CommandBus.Commands;

namespace Catalog.API.Commands
{
    public class AddProductCommand : Command
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string GenreName { get; set; }
        public bool? Cutout { get; set; }
        public bool ParentalCaution { get; set; }
        public decimal Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string UPC { get; set; }
    }
}