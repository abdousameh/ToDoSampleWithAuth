using System;
using System.Threading.Tasks;
using System.Net;

namespace PerToDo
{
	public interface IAuthenticationService
	{
		Task Register(string phoneNumber);

		Task<HttpStatusCode> AuthorizeUser(string phoneNumber, string verificationCode);
	}
}
