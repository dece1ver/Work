using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace remeLog.Infrastructure.Services
{
    public class WindchillService
    {
        private readonly string _serverUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _localType;

        public WindchillService(string serverUrl, string username, string password, string localType)
        {
            _serverUrl = serverUrl;
            _username = username;
            _password = password;
            _localType = localType;
        }

        public async Task<string> SearchDocumentsAsync(string searchQuery, CancellationToken cancellationToken)
        {
            using var client = new WindchillClient(_serverUrl, _username, _password, _localType);

            var isAuthorized = await client.AuthorizeAsync(cancellationToken).ConfigureAwait(false);
            Util.Debug(isAuthorized);
            if (!isAuthorized)
                throw new UnauthorizedAccessException("Не удалось авторизоваться в Windchill");
            cancellationToken.ThrowIfCancellationRequested();
            return await client.SearchAsync(searchQuery, cancellationToken);
        }
    }
}
