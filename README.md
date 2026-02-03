Configuração de Aplicação no Dropbox

Para permitir a integração com o Dropbox, é necessário criar e configurar um aplicativo na plataforma de desenvolvedores. O procedimento deve ser realizado conforme as etapas abaixo.

Inicialmente, acesse o portal de desenvolvedores do Dropbox por meio do endereço:

https://www.dropbox.com/developers/apps

Na página inicial, selecione a opção Create app para iniciar o processo de criação de um novo aplicativo. Ao configurar o aplicativo, defina a API como Scoped access. Em seguida, escolha o tipo de acesso desejado, selecionando Full Dropbox para permitir acesso completo à conta ou App folder caso seja necessário restringir o acesso apenas a uma pasta específica. Após isso, informe um nome para o aplicativo e finalize a criação.

<img width="1066" height="733" alt="image" src="https://github.com/user-attachments/assets/a9f45ca6-69c2-4378-abc1-2971410912d1" />


Com o aplicativo criado, prossiga para a etapa de configuração de permissões. No menu Permissions, habilite os seguintes scopes:

<img width="1078" height="537" alt="image" src="https://github.com/user-attachments/assets/199630f8-cabd-414d-9eb4-1bda5e63c0a8" />

- files.content.write
- files.content.read
- files.metadata.read

Essas permissões são necessárias para permitir a leitura, gravação e consulta de metadados de arquivos no Dropbox. Após selecionar as permissões, clique em Submit para salvar as alterações.

<img width="1092" height="284" alt="image" src="https://github.com/user-attachments/assets/d378e25c-4643-429a-96ec-d595d350443d" />

Por fim, é necessário gerar o Access Token que será utilizado pela aplicação para autenticação. Acesse a aba Settings do aplicativo e clique na opção Generate access token. Após a geração, copie o token e armazene-o em local seguro.

<img width="1868" height="566" alt="image" src="https://github.com/user-attachments/assets/113a33fa-1813-46b9-84cc-d175eb359428" />

Atenção: o access token gerado concede acesso direto à conta do Dropbox associada ao aplicativo. Por esse motivo, ele deve ser tratado como informação sensível, evitando compartilhamento indevido ou exposição em código-fonte público.


<img width="1817" height="599" alt="image" src="https://github.com/user-attachments/assets/749fd1e3-9fb4-4b8d-8258-89f9ce457611" />



