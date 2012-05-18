using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BeerniqueWeb.Helpers;
using BeerniqueWeb.Session;
using Enyim.Caching.Memcached;
using System.Text;
using System.Json;

namespace BeerniqueWeb.Controllers
{
    public class BeersController : CouchbaseControllerBase
    {
		[HttpGet]
		public ActionResult Details(string id)
		{
			var beerId = "beer_" + Server.UrlDecode(id).Replace(" ", "_");
			var beer = JsonObject.Parse(_Client.Get<string>(beerId));
			if (beer != null)
			{
			  if (beer.ContainsKey("brewery")) {
				beer["brewery_url"] = beer["brewery"].ReadAs<string>().Replace(" ", "_");
			  }
			  //$app->view()->appendData((array)$beer);
			  //$content = $app->view()->render('beer.mustache');
			  //$app->render('layout.mustache', compact('content'));
			  return View(beer);
			} 
			else 
			{
			  return HttpNotFound();
			}
		}

        //
        // GET: /Beers/
		[HttpPost]
        public ActionResult Details(FormCollection coll)
        {
			var beerId = "beer_" + Server.UrlDecode(coll["id"]).Replace(" ", "_");
			if (_Client.Get(beerId) == null)
			{
				return HttpNotFound();
			}

			var hashedEmail = HashHelper.Hash(SessionUser.Current.Email);
			if (_Client.Get(hashedEmail) != null) 
			{
				var data = new ArraySegment<byte>(Encoding.Default.GetBytes('|' + beerId));
				_Client.Append(hashedEmail, data);
			} 
			else 
			{
				_Client.Store(StoreMode.Set, hashedEmail, beerId);
			}				

            return RedirectToAction("Details", new { id = coll["id"] });
        }
				
    }
}
