using System;

namespace Promart.Api.Infrastructure.Services
{
    public interface IIdentityService
    {
        string GetUserIdentity();

        string GetUserName();

        string GetUserIP();

        Guid GetUserGuid();
    }
}
