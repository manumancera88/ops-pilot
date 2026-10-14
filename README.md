# OpsPilot AI

SaaS multi-tenant para gestão de clientes e agendamentos, com arquitetura 
pensada para um futuro AI Copilot controlado por autorização — construído 
com Angular + .NET + SQL Server.

## Stack
Angular 21 · ASP.NET Core (.NET 10) · SQL Server · EF Core · Docker · JWT

## Arquitetura

Angular → .NET API → Service Layer → EF Core → SQL Server

A IA (planejada) nunca acessa o banco de dados diretamente — toda ação 
passa por tools controladas, validação e autorização.

## Estado atual

✅ Modelo de dados multi-tenant (Company, Customer, Appointment, User)
✅ CRUD completo com validações de negócio
✅ Arquitetura em camadas (Controller → Service → DbContext)
✅ DTOs separados por entidade
✅ Autenticação JWT funcionando ponta a ponta (register, login, endpoints protegidos)
🔄 Em progresso: multi-tenancy real (extrair CompanyId do JWT em vez de confiar no frontend)
⏳ Próximo: frontend Angular (login, dashboard, features), AI Copilot com tool calling

## Como rodar localmente

\`\`\`bash
docker compose up -d
cd backend
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=OpsPilotDb;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=true"
dotnet user-secrets set "Jwt:Key" "SUA_CHAVE_JWT"
dotnet ef database update
dotnet run
\`\`\`

## Decisões técnicas

- **User Secrets** em vez de credenciais no código, para manter segredos fora do repositório
- **Service Layer** separando lógica de negócio dos controllers, facilitando testes
- **DTOs dedicados** por operação (Create/Update/Read) para controlar exatamente o que a API expõe e recebe
