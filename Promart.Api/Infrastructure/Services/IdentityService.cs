using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Promart.Api.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private IHttpContextAccessor _context;

        public IdentityService(IHttpContextAccessor context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string GetUserIP()
        {
            if (_context.HttpContext.Request.Headers.ContainsKey("IpClient"))
                return _context.HttpContext.Request.Headers["IpClient"].ToString();
            else
                return _context.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        public Guid GetUserGuid()
        {
            var basePrincipal = new List<Claim>();
            basePrincipal.AddRange(_context.HttpContext.User.Claims);
            return basePrincipal == null || basePrincipal.Count == 0 ? Guid.Empty : Guid.Parse(basePrincipal.Find(x => x.Type == "guid_usuario")?.Value);
        }

        public string GetUserIdentity()
        {
            return _context.HttpContext.User.FindFirst("sub").Value;
        }

        public string GetUserName()
        {
            return _context.HttpContext.User.Identity.Name;
        }
    }
}
