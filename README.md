# LarFinance

[![Continuous Integration](https://github.com/lucastnunes06/controle-gastos-residenciais/actions/workflows/ci.yml/badge.svg)](https://github.com/lucastnunes06/controle-gastos-residenciais/actions/workflows/ci.yml)

Sistema full stack para controle de gastos residenciais, desenvolvido com **ASP.NET Core**, **C#**, **React** e **TypeScript**.

A aplicação permite cadastrar pessoas e transações, acompanhar receitas e despesas e consultar os saldos individuais e gerais da residência. O projeto também inclui validações no servidor, interface responsiva, persistência local, testes automatizados, documentação técnica, Docker e integração contínua.

---

## Funcionalidades

| Área | Funcionalidades |
| --- | --- |
| Pessoas | Cadastro, listagem e exclusão |
| Transações | Cadastro e listagem de receitas e despesas |
| Regras de negócio | Pessoa obrigatória, restrição para menores e exclusão em cascata |
| Totais | Receitas, despesas e saldo por pessoa, além do consolidado geral |
| Experiência | Dashboard, filtros, estados vazios, carregamento e mensagens de erro |
| Qualidade | 15 testes no back-end, 4 testes no front-end, lint, Prettier e CI |
| Documentação | README, documentação da API, arquitetura e exemplos de requisições |

---

## Requisitos atendidos

### Cadastro de pessoas

O sistema permite:

- cadastrar uma pessoa;
- listar as pessoas cadastradas;
- excluir uma pessoa;
- gerar automaticamente um identificador único;
- armazenar nome e idade;
- validar os dados informados.

Ao excluir uma pessoa, todas as transações vinculadas a ela também são removidas.

### Cadastro de transações

O sistema permite:

- cadastrar receitas e despesas;
- listar todas as transações;
- relacionar cada transação a uma pessoa cadastrada;
- gerar automaticamente um identificador único;
- informar descrição, valor e tipo;
- visualizar a pessoa responsável e a data da movimentação.

A edição e a exclusão individual de transações não foram implementadas porque não fazem parte dos requisitos obrigatórios.

### Restrição para menores de idade

Pessoas com menos de 18 anos podem cadastrar somente transações do tipo **despesa**.

A interface antecipa essa restrição ao desabilitar a opção de receita. A regra também é validada no back-end, impedindo que seja contornada por chamadas diretas à API.

Pessoas com exatamente 18 anos já podem cadastrar receitas.

### Consulta de totais

A consulta apresenta, para cada pessoa:

- total de receitas;
- total de despesas;
- saldo individual, calculado por receitas menos despesas.

Também são apresentados:

- total geral de receitas;
- total geral de despesas;
- saldo líquido geral.

Pessoas sem transações permanecem na listagem com valores zerados.

---

## Diferenciais implementados

Além dos requisitos obrigatórios, o projeto possui:

- dashboard com indicadores financeiros;
- resumo do saldo por pessoa;
- visualização das movimentações mais recentes;
- filtro de transações por tipo;
- confirmação antes da exclusão de uma pessoa;
- aviso sobre a remoção das transações vinculadas;
- mensagens de sucesso e erro;
- estados de carregamento;
- estados vazios com orientações ao usuário;
- prevenção de envios duplicados;
- recuperação de falhas de comunicação;
- validações no front-end e no back-end;
- documentação da API;
- endpoint simples de disponibilidade da API;
- layout responsivo;
- navegação por teclado;
- suporte à preferência de redução de movimento;
- execução com Docker;
- testes automatizados;
- pipeline de integração contínua.

---

## Tecnologias

### Back-end

- .NET 10
- ASP.NET Core
- C#
- Swagger e OpenAPI
- `ProblemDetails` para respostas de erro
- persistência local em JSON
- `SemaphoreSlim` para sincronização
- xUnit
- Microsoft.NET.Test.Sdk
- Coverlet Collector

### Front-end

- React 19
- TypeScript 5.9
- Vite 8
- Vitest 4
- ESLint
- Prettier
- Lucide React
- CSS responsivo

### Infraestrutura e qualidade

- Docker
- GitHub Actions
- EditorConfig
- build automatizado
- testes automatizados
- lint
- verificação de formatação

---

## Persistência

Os dados são armazenados localmente no arquivo:

```text
App_Data/larfinance.json
```

O arquivo é criado automaticamente na primeira gravação e mantém os dados disponíveis após o encerramento da aplicação.

A implementação utiliza:

- acesso sincronizado por `SemaphoreSlim`;
- gravação inicial em arquivo temporário;
- substituição do arquivo principal após a gravação;
- serialização de enums como texto;
- abstração por meio de `IHouseholdRepository`.

A escrita em arquivo temporário reduz o risco de deixar o JSON parcialmente gravado caso ocorra uma interrupção durante a operação.

Como o desafio exige persistência, mas não determina um banco de dados específico, o JSON permite que a aplicação seja executada sem configuração de serviços externos ou migrations.

A implementação pode ser substituída futuramente por SQLite, Entity Framework Core ou outro mecanismo de armazenamento sem exigir alterações significativas nos controllers.

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

Pessoas com menos de 18 anos podem registrar somente despesas.

A interface fornece feedback antecipado, mas o back-end continua sendo a autoridade da regra.

### Exclusão em cascata

Quando uma pessoa é excluída, todas as transações relacionadas ao seu identificador também são removidas.

### Valores monetários

As transações exigem valor maior que zero e dentro do limite definido pela aplicação.

Os valores utilizam `decimal`, evitando problemas de precisão associados a `float` e `double`.

### Campos textuais

Nomes e descrições:

- são obrigatórios;
- possuem limites de tamanho;
- não podem conter apenas espaços;
- são normalizados antes da gravação.

### Totais calculados sob demanda

Receitas, despesas e saldos são calculados a partir das transações persistidas.

Os totais não são armazenados separadamente, evitando duplicidade e inconsistência de dados.

### Datas

As datas das transações são armazenadas em UTC utilizando `DateTimeOffset`.

---

## Testes automatizados

O projeto possui:

- **15 testes automatizados no back-end**;
- **4 testes automatizados no front-end**.

### Cobertura do back-end

A suíte do back-end cobre:

- criação de pessoa com dados válidos;
- geração de identificador;
- normalização do nome;
- cadastro de despesa para menor de idade;
- bloqueio de receita para menor de idade;
- permissão de receita para pessoa com exatamente 18 anos;
- rejeição de transação para pessoa inexistente;
- exclusão de pessoa;
- exclusão das transações relacionadas;
- tentativa de exclusão de pessoa inexistente;
- totais individuais;
- totais gerais;
- pessoa sem transações;
- persistência entre instâncias do repositório;
- nome obrigatório;
- idade menor que zero;
- idade acima do limite;
- valor igual a zero;
- valor negativo.

Os testes utilizam arquivos temporários independentes, evitando alterações nos dados reais da aplicação.

### Cobertura do front-end

Os testes do front-end cobrem:

- regras auxiliares relacionadas à idade;
- restrição de receitas;
- formatação monetária;
- formatação dos valores exibidos.

---

## Rastreabilidade dos requisitos

| Requisito | Implementação principal | Validação |
| --- | --- | --- |
| Criar e listar pessoas | `PeopleController` e repositório | Testes e interface |
| Excluir pessoa | `DeletePersonAsync` | Teste automatizado |
| Excluir transações relacionadas | Remoção em cascata no repositório | Teste automatizado |
| Criar e listar transações | `TransactionsController` e repositório | Testes e interface |
| Exigir pessoa existente | `AddTransactionAsync` | Teste com ID inexistente |
| Bloquear receita para menor | Regra no servidor | Testes com 16 e 18 anos |
| Calcular totais individuais | `GetTotalsAsync` | Teste com valores conhecidos |
| Calcular totais gerais | `GetTotalsAsync` | Teste consolidado |
| Incluir pessoa sem transações | Consulta iniciada pelas pessoas | Teste com valores zerados |
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

A API ficará disponível em:

```text
http://localhost:5080
```

A documentação da API poderá ser acessada em:

```text
http://localhost:5080/swagger
```

O documento OpenAPI poderá ser acessado em:

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

A interface ficará disponível em:

```text
http://localhost:5173
```

Durante o desenvolvimento, o Vite encaminha as requisições iniciadas por `/api` para:

```text
http://localhost:5080
```

> Caso o PowerShell bloqueie o arquivo `npm.ps1`, utilize `npm.cmd install`, `npm.cmd run dev` e os demais comandos com `npm.cmd`.

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

A aplicação ficará disponível em:

```text
http://localhost:8080
```

O volume `larfinance-data` mantém os dados mesmo que o contêiner seja removido e criado novamente.

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

No PowerShell, os mesmos comandos podem ser executados como:

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

O pipeline executa:

1. checkout do código;
2. configuração do .NET;
3. restauração das dependências .NET;
4. build da solução;
5. testes do back-end;
6. configuração do Node.js;
7. instalação limpa das dependências do front-end;
8. lint;
9. verificação de formatação;
10. testes do front-end;
11. build de produção.

---

## Endpoints da API

| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/people` | Lista todas as pessoas |
| `POST` | `/api/people` | Cadastra uma pessoa |
| `DELETE` | `/api/people/{id}` | Exclui uma pessoa e suas transações |
| `GET` | `/api/transactions` | Lista todas as transações |
| `POST` | `/api/transactions` | Cadastra uma transação |
| `GET` | `/api/totals` | Retorna os totais individuais e gerais |
| `GET` | `/api/health` | Informa a disponibilidade da API |

Exemplos completos estão disponíveis em:

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

```http
POST /api/transactions
Content-Type: application/json
```

```json
{
  "description": "Salário",
  "amount": 2500,
  "type": "Income",
  "personId": "identificador-da-pessoa"
}
```

Uma tentativa de cadastrar receita para uma pessoa com menos de 18 anos será recusada pela API.

---

## Tratamento de erros

A API utiliza respostas padronizadas no formato `ProblemDetails`.

São tratados cenários como:

- campos obrigatórios ausentes;
- nome ou descrição inválidos;
- idade fora do intervalo permitido;
- valor igual ou inferior a zero;
- valor acima do limite;
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
- facilitar a avaliação;
- evitar dependências externas;
- permitir execução imediata;
- atender à exigência de persistência.

### Abstração da persistência

O acesso aos dados é realizado por meio de `IHouseholdRepository`.

Essa abstração permite substituir o armazenamento atual por SQLite, Entity Framework Core ou outro mecanismo sem alterar significativamente os controllers.

### Sincronização

Um `SemaphoreSlim` controla o acesso ao arquivo compartilhado por pessoas e transações.

### Escrita segura

As alterações são gravadas inicialmente em um arquivo temporário. Depois, o arquivo principal é substituído, reduzindo o risco de dados parcialmente escritos.

### Valores financeiros

Valores monetários utilizam `decimal`, evitando problemas de precisão associados a `float` e `double`.

### Totais calculados sob demanda

Os totais são derivados das transações persistidas e não são armazenados separadamente.

### Validação em duas camadas

O front-end oferece feedback imediato, enquanto o back-end continua sendo a autoridade das regras de negócio.

### Escopo controlado

Não foram adicionados autenticação, edição de transações ou recursos que aumentariam a complexidade sem contribuir diretamente para os requisitos.

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

Como evoluções além do escopo atual, podem ser consideradas:

- persistência com SQLite e Entity Framework Core;
- consultas por período;
- filtro de transações por pessoa;
- pesquisa por descrição;
- ordenação das movimentações;
- edição e exclusão de transações;
- paginação para grandes volumes;
- exportação de relatórios;
- testes de integração dos endpoints;
- ampliação dos testes de componentes;
- health checks mais completos.

Essas melhorias não são necessárias para atender aos requisitos atuais.

---

## Autor

Desenvolvido por **Lucas Nunes**.

- GitHub: [lucastnunes06](https://github.com/lucastnunes06)
