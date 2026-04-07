/* ═══════════════════════════════════════════════════════
   804 AVENUE — Shared JavaScript
   ═══════════════════════════════════════════════════════ */

/* ── Theme Switcher (runs immediately before paint) ──
   Default: Amber Gold. Empty string in localStorage = user chose Electric Blue. */
(function() {
  var saved = localStorage.getItem('804-theme');
  if (saved === null) {
    document.documentElement.setAttribute('data-theme', 'gold');
    localStorage.setItem('804-theme', 'gold');
  } else if (saved === '') {
    document.documentElement.removeAttribute('data-theme');
  } else {
    document.documentElement.setAttribute('data-theme', saved);
  }
})();

(function() {
  'use strict';

  /* ── Theme Switcher UI ── */
  var themes = [
    { id: 'gold', label: 'Amber Gold', swatch: 'gold' },
    { id: '', label: 'Electric Blue', swatch: 'blue' },
    { id: 'green', label: 'Emerald', swatch: 'green' },
    { id: 'red', label: 'Ruby Red', swatch: 'red' },
    { id: 'violet', label: 'Royal Violet', swatch: 'violet' },
    { id: 'orange', label: 'Sunset Orange', swatch: 'orange' }
  ];

  var rawTheme = localStorage.getItem('804-theme');
  var currentTheme = rawTheme === null ? 'gold' : rawTheme;

  /* ── Runtime stylesheet patcher ──
     Replaces hardcoded blue accent colors in embedded <style> blocks
     so theme changes propagate to every page without editing HTML files */
  var themeColors = {
    '':      { hex: '#3B82F6', hover: '#2563EB', light: '#60A5FA', rgb: '59,130,246', cHex: '#06B6D4', cRgb: '6,182,212' },
    gold:    { hex: '#F59E0B', hover: '#D97706', light: '#FBBF24', rgb: '245,158,11', cHex: '#EAB308', cRgb: '234,179,8' },
    green:   { hex: '#10B981', hover: '#059669', light: '#34D399', rgb: '16,185,129', cHex: '#06D6A0', cRgb: '6,214,160' },
    red:     { hex: '#EF4444', hover: '#DC2626', light: '#F87171', rgb: '239,68,68',  cHex: '#F97316', cRgb: '249,115,22' },
    violet:  { hex: '#8B5CF6', hover: '#7C3AED', light: '#A78BFA', rgb: '139,92,246', cHex: '#EC4899', cRgb: '236,72,153' },
    orange:  { hex: '#F97316', hover: '#EA580C', light: '#FB923C', rgb: '249,115,22', cHex: '#FBBF24', cRgb: '251,191,36' }
  };
  var origStyles = [];

  function captureOriginalStyles() {
    document.querySelectorAll('style').forEach(function(el) {
      origStyles.push({ el: el, text: el.textContent });
    });
  }

  function patchStyles(themeId) {
    var target = themeColors[themeId] || themeColors[''];
    var base = themeColors[''];
    origStyles.forEach(function(entry) {
      var t = entry.text;
      if (themeId && themeId !== '') {
        t = t.split(base.hex).join(target.hex);
        t = t.split(base.hover).join(target.hover);
        t = t.split(base.light).join(target.light);
        t = t.split(base.rgb).join(target.rgb);
        t = t.split(base.cHex).join(target.cHex);
        t = t.split(base.cRgb).join(target.cRgb);
      }
      entry.el.textContent = t;
    });
  }

  function buildSwitcher() {
    captureOriginalStyles();
    if (currentTheme) patchStyles(currentTheme);
    var btn = document.createElement('button');
    btn.className = 'theme-switcher-btn';
    btn.setAttribute('aria-label', 'Change theme');
    btn.innerHTML = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="5"/><path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/></svg>';

    var panel = document.createElement('div');
    panel.className = 'theme-panel';
    var html = '<div class="theme-panel-title">Choose Theme</div><div class="theme-options">';
    themes.forEach(function(t) {
      var isActive = (t.id === currentTheme) ? ' active' : '';
      html += '<button class="theme-option' + isActive + '" data-theme-id="' + t.id + '">'
        + '<span class="theme-swatch theme-swatch-' + t.swatch + '"></span>'
        + '<span class="theme-label">' + t.label + '</span>'
        + '</button>';
    });
    html += '</div>';
    panel.innerHTML = html;

    document.body.appendChild(btn);
    document.body.appendChild(panel);

    btn.addEventListener('click', function(e) {
      e.stopPropagation();
      panel.classList.toggle('open');
    });

    document.addEventListener('click', function(e) {
      if (!panel.contains(e.target) && e.target !== btn) {
        panel.classList.remove('open');
      }
    });

    panel.querySelectorAll('.theme-option').forEach(function(opt) {
      opt.addEventListener('click', function() {
        var id = this.getAttribute('data-theme-id');
        currentTheme = id;
        if (id) {
          document.documentElement.setAttribute('data-theme', id);
          localStorage.setItem('804-theme', id);
        } else {
          document.documentElement.removeAttribute('data-theme');
          localStorage.setItem('804-theme', '');
        }
        panel.querySelectorAll('.theme-option').forEach(function(o) { o.classList.remove('active'); });
        this.classList.add('active');
        patchStyles(id);
      });
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', buildSwitcher);
  } else {
    buildSwitcher();
  }

  var navbar = document.getElementById('navbar');
  if (navbar) {
    window.addEventListener('scroll', function() {
      navbar.classList.toggle('scrolled', window.scrollY > 60);
    }, { passive: true });
  }

  var hamburger = document.getElementById('hamburger');
  var mobileNav = document.getElementById('mobileNav');
  if (hamburger && mobileNav) {
    hamburger.addEventListener('click', function() {
      hamburger.classList.toggle('active');
      mobileNav.classList.toggle('open');
      document.body.style.overflow = mobileNav.classList.contains('open') ? 'hidden' : '';
    });
  }
  window.closeMobile = function() {
    if (hamburger) hamburger.classList.remove('active');
    if (mobileNav) mobileNav.classList.remove('open');
    document.body.style.overflow = '';
  };

  var fadeEls = document.querySelectorAll('.fade-up');
  if (fadeEls.length) {
    var obs = new IntersectionObserver(function(entries) {
      entries.forEach(function(e, i) {
        if (e.isIntersecting) {
          setTimeout(function() { e.target.classList.add('visible'); }, i * 80);
          obs.unobserve(e.target);
        }
      });
    }, { threshold: 0.1, rootMargin: '0px 0px -30px 0px' });
    fadeEls.forEach(function(el) { obs.observe(el); });
  }

  var statNums = document.querySelectorAll('[data-count]');
  if (statNums.length) {
    var counted = false;
    var sObs = new IntersectionObserver(function(entries) {
      entries.forEach(function(entry) {
        if (entry.isIntersecting && !counted) {
          counted = true;
          statNums.forEach(function(num) {
            var target = parseInt(num.getAttribute('data-count'), 10);
            var suffix = num.getAttribute('data-suffix') || '+';
            var cur = 0, inc = Math.ceil(target / 50);
            var t = setInterval(function() {
              cur += inc;
              if (cur >= target) { cur = target; clearInterval(t); }
              num.textContent = cur + suffix;
            }, 28);
          });
        }
      });
    }, { threshold: 0.3 });
    var sw = statNums[0] ? statNums[0].closest('section, .stats-bar') : null;
    if (sw) sObs.observe(sw);
  }

  var btt = document.getElementById('backToTop');
  if (btt) {
    window.addEventListener('scroll', function() {
      btt.classList.toggle('show', window.scrollY > 300);
    }, { passive: true });
    btt.addEventListener('click', function() {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    });
  }

  document.querySelectorAll('a[href^="#"]').forEach(function(a) {
    a.addEventListener('click', function(e) {
      var id = this.getAttribute('href');
      if (id === '#' || id.length < 2) return;
      var el = document.querySelector(id);
      if (el) {
        e.preventDefault();
        window.scrollTo({ top: el.getBoundingClientRect().top + window.scrollY - 80, behavior: 'smooth' });
      }
    });
  });

  function closeModal(root) {
    if (!root) return;
    root.classList.remove('open');
    root.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    if (typeof history !== 'undefined' && history.replaceState) {
      var h = window.location.hash;
      if (h === '#modal-list-property' || h === '#modal-inquiry') {
        history.replaceState(null, '', window.location.pathname + window.location.search);
      }
    }
  }

  function openModal(id) {
    var root = document.getElementById(id);
    if (!root) return;
    document.querySelectorAll('.modal-root.open').forEach(function(m) {
      if (m !== root) closeModal(m);
    });
    root.classList.add('open');
    root.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
    var closeBtn = root.querySelector('.modal-close');
    if (closeBtn) closeBtn.focus();
  }

  document.querySelectorAll('[data-modal-open]').forEach(function(btn) {
    btn.addEventListener('click', function(e) {
      e.preventDefault();
      var id = btn.getAttribute('data-modal-open');
      if (!id) return;
      openModal(id);
    });
  });

  document.querySelectorAll('.modal-root').forEach(function(root) {
    root.setAttribute('aria-hidden', 'true');
    root.querySelectorAll('[data-modal-close]').forEach(function(el) {
      el.addEventListener('click', function() {
        closeModal(root);
      });
    });
    var bd = root.querySelector('.modal-backdrop');
    if (bd) {
      bd.addEventListener('click', function() {
        closeModal(root);
      });
    }
  });

  document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
      document.querySelectorAll('.modal-root.open').forEach(closeModal);
    }
  });

  (function openModalFromHash() {
    var id = (window.location.hash || '').replace(/^#/, '');
    if (id === 'modal-list-property' || id === 'modal-inquiry') {
      openModal(id);
    }
  })();
  window.addEventListener('hashchange', function() {
    var id = (window.location.hash || '').replace(/^#/, '');
    if (id === 'modal-list-property' || id === 'modal-inquiry') {
      openModal(id);
    }
  });

  (function propertyInquiryDetailFilter() {
    var form = document.getElementById('propertyListingInquiryForm');
    if (!form) return;
    var typeSel = document.getElementById('pliPropType');
    var detailSel = document.getElementById('pliDetail');
    if (!typeSel || !detailSel) return;

    var detailOptions = [];
    detailSel.querySelectorAll('option[data-for-types]').forEach(function(opt) {
      var raw = opt.getAttribute('data-for-types') || '[]';
      var types = [];
      try { types = JSON.parse(raw); } catch (e) { types = []; }
      if (!Array.isArray(types)) types = [];
      detailOptions.push({ el: opt, types: types });
    });

    function selectedTypeCode() {
      var o = typeSel.options[typeSel.selectedIndex];
      return (o && o.getAttribute('data-type-code')) ? String(o.getAttribute('data-type-code')).toLowerCase() : '';
    }

    function refreshDetails() {
      var code = selectedTypeCode();
      var cur = detailSel.value;
      detailOptions.forEach(function(x) {
        var show = !x.types.length || x.types.indexOf(code) >= 0;
        x.el.hidden = !show;
        x.el.disabled = !show;
      });
      var keep = cur ? detailSel.querySelector('option[value="' + cur + '"]') : null;
      if (!keep || keep.hidden || keep.disabled) detailSel.value = '';
    }

    typeSel.addEventListener('change', refreshDetails);
    refreshDetails();
  })();
})();
