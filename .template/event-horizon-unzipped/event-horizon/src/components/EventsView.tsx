import { useState } from "react";
import { motion, AnimatePresence } from "motion/react";
import { LayoutGrid, List, ArrowRight, ChevronDown, Calendar, Search, X } from "lucide-react";

const EVENTS = [
  { 
    id: 1, 
    title: "NEON DYSTOPIA", 
    date: "24 MAI 2026", 
    location: "Lisboa, LX Factory", 
    image: "https://www.fazemag.de/wp-content/uploads/2025/01/Holy_Priest_Facebook.jpg", 
    tags: ["TECHNO", "LIVE AV"] 
  },
  { 
    id: 2, 
    title: "SYNTHESIS CORE", 
    date: "12 JUN 2026", 
    location: "Porto, Hard Club", 
    image: "https://images.unsplash.com/photo-1574391884720-bbc3740c59d1?q=80&w=2000&auto=format&fit=crop", 
    tags: ["INDUSTRIAL", "VINYL ONLY"] 
  },
  { 
    id: 3, 
    title: "VOID FREQUENCIES", 
    date: "03 JUL 2026", 
    location: "Secret Location", 
    image: "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=2000&auto=format&fit=crop", 
    tags: ["AMBIENT", "ALL NIGHT LONG"] 
  },
  { 
    id: 4, 
    title: "ECHO CHAMBER", 
    date: "21 AGO 2026", 
    location: "Lisboa, Pavilhão Carlos Lopes", 
    image: "https://images.unsplash.com/photo-1459749411175-04bf5292ceea?q=80&w=2000&auto=format&fit=crop", 
    tags: ["HOUSE", "MAIN STAGE"] 
  }
];

export function EventsView({ onEventClick }: { onEventClick?: (event: any) => void }) {
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [showFilters, setShowFilters] = useState(false);

  return (
    <motion.div 
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className="max-w-[1600px] mx-auto px-10 pt-20 pb-32"
    >
      {/* Header section matching Home aesthetic */}
      <div className="flex flex-col md:flex-row md:items-end justify-end gap-12 mb-10 relative">
        <h1 className="text-7xl md:text-[110px] font-medium tracking-tighter leading-none text-right shrink-0 relative">
          Eventos
        </h1>
      </div>

      {/* Toolbar */}
      <div className="flex justify-between items-center mb-8 border-b border-white/10 pb-6">
         <div className="flex items-center gap-6">
           <div className="text-white/60 font-mono text-sm uppercase tracking-widest hidden sm:block">
             {EVENTS.length} Eventos encontrados
           </div>
           
           {/* Toggle Filters Button */}
           <button 
             onClick={() => setShowFilters(!showFilters)}
             className={`flex items-center gap-2 text-xs font-bold font-mono tracking-widest uppercase transition-colors px-4 py-2 rounded-full border ${showFilters ? 'bg-yellow-400 text-black border-yellow-400 shadow-[0_0_15px_rgba(250,204,21,0.3)]' : 'bg-transparent text-white border-white/20 hover:border-white/60'}`}
           >
             <Search className="w-4 h-4" />
             {showFilters ? 'Esconder Pesquisa' : 'Nova Pesquisa'}
           </button>
         </div>

         <div className="flex gap-2 bg-[#1a1a1a] p-1 rounded-full border border-white/10">
            <button 
              onClick={() => setViewMode('grid')}
              className={`p-3 rounded-full transition-all duration-300 ${viewMode === 'grid' ? 'bg-white text-black shadow-lg scale-100' : 'text-white/50 hover:text-white scale-95'}`}
            >
               <LayoutGrid className="w-4 h-4" />
            </button>
            <button 
              onClick={() => setViewMode('list')}
              className={`p-3 rounded-full transition-all duration-300 ${viewMode === 'list' ? 'bg-white text-black shadow-lg scale-100' : 'text-white/50 hover:text-white scale-95'}`}
            >
               <List className="w-4 h-4" />
            </button>
         </div>
      </div>

      {/* Expandable Search & Filters Row */}
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
                placeholder="Pesquisar por nome..." 
                className="min-w-[200px] flex-1 bg-transparent border border-white/20 rounded-full px-5 py-2.5 text-xs text-white placeholder-white/50 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
                autoFocus
              />
              
              <div className="relative min-w-[150px] shrink-0 group border border-white/20 rounded-full focus-within:border-yellow-400 focus-within:bg-white/[0.02] transition-colors">
                <input 
                  type="date" 
                  aria-label="Data do evento"
                  className="w-full h-full bg-transparent rounded-full pl-5 pr-10 py-2.5 text-xs text-white/50 focus:outline-none focus:text-white transition-colors relative z-10 [&::-webkit-calendar-picker-indicator]:opacity-0 [&::-webkit-calendar-picker-indicator]:absolute [&::-webkit-calendar-picker-indicator]:inset-0 [&::-webkit-calendar-picker-indicator]:w-full [&::-webkit-calendar-picker-indicator]:h-full [&::-webkit-calendar-picker-indicator]:cursor-pointer"
                  style={{ colorScheme: "dark" }}
                />
                <Calendar className="absolute right-4 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-white/50 pointer-events-none z-0 group-focus-within:text-yellow-400 transition-colors" />
              </div>

              <input 
                type="text" 
                placeholder="Pesquisar por local..." 
                className="min-w-[200px] flex-1 bg-transparent border border-white/20 rounded-full px-5 py-2.5 text-xs text-white placeholder-white/50 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
              />

              <div className="relative min-w-[180px] shrink-0 group border border-white/20 rounded-full focus-within:border-yellow-400 focus-within:bg-white/[0.02] transition-colors">
                <select 
                  className="w-full h-full bg-transparent rounded-full pl-5 pr-10 py-2.5 text-xs text-white focus:outline-none appearance-none cursor-pointer relative z-10"
                  defaultValue=""
                >
                  <option value="" disabled hidden className="bg-[#1a1a1a] text-white/50">Todas as categorias</option>
                  <option value="todas" className="bg-[#1a1a1a]">Todas as categorias</option>
                  <option value="techno" className="bg-[#1a1a1a]">Techno</option>
                  <option value="house" className="bg-[#1a1a1a]">House</option>
                  <option value="live-av" className="bg-[#1a1a1a]">Live AV</option>
                </select>
                <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-white/50 pointer-events-none z-20 group-focus-within:text-yellow-400 transition-colors" />
              </div>

              {/* Clear button */}
              <button 
                className="shrink-0 flex items-center justify-center w-10 h-10 bg-transparent border border-white/20 rounded-full text-white/60 hover:bg-white/10 hover:text-white hover:border-white transition-all group"
                aria-label="Limpar Filtros"
                title="Limpar Filtros"
              >
                <X className="w-5 h-5 group-hover:scale-110 transition-transform duration-300" />
              </button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Grid View */}
      {viewMode === 'grid' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-16">
           {EVENTS.map((event, i) => (
              <motion.div 
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.1 }}
                key={event.id} 
                className="group cursor-pointer"
                onClick={() => onEventClick?.(event)}
              >
                <div className="relative aspect-[4/3] overflow-hidden rounded-[20px] mb-6 border border-white/10">
                  <div className="absolute inset-0 bg-black/20 group-hover:bg-transparent transition-colors z-10" />
                  <img src={event.image} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700" alt={event.title} />
                  <div className="absolute top-4 left-4 z-20 flex gap-2 flex-wrap">
                     {event.tags.map(tag => (
                       <span key={tag} className="px-3 py-1 bg-black/80 backdrop-blur-md border border-white/20 text-white text-[9px] uppercase tracking-widest font-bold whitespace-nowrap rounded-full">
                         {tag}
                       </span>
                     ))}
                  </div>
                </div>
                <div className="flex justify-between items-start gap-4">
                  <div>
                    <h3 className="text-3xl lg:text-4xl font-bold tracking-tighter uppercase mb-2 group-hover:text-yellow-400 transition-colors line-clamp-1">{event.title}</h3>
                    <div className="flex gap-4 text-white/50 text-sm font-mono uppercase tracking-wide">
                       <span>{event.date}</span>
                       <span>&bull;</span>
                       <span>{event.location}</span>
                    </div>
                  </div>
                  <div className="w-12 h-12 rounded-full border border-white/20 flex items-center justify-center group-hover:bg-yellow-400 group-hover:text-black group-hover:border-yellow-400 transition-all rotate-[-45deg] group-hover:rotate-0 shrink-0">
                     <ArrowRight className="w-5 h-5" />
                  </div>
                </div>
              </motion.div>
           ))}
        </div>
      )}

      {/* List View */}
      {viewMode === 'list' && (
        <div className="flex flex-col border-t border-white/10">
           {EVENTS.map((event, i) => (
             <motion.div 
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: i * 0.05 }}
                key={event.id} 
                className="group cursor-pointer border-b border-white/10 py-10 flex flex-col lg:flex-row lg:items-center justify-between gap-6 hover:bg-white/[0.02] transition-colors lg:px-4 lg:-mx-4 rounded-xl"
                onClick={() => onEventClick?.(event)}
              >
                <div className="flex flex-col lg:flex-row lg:items-center gap-4 lg:gap-12 w-full lg:flex-1">
                  <span className="text-white/40 font-mono text-sm uppercase tracking-widest w-32 shrink-0">{event.date}</span>
                  <h3 className="text-4xl lg:text-6xl font-bold tracking-tighter uppercase group-hover:text-yellow-400 transition-colors">{event.title}</h3>
                </div>
                <div className="flex items-center gap-8 w-full lg:w-auto justify-between lg:justify-end shrink-0">
                  <div className="flex gap-2">
                    {event.tags.map(tag => (
                      <span key={tag} className="px-3 py-1 bg-white/5 text-white/60 text-[9px] uppercase tracking-widest font-bold whitespace-nowrap rounded-full hidden lg:inline-block">
                        {tag}
                      </span>
                    ))}
                  </div>
                  <span className="text-white/60 font-mono text-xs text-right whitespace-nowrap hidden sm:block w-40">{event.location}</span>
                  <div className="w-12 h-12 rounded-full border border-white/20 flex items-center justify-center group-hover:bg-yellow-400 group-hover:text-black group-hover:border-yellow-400 transition-all rotate-[-45deg] group-hover:rotate-0 shrink-0">
                     <ArrowRight className="w-5 h-5" />
                  </div>
                </div>
              </motion.div>
           ))}
        </div>
      )}
    </motion.div>
  );
}
