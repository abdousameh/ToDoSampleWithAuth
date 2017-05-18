using System;
using System.Threading.Tasks;

namespace PerToDo
{
	public interface IAuthenticationService
	{
		Task Register(string phoneNumber);

		Task AuthorizeUser(string phoneNumber, string verificationCode);
	}
}
