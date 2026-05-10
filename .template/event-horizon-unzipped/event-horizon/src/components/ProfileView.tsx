import { motion } from "motion/react";
import { ArrowLeft, Edit3, User, Mail, Phone, Shield } from "lucide-react";

export function ProfileView({ user, onNavigate }: { user: any, onNavigate: (page: string) => void }) {
  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="w-full min-h-screen relative flex flex-col pt-32 pb-32 px-4"
    >
      {/* Background Image Setup */}
      <div className="absolute inset-0 z-0">
        <img 
          src="https://images.unsplash.com/photo-1549490349-8643362247b5?q=80&w=2000&auto=format&fit=crop" 
          alt="Profile Background" 
          className="w-full h-full object-cover opacity-20 grayscale" 
        />
        <div className="absolute inset-0 bg-gradient-to-t from-[#333533] via-[#333533]/80 to-[#333533]/90" />
      </div>

      <div className="max-w-3xl mx-auto w-full relative z-10">
        {/* Header */}
        <button 
          onClick={() => onNavigate('home')}
          className="flex items-center gap-2 text-white/50 hover:text-yellow-400 font-mono text-sm uppercase tracking-widest transition-colors mb-12 group w-fit"
        >
          <ArrowLeft className="w-4 h-4 group-hover:-translate-x-1 transition-transform" /> Voltar
        </button>

        <h1 className="text-6xl md:text-[90px] font-medium tracking-tighter leading-none mb-16">
          O Meu Perfil
        </h1>

        <div className="w-full bg-[#0a0a0a]/60 backdrop-blur-xl border border-white/10 rounded-[30px] p-10 md:p-16 relative overflow-hidden shadow-2xl">
        {/* Decorative element */}
        <div className="absolute top-0 right-0 w-64 h-64 bg-yellow-400/5 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2 pointer-events-none" />

        <div className="flex flex-col md:flex-row md:items-center justify-between mb-12 border-b border-white/10 pb-8 gap-6 relative z-10">
          <h2 className="text-3xl font-bold tracking-tighter uppercase">Informações da Conta</h2>
          <div className="flex gap-4">
            <button 
              onClick={() => onNavigate('edit_profile')}
              className="flex items-center gap-2 px-6 py-3 bg-white/5 hover:bg-white/10 border border-white/20 rounded-full text-xs font-bold font-mono uppercase tracking-widest transition-colors shadow-lg hover:border-yellow-400 hover:text-yellow-400"
            >
              <Edit3 className="w-4 h-4" /> Editar Perfil
            </button>
          </div>
        </div>

        <div className="space-y-10 relative z-10">
          <div className="flex flex-col md:flex-row md:items-center gap-2 md:gap-10">
            <div className="w-40 text-white/40 font-mono uppercase tracking-widest text-xs flex items-center gap-2">
              <User className="w-4 h-4" /> Nome
            </div>
            <div className="text-xl md:text-2xl font-medium">{user.name}</div>
          </div>
          
          <div className="flex flex-col md:flex-row md:items-center gap-2 md:gap-10">
            <div className="w-40 text-white/40 font-mono uppercase tracking-widest text-xs flex items-center gap-2">
              <Mail className="w-4 h-4" /> Email
            </div>
            <div className="text-xl md:text-2xl font-medium">{user.email}</div>
          </div>

          <div className="flex flex-col md:flex-row md:items-center gap-2 md:gap-10">
            <div className="w-40 text-white/40 font-mono uppercase tracking-widest text-xs flex items-center gap-2">
              <Phone className="w-4 h-4" /> Telemóvel
            </div>
            <div className="text-xl md:text-2xl font-medium">{user.phone || "Não definido"}</div>
          </div>

          <div className="flex flex-col md:flex-row md:items-center gap-2 md:gap-10">
            <div className="w-40 text-white/40 font-mono uppercase tracking-widest text-xs flex items-center gap-2">
              <Shield className="w-4 h-4" /> Tipo
            </div>
            <div>
              <span className="inline-block px-4 py-2 bg-yellow-400 text-black text-[10px] font-black uppercase tracking-widest rounded-full shadow-[0_0_15px_rgba(250,204,21,0.2)]">
                {user.type}
              </span>
            </div>
          </div>
        </div>
      </div>
      </div>
    </motion.div>
  );
}
