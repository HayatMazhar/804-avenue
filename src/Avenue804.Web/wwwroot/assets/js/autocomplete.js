/* ═══════════════════════════════════════════════════════
   804 AVENUE — UAE Area Autocomplete
   Usage: initAutocomplete(inputEl, onSelect, options)
   ═══════════════════════════════════════════════════════ */

(function () {
  'use strict';

  var STYLE_INJECTED = false;
  function injectStyles() {
    if (STYLE_INJECTED) return;
    STYLE_INJECTED = true;
    var s = document.createElement('style');
    s.textContent = [
      '.ac-wrap{position:relative;width:100%}',
      '.ac-dropdown{position:absolute;top:100%;left:0;right:0;z-index:9999;background:var(--bg-card,#fff);border:1px solid var(--border-accent,#ccc);border-radius:var(--radius-sm,8px);box-shadow:0 16px 48px rgba(15,23,42,.14);max-height:280px;overflow-y:auto;display:none;margin-top:2px;scrollbar-width:thin}',
      '.ac-item{display:flex;align-items:center;gap:10px;padding:10px 14px;cursor:pointer;border-bottom:1px solid var(--border-subtle,#f0f0f0);font-size:.85rem;color:var(--text-primary,#0f172a);transition:background .15s ease}',
      '.ac-item:last-child{border-bottom:none}',
      '.ac-item:hover,.ac-item.ac-active{background:var(--accent-10,rgba(245,158,11,.1))}',
      '.ac-name{font-weight:600;flex:1}',
      '.ac-meta{font-size:.7rem;color:var(--text-tertiary,#94a3b8);text-align:right}',
      '.ac-type{font-size:.58rem;font-family:var(--font-nav,sans-serif);font-weight:700;letter-spacing:.08em;text-transform:uppercase;padding:2px 8px;border-radius:999px;background:var(--accent-10,rgba(245,158,11,.1));color:var(--accent,#f59e0b);white-space:nowrap}',
      '.ac-no-results{padding:12px 14px;font-size:.82rem;color:var(--text-tertiary,#94a3b8);text-align:center}',
      '.ac-loading{padding:12px 14px;font-size:.82rem;color:var(--text-tertiary,#94a3b8);text-align:center}'
    ].join('');
    document.head.appendChild(s);
  }

  function debounce(fn, delay) {
    var t;
    return function () {
      var args = arguments;
      clearTimeout(t);
      t = setTimeout(function () { fn.apply(this, args); }, delay);
    };
  }

  window.initAutocomplete = function (input, onSelect, opts) {
    if (!input) return;
    if (input.getAttribute('data-ac-initialized') === '1') return;
    if (input.closest('.ac-wrap')) return;
    input.setAttribute('data-ac-initialized', '1');
    opts = opts || {};
    var apiUrl = opts.apiUrl || '/Api/UaeAreas?handler=Search';
    var minChars = opts.minChars || 2;
    var placeholder = opts.placeholder || input.getAttribute('placeholder') || 'Search UAE areas…';
    var maxResults = opts.maxResults || 10;

    injectStyles();

    // Wrap input
    var parent = input.parentNode;
    var wrap = document.createElement('div');
    wrap.className = 'ac-wrap';
    parent.insertBefore(wrap, input);
    wrap.appendChild(input);

    // Create dropdown
    var dropdown = document.createElement('div');
    dropdown.className = 'ac-dropdown';
    wrap.appendChild(dropdown);

    var activeIdx = -1;
    var currentResults = [];
    var currentXhr = null;

    function show() { dropdown.style.display = 'block'; }
    function hide() { dropdown.style.display = 'none'; activeIdx = -1; }

    function setLoading() {
      dropdown.innerHTML = '<div class="ac-loading">Searching…</div>';
      show();
    }

    function renderResults(results) {
      currentResults = results;
      if (!results.length) {
        dropdown.innerHTML = '<div class="ac-no-results">No matching areas found</div>';
        show();
        return;
      }

      var typeLabels = {
        0: 'City', 1: 'Community', 2: 'District', 3: 'Island',
        4: 'Development', 5: 'Building', 6: 'Industrial', 7: 'Free Zone'
      };

      dropdown.innerHTML = '';
      results.forEach(function (r, i) {
        var item = document.createElement('div');
        item.className = 'ac-item';
        item.setAttribute('data-idx', i);
        var typeLabel = typeLabels[r.type] || '';
        var meta = r.city && r.city !== r.emirate ? r.city + ', ' + r.emirate : r.emirate;
        item.innerHTML =
          '<svg class="ac-icon" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/></svg>' +
          '<span class="ac-name">' + escHtml(r.name) + '</span>' +
          '<span class="ac-meta">' + escHtml(meta) + '</span>' +
          (typeLabel ? '<span class="ac-type">' + typeLabel + '</span>' : '');
        item.addEventListener('mousedown', function (e) {
          e.preventDefault();
          selectResult(r);
        });
        dropdown.appendChild(item);
      });
      show();
    }

    function escHtml(s) {
      return String(s || '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    function selectResult(r) {
      input.value = r.name + (r.city && r.city !== r.emirate ? ', ' + r.city : '') + ', ' + r.emirate;
      hide();

      // Auto-fill a linked emirate dropdown if configured
      var emirateTargetId = input.getAttribute('data-ac-emirate-target');
      if (emirateTargetId && r.emirate) {
        var sel = document.getElementById(emirateTargetId);
        if (sel) {
          // Try exact match first, then partial
          var matched = false;
          for (var i = 0; i < sel.options.length; i++) {
            if (sel.options[i].value === r.emirate) {
              sel.value = r.emirate;
              matched = true;
              break;
            }
          }
          if (!matched) {
            for (var j = 0; j < sel.options.length; j++) {
              if (r.emirate.indexOf(sel.options[j].value) !== -1 || sel.options[j].value.indexOf(r.emirate) !== -1) {
                sel.value = sel.options[j].value;
                break;
              }
            }
          }
          sel.dispatchEvent(new Event('change'));
          // Update Tom Select if present
          if (sel.tomselect) { try { sel.tomselect.sync(); } catch(e) {} }
        }
      }

      if (onSelect) onSelect(r);
      input.dispatchEvent(new Event('change'));
    }

    function highlightItem(idx) {
      var items = dropdown.querySelectorAll('.ac-item');
      items.forEach(function (el) { el.classList.remove('ac-active'); });
      if (idx >= 0 && idx < items.length) {
        items[idx].classList.add('ac-active');
        items[idx].scrollIntoView({ block: 'nearest' });
      }
      activeIdx = idx;
    }

    var fetchResults = debounce(function (q) {
      if (currentXhr) currentXhr.abort && currentXhr.abort();
      setLoading();
      var url = apiUrl + '&q=' + encodeURIComponent(q) + '&limit=' + maxResults;
      if (opts.emirate) url += '&emirate=' + encodeURIComponent(opts.emirate);
      var controller = new AbortController();
      currentXhr = controller;
      fetch(url, { signal: controller.signal, credentials: 'same-origin', headers: { Accept: 'application/json' } })
        .then(function (r) {
          if (!r.ok) throw new Error('HTTP ' + r.status);
          return r.json();
        })
        .then(function (data) {
          renderResults(Array.isArray(data) ? data : []);
        })
        .catch(function () { hide(); });
    }, 200);

    input.addEventListener('input', function () {
      var q = this.value.trim();
      if (q.length < minChars) { hide(); return; }
      fetchResults(q);
    });

    input.addEventListener('keydown', function (e) {
      var items = dropdown.querySelectorAll('.ac-item');
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        highlightItem(Math.min(activeIdx + 1, items.length - 1));
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        highlightItem(Math.max(activeIdx - 1, 0));
      } else if (e.key === 'Enter') {
        if (activeIdx >= 0 && currentResults[activeIdx]) {
          e.preventDefault();
          selectResult(currentResults[activeIdx]);
        } else {
          hide();
        }
      } else if (e.key === 'Escape') {
        hide();
      }
    });

    input.addEventListener('blur', function () {
      setTimeout(hide, 150);
    });

    input.addEventListener('focus', function () {
      var q = this.value.trim();
      if (q.length >= minChars && !currentResults.length) fetchResults(q);
      else if (currentResults.length) show();
    });
  };

  function autoInitUaeAutocomplete() {
    document.querySelectorAll('[data-autocomplete="uae"]').forEach(function (el) {
      var opts = {};
      if (el.dataset.acEmirate) opts.emirate = el.dataset.acEmirate;
      initAutocomplete(el, null, opts);
    });
  }

  // Run after DOM is ready (listener alone misses if script loads after DOMContentLoaded)
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', autoInitUaeAutocomplete);
  } else {
    autoInitUaeAutocomplete();
  }
})();
