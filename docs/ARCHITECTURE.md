# Arquitetura

## Visão geral

A solução separa apresentação, transporte HTTP, regras e persistência. A interface nunca acessa o arquivo de dados diretamente e não calcula os totais oficiais.

```text
React + TypeScript
        | HTTP/JSON
        ?
ASP.NET Core Controllers
        |
        ?
IHouseholdRepository
        |
        ?
Arquivo JSON persistente
```

## Back-end

Os controllers cuidam do contrato HTTP, validações de entrada e códigos de resposta. As regras que dependem do estado existente ficam no repositório: vínculo com pessoa, restrição de idade, cascata e totais.

`IHouseholdRepository` cria um limite explícito entre aplicação e armazenamento. A escolha atual evita infraestrutura externa, mas preserva uma migração simples para um banco relacional.

### Consistência

Todas as leituras e escritas usam um `SemaphoreSlim`. Uma exclusão de pessoa remove suas transações antes da persistência, dentro da mesma seção crítica. A escrita ocorre primeiro em `.tmp`; somente após a serialização completa o arquivo principal é substituído.

### Respostas

- `201 Created` para cadastros válidos.
- `204 No Content` para exclusão bem-sucedida.
- `400 Bad Request` para formato ou campos inválidos.
- `404 Not Found` para pessoa inexistente na exclusão.
- `422 Unprocessable Entity` quando os dados são válidos, mas violam uma regra do domínio.

## Front-end

A interface possui quatro áreas: visão geral, pessoas, transações e totais. Os dados compartilhados são carregados em paralelo, reduzindo o tempo inicial. Depois de uma mutação, a interface consulta novamente a API, evitando estados divergentes.

A restrição para menores também aparece no formulário para dar feedback imediato, mas a API repete obrigatoriamente a validação. Esse desenho evita que chamadas manuais contornem a regra.

## Trade-offs

### Por que JSON em vez de banco relacional?

Para este escopo, JSON oferece execução imediata, persistência real e ausência de configuração. O custo é não ser apropriado para várias instâncias da API ou um grande volume de escritas. Em produção distribuída, a implementação de `IHouseholdRepository` deveria usar um banco transacional.

### Por que não armazenar os totais?

Totais são dados derivados. Persisti-los criaria duas fontes de verdade e exigiria sincronização a cada alteração. Calculá-los durante a consulta mantém o modelo consistente e é adequado ao volume residencial esperado.

## Evoluções possíveis

Sem alterar as regras atuais, a arquitetura permite adicionar autenticação, categorias, datas escolhidas pelo usuário, paginação, relatórios por período e uma implementação relacional do repositório.
