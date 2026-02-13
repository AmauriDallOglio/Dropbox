Configuração de Aplicação no Dropbox

Para permitir a integração com o Dropbox, é necessário criar e configurar um aplicativo na plataforma de desenvolvedores. O procedimento deve ser realizado conforme as etapas abaixo.

Inicialmente, acesse o portal de desenvolvedores do Dropbox por meio do endereço:

https://www.dropbox.com/developers/apps

Na página inicial, selecione a opção Create app para iniciar o processo de criação de um novo aplicativo.

Choose an API (Escolher uma API): Scoped access (selecionado) → significa que seu app só terá acesso às permissões (scopes) que você habilitar. Isso é mais seguro, porque você pode dar apenas o que precisa (ex: ler arquivos, criar links, etc.). É o modelo atual recomendado pelo Dropbox.

Choose the type of access you need (Tipo de acesso): Aqui você define onde o app pode mexer dentro do Dropbox do usuário:
- App folder → cria uma pasta exclusiva para o seu app dentro do Dropbox do usuário. O app só consegue acessar essa pasta. Bom para apps que não precisam ver o Dropbox inteiro, apenas seus próprios arquivos.
- Full Dropbox (selecionado) → dá acesso a todos os arquivos e pastas do usuário. Necessário se você quer listar, baixar ou manipular qualquer arquivo do Dropbox. Requer mais confiança do usuário, porque é acesso total.

Name your app (Nome do app)
Aqui você escolhe o nome do aplicativo (no meu caso, “Amauri”). Esse nome aparece para o usuário quando ele autoriza o app. É importante escolher algo claro, porque será exibido na tela de consentimento.


<img width="1066" height="733" alt="image" src="https://github.com/user-attachments/assets/a9f45ca6-69c2-4378-abc1-2971410912d1" />

Com o aplicativo criado, temos o painel de gerenciamento do seu aplicativo no Dropbox API Console. 

<img width="1864" height="567" alt="image" src="https://github.com/user-attachments/assets/5556568f-fc3d-4693-ba32-3380aad1643e" />

O nome do app aparece no topo (“Amauri”) e logo abaixo você tem quatro abas principais. Cada uma serve para configurar ou visualizar aspectos diferentes do app:

<img width="1653" height="310" alt="image" src="https://github.com/user-attachments/assets/9b971119-f275-4dfc-afb3-2a590e0567d1" />

Settings (Configurações): Onde você define as opções básicas do app:
- Tipo de permissão (Scoped App ou não).
- App key e App secret.
- URIs de redirecionamento para OAuth.
- Se o app permite clientes públicos (PKCE).
- É a parte técnica de autenticação e credenciais.
- <img width="1238" height="708" alt="image" src="https://github.com/user-attachments/assets/87feed71-27dd-4a76-b328-efe441dd6a9f" />

Permissions (Permissões)
- Aqui você escolhe os escopos de acesso que o app terá.
- Exemplo: ler metadados de arquivos, baixar conteúdo, criar links de compartilhamento, acessar informações da conta.
- É fundamental configurar corretamente, porque sem os escopos certos o app não consegue chamar determinados endpoints da API.
- <img width="1237" height="755" alt="image" src="https://github.com/user-attachments/assets/5e34557e-7051-4d0f-b43d-cf5a3f1052c5" />


Branding (Identidade visual)
- Permite personalizar como seu app aparece para o usuário quando ele autoriza o acesso.
- Você pode definir: nome, ícone, descrição, cores.
- Isso ajuda a dar confiança e clareza ao usuário sobre quem está pedindo acesso.
- <img width="1222" height="667" alt="image" src="https://github.com/user-attachments/assets/822f591e-2ba9-47a8-84c5-4de056c14ffd" />


Analytics (Métricas)
- Mostra estatísticas de uso do app.
- Quantos usuários autorizaram, quantas chamadas à API foram feitas, erros, etc.
- É útil para monitorar desempenho e entender como seu app está sendo usado.
- <img width="1211" height="731" alt="image" src="https://github.com/user-attachments/assets/78c7b7f7-d114-4ee8-af4e-b6d063adddba" />




Prossiga para a etapa de configuração de permissões. No menu Permissions, habilite as seguintes etapas:

<img width="1637" height="320" alt="image" src="https://github.com/user-attachments/assets/cde44fdd-0653-46dd-ba44-873dc4a5fe80" />


Account Info:

- account_info.write (ativado) → permite ver e editar informações básicas da conta, como foto de perfil.
- account_info.read (desativado) → permitiria apenas ler informações básicas (nome, e-mail, país), sem editar.

<img width="1101" height="194" alt="image" src="https://github.com/user-attachments/assets/8e27264a-a885-4fd1-97c4-60173127478f" />


Files and Folders

- files.metadata.write (ativado) → permite ver e editar metadados dos arquivos/pastas (nome, caminho, etc.).
- files.metadata.read (desativado) → permitiria apenas ler metadados (sem editar).
- files.content.write (ativado) → permite editar o conteúdo dos arquivos (upload, sobrescrever).
- files.content.read (ativado) → permite ler o conteúdo dos arquivos (download).

<img width="1114" height="293" alt="image" src="https://github.com/user-attachments/assets/a8c88045-7181-480d-b857-e965644bd799" />


Collaboration

- sharing.write (ativado) → permite criar e gerenciar links de compartilhamento e colaboradores.
- sharing.read (desativado) → permitiria apenas ler configurações de compartilhamento já existentes.
- file_requests.write (ativado) → permite criar e gerenciar solicitações de arquivos (quando você pede que alguém envie arquivos para sua conta).
- file_requests.read (desativado) → permitiria apenas ler solicitações de arquivos existentes.
- contacts.write (ativado) → permite gerenciar contatos adicionados manualmente no Dropbox.
- contacts.read (desativado) → permitiria apenas ler contatos.

<img width="1131" height="429" alt="image" src="https://github.com/user-attachments/assets/9025d054-8c63-40f6-8243-986cf12ee67d" />

Connect

- profile (ativado) → permite ler informações básicas do perfil do usuário.
- openid (ativado e obrigatório) → necessário para o fluxo de autenticação OpenID Connect.
- email (desativado) → permitiria ler o e-mail básico do usuário.

<img width="1113" height="261" alt="image" src="https://github.com/user-attachments/assets/a51f805e-81f2-481c-8f74-f03666613fb0" />

Essas permissões são necessárias para permitir a leitura, gravação e consulta de metadados de arquivos no Dropbox. Após selecionar as permissões, clique em Submit para salvar as alterações.


<img width="1732" height="880" alt="image" src="https://github.com/user-attachments/assets/18416137-0516-4928-9309-5c2928be3f53" />

Por fim, é necessário gerar o Access Token que será utilizado pela aplicação para autenticação. Acesse a aba Settings do aplicativo:

<img width="1760" height="317" alt="image" src="https://github.com/user-attachments/assets/1739696b-5d9a-4a04-82ab-415e376910af" />

- Scoped App → significa que o app só terá acesso aos escopos (permissões) que você marcar na aba de permissões. É o modelo moderno e seguro, diferente do antigo que dava acesso amplo sem granularidade.
- App key → identificador público do seu aplicativo.
- App secret → senha privada do app, usada junto com o App key para autenticação. O secret deve ser guardado com segurança, nunca exposto em cliente web ou mobile.
- OAuth 2: É o protocolo de autenticação que o Dropbox usa. Ele garante que o usuário faça login de forma segura e que o app receba um token de acesso com as permissões concedidas. O fluxo pode ser interativo (usuário loga e autoriza) ou automático (com refresh token).
- Redirect URIs: São os endereços para onde o Dropbox redireciona o usuário depois do login. Exemplo: http://localhost durante desenvolvimento. É aqui que o app recebe o código de autorização, que depois troca por um token.
- Allow public clientes: Permite que apps sem segredo (ex: mobile ou desktop) usem PKCE ou Implicit Grant. Isso é útil quando você não pode armazenar o App secret com segurança no cliente.
- Generated access token: Botão para gerar um token de acesso rápido (short-lived). Esse token pode ser usado para testar chamadas à API sem passar pelo fluxo completo de OAuth. Mas ele expira rápido e não é recomendado em produção.


Diferença entre Token e OAuth: 
- Token de acesso → é a “chave temporária” que permite chamar a API. Expira em pouco tempo.
- OAuth 2.0 → é o processo completo de autenticação/autorização. Ele gera o token de acesso e, se configurado, também um refresh token (que não expira).

Em produção, você sempre usa OAuth para obter tokens válidos e renová-los automaticamente. O botão “Generate token” é só para testes rápidos.



Clique na opção Generate access token. Após a geração, copie o token e armazene-o em local seguro.

<img width="1388" height="613" alt="image" src="https://github.com/user-attachments/assets/4b9ae8a2-68e8-46c6-9345-21bc44ee2248" />


Atenção: o access token gerado concede acesso direto à conta do Dropbox associada ao aplicativo. Por esse motivo, ele deve ser tratado como informação sensível, evitando compartilhamento indevido ou exposição em código-fonte público.

<img width="1513" height="888" alt="image" src="https://github.com/user-attachments/assets/64a48dfa-c2c0-4294-ae03-f66e7845001e" />


<img width="1817" height="599" alt="image" src="https://github.com/user-attachments/assets/749fd1e3-9fb4-4b8d-8258-89f9ce457611" />



