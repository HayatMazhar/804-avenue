// Home page hero behaviour:
//   - 4-tab service quote widget (Maintenance / AMC / Contracting / Facility Mgmt)
//     → routes the visitor to the deeper form anchor on the relevant
//       service page, with a hash that scrolls to the form.
//   - Stats counter animation (uses real DB-backed counts from
//     IHomeStatsService — see Pages/Index.cshtml.cs).
//   - Fade-up reveal animation.
// External so it can be loaded with the CSP nonce.
(function () {
  'use strict';

  // ── 3-tab service quote widget ─────────────────────────────
  var widget = document.getElementById('heroQuoteWidget');
  if (widget) {
    var tabs   = widget.querySelectorAll('[data-quote-tab]');
    var panels = widget.querySelectorAll('[data-quote-panel]');

    function showPanel(name) {
      tabs.forEach(function (t) {
        var on = t.dataset.quoteTab === name;
        t.classList.toggle('active', on);
        t.setAttribute('aria-selected', on ? 'true' : 'false');
      });
      panels.forEach(function (p) {
        p.hidden = (p.dataset.quotePanel !== name);
      });
    }

    tabs.forEach(function (t) {
      t.addEventListener('click', function () { showPanel(t.dataset.quoteTab); });
    });

    // Each "Go" button routes the user to the appropriate deep-form anchor
    // with a query-string prefill the form pages can read in a future enhancement.
    var routes = {
      maintenance: function () {
        var issue   = (document.getElementById('heroMaintIssue') || {}).value || '';
        var emirate = (document.getElementById('heroMaintEmirate') || {}).value || '';
        var qs = [];
        if (issue)   qs.push('issue='   + encodeURIComponent(issue));
        if (emirate) qs.push('emirate=' + encodeURIComponent(emirate));
        return '/Maintenance' + (qs.length ? ('?' + qs.join('&')) : '') + '#emergency';
      },
      amc: function () {
        var bt   = (document.getElementById('heroAmcType') || {}).value || '';
        var size = (document.getElementById('heroAmcSize') || {}).value || '';
        var qs = [];
        if (bt)   qs.push('buildingType=' + encodeURIComponent(bt));
        if (size) qs.push('size='         + encodeURIComponent(size));
        return '/Maintenance' + (qs.length ? ('?' + qs.join('&')) : '') + '#amc-quote';
      },
      contracting: function () {
        var svc  = (document.getElementById('heroContrService') || {}).value || '';
        var area = (document.getElementById('heroContrArea') || {}).value || '';
        var qs = [];
        if (svc)  qs.push('serviceType=' + encodeURIComponent(svc));
        if (area) qs.push('areaSqm='     + encodeURIComponent(area));
        return '/Contracting' + (qs.length ? ('?' + qs.join('&')) : '') + '#get-quote';
      },
      facility: function () {
        var asset = (document.getElementById('heroFmAsset') || {}).value || '';
        var hours = (document.getElementById('heroFmHours') || {}).value || '';
        var qs = ['subject=facility-management'];
        if (asset) qs.push('assetType=' + encodeURIComponent(asset));
        if (hours) qs.push('serviceHours=' + encodeURIComponent(hours));
        // FacilityManagement page exposes an #enquire anchor that scrolls to
        // the CTA → Contact form. Pre-filled query params are kept for
        // future server-side consumption on the Contact page.
        return '/FacilityManagement?' + qs.join('&') + '#enquire';
      }
    };

    widget.querySelectorAll('[data-quote-go]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var name = btn.dataset.quoteGo;
        var url  = routes[name] && routes[name]();
        if (url) window.location.assign(url);
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
})();
