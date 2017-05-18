using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace PerToDo
{
	public class ToDoService : IToDoService
	{
		//I recommend to use 
		//https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client

		HttpClient client;
		//IUserDetailsStore storeService;
		readonly string token;
		readonly Uri baseUri = new Uri(Constants.BaseURL);

		public ToDoService()
		{
			//storeService = DependencyService.Get<IUserDetailsStore>();
			token = AccountDetailsStore.Instance.Token;
			client = new HttpClient();
			client.MaxResponseContentBufferSize = 256000;
		}

		public async Task<HttpStatusCode> DeleteToDoItem(int id)
		{
			var uri = new Uri(baseUri, string.Format("api/todo/{0}", id));

			client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

			var response = await client.DeleteAsync(uri);

			return response.StatusCode;
		}

		public async Task<Uri> AddNewToDoItem(ToDoItem item)
		{
			var uri = new Uri(baseUri, "api/todo/");

			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

			var json = JsonConvert.SerializeObject(item);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await client.PostAsync(uri, content);

			response.EnsureSuccessStatusCode();

			// Return 	the URI of the created resource.
			return response.Headers.Location;
		}

		public async Task<ToDoItem> UpdateToDoItem(int id, ToDoItem item)
		{
			var uri = new Uri(baseUri, string.Format("api/todo/{0}", id));

			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

			var json = JsonConvert.SerializeObject(item);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await client.PutAsync(uri, content);

			// Deserialize the updated product from the response body.
			var responseContent = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<ToDoItem>(responseContent);
		}

		public async Task<List<ToDoItem>> GetToDoList()
		{
			var uri = new Uri(baseUri, "api/todo/");

			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

			var response = await client.GetAsync(uri);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<List<ToDoItem>>(content);
			}
			return new List<ToDoItem>();
		}
	}
}
