using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.API.Contracts;
using Catalog.API.Domain.Entities;
using Catalog.API.Events;
using EventBus.Bus;
using Microsoft.Extensions.Logging;

namespace Catalog.API.Domain.BusinessServices
{
    public class CatalogBusinessServices : ICatalogBusinessServices
    {
        private readonly IArtistRepository _artistRepository;
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly IGenreRepository _genreRepository;
        private readonly ILogger<CatalogBusinessServices> _logger;
        private readonly IMusicRepository _musicRepository;

        public CatalogBusinessServices(IMusicRepository musicRepository,
            IGenreRepository genreRepository,
            IArtistRepository artistRepository,
            IEventBusPublisher eventBusPublisher,
            ILogger<CatalogBusinessServices> logger)
        {
            _musicRepository = musicRepository;
            _genreRepository = genreRepository;
            _artistRepository = artistRepository;
            _eventBusPublisher = eventBusPublisher;
            _logger = logger;
        }

        public async Task<List<Product>> GetAllMusic(string correlationToken)
        {
            return await _musicRepository.GetAll(correlationToken);
        }

        public async Task<Product> GetMusic(string correlationToken, int albumId)
        {
            return await _musicRepository.GetById(albumId, correlationToken);
        }

        public async Task<List<Product>> GetTopSellingMusic(string correlationToken, int count)
        {
            return await _musicRepository.GetTopSellers(count, correlationToken);
        }

        public async Task<List<Genre>> GetAllGenres(string correlationToken, bool includeAlbums = false)
        {
            return includeAlbums
                ? await _genreRepository.GetAllAndAlbums(correlationToken)
                : await _genreRepository.GetAll(correlationToken);
        }

        public async Task<Genre> GetGenre(int genreId, string correlationToken, bool includeAlbums = false)
        {
            return await _genreRepository.GetById(genreId, correlationToken, includeAlbums);
        }

        public async Task<List<Artist>> GetAllArtists(string correlationToken)
        {
            return await _artistRepository.GetAll(correlationToken);
        }

        public async Task<Artist> GetArtist(int artistID, string correlationToken)
        {
            return await _artistRepository.GetById(artistID, correlationToken);
        }

        public async Task Add(string correlationToken, Product product)
        {
            // Idempotent write check. Ensure insert with same correlation token has
            // not already happened. This would most likely do to a retry after the
            // product has been added.
            var targetAlbum = await _musicRepository.GetByIdWithIdempotencyCheck(product.Id, correlationToken);

            if (targetAlbum == null)
            {
                // Product has not been added yet
                await _musicRepository.Add(product);

                // Hack: Yet another transformation of same data.
                //       Added to remove issue in new Core Serializer which doesn't allow circular references.
                var productUpsert = new ProductUpsert
                {
                    Id = product.Id,
                    ArtistId = product.ArtistId,
                    GenreId = product.GenreId,
                    Title = product.Title,
                    ParentalCaution = product.ParentalCaution,
                    Cutout = product.Cutout,
                    Price = product.Price,
                    ReleaseDate = product.ReleaseDate,
                    Upc = product.Upc
                };

                await _eventBusPublisher.Publish<ProductChangedEvent>(
                    await PrepareProductChangedEvent(productUpsert, correlationToken));

                //product = await _musicRepository.GetById(product.Id, correlationToken);
            }
        }

        public async Task Update(string correlationToken, Product product)
        {
            await _musicRepository.Update(product);

            // Hack: Yet another transformation of same data.
            //       Added to remove issue in new Core Serializer which doesn't allow circular references.
            var productUpsert = new ProductUpsert
            {
                Id = product.Id,
                ArtistId = product.ArtistId,
                GenreId = product.GenreId,
                Title = product.Title,
                ParentalCaution = product.ParentalCaution,
                Cutout = product.Cutout,
                Price = product.Price,
                ReleaseDate = product.ReleaseDate,
                Upc = product.Upc
            };

            //************** Publish Event  *************************
            await _eventBusPublisher.Publish<ProductChangedEvent>(
                await PrepareProductChangedEvent(productUpsert, correlationToken));
        }

        public async Task ManageReadModel(string correlationToken)
        {
            var productId = 0;
            var productTitle = string.Empty;
            var counter = 0;

            try
            {
                var products = await _musicRepository.GetAll(correlationToken);

                if (products.Count < 1) throw new Exception("Error in UpPropagateReadModel -- No products exist");

                foreach (var product in products)
                {
                    productId = product.Id;
                    productTitle = product.Title;
                    counter++;

                    var productChangedEvent = new ProductChangedEvent
                    {
                        Id = product.Id,
                        Title = product.Title,
                        ArtistName = product.Artist.Name,
                        GenreName = product.Genre.Name,
                        ParentalCaution = product.ParentalCaution,
                        Cutout = product.Cutout,
                        Price = product.Price,
                        ReleaseDate = product.ReleaseDate,
                        Upc = product.Upc,
                        CorrelationToken = correlationToken
                    };

                    productChangedEvent.CorrelationToken = correlationToken;

                    _logger.LogInformation(
                        $"Adding product #{counter} to topic with {productId} and title {product.Title} with CorrelationToken {correlationToken}");

                    await _eventBusPublisher.Publish<ProductChangedEvent>(productChangedEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(
                    $"Exception adding product #{counter} topic queue with {productId} and title {productTitle} with CorrelationToken {correlationToken}");

                throw new Exception(
                    $"Could not propegate product read model - exception thrown in PropegateReadModel in CatalogBusinessService - Message : {ex.Message}");
            }
        }

        private async Task<ProductChangedEvent> PrepareProductChangedEvent(ProductUpsert productUpsert,
            string correlationToken)
        {
            // Perform Lookup to get Genre and Artist Names
            var artistName = (await _artistRepository.GetById(productUpsert.ArtistId, correlationToken)).Name;
            var genreName = (await _genreRepository.GetById(productUpsert.GenreId, correlationToken)).Name;

            //// Provide fallback logic in the event we cannot fetch Artist or Genre name
            genreName ??= "Unknown Genre";
            artistName ??= "Unknown Artist";

            // Populate data in the event object
            var productChangedEvent = new ProductChangedEvent
            {
                Id = productUpsert.Id,
                Title = productUpsert.Title,
                ArtistName = artistName,
                GenreName = genreName,
                Price = productUpsert.Price,
                ReleaseDate = productUpsert.ReleaseDate ?? DateTime.UtcNow.Date,
                ParentalCaution = productUpsert.ParentalCaution,
                Upc = productUpsert.Upc,
                Cutout = productUpsert.Cutout
            };

            productChangedEvent.CorrelationToken = correlationToken;

            return productChangedEvent;
        }
    }
}