// Home page hero behaviour:
//   - Real Estate quick-search (Buy/Rent + cascading property type → details)
//     submits to /Properties with offer / type / location query params.
//   - Stats counter animation (uses real DB-backed counts from
//     IHomeStatsService — see Pages/Index.cshtml.cs).
//   - Fade-up reveal animation.
// External so it can be loaded with the CSP nonce.
(function () {
  'use strict';

  // ── Real Estate quick search ───────────────────────────────
  // Cascading Property Details: the option lists live in
  //   data-details-residential / data-details-commercial on #heroReDetails,
  // so the markup is the single source of truth and the page works even
  // without JS (defaults to Residential options at render time).
  var reForm = document.getElementById('heroReSearch');
  if (reForm) {
    var category = document.getElementById('heroReCategory');
    var details  = document.getElementById('heroReDetails');
    var offer    = document.getElementById('heroReOffer');
    var modeTabs = reForm.querySelectorAll('[data-re-mode]');

    function readDetailsList(cat) {
      if (!details) return [];
      var raw = details.getAttribute('data-details-' + String(cat || '').toLowerCase());
      if (!raw) return [];
      try { return JSON.parse(raw); } catch (e) { return []; }
    }

    function repopulateDetails() {
      if (!category || !details) return;
      var list = readDetailsList(category.value);
      if (!list.length) return;

      // ── Native <select>: rebuild options directly ──────────
      details.innerHTML = '';
      list.forEach(function (label) {
        var opt = document.createElement('option');
        opt.value = label;
        opt.textContent = label;
        details.appendChild(opt);
      });

      // ── TomSelect: sync internal options if present ─────────
      var ts = details.tomselect;
      if (ts) {
        try {
          ts.clear(true);
          ts.clearOptions();
          list.forEach(function (label) {
            ts.addOption({ value: label, text: label });
          });
          ts.refreshOptions(false);
          ts.setValue(list[0], true);
        } catch (e) {}
      }
    }

    if (category) {
      // Native change event (works for plain selects and TomSelect both dispatch it)
      category.addEventListener('change', repopulateDetails);
    }

    // Run once after TomSelect has had a chance to initialise (it boots on DOMContentLoaded)
    // so the Property Details list reflects the default Property Type correctly.
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', repopulateDetails);
    } else {
      // Already interactive — defer one tick so TomSelect's own DOMContentLoaded
      // handler runs first (tom-select-init.js also listens on DOMContentLoaded).
      setTimeout(repopulateDetails, 0);
    }

    modeTabs.forEach(function (t) {
      t.addEventListener('click', function () {
        modeTabs.forEach(function (o) {
          o.classList.toggle('active', o === t);
          o.setAttribute('aria-selected', o === t ? 'true' : 'false');
        });
        if (offer) offer.value = t.dataset.reMode || 'sale';
      });
    });
  }

  // ── Stats counter animation ────────────────────────────────
  var observed = new Set();
  var statsIo = new IntersectionObserver(function (entries) {
    entries.forEach(function (e) {
      if (!e.isIntersecting || observed.has(e.target)) return;
      observed.add(e.target);
      var el = e.target;
      var end = parseInt(el.dataset.count, 10) || 0;
      var suffix = el.dataset.suffix || '';
      var duration = 1600;
      var startTime = null;
      function step(ts) {
        if (!startTime) startTime = ts;
        var p = Math.min((ts - startTime) / duration, 1);
        var val = Math.floor(p * end);
        el.textContent = val.toLocaleString() + suffix;
        if (p < 1) requestAnimationFrame(step);
        else el.textContent = end.toLocaleString() + suffix;
      }
      requestAnimationFrame(step);
    });
  }, { threshold: .3 });
  document.querySelectorAll('[data-count]').forEach(function (el) { statsIo.observe(el); });

  // ── Fade-up reveal ─────────────────────────────────────────
  var fadeIo = new IntersectionObserver(function (entries) {
    entries.forEach(function (e) {
      if (e.isIntersecting) { e.target.classList.add('is-visible'); fadeIo.unobserve(e.target); }
    });
  }, { threshold: 0.08 });
  document.querySelectorAll('.fade-up').forEach(function (el) { fadeIo.observe(el); });

  // ── Hero background slideshow ──────────────────────────────
  var slides = document.querySelectorAll('#heroSlider .ph-hero__slide');
  if (slides.length > 1) {
    var current = 0;
    var INTERVAL = 6000;
    function nextSlide() {
      slides[current].classList.remove('ph-hero__slide--active');
      current = (current + 1) % slides.length;
      slides[current].classList.add('ph-hero__slide--active');
    }
    setInterval(nextSlide, INTERVAL);
  }
})();
