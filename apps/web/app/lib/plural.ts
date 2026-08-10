import { LOCALE } from "./locale";

const rules = new Intl.PluralRules(LOCALE);

export function plural(
  count: number,
  forms: Partial<Record<Intl.LDMLPluralRule, string>> & { other: string },
): string {
  return forms[rules.select(count)] ?? forms.other;
}
