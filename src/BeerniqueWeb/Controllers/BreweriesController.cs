using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Json;

namespace BeerniqueWeb.Controllers
{
	public class BreweriesController : CouchbaseControllerBase
	{
		//
		// GET: /Breweries/

		public ActionResult Details(string id)
		{
			var breweryId = "brewery_" + Server.UrlDecode(id).Replace(" ", "");
			var brewery = JsonObject.Parse(_Client.Get<string>(breweryId));
			if (brewery != null)
			{
				//if (brewery.ContainsKey("geo")) // && isset($brewery->geo->loc)
				//    //&& is_array($brewery->geo->loc)
				//&& count($brewery->geo->loc) > 0) {
				//$brewery->geo->latitude = $brewery->geo->loc[0];
				//$brewery->geo->longitude = $brewery->geo->loc[1];
				//} else {
				//  unset($brewery->geo);
				//}

				return View(brewery);
			}
			else
			{
				return new HttpNotFoundResult();
			}
		}

	}
}
