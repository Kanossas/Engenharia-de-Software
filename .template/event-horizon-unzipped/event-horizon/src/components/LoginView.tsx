import { motion } from "motion/react";
import { ArrowLeft, Mail, Lock, ArrowRight } from "lucide-react";

export function LoginView({ onNavigate, onLogin }: { onNavigate: (page: string) => void, onLogin: () => void }) {
  return (
    <motion.div 
      initial={{opacity: 0, y: 20}} 
      animate={{opacity: 1, y: 0}} 
      exit={{opacity: 0, y: -20}} 
      className="min-h-screen flex flex-col pt-24 pb-12 px-4 relative justify-center"
    >
      {/* Background Image Setup */}
      <div className="absolute inset-0 z-0">
        <img 
          src="https://images.unsplash.com/photo-1574391884720-bbc3740c59d1?q=80&w=2000&auto=format&fit=crop" 
          alt="Login Background" 
          className="w-full h-full object-cover opacity-20 grayscale" 
        />
        <div className="absolute inset-0 bg-gradient-to-t from-[#333533] via-[#333533]/80 to-[#333533]/90" />
      </div>

      <div className="flex flex-col items-center justify-center flex-1 z-10 w-full max-w-[1600px] mx-auto">
        <div className="w-full max-w-md bg-[#0a0a0a]/60 backdrop-blur-xl border border-white/10 p-10 md:p-12 rounded-[30px] shadow-2xl relative">
          
          <button 
            onClick={() => onNavigate('home')} 
            className="flex items-center gap-2 text-white/50 hover:text-yellow-400 font-mono text-xs uppercase tracking-widest transition-colors mb-10 group w-fit"
          >
            <ArrowLeft className="w-4 h-4 group-hover:-translate-x-1 transition-transform" /> Voltar
          </button>

          <h1 className="text-4xl md:text-5xl font-bold tracking-tighter uppercase mb-2 leading-none">
            Bem-vindo<br/><span className="text-yellow-400">de volta</span>
          </h1>
          <p className="text-white/50 text-sm font-mono tracking-wide mb-10">Introduz as tuas credenciais para entrar.</p>

          <form 
            className="space-y-5" 
            onSubmit={(e) => { 
                e.preventDefault(); 
                onLogin();
                onNavigate('home');
            }}
          >
            <div className="space-y-2">
              <label className="text-[10px] font-mono text-white/50 uppercase tracking-widest ml-1 block">Email</label>
              <div className="relative group">
                <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-white/40 group-focus-within:text-yellow-400 transition-colors" />
                <input 
                  type="email" 
                  placeholder="teu@email.com" 
                  required 
                  className="w-full bg-white/[0.03] border border-white/10 rounded-xl pl-12 pr-4 py-3.5 text-sm text-white placeholder-white/20 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors" 
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-[10px] font-mono text-white/50 uppercase tracking-widest ml-1 block">Password</label>
              <div className="relative group">
                <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-white/40 group-focus-within:text-yellow-400 transition-colors" />
                <input 
                  type="password" 
                  placeholder="••••••••" 
                  required 
                  className="w-full bg-white/[0.03] border border-white/10 rounded-xl pl-12 pr-4 py-3.5 text-sm text-white placeholder-white/20 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors" 
                />
              </div>
            </div>

            <div className="flex items-center justify-end pt-2">
              <button type="button" className="text-[11px] font-mono text-white/40 hover:text-yellow-400 uppercase tracking-widest transition-colors">
                Esqueceste-te da password?
              </button>
            </div>

            <button 
              type="submit" 
              className="w-full group/btn relative flex items-center justify-center gap-2 bg-yellow-400 text-black px-8 py-4 rounded-xl font-bold uppercase tracking-widest hover:bg-yellow-300 transition-all duration-300 mt-8 shadow-[0_0_20px_rgba(250,204,21,0.15)] hover:shadow-[0_0_30px_rgba(250,204,21,0.3)]"
            >
              <span>Entrar na dimensão</span>
              <ArrowRight className="w-5 h-5 group-hover/btn:translate-x-1 transition-transform" />
            </button>
          </form>

          <div className="mt-10 text-center border-t border-white/5 pt-8">
            <p className="text-xs text-white/50 font-mono tracking-wide">
              Não tens bilhete para esta viagem? <br className="md:hidden" />
              <button onClick={() => onNavigate('register')} className="text-yellow-400 hover:text-yellow-300 uppercase tracking-widest font-bold ml-1 transition-colors">
                Regista-te
              </button>
            </p>
          </div>
        </div>
      </div>
    </motion.div>
  );
}
