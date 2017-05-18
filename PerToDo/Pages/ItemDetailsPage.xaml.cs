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
			toDoItem.name = nameEntry.Text.Trim();
			toDoItem.notes = notesEditor.Text.Trim();
			toDoItem.isDone = doneSwitch.IsToggled;

			if (toDoItem.id == 0)
			{ //Add new record
				await toDoRepo.AddNewToDoItem(toDoItem);
			}
			else
			{
				await toDoRepo.UpdateToDoItem(toDoItem.id, toDoItem);
			}
			await Navigation.PopAsync(true);
		}
	}
}
