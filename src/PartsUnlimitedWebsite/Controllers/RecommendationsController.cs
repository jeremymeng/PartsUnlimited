// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using PartsUnlimited.Models;
using PartsUnlimited.Recommendations;
using PartsUnlimited.WebsiteConfiguration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PartsUnlimited.Controllers
{
    public class RecommendationsController : Controller
    {
        [FromServices]
        public IPartsUnlimitedContext DbContext { get; set; }

        [FromServices]
        public IRecommendationEngine Recommendation { get; set; }

        [FromServices]
        public IWebsiteOptions Option { get; set; }

        public async Task<IActionResult> GetRecommendations(string recommendationId)
        {
            if (!Option.ShowRecommendations)
            {
                return new EmptyResult();
            }

            var recommendedProductIds = await Recommendation.GetRecommendationsAsync(recommendationId);

            var productTasks = recommendedProductIds
                .Select(item => DbContext.Products.SingleOrDefaultAsync(c => c.RecommendationId == Convert.ToInt32(item)))
                .ToList();

            await Task.WhenAll(productTasks);

            var recommendedProducts = productTasks
                .Select(p => p.Result)
                .Where(p => p != null && p.RecommendationId != Convert.ToInt32(recommendationId))
                .ToList();

            return PartialView("_Recommendations", recommendedProducts);
        }
    }
}