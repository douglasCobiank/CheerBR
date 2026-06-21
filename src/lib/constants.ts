export const CATEGORIAS = ["Universitário", "All star", "Escolar", "Outro"] as const;

export const STATUSES = ["Ativo", "Inativo", "Desconhecido"] as const;

export const IMPORTANCIAS = [
  "Internacional",
  "Nacional",
  "Estadual",
  "Regional",
  "Municipal",
] as const;

export const TIPOS_CATEGORIA = [
  "Team Cheer",
  "Categoria de Grupo",
  "Coed",
  "All Girl",
  "All Boy",
  "Elite",
  "Partner / Duplas",
  "Skills Individuais",
  "Best Jump",
  "Best Tumbling",
  "Best Basket",
  "Best Cheer",
] as const;

export const NIVEL_MAX = 6;

export const INPUT_CLASS =
  "w-full rounded-lg border border-border bg-input/60 px-3 py-2 text-sm outline-none focus:border-primary";

export const CHART_COLORS = [
  "oklch(0.68 0.27 350)",
  "oklch(0.78 0.18 200)",
  "oklch(0.83 0.16 90)",
  "oklch(0.66 0.13 55)",
  "oklch(0.55 0.25 320)",
];

export const CHART_TOOLTIP_STYLES = {
  background: "oklch(0.21 0.05 270)",
  border: "1px solid oklch(0.32 0.05 270)",
  borderRadius: 8,
} as const;
