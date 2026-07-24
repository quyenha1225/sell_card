(function () {
    'use strict';
    document.addEventListener('click', function (event) {
        var button = event.target.closest('.js-quick-add');
        if (!button || button.disabled) return;
        var original = button.innerHTML;
        button.disabled = true;
        button.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i><span>Đang thêm</span>';
        fetch(button.getAttribute('data-url'), { method: 'POST', headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' })
            .then(function (response) {
                if (response.redirected) { window.location.href = response.url; return null; }
                if (!response.ok) throw new Error('Không thể thêm sản phẩm.');
                return response.json();
            }).then(function (result) {
                if (!result) return;
                document.querySelectorAll('.cart-count').forEach(function (badge) { badge.textContent = result.cartCount; badge.classList.add('has-items', 'cart-pop'); });
                button.innerHTML = '<i class="bx bx-check"></i><span>Đã thêm</span>';
                if (window.Toastify) Toastify({ text: 'Đã thêm vào giỏ. Bạn có thể tiếp tục mua sắm.', duration: 2800, gravity: 'top', position: 'right', style: { background: 'linear-gradient(135deg,#259cd8,#59c9ed)' } }).showToast();
                setTimeout(function () { button.innerHTML = original; }, 1500);
            }).catch(function (error) {
                if (window.Toastify) Toastify({ text: error.message, duration: 3000, gravity: 'top', position: 'right' }).showToast();
            }).finally(function () { button.disabled = false; });
    });

    if (!window.matchMedia('(prefers-reduced-motion: reduce)').matches && window.innerWidth >= 992) {
        document.querySelectorAll('.modern-product-card').forEach(function (card) {
            card.addEventListener('mousemove', function (event) {
                var rect = card.getBoundingClientRect();
                var rotateY = ((event.clientX - rect.left) / rect.width - .5) * 7;
                var rotateX = ((event.clientY - rect.top) / rect.height - .5) * -7;
                card.style.transform = 'perspective(900px) rotateX(' + rotateX + 'deg) rotateY(' + rotateY + 'deg) translateY(-5px)';
            });
            card.addEventListener('mouseleave', function () { card.style.transform = ''; });
        });
    }
}());
