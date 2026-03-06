console.log("BOOK OF KINDNESS JS NEW VERSION LOADED");
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
  { text: "No act of kindness, no matter how small, is ever wasted.", author: "Aesop", tags: ["Kindness", "Impact"] },
  { text: "In a gentle way, you can shake the world.", author: "Mahatma Gandhi", tags: ["Gentle power", "Courage"] },
  { text: "Act as if what you do makes a difference. It does.", author: "William James", tags: ["Action", "Impact"] },
  { text: "Where there is kindness, there is goodness. Where there is goodness, there is magic.", author: "Unknown", tags: ["Kindness", "Goodness"] },
  { text: "Thousands of candles can be lit from a single candle.", author: "Buddha", tags: ["Giving", "Light"] },
  { text: "The smallest act of kindness is worth more than the grandest intention.", author: "Oscar Wilde", tags: ["Action", "Kindness"] },
  { text: "Be the reason someone believes in the goodness of people.", author: "Unknown", tags: ["Hope", "Goodness"] },
  { text: "Kindness is a language which the deaf can hear and the blind can see.", author: "Mark Twain", tags: ["Kindness", "Universal"] },
  { text: "Carry out a random act of kindness, with no expectation of reward.", author: "Princess Diana", tags: ["Generosity", "Service"] },
  { text: "A warm smile is the universal language of kindness.", author: "William Arthur Ward", tags: ["Smile", "Kindness"] },
  { text: "No one has ever become poor by giving.", author: "Anne Frank", tags: ["Giving", "Hope"] },
  { text: "Try to be a rainbow in someone else’s cloud.", author: "Maya Angelou", tags: ["Encouragement", "Care"] },
  { text: "Too often we underestimate the power of a touch, a smile, a kind word, or the smallest act of caring.", author: "Leo Buscaglia", tags: ["Care", "Small acts"] },
  { text: "Love and kindness are never wasted.", author: "Barbara De Angelis", tags: ["Love", "Kindness"] },
  { text: "Unexpected kindness is the most powerful, least costly, and most underrated agent of human change.", author: "Bob Kerrey", tags: ["Change", "Kindness"] },
  { text: "How beautiful a day can be when kindness touches it.", author: "George Elliston", tags: ["Beauty", "Kindness"] },
  { text: "You cannot do a kindness too soon, for you never know how soon it will be too late.", author: "Ralph Waldo Emerson", tags: ["Urgency", "Kindness"] },
  { text: "Kindness begins with the understanding that we all struggle.", author: "Charles Glassman", tags: ["Empathy", "Understanding"] },
  { text: "Be kind whenever possible. It is always possible.", author: "Dalai Lama", tags: ["Always", "Kindness"] },
  { text: "Remember there’s no such thing as a small act of kindness. Every act creates a ripple.", author: "Scott Adams", tags: ["Ripple", "Impact"] },
  { text: "One kind word can warm three winter months.", author: "Japanese Proverb", tags: ["Words", "Warmth"] },
  { text: "Kind hearts are the gardens, kind thoughts are the roots, kind words are the blossoms, kind deeds are the fruits.", author: "Henry Wadsworth Longfellow", tags: ["Kindness", "Growth"] },
  { text: "Do things for people not because of who they are or what they do in return, but because of who you are.", author: "Harold S. Kushner", tags: ["Character", "Giving"] },
  { text: "There is no exercise better for the heart than reaching down and lifting people up.", author: "John Holmes", tags: ["Lift others", "Heart"] },
  { text: "The best portion of a good life is the little, nameless, unremembered acts of kindness and love.", author: "William Wordsworth", tags: ["Life", "Small acts"] },
  { text: "Great opportunities to help others seldom come, but small ones surround us every day.", author: "Sally Koch", tags: ["Opportunity", "Daily kindness"] },
  { text: "Three things in human life are important: the first is to be kind; the second is to be kind; and the third is to be kind.", author: "Henry James", tags: ["Priority", "Kindness"] },
  { text: "Do all the good you can, by all the means you can, in all the ways you can.", author: "John Wesley", tags: ["Do good", "Service"] },
  { text: "Only a life lived for others is a life worthwhile.", author: "Albert Einstein", tags: ["Purpose", "Service"] },
  { text: "If you light a lamp for someone else, it will also brighten your path.", author: "Buddha", tags: ["Giving", "Light"] },
  { text: "A little thought and a little kindness are often worth more than a great deal of money.", author: "John Ruskin", tags: ["Kindness", "Value"] },
  { text: "We rise by lifting others.", author: "Robert Ingersoll", tags: ["Lift", "Together"] },
  { text: "You have not lived today until you have done something for someone who can never repay you.", author: "John Bunyan", tags: ["Giving", "Selflessness"] },
  { text: "A single act of kindness throws out roots in all directions.", author: "Amelia Earhart", tags: ["Ripple", "Roots"] },
  { text: "Sometimes it takes only one act of kindness and caring to change a person’s life.", author: "Jackie Chan", tags: ["Change", "Caring"] },
  { text: "What wisdom can you find that is greater than kindness?", author: "Jean-Jacques Rousseau", tags: ["Wisdom", "Kindness"] },
  { text: "True beauty is born through our actions and aspirations and in the kindness we offer to others.", author: "Alek Wek", tags: ["Beauty", "Action"] },
  { text: "Constant kindness can accomplish much.", author: "Albert Schweitzer", tags: ["Consistency", "Kindness"] },
  { text: "An effort made for the happiness of others lifts us above ourselves.", author: "Lydia M. Child", tags: ["Happiness", "Service"] },
  { text: "No one is useless in this world who lightens the burden of another.", author: "Charles Dickens", tags: ["Help", "Purpose"] },
  { text: "We make a living by what we get. We make a life by what we give.", author: "Winston Churchill", tags: ["Giving", "Life"] },
  { text: "The only gift is a portion of thyself.", author: "Ralph Waldo Emerson", tags: ["Giving", "Self"] },
  { text: "You give but little when you give of your possessions. It is when you give of yourself that you truly give.", author: "Kahlil Gibran", tags: ["Giving", "Self"] },
  { text: "A kind gesture can reach a wound that only compassion can heal.", author: "Steve Maraboli", tags: ["Compassion", "Healing"] },
  { text: "From caring comes courage.", author: "Lao Tzu", tags: ["Care", "Courage"] },
  { text: "The smallest deed is better than the greatest intention.", author: "John Burroughs", tags: ["Action", "Deeds"] },
  { text: "Let us be kinder to one another.", author: "Aldous Huxley", tags: ["Community", "Kindness"] },
  { text: "Guard well within yourself that treasure, kindness.", author: "George Sand", tags: ["Inner life", "Kindness"] },
  { text: "Compassion is the wish to see others free from suffering.", author: "Dalai Lama", tags: ["Compassion", "Relief"] },
  { text: "A good head and a good heart are always a formidable combination.", author: "Nelson Mandela", tags: ["Wisdom", "Heart"] },
  { text: "The whole purpose of life is to be useful, honorable, and compassionate.", author: "Leo Rosten", tags: ["Purpose", "Compassion"] },
  { text: "Helping one person might not change the whole world, but it could change the world for one person.", author: "Unknown", tags: ["Impact", "Help"] },
  { text: "The best way to cheer yourself is to try to cheer somebody else up.", author: "Mark Twain", tags: ["Joy", "Encouragement"] },
  { text: "Small acts, when multiplied by millions of people, can transform the world.", author: "Howard Zinn", tags: ["Collective impact", "Action"] },
  { text: "The purpose of human life is to serve, and to show compassion and the will to help others.", author: "Albert Schweitzer", tags: ["Purpose", "Service"] },
  { text: "Wherever there is a human being, there is an opportunity for kindness.", author: "Seneca", tags: ["Opportunity", "Humanity"] },
  { text: "Be somebody who makes everybody feel like a somebody.", author: "Kid President", tags: ["Belonging", "Care"] },
  { text: "When you are kind to others, it not only changes you, it changes the world.", author: "Harold S. Kushner", tags: ["Change", "Kindness"] },
  { text: "A candle loses nothing by lighting another candle.", author: "James Keller", tags: ["Sharing", "Light"] },
  { text: "The world is full of kind people. If you can’t find one, be one.", author: "Unknown", tags: ["Leadership", "Kindness"] },
  { text: "No beauty shines brighter than that of a good heart.", author: "Unknown", tags: ["Heart", "Beauty"] }
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
