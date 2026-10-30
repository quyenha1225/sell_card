(function () {
    'use strict';

    function getAntiForgeryToken() {
        var token = document.querySelector(
            '#storefrontAntiForgery input[name="__RequestVerificationToken"], ' +
            'input[name="__RequestVerificationToken"]'
        );

        return token ? token.value : '';
    }

    function showMessage(message, isError) {
        if (window.Toastify) {
            window.Toastify({
                text: message,
                duration: 3000,
                gravity: 'top',
                position: 'right',
                style: {
                    background: isError
                        ? 'linear-gradient(135deg,#d94841,#f16b5d)'
                        : 'linear-gradient(135deg,#259cd8,#59c9ed)'
                }
            }).showToast();
            return;
        }

        window.alert(message);
    }

    function updateCartCount(count) {
        document.querySelectorAll('.cart-count').forEach(function (badge) {
            badge.textContent = count || 0;
            badge.classList.toggle('has-items', (count || 0) > 0);
            badge.classList.add('cart-pop');

            window.setTimeout(function () {
                badge.classList.remove('cart-pop');
            }, 450);
        });
    }

    document.addEventListener('click', function (event) {
        var button = event.target.closest('.js-quick-add');

        if (!button || button.disabled) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        var productId = button.getAttribute('data-product-id');
        var verificationToken = getAntiForgeryToken();

        if (!productId) {
            showMessage('Không xác định được sản phẩm.', true);
            return;
        }

        if (!verificationToken) {
            showMessage(
                'Phiên bảo mật của trang đã hết hạn. Hãy tải lại trang.',
                true
            );
            return;
        }

        var original = button.innerHTML;
        var body = new URLSearchParams();

        body.append('__RequestVerificationToken', verificationToken);
        body.append('productID', productId);
        body.append('quantity', '1');

        button.disabled = true;
        button.innerHTML =
            '<i class="bx bx-loader-alt bx-spin"></i>' +
            '<span>Đang thêm</span>';

        fetch(window.appUrl('/Cart/AddItem'), {
            method: 'POST',
            headers: {
                'Content-Type':
                    'application/x-www-form-urlencoded; charset=UTF-8',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin',
            body: body.toString()
        })
        .then(function (response) {
            var contentType =
                response.headers.get('content-type') || '';

            if (response.redirected) {
                window.location.href = response.url;
                return null;
            }

            if (response.status === 401) {
                return response.text().then(function (raw) {
                    var result = null;

                    try {
                        result = raw ? JSON.parse(raw) : null;
                    } catch (error) {
                        result = null;
                    }

                    var returnUrl =
                        window.location.pathname +
                        window.location.search +
                        window.location.hash;

                    window.location.href =
                        result && result.loginUrl
                            ? window.appUrl(result.loginUrl)
                            : window.appUrl('/dang-nhap?returnUrl=') +
                              encodeURIComponent(returnUrl);

                    return null;
                });
            }

            if (!response.ok) {
                if (contentType.indexOf('application/json') >= 0) {
                    return response.json().then(function (result) {
                        throw new Error(
                            result.message ||
                            'Không thể thêm sản phẩm vào giỏ.'
                        );
                    });
                }

                throw new Error(
                    'Không thể thêm sản phẩm vào giỏ.'
                );
            }

            return response.json();
        })
        .then(function (result) {
            if (!result) {
                return;
            }

            if (!result.status) {
                throw new Error(
                    result.message ||
                    'Không thể thêm sản phẩm vào giỏ.'
                );
            }

            updateCartCount(result.cartCount);

            button.innerHTML =
                '<i class="bx bx-check"></i>' +
                '<span>Đã thêm</span>';

            showMessage(
                result.message || 'Đã thêm sản phẩm vào giỏ.',
                false
            );

            window.setTimeout(function () {
                button.innerHTML = original;
            }, 1400);
        })
        .catch(function (error) {
            button.innerHTML = original;
            showMessage(
                error.message || 'Không thể thêm sản phẩm vào giỏ.',
                true
            );
        })
        .finally(function () {
            button.disabled = false;
        });
    });

    if (!window.matchMedia('(prefers-reduced-motion: reduce)').matches &&
        window.innerWidth >= 992) {
        document.querySelectorAll('.modern-product-card')
            .forEach(function (card) {
                card.addEventListener('mousemove', function (event) {
                    var rect = card.getBoundingClientRect();
                    var rotateY =
                        ((event.clientX - rect.left) / rect.width - .5) * 7;
                    var rotateX =
                        ((event.clientY - rect.top) / rect.height - .5) * -7;

                    card.style.transform =
                        'perspective(900px) rotateX(' +
                        rotateX +
                        'deg) rotateY(' +
                        rotateY +
                        'deg) translateY(-5px)';
                });

                card.addEventListener('mouseleave', function () {
                    card.style.transform = '';
                });
            });
    }
}());
