(function () {
    'use strict';

    var bar = document.querySelector('.admin-loadbar');
    var safetyTimer;

    function finishLoading() {
        if (!bar) return;
        clearTimeout(safetyTimer);
        bar.classList.remove('is-loading');
        bar.classList.add('is-complete');
        setTimeout(function () { bar.classList.remove('is-complete'); }, 300);
    }

    function startLoading() {
        if (!bar) return;
        bar.classList.remove('is-complete');
        bar.classList.add('is-loading');
        clearTimeout(safetyTimer);
        safetyTimer = setTimeout(finishLoading, 4000);
    }

    document.querySelectorAll('.sidebar-link').forEach(function (link) {
        if (link.pathname === location.pathname || (link.pathname !== '/Admin' && location.pathname.indexOf(link.pathname) === 0)) {
            link.classList.add('active');
        }
    });

    document.addEventListener('click', function (event) {
        var link = event.target.closest('a');
        if (link && link.href && link.origin === location.origin && !link.href.includes('#') && !link.hasAttribute('data-bs-toggle')) {
            startLoading();
        }
    });
    document.addEventListener('submit', startLoading);
    document.addEventListener('DOMContentLoaded', finishLoading);
    window.addEventListener('load', finishLoading);
    window.addEventListener('pageshow', finishLoading);

    if (document.readyState === 'interactive' || document.readyState === 'complete') finishLoading();
}());
