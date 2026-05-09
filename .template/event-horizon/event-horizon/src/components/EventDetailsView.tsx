import { useEffect, useMemo, useState } from "react";
import { motion, AnimatePresence } from "motion/react";
import {
  ArrowLeft,
  CheckCircle2,
  ChevronDown,
  Clock,
  Diamond,
  MapPin,
  Search,
  ShoppingCart,
  Star,
  Users,
  X,
} from "lucide-react";

type UiEventDetails = {
  id: number;
  title: string;
  date: string;
  location: string;
  image?: string | null;
};

type ApiEventDetails = {
  id: number;
  nome: string;
  data: string | null;
  horaInicio: string | null;
  local: string | null;
  descricao: string | null;
  capacidadeMax: number | null;
  categoria: { id: number; nome: string } | null;
  imageUrl: string | null;
};

type ApiActivity = {
  id: number;
  nome: string;
  local: string;
  capacidade: number;
  categoria?: { id: number; nome: string };
};

type UiActivity = {
  id: number;
  name: string;
  location: string;
  capacity: number;
  category: string;
  time: string;
};

type TicketOffer = {
  idBilheteEvento: number;
  nomeBilhete: string;
  tipoBilhete: string;
  descricaoAcesso: string;
  preco: number;
  quantidadeDisponivel: number;
  esgotado: boolean;
};

type TicketsResponse = {
  jaInscrito: boolean;
  idBilheteAtivo: number | null;
  ofertas: TicketOffer[];
};

const DEFAULT_EVENT_IMAGE =
  "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=2000&auto=format&fit=crop";

const MONTHS_PT = ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"] as const;

function pad2(n: number) {
  return String(n).padStart(2, "0");
}

function timeByIndex(i: number) {
  const base = 18 * 60;
  const t = base + i * 60;
  return `${pad2(Math.floor(t / 60) % 24)}:${pad2(t % 60)}`;
}

function formatDate(dateIso: string | null, fallback: string) {
  if (!dateIso) return fallback || "-";
  const [y, m, d] = dateIso.split("-").map((part) => Number(part));
  if (!y || !m || !d) return fallback || "-";
  return `${pad2(d)} ${MONTHS_PT[m - 1]} ${y}`;
}

function formatPrice(value: number) {
  return new Intl.NumberFormat("pt-PT", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function normalizeTicketType(type: string) {
  return type.trim().toUpperCase();
}

function ticketIcon(type: string) {
  const normalized = normalizeTicketType(type);
  if (normalized === "GOLD") return <Star className="w-5 h-5 fill-current" />;
  if (normalized === "VIP") return <Diamond className="w-5 h-5" />;
  return <CheckCircle2 className="w-5 h-5" />;
}

function ticketClasses(type: string) {
  const normalized = normalizeTicketType(type);
  if (normalized === "GOLD") {
    return {
      card: "from-yellow-400/10 border-yellow-400/30 hover:from-yellow-400/20 shadow-[0_0_30px_rgba(250,204,21,0.06)]",
      icon: "bg-yellow-400/20 border-yellow-400/50 text-yellow-400",
      title: "text-yellow-400",
      price: "border-yellow-400/30 text-yellow-400",
      button: "bg-yellow-400 text-black hover:bg-yellow-300 shadow-[0_0_15px_rgba(250,204,21,0.2)]",
    };
  }

  if (normalized === "VIP") {
    return {
      card: "from-indigo-500/10 border-indigo-500/30 hover:from-indigo-500/20",
      icon: "bg-indigo-500/20 border-indigo-500/50 text-indigo-400",
      title: "text-indigo-400",
      price: "border-indigo-500/30 text-indigo-400",
      button: "bg-indigo-500 text-white hover:bg-indigo-400 shadow-[0_0_15px_rgba(99,102,241,0.2)]",
    };
  }

  return {
    card: "from-white/[0.03] border-white/10 hover:from-white/[0.05]",
    icon: "bg-[#111111] border-white/10 text-white/40",
    title: "text-white",
    price: "border-white/10 text-white",
    button: "bg-white text-black hover:bg-yellow-300",
  };
}

export function EventDetailsView({ event, onBack }: { event: UiEventDetails; onBack: () => void }) {
  const [details, setDetails] = useState<ApiEventDetails | null>(null);
  const [tickets, setTickets] = useState<TicketsResponse>({ jaInscrito: false, idBilheteAtivo: null, ofertas: [] });
  const [activities, setActivities] = useState<UiActivity[]>([]);
  const [showFilters, setShowFilters] = useState(false);
  const [searchName, setSearchName] = useState("");
  const [searchLocation, setSearchLocation] = useState("");
  const [sortOrder, setSortOrder] = useState<"time_asc" | "time_desc" | "alpha">("time_asc");

  useEffect(() => {
    let cancelled = false;

    fetch(`/api/eventos/${event.id}`, { credentials: "include" })
      .then(async (r) => (r.ok ? ((await r.json()) as ApiEventDetails) : null))
      .then((data) => {
        if (!cancelled) setDetails(data);
      })
      .catch(() => {
        if (!cancelled) setDetails(null);
      });

    fetch(`/api/eventos/${event.id}/bilhetes`, { credentials: "include" })
      .then(async (r) => (r.ok ? ((await r.json()) as TicketsResponse) : null))
      .then((data) => {
        if (!cancelled && data) setTickets(data);
      })
      .catch(() => {
        if (!cancelled) setTickets({ jaInscrito: false, idBilheteAtivo: null, ofertas: [] });
      });

    fetch(`/api/eventos/${event.id}/atividades`, { credentials: "include" })
      .then(async (r) => {
        if (!r.ok) return [];
        const json = await r.json();
        return Array.isArray(json) ? (json as ApiActivity[]) : [];
      })
      .then((list) => {
        if (cancelled) return;
        setActivities(
          list.map((activity, index) => ({
            id: activity.id,
            name: activity.nome,
            location: activity.local || "-",
            capacity: activity.capacidade,
            category: activity.categoria?.nome ?? "Atividade",
            time: timeByIndex(index),
          })),
        );
      })
      .catch(() => {
        if (!cancelled) setActivities([]);
      });

    return () => {
      cancelled = true;
    };
  }, [event.id]);

  const title = details?.nome ?? event.title;
  const image = details?.imageUrl || event.image || DEFAULT_EVENT_IMAGE;
  const category = details?.categoria?.nome ?? "Evento";
  const date = formatDate(details?.data ?? null, event.date);
  const hour = details?.horaInicio ?? "Hora a definir";
  const location = details?.local || event.location || "Local a definir";
  const description = details?.descricao || `${title} at ${location}`;
  const capacity = details?.capacidadeMax?.toString() ?? "Sem limite definido";

  const filteredActivities = useMemo(() => {
    let result = [...activities];

    if (searchName) {
      result = result.filter((activity) => activity.name.toLowerCase().includes(searchName.toLowerCase()));
    }

    if (searchLocation) {
      result = result.filter((activity) => activity.location.toLowerCase().includes(searchLocation.toLowerCase()));
    }

    result.sort((a, b) => {
      if (sortOrder === "alpha") return a.name.localeCompare(b.name);

      const timeA = Number(a.time.replace(":", ""));
      const timeB = Number(b.time.replace(":", ""));
      return sortOrder === "time_asc" ? timeA - timeB : timeB - timeA;
    });

    return result;
  }, [activities, searchName, searchLocation, sortOrder]);

  return (
    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="w-full min-h-screen pb-32">
      <div className="max-w-[1600px] mx-auto px-4 md:px-10 pt-32">
        <section className="relative overflow-hidden rounded-[30px] border border-white/10 bg-[#0a0a0a]/60 shadow-2xl backdrop-blur-xl">
          <div className="absolute inset-0 pointer-events-none">
            <img src={image} alt={title} className="w-full h-full object-cover grayscale opacity-25" />
            <div className="absolute inset-0 bg-gradient-to-r from-black/85 via-black/55 to-black/35" />
            <div className="absolute inset-x-0 bottom-0 h-64 bg-gradient-to-t from-[#0a0a0a] to-transparent" />
          </div>

          <div className="relative z-10 p-8 md:p-16">
            <button
              type="button"
              onClick={onBack}
              className="mb-12 flex items-center gap-2 font-mono text-sm uppercase tracking-widest text-white/50 transition-colors hover:text-yellow-400 group"
            >
              <ArrowLeft className="w-4 h-4 transition-transform group-hover:-translate-x-1" />
              Voltar para eventos
            </button>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 mb-20 pb-20 border-b border-white/5">
              <div>
                <div className="flex gap-2 flex-wrap mb-4">
                  <span className="px-3 py-1 rounded-full bg-yellow-400 text-black text-[10px] font-black uppercase tracking-widest">
                    {category}
                  </span>
                </div>

                <h1 className="mb-12 text-4xl md:text-6xl font-medium tracking-tighter leading-none uppercase">{title}</h1>

                <h2 className="mb-6 text-2xl font-bold tracking-tighter uppercase">Informacoes do evento</h2>

                <div className="flex flex-col gap-4">
                  {[
                    ["Data", date],
                    ["Hora", hour],
                    ["Local", location],
                    ["Descricao", description],
                    ["Capacidade maxima", capacity],
                    ["Categoria", category],
                  ].map(([label, value]) => (
                    <div key={label} className="grid grid-cols-3 gap-4 border-b border-white/5 pb-4">
                      <span className="text-white/70 font-bold uppercase tracking-widest text-xs">{label}</span>
                      <span className="col-span-2 text-white font-mono text-sm break-words">{value}</span>
                    </div>
                  ))}
                </div>
              </div>

              <aside className="flex flex-col">
                <div className="flex items-center justify-between gap-4 mb-2">
                  <h2 className="text-3xl font-bold tracking-tighter uppercase">Comprar bilhete</h2>
                  <ShoppingCart className="w-7 h-7 text-yellow-400" />
                </div>
                <p className="text-white/50 text-sm font-mono tracking-wide mb-8">
                  Escolhe o acesso que melhor combina com a tua experiencia.
                </p>

                <div className="flex flex-col gap-4">
                  {tickets.ofertas.length === 0 ? (
                    <div className="rounded-2xl border border-white/10 bg-white/[0.03] p-6 text-white/50 font-mono text-sm uppercase tracking-widest">
                      Bilhetes indisponiveis
                    </div>
                  ) : (
                    tickets.ofertas.map((ticket) => {
                      const classes = ticketClasses(ticket.tipoBilhete);
                      const unavailable = tickets.jaInscrito || ticket.esgotado;

                      return (
                        <article
                          key={ticket.idBilheteEvento}
                          className={`relative overflow-hidden rounded-2xl border bg-gradient-to-r p-6 transition-colors ${classes.card} ${
                            unavailable ? "opacity-70" : ""
                          }`}
                        >
                          <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4 mb-4">
                            <div className="flex items-center gap-4 min-w-0">
                              <div
                                className={`w-12 h-12 shrink-0 rounded-full border flex items-center justify-center ${classes.icon}`}
                              >
                                {ticketIcon(ticket.tipoBilhete)}
                              </div>
                              <div className="min-w-0">
                                <h3 className={`text-xl font-bold tracking-tighter uppercase ${classes.title}`}>
                                  {ticket.nomeBilhete}
                                </h3>
                                <p className="text-white/50 text-xs font-mono uppercase tracking-widest">{ticket.tipoBilhete}</p>
                              </div>
                            </div>
                            <div className={`bg-black/50 px-4 py-2 rounded-full border backdrop-blur-md ${classes.price}`}>
                              <span className="font-bold text-sm whitespace-nowrap">{formatPrice(ticket.preco)} EUR</span>
                            </div>
                          </div>

                          <p className="text-white/55 text-sm font-mono leading-relaxed">{ticket.descricaoAcesso}</p>

                          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mt-6">
                            <span className="inline-flex items-center gap-2 rounded-full border border-white/5 bg-white/5 px-3 py-1 text-white/35 text-xs font-mono tracking-widest">
                              <span className="w-1.5 h-1.5 rounded-full bg-yellow-400/60" />
                              {ticket.esgotado ? "Esgotado" : `${ticket.quantidadeDisponivel} disponiveis`}
                            </span>

                            {tickets.jaInscrito ? (
                              <button
                                type="button"
                                disabled
                                className="rounded-full bg-white/10 px-6 py-2 text-xs font-bold uppercase tracking-widest text-white/50 cursor-not-allowed"
                              >
                                Inscricao ativa
                              </button>
                            ) : ticket.esgotado ? (
                              <button
                                type="button"
                                disabled
                                className="rounded-full bg-white/10 px-6 py-2 text-xs font-bold uppercase tracking-widest text-white/50 cursor-not-allowed"
                              >
                                Sem disponibilidade
                              </button>
                            ) : (
                              <a
                                href={`/Bilhete/Checkout/${ticket.idBilheteEvento}`}
                                className={`rounded-full px-6 py-2 text-xs font-bold uppercase tracking-widest transition-transform hover:scale-105 ${classes.button}`}
                              >
                                Comprar
                              </a>
                            )}
                          </div>
                        </article>
                      );
                    })
                  )}
                </div>
              </aside>
            </div>

            <section>
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-8 border-b border-white/10 pb-6">
                <div className="flex items-center gap-6">
                  <h2 className="text-3xl font-bold tracking-tighter uppercase">Atividades</h2>
                  <div className="text-yellow-400/80 font-mono text-xs uppercase tracking-widest hidden sm:block">
                    {filteredActivities.length} encontradas
                  </div>
                </div>

                <div className="flex items-center gap-4 flex-wrap">
                  <div className="relative min-w-[200px] shrink-0 group border border-white/20 rounded-full focus-within:border-yellow-400 focus-within:bg-white/[0.02] transition-colors h-10">
                    <select
                      className="w-full h-full bg-transparent rounded-full pl-5 pr-10 text-xs text-white font-mono tracking-widest uppercase focus:outline-none appearance-none cursor-pointer relative z-10"
                      value={sortOrder}
                      onChange={(e) => setSortOrder(e.target.value as typeof sortOrder)}
                    >
                      <option value="time_asc" className="bg-[#1a1a1a]">
                        Hora crescente
                      </option>
                      <option value="time_desc" className="bg-[#1a1a1a]">
                        Hora decrescente
                      </option>
                      <option value="alpha" className="bg-[#1a1a1a]">
                        A-Z
                      </option>
                    </select>
                    <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-white/50 pointer-events-none z-20 group-focus-within:text-yellow-400 transition-colors" />
                  </div>

                  <button
                    type="button"
                    onClick={() => setShowFilters(!showFilters)}
                    className={`flex items-center gap-2 h-10 px-4 rounded-full border text-xs font-bold font-mono tracking-widest uppercase transition-colors ${
                      showFilters
                        ? "bg-yellow-400 text-black border-yellow-400 shadow-[0_0_15px_rgba(250,204,21,0.3)]"
                        : "bg-transparent text-white border-white/20 hover:border-white/60"
                    }`}
                  >
                    <Search className="w-4 h-4" />
                    {showFilters ? "Esconder" : "Pesquisar"}
                  </button>
                </div>
              </div>

              <AnimatePresence>
                {showFilters && (
                  <motion.div
                    initial={{ height: 0, opacity: 0, y: -10 }}
                    animate={{ height: "auto", opacity: 1, y: 0 }}
                    exit={{ height: 0, opacity: 0, y: -10 }}
                    className="overflow-hidden"
                  >
                    <div className="flex flex-row items-center gap-3 mb-10 overflow-x-auto w-full pb-3 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden shrink-0">
                      <input
                        type="text"
                        placeholder="Pesquisar por nome da atividade..."
                        value={searchName}
                        onChange={(e) => setSearchName(e.target.value)}
                        className="min-w-[250px] flex-1 bg-transparent border border-white/20 rounded-full px-5 py-2.5 text-xs text-white placeholder-white/50 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
                        autoFocus
                      />

                      <input
                        type="text"
                        placeholder="Pesquisar por local/palco..."
                        value={searchLocation}
                        onChange={(e) => setSearchLocation(e.target.value)}
                        className="min-w-[200px] flex-1 bg-transparent border border-white/20 rounded-full px-5 py-2.5 text-xs text-white placeholder-white/50 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
                      />

                      <button
                        type="button"
                        onClick={() => {
                          setSearchName("");
                          setSearchLocation("");
                        }}
                        className="shrink-0 flex items-center justify-center w-10 h-10 bg-transparent border border-white/20 rounded-full text-white/60 hover:bg-white/10 hover:text-white hover:border-white transition-all group"
                        aria-label="Limpar filtros"
                        title="Limpar filtros"
                      >
                        <X className="w-5 h-5 group-hover:scale-110 transition-transform duration-300" />
                      </button>
                    </div>
                  </motion.div>
                )}
              </AnimatePresence>

              <div className="flex flex-col border-t border-white/5">
                {filteredActivities.length === 0 ? (
                  <div className="py-20 text-center text-white/40 font-mono tracking-widest text-sm uppercase">
                    Nenhuma atividade encontrada
                  </div>
                ) : (
                  filteredActivities.map((activity, index) => (
                    <motion.article
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: index * 0.05 }}
                      key={activity.id}
                      className="group border-b border-white/5 py-8 flex flex-col md:flex-row md:items-center justify-between gap-6 hover:bg-white/[0.02] transition-colors md:px-6 md:-mx-6 rounded-xl"
                    >
                      <div className="flex items-center gap-6 md:gap-10 w-full md:flex-1">
                        <span className="text-yellow-400 font-mono text-xl font-bold flex items-center gap-2">
                          <Clock className="w-5 h-5 opacity-50" />
                          {activity.time}
                        </span>
                        <div>
                          <h3 className="text-2xl md:text-3xl font-bold tracking-tighter uppercase group-hover:text-yellow-400 transition-colors mb-2">
                            {activity.name}
                          </h3>
                          <div className="flex flex-wrap items-center gap-4 text-white/40 font-mono text-xs uppercase tracking-widest">
                            <span className="flex items-center gap-1">
                              <MapPin className="w-3.5 h-3.5" />
                              {activity.location}
                            </span>
                            <span>&bull;</span>
                            <span className="flex items-center gap-1">
                              <Users className="w-3.5 h-3.5" />
                              Cap: {activity.capacity}
                            </span>
                            <span>&bull;</span>
                            <span>{activity.category}</span>
                          </div>
                        </div>
                      </div>
                      <div className="w-full md:w-auto mt-2 md:mt-0">
                        <button className="w-full md:w-auto px-6 py-3 border border-white/10 rounded-full text-xs font-bold font-mono uppercase tracking-widest text-white hover:bg-white hover:text-black transition-colors focus:outline-none">
                          Inscrever
                        </button>
                      </div>
                    </motion.article>
                  ))
                )}
              </div>
            </section>
          </div>
        </section>
      </div>
    </motion.div>
  );
}
