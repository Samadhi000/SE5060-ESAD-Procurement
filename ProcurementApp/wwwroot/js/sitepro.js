// SitePro Procurement — small bits of interaction the wireframes imply.
// No framework: the screens are server-rendered Razor, this only wires up the
// quantity steppers and the Good/Damaged/Short segmented control.

(function () {
    'use strict';

    // ---- status-bar clock: real Sri Lankan time, whatever the device's own timezone ----
    // The server renders the first value; this keeps it current while the page is open.
    var clock = document.querySelector('[data-clock]');
    if (clock && window.Intl && Intl.DateTimeFormat) {
        var colombo = new Intl.DateTimeFormat('en-GB', {
            timeZone: 'Asia/Colombo',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        });

        var tick = function () {
            try {
                // formatToParts avoids locale quirks around the AM/PM suffix, which the
                // phone status bar in the design's mockups does not show.
                var parts = colombo.formatToParts(new Date());
                var hour = '', minute = '';
                parts.forEach(function (p) {
                    if (p.type === 'hour') { hour = p.value; }
                    if (p.type === 'minute') { minute = p.value; }
                });
                if (hour && minute) { clock.textContent = hour + ':' + minute; }
            } catch (e) {
                /* leave the server-rendered time in place */
            }
        };

        tick();
        setInterval(tick, 15000);
    }

    // ---- quantity steppers: the "− 10 +" control on the catalogue and delivery screens ----
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-step]');
        if (!btn) return;
        e.preventDefault();

        var box = btn.closest('.stepper');
        var input = box && box.querySelector('input');
        if (!input) return;

        var step = parseInt(btn.getAttribute('data-step'), 10) || 0;
        var min = input.hasAttribute('min') ? parseInt(input.min, 10) : 0;
        var max = input.hasAttribute('max') ? parseInt(input.max, 10) : Number.MAX_SAFE_INTEGER;
        var next = (parseInt(input.value, 10) || 0) + step;

        input.value = Math.min(max, Math.max(min, next));
        input.dispatchEvent(new Event('change', { bubbles: true }));
    });

    // ---- condition segmented control ----
    // Selecting "Damaged" or "Short" reveals the note box and tints the card amber,
    // exactly as record-delivery.png shows.
    document.addEventListener('change', function (e) {
        var radio = e.target;
        if (!radio.matches || !radio.matches('.segmented input')) return;

        var card = radio.closest('.icard');
        if (!card) return;

        // Each line has its own radio group, so mirror the choice into the hidden
        // "condition" field that keeps the posted condition[] array in line order.
        var hidden = card.querySelector('[data-condition]');
        if (hidden) hidden.value = radio.value;

        var note = card.querySelector('[data-note]');
        var needsNote = radio.value === 'Damaged' || radio.value === 'Short';

        card.classList.toggle('icard--amber', radio.value === 'Damaged');
        if (note) note.style.display = needsNote ? 'block' : 'none';
    });
})();
