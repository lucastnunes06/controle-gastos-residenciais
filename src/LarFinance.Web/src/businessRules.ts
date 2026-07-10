/** Regra espelhada apenas para feedback imediato; a API continua sendo a autoridade. */
export function canRegisterIncome(age: number): boolean {
  return age >= 18;
}
