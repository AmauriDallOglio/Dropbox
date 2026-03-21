using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dropbox.Servicos.Dto
{
    public class TokenArquivoDto
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTime? ExpiresAt { get; set; }

        public bool IsAccessTokenExpired()
        {
            if (!ExpiresAt.HasValue)
                return false;

            return DateTime.UtcNow >= ExpiresAt.Value.AddMinutes(-2);
        }
    }
}
