export type Person = {
  id: string;
  name: string;
  age: number;
};

export type Tx = {
  id: string;
  description: string;
  amount: number;
  type: "Expense" | "Income";
  personId: string;
  createdAt: string;
};

export type Totals = {
  people: Array<{
    personId: string;
    name: string;
    income: number;
    expenses: number;
    balance: number;
  }>;
  general: {
    income: number;
    expenses: number;
    balance: number;
  };
};

export type Tab = "overview" | "people" | "transactions" | "totals";
