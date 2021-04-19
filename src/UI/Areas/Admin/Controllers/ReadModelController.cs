using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicStore.Helper;
using MusicStore.Models;

namespace MusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReadModelController : Controller
    {
        private const string baseUrl = "api/CatalogGateway";
        private readonly IRestClient _IRestClient;
        private readonly ILogger<ReadModelController> _logger;

        public ReadModelController(IRestClient iuiRestClient, ILogger<ReadModelController> logger)
        {
            _IRestClient = iuiRestClient;
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: ReadModelController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            PropagateReadModel model;

            var basket = await _IRestClient.PostAsync<PropagateReadModel>($"{baseUrl}/Catalog/CreateBasketReadModel");

            _logger.LogInformation($"Catalog readmodel successfully propagated");

            return RedirectToAction("Index");
        }
    }
}
