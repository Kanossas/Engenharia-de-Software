import { useState } from "react";
import { motion, AnimatePresence } from "motion/react";
import { Calendar, ChevronDown, Clock, Image as ImageIcon, Info, Plus, X } from "lucide-react";

export function CreateEventView() {
  const [imageUrl, setImageUrl] = useState("");
  const [showRulesModal, setShowRulesModal] = useState(false);

  return (
    <motion.div 
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className="max-w-[1600px] mx-auto px-10 pt-20 pb-32"
    >
      {/* Header section matching Home aesthetic */}
      <div className="flex flex-col md:flex-row md:items-end justify-end gap-12 mb-20 relative border-b border-white/10 pb-8">
        <h1 className="text-7xl md:text-[110px] font-medium tracking-tighter leading-none text-right shrink-0 relative">
          Criar Evento
        </h1>
      </div>

      {/* Form Body Container */}
      <div className="w-full max-w-[1000px] mx-auto">
        <div className="flex flex-col gap-y-8">
          
          {/* Row 1: Nome, Data, Hora */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <div className="md:col-span-2">
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Nome do evento</label>
              <input 
                type="text" 
                placeholder="Ex: Holy Priest Rave" 
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
               />
            </div>
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Data</label>
              <div className="relative group">
                <input 
                  type="date" 
                  className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white/50 focus:outline-none focus:text-white focus:border-yellow-400 focus:bg-white/[0.02] transition-colors relative z-10 [&::-webkit-calendar-picker-indicator]:opacity-0 [&::-webkit-calendar-picker-indicator]:absolute [&::-webkit-calendar-picker-indicator]:inset-0 [&::-webkit-calendar-picker-indicator]:w-full [&::-webkit-calendar-picker-indicator]:h-full [&::-webkit-calendar-picker-indicator]:cursor-pointer"
                  style={{ colorScheme: "dark" }}
                />
                <Calendar className="absolute right-6 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50 pointer-events-none group-focus-within:text-yellow-400 transition-colors z-0" />
              </div>
            </div>
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Hora</label>
              <div className="relative group cursor-text">
                <input 
                  type="time" 
                  className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white/50 focus:outline-none focus:text-white focus:border-yellow-400 focus:bg-white/[0.02] transition-colors relative z-10 [&::-webkit-calendar-picker-indicator]:opacity-0 [&::-webkit-calendar-picker-indicator]:absolute [&::-webkit-calendar-picker-indicator]:inset-0 [&::-webkit-calendar-picker-indicator]:w-full [&::-webkit-calendar-picker-indicator]:h-full [&::-webkit-calendar-picker-indicator]:cursor-pointer"
                  style={{ colorScheme: "dark" }}
                />
                <Clock className="absolute right-6 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50 pointer-events-none group-focus-within:text-yellow-400 transition-colors z-0" />
              </div>
            </div>
          </div>

          {/* Row 2: Local, Capacidade Maxima, Preço base */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <div className="md:col-span-2">
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Local</label>
              <input 
                type="text" 
                placeholder="Ex: Porto" 
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
               />
            </div>
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Capacidade maxima</label>
              <input 
                type="number" 
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
               />
            </div>
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Preco base</label>
              <input 
                type="number" 
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
               />
            </div>
          </div>

          {/* Row 3: Bilhetes */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Bilhetes Standard</label>
              <input 
                type="number" 
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
               />
            </div>
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Bilhetes Gold</label>
              <input 
                type="number" 
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
               />
            </div>
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Bilhetes VIP</label>
              <input 
                type="number" 
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
               />
            </div>
          </div>

          {/* Row 4: Categoria */}
          <div className="grid grid-cols-1">
            <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Categoria</label>
            <div className="relative group border border-white/20 rounded-xl focus-within:border-yellow-400 focus-within:bg-white/[0.02] transition-colors">
              <select 
                className="w-full h-full bg-transparent rounded-xl pl-6 pr-12 py-4 text-sm text-white focus:outline-none appearance-none cursor-pointer relative z-10"
                defaultValue=""
              >
                <option value="" disabled hidden className="bg-[#1a1a1a] text-white/50">Seleciona uma categoria</option>
                <option value="techno" className="bg-[#1a1a1a]">Techno</option>
                <option value="house" className="bg-[#1a1a1a]">House</option>
                <option value="live-av" className="bg-[#1a1a1a]">Live AV</option>
              </select>
              <ChevronDown className="absolute right-6 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50 pointer-events-none group-focus-within:text-yellow-400 transition-colors z-20" />
            </div>
            <button className="flex items-center gap-1.5 text-blue-500 border border-blue-500/50 hover:bg-blue-500/10 px-3 py-1.5 rounded-md text-xs w-fit mt-3 transition-colors">
              <Plus className="w-4 h-4" /> Criar Nova
            </button>
          </div>
          
          {/* Row 5: Descrição */}
          <div className="pt-2">
            <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Descricao</label>
            <textarea 
              placeholder=""
              className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-6 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors min-h-[160px] resize-y leading-relaxed"
            ></textarea>
          </div>

          {/* Image Upload section */}
          <div className="pt-2 pb-6 border-b border-white/10">
            <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">URL da Imagem</label>
            <input 
              type="text" 
              placeholder="https://exemplo.com/imagem.jpg" 
              value={imageUrl}
              onChange={(e) => setImageUrl(e.target.value)}
              className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors mb-8"
            />
            {/* Image Preview Container */}
            <div className={`w-full h-[320px] shrink-0 rounded-2xl border border-dashed flex flex-col items-center justify-center p-3 overflow-hidden relative transition-colors ${imageUrl ? 'bg-black/40 border-white/10' : 'bg-black/10 border-white/20 hover:border-white/30'}`}>
              {imageUrl ? (
                <img 
                  src={imageUrl} 
                  alt="Preview da Imagem do Evento" 
                  className="w-full h-full object-cover rounded-xl shadow-2xl" 
                  onError={(e) => e.currentTarget.style.display = 'none'} 
                  onLoad={(e) => e.currentTarget.style.display = 'block'} 
                />
              ) : (
                <div className="text-white/30 flex flex-col items-center gap-4">
                  <ImageIcon className="w-10 h-10 opacity-50" />
                  <span className="text-sm font-mono uppercase tracking-widest text-center">Preview da Imagem</span>
                </div>
              )}
            </div>
          </div>
          
        </div>

        {/* Info Rules Button */}
        <div className="flex justify-start">
          <button 
            type="button" 
            onClick={() => setShowRulesModal(true)} 
            className="flex items-center gap-2 text-cyan-400 hover:text-cyan-300 bg-cyan-400/10 px-4 py-3 rounded-xl text-sm font-mono tracking-wide transition-colors border border-cyan-400/20"
          >
            <Info className="w-5 h-5" /> Consultar Regras dos Bilhetes
          </button>
        </div>

        {/* Footer Actions */}
        <div className="flex justify-end gap-4 pt-8">
          <button className="px-10 py-4 bg-transparent border border-white/20 rounded-xl text-sm text-white font-bold tracking-widest uppercase hover:bg-white/5 hover:border-white transition-all">
            Cancelar
          </button>
          <button className="px-10 py-4 bg-[#0a58ca] hover:bg-[#0b5ed7] text-white rounded-xl text-sm font-bold tracking-widest transition-all drop-shadow-xl">
            Criar Evento
          </button>
        </div>

      </div>

      <AnimatePresence>
        {showRulesModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm">
            <motion.div 
               initial={{ opacity: 0, scale: 0.95 }}
               animate={{ opacity: 1, scale: 1 }}
               exit={{ opacity: 0, scale: 0.95 }}
               className="bg-[#1a1a1a] border border-white/10 rounded-2xl p-8 max-w-lg w-full relative shadow-2xl"
            >
              <button 
                onClick={() => setShowRulesModal(false)} 
                className="absolute top-4 right-4 text-white/50 hover:text-white transition-colors"
                aria-label="Fechar janela"
              >
                <X className="w-5 h-5" />
              </button>
              
              <h2 className="text-xl font-bold uppercase tracking-tighter mb-6 text-cyan-400 flex items-center gap-3 border-b border-white/10 pb-4">
                <Info className="w-6 h-6" /> Regras dos Bilhetes
              </h2>
              
              <div className="space-y-6">
                <div>
                  <h3 className="text-white font-bold mb-1">Standard</h3>
                  <p className="text-white/60 text-sm">Para acesso ao evento apenas.</p>
                </div>
                <div>
                  <h3 className="text-yellow-400 font-bold mb-1">Gold</h3>
                  <p className="text-white/60 text-sm">Para evento + todas as atividades.</p>
                </div>
                <div>
                  <h3 className="text-indigo-400 font-bold mb-1">VIP</h3>
                  <p className="text-white/60 text-sm">Para evento + atividades + zonas restritas.</p>
                </div>
              </div>

              <div className="mt-8 bg-red-500/10 border border-red-500/20 text-red-300 p-4 rounded-xl text-xs font-mono leading-relaxed">
                A soma das quantidades de todos os bilhetes não pode ultrapassar a capacidade máxima do evento.
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </motion.div>
  );
}
