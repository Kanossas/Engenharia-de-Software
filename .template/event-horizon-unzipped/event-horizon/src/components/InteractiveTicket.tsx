import { useState, useRef, useEffect } from "react";
import { motion, useMotionValue, useTransform, useAnimation } from "motion/react";

export function InteractiveTicket({ onTear }: { onTear: () => void }) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [width, setWidth] = useState(320); 

  useEffect(() => {
    if (containerRef.current) {
      setWidth(containerRef.current.offsetWidth);
    }
  }, []);

  const x = useMotionValue(0);
  const controls = useAnimation();

  // Page Curl Math
  const bottomClipPath = useTransform(x, (val) => {
    const v = Math.max(val, 0);
    const d = Math.min(v * 1.5, 300);
    return `polygon(${v}px 0px, 100% 0px, 100% 100%, 0px 100%, 0px ${d}px)`;
  });

  const flapClipPath = useTransform(x, (val) => {
    if (val <= 0.1) return `polygon(0px 0px, 0px 0px, 0px 0px)`;
    const v = val;
    const d = Math.min(v * 1.5, 300); 

    // Geometric mirror of origin (0,0) across the diagonal tear line to fold perfectly
    const tipX = (2 * d * d * v) / (d * d + v * v);
    const tipY = (2 * v * v * d) / (d * d + v * v);
    
    return `polygon(${v}px 0px, ${tipX}px ${tipY}px, 0px ${d}px)`;
  });
  
  const handleDragEnd = async (e: any, info: any) => {
    const threshold = width * 0.5;
    
    if (info.offset.x > threshold || info.velocity.x > 200) {
      // Snap to stop point near the end of the dashed line (doesn't rip 100%)
      controls.start({ x: width * 0.82, transition: { type: "spring", stiffness: 200, damping: 25 } });
      
      // Navigate after snapping
      setTimeout(() => {
        onTear();
      }, 500);
    } else {
      // Snap back to intact ticket
      controls.start({ x: 0, transition: { type: "spring", stiffness: 300, damping: 20 } });
    }
  };

  const TicketContent = ({ idSuffix }: { idSuffix: string }) => (
    <div 
      className="w-full lg:w-[320px] bg-yellow-400 p-10 flex flex-col items-start justify-between absolute left-0 text-black h-[500px]"
      style={{
        clipPath: "polygon(0% 0%, 100% 0%, 100% 36%, 96.5% 40%, 100% 44%, 100% 100%, 0% 100%, 0% 44%, 3.5% 40%, 0% 36%)",
        borderRadius: "40px"
      }}
    >
      <div 
        className="absolute inset-0 opacity-[0.05] pointer-events-none mix-blend-multiply z-10" 
        style={{ 
          backgroundImage: `url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter${idSuffix}'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='5' stitchTiles='stitch'/%3E%3CfeColorMatrix type='matrix' values='0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2.5 -1'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter${idSuffix})'/%3E%3C/svg%3E")`,
          backgroundSize: '200px 200px'
        }} 
      />
      <div className="absolute inset-0 opacity-[0.02] pointer-events-none mix-blend-overlay z-10"
           style={{ 
             backgroundImage: `url("data:image/svg+xml,%3Csvg viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='fiberFilter${idSuffix}'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.02' numOctaves='3'/%3E%3CfeDisplacementMap in='SourceGraphic' scale='15'/%3E%3CfeColorMatrix type='saturate' values='0'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23fiberFilter${idSuffix})'/%3E%3C/svg%3E")`
           }} 
      />

      <div className="w-full relative z-20">
         <h3 className="text-4xl font-bold tracking-tighter mt-4 text-black/60">
           Meus Bilhetes
         </h3>
      </div>

      {/* Dashed Target Line */}
      <div className="absolute top-[40%] left-0 w-full flex justify-between items-center pointer-events-none gap-[2px] z-20 opacity-40">
        {Array.from({ length: 65 }).map((_, i) => (
          <div key={i} className="w-[3px] h-[3px] rounded-full shrink-0" style={{ backgroundColor: "#000000" }} />
        ))}
      </div>

      <div className="w-full flex-1 flex flex-col justify-end relative z-20 pb-4">
        <div className="flex flex-col mt-auto w-full">
          <div className="flex flex-col mb-2">
            <span className="text-[10px] font-bold uppercase tracking-widest text-[#0a0a0a]/50 mb-1">
              Total de Bilhetes
            </span>
            <span className="text-6xl font-black tracking-tighter leading-none text-[#0a0a0a]">
              05
            </span>
          </div>

          <div className="flex flex-row justify-between mt-6 pt-5 border-t border-[#0a0a0a]/10 w-full">
            <div className="flex flex-col">
              <span className="text-[10px] font-bold uppercase tracking-widest text-[#0a0a0a]/50 mb-1">
                Usados
              </span>
              <span className="text-3xl font-black tracking-tighter text-[#0a0a0a]">
                02
              </span>
            </div>
            
            <div className="flex flex-col text-right">
              <span className="text-[10px] font-bold uppercase tracking-widest text-[#0a0a0a]/50 mb-1">
                Por Usar
              </span>
              <span className="text-3xl font-black tracking-tighter text-[#0a0a0a]">
                03
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );

  return (
    <motion.div 
      ref={containerRef}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.3 }}
      className="relative w-full lg:w-[320px] h-[500px]"
      style={{ perspective: 1200 }}
    >
      {/* STATIC TEXT */}
      <div className="absolute top-[176px] left-[20px] text-[10px] font-bold uppercase tracking-widest text-black/50 z-30 pointer-events-none">
        Arraste para Abrir
      </div>

      {/* TOP HALF (Fixed) */}
      <div className="absolute top-0 left-0 w-full h-[200px] overflow-hidden drop-shadow-2xl z-20 pointer-events-none" style={{ transformStyle: "preserve-3d" }}>
         <div className="absolute top-0 left-0 w-full h-[500px]">
             <TicketContent idSuffix="Top" />
         </div>
      </div>

      {/* Dark underlay gap when torn */}
      <div className="absolute top-[198px] left-[20px] w-[90%] h-[20px] bg-black/10 z-0 rounded-full blur-[3px]" />

      {/* BOTTOM HALF (Flat remaining part, clipped by tear) */}
      <div className="absolute top-[200px] left-0 w-full h-[300px] z-10">
         <motion.div className="absolute top-0 left-0 w-full h-full drop-shadow-[0_20px_30px_rgba(0,0,0,0.3)]" style={{ clipPath: bottomClipPath }}>
            <div className="absolute top-[-200px] left-0 w-full h-[500px]">
                <TicketContent idSuffix="Bottom" />
            </div>
         </motion.div>

         {/* FLAP OVERLAY (The paper curling and rolling inward manually drawn) */}
         <motion.div className="absolute top-0 left-0 w-full h-full pointer-events-none z-20 drop-shadow-[2px_5px_8px_rgba(0,0,0,0.5)]">
             <motion.div 
               className="w-full h-full relative overflow-hidden"
               style={{ 
                 clipPath: flapClipPath,
                 // Smooth rounded paper shine from the fold simulating volume
                 background: "linear-gradient(135deg, #fef08a 0%, #eab308 30%, #a16207 70%, #422006 100%)"
               }}
             >
                {/* Paper texture for the back of the ticket */}
                <div 
                  className="absolute inset-0 opacity-[0.1] mix-blend-multiply" 
                  style={{ 
                    backgroundImage: `url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='nfFlap'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='5' stitchTiles='stitch'/%3E%3CfeColorMatrix type='matrix' values='0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2.5 -1'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23nfFlap)'/%3E%3C/svg%3E")`,
                    backgroundSize: '200px 200px'
                  }} 
                />
             </motion.div>
         </motion.div>
      </div>

      {/* DRAG HANDLE */}
      <div className="absolute top-[180px] left-0 w-full h-0 z-40 flex items-center">
        {/* Invisible hit area covering the drag path */}
        <div className="absolute top-[-25px] left-0 w-[90%] h-[80px] pointer-events-none" />
        
        {/* Completely invisible drag thumb, now larger for easy target */}
        <motion.div
          drag="x"
          dragConstraints={{ left: 0, right: width * 0.82 }} // Stop before the rip is complete
          dragElastic={0.05}
          dragMomentum={false}
          onDragEnd={handleDragEnd}
          style={{ x }}
          animate={controls}
          className="absolute left-[-20px] top-[-30px] w-[80px] h-[80px] cursor-grab active:cursor-grabbing z-40"
        />
      </div>
    </motion.div>
  );
}
