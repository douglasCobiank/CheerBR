import { Link } from "@tanstack/react-router";

const nav = [
  { to: "/", label: "Início" },
  { to: "/equipes", label: "Equipes" },
  { to: "/ranking", label: "Ranking" },
  { to: "/dashboard", label: "Dashboard" },
] as const;

export function SiteHeader() {
  return (
    <header className="sticky top-0 z-40 border-b border-border/60 bg-background/70 backdrop-blur-xl">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6">
        <Link to="/" className="flex items-center gap-2">
          <span className="grid h-9 w-9 place-items-center rounded-lg bg-[image:var(--gradient-hero)] font-display text-lg shadow-[var(--shadow-glow)]">
            C
          </span>
          <div className="leading-tight">
            <div className="font-display text-xl tracking-wider">CHEER BR</div>
            <div className="text-[10px] uppercase tracking-[0.2em] text-muted-foreground">
              Mapa oficial
            </div>
          </div>
        </Link>
        <nav className="flex items-center gap-1 sm:gap-2">
          {nav.map((n) => (
            <Link
              key={n.to}
              to={n.to}
              className="rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition hover:bg-secondary hover:text-foreground"
              activeProps={{ className: "bg-secondary text-foreground" }}
              activeOptions={{ exact: n.to === "/" }}
            >
              {n.label}
            </Link>
          ))}
        </nav>
      </div>
    </header>
  );
}