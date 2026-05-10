/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import { useState, useEffect } from "react";
import { motion, AnimatePresence } from "motion/react";
import { ChevronDown, ArrowRight, ArrowLeft, Circle, Info, Layers, Users, Zap } from "lucide-react";
import { EventsView } from "./components/EventsView";
import { CreateEventView } from "./components/CreateEventView";
import { EventDetailsView } from "./components/EventDetailsView";
import { LoginView } from "./components/LoginView";
import { RegisterView } from "./components/RegisterView";
import { ProfileView } from "./components/ProfileView";
import { EditProfileView } from "./components/EditProfileView";

const POPULAR_EVENTS = [
  {
    title: "NEON DYSTOPIA",
    date: "14 Jul 2026",
    location: "LX Factory, Lisboa",
    image: "https://www.fazemag.de/wp-content/uploads/2025/01/Holy_Priest_Facebook.jpg",
    tags: ["TECHNO", "LIVE AV"],
    activities: [
      { id: 1, name: "Holy Priest", type: "Live AV", time: "02:00", capacity: 2000, location: "Main Stage" },
      { id: 2, name: "Sara Landry", type: "Main Stage", time: "00:00", capacity: 2000, location: "Main Stage" },
      { id: 3, name: "Nico Moreno", type: "Closing", time: "04:00", capacity: 2000, location: "Main Stage" },
      { id: 11, name: "Warm up B2B", type: "Warm Up", time: "22:00", capacity: 500, location: "Boiler Room" },
      { id: 12, name: "Local Support", type: "Opening", time: "23:00", capacity: 500, location: "Boiler Room" }
    ]
  },
  {
    title: "SYNTHESIS CORE",
    date: "28 Ago 2026",
    location: "Pavilhão Carlos Lopes",
    image: "https://images.unsplash.com/photo-1574391884720-bbc3740c59d1?q=80&w=2000&auto=format&fit=crop",
    tags: ["INDUSTRIAL", "VINYL ONLY"],
    activities: [
      { id: 4, name: "Paula Temple", type: "Warehouse", time: "03:00", capacity: 3000, location: "Warehouse" },
      { id: 5, name: "SNTS", type: "Live Set", time: "01:00", capacity: 3000, location: "Warehouse" },
      { id: 6, name: "I Hate Models", type: "Main Stage", time: "05:00", capacity: 3000, location: "Warehouse" },
      { id: 13, name: "Intro Set", type: "Warm Up", time: "23:00", capacity: 2000, location: "Room 2" },
      { id: 14, name: "Vinyl Session", type: "Vinyl Only", time: "00:00", capacity: 2000, location: "Room 2" }
    ]
  },
  {
    title: "VOID FREQUENCIES",
    date: "15 Set 2026",
    location: "Gare, Porto",
    image: "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=2000&auto=format&fit=crop",
    tags: ["AMBIENT", "ALL NIGHT LONG"],
    activities: [
      { id: 7, name: "Tale of Us", type: "Garden", time: "02:00", capacity: 1500, location: "Garden" },
      { id: 8, name: "Dixon", type: "All Night", time: "23:00", capacity: 1000, location: "Terrace" },
      { id: 9, name: "Mind Against", type: "Sunset", time: "20:00", capacity: 1500, location: "Garden" },
      { id: 15, name: "Opening Ambient", type: "Ambient", time: "18:00", capacity: 800, location: "Terrace" },
      { id: 16, name: "Closing Set", type: "Deep House", time: "06:00", capacity: 1500, location: "Garden" }
    ]
  }
];

export default function App() {
  const [currentPage, setCurrentPage] = useState<'home' | 'events' | 'create_event' | 'event_details' | 'login' | 'register' | 'profile' | 'edit_profile'>('home');
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [userProfile, setUserProfile] = useState({
    name: "Hugo Marques",
    email: "hugoes2@gmail.com",
    phone: "913647508",
    type: "Utilizador Comum"
  });
  const [selectedEvent, setSelectedEvent] = useState<any>(null);
  const [activeSlide, setActiveSlide] = useState(0);
  const [isScrolled, setIsScrolled] = useState(false);

  const SLIDE_DURATION = 5000;

  const handleNext = () => {
    setActiveSlide((prev) => (prev + 1) % POPULAR_EVENTS.length);
  };

  const handlePrev = () => {
    setActiveSlide((prev) => (prev - 1 + POPULAR_EVENTS.length) % POPULAR_EVENTS.length);
  };

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
    const timer = setInterval(handleNext, SLIDE_DURATION);
    return () => clearInterval(timer);
  }, [activeSlide]);

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
              <li 
                onClick={() => setCurrentPage('home')}
                className={`relative group cursor-pointer transition-colors ${currentPage === 'home' ? 'text-white' : 'hover:text-white'}`}
              >
                Página inicial
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
                  <div 
                    onClick={() => setCurrentPage('events')}
                    className="px-5 py-3 hover:bg-white/10 text-white/70 hover:text-white transition-colors cursor-pointer"
                  >
                    Ver Eventos
                  </div>
                  <div 
                    onClick={() => setCurrentPage('create_event')}
                    className="px-5 py-3 hover:bg-white/10 text-white/70 hover:text-white transition-colors border-t border-white/5 cursor-pointer"
                  >
                    Criar Eventos
                  </div>
                </div>
              </li>
            </ul>
          </div>

          {/* User Profile / Auth */}
          {isAuthenticated ? (
            <div className="relative group cursor-pointer">
              <div className="flex items-center gap-3 text-sm font-medium tracking-wide text-white group-hover:text-yellow-400 transition-colors">
                Hugo Marques
                <ChevronDown className="w-4 h-4 transition-transform group-hover:rotate-180" />
              </div>
              <div className="absolute top-full right-0 mt-6 w-40 bg-[#0a0a0a] border border-white/10 rounded-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 overflow-hidden z-50 shadow-2xl">
                <div 
                  onClick={() => setCurrentPage('profile')}
                  className="px-5 py-3 hover:bg-white/10 text-white/70 hover:text-white transition-colors"
                >
                  O Meu Perfil
                </div>
                <div 
                  onClick={() => setIsAuthenticated(false)}
                  className="px-5 py-3 hover:bg-white/10 text-white/70 hover:text-red-400 transition-colors border-t border-white/5"
                >
                  Sair
                </div>
              </div>
            </div>
          ) : (
            <div className="flex items-center gap-4">
              <button 
                onClick={() => setCurrentPage('login')} 
                className="text-sm font-medium tracking-wide text-white/70 hover:text-white transition-colors"
              >
                Entrar
              </button>
              <button 
                onClick={() => setCurrentPage('register')} 
                className="bg-yellow-400 text-black px-5 py-2.5 rounded-full text-[10px] font-bold uppercase tracking-widest hover:bg-yellow-300 hover:scale-105 transition-all shadow-[0_0_15px_rgba(250,204,21,0.2)] hover:shadow-[0_0_25px_rgba(250,204,21,0.4)]"
              >
                Registar
              </button>
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
                  src={POPULAR_EVENTS[activeSlide].image} 
                  alt={POPULAR_EVENTS[activeSlide].title} 
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
                        {POPULAR_EVENTS[activeSlide].title}
                      </h1>
                      <div className="flex items-center gap-6 mb-10 text-lg md:text-xl font-mono text-white/80 tracking-wide">
                        <span>{POPULAR_EVENTS[activeSlide].date}</span>
                        <div className="w-1.5 h-1.5 bg-yellow-400 rounded-full" />
                        <span>{POPULAR_EVENTS[activeSlide].location}</span>
                      </div>
                      <button 
                         onClick={() => { setSelectedEvent(POPULAR_EVENTS[activeSlide]); setCurrentPage('event_details'); }}
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
              {POPULAR_EVENTS.map((_, idx) => (
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
                  onClick={() => { setSelectedEvent(POPULAR_EVENTS[activeSlide]); setCurrentPage('event_details'); }}
                >
                  <div className="absolute inset-0 bg-gradient-to-t from-[#0a0a0a] via-black/20 to-transparent z-10" />
                  <img src={POPULAR_EVENTS[activeSlide].image} alt={POPULAR_EVENTS[activeSlide].title} className="w-full h-full object-cover grayscale opacity-80 group-hover:scale-105 transition-transform duration-700" />
                  <div className="absolute bottom-8 left-8 right-8 z-20">
                     <span className="text-yellow-400 font-mono text-xs uppercase tracking-widest mb-2 block">Evento Atual</span>
                     <h3 className="text-4xl font-bold tracking-tighter uppercase leading-none text-white">{POPULAR_EVENTS[activeSlide].title}</h3>
                  </div>
                </div>

                {/* Right Side: Popular Activities List */}
                <div className="flex-1 flex flex-col justify-center gap-2">
                  {POPULAR_EVENTS[activeSlide].activities.map((activity, idx) => (
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
        <EventsView onEventClick={(event) => { setSelectedEvent(event); setCurrentPage('event_details'); }} />
      ) : currentPage === 'create_event' ? (
        <CreateEventView />
      ) : currentPage === 'event_details' && selectedEvent ? (
        <EventDetailsView event={selectedEvent} onBack={() => setCurrentPage('events')} />
      ) : currentPage === 'login' ? (
        <LoginView onNavigate={setCurrentPage} onLogin={() => setIsAuthenticated(true)} />
      ) : currentPage === 'register' ? (
        <RegisterView onNavigate={setCurrentPage} onLogin={() => setIsAuthenticated(true)} />
      ) : currentPage === 'profile' ? (
        <ProfileView user={userProfile} onNavigate={setCurrentPage} />
      ) : currentPage === 'edit_profile' ? (
        <EditProfileView user={userProfile} onNavigate={setCurrentPage} onSave={setUserProfile} />
      ) : null}
    </div>
  );
}
