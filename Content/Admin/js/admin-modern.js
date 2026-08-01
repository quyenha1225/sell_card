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

(function () {
    'use strict';

    var storageKey = 'gifttoi.admin.readNotifications';

    function getReadIds() {
        try {
            return JSON.parse(localStorage.getItem(storageKey) || '[]');
        } catch (error) {
            return [];
        }
    }

    function saveReadIds(ids) {
        try {
            localStorage.setItem(storageKey, JSON.stringify(ids));
        } catch (error) {
            // Trình duyệt chặn localStorage thì chuông vẫn hoạt động bình thường.
        }
    }

    function refreshNotificationState() {
        var readIds = getReadIds();
        var unread = 0;

        document.querySelectorAll('[data-notification-id]')
            .forEach(function (item) {
                var id = item.getAttribute('data-notification-id');
                var isRead = readIds.indexOf(id) >= 0;

                item.classList.toggle('is-read', isRead);

                if (!isRead) {
                    unread += 1;
                }
            });

        var badge = document.querySelector('[data-notification-count]');

        if (badge) {
            badge.textContent = unread;
            badge.style.display = unread > 0 ? 'grid' : 'none';
        }
    }

    document.addEventListener('click', function (event) {
        var notification =
            event.target.closest('[data-notification-id]');

        if (notification) {
            var ids = getReadIds();
            var id = notification.getAttribute('data-notification-id');

            if (ids.indexOf(id) < 0) {
                ids.push(id);
                saveReadIds(ids.slice(-100));
                refreshNotificationState();
            }
        }

        var markAll =
            event.target.closest('[data-mark-all-notifications]');

        if (markAll) {
            event.preventDefault();

            var allIds = Array.prototype.map.call(
                document.querySelectorAll('[data-notification-id]'),
                function (item) {
                    return item.getAttribute('data-notification-id');
                }
            );

            saveReadIds(allIds.slice(-100));
            refreshNotificationState();
        }
    });

    document.addEventListener(
        'DOMContentLoaded',
        refreshNotificationState
    );

    if (document.readyState === 'interactive' ||
        document.readyState === 'complete') {
        refreshNotificationState();
    }
}());
