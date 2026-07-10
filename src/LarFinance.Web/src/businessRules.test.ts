import { describe, expect, it } from "vitest";
import { canRegisterIncome } from "./businessRules";

describe("canRegisterIncome", () => {
  it("bloqueia receita antes dos 18 anos", () => {
    expect(canRegisterIncome(16)).toBe(false);
    expect(canRegisterIncome(17)).toBe(false);
  });

  it("permite receita a partir dos 18 anos", () => {
    expect(canRegisterIncome(18)).toBe(true);
    expect(canRegisterIncome(30)).toBe(true);
  });
});
