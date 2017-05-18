using System;
using System.Threading.Tasks;

namespace PerToDo
{
	public class AuthenticationRepository
	{
		IAuthenticationService authenticationService;

		public AuthenticationRepository(IAuthenticationService service)
		{
			authenticationService = service;
		}

		public Task RegisterAccount(string phoneNumber)
		{
			return authenticationService.Register(phoneNumber);
		}

		public Task AutherizeAccount(string phoneNumber, string verificationCode)
		{
			return authenticationService.AuthorizeUser(phoneNumber, verificationCode);
		}
	}
}