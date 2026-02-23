// wwwroot/js/bookOfKindness.js
(function () {
  const elQuote = document.getElementById("bokQuoteText");
  const elAuthor = document.getElementById("bokQuoteAuthor");
  const elTags = document.getElementById("bokQuoteTags");
  const elAutoBtn = document.getElementById("bokAutoBtn");
  const elAutoText = document.getElementById("bokAutoText");
  const elShuffleBtn = document.getElementById("bokShuffleBtn");
  const elSaveBtn = document.getElementById("bokSaveBtn");
  const elCopyBtn = document.getElementById("bokCopyBtn");
  const elProgress = document.getElementById("bokProgressBar");
  const elToast = document.getElementById("bokToast");
  const elPagePill = document.getElementById("bokPagePill");
  const elSavedCount = document.getElementById("bokSavedCount");

  if (!elQuote || !elAuthor || !elTags || !elAutoBtn || !elShuffleBtn || !elSaveBtn || !elCopyBtn || !elProgress) {
    return;
  }

  // Short quotes only (no long copyrighted text)
  const QUOTES = [
    { text: "In a gentle way, you can shake the world.", author: "Mahatma Gandhi", tags: ["Gentle power", "Courage"] },
    { text: "No one has ever become poor by giving.", author: "Anne Frank", tags: ["Giving", "Hope"] },
    { text: "What you do makes a difference, and you have to decide what kind of difference you want to make.", author: "Jane Goodall", tags: ["Choice", "Impact"] },
    { text: "If you want others to be happy, practice compassion.", author: "Dalai Lama", tags: ["Compassion", "Practice"] },
    { text: "Kind words can be short and easy to speak, but their echoes are truly endless.", author: "Mother Teresa", tags: ["Kindness", "Words"] },
    { text: "The best way to find yourself is to lose yourself in the service of others.", author: "Mahatma Gandhi", tags: ["Service", "Meaning"] },
    { text: "Do small things with great love.", author: "Mother Teresa", tags: ["Small acts", "Love"] },
    { text: "Be kind whenever possible. It is always possible.", author: "Dalai Lama", tags: ["Always", "Kindness"] },
    { text: "We rise by lifting others.", author: "Robert Ingersoll", tags: ["Lift", "Together"] },
    { text: "A little help is worth a great deal of pity.", author: "Kahlil Gibran", tags: ["Help", "Action"] },
  ];

  const LS_AUTO = "karuna_bok_auto";
  const LS_INDEX = "karuna_bok_index";
  const LS_SAVED = "karuna_bok_saved";

  const ROTATE_MS = 12000; // 12 seconds feels like a calm “page turn”

  let auto = (localStorage.getItem(LS_AUTO) ?? "true") === "true";
  let index = parseInt(localStorage.getItem(LS_INDEX) || "", 10);
  if (!Number.isFinite(index) || index < 0 || index >= QUOTES.length) {
    index = todaysIndex();
  }

  let timer = null;
  let progressTimer = null;
  let progressStart = 0;

  function todaysIndex() {
    // Deterministic “today’s page” so it feels like a ritual
    const d = new Date();
    const start = new Date(d.getFullYear(), 0, 0);
    const diff = d - start;
    const oneDay = 1000 * 60 * 60 * 24;
    const dayOfYear = Math.floor(diff / oneDay);
    return dayOfYear % QUOTES.length;
  }

  function toast(msg) {
    if (!elToast) return;
    elToast.textContent = msg;
    elToast.classList.add("show");
    window.clearTimeout(elToast._t);
    elToast._t = window.setTimeout(() => elToast.classList.remove("show"), 1600);
  }

  function render(i) {
    const q = QUOTES[i];
    elQuote.textContent = q.text;
    elAuthor.textContent = "— " + q.author;

    elTags.innerHTML = "";
    (q.tags || []).forEach(t => {
      const span = document.createElement("span");
      span.className = "bok-tag";
      span.textContent = t;
      elTags.appendChild(span);
    });

    elPagePill.textContent = `Page ${i + 1}`;
    localStorage.setItem(LS_INDEX, String(i));
    updateSavedCount();
  }

  function updateSavedCount() {
    try {
      const saved = JSON.parse(localStorage.getItem(LS_SAVED) || "[]");
      const count = Array.isArray(saved) ? saved.length : 0;
      if (elSavedCount) elSavedCount.textContent = `Saved: ${count}`;
    } catch {
      if (elSavedCount) elSavedCount.textContent = "Saved: 0";
    }
  }

  function startProgress() {
    stopProgress();
    progressStart = Date.now();
    elProgress.style.width = "0%";
    progressTimer = window.setInterval(() => {
      const pct = Math.min(100, ((Date.now() - progressStart) / ROTATE_MS) * 100);
      elProgress.style.width = pct.toFixed(2) + "%";
    }, 120);
  }

  function stopProgress() {
    if (progressTimer) window.clearInterval(progressTimer);
    progressTimer = null;
  }

  function setAuto(next) {
    auto = next;
    localStorage.setItem(LS_AUTO, String(auto));
    elAutoText.textContent = auto ? "Auto" : "Paused";
    elAutoBtn.setAttribute("aria-pressed", auto ? "true" : "false");

    if (auto) {
      startAuto();
      toast("Auto rotation on");
    } else {
      stopAuto();
      toast("Paused");
    }
  }

  function startAuto() {
    stopAuto();
    startProgress();
    timer = window.setInterval(() => {
      nextQuote();
    }, ROTATE_MS);
  }

  function stopAuto() {
    if (timer) window.clearInterval(timer);
    timer = null;
    stopProgress();
    elProgress.style.width = "0%";
  }

  function nextQuote() {
    index = (index + 1) % QUOTES.length;
    render(index);
    if (auto) startProgress();
  }

  function saveCurrent() {
    try {
      const saved = JSON.parse(localStorage.getItem(LS_SAVED) || "[]");
      const list = Array.isArray(saved) ? saved : [];
      const q = QUOTES[index];
      const key = `${q.text}||${q.author}`;

      if (list.some(x => x.key === key)) {
        toast("Already saved");
        return;
      }

      list.unshift({
        key,
        text: q.text,
        author: q.author,
        tags: q.tags || [],
        savedAt: new Date().toISOString(),
      });

      localStorage.setItem(LS_SAVED, JSON.stringify(list.slice(0, 50)));
      updateSavedCount();
      toast("Saved to your pages");
    } catch {
      toast("Could not save (storage blocked)");
    }
  }

  function copyLink() {
    const url = new URL(window.location.href);
    url.searchParams.set("bok", String(index));

    const toCopy = url.toString();
    navigator.clipboard.writeText(toCopy)
      .then(() => toast("Link copied"))
      .catch(() => toast("Could not copy link"));
  }

  // If link has bok index: ?bok=3
  try {
    const url = new URL(window.location.href);
    const bokParam = parseInt(url.searchParams.get("bok") || "", 10);
    if (Number.isFinite(bokParam) && bokParam >= 0 && bokParam < QUOTES.length) {
      index = bokParam;
    }
  } catch {}

  // Events
  elShuffleBtn.addEventListener("click", () => {
    nextQuote();
    toast("Turned the page");
  });

  elSaveBtn.addEventListener("click", () => saveCurrent());
  elCopyBtn.addEventListener("click", () => copyLink());

  elAutoBtn.addEventListener("click", () => {
    setAuto(!auto);
  });

  // First paint
  render(index);
  elAutoText.textContent = auto ? "Auto" : "Paused";
  elAutoBtn.setAttribute("aria-pressed", auto ? "true" : "false");
  updateSavedCount();
  if (auto) startAuto();
})();
