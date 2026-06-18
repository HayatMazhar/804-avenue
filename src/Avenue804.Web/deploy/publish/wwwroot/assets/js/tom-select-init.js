/**
 * Tom Select initialisation.
 *
 * Two groups:
 *  1. Hero search selects (.ph-hero-select) — minimal chrome, dropdownParent:'body'
 *     so the dropdown floats above all page content.
 *  2. General form selects inside .enhance-dd — full Tom Select, also dropdownParent:'body'.
 *
 * Skips: [multiple], .no-ts, already-initialised elements.
 */
(function () {
  if (typeof TomSelect === 'undefined') return;

  /* ── shared option renderer ── */
  var optRender = {
    option: function (data, escape) {
      return '<div class="py-1 px-2">' + escape(data.text) + '</div>';
    }
  };

  /* ── 1. Hero selects ── */
  function initHeroSelects() {
    document.querySelectorAll('select.ph-hero-select').forEach(function (sel) {
      if (sel.dataset.tsInit === '1') return;
      sel.dataset.tsInit = '1';
      try {
        new TomSelect(sel, {
          allowEmptyOption: true,
          create: false,
          hideSelected: false,
          dropdownParent: 'body',   /* float above page — no overlap with stats */
          render: optRender
        });
      } catch (e) {
        sel.dataset.tsInit = '';
      }
    });
  }

  /* ── 2. General form selects ── */
  function initFormSelects() {
    document.querySelectorAll('.enhance-dd select:not([multiple]):not(.no-ts):not(.ph-hero-select)').forEach(function (sel) {
      if (sel.dataset.tsInit === '1') return;
      if (sel.disabled) return;
      sel.dataset.tsInit = '1';
      try {
        new TomSelect(sel, {
          allowEmptyOption: true,
          create: false,
          hideSelected: false,
          dropdownParent: 'body',
          render: optRender
        });
      } catch (e) {
        sel.dataset.tsInit = '';
      }
    });
  }

  function boot() {
    initHeroSelects();
    initFormSelects();

    window.Avenue804 = window.Avenue804 || {};
    window.Avenue804.refreshTomSelect = function (sel) {
      if (!sel || !sel.tomselect) return;
      try { sel.tomselect.sync(); } catch (e) {}
    };
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();
