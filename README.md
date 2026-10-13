# WebLib — Sistema de Gerenciamento de Biblioteca

Sistema web de gerenciamento de biblioteca desenvolvido como projeto de estudo, com o objetivo de aplicar conceitos de desenvolvimento back-end, organização de código e integração com banco de dados.

## Sobre o projeto

O WebLib permite organizar o acervo de uma biblioteca e gerenciar operações relacionadas a livros, exemplares, usuários e empréstimos, por meio de uma interface web.

O projeto está em desenvolvimento e faz parte da minha jornada de aprendizado em Análise e Desenvolvimento de Sistemas.

## Demonstração

Confira o vídeo para conhecer a interface do WebLib,
navegar pelo catálogo e visualizar as principais funcionalidades
implementadas no sistema.

▶️ [Assistir à demonstração do WebLib](https://youtu.be/ud_VIjblMJQ)


## Funcionalidades

- Cadastro e gerenciamento de livros, autores, editoras e categorias.
- Organização de exemplares do acervo.
- Cadastro e gerenciamento de usuários.
- Solicitação e gerenciamento de empréstimos.
- Acompanhamento de prazos e devoluções.
- Catálogo de livros com filtros e categorias em destaque.
- Sistema de favoritos.
- Área administrativa para gerenciamento do sistema.
- Newsletter para cadastro de e-mails.
- Página institucional Sobre Nós.

## Tecnologias utilizadas

- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server LocalDB
- Razor Views
- HTML5 e CSS3
- Bootstrap
- JavaScript


## Arquitetura e organização

O projeto utiliza o padrão MVC (Model-View-Controller) e separa responsabilidades em diretórios para controllers, modelos, views, repositórios, serviços e acesso a dados.

As migrações do Entity Framework Core são utilizadas para versionar as alterações na estrutura do banco de dados.


## Como executar o projeto

### Pré-requisitos

- .NET SDK compatível com o projeto (.NET 10).
- Visual Studio ou outro editor compatível com .NET.
- SQL Server LocalDB.

### Passos

1. Clone o repositório:

   ```bash
   git clone https://github.com/GSegatto/WebLib.git
   ```

2. Entre na pasta do projeto que contém o arquivo `Biblioteca.csproj`.

3. Configure a conexão com o banco de dados no arquivo `appsettings.json`, conforme o ambiente local.

4. Configure os User Secrets necessários para as senhas utilizadas na criação dos dados iniciais, caso o seed do projeto os exija.

5. Aplique as migrações do banco de dados:

   ```bash
   dotnet ef database update
   ```

   Se o comando `dotnet ef` não estiver disponível, verifique se a ferramenta EF Core está instalada.

6. Execute a aplicação:

   ```bash
   dotnet run
   ```

7. Abra no navegador a URL local informada pelo terminal.

## Aprendizados

Este projeto permite praticar conceitos de desenvolvimento web com C#, organização de aplicações MVC, operações com banco de dados, Entity Framework Core, injeção de dependência e implementação de funcionalidades comuns em um sistema de biblioteca.

## Autor

**GSegatto**

Estudante de Análise e Desenvolvimento de Sistemas.

GitHub: https://github.com/GSegatto
