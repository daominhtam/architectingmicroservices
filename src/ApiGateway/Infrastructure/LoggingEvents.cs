namespace ApiGateway.API.Infrastructure
{
    public class LoggingEvents
    {
        // Catalog Services
        public const int GetAllMusic = 1000;
        public const int GetMusicById = 1001;
        public const int GetTopSellingMusic = 1002;
        public const int GetAllGenres = 1003;
        public const int GetGenreById = 1004;
        public const int GetAllArtists = 1005;
        public const int InsertMusic = 1006;
        public const int ModifyMusic = 1007;
        public const int GetMusic = 1008;
        public const int CatalogPost = 1009;
        public const int CatalogPut = 1010;
        public const int CatalogPostReadModel = 1010;

        public const int GetAllBaskets = 2000;
        public const int GetBasket = 2001;
        public const int AddItemToBasket = 2002;
        public const int Checkout = 2003;
        public const int DeleteLineItem = 2004;
        public const int Delete = 2005;
        
        public const int GetOrder = 3000;
        public const int GetOrders = 3001;


    }
}