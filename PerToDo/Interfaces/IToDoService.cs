using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PerToDo
{
	public interface IToDoService
	{
		Task<List<ToDoItem>> GetToDoList();

		Task<HttpStatusCode> DeleteToDoItem(int itemId);

		Task<Uri> AddNewToDoItem(ToDoItem item);

		Task<ToDoItem> UpdateToDoItem(int id, ToDoItem item);
	}
}
