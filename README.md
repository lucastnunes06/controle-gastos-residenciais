# LarFinance

Sistema full-stack para controle de gastos residenciais, construído com **ASP.NET Core**, **React** e **TypeScript**. A aplicação cobre integralmente as regras do desafio e acrescenta validações, responsividade, filtros, documentação, automação de qualidade e execução em contêiner.

## O que foi implementado

| Área | Funcionalidades |
| --- | --- |
| Pessoas | Criação, listagem e exclusão com confirmação |
| Transações | Criação e listagem de receitas e despesas |
| Regras | Pessoa obrigatória; menor de 18 anos aceita somente despesas; exclusão em cascata |
| Totais | Receitas, despesas e saldo por pessoa, além do consolidado geral |
| Experiência | Dashboard responsivo, filtros, estados vazios, feedback de erro e prevenção de ações inválidas |
| Qualidade | Cenários automatizados, lint, formatação, CI e documentação da arquitetura |

Os dados são salvos em `App_Data/larfinance.json` e permanecem disponíveis após a aplicação ser encerrada. Como a persistência não utiliza banco relacional, migrations não se aplicam; o arquivo é criado automaticamente na primeira gravação.


## Tecnologias

- ASP.NET Core 10 e C# no back-end
- React 19.2 e TypeScript 5.9 no front-end
- Vite 8 para desenvolvimento e build
- Vitest 4, ESLint e Prettier para qualidade
- JSON local com escrita atômica para persistência

## Funcionalidades adicionais

- Dashboard com indicadores e últimas movimentações
- Filtro de transações por tipo
- Confirmação acessível de exclusão
- Toasts, estados vazios e recuperação de falhas
- Health check, OpenAPI e Swagger
- Dockerfile e integração contínua
- Layout responsivo e suporte a redução de movimento

## Executar localmente

### Pré-requisitos

- .NET SDK 10
- Node.js 22 ou superior

### 1. Iniciar a API

```bash
cd src/LarFinance.Api
dotnet run --urls http://localhost:5080
```

### 2. Iniciar a interface

Em outro terminal:

```bash
cd src/LarFinance.Web
npm install
npm run dev
```

Acesse [http://localhost:5173](http://localhost:5173). Durante o desenvolvimento, o Vite encaminha as chamadas `/api` para `http://localhost:5080`.

A documentação interativa da API fica em [http://localhost:5080/swagger](http://localhost:5080/swagger), e o documento OpenAPI em `http://localhost:5080/openapi/v1.json`.

## Executar com Docker

```bash
docker build -t larfinance .
docker run --rm -p 8080:8080 -v larfinance-data:/app/App_Data larfinance
```

Acesse [http://localhost:8080](http://localhost:8080). O volume mantém os dados mesmo que o contêiner seja recriado.

## Validar o projeto

```bash
# API e cenários de negócio
dotnet build LarFinance.slnx
dotnet run --project tests/LarFinance.Tests --no-build

# Interface
cd src/LarFinance.Web
npm ci
npm run lint
npm run format:check
npm test
npm run build
```

A suíte cobre 18 cenários de negócio no back-end e 4 testes no front-end. A automação em `.github/workflows/ci.yml` repete essas verificações em cada push e pull request.

## Regras de negócio e onde estão

- **IDs únicos:** criados com `Guid.NewGuid()` pelo repositório.
- **Pessoa existente:** a API rejeita uma transação cujo `personId` não esteja cadastrado.
- **Restrição por idade:** receitas para menores de 18 anos são bloqueadas no servidor. A interface antecipa a regra, mas não é a autoridade.
- **Exclusão em cascata:** pessoa e respectivas transações são removidas na mesma seção crítica.
- **Valores válidos:** transações exigem valor positivo; nomes e descrições não aceitam conteúdo vazio ou somente espaços.
- **Totais confiáveis:** valores consolidados são derivados das transações persistidas, sem armazenar dados calculados duplicados.

## Endpoints

| Método | Rota | Resultado |
| --- | --- | --- |
| `GET` | `/api/people` | Lista pessoas |
| `POST` | `/api/people` | Cria uma pessoa |
| `DELETE` | `/api/people/{id}` | Exclui pessoa e transações |
| `GET` | `/api/transactions` | Lista transações |
| `POST` | `/api/transactions` | Cria uma transação |
| `GET` | `/api/totals` | Retorna totais individuais e geral |
| `GET` | `/api/health` | Informa a disponibilidade da API |

Exemplos completos estão em [`docs/API.md`](docs/API.md) e também podem ser executados pelo arquivo [`LarFinance.Api.http`](src/LarFinance.Api/LarFinance.Api.http).

## Decisões técnicas

A persistência foi isolada por `IHouseholdRepository`. O arquivo JSON atende à exigência de durabilidade sem exigir que o avaliador instale banco de dados. Um `SemaphoreSlim` serializa o acesso, enquanto a gravação em arquivo temporário seguida de substituição reduz o risco de corrupção. Essa implementação pode ser trocada por EF Core e banco relacional sem modificar os controllers.

Valores monetários usam `decimal`, indicado para cálculos financeiros. Horários são gravados em UTC com `DateTimeOffset`. As respostas de erro seguem `ProblemDetails`, oferecendo uma estrutura previsível para clientes da API.

Mais detalhes e trade-offs estão em [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## Estrutura

```text
LarFinance/
+-- src/
|   +-- LarFinance.Api/       # API, regras e persistência
|   +-- LarFinance.Web/       # React + TypeScript
+-- tests/LarFinance.Tests/   # Cenários críticos do domínio
+-- docs/                     # API e arquitetura
+-- .github/workflows/        # Integração contínua
+-- Dockerfile                # Publicação integrada
```


## Melhorias futuras

Para uma evolução além do escopo, os próximos passos naturais seriam migrar a implementação de **IHouseholdRepository** para SQLite, adicionar consultas por período e ampliar os testes de componentes da interface. Essas melhorias não são necessárias para os requisitos atuais.
