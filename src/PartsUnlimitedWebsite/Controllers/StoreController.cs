// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Caching.Memory;
using PartsUnlimited.Models;
using System;
using System.Linq;

namespace PartsUnlimited.Controllers
{
    public class StoreController : Controller
    {
        [FromServices]
        public IPartsUnlimitedContext DbContext { get; set; }

        [FromServices]
        public IMemoryCache Cache { get; set; }

        //
        // GET: /Store/

        public IActionResult Index()
        {
            var category = DbContext.Categories.ToList();

            return View(category);
        }

        //
        // GET: /Store/Browse?category=Brakes

        public IActionResult Browse(int categoryId)
        {
            // Retrieve Category category and its Associated associated Products products from database

            // TODO [EF] Swap to native support for loading related data when available
            var categoryModel = DbContext.Categories.Single(g => g.CategoryId == categoryId);
            categoryModel.Products = DbContext.Products.Where(a => a.CategoryId == categoryModel.CategoryId).ToList();

            return View(categoryModel);
        }

        public IActionResult Details(int id)
        {
            Product productData;

            if (!Cache.TryGetValue(string.Format("product_{0}", id), out productData))
            {
                productData = DbContext.Products.Single(a => a.ProductId == id);
                productData.Category = DbContext.Categories.Single(g => g.CategoryId == productData.CategoryId);

                if (productData != null)
                {
                    Cache.Set(string.Format("product_{0}", id), productData, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }                
            }

            return View(productData);
        }
    }
}