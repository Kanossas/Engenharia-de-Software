/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import { useMemo, useState, useEffect } from "react";
import { motion, AnimatePresence } from "motion/react";
import { ChevronDown, ArrowRight, ArrowLeft, Circle, Info, Layers, Users, Zap } from "lucide-react";
import { EventsView, type UiEvent } from "./components/EventsView";
import { CreateEventView } from "./components/CreateEventView";
import { EventDetailsView } from "./components/EventDetailsView";

const POPULAR_EVENTS = [
  {
    title: "NEON DYSTOPIA",
    date: "14 Jul 2026",
    location: "LX Factory, Lisboa",
    image: "https://www.fazemag.de/wp-content/uploads/2025/01/Holy_Priest_Facebook.jpg",
    activities: [
      { id: 1, name: "Holy Priest", type: "Live AV", time: "02:00" },
      { id: 2, name: "Sara Landry", type: "Main Stage", time: "00:00" },
      { id: 3, name: "Nico Moreno", type: "Closing", time: "04:00" }
    ]
  },
  {
    title: "SYNTHESIS CORE",
    date: "28 Ago 2026",
    location: "Pavilhão Carlos Lopes",
    image: "https://images.unsplash.com/photo-1574391884720-bbc3740c59d1?q=80&w=2000&auto=format&fit=crop",
    activities: [
      { id: 4, name: "Paula Temple", type: "Warehouse", time: "03:00" },
      { id: 5, name: "SNTS", type: "Live Set", time: "01:00" },
      { id: 6, name: "I Hate Models", type: "Main Stage", time: "05:00" }
    ]
  },
  {
    title: "VOID FREQUENCIES",
    date: "15 Set 2026",
    location: "Gare, Porto",
    image: "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=2000&auto=format&fit=crop",
    activities: [
      { id: 7, name: "Tale of Us", type: "Garden", time: "02:00" },
      { id: 8, name: "Dixon", type: "All Night", time: "23:00" },
      { id: 9, name: "Mind Against", type: "Sunset", time: "20:00" }
    ]
  }
];

type Page = "home" | "events" | "create_event" | "event_details";

type ApiEvent = {
  id: number;
  nome: string;
  data: string | null; // yyyy-MM-dd
  horaInicio: string | null; // HH:mm
  local: string | null;
  descricao: string | null;
  capacidadeMax: number | null;
  categoria: { id: number; nome: string } | null;
  imageUrl: string | null;
};

type AppEvent = UiEvent & {
  dateIso: string | null;
  locationRaw: string | null;
};

type FeaturedActivity = { id: number; name: string; type: string; time: string };

const DEFAULT_EVENT_IMAGE =
  "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=2000&auto=format&fit=crop";

const MONTHS_PT = ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"] as const;

function formatDateHome(dateIso: string | null) {
  if (!dateIso) return "-";
  const [y, m, d] = dateIso.split("-").map((x) => Number(x));
  if (!y || !m || !d) return "-";
  return `${String(d).padStart(2, "0")} ${MONTHS_PT[m - 1]} ${y}`;
}

function formatDateList(dateIso: string | null) {
  if (!dateIso) return "-";
  const [y, m, d] = dateIso.split("-").map((x) => Number(x));
  if (!y || !m || !d) return "-";
  return `${String(d).padStart(2, "0")} ${MONTHS_PT[m - 1].toUpperCase()} ${y}`;
}

function pad2(n: number) {
  return String(n).padStart(2, "0");
}

function timeByIndex(i: number) {
  const base = 18 * 60;
  const t = base + i * 60;
  const hh = Math.floor(t / 60) % 24;
  const mm = t % 60;
  return `${pad2(hh)}:${pad2(mm)}`;
}

export default function App() {
  const initialPage = (window as any).__EH_INITIAL_PAGE__ as Page | undefined;
  const [currentPage, setCurrentPage] = useState<Page>(initialPage ?? "home");
  const [selectedEvent, setSelectedEvent] = useState<AppEvent | null>(null);
  const [activeSlide, setActiveSlide] = useState(0);
  const [isScrolled, setIsScrolled] = useState(false);

  const [auth, setAuth] = useState<{ isAuthenticated: boolean; userName: string | null }>({
    isAuthenticated: false,
    userName: null,
  });

  const [events, setEvents] = useState<AppEvent[]>([]);
  const popularEvents = useMemo(() => events.slice(0, 3), [events]);
  const [featuredByEventId, setFeaturedByEventId] = useState<Record<number, FeaturedActivity[]>>({});

  const SLIDE_DURATION = 5000;

  const slideEvents: any[] = useMemo(() => (popularEvents.length ? popularEvents : (POPULAR_EVENTS as any[])), [popularEvents]);

  const handleNext = () => {
    if (!slideEvents.length) return;
    setActiveSlide((prev) => (prev + 1) % slideEvents.length);
  };

  const handlePrev = () => {
    if (!slideEvents.length) return;
    setActiveSlide((prev) => (prev - 1 + slideEvents.length) % slideEvents.length);
  };

  const openEventDetails = (ev: any) => {
    if (!ev || typeof ev.id !== "number" || ev.id <= 0) return;
    setSelectedEvent(ev as AppEvent);
    setCurrentPage("event_details");
  };

  const loadAuth = async () => {
    try {
      const res = await fetch("/api/auth/status", { credentials: "include" });
      if (!res.ok) return;
      const json = await res.json();
      setAuth({
        isAuthenticated: !!json?.isAuthenticated,
        userName: json?.userName ?? null,
      });
    } catch {
      // ignore
    }
  };

  const loadEvents = async () => {
    try {
      const res = await fetch("/api/eventos", { credentials: "include" });
      if (!res.ok) {
        setEvents([]);
        return;
      }
      const json = await res.json();
      const list: ApiEvent[] = Array.isArray(json) ? json : [];
      const mapped: AppEvent[] = list.map((e) => ({
        id: e.id,
        title: e.nome,
        date: formatDateList(e.data),
        location: e.local ?? "-",
        locationRaw: e.local,
        dateIso: e.data,
        image: e.imageUrl || DEFAULT_EVENT_IMAGE,
        tags: e.categoria?.nome ? [e.categoria.nome.toUpperCase()] : [],
      }));
      setEvents(mapped);
    } catch {
      setEvents([]);
    }
  };

  const logout = async () => {
    try {
      await fetch("/Login/Logout", { method: "POST", credentials: "include" });
    } catch {
      // ignore
    } finally {
      window.location.href = "/Home/Index";
    }
  };

  useEffect(() => {
    loadAuth();
    loadEvents();
  }, []);

  useEffect(() => {
    if (activeSlide >= slideEvents.length) {
      setActiveSlide(0);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [slideEvents.length]);

  useEffect(() => {
    const slideEvent = slideEvents[activeSlide] as any;
    const id = typeof slideEvent?.id === "number" ? slideEvent.id : null;
    if (!id || id <= 0) return;
    if (featuredByEventId[id]?.length) return;

    fetch(`/api/eventos/${id}/atividades`, { credentials: "include" })
      .then(async (r) => {
        if (!r.ok) return [];
        const json = await r.json();
        return Array.isArray(json) ? json : [];
      })
      .then((list: any[]) => {
        const top = list.slice(0, 3).map((a, i) => ({
          id: a.id,
          name: a.nome,
          type: a.local || a?.categoria?.nome || "Atividade",
          time: timeByIndex(i),
        }));
        setFeaturedByEventId((prev) => ({ ...prev, [id]: top }));
      })
      .catch(() => {});
  }, [activeSlide, slideEvents, featuredByEventId]);

  useEffect(() => {
    const handleScroll = () => {
      // Makes navbar fixed after scrolling past the banner (80vh approx)
      if (window.scrollY > window.innerHeight * 0.8) {
        setIsScrolled(true);
      } else {
        setIsScrolled(false);
      }
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  useEffect(() => {
    if (slideEvents.length <= 1) return;
    const timer = setInterval(handleNext, SLIDE_DURATION);
    return () => clearInterval(timer);
  }, [activeSlide, slideEvents.length]);

  const slideEvent: any = slideEvents[activeSlide];
  const slideTitle = slideEvent?.title ?? "";
  const slideImage = slideEvent?.image ?? DEFAULT_EVENT_IMAGE;
  const slideDate = slideEvent?.dateIso !== undefined ? formatDateHome(slideEvent.dateIso) : (slideEvent?.date ?? "-");
  const slideLocation = slideEvent?.locationRaw ?? slideEvent?.location ?? "-";
  const slideActivities: FeaturedActivity[] =
    (Array.isArray(slideEvent?.activities) ? slideEvent.activities : null) ||
    (typeof slideEvent?.id === "number" ? featuredByEventId[slideEvent.id] : null) ||
    [];

  return (
    <div className="relative min-h-screen text-white font-sans selection:bg-yellow-400 selection:text-black overflow-x-hidden bg-[#333533]">
      {/* Navigation - Overlay at top, Fixed when scrolled */}
      <header className={`w-full z-50 transition-all duration-300 ${
        isScrolled 
          ? 'fixed top-0 left-0 bg-[#333533]/95 backdrop-blur-md border-b border-white/10 shadow-2xl translate-y-0' 
          : 'absolute top-0 left-0 bg-transparent border-transparent pt-2'
      }`}>
        <nav className="flex items-center justify-between px-10 py-6 max-w-[1600px] mx-auto">
          <div className="flex items-center gap-10">
            <div className="flex items-center gap-4">
              <div className="w-8 h-8 rounded-full border-2 border-white flex items-center justify-center p-1">
                <div className="w-2 h-2 bg-white rounded-full" />
              </div>
              <span className="text-xl font-black uppercase tracking-tighter italic">Event Horizon</span>
            </div>
            
            <ul className="hidden md:flex items-center gap-12 text-sm font-medium tracking-wide text-white/60">
              <li className={`relative group transition-colors ${currentPage === 'home' ? 'text-white' : 'hover:text-white'}`}>
                <a href="/Home/Index" className="cursor-pointer">
                  Página inicial
                </a>
                {currentPage === 'home' && (
                  <div className="absolute -bottom-4 left-0 w-full h-[2px] bg-white transform origin-left transition-transform scale-x-100" />
                )}
              </li>
              <li className={`relative group cursor-pointer transition-colors ${currentPage === 'events' ? 'text-white' : 'hover:text-white'}`}>
                <div className="flex items-center gap-1">
                  Eventos
                  <ChevronDown className="w-4 h-4 transition-transform group-hover:rotate-180" />
                </div>
                {currentPage === 'events' && (
                  <div className="absolute -bottom-4 left-0 w-full h-[2px] bg-white transform origin-left transition-transform scale-x-100" />
                )}
                
                {/* Dropdown Menu */}
                <div className="absolute top-full left-0 mt-6 w-48 bg-[#0a0a0a] border border-white/10 rounded-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 overflow-hidden z-50 shadow-2xl">
                  <button
                    type="button"
                    onClick={() => setCurrentPage("events")}
                    className="block px-5 py-3 hover:bg-white/10 text-white/70 hover:text-white transition-colors cursor-pointer"
                  >
                    Ver Eventos
                  </button>
                  <a
                    href="/Evento/Criar"
                    className="block px-5 py-3 hover:bg-white/10 text-white/70 hover:text-white transition-colors border-t border-white/5 cursor-pointer"
                  >
                    Criar Eventos
                  </a>
                </div>
              </li>
              {auth.isAuthenticated && (
                <>
                  <li className="relative group transition-colors hover:text-white">
                    <a href="/Atividade/Index" className="cursor-pointer">
                      Atividades
                    </a>
                  </li>
                  <li className="relative group transition-colors hover:text-white">
                    <a href="/Bilhete/Index" className="cursor-pointer">
                      Bilhetes
                    </a>
                  </li>
                </>
              )}
            </ul>
          </div>

          {auth.isAuthenticated ? (
            <div className="relative group cursor-pointer">
              <div className="flex items-center gap-3 text-sm font-medium tracking-wide text-white group-hover:text-yellow-400 transition-colors">
                {auth.userName ?? "Utilizador"}
                <ChevronDown className="w-4 h-4 transition-transform group-hover:rotate-180" />
              </div>
              <div className="absolute top-full right-0 mt-6 w-44 bg-[#0a0a0a] border border-white/10 rounded-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 overflow-hidden z-50 shadow-2xl">
                <a
                  href="/Utilizador/Detalhes"
                  className="block px-5 py-3 hover:bg-white/10 text-white/70 hover:text-white transition-colors"
                >
                  Mais Detalhes
                </a>
                <button
                  type="button"
                  onClick={logout}
                  className="w-full text-left px-5 py-3 hover:bg-white/10 text-white/70 hover:text-white transition-colors border-t border-white/5 cursor-pointer"
                >
                  Sair
                </button>
              </div>
            </div>
          ) : (
            <div className="flex items-center gap-8 text-sm font-medium tracking-wide text-white/60">
              <a href="/Login/Index" className="hover:text-white transition-colors">
                Login
              </a>
              <a href="/Registo/Registo" className="hover:text-white transition-colors">
                Registo
              </a>
            </div>
          )}
        </nav>
      </header>

      {currentPage === 'home' ? (
        <main className="pb-32 min-h-screen">
          {/* Banner Hero Carousel */}
          <div className="relative w-full h-[80vh] min-h-[600px] overflow-hidden">
            <AnimatePresence mode="popLayout" initial={false}>
              <motion.div
                key={activeSlide}
                initial={{ opacity: 0, scale: 1.05 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 1, ease: "easeInOut" }}
                className="absolute inset-0"
              >
                <img 
                  src={slideImage} 
                  alt={slideTitle} 
                  className="w-full h-full object-cover" 
                />
                {/* Fade at bottom matching background color */}
                <div className="absolute inset-0 bg-gradient-to-r from-black/80 via-black/40 to-transparent z-10" />
                <div className="absolute inset-x-0 bottom-0 h-48 bg-gradient-to-t from-[#333533] to-transparent z-10" />
              </motion.div>
            </AnimatePresence>

            <div className="absolute inset-0 z-20 flex flex-col justify-end px-10 pb-32 max-w-[1600px] mx-auto pointer-events-none">
              <div className="max-w-3xl pointer-events-auto">
                 <AnimatePresence mode="wait">
                    <motion.div
                      key={activeSlide}
                      initial={{ opacity: 0, y: 20 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: -20 }}
                      transition={{ duration: 0.6 }}
                    >
                      <h1 className="text-5xl md:text-8xl font-black uppercase tracking-tighter mb-4 leading-none drop-shadow-xl">
                        {slideTitle}
                      </h1>
                      <div className="flex items-center gap-6 mb-10 text-lg md:text-xl font-mono text-white/80 tracking-wide">
                        <span>{slideDate}</span>
                        <div className="w-1.5 h-1.5 bg-yellow-400 rounded-full" />
                        <span>{slideLocation}</span>
                      </div>
                      <button 
                         onClick={() => openEventDetails(slideEvent)}
                         className="group relative flex items-center gap-4 bg-white/10 backdrop-blur-md border border-white/20 px-8 py-4 rounded-lg font-bold uppercase tracking-widest hover:bg-yellow-400 hover:text-black hover:border-yellow-400 transition-all duration-300 shadow-xl"
                      >
                         <span>Ver Mais</span>
                         <ArrowRight className="w-5 h-5 text-yellow-400 group-hover:text-black group-hover:translate-x-1 transition-all duration-300" />
                      </button>
                    </motion.div>
                 </AnimatePresence>
              </div>
            </div>

            {/* Carousel Controls */}
            <div className="absolute bottom-10 left-1/2 -translate-x-1/2 z-30 flex items-center gap-3">
              {slideEvents.map((_, idx) => (
                <button
                  key={idx}
                  onClick={() => setActiveSlide(idx)}
                  className="group py-4 px-1 focus:outline-none flex flex-col justify-center"
                  aria-label={`Ir para evento ${idx + 1}`}
                >
                  <div className={`h-1 transition-all duration-500 rounded-full ${activeSlide === idx ? 'w-16 bg-yellow-400 shadow-[0_0_10px_rgba(250,204,21,0.5)]' : 'w-8 bg-white/40 group-hover:bg-white/80'}`} />
                </button>
              ))}
            </div>
          </div>

          {/* Atividades em Destaque */}
          <div className="max-w-[1600px] mx-auto px-10 pt-20 mt-10">
            <div className="flex items-end justify-between mb-16 border-b border-white/10 pb-6">
              <h2 className="text-4xl md:text-5xl font-medium tracking-tighter">Atividades em Destaque</h2>
            </div>

          <AnimatePresence mode="wait">
             <motion.div
                key={activeSlide}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                transition={{ duration: 0.5, ease: "easeInOut" }}
                className="flex flex-col lg:flex-row gap-12"
             >
                {/* Left Side: Event Poster */}
                <div 
                  className="w-full lg:w-[35%] relative aspect-[4/5] rounded-[30px] overflow-hidden border border-white/10 shrink-0 group cursor-pointer"
                  onClick={() => openEventDetails(slideEvent)}
                >
                  <div className="absolute inset-0 bg-gradient-to-t from-[#0a0a0a] via-black/20 to-transparent z-10" />
                  <img src={slideImage} alt={slideTitle} className="w-full h-full object-cover grayscale opacity-80 group-hover:scale-105 transition-transform duration-700" />
                  <div className="absolute bottom-8 left-8 right-8 z-20">
                     <span className="text-yellow-400 font-mono text-xs uppercase tracking-widest mb-2 block">Evento Atual</span>
                     <h3 className="text-4xl font-bold tracking-tighter uppercase leading-none text-white">{slideTitle}</h3>
                  </div>
                </div>

                {/* Right Side: Popular Activities List */}
                <div className="flex-1 flex flex-col justify-center gap-2">
                  {slideActivities.map((activity, idx) => (
                    <motion.div 
                       key={activity.id}
                       initial={{ opacity: 0, x: 20 }}
                       animate={{ opacity: 1, x: 0 }}
                       transition={{ delay: idx * 0.15 + 0.2 }}
                       className="group/item flex flex-col md:flex-row md:items-center justify-between py-8 border-b border-white/5 hover:border-white/20 transition-all cursor-pointer"
                    >
                       <div className="flex items-center gap-8">
                         <span className="text-white/20 font-mono text-xl font-bold">{(idx + 1).toString().padStart(2, '0')}</span>
                         <div>
                            <div className="text-white/40 text-xs font-mono mb-2">{activity.time} &bull; {activity.type}</div>
                            <h4 className="text-4xl md:text-5xl font-black tracking-tighter uppercase group-hover/item:text-yellow-400 group-hover/item:translate-x-2 transition-all duration-300">
                               {activity.name}
                            </h4>
                         </div>
                       </div>
                       
                       <div className="mt-4 md:mt-0 flex items-center justify-between md:justify-end md:w-32 shrink-0">
                          <div className="w-12 h-12 rounded-full border border-white/10 flex items-center justify-center group-hover/item:border-yellow-400 group-hover/item:bg-white/5 transition-colors">
                             <ArrowRight className="w-5 h-5 text-white/40 group-hover/item:text-yellow-400 transition-colors" />
                          </div>
                       </div>
                    </motion.div>
                  ))}
                </div>
             </motion.div>
          </AnimatePresence>
        </div>

        {/* Footer Text */}
        <footer className="mt-20 pt-16 border-t border-white/5 flex flex-col md:flex-row items-center justify-end gap-12">
          <div className="text-right">
            <span className="text-[10px] font-black uppercase tracking-widest text-white/30 block">NEXT MEETUP</span>
            <span className="text-xs font-bold uppercase tracking-widest">LISBOA — NOVEMBRO 2026</span>
          </div>
        </footer>
      </main>
      ) : currentPage === 'events' ? (
        <EventsView events={events} onEventClick={(event) => openEventDetails(event)} />
      ) : currentPage === 'create_event' ? (
        <CreateEventView
          onCancel={() => setCurrentPage("events")}
          onCreated={async () => {
            await loadEvents();
            setCurrentPage("events");
          }}
        />
      ) : currentPage === 'event_details' && selectedEvent ? (
        <EventDetailsView
          event={{
            id: selectedEvent.id,
            title: selectedEvent.title,
            date: formatDateHome(selectedEvent.dateIso),
            location: selectedEvent.location,
            image: selectedEvent.image,
          }}
          onBack={() => setCurrentPage("events")}
        />
      ) : null}
    </div>
  );
}
