using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PerToDo
{
	public class ToDoRepository
	{
		readonly IToDoService todoService;

		public Task<List<ToDoItem>> GetToDoList()
		{
			return todoService.GetToDoList();
		}

		public ToDoRepository(IToDoService todoService)
		{
			this.todoService = todoService;
		}

		public Task<HttpStatusCode> DeleteToDoItem(int itemId)
		{
			return todoService.DeleteToDoItem(itemId);
		}

		public Task<Uri> AddNewToDoItem(ToDoItem item)
		{
			return todoService.AddNewToDoItem(item);
		}

		public Task<ToDoItem> UpdateToDoItem(int id, ToDoItem item)
		{
			return todoService.UpdateToDoItem(id, item);
		}
	}
}
