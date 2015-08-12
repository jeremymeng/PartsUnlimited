﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using PartsUnlimited.Queries;
using PartsUnlimited.Telemetry;
using PartsUnlimited.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PartsUnlimited.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        [FromServices]
        public IOrdersQuery OrdersQuery { get; set; }

        [FromServices]
        public ITelemetryProvider Telemetry { get; set; }

        public async Task<IActionResult> Index(DateTime? start, DateTime? end, string invalidOrderSearch)
        {
            var username = User.GetUserName();

            return View(await OrdersQuery.IndexHelperAsync(username, start, end, 10, invalidOrderSearch, false));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                Telemetry.TrackTrace("Order/Server/NullId");
                return RedirectToAction("Index", new { invalidOrderSearch = Request.Query["id"] });
            }

            var order = await OrdersQuery.FindOrderAsync(id.Value);
            var username = User.GetUserName();

            // If the username isn't the same as the logged in user, return as if the order does not exist
            if (order == null || !String.Equals(order.Username, username, StringComparison.Ordinal))
            {
                Telemetry.TrackTrace("Order/Server/UsernameMismatch");
                return RedirectToAction("Index", new { invalidOrderSearch = id.ToString() });
            }

            // Capture order review event for analysis
            var eventProperties = new Dictionary<string, string>()
                {
                    {"Id", id.ToString() },
                    {"Username", username }
                };
            if (order.OrderDetails == null)
            {
                Telemetry.TrackEvent("Order/Server/NullDetails", eventProperties, null);
            }
            else
            {
                var eventMeasurements = new Dictionary<string, double>()
                {
                    {"LineItemCount", order.OrderDetails.Count }
                };
                Telemetry.TrackEvent("Order/Server/Details", eventProperties, eventMeasurements);
            }

            var itemsCount = order.OrderDetails.Sum(x => x.Quantity);
            var subTotal = order.OrderDetails.Sum(x => x.Quantity * x.Product.Price);
            var shipping = itemsCount * (decimal)5.00;
            var tax = (subTotal + shipping) * (decimal)0.05;
            var total = subTotal + shipping + tax;

            var costSummary = new OrderCostSummary
            {
                CartSubTotal = subTotal.ToString("C"),
                CartShipping = shipping.ToString("C"),
                CartTax = tax.ToString("C"),
                CartTotal = total.ToString("C")
            };

            var viewModel = new OrderDetailsViewModel
            {
                OrderCostSummary = costSummary,
                Order = order
            };

            return View(viewModel);
        }
    }
}
