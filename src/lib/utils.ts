import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Agrapa`
 * elementos por uma chave (accessor) e conta ocorrencias,
 * retornando pares { name, value } prontos para os graficos.
 */
export function countBy<T>(items: T[], accessor: (item: T) => string) {
  const m: Record<string, number> = {};
  for (const item of items) {
    const key = accessor(item);
    m[key] = (m[key] || 0) + 1;
  }
  return Object.entries(m).map(([name, value]) => ({ name, value }));
}
