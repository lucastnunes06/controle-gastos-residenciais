~# LarFinance

Sistema full stack para controle de gastos residenciais, desenvolvido com **ASP.NET Core**, **C#**, **React** e **TypeScript**.

A aplicação permite cadastrar pessoas e transações, consultar receitas, despesas e saldos e visualizar um resumo financeiro geral. Além dos requisitos principais, o projeto inclui validações, filtros, interface responsiva, documentação da API, persistência local, Docker e integração contínua.

## Funcionalidades

| Área | Funcionalidades |
| --- | --- |
| Pessoas | Cadastro, listagem e exclusão com confirmação |
| Transações | Cadastro e listagem de receitas e despesas |
| Regras de negócio | Validação da pessoa, restrição para menores de idade e exclusão em cascata |
| Totais | Receitas, despesas e saldo por pessoa, além do consolidado geral |
| Experiência | Dashboard, filtros, estados vazios, mensagens de erro e prevenção de ações inválidas |
| Qualidade | Testes do front-end, lint, formatação, integração contínua e documentação técnica |

## Requisitos atendidos

### Cadastro de pessoas

O sistema permite:

- cadastrar uma pessoa;
- listar as pessoas cadastradas;
- excluir uma pessoa;
- gerar automaticamente um identificador único;
- armazenar nome e idade.

Ao excluir uma pessoa, todas as transações vinculadas a ela também são removidas.

### Cadastro de transações

O sistema permite:

- cadastrar receitas e despesas;
- listar as transações cadastradas;
- relacionar cada transação a uma pessoa existente;
- gerar automaticamente um identificador único;
- informar descrição, valor e tipo.

Pessoas menores de 18 anos podem receber apenas transações do tipo despesa. A regra é validada no back-end, evitando que seja contornada por uma chamada direta à API.

### Consulta de totais

A consulta de totais apresenta, para cada pessoa:

- total de receitas;
- total de despesas;
- saldo individual, calculado por receitas menos despesas.

Também são exibidos:

- total geral de receitas;
- total geral de despesas;
- saldo líquido geral.

Pessoas sem transações permanecem na listagem com valores zerados.

## Persistência

Os dados são armazenados no arquivo:

```text
App_Data/larfinance.json
```

O arquivo é criado automaticamente na primeira gravação e mantém os dados disponíveis mesmo após o encerramento da aplicação.

A persistência utiliza escrita em arquivo temporário seguida de substituição, reduzindo o risco de corrupção em caso de interrupção durante uma gravação.

Como o projeto utiliza persistência em JSON, não é necessário configurar banco de dados ou executar migrations.

## Tecnologias utilizadas

### Back-end

- ASP.NET Core 10
- C#
- Swagger e OpenAPI
- `ProblemDetails` para respostas de erro padronizadas
- Persistência local em JSON
- `SemaphoreSlim` para sincronização do acesso aos dados

### Front-end

- React
- TypeScript
- Vite
- Vitest
- ESLint
- Prettier
- CSS responsivo

### Infraestrutura e qualidade

- Docker
- GitHub Actions
- EditorConfig
- Integração contínua para build e validação do projeto

## Funcionalidades adicionais

Além dos requisitos obrigatórios, foram implementados:

- dashboard com indicadores financeiros;
- visualização das movimentações recentes;
- filtro de transações por tipo;
- confirmação antes da exclusão de uma pessoa;
- mensagens de sucesso e erro;
- estados de carregamento;
- estados vazios com orientações ao usuário;
- prevenção de envios duplicados;
- tratamento de falhas de comunicação com a API;
- health check;
- documentação interativa com Swagger;
- layout responsivo;
- suporte à preferência de redução de movimento;
- execução da aplicação em contêiner Docker;
- pipeline de integração contínua.

## Regras de negócio

### Identificadores únicos

Os identificadores são gerados automaticamente com `Guid.NewGuid()`.

### Pessoa obrigatória

Uma transação somente pode ser cadastrada quando o identificador informado pertence a uma pessoa existente.

### Restrição para menores de idade

Pessoas com menos de 18 anos podem cadastrar somente despesas.

A interface antecipa essa restrição para melhorar a experiência do usuário, mas a validação definitiva ocorre no servidor.

### Exclusão em cascata

Quando uma pessoa é excluída, suas transações são removidas na mesma operação, impedindo a existência de registros órfãos.

### Valores válidos

As transações exigem valor maior que zero.

Valores monetários utilizam o tipo `decimal`, adequado para cálculos financeiros.

### Campos textuais

Nomes e descrições:

- são obrigatórios;
- não podem conter somente espaços;
- são normalizados antes da gravação.

### Cálculo dos totais

Receitas, despesas e saldos são calculados a partir das transações persistidas.

Os totais não são armazenados separadamente, evitando duplicidade e inconsistência de dados.

## Pré-requisitos

Para executar o projeto localmente, instale:

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 22 ou superior](https://nodejs.org/)
- Git

O Docker é opcional.

## Executar localmente

Clone o repositório:

```bash
git clone https://github.com/lucastnunes06/controle-gastos-residenciais.git
cd controle-gastos-residenciais
```

### 1. Iniciar o back-end

Na raiz do projeto:

```bash
cd src/LarFinance.Api
dotnet restore
dotnet run --urls http://localhost:5080
```

A API estará disponível em:

```text
http://localhost:5080
```

A documentação Swagger estará disponível em:

```text
http://localhost:5080/swagger
```

O documento OpenAPI estará disponível em:

```text
http://localhost:5080/openapi/v1.json
```

### 2. Iniciar o front-end

Abra outro terminal e execute:

```bash
cd src/LarFinance.Web
npm install
npm run dev
```

A interface estará disponível em:

```text
http://localhost:5173
```

Durante o desenvolvimento, o Vite encaminha as chamadas iniciadas por `/api` para:

```text
http://localhost:5080
```

## Executar com Docker

Na raiz do repositório, construa a imagem:

```bash
docker build -t larfinance .
```

Execute o contêiner:

```bash
docker run --rm -p 8080:8080 -v larfinance-data:/app/App_Data larfinance
```

A aplicação estará disponível em:

```text
http://localhost:8080
```

O volume `larfinance-data` preserva os dados mesmo que o contêiner seja removido e criado novamente.

## Validar o projeto

### Back-end

Na raiz do repositório:

```bash
dotnet restore
dotnet build LarFinance.slnx --configuration Release
```

### Front-end

```bash
cd src/LarFinance.Web
npm ci
npm run lint
npm run format:check
npm test
npm run build
```

A integração contínua configurada no GitHub Actions executa o build do back-end, os testes automatizados do front-end e o build de produção a cada push ou pull request para a branch principal.

## Endpoints da API

| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/people` | Lista todas as pessoas |
| `POST` | `/api/people` | Cadastra uma pessoa |
| `DELETE` | `/api/people/{id}` | Exclui a pessoa e suas transações |
| `GET` | `/api/transactions` | Lista as transações |
| `POST` | `/api/transactions` | Cadastra uma transação |
| `GET` | `/api/totals` | Retorna os totais individuais e gerais |
| `GET` | `/api/health` | Informa a disponibilidade da API |

Exemplos detalhados de requisições e respostas estão disponíveis em:

- [`docs/API.md`](docs/API.md)
- [`src/LarFinance.Api/LarFinance.Api.http`](src/LarFinance.Api/LarFinance.Api.http)

## Exemplo de regra para menores de idade

Uma pessoa com 16 anos pode receber a seguinte transação:

```json
{
  "description": "Material escolar",
  "value": 120.5,
  "type": "expense",
  "personId": "identificador-da-pessoa"
}
```

Uma transação do tipo receita para essa mesma pessoa será recusada pela API.

## Tratamento de erros

A API utiliza respostas padronizadas no formato `ProblemDetails`.

As mensagens tratam situações como:

- dados obrigatórios ausentes;
- nome ou descrição inválidos;
- idade fora do intervalo aceito;
- valor igual ou inferior a zero;
- pessoa inexistente;
- receita para pessoa menor de idade;
- recurso não encontrado;
- falha inesperada de persistência.

O front-end apresenta mensagens compreensíveis ao usuário, sem expor detalhes internos da aplicação.

## Decisões técnicas

### Persistência em JSON

A especificação exige que os dados permaneçam disponíveis após o encerramento da aplicação, mas não determina um banco de dados específico.

A persistência em JSON foi escolhida para:

- reduzir a configuração necessária;
- facilitar a avaliação do projeto;
- evitar dependências externas;
- manter os dados após o encerramento;
- permitir execução imediata.

O acesso aos dados foi isolado por meio de `IHouseholdRepository`. Dessa forma, a implementação pode ser substituída futuramente por SQLite, Entity Framework Core ou outro mecanismo de persistência sem exigir alterações significativas nos controllers.

### Sincronização de acesso

Um `SemaphoreSlim` controla o acesso ao arquivo, evitando gravações simultâneas que poderiam causar inconsistências.

### Escrita atômica

As alterações são gravadas primeiro em um arquivo temporário. Em seguida, o arquivo principal é substituído, reduzindo o risco de dados parcialmente escritos.

### Valores financeiros

Valores monetários utilizam `decimal`, evitando os problemas de precisão associados a `float` e `double`.

### Datas

As datas são armazenadas em UTC com `DateTimeOffset`, tornando o comportamento previsível em ambientes diferentes.

### Erros da API

As respostas de erro seguem o padrão `ProblemDetails`, fornecendo uma estrutura consistente para o front-end e para outros possíveis consumidores da API.

Mais detalhes sobre a arquitetura estão disponíveis em:

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)

## Estrutura do projeto

```text
LarFinance/
├── .github/
│   └── workflows/
│       └── ci.yml
├── docs/
│   ├── API.md
│   └── ARCHITECTURE.md
├── src/
│   ├── LarFinance.Api/
│   │   ├── Controllers/
│   │   ├── Contracts/
│   │   ├── Domain/
│   │   ├── Persistence/
│   │   └── Program.cs
│   └── LarFinance.Web/
│       ├── src/
│       ├── public/
│       └── package.json
├── .dockerignore
├── .editorconfig
├── .gitignore
├── Dockerfile
├── LarFinance.slnx
├── NuGet.Config
└── README.md
```

## Integração contínua

O workflow localizado em:

```text
.github/workflows/ci.yml
```

é executado automaticamente em pushes e pull requests para a branch `main`.

O pipeline verifica:

- checkout do código;
- configuração do ambiente .NET;
- build do back-end;
- configuração do Node.js;
- instalação limpa das dependências;
- execução dos testes do front-end;
- build de produção do front-end.

## Possíveis melhorias futuras

Como evoluções além do escopo atual, poderiam ser implementados:

- persistência com SQLite e Entity Framework Core;
- testes automatizados das regras de negócio do back-end;
- consultas de transações por período;
- edição e exclusão de transações;
- paginação das listagens;
- filtros por pessoa;
- exportação de relatórios;
- ampliação dos testes de componentes do front-end.

Essas melhorias não são necessárias para o atendimento dos requisitos atuais.

## Autor

Desenvolvido por **Lucas Nunes**.

- GitHub: [lucastnunes06](https://github.com/lucastnunes06)