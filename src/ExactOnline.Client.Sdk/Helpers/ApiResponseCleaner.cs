﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using ExactOnline.Client.Sdk.Exceptions;

namespace ExactOnline.Client.Sdk.Helpers
{
	/// <summary>
	/// Class for stripping unnecessary Json tags from API Response
	/// </summary>
	public class ApiResponseCleaner
	{
		#region Public methods

		/// <summary>
		/// Fetch Json Object (Json within ['d'] name/value pair) from response
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public static string GetJsonObject(string response)
		{
			var serializer = new JavaScriptSerializer();
			serializer.RegisterConverters(new JavaScriptConverter[] { new JssDateTimeConverter() });
			var dict = (Dictionary<string, object>)serializer.Deserialize<object>(response);

			var d = (Dictionary<string, object>)dict["d"];
			string output = GetJsonFromDictionary(d);
			return output;
		}

		/// <summary>
		/// Fetch Json Array (Json within ['d']['results']) from response
		/// </summary>
		public static string GetJsonArray(string response)
		{
			var serializer = new JavaScriptSerializer();
			serializer.RegisterConverters(new JavaScriptConverter[] { new JssDateTimeConverter() });
			try
			{
				ArrayList results;
				var dict = (Dictionary<string, object>)serializer.Deserialize<object>(response);
				var innerPart = dict["d"];
				if (innerPart.GetType() == typeof(Dictionary<string, object>))
				{
					var d = (Dictionary<string, object>)dict["d"];
					results = (ArrayList)d["results"];
				}
				else
				{
					results = (ArrayList)innerPart;
				}
				return GetJsonFromResultDictionary(results);
			}
			catch (Exception e)
			{
				throw new IncorrectJsonException(e.Message);
			}

		}

		#endregion

		#region Private methods

		/// <summary>
		/// Converts key/value pairs to json
		/// </summary>
		private static string GetJsonFromDictionary(Dictionary<string, object> dictionary)
		{
			string json = "{";

			foreach (var entry in dictionary)
			{
				if (entry.Value == null || entry.Value.GetType() != typeof(Dictionary<string, object>))
				{
					// Entry is of type keyvaluepair
					json += "\"" + entry.Key + "\":";
					if (entry.Value == null)
					{
						json += "null";
					}
					else
					{
						json += "\"" + entry.Value.ToString().Replace("\"", "\\" + "\"") + "\"";
					}
					json += ",";
				}
				else
				{
					// Create linked entities json
					var subcollection = (Dictionary<string, object>)entry.Value;
					if (subcollection.Keys.Contains("results"))
					{
						var results = (ArrayList)subcollection["results"];
						string subjson = GetJsonFromResultDictionary(results);
						if (subjson.Length > 0)
						{
							json += "\"" + entry.Key + "\":";
							json += subjson;
							json += ",";
						}
					}
				}
			}

			json = json.Remove(json.Length - 1, 1); // Remove last comma
			json += "}";

			return json;
		}

		private static string GetJsonFromResultDictionary(ArrayList results)
		{
			string json = "[";
			if (results != null && results.Count > 0)
			{
				foreach (Dictionary<string, object> entity in results)
				{
					json += GetJsonFromDictionary(entity) + ",";
				}

				json = json.Remove(json.Length - 1, 1); // Remove last comma
			}
			json += "]";
			return json;
		}

		#endregion

	}
}
