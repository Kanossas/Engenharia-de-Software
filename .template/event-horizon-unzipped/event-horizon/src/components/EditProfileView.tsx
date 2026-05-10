import React, { useState } from "react";
import { motion } from "motion/react";
import { ArrowLeft, User, Mail, Phone, Save, X } from "lucide-react";

export function EditProfileView({ user, onNavigate, onSave }: { user: any, onNavigate: (page: string) => void, onSave: (user: any) => void }) {
  const [formData, setFormData] = useState({
    name: user.name,
    email: user.email,
    phone: user.phone || ""
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave({ ...user, ...formData });
    onNavigate('profile');
  };

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

      <div className="max-w-2xl mx-auto w-full relative z-10">
        {/* Header */}
        <button 
          onClick={() => onNavigate('profile')}
          className="flex items-center gap-2 text-white/50 hover:text-yellow-400 font-mono text-sm uppercase tracking-widest transition-colors mb-12 group w-fit"
        >
          <ArrowLeft className="w-4 h-4 group-hover:-translate-x-1 transition-transform" /> Voltar
        </button>

        <h1 className="text-6xl md:text-[90px] font-medium tracking-tighter leading-none mb-16 relative">
          Editar<br />
          <span className="text-yellow-400">Perfil</span>
        </h1>

        <div className="w-full bg-[#0a0a0a]/60 backdrop-blur-xl border border-white/10 rounded-[30px] p-10 md:p-16 relative shadow-2xl overflow-hidden">
        {/* Decorative element */}
        <div className="absolute top-0 right-0 w-64 h-64 bg-yellow-400/5 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2 pointer-events-none" />

        <form onSubmit={handleSubmit} className="space-y-8 relative z-10">
          
          <div className="space-y-3">
            <label className="text-[10px] font-mono text-white/50 uppercase tracking-widest ml-1 block">Nome</label>
            <div className="relative group">
              <User className="absolute left-5 top-1/2 -translate-y-1/2 w-5 h-5 text-white/40 group-focus-within:text-yellow-400 transition-colors" />
              <input 
                type="text" 
                value={formData.name}
                onChange={(e) => setFormData({...formData, name: e.target.value})}
                required 
                className="w-full bg-white/[0.03] border border-white/10 rounded-2xl pl-14 pr-5 py-4 text-white placeholder-white/20 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors text-lg" 
              />
            </div>
          </div>

          <div className="space-y-3">
            <label className="text-[10px] font-mono text-white/50 uppercase tracking-widest ml-1 block">Email</label>
            <div className="relative group">
              <Mail className="absolute left-5 top-1/2 -translate-y-1/2 w-5 h-5 text-white/40 group-focus-within:text-yellow-400 transition-colors" />
              <input 
                type="email" 
                value={formData.email}
                onChange={(e) => setFormData({...formData, email: e.target.value})}
                required 
                className="w-full bg-white/[0.03] border border-white/10 rounded-2xl pl-14 pr-5 py-4 text-white placeholder-white/20 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors text-lg" 
              />
            </div>
          </div>

          <div className="space-y-3">
            <label className="text-[10px] font-mono text-white/50 uppercase tracking-widest ml-1 block">Telemóvel</label>
            <div className="relative group">
              <Phone className="absolute left-5 top-1/2 -translate-y-1/2 w-5 h-5 text-white/40 group-focus-within:text-yellow-400 transition-colors" />
              <input 
                type="tel" 
                value={formData.phone}
                onChange={(e) => setFormData({...formData, phone: e.target.value})}
                className="w-full bg-white/[0.03] border border-white/10 rounded-2xl pl-14 pr-5 py-4 text-white placeholder-white/20 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors text-lg" 
              />
            </div>
          </div>

          <div className="flex flex-col sm:flex-row items-center gap-4 pt-8 border-t border-white/10">
            <button 
              type="submit" 
              className="w-full sm:w-auto flex flex-1 items-center justify-center gap-2 bg-yellow-400 text-black px-8 py-4 rounded-xl font-bold uppercase tracking-widest hover:bg-yellow-300 transition-all shadow-[0_0_15px_rgba(250,204,21,0.2)] hover:shadow-[0_0_25px_rgba(250,204,21,0.4)]"
            >
              <Save className="w-5 h-5" /> Guardar Alterações
            </button>
            <button 
              type="button" 
              onClick={() => onNavigate('profile')}
              className="w-full sm:w-auto flex items-center justify-center gap-2 bg-white/5 border border-white/20 text-white px-8 py-4 rounded-xl font-bold uppercase tracking-widest hover:bg-white/10 hover:border-white/40 transition-colors"
            >
              <X className="w-5 h-5" /> Cancelar
            </button>
          </div>

        </form>
      </div>
      </div>
    </motion.div>
  );
}
