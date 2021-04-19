using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiGateway.API.Dtos.Catalog
{
    [DataContract]
    public class GenreDto
    {
        [DataMember] public int GenreId { get; set; }

        [DataMember] public string Name { get; set; }

        [DataMember] public string Description { get; set; }

        [DataMember] public IEnumerable<MusicDto> Albums { get; set; }
    }
}