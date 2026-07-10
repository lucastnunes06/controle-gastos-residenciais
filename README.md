# LarFinance

[![Continuous Integration](https://github.com/lucastnunes06/controle-gastos-residenciais/actions/workflows/ci.yml/badge.svg)](https://github.com/lucastnunes06/controle-gastos-residenciais/actions/workflows/ci.yml)

Sistema full stack para controle de gastos residenciais, desenvolvido com **ASP.NET Core**, **C#**, **React** e **TypeScript**.

A aplicação permite cadastrar pessoas e transações, consultar receitas, despesas e saldos e acompanhar um resumo financeiro da residência. Além dos requisitos obrigatórios, o projeto inclui dashboard, filtros, validações no servidor, interface responsiva, documentação da API, persistência local, testes automatizados, Docker e integração contínua.

---

## Funcionalidades

| Área | Funcionalidades |
| --- | --- |
| Pessoas | Cadastro, listagem e exclusão com confirmação |
| Transações | Cadastro e listagem de receitas e despesas |
| Regras de negócio | Pessoa obrigatória, restrição para menores de idade e exclusão em cascata |
| Totais | Receitas, despesas e saldo por pessoa, além do consolidado geral |
| Experiência | Dashboard, filtros, estados vazios, carregamento e mensagens de erro |
| Qualidade | 15 testes no back-end, 4 testes no front-end, lint, formatação e CI |
| Documentação | README, documentação da API, arquitetura e exemplos de requisições |

---

## Requisitos atendidos

### Cadastro de pessoas

O sistema permite:

- cadastrar uma pessoa;
- listar todas as pessoas cadastradas;
- excluir uma pessoa;
- gerar automaticamente um identificador único;
- armazenar nome e idade;
- validar nome e faixa etária.

Ao excluir uma pessoa, todas as transações vinculadas a ela também são removidas.

### Cadastro de transações

O sistema permite:

- cadastrar receitas e despesas;
- listar as transações cadastradas;
- relacionar cada transação a uma pessoa existente;
- gerar automaticamente um identificador único;
- informar descrição, valor e tipo;
- visualizar a pessoa responsável e a data da movimentação.

Não é necessário implementar edição ou exclusão individual de transações, conforme o escopo proposto.

### Regra para menores de idade

Pessoas menores de 18 anos podem cadastrar apenas transações do tipo **despesa**.

A interface antecipa essa restrição ao desabilitar a opção de receita, mas a validação definitiva também ocorre no back-end. Dessa forma, a regra não pode ser contornada por uma chamada direta à API.

Pessoas com exatamente 18 anos já podem registrar receitas.

### Consulta de totais

A consulta apresenta, para cada pessoa:

- total de receitas;
- total de despesas;
- saldo individual, calculado por receitas menos despesas.

Também são apresentados:

- total geral de receitas;
- total geral de despesas;
- saldo líquido geral.

Pessoas sem transações permanecem na listagem com os valores zerados.

---

## Diferenciais implementados

Além das funcionalidades obrigatórias, o projeto possui:

- dashboard com indicadores financeiros;
- resumo do saldo por pessoa;
- listagem das movimentações mais recentes;
- filtro de transações por tipo;
- confirmação antes da exclusão de uma pessoa;
- aviso sobre a remoção das transações vinculadas;
- mensagens de sucesso e erro;
- estados de carregamento;
- estados vazios com orientações ao usuário;
- prevenção de envios duplicados;
- recuperação de falhas de comunicação;
- validações no front-end e no back-end;
- documentação Swagger e OpenAPI;
- endpoint de disponibilidade da API;
- layout responsivo;
- navegação acessível por teclado;
- suporte à preferência de redução de movimento;
- execução com Docker;
- testes automatizados;
- pipeline de integração contínua.

---

## Tecnologias utilizadas

### Back-end

- .NET 10
- ASP.NET Core
- C#
- Swagger e OpenAPI
- `ProblemDetails` para respostas de erro padronizadas
- persistência local em JSON
- `SemaphoreSlim` para sincronização de acesso
- xUnit para testes automatizados
- Microsoft.NET.Test.Sdk
- Coverlet Collector

### Front-end

- React 19
- TypeScript
- Vite
- Vitest
- ESLint
- Prettier
- Lucide React
- CSS responsivo

### Infraestrutura e qualidade

- Docker
- GitHub Actions
- EditorConfig
- integração contínua
- build automatizado
- testes automatizados
- lint e verificação de formatação

---

## Persistência

Os dados são armazenados localmente no arquivo:

```text
App_Data/larfinance.json
```

O arquivo é criado automaticamente na primeira gravação e mantém os dados disponíveis após o encerramento da aplicação.

A persistência utiliza:

- acesso sincronizado por `SemaphoreSlim`;
- escrita inicial em arquivo temporário;
- substituição do arquivo principal após a gravação;
- serialização dos enums como texto;
- abstração por meio de `IHouseholdRepository`.

Essa abordagem reduz o risco de gravações simultâneas e de arquivos parcialmente escritos.

Como a especificação exige persistência, mas não determina um banco de dados, o armazenamento em JSON permite executar e avaliar a aplicação sem instalar serviços externos ou executar migrations.

A implementação pode ser substituída futuramente por SQLite, Entity Framework Core ou outro mecanismo de persistência sem exigir mudanças significativas nos controllers.

---

## Regras de negócio

### Identificadores únicos

Pessoas e transações recebem identificadores únicos gerados automaticamente com:

```csharp
Guid.NewGuid()
```

### Pessoa existente

Uma transação somente pode ser cadastrada quando o identificador informado pertence a uma pessoa existente.

### Restrição por idade

Uma pessoa com menos de 18 anos pode registrar apenas despesas.

A regra é protegida no back-end e também antecipada pela interface.

### Exclusão em cascata

Quando uma pessoa é excluída, todas as transações associadas ao seu identificador são removidas na mesma operação.

### Valores monetários

As transações exigem valor maior que zero e dentro do limite aceito pela aplicação.

Os valores utilizam o tipo `decimal`, adequado para cálculos financeiros.

### Campos textuais

Nomes e descrições:

- são obrigatórios;
- possuem limites de tamanho;
- não podem ser formados somente por espaços;
- são normalizados antes da gravação.

### Cálculo dos totais

Receitas, despesas e saldos são calculados a partir das transações persistidas.

Os totais não são armazenados separadamente, evitando duplicidade e inconsistência de dados.

### Datas

As datas das transações são armazenadas em UTC com `DateTimeOffset`.

---

## Testes automatizados

O projeto possui:

- **15 testes no back-end**;
- **4 testes no front-end**.

### Cenários cobertos no back-end

A suíte de testes valida:

- criação de pessoa com dados válidos;
- normalização do nome;
- cadastro de despesa para menor de idade;
- bloqueio de receita para menor de idade;
- permissão de receita para pessoa com exatamente 18 anos;
- rejeição de transação para pessoa inexistente;
- exclusão de pessoa;
- exclusão das transações vinculadas;
- tentativa de exclusão de pessoa inexistente;
- totais individuais;
- totais gerais;
- pessoa sem transações;
- persistência entre diferentes instâncias do repositório;
- idade fora do intervalo permitido;
- valor monetário igual ou inferior a zero.

Os testes utilizam arquivos temporários independentes, evitando alterações nos dados reais da aplicação.

### Cenários cobertos no front-end

Os testes verificam:

- regras auxiliares relacionadas à idade;
- comportamento da restrição de receitas;
- formatação monetária;
- formatação de valores exibidos pela interface.

---

## Rastreabilidade dos requisitos

| Requisito | Implementação principal | Validação |
| --- | --- | --- |
| Criar e listar pessoas | `PeopleController` e `JsonHouseholdRepository` | Testes e fluxo da interface |
| Excluir pessoa | `DeletePersonAsync` | Teste automatizado |
| Excluir transações relacionadas | Remoção em cascata no repositório | Teste automatizado |
| Criar e listar transações | `TransactionsController` e repositório | Testes e fluxo da interface |
| Exigir pessoa existente | `AddTransactionAsync` | Teste com identificador inexistente |
| Bloquear receita para menor | Regra no servidor | Testes com 16 e 18 anos |
| Calcular totais individuais | `GetTotalsAsync` | Testes com valores conhecidos |
| Calcular totais gerais | `GetTotalsAsync` | Teste consolidado |
| Exibir pessoa sem transações | Consulta iniciada pela lista de pessoas | Teste com valores zerados |
| Persistir após reiniciar | `JsonHouseholdRepository` | Teste com nova instância |

---

## Pré-requisitos

Para executar o projeto localmente, instale:

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 22 ou superior](https://nodejs.org/)
- Git

O Docker é opcional.

---

## Clonar o repositório

```bash
git clone https://github.com/lucastnunes06/controle-gastos-residenciais.git
cd controle-gastos-residenciais
```

---

## Executar localmente

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

Abra outro terminal na raiz do projeto:

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

> No PowerShell, caso a execução de `npm.ps1` esteja bloqueada, os comandos também podem ser executados como `npm.cmd install`, `npm.cmd run dev` e equivalentes.

---

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

---

## Validar o projeto

### Back-end

Na raiz do repositório:

```bash
dotnet restore LarFinance.slnx
dotnet build LarFinance.slnx --configuration Release
dotnet test LarFinance.slnx --configuration Release
```

Resultado esperado:

```text
15 testes aprovados
0 testes com falha
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

Resultado esperado:

```text
4 testes aprovados
lint sem erros
formatação validada
build de produção concluído
```

No PowerShell, os mesmos comandos podem ser executados com:

```powershell
npm.cmd ci
npm.cmd run lint
npm.cmd run format:check
npm.cmd test
npm.cmd run build
```

---

## Integração contínua

O workflow está localizado em:

```text
.github/workflows/ci.yml
```

Ele é executado automaticamente em pushes e pull requests para a branch `main`.

O pipeline verifica:

1. checkout do código;
2. configuração do .NET;
3. restauração das dependências .NET;
4. build da solução;
5. execução dos testes do back-end;
6. configuração do Node.js;
7. instalação limpa das dependências do front-end;
8. lint do front-end;
9. verificação de formatação;
10. execução dos testes do front-end;
11. build de produção do front-end.

---

## Endpoints da API

| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/people` | Lista todas as pessoas |
| `POST` | `/api/people` | Cadastra uma pessoa |
| `DELETE` | `/api/people/{id}` | Exclui a pessoa e suas transações |
| `GET` | `/api/transactions` | Lista todas as transações |
| `POST` | `/api/transactions` | Cadastra uma transação |
| `GET` | `/api/totals` | Retorna totais individuais e gerais |
| `GET` | `/api/health` | Informa a disponibilidade da API |

Exemplos detalhados de requisições e respostas estão disponíveis em:

- [`docs/API.md`](docs/API.md)
- [`src/LarFinance.Api/LarFinance.Api.http`](src/LarFinance.Api/LarFinance.Api.http)

---

## Exemplos de requisição

### Cadastrar pessoa

```http
POST /api/people
Content-Type: application/json
```

```json
{
  "name": "Ana Souza",
  "age": 30
}
```

### Cadastrar despesa

```http
POST /api/transactions
Content-Type: application/json
```

```json
{
  "description": "Conta de energia",
  "amount": 180.5,
  "type": "Expense",
  "personId": "identificador-da-pessoa"
}
```

### Cadastrar receita

```json
{
  "description": "Salário",
  "amount": 2500,
  "type": "Income",
  "personId": "identificador-da-pessoa"
}
```

Uma tentativa de cadastrar receita para uma pessoa menor de 18 anos será recusada pela API.

---

## Tratamento de erros

A API utiliza respostas padronizadas no formato `ProblemDetails`.

São tratados cenários como:

- dados obrigatórios ausentes;
- nome ou descrição inválidos;
- idade fora do intervalo aceito;
- valor igual ou inferior a zero;
- valor acima do limite permitido;
- tipo de transação inválido;
- pessoa inexistente;
- receita para pessoa menor de idade;
- recurso não encontrado;
- falha inesperada de persistência.

O front-end apresenta mensagens compreensíveis ao usuário sem expor stack traces ou detalhes internos da aplicação.

---

## Decisões técnicas

### Persistência em JSON

A especificação exige que os dados permaneçam disponíveis após o encerramento da aplicação, mas não determina um banco de dados específico.

O JSON foi escolhido para:

- reduzir a configuração necessária;
- facilitar a avaliação do projeto;
- evitar dependências externas;
- permitir execução imediata;
- atender à exigência de persistência.

### Abstração da persistência

O acesso aos dados é realizado por meio de `IHouseholdRepository`.

Essa abstração permite substituir o armazenamento atual por SQLite, Entity Framework Core ou outro mecanismo sem exigir alterações relevantes nos controllers.

### Sincronização

Um `SemaphoreSlim` controla o acesso ao arquivo compartilhado por pessoas e transações.

### Escrita segura

As alterações são gravadas inicialmente em um arquivo temporário. Depois, o arquivo principal é substituído, reduzindo o risco de dados parcialmente escritos.

### Valores financeiros

Valores monetários utilizam `decimal`, evitando problemas de precisão associados a `float` e `double`.

### Totais calculados sob demanda

Os totais são derivados das transações persistidas e não são armazenados separadamente.

### Validação em duas camadas

O front-end fornece feedback imediato, enquanto o back-end continua sendo a autoridade das regras de negócio.

### Escopo controlado

Não foram adicionados autenticação, edição de transações ou outros recursos que aumentariam a complexidade sem contribuir diretamente para os requisitos.

Mais detalhes estão disponíveis em:

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)

---

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
│   │   ├── Models/
│   │   ├── Persistence/
│   │   ├── App_Data/
│   │   └── Program.cs
│   └── LarFinance.Web/
│       ├── public/
│       ├── src/
│       │   ├── services/
│       │   ├── App.tsx
│       │   ├── businessRules.ts
│       │   ├── formatters.ts
│       │   └── types.ts
│       └── package.json
├── tests/
│   └── LarFinance.Tests/
│       ├── HouseholdRepositoryTests.cs
│       └── LarFinance.Tests.csproj
├── .dockerignore
├── .editorconfig
├── .gitignore
├── Dockerfile
├── LarFinance.slnx
├── NuGet.Config
└── README.md
```

---

## Melhorias futuras

Como evoluções além do escopo atual, poderiam ser implementados:

- persistência com SQLite e Entity Framework Core;
- consultas por período;
- filtro de transações por pessoa;
- pesquisa por descrição;
- edição e exclusão de transações;
- paginação para grandes volumes;
- exportação de relatórios;
- testes de integração dos endpoints;
- ampliação dos testes de componentes do front-end;
- observabilidade e health checks mais completos.

Essas melhorias não são necessárias para o atendimento dos requisitos atuais.

---

## Autor

Desenvolvido por **Lucas Nunes**.

- GitHub: [lucastnunes06](https://github.com/lucastnunes06)