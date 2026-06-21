import type { Team } from "@/lib/teams-store";
import { Instagram, Facebook, MapPin, User } from "lucide-react";
import { Link } from "@tanstack/react-router";

const tierColor = (n: number | null) => {
  if (!n) return "bg-muted text-muted-foreground";
  if (n >= 5) return "bg-[var(--gold)] text-background";
  if (n >= 4) return "bg-primary text-primary-foreground";
  if (n >= 3) return "bg-accent text-accent-foreground";
  return "bg-secondary text-secondary-foreground";
};

export function TeamCard({ team, rank }: { team: Team; rank?: number }) {
  return (
    <div className="group relative overflow-hidden rounded-2xl border border-border/60 bg-[image:var(--gradient-card)] p-5 shadow-[var(--shadow-card)] transition hover:border-primary/60 hover:shadow-[var(--shadow-glow)]">
      <Link
        to="/equipes/$id"
        params={{ id: String(team.id) }}
        className="absolute inset-0 z-0"
        aria-label={`Ver detalhes de ${team.nome}`}
      />
      {rank !== undefined && (
        <div className="absolute right-4 top-4 font-display text-4xl text-primary/70">
          #{rank}
        </div>
      )}
      <div className="flex items-start gap-3">
        {team.logoUrl ? (
          <img src={team.logoUrl} alt={team.nome} className="h-12 w-12 shrink-0 rounded-xl object-cover border border-border" />
        ) : (
          <span
            className={`grid h-12 w-12 shrink-0 place-items-center rounded-xl font-display text-xl ${tierColor(team.nivel)}`}
          >
            {team.nivel ?? "?"}
          </span>
        )}
        <div className="min-w-0 relative z-10 pointer-events-none">
          <h3 className="truncate font-display text-2xl leading-tight text-foreground group-hover:text-primary transition-colors">
            {team.nome}
          </h3>
          {team.programa && (
            <p className="truncate text-xs text-muted-foreground">
              {team.programa}
            </p>
          )}
        </div>
      </div>

      <div className="mt-4 space-y-1.5 text-sm">
        <p className="flex items-center gap-2 text-muted-foreground">
          <MapPin className="h-3.5 w-3.5" /> {team.cidade}, {team.estado}
        </p>
        {team.coach && (
          <p className="flex items-center gap-2 text-muted-foreground">
            <User className="h-3.5 w-3.5" />
            <span className="truncate">{team.coach}</span>
          </p>
        )}
      </div>

      <div className="mt-4 flex flex-wrap items-center gap-2">
        <span className="rounded-full border border-border bg-secondary/60 px-2.5 py-0.5 text-[10px] uppercase tracking-wider">
          {team.categoria}
        </span>
        <span
          className={`rounded-full px-2.5 py-0.5 text-[10px] uppercase tracking-wider ${
            team.status === "Ativo"
              ? "bg-accent/20 text-accent"
              : team.status === "Inativo"
                ? "bg-destructive/20 text-destructive"
                : "bg-muted text-muted-foreground"
          }`}
        >
          {team.status}
        </span>
        <span className="ml-auto font-display text-lg text-primary">
          {team.score}
        </span>
      </div>

      {(team.instagram || team.facebook) && (
        <div className="mt-3 flex gap-3 border-t border-border/60 pt-3 text-muted-foreground">
          {team.instagram && (
            <a
              href={`https://instagram.com/${team.instagram.replace("@", "")}`}
              target="_blank"
              rel="noreferrer"
              className="relative z-10 flex items-center gap-1 text-xs hover:text-primary cursor-pointer"
            >
              <Instagram className="h-3.5 w-3.5" /> {team.instagram}
            </a>
          )}
          {team.facebook && (
            <a
              href={team.facebook.startsWith("http") ? team.facebook : `https://${team.facebook}`}
              target="_blank"
              rel="noreferrer"
              className="relative z-10 flex items-center gap-1 text-xs hover:text-primary cursor-pointer"
            >
              <Facebook className="h-3.5 w-3.5" /> Facebook
            </a>
          )}
        </div>
      )}
    </div>
  );
}
