/**
 * Progressive enhancement: replaces native <select> open UI with themed custom panels.
 * Requires: .enhance-dd ancestor (or init root), select:not([multiple]):not(.ui-dd-skip)
 */
(function (global) {
  'use strict';

  var DD_OPEN = 'ui-dd--open';
  var docListeners = false;

  function closeAll(except) {
    document.querySelectorAll('.ui-dd.' + DD_OPEN + ', .hero-dd.hero-dd--open').forEach(function (w) {
      if (except && w === except) return;
      w.classList.remove(DD_OPEN);
      if (w.classList.contains('hero-dd')) w.classList.remove('hero-dd--open');
      var b = w.querySelector('.ui-dd__btn, .hero-dd__btn');
      var m = w.querySelector('.ui-dd__menu, .hero-dd__menu');
      if (b) b.setAttribute('aria-expanded', 'false');
      if (m) m.setAttribute('hidden', '');
    });
  }

  function bindDocOnce() {
    if (docListeners) return;
    docListeners = true;
    document.addEventListener('click', function () {
      closeAll(null);
    });
    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') closeAll(null);
    });
  }

  function buildMenuItems(sel, menu) {
    menu.innerHTML = '';
    var lastGroup = null;
    for (var i = 0; i < sel.options.length; i++) {
      var opt = sel.options[i];
      if (opt.hidden) continue;

      var parent = opt.parentElement;
      if (parent && parent.tagName === 'OPTGROUP') {
        var glabel = parent.label || '';
        if (glabel && glabel !== lastGroup) {
          lastGroup = glabel;
          var hdr = document.createElement('li');
          hdr.className = 'ui-dd__opt is-muted hero-dd__opt';
          hdr.setAttribute('role', 'presentation');
          hdr.textContent = glabel;
          menu.appendChild(hdr);
        }
      } else {
        lastGroup = null;
      }

      var li = document.createElement('li');
      li.setAttribute('role', 'option');
      li.className = 'ui-dd__opt hero-dd__opt';
      li.setAttribute('data-index', String(i));
      li.setAttribute('data-value', opt.value);
      if (opt.disabled) li.setAttribute('aria-disabled', 'true');
      li.textContent = opt.text || opt.value;
      menu.appendChild(li);
    }
  }

  function syncFromSelect(sel, state) {
    var selected = sel.options[sel.selectedIndex];
    var label = selected && !selected.hidden ? (selected.text || selected.value) : '';
    state.valEl.textContent = (label || '').trim();
    if (state.hid) state.hid.value = sel.value;
    state.menu.querySelectorAll('li[role="option"]').forEach(function (li) {
      var idx = parseInt(li.getAttribute('data-index'), 10);
      if (isNaN(idx)) return;
      li.classList.toggle('is-selected', idx === sel.selectedIndex);
    });
  }

  function enhanceSelect(sel) {
    if (!sel || sel.multiple || sel.disabled || sel.classList.contains('ui-dd-skip')) return;
    if (sel.dataset.uiDdEnhanced === '1') return;
    if (sel.closest('.hero-dd')) return;

    sel.dataset.uiDdEnhanced = '1';
    bindDocOnce();

    var parent = sel.parentNode;
    var wrap = document.createElement('div');
    wrap.className = 'ui-dd';

    var origName = sel.getAttribute('name');
    if (origName) sel.removeAttribute('name');

    var hid = document.createElement('input');
    hid.type = 'hidden';
    if (origName) hid.setAttribute('name', origName);
    var btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'ui-dd__btn hero-dd__btn';
    btn.setAttribute('aria-haspopup', 'listbox');
    btn.setAttribute('aria-expanded', 'false');

    var valSpan = document.createElement('span');
    valSpan.className = 'ui-dd__value hero-dd__value';
    btn.appendChild(valSpan);
    var chev = document.createElement('span');
    chev.className = 'ui-dd__chev hero-dd__chev';
    chev.innerHTML =
      '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M6 9l6 6 6-6"/></svg>';
    btn.appendChild(chev);

    var menu = document.createElement('ul');
    menu.className = 'ui-dd__menu hero-dd__menu';
    menu.setAttribute('role', 'listbox');
    menu.setAttribute('hidden', '');

    buildMenuItems(sel, menu);

    sel.classList.add('ui-dd-native');
    sel.setAttribute('tabindex', '-1');

    wrap.appendChild(hid);
    wrap.appendChild(btn);
    wrap.appendChild(menu);
    wrap.appendChild(sel);

    parent.insertBefore(wrap, sel);

    if (sel.required) hid.required = true;

    var state = { wrap: wrap, btn: btn, menu: menu, valEl: valSpan, hid: hid, sel: sel };
    sel._uiDdState = state;

    syncFromSelect(sel, state);

    wrap.addEventListener('click', function (e) {
      e.stopPropagation();
    });

    btn.addEventListener('click', function (e) {
      e.stopPropagation();
      buildMenuItems(sel, menu);
      syncFromSelect(sel, state);
      var open = !wrap.classList.contains(DD_OPEN);
      closeAll(open ? wrap : null);
      wrap.classList.toggle(DD_OPEN, open);
      btn.setAttribute('aria-expanded', open ? 'true' : 'false');
      if (open) menu.removeAttribute('hidden');
      else menu.setAttribute('hidden', '');
    });

    menu.addEventListener('click', function (e) {
      var li = e.target.closest('[role="option"]');
      if (!li || li.getAttribute('aria-disabled') === 'true') return;
      e.stopPropagation();
      var idx = parseInt(li.getAttribute('data-index'), 10);
      if (isNaN(idx) || !sel.options[idx]) return;
      sel.selectedIndex = idx;
      if (hid) hid.value = sel.value;
      syncFromSelect(sel, state);
      wrap.classList.remove(DD_OPEN);
      btn.setAttribute('aria-expanded', 'false');
      menu.setAttribute('hidden', '');
      sel.dispatchEvent(new Event('input', { bubbles: true }));
      sel.dispatchEvent(new Event('change', { bubbles: true }));
    });
  }

  function refreshUiDropdown(sel) {
    if (!sel || !sel._uiDdState) return;
    var state = sel._uiDdState;
    buildMenuItems(sel, state.menu);
    syncFromSelect(sel, state);
  }

  function initUiDropdowns(root) {
    root = root || document;
    root.querySelectorAll('.enhance-dd select:not([multiple]):not(.ui-dd-skip)').forEach(function (sel) {
      enhanceSelect(sel);
    });
  }

  global.Avenue804 = global.Avenue804 || {};
  global.Avenue804.initUiDropdowns = initUiDropdowns;
  global.Avenue804.refreshUiDropdown = refreshUiDropdown;

  function boot() {
    initUiDropdowns(document);
  }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})(typeof window !== 'undefined' ? window : this);
