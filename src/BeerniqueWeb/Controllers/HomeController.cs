using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BeerniqueWeb.Session;
using BeerniqueWeb.Helpers;
using System.Json;

namespace BeerniqueWeb.Controllers
{
	public class HomeController : CouchbaseControllerBase
	{
		//
		// GET: /Home/

		public ActionResult Index()
		{
			string userBeers = null;
			var breweries = new Dictionary<string, int>();
			var uniqueBeers = new List<string>();
			var beers = new List<JsonValue>();
			var userBeerCounts = new Dictionary<string, int>();

			if (!string.IsNullOrEmpty(SessionUser.Current.Email) &&
				(userBeers = _Client.Get<string>(HashHelper.Hash(SessionUser.Current.Email))) != null)
			{
				var userBeersKeys = userBeers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				if (userBeersKeys.Length > 0)
				{

					foreach (var beerId in userBeersKeys)
					{
						userBeerCounts[beerId] = userBeersKeys.Where(k => k == beerId).Count();
						var beer = JsonObject.Parse(_Client.Get<string>(beerId));

						// if a non-existent beer was accidently added to the users doc, skip it
						if (beer == null)
						{
							continue;
						}

						// add to the brewery counter for "mostly_by" list
						if (!breweries.ContainsKey(beer["brewery"].ReadAs<string>()))
						{
							breweries[beer["brewery"].ReadAs<string>()] = 1;
						}
						else
						{
							breweries[beer["brewery"].ReadAs<string>()] = ++breweries[beer["brewery"].ReadAs<string>()];
						}

						// if we already have the beer in the list, though, let's skip it
						if (uniqueBeers.Contains(escapeBeerId(beerId)))
						{
							continue;
						}

						beer["beer_url"] = Url.Action("Details", "Beers", new { id = escapeBeerId(beerId) });
						beer["brewery_url"] = "breweries/";// . str_replace(' ', '_', $beer->brewery);
						beer["drank_times"] = userBeerCounts[beerId];
						beers.Add(beer);
						uniqueBeers.Add(escapeBeerId(beerId));
					}

				}

			}
			ViewBag.HasBeers = uniqueBeers.Count > 0;
			ViewBag.OnIndex = true;
			ViewBag.Beers = beers;
			ViewBag.MostlyDrink = escapeBeerId(userBeerCounts.OrderByDescending(k => k.Value).FirstOrDefault().Key);
			ViewBag.MostlyBy = breweries.OrderByDescending(k => k.Value).FirstOrDefault().Key;			
			return View();
		}

		private string escapeBeerId(string beerId)
		{
			return beerId.Replace("beer_", "").Replace("_", " ");
		}
		
	}
}
