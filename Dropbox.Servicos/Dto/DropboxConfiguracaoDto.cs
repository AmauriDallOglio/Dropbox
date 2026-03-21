using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dropbox.Servicos.Dto
{
    public class DropboxConfiguracaoDto
    {
        public string AppKey { get; set; } = "";
        public string AppSecret { get; set; } = "";
        public string RedirectUri { get; set; } = "";
        public string Pasta { get; set; } = "";
        public string NomeArquivo { get; set; } = "";
    }
}
