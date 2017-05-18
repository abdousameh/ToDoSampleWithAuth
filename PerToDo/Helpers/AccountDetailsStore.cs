using System;
namespace PerToDo
{
	public sealed class AccountDetailsStore
	{
		private static readonly AccountDetailsStore instance = new AccountDetailsStore();

		private AccountDetailsStore() { }

		public static AccountDetailsStore Instance
		{
			get
			{
				return instance;
			}
		}

		public string PhoneNumber { get; set; }
		public string Token { get; set; }
	}
}