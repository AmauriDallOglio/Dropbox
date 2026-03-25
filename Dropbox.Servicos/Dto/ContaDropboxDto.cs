using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dropbox.Servicos.Dto
{
    public class ContaDropboxDto
    {

        public string Nome { get; private set; }
        public string Email { get; private set; }
        public string Pais { get; private set; }
        public string AccountId { get; private set; }

        public bool TipoBasico { get; private set; }
        public bool TipoBusiness { get; private set; }
        public bool TipoPro { get; private set; }

        // CONSTRUTOR PRIVADO (DDD)
        private ContaDropboxDto() { }

        public ContaDropboxDto(string nome, string email, string pais, string accountId, bool tipoBasico, bool tipoBusiness, bool tipoPro)
        {
            Nome = nome;
            Email = email;
            Pais = pais;
            AccountId = accountId;
            TipoBasico = tipoBasico;
            TipoBusiness = tipoBusiness;
            TipoPro = tipoPro;

            Validar();
        }

        // REGRA DE NEGÓCIO
        private void Validar()
        {
            if (string.IsNullOrWhiteSpace(AccountId))
                throw new Exception("Conta inválida");

            if (!TipoBasico && !TipoBusiness && !TipoPro)
                throw new Exception("Tipo de conta não definido");
        }


    }
}
