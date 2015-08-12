// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNet.Mvc;
using PartsUnlimited.Search;
using System.Threading.Tasks;

namespace PartsUnlimited.Controllers
{
    public class SearchController : Controller
    {
        [FromServices]
        public IProductSearch Search { get; set; }

        public async Task<IActionResult> Index(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return View(null);
            }

            var result = await Search.Search(q);

            return View(result);
        }
    }
}
