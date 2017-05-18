using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PerToDo
{
	public partial class MainPage : ContentPage
	{
		//List<ToDoItem> todoList;
		ToDoRepository todoRepo = new ToDoRepository(new ToDoService());

		public MainPage()
		{
			InitializeComponent();

			configurePage();
		}

		private void configurePage()
		{
			Title = "To do list";
			AddTaskButton.Clicked += AddTaskButton_Clicked;
			NavigationPage.SetHasBackButton(this, false);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			var _ = configureTodoList();
		}

		private async Task configureTodoList()
		{
			todoListView.ItemsSource = await todoRepo.GetToDoList();
		}

		void AddTaskButton_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ItemDetailsPage(new ToDoItem()));
		}

		void todoListView_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
		{
			var todoItem = e.SelectedItem as ToDoItem;
            var itemDetailsPage = new ItemDetailsPage(todoItem)
            {
                BindingContext = todoItem
            };
            Navigation.PushAsync(itemDetailsPage);
		}

        public async void OnTaskDelete(object sender, EventArgs e)
		{
			var menuItem = ((MenuItem)sender);
            var taskId = (Int32)menuItem.CommandParameter;
            var isConfirmed = await DisplayAlert("Delete Confirmation", "Are you sure you want to delete this task?" ,"Confirm", "Cancel");

            if (isConfirmed) {
                await todoRepo.DeleteToDoItem(taskId);
                await configureTodoList();
            }
		}
	}
}