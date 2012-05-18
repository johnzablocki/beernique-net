using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase;
using Couchbase.Configuration;
using System.IO;
using System.Json;
using System.Text.RegularExpressions;
using Enyim.Caching.Memcached;

namespace BeerImporter
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var config = new CouchbaseClientConfiguration();
				config.Urls.Add(new Uri("http://localhost:8091/pools/default"));
				config.Bucket = "beernique";
				config.BucketPassword = "b33rs";

				var client = new CouchbaseClient(config);

				var root = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\beer-sample");
				import(client, Path.Combine(root, "breweries"));
				import(client, Path.Combine(root, "beer"));				

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private static void import(CouchbaseClient client, string directory)
		{
			var dir = new DirectoryInfo(directory);
			foreach (var file in dir.GetFiles())
			{
				if (file.Extension != ".json") continue;
				Console.WriteLine("Adding {0}", file);

				var json = File.ReadAllText(file.FullName);
				var key = file.Name.Replace(file.Extension, "");
				json = Regex.Replace(json.Replace(key, "LAZY"), "\"_id\":\"LAZY\",","");
				var storeResult = client.ExecuteStore(StoreMode.Set, key, json);
				Console.WriteLine(storeResult.Message);
			}
						
		}
	}

}
