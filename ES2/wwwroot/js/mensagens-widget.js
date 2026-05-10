(() => {
  function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text ?? "";
    return div.innerHTML;
  }

  function initMensagensWidget() {
    const root = document.querySelector("#eh-msg-widget");
    if (!root) return;

    const csrf = root.getAttribute("data-csrf");
    const toggle = root.querySelector("#eh-msg-toggle");
    const panel = root.querySelector("#eh-msg-panel");
    const closeBtn = root.querySelector("#eh-msg-close");
    const list = root.querySelector("#eh-msg-list");
    const badge = root.querySelector("#eh-msg-badge");

    let isOpen = false;
    let pollHandle = null;

    function setOpen(nextOpen) {
      isOpen = nextOpen;
      panel?.classList.toggle("eh-msg-hidden", !nextOpen);
      toggle?.setAttribute("aria-expanded", nextOpen ? "true" : "false");
    }

    function setBadgeCount(n) {
      const count = Number(n) || 0;
      badge?.classList.toggle("eh-msg-hidden", count <= 0);
      if (badge) badge.textContent = count > 99 ? "99+" : String(count);
    }

    async function load() {
      try {
        const res = await fetch("/api/mensagens?take=15", { credentials: "same-origin" });
        if (!res.ok) throw new Error();
        const data = await res.json();

        const items = data?.items ?? [];
        setBadgeCount(data?.count ?? items.length);

        if (!list) return;
        if (items.length === 0) {
          list.innerHTML = '<div class="eh-msg-empty">Sem notificações.</div>';
          return;
        }

        list.innerHTML = items
          .map(
            (it) => `
              <div class="eh-msg-item" data-id="${it.id}">
                <div class="eh-msg-text">${escapeHtml(it.conteudo)}</div>
                <button type="button" class="eh-msg-dismiss" title="Remover" aria-label="Remover">
                  <i class="bi bi-trash3"></i>
                </button>
              </div>
            `
          )
          .join("");

        list.querySelectorAll(".eh-msg-dismiss").forEach((btn) => {
          btn.addEventListener("click", async (e) => {
            const itemEl = e.currentTarget.closest(".eh-msg-item");
            const id = Number(itemEl?.getAttribute("data-id"));
            if (!id) return;
            await dismiss(id);
          });
        });
      } catch {
        root.classList.add("eh-msg-hidden");
        if (pollHandle) clearInterval(pollHandle);
      }
    }

    async function dismiss(id) {
      const headers = csrf
        ? { RequestVerificationToken: csrf }
        : undefined;

      const res = await fetch(`/api/mensagens/${id}/dismiss`, {
        method: "POST",
        headers,
        credentials: "same-origin",
      });
      if (!res.ok) return;

      await load();
    }

    toggle?.addEventListener("click", async () => {
      setOpen(!isOpen);
      if (isOpen) await load();
    });

    closeBtn?.addEventListener("click", () => setOpen(false));
    document.addEventListener("keydown", (e) => {
      if (e.key === "Escape") setOpen(false);
    });
    document.addEventListener("click", (e) => {
      if (!isOpen) return;
      if (!root.contains(e.target)) setOpen(false);
    });

    setOpen(false);
    setBadgeCount(0);

    load();
    pollHandle = setInterval(load, 15000);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initMensagensWidget);
  } else {
    initMensagensWidget();
  }
})();

