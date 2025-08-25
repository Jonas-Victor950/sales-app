# SalesApp — .NET 8 + React + MySQL (Docker)

Aplicação full-stack convertida de um teste técnico originalmente desktop para **Web API (.NET 8)** + **React (Vite + TS)** + **MySQL**.  
Focada em **Clean Architecture**, **testabilidade** e **DX** simples (subir tudo com `docker compose`).

---

## Sumário

- [Arquitetura & Stack](#arquitetura--stack)
- [Estrutura de Pastas](#estrutura-de-pastas)
- [Domínio & Regras](#domínio--regras)
- [Como Rodar (Docker Compose)](#como-rodar-docker-compose)
- [Como Rodar (Dev local, sem Docker)](#como-rodar-dev-local-sem-docker)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [API (principais endpoints)](#api-principais-endpoints)
- [Frontend (páginas)](#frontend-páginas)
- [Decisões Técnicas & Trade-offs](#decisões-técnicas--trade-offs)
- [Troubleshooting](#troubleshooting)
- [Próximos Passos](#próximos-passos)

---

## Arquitetura & Stack

**Backend**
- .NET **8** (Web API, Controllers)
- **Clean Architecture** (Domain / Application / Infrastructure / WebApi)
- **EF Core** + **Pomelo MySQL Provider**
- **MediatR v13** (CQRS para comandos/queries)
- **FluentValidation** (DTOs)
- **Serilog** (logs), **Swagger/OpenAPI**
- Migrations aplicadas no startup (`Database.Migrate()`)

**Frontend**
- **React 18** + **TypeScript** + **Vite**
- **MUI** (UI)
- **React Hook Form** + **Zod** (forms/validação)
- **Axios** (HTTP)
- **Estratégia de base URL: variável de ambiente (`.env`)**

**Infra**
- **Docker Compose**: `db` (MySQL 8) + `api` (.NET) + `web` (Nginx servindo React build) + `adminer`
- Portas (host): **5173** (frontend), **8080** (API), **8081** (Adminer), **3307** (MySQL)

---

## Estrutura de Pastas

```
sales-app/
├─ src/
│  ├─ Domain/           # Entidades e regras do domínio
│  ├─ Application/      # Casos de uso (MediatR), DTOs, validações
│  ├─ Infrastructure/   # EF Core/MySQL (DbContext, Migrations, Seed)
│  └─ WebApi/           # ASP.NET Core, Controllers, DI, Swagger
├─ frontend-app/        # React + Vite + TS (MUI, React Query, RHF, Zod)
├─ docker-compose.yml
├─ README.md (este arquivo)
└─ .editorconfig, etc.
```

---

## Domínio & Regras

Entidades principais:

- **Pessoa**: `Nome`, `CPF` (válido/único), `Endereço`.
- **Produto**: `Nome`, `Código` (único), `Valor`.
- **Pedido**: vinculado a `Pessoa`, `Itens` (produto+quantidade), `DataVenda` (UTC), `FormaPagamento`, `Status`.

**Enums (numéricos na API)**
- `FormaPagamento`: **0=Dinheiro**, **1=Cartão**, **2=Boleto**
- `PedidoStatus`: **0=Pendente**, **1=Pago**, **2=Enviado**, **3=Recebido**

**Regras**
- CPF válido.
- `Produto.Codigo` único; `Produto.Valor` `decimal(18,2)`.
- **Pedido finalizado** na criação (snapshot do preço em `PedidoItem.ValorUnitario`), **sem edição posterior**.
- Transições: `Pendente → Pago → Enviado → Recebido`. Transições inválidas retornam `409`.

**Seed (na primeira subida)**
- 2 Pessoas e 2 Produtos de exemplo (ver `Infrastructure/Seed/DbInitializer.cs`).

---

## Como Rodar (Docker Compose)

> Requisitos: Docker Desktop e Docker Compose.  
> Observação: usamos **3307** no host para evitar conflito com MySQL local (internamente os containers conversam na 3306).

1) **Clonar o projeto**
```bash
git clone https://github.com/Jonas-Victor950/sales-app.git
cd sales-app
```

2) **Subir tudo**
```bash
docker compose down --remove-orphans
docker compose up -d --build
```

3) **Acessos**
- Frontend: http://localhost:5173  
- API Swagger: http://localhost:8080/swagger  
- Healthcheck: http://localhost:8080/health  
- Adminer: http://localhost:8081 (Server: `db`, User: `app`, Pass: `apppwd`, DB: `appdb`)

> A API sobe com `ASPNETCORE_ENVIRONMENT=Development`, habilitando Swagger no container.

**Sobre as migrations**  
As migrations já estão incluídas e são **aplicadas automaticamente** no startup (`Database.Migrate()`), junto com o **seed** inicial.

---

## Como Rodar (Dev local, sem Docker)

### Backend
1) Configure (se necessário) `src/WebApi/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Default": "server=127.0.0.1;port=3307;database=appdb;user=app;password=apppwd;TreatTinyAsBoolean=false"
  }
}
```
2) Suba o MySQL local (ou o do Docker na porta 3307).
3) Rodar API:
```bash
dotnet build
dotnet run --project src/WebApi
```
Swagger: http://localhost:8080/swagger

### Frontend
1) Na pasta `frontend-app/`, copie `.env.example` para `.env` e ajuste:
```
VITE_API_BASE_URL=http://localhost:8080
```
2) Instale e suba:
```bash
npm i
npm run dev
```
Frontend: http://localhost:5173

> **Opção B adotada**: o front lê `VITE_API_BASE_URL` e chama a API por **`http://localhost:8080`** (sem proxy).  
> No Docker Compose, essa mesma URL funciona, pois a API também está publicada em `8080` no host.

---

## Variáveis de Ambiente

### Backend (WebApi – `docker-compose.yml`)
- `ASPNETCORE_ENVIRONMENT=Development` (habilita Swagger)
- `ASPNETCORE_URLS=http://+:8080`
- `ConnectionStrings__Default=server=db;port=3306;database=appdb;user=app;password=apppwd;TreatTinyAsBoolean=false`

### Frontend (`frontend-app/.env.example`)
```dotenv
# URL pública da API (usada pelo navegador)
# Dev local e Docker Compose publicam a API em http://localhost:8080
VITE_API_BASE_URL=http://localhost:8080
```

### Banco (MySQL – `docker-compose.yml`)
- Host port: **3307** → container port **3306**
- Credenciais: `app / apppwd`
- DB: `appdb`

---

## API (principais endpoints)

### Pessoas
- `GET /api/pessoas?nome=&cpf=`  
- `GET /api/pessoas/{id}`
- `POST /api/pessoas`
- `PUT /api/pessoas/{id}`
- `DELETE /api/pessoas/{id}`

### Produtos
- `GET /api/produtos?nome=&codigo=&valorMin=&valorMax=`  
- `GET /api/produtos/{id}`
- `POST /api/produtos`
- `PUT /api/produtos/{id}`
- `DELETE /api/produtos/{id}`

### Pedidos
- `GET /api/pedidos?status=&pessoaId=` (status **numérico**)
- `GET /api/pedidos/{id}`
- `POST /api/pedidos`  
  **Body exemplo** (enviando números nos enums):
  ```json
  {
    "pessoaId": 1,
    "formaPagamento": 1,
    "itens": [
      { "produtoId": 1, "quantidade": 2 },
      { "produtoId": 2, "quantidade": 1 }
    ]
  }
  ```
- `POST /api/pedidos/{id}/marcar-pago`
- `POST /api/pedidos/{id}/marcar-enviado`
- `POST /api/pedidos/{id}/marcar-recebido`

**Códigos comuns**: `200/201`, `404` (não encontrado), `409` (conflito/validação de negócio), `400/422` (validação DTO).

---

## Frontend (páginas)

- **/pessoas**: lista, filtros (nome/cpf), criar/editar/excluir (modal).
- **/produtos**: lista, filtros (nome/código/faixa de valor), criar/editar/excluir.
- **/pedidos**: listagem com filtros (status numérico / pessoaId).
- **/pedidos/novo**: cria pedido (seleciona pessoa, busca produto por nome, adiciona itens, totaliza).  
  Envia `formaPagamento` **numérico** (0/1/2), conforme o solicitado.

**Observações de UX**
- Uso de **TanStack Query** para cache/edit/invalidate.
- **React Hook Form + Zod** nas telas de criação/edição.
- **MUI** para componentes visuais.

---

## Decisões Técnicas & Trade-offs

- **Clean Architecture** para separar regras de negócio (Domain/Application) das bordas (Infra/Web).
- **MediatR v13** para orquestrar Commands/Queries (simplicidade > overengineering).
- **Enums numéricos** na API por padrão do .NET; o front envia números (pedido do teste).
  - Alternativa: habilitar `JsonStringEnumConverter` e usar strings.
- **Snapshot de preço** no `PedidoItem` (garante histórico para relatórios).
- **Sem edição de pedidos finalizados** (regra do enunciado).

---

## Troubleshooting

- **Porta 3306 ocupada** no Windows  
  O compose já publica **3307:3306**. Se quiser 3306, pare o MySQL local ou mude a porta do host.
- **Swagger 404**  
  Garanta `ASPNETCORE_ENVIRONMENT=Development` no serviço `api`.
- **Adminer não conecta**  
  Use **Server: `db`** (nome do serviço Docker), não `localhost`.
- **Front chamando `http://api:8080`**  
  No front usamos **Opção B**. Ajuste `.env` para `VITE_API_BASE_URL=http://localhost:8080` e rebuild do `web`.
- **Tipos do Vite** (`import.meta.env`)  
  O projeto contém `src/vite-env.d.ts`. Se mover arquivos, mantenha a referência `/// <reference types="vite/client" />`.

---

## Próximos Passos

- Ações de status no front (botões **Pagar / Enviar / Receber** na listagem de pedidos).
- Paginação/ordenação (MUI DataGrid).
- Máscara/validação de CPF no front.
- Autenticação (JWT) e perfis de acesso (opcional para o teste).
- Testes automatizados (xUnit no backend; RTL/Vitest no front).
- Pipeline CI (GitHub Actions) para build e imagens Docker.

---

## Comandos úteis

```bash
# Subir tudo
docker compose up -d --build

# Logs em tempo real
docker compose logs -f

# Derrubar
docker compose down --remove-orphans

# Backend (local)
dotnet restore && dotnet build && dotnet run --project src/WebApi

# Frontend (local)
cd frontend-app
npm i
npm run dev
```
