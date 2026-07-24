(function () {
    'use strict';

    var quantity = document.getElementById('productQuantity');
    var form = document.getElementById('addToCartForm');

    function normaliseQuantity(value) {
        var min = parseInt(quantity.min, 10) || 1;
        var max = parseInt(quantity.max, 10) || 999;
        var parsed = parseInt(value, 10);
        if (isNaN(parsed)) parsed = min;
        return Math.min(max, Math.max(min, parsed));
    }

    if (quantity) {
        document.querySelectorAll('.quantity-control button').forEach(function (button) {
            button.addEventListener('click', function () {
                var step = button.getAttribute('data-action') === 'plus' ? 1 : -1;
                quantity.value = normaliseQuantity(parseInt(quantity.value, 10) + step);
            });
        });
        quantity.addEventListener('change', function () { quantity.value = normaliseQuantity(quantity.value); });
    }

    if (form) {
        form.addEventListener('submit', function (event) {
            event.preventDefault();
            quantity.value = normaliseQuantity(quantity.value);
            var button = form.querySelector('.add-cart-btn');
            if (!button || button.disabled) return;
            var originalHtml = button.innerHTML;
            button.disabled = true;
            button.classList.add('is-loading');
            button.innerHTML = '<i class="bx bx-loader-alt"></i><span>Đang thêm...</span>';

            fetch(form.action, {
                method: 'POST', headers: { 'X-Requested-With': 'XMLHttpRequest' },
                body: new FormData(form), credentials: 'same-origin'
            }).then(function (response) {
                if (response.redirected) { window.location.href = response.url; return null; }
                if (!response.ok) throw new Error('Không thể thêm sản phẩm.');
                return response.json();
            }).then(function (result) {
                if (!result) return;
                if (!result.status) throw new Error(result.message || 'Không thể thêm sản phẩm.');
                document.querySelectorAll('.cart-count').forEach(function (badge) {
                    badge.textContent = result.cartCount;
                    badge.classList.toggle('has-items', result.cartCount > 0);
                    badge.classList.remove('cart-pop'); void badge.offsetWidth; badge.classList.add('cart-pop');
                });
                if (window.Toastify) Toastify({ text: result.message + ' Bạn có thể tiếp tục mua sắm.', duration: 3200, gravity: 'top', position: 'right', style: { background: '#17202f' } }).showToast();
                button.innerHTML = '<i class="bx bx-check"></i><span>Đã thêm vào giỏ</span>';
                setTimeout(function () { button.innerHTML = originalHtml; }, 1600);
            }).catch(function (error) {
                if (window.Toastify) Toastify({ text: error.message, duration: 3500, gravity: 'top', position: 'right', style: { background: '#c0392b' } }).showToast();
            }).finally(function () { button.disabled = false; button.classList.remove('is-loading'); });
        });
    }

    var revealItems = document.querySelectorAll('.reveal-item');
    if ('IntersectionObserver' in window) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });
        revealItems.forEach(function (item, index) {
            item.style.transitionDelay = Math.min(index * 80, 240) + 'ms';
            observer.observe(item);
        });
    } else {
        revealItems.forEach(function (item) { item.classList.add('is-visible'); });
    }
}());
