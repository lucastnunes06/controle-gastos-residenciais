import { useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { api } from "./services/api";
import { money } from "./formatters";
import type { Person, Tab, Totals, Tx } from "./types";
import {
  ArrowDownRight,
  ArrowUpRight,
  ChartNoAxesCombined,
  LayoutDashboard,
  Plus,
  ReceiptText,
  Trash2,
  Users,
  WalletCards,
  X,
} from "lucide-react";
export default function App() {
  const [tab, setTab] = useState<Tab>("overview"),
    [people, setPeople] = useState<Person[]>([]),
    [txs, setTxs] = useState<Tx[]>([]),
    [totals, setTotals] = useState<Totals>({
      people: [],
      general: { income: 0, expenses: 0, balance: 0 },
    }),
    [modal, setModal] = useState<"person" | "tx" | null>(null),
    [personToDelete, setPersonToDelete] = useState<Person | null>(null),
    [toast, setToast] = useState(""),
    [loadError, setLoadError] = useState(""),
    [loading, setLoading] = useState(true);
  const refresh = async () => {
    setLoadError("");
    try {
      const [p, t, s] = await Promise.all([
        api<Person[]>("/api/people"),
        api<Tx[]>("/api/transactions"),
        api<Totals>("/api/totals"),
      ]);
      setPeople(p);
      setTxs(t);
      setTotals(s);
    } catch (e) {
      const message = (e as Error).message;
      setLoadError(message);
      setToast(message);
    } finally {
      setLoading(false);
    }
  };
  useEffect(() => {
    refresh();
  }, []);
  const show = (m: string) => {
    setToast(m);
    setTimeout(() => setToast(""), 3000);
  };
  const nav = [
    ["overview", "Visão geral", LayoutDashboard],
    ["people", "Pessoas", Users],
    ["transactions", "Transações", ReceiptText],
    ["totals", "Totais", ChartNoAxesCombined],
  ] as const;
  return (
    <div className="shell">
      <aside>
        <div className="brand">
          <span>
            <WalletCards />
          </span>
          <b>LarFinance</b>
        </div>
        <nav>
          {nav.map(([id, label, Icon]) => (
            <button
              className={tab === id ? "active" : ""}
              onClick={() => setTab(id)}
              key={id}
            >
              <Icon />
              {label}
            </button>
          ))}
        </nav>
        <div className="aside-note">
          <b>Finanças em ordem.</b>
          <p>Decisões mais leves começam com números claros.</p>
        </div>
      </aside>
      <main>
        <header>
          <div>
            <p className="eyebrow">CONTROLE RESIDENCIAL</p>
            <h1>
              {tab === "overview"
                ? "Olá, sua casa está aqui."
                : tab === "people"
                  ? "Pessoas da casa"
                  : tab === "transactions"
                    ? "Movimentações"
                    : "Totais da residência"}
            </h1>
            <p>
              {tab === "overview"
                ? "Um retrato simples das finanças de todos."
                : tab === "people"
                  ? "Gerencie quem participa das finanças."
                  : tab === "transactions"
                    ? "Acompanhe cada entrada e saída."
                    : "Compare receitas, despesas e saldos por pessoa."}
            </p>
          </div>
          <div className="actions">
            <button className="secondary" onClick={() => setModal("person")}>
              <Users />
              Nova pessoa
            </button>
            <button
              className="primary"
              onClick={() => setModal("tx")}
              disabled={!people.length}
            >
              <Plus />
              Nova transação
            </button>
          </div>
        </header>
        {loading ? (
          <div className="loading">Organizando os números…</div>
        ) : loadError ? (
          <div className="load-error" role="alert">
            <ReceiptText aria-hidden="true" />
            <h2>Não foi possível carregar os dados</h2>
            <p>{loadError}</p>
            <button className="primary" onClick={refresh}>
              Tentar novamente
            </button>
          </div>
        ) : tab === "overview" ? (
          <Overview totals={totals} txs={txs} people={people} />
        ) : tab === "people" ? (
          <People
            people={people}
            totals={totals}
            onDelete={setPersonToDelete}
          />
        ) : tab === "transactions" ? (
          <Transactions txs={txs} people={people} />
        ) : (
          <TotalsPage totals={totals} />
        )}
      </main>
      {modal === "person" && (
        <PersonModal
          close={() => setModal(null)}
          done={() => {
            setModal(null);
            show("Pessoa cadastrada.");
            refresh();
          }}
        />
      )}
      {modal === "tx" && (
        <TxModal
          people={people}
          close={() => setModal(null)}
          done={() => {
            setModal(null);
            show("Transação cadastrada.");
            refresh();
          }}
        />
      )}
      {personToDelete && (
        <ConfirmDeleteModal
          person={personToDelete}
          close={() => setPersonToDelete(null)}
          confirmed={async () => {
            await api(`/api/people/${personToDelete.id}`, { method: "DELETE" });
            setPersonToDelete(null);
            show("Pessoa removida com suas transações.");
            await refresh();
          }}
        />
      )}
      {toast && (
        <div className="toast" role="status" aria-live="polite">
          {toast}
        </div>
      )}
    </div>
  );
}
function Card({
  label,
  value,
  kind,
}: {
  label: string;
  value: number;
  kind: "income" | "expense" | "balance";
}) {
  const Icon =
    kind === "income"
      ? ArrowUpRight
      : kind === "expense"
        ? ArrowDownRight
        : WalletCards;
  return (
    <article className={`metric ${kind}`}>
      <div>
        <span>{label}</span>
        <strong>{money.format(value)}</strong>
      </div>
      <Icon />
    </article>
  );
}
function Overview({
  totals,
  txs,
  people,
}: {
  totals: Totals;
  txs: Tx[];
  people: Person[];
}) {
  return (
    <>
      <section className="metrics">
        <Card
          label="Receitas totais"
          value={totals.general.income}
          kind="income"
        />
        <Card
          label="Despesas totais"
          value={totals.general.expenses}
          kind="expense"
        />
        <Card
          label="Saldo líquido"
          value={totals.general.balance}
          kind="balance"
        />
      </section>
      <section className="grid">
        <div className="panel">
          <div className="panel-title">
            <div>
              <h2>Saldo por pessoa</h2>
              <p>Receitas menos despesas</p>
            </div>
            <span>{people.length} pessoas</span>
          </div>
          {totals.people.length ? (
            <div className="balance-list">
              {totals.people.map((p) => {
                const max = Math.max(
                  ...totals.people.map((x) => Math.abs(x.balance)),
                  1,
                );
                return (
                  <div className="balance-row" key={p.personId}>
                    <div>
                      <b>{p.name}</b>
                      <small>{money.format(p.income)} em receitas</small>
                    </div>
                    <div className="bar">
                      <i
                        style={{
                          width: `${Math.max((Math.abs(p.balance) / max) * 100, 3)}%`,
                        }}
                      />
                    </div>
                    <strong className={p.balance < 0 ? "negative" : ""}>
                      {money.format(p.balance)}
                    </strong>
                  </div>
                );
              })}
            </div>
          ) : (
            <Empty text="Cadastre a primeira pessoa para começar." />
          )}
        </div>
        <div className="panel recent">
          <div className="panel-title">
            <div>
              <h2>Últimas transações</h2>
              <p>Movimentações mais recentes</p>
            </div>
          </div>
          {txs.slice(0, 5).map((t) => (
            <TxRow
              key={t.id}
              t={t}
              person={people.find((p) => p.id === t.personId)}
            />
          ))}
          {!txs.length && <Empty text="Nenhuma movimentação por enquanto." />}
        </div>
      </section>
    </>
  );
}
function TotalsPage({ totals }: { totals: Totals }) {
  return (
    <>
      <section className="metrics" aria-label="Totais gerais">
        <Card
          label="Receitas gerais"
          value={totals.general.income}
          kind="income"
        />
        <Card
          label="Despesas gerais"
          value={totals.general.expenses}
          kind="expense"
        />
        <Card
          label="Saldo líquido"
          value={totals.general.balance}
          kind="balance"
        />
      </section>
      <div className="panel table-panel">
        <table>
          <thead>
            <tr>
              <th>Pessoa</th>
              <th>Receitas</th>
              <th>Despesas</th>
              <th>Saldo</th>
            </tr>
          </thead>
          <tbody>
            {totals.people.map((person) => (
              <tr key={person.personId}>
                <td>
                  <b>{person.name}</b>
                </td>
                <td className="green">{money.format(person.income)}</td>
                <td className="red">{money.format(person.expenses)}</td>
                <td className={person.balance < 0 ? "red" : "green"}>
                  <b>{money.format(person.balance)}</b>
                </td>
              </tr>
            ))}
          </tbody>
          {totals.people.length > 0 && (
            <tfoot>
              <tr>
                <th>Total geral</th>
                <th>{money.format(totals.general.income)}</th>
                <th>{money.format(totals.general.expenses)}</th>
                <th>{money.format(totals.general.balance)}</th>
              </tr>
            </tfoot>
          )}
        </table>
        {!totals.people.length && (
          <Empty text="Cadastre uma pessoa para visualizar os totais da residência." />
        )}
      </div>
    </>
  );
}
function People({
  people,
  totals,
  onDelete,
}: {
  people: Person[];
  totals: Totals;
  onDelete: (p: Person) => void;
}) {
  return (
    <div className="panel table-panel">
      <table>
        <thead>
          <tr>
            <th>Pessoa</th>
            <th>Idade</th>
            <th>Receitas</th>
            <th>Despesas</th>
            <th>Saldo</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {people.map((p) => {
            const t = totals.people.find((x) => x.personId === p.id);
            return (
              <tr key={p.id}>
                <td>
                  <div className="person">
                    <span>{p.name[0].toUpperCase()}</span>
                    <b>{p.name}</b>
                  </div>
                </td>
                <td>
                  {p.age} anos {p.age < 18 && <em>menor</em>}
                </td>
                <td className="green">{money.format(t?.income || 0)}</td>
                <td className="red">{money.format(t?.expenses || 0)}</td>
                <td>
                  <b>{money.format(t?.balance || 0)}</b>
                </td>
                <td>
                  <button
                    className="icon"
                    aria-label={`Excluir ${p.name}`}
                    onClick={() => onDelete(p)}
                  >
                    <Trash2 />
                  </button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
      {!people.length && <Empty text="Ainda não há pessoas cadastradas." />}
    </div>
  );
}
function Transactions({ txs, people }: { txs: Tx[]; people: Person[] }) {
  const [filter, setFilter] = useState("all");
  const visible = useMemo(
    () => txs.filter((t) => filter === "all" || t.type === filter),
    [txs, filter],
  );
  return (
    <div className="panel table-panel">
      <div className="filters">
        <button
          className={filter === "all" ? "selected" : ""}
          onClick={() => setFilter("all")}
        >
          Todas
        </button>
        <button
          className={filter === "Income" ? "selected" : ""}
          onClick={() => setFilter("Income")}
        >
          Receitas
        </button>
        <button
          className={filter === "Expense" ? "selected" : ""}
          onClick={() => setFilter("Expense")}
        >
          Despesas
        </button>
      </div>
      <table>
        <thead>
          <tr>
            <th>Descrição</th>
            <th>Pessoa</th>
            <th>Data</th>
            <th>Tipo</th>
            <th>Valor</th>
          </tr>
        </thead>
        <tbody>
          {visible.map((t) => (
            <tr key={t.id}>
              <td>
                <b>{t.description}</b>
              </td>
              <td>{people.find((p) => p.id === t.personId)?.name || "—"}</td>
              <td>{new Date(t.createdAt).toLocaleDateString("pt-BR")}</td>
              <td>
                <span className={`tag ${t.type}`}>
                  {t.type === "Income" ? "Receita" : "Despesa"}
                </span>
              </td>
              <td className={t.type === "Income" ? "green" : "red"}>
                <b>
                  {t.type === "Income" ? "+" : "-"} {money.format(t.amount)}
                </b>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {!visible.length && <Empty text="Nenhuma transação neste filtro." />}
    </div>
  );
}
function TxRow({ t, person }: { t: Tx; person?: Person }) {
  return (
    <div className="tx-row">
      <span className={t.type}>
        <>{t.type === "Income" ? <ArrowUpRight /> : <ArrowDownRight />}</>
      </span>
      <div>
        <b>{t.description}</b>
        <small>{person?.name || "—"}</small>
      </div>
      <strong className={t.type === "Income" ? "green" : "red"}>
        {t.type === "Income" ? "+" : "-"} {money.format(t.amount)}
      </strong>
    </div>
  );
}
function Empty({ text }: { text: string }) {
  return (
    <div className="empty">
      <ReceiptText />
      <p>{text}</p>
    </div>
  );
}
function Modal({
  title,
  close,
  children,
  eyebrow = "NOVO CADASTRO",
}: {
  title: string;
  close: () => void;
  children: ReactNode;
  eyebrow?: string;
}) {
  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") close();
    };

    document.addEventListener("keydown", handleEscape);
    return () => document.removeEventListener("keydown", handleEscape);
  }, [close]);

  return (
    <div
      className="backdrop"
      onMouseDown={(event) => event.target === event.currentTarget && close()}
    >
      <div
        className="modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
      >
        <div className="modal-head">
          <div>
            <p className="eyebrow">{eyebrow}</p>
            <h2 id="modal-title">{title}</h2>
          </div>
          <button className="icon" onClick={close} aria-label="Fechar janela">
            <X aria-hidden="true" />
          </button>
        </div>
        {children}
      </div>
    </div>
  );
}
function ConfirmDeleteModal({
  person,
  close,
  confirmed,
}: {
  person: Person;
  close: () => void;
  confirmed: () => Promise<void>;
}) {
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  return (
    <Modal
      title={`Excluir ${person.name}?`}
      eyebrow="AÇÃO IRREVERSÍVEL"
      close={close}
    >
      <p className="confirm-copy">
        Ao excluir esta pessoa, todas as transações relacionadas também serão
        removidas. Esta ação não poderá ser desfeita.
      </p>
      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}
      <div className="form-actions">
        <button
          type="button"
          className="secondary"
          onClick={close}
          disabled={submitting}
        >
          Manter pessoa
        </button>
        <button
          type="button"
          className="danger-button"
          disabled={submitting}
          onClick={async () => {
            setSubmitting(true);
            setError("");
            try {
              await confirmed();
            } catch (reason) {
              setError((reason as Error).message);
              setSubmitting(false);
            }
          }}
        >
          {submitting ? "Excluindo…" : "Excluir pessoa"}
        </button>
      </div>
    </Modal>
  );
}
function PersonModal({ close, done }: { close: () => void; done: () => void }) {
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  return (
    <Modal title="Adicionar pessoa" close={close}>
      <form
        onSubmit={async (event) => {
          event.preventDefault();
          const form = new FormData(event.currentTarget);
          setSubmitting(true);
          setError("");

          try {
            await api("/api/people", {
              method: "POST",
              body: JSON.stringify({
                name: form.get("name"),
                age: Number(form.get("age")),
              }),
            });
            done();
          } catch (reason) {
            setError((reason as Error).message);
            setSubmitting(false);
          }
        }}
      >
        <label>
          Nome completo
          <input
            name="name"
            minLength={2}
            maxLength={120}
            placeholder="Ex.: Marina Silva"
            required
            autoFocus
            disabled={submitting}
          />
        </label>
        <label>
          Idade
          <input
            name="age"
            type="number"
            min="0"
            max="130"
            placeholder="Ex.: 32"
            required
            disabled={submitting}
          />
        </label>
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
        <div className="form-actions">
          <button
            type="button"
            className="secondary"
            onClick={close}
            disabled={submitting}
          >
            Cancelar
          </button>
          <button className="primary" disabled={submitting}>
            {submitting ? "Cadastrando…" : "Cadastrar pessoa"}
          </button>
        </div>
      </form>
    </Modal>
  );
}

function TxModal({
  people,
  close,
  done,
}: {
  people: Person[];
  close: () => void;
  done: () => void;
}) {
  const [personId, setPersonId] = useState(people[0]?.id ?? "");
  const [type, setType] = useState<"Expense" | "Income">("Expense");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const minor =
    (people.find((person) => person.id === personId)?.age ?? 18) < 18;

  return (
    <Modal title="Adicionar transação" close={close}>
      <form
        onSubmit={async (event) => {
          event.preventDefault();
          const form = new FormData(event.currentTarget);
          setSubmitting(true);
          setError("");

          try {
            await api("/api/transactions", {
              method: "POST",
              body: JSON.stringify({
                description: form.get("description"),
                amount: Number(form.get("amount")),
                type,
                personId,
              }),
            });
            done();
          } catch (reason) {
            setError((reason as Error).message);
            setSubmitting(false);
          }
        }}
      >
        <label>
          Descrição
          <input
            name="description"
            minLength={2}
            maxLength={160}
            placeholder="Ex.: Conta de energia"
            required
            autoFocus
            disabled={submitting}
          />
        </label>
        <div className="two">
          <label>
            Valor (R$)
            <input
              name="amount"
              type="number"
              min="0.01"
              step="0.01"
              placeholder="0,00"
              required
              disabled={submitting}
            />
          </label>
          <label>
            Pessoa
            <select
              value={personId}
              disabled={submitting}
              onChange={(event) => {
                const nextPerson = people.find(
                  (person) => person.id === event.target.value,
                );
                setPersonId(event.target.value);
                if (nextPerson && nextPerson.age < 18) setType("Expense");
              }}
            >
              {people.map((person) => (
                <option key={person.id} value={person.id}>
                  {person.name}
                </option>
              ))}
            </select>
          </label>
        </div>
        <fieldset disabled={submitting}>
          <legend>Tipo</legend>
          <div className="type-pick">
            <button
              type="button"
              className={type === "Expense" ? "picked expense" : ""}
              onClick={() => setType("Expense")}
            >
              <ArrowDownRight aria-hidden="true" />
              Despesa
            </button>
            <button
              type="button"
              disabled={minor || submitting}
              title={minor ? "Menores só podem cadastrar despesas" : ""}
              className={type === "Income" ? "picked income" : ""}
              onClick={() => setType("Income")}
            >
              <ArrowUpRight aria-hidden="true" />
              Receita
            </button>
          </div>
          {minor && (
            <small className="hint">
              Menores de 18 anos podem registrar apenas despesas.
            </small>
          )}
        </fieldset>
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
        <div className="form-actions">
          <button
            type="button"
            className="secondary"
            onClick={close}
            disabled={submitting}
          >
            Cancelar
          </button>
          <button className="primary" disabled={submitting || !personId}>
            {submitting ? "Cadastrando…" : "Cadastrar transação"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
