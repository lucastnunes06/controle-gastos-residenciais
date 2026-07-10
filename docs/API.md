# Exemplos da API

## Criar pessoa
`POST /api/people`
```json
{ "name": "Ana Souza", "age": 27 }
```

## Criar transação
`POST /api/transactions`
```json
{ "description": "Supermercado", "amount": 245.90, "type": "Expense", "personId": "UUID-DA-PESSOA" }
```
Tipos aceitos: `Expense` e `Income`. Uma receita para menor retorna `422 Unprocessable Entity` com uma mensagem explicativa.

## Totais
`GET /api/totals`
```json
{
  "people": [{ "personId": "...", "name": "Ana Souza", "income": 5000, "expenses": 245.90, "balance": 4754.10 }],
  "general": { "income": 5000, "expenses": 245.90, "balance": 4754.10 }
}
```
