import { describe, expect, it } from "vitest";
import { money } from "./formatters";

describe("money", () => {
  it("formata valores em real brasileiro com centavos", () => {
    expect(money.format(1234.56)).toContain("1.234,56");
  });

  it("preserva a indica��o de valores negativos", () => {
    expect(money.format(-100)).toContain("-R$");
  });
});
