(function () {
  'use strict';
  function init() {
    if (typeof window.flatpickr === 'undefined') return;
    var dateOpts = {
      dateFormat: 'Y-m-d',
      disableMobile: false,
      allowInput: true,
      static: false,
      animate: true
    };
    document.querySelectorAll('input[type="date"]:not(.flatpickr-input)').forEach(function (el) {
      window.flatpickr(el, Object.assign({}, dateOpts));
    });
    document.querySelectorAll('input[type="range"]').forEach(function (r) {
      function syncRange() {
        var min = parseFloat(r.min) || 0;
        var max = parseFloat(r.max) || 100;
        var val = parseFloat(r.value) || 0;
        var pct = ((val - min) / (max - min) * 100).toFixed(1) + '%';
        r.style.setProperty('--range-pct', pct);
      }
      syncRange();
      r.addEventListener('input', syncRange);
    });
  }
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
