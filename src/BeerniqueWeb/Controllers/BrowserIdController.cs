using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammock;
using Hammock.Web;
using System.Text;
using System.Json;
using BeerniqueWeb.Session;
using BeerniqueWeb.Helpers;
using Enyim.Caching.Memcached;

namespace BeerniqueWeb.Controllers
{
	public class BrowserIdController : CouchbaseControllerBase
	{
		
		public ActionResult WhoAmI()
		{
			if (SessionUser.Current.Email != null)
			{
				return Json(SessionUser.Current.Email, JsonRequestBehavior.AllowGet);
			}

			return Json(null, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult LogOut()
		{
			SessionUser.Current.Email = null;
			return Content("OK");
		}

		//
		// GET: /BrowserId/
		[HttpPost]
		public ActionResult LogIn(string assertion)
		{
			var client = new RestClient()
			{
				Authority = "https://browserid.org"
			};

			var request = new RestRequest
			{
				Path = "/verify",
				Method = WebMethod.Post,
			};

			var audience = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port).Uri.ToString();
			var postData = string.Format("assertion={0}&audience={1}", assertion, audience);
			request.AddPostContent(Encoding.Default.GetBytes(postData));
			var response = client.Request(request);

			var json = JsonObject.Parse(response.Content);
			if (json["status"].ReadAs<string>() == "okay")
			{
				// This logs the user in if we have an account for that email address,
				// or creates it otherwise
				SessionUser.Current.Email = json["email"].ReadAs<string>();
				var hashedEmail = HashHelper.Hash(SessionUser.Current.Email);				 
				if (_Client.Get(hashedEmail) == null)
				{
					_Client.Store(StoreMode.Add, hashedEmail, string.Empty);
				}
				return Json(SessionUser.Current.Email);
			}
			else
			{
				var message = new KeyValuePair<string, JsonValue>("message", "Could not log you in");
				var status = new KeyValuePair<string, JsonValue>("status", false);
				return Json(new JsonObject(message, status));
			}
												
		}

		//  $msg = 'Could not log you in';
		//  $status = false;
		//  echo json_encode(array('message'=>$msg,'status'=>$status));
		//}

	}
}

#region [ License information          ]
/* ************************************************************
 * 
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2012 Couchbase, Inc.
 *    
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion