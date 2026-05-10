import { useState, useMemo } from "react";
import { motion, AnimatePresence } from "motion/react";
import { Search, ChevronDown, Clock, Users, MapPin, ArrowLeft, X, ShoppingCart, CheckCircle2, Star, Diamond, Plus } from "lucide-react";

// Mock activities generation based on event title
const generateMockActivities = (eventTitle: string) => [
  { id: 1, name: "Abertura Oficial", location: "Stage Principal", capacity: 5000, time: "18:00" },
  { id: 2, name: `${eventTitle} - Warm up`, location: "Stage Secundário", capacity: 1000, time: "19:00" },
  { id: 3, name: "Live Set Exclusive", location: "Stage Principal", capacity: 5000, time: "20:30" },
  { id: 4, name: "Secret B2B", location: "Boiler Room", capacity: 300, time: "22:00" },
  { id: 5, name: "Closing Ceremony", location: "Stage Principal", capacity: 5000, time: "04:00" },
];

export function EventDetailsView({ event, onBack }: { event: any; onBack: () => void }) {
  const [showFilters, setShowFilters] = useState(false);
  const [searchName, setSearchName] = useState("");
  const [searchLocation, setSearchLocation] = useState("");
  const [sortOrder, setSortOrder] = useState<"time_asc" | "time_desc" | "alpha">("time_asc");

  // Fallback if event doesn't have activities
  const initialActivities = event.activities || generateMockActivities(event.title);

  // Filter and Sort logic
  const filteredActivities = useMemo(() => {
    let result = [...initialActivities];
    
    // Filter by name
    if (searchName) {
      result = result.filter(act => act.name.toLowerCase().includes(searchName.toLowerCase()));
    }
    // Filter by location
    if (searchLocation) {
      result = result.filter(act => act.location.toLowerCase().includes(searchLocation.toLowerCase()));
    }

    // Sort
    result.sort((a, b) => {
      if (sortOrder === 'alpha') {
        return a.name.localeCompare(b.name);
      }
      
      const timeA = parseInt(a.time.replace(":", ""));
      const timeB = parseInt(b.time.replace(":", ""));
      
      if (sortOrder === 'time_asc') return timeA - timeB;
      if (sortOrder === 'time_desc') return timeB - timeA;
      return 0;
    });

    return result;
  }, [initialActivities, searchName, searchLocation, sortOrder]);

  return (
    <motion.div 
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="w-full min-h-screen relative pb-32"
    >
      {/* Background Image with Fade */}
      <div className="absolute top-0 left-0 w-full h-[80vh] min-h-[600px] -z-10">
        <img 
          src={event.image} 
          alt={event.title} 
          className="w-full h-full object-cover" 
        />
        <div className="absolute inset-0 bg-gradient-to-r from-black/80 via-black/40 to-transparent" />
        <div className="absolute inset-x-0 bottom-0 h-48 bg-gradient-to-t from-[#333533] to-transparent" />
      </div>

      <div className="max-w-[1600px] mx-auto px-4 md:px-10 pt-32 relative z-10 w-full">
        <div className="w-full bg-[#0a0a0a]/60 backdrop-blur-xl border border-white/10 rounded-[30px] relative shadow-2xl overflow-hidden mb-20">
          
          {/* Background image inside the card */}
          <div 
            className="absolute top-0 left-0 w-full h-[600px] z-0 pointer-events-none"
            style={{
              WebkitMaskImage: 'linear-gradient(to bottom, black 0%, black 50%, transparent 100%)',
              maskImage: 'linear-gradient(to bottom, black 0%, black 50%, transparent 100%)'
            }}
          >
            <img 
              src={event.image} 
              alt={event.title} 
              className="w-full h-full object-cover grayscale opacity-20" 
            />
          </div>

          <div className="relative z-10 p-8 md:p-16">
            {/* Back Button */}
            <button 
              onClick={onBack}
          className="flex items-center gap-2 text-white/50 hover:text-yellow-400 font-mono text-sm uppercase tracking-widest transition-colors mb-12 group"
        >
          <ArrowLeft className="w-4 h-4 group-hover:-translate-x-1 transition-transform" /> Voltar para Eventos
        </button>

        {/* Event Header Info and Tickets Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 mb-20 pb-20 border-b border-white/5">
          {/* Left Column: Event Details */}
          <div>
            <div className="flex gap-2 flex-wrap mb-4">
              {event.tags?.map((tag: string) => (
                <span key={tag} className="px-3 py-1 bg-yellow-400 text-black text-[10px] uppercase tracking-widest font-black rounded-full">
                  {tag}
                </span>
              ))}
            </div>
            
            <h1 className="text-4xl md:text-6xl font-medium tracking-tighter leading-none mb-12">
              {event.title}
            </h1>
            
            <h2 className="text-2xl font-bold tracking-tighter uppercase mb-6">Informações do evento</h2>
            
            <div className="flex flex-col gap-4">
              <div className="grid grid-cols-3 gap-4 border-b border-white/5 pb-4">
                <span className="text-white/70 font-bold uppercase tracking-widest text-xs">Data</span>
                <span className="col-span-2 text-white font-mono text-sm">{event.date}</span>
              </div>
              <div className="grid grid-cols-3 gap-4 border-b border-white/5 pb-4">
                <span className="text-white/70 font-bold uppercase tracking-widest text-xs">Hora</span>
                <span className="col-span-2 text-white font-mono text-sm">20:00</span>
              </div>
              <div className="grid grid-cols-3 gap-4 border-b border-white/5 pb-4">
                <span className="text-white/70 font-bold uppercase tracking-widest text-xs">Local</span>
                <span className="col-span-2 text-white font-mono text-sm">{event.location}</span>
              </div>
              <div className="grid grid-cols-3 gap-4 border-b border-white/5 pb-4">
                <span className="text-white/70 font-bold uppercase tracking-widest text-xs">Descrição</span>
                <span className="col-span-2 text-white font-mono text-sm">{event.title} at {event.location}</span>
              </div>
              <div className="grid grid-cols-3 gap-4 border-b border-white/5 pb-4">
                <span className="text-white/70 font-bold uppercase tracking-widest text-xs">Capacidade Máxima</span>
                <span className="col-span-2 text-white font-mono text-sm">1000</span>
              </div>
              <div className="grid grid-cols-3 gap-4 pb-4">
                <span className="text-white/70 font-bold uppercase tracking-widest text-xs">Categoria</span>
                <span className="col-span-2 text-white font-mono text-sm">{event.tags?.[0] || 'Techno'}</span>
              </div>
            </div>
          </div>

          {/* Right Column: Tickets */}
          <div className="flex flex-col">
            <div className="flex items-center justify-between mb-2">
              <h2 className="text-3xl font-bold tracking-tighter uppercase">Comprar bilhete</h2>
              <ShoppingCart className="w-6 h-6 text-yellow-400" />
            </div>
            <p className="text-white/50 text-sm font-mono tracking-wide mb-8">Escolhe o acesso que melhor combina com a tua experiência.</p>

            {/* Existing Ticket Alert (Mocking as inactive based on screenshot layout, but let's just show tickets directly) */}

            <div className="flex flex-col gap-4">
              {/* Standard Ticket */}
              <div className="bg-white/[0.03] border border-white/10 rounded-2xl p-6 hover:bg-white/[0.05] transition-colors relative overflow-hidden group cursor-pointer">
                <div className="flex justify-between items-start mb-4">
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 rounded-full bg-[#111111] border border-white/10 flex items-center justify-center shrink-0">
                      <CheckCircle2 className="w-5 h-5 text-white/40" />
                    </div>
                    <div>
                      <h3 className="text-xl font-bold tracking-tighter uppercase">Bilhete Standard</h3>
                      <p className="text-white/50 text-xs font-mono uppercase tracking-widest">Standard</p>
                    </div>
                  </div>
                  <div className="bg-[#111111] px-4 py-2 rounded-full border border-white/10">
                    <span className="font-bold text-sm">50,00 EUR</span>
                  </div>
                </div>
                <div className="flex justify-between items-center mt-6">
                  <span className="text-white/20 text-xs font-mono tracking-widest px-3 py-1 bg-white/5 border border-white/5 rounded-full inline-flex items-center gap-1 group-hover:text-yellow-400 transition-colors">
                    <div className="w-1.5 h-1.5 rounded-full bg-yellow-400/50" /> Esgotado
                  </span>
                  <button disabled className="bg-white/10 text-white/50 px-6 py-2 rounded-full text-xs font-bold uppercase tracking-widest cursor-not-allowed">
                    Inscrição Ativa
                  </button>
                </div>
              </div>

              {/* Gold Ticket */}
              <div className="bg-gradient-to-r from-yellow-400/10 to-transparent border border-yellow-400/30 rounded-2xl p-6 hover:from-yellow-400/20 transition-colors relative overflow-hidden group cursor-pointer shadow-[0_0_30px_rgba(250,204,21,0.05)]">
                <div className="flex justify-between items-start mb-4">
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 rounded-full bg-yellow-400/20 border border-yellow-400/50 flex items-center justify-center shrink-0">
                       <Star className="w-5 h-5 text-yellow-400 fill-yellow-400" />
                    </div>
                    <div>
                      <h3 className="text-xl font-bold tracking-tighter uppercase text-yellow-400">Bilhete Gold</h3>
                      <p className="text-white/50 text-xs font-mono uppercase tracking-widest">Gold</p>
                    </div>
                  </div>
                  <div className="bg-black/50 px-4 py-2 rounded-full border border-yellow-400/30 backdrop-blur-md">
                    <span className="font-bold text-sm text-yellow-400">75,00 EUR</span>
                  </div>
                </div>
                <div className="flex justify-end items-center mt-6">
                  <button className="bg-yellow-400 text-black px-6 py-2 rounded-full text-xs font-bold uppercase tracking-widest shadow-[0_0_15px_rgba(250,204,21,0.2)] hover:bg-yellow-300 transition-colors hover:scale-105">
                    Comprar
                  </button>
                </div>
              </div>

              {/* VIP Ticket */}
              <div className="bg-gradient-to-r from-indigo-500/10 to-transparent border border-indigo-500/30 rounded-2xl p-6 hover:from-indigo-500/20 transition-colors relative overflow-hidden group cursor-pointer">
                <div className="flex justify-between items-start mb-4">
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 rounded-full bg-indigo-500/20 border border-indigo-500/50 flex items-center justify-center shrink-0">
                      <Diamond className="w-5 h-5 text-indigo-400 fill-indigo-400/20" />
                    </div>
                    <div>
                      <h3 className="text-xl font-bold tracking-tighter uppercase text-indigo-400">Bilhete VIP</h3>
                      <p className="text-white/50 text-xs font-mono uppercase tracking-widest">VIP</p>
                    </div>
                  </div>
                  <div className="bg-black/50 px-4 py-2 rounded-full border border-indigo-500/30 backdrop-blur-md">
                    <span className="font-bold text-sm text-indigo-400">110,00 EUR</span>
                  </div>
                </div>
                <div className="flex justify-end items-center mt-6">
                  <button className="bg-indigo-500 text-white px-6 py-2 rounded-full text-xs font-bold uppercase tracking-widest shadow-[0_0_15px_rgba(99,102,241,0.2)] hover:bg-indigo-400 transition-colors hover:scale-105">
                    Comprar
                  </button>
                </div>
              </div>

            </div>
          </div>
        </div>

        {/* Activities Section */}
        <div>
          {/* Toolbar */}
          <div className="flex justify-between items-center mb-8 border-b border-white/10 pb-6">
             <div className="flex items-center gap-6">
               <h2 className="text-3xl font-bold tracking-tighter uppercase hidden sm:block">Atividades</h2>
               <div className="text-yellow-400/80 font-mono text-xs uppercase tracking-widest mt-1 hidden lg:block">
                 {filteredActivities.length} Encontradas
               </div>
             </div>

             <div className="flex items-center gap-4">
               {/* Sort Select */}
               <div className="relative min-w-[200px] shrink-0 group border border-white/20 rounded-full focus-within:border-yellow-400 focus-within:bg-white/[0.02] transition-colors h-10 hidden md:block">
                 <select 
                   className="w-full h-full bg-transparent rounded-full pl-5 pr-10 text-xs text-white font-mono tracking-widest uppercase focus:outline-none appearance-none cursor-pointer relative z-10"
                   value={sortOrder}
                   onChange={(e) => setSortOrder(e.target.value as any)}
                 >
                   <option value="time_asc" className="bg-[#1a1a1a]">Hora Crescente</option>
                   <option value="time_desc" className="bg-[#1a1a1a]">Hora Decrescente</option>
                   <option value="alpha" className="bg-[#1a1a1a]">A-Z</option>
                 </select>
                 <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-white/50 pointer-events-none z-20 group-focus-within:text-yellow-400 transition-colors" />
               </div>

               {/* Toggle Filters Button */}
               <button 
                 onClick={() => setShowFilters(!showFilters)}
                 className={`flex items-center gap-2 text-xs font-bold font-mono tracking-widest uppercase transition-colors px-4 py-2 h-10 rounded-full border ${showFilters ? 'bg-yellow-400 text-black border-yellow-400 shadow-[0_0_15px_rgba(250,204,21,0.3)]' : 'bg-transparent text-white border-white/20 hover:border-white/60'}`}
               >
                 <Search className="w-4 h-4" />
                 {showFilters ? 'Esconder' : 'Pesquisar'}
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

                  {/* Clear button */}
                  <button 
                    onClick={() => { setSearchName(""); setSearchLocation(""); }}
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

          {/* Activities List */}
          <div className="flex flex-col border-t border-white/5">
            {filteredActivities.length === 0 ? (
              <div className="py-20 text-center text-white/40 font-mono tracking-widest text-sm uppercase">Nenhuma atividade encontrada</div>
            ) : (
              filteredActivities.map((activity, i) => (
                <motion.div 
                   initial={{ opacity: 0, y: 10 }}
                   animate={{ opacity: 1, y: 0 }}
                   transition={{ delay: i * 0.05 }}
                   key={activity.id} 
                   className="group border-b border-white/5 py-8 flex flex-col md:flex-row md:items-center justify-between gap-6 hover:bg-white/[0.02] transition-colors md:px-6 md:-mx-6 rounded-xl"
                 >
                   <div className="flex items-center gap-6 md:gap-10 w-full md:flex-1">
                     <span className="text-yellow-400 font-mono text-xl font-bold flex items-center gap-2">
                        <Clock className="w-5 h-5 opacity-50" /> {activity.time}
                     </span>
                     <div>
                       <h3 className="text-2xl md:text-3xl font-bold tracking-tighter uppercase group-hover:text-yellow-400 transition-colors mb-2">{activity.name}</h3>
                       <div className="flex flex-wrap items-center gap-4 text-white/40 font-mono text-xs uppercase tracking-widest">
                         <span className="flex items-center gap-1"><MapPin className="w-3.5 h-3.5" /> {activity.location}</span>
                         <span>&bull;</span>
                         <span className="flex items-center gap-1"><Users className="w-3.5 h-3.5" /> Cap: {activity.capacity}</span>
                       </div>
                     </div>
                   </div>
                   <div className="w-full md:w-auto mt-2 md:mt-0">
                      <button className="w-full md:w-auto px-6 py-3 border border-white/10 rounded-full text-xs font-bold font-mono uppercase tracking-widest text-white hover:bg-white hover:text-black transition-colors focus:outline-none">
                         Inscrever
                      </button>
                   </div>
                 </motion.div>
              ))
            )}
          </div>

          {/* Create New Activity Form */}
          <div className="mt-12 bg-white/[0.02] border border-white/10 rounded-2xl p-8 max-w-5xl mx-auto w-full">
            <h3 className="text-xl font-bold tracking-tighter uppercase mb-6 flex items-center gap-2">
              <Plus className="w-5 h-5 text-yellow-400" /> Nova Atividade
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              <div>
                <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Nome</label>
                <input 
                  type="text" 
                  placeholder="Ex: Main stage warmup" 
                  className="w-full bg-transparent border border-white/20 rounded-xl px-4 py-3 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors"
                 />
              </div>
              <div>
                <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Local</label>
                <input 
                  type="text" 
                  placeholder="Ex: Palco Secundário" 
                  className="w-full bg-transparent border border-white/20 rounded-xl px-4 py-3 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors"
                 />
              </div>
              <div>
                <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Capacidade</label>
                <input 
                  type="number" 
                  placeholder="Ex: 500" 
                  className="w-full bg-transparent border border-white/20 rounded-xl px-4 py-3 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors"
                 />
              </div>
              <div>
                <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Categoria</label>
                <div className="relative group border border-white/20 rounded-xl focus-within:border-yellow-400 focus-within:bg-white/[0.05] transition-colors">
                  <select 
                    className="w-full h-[46px] bg-transparent rounded-xl pl-4 pr-10 text-sm text-white focus:outline-none appearance-none cursor-pointer relative z-10"
                    defaultValue=""
                  >
                    <option value="" disabled hidden className="bg-[#1a1a1a] text-white/50">Seleciona Categoria</option>
                    <option value="techno" className="bg-[#1a1a1a]">Techno</option>
                    <option value="house" className="bg-[#1a1a1a]">House</option>
                    <option value="live-av" className="bg-[#1a1a1a]">Live AV</option>
                  </select>
                  <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50 pointer-events-none group-focus-within:text-yellow-400 transition-colors z-20" />
                </div>
              </div>
            </div>
            <div className="mt-6 flex justify-end">
              <button className="bg-[#0a58ca] hover:bg-[#0b5ed7] text-white px-6 py-3 rounded-xl text-xs font-bold font-mono uppercase tracking-widest transition-all shadow-lg hover:shadow-xl hover:scale-105 active:scale-95">
                Adicionar
              </button>
            </div>
          </div>

          </div>
        </div>
        </div>
      </div>
    </motion.div>
  );
}
