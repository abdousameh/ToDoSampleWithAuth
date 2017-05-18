using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace PerToDo
{
	public partial class ItemDetailsPage : ContentPage
	{
		ToDoItem toDoItem;
		ToDoRepository toDoRepo = new ToDoRepository(new ToDoService());

		public ItemDetailsPage(ToDoItem item)
		{
			InitializeComponent();
			this.toDoItem = item;
			Title = "Details";
		}

		async void SaveItemButton_Clicked(object sender, System.EventArgs e)
		{
			toDoItem.Name = nameEntry.Text.Trim();
			toDoItem.Notes = notesEditor.Text.Trim();
			toDoItem.Done = doneSwitch.IsToggled;

			if (toDoItem.Id == 0)
			{
				await toDoRepo.AddNewToDoItem(toDoItem);
			}
			else
			{
				await toDoRepo.UpdateToDoItem(toDoItem.Id, toDoItem);
			}
			await Navigation.PopAsync(true);
		}
	}
}
