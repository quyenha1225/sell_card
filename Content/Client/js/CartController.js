(function ($) {
    'use strict';

    var updateTimer = null;
    var requestInProgress = false;

    function token() {
        return $(
            'input[name="__RequestVerificationToken"]'
        ).first().val() || '';
    }

    function formatMoney(value) {
        var number = Number(value || 0);

        return number.toLocaleString('vi-VN') + 'đ';
    }

    function notify(message, isError) {
        if (typeof showToast === 'function') {
            showToast(message);
            return;
        }

        if (window.Toastify) {
            Toastify({
                text: message,
                duration: 3200,
                style: {
                    background:
                        isError
                            ? '#dc3545'
                            : '#198754'
                }
            }).showToast();
            return;
        }

        window.alert(message);
    }

    function redirectToLogin(xhr) {
        if (!xhr || xhr.status !== 401) {
            return false;
        }

        var result = xhr.responseJSON || null;
        var returnUrl =
            window.location.pathname +
            window.location.search +
            window.location.hash;

        window.location.href =
            result && result.loginUrl
                ? window.appUrl(result.loginUrl)
                : window.appUrl('/dang-nhap?returnUrl=') +
                  encodeURIComponent(returnUrl);

        return true;
    }

    function collectCart() {
        var items = [];

        $('.productQuantity').each(function () {
            var quantity =
                parseInt($(this).val(), 10);

            items.push({
                Quantity:
                    Number.isFinite(quantity)
                        ? Math.max(1, quantity)
                        : 1,
                product: {
                    Id: $(this).data('id')
                }
            });
        });

        return items;
    }

    function applyServerCart(response) {
        if (!response || !response.items) {
            return;
        }

        response.items.forEach(function (item) {
            var row = $(
                'tr[data-product-id="' +
                item.productId +
                '"]'
            );

            row.find('.productQuantity')
                .val(item.quantity);

            row.find('.unit-price')
                .attr('data-value', item.unitPrice)
                .text(formatMoney(item.unitPrice));

            row.find('.line-total')
                .text(formatMoney(item.lineTotal));
        });

        $('#cartGrandTotal')
            .text(formatMoney(response.total));

        $('.cart-count')
            .text(response.cartCount || 0)
            .toggleClass(
                'has-items',
                (response.cartCount || 0) > 0
            );
    }

    function updateCart() {
        if (requestInProgress) {
            return;
        }

        var items = collectCart();

        if (!items.length) {
            return;
        }

        requestInProgress = true;

        $.ajax({
            url: window.appUrl('/Cart/Update'),
            type: 'POST',
            dataType: 'json',
            data: {
                __RequestVerificationToken: token(),
                cartModel: JSON.stringify(items)
            }
        })
        .done(function (response) {
            if (!response.status) {
                notify(
                    response.message ||
                    'Không thể cập nhật giỏ hàng.',
                    true
                );

                // Reload to restore the last valid server state.
                window.setTimeout(function () {
                    window.location.reload();
                }, 900);

                return;
            }

            applyServerCart(response);
        })
        .fail(function (xhr) {
            if (redirectToLogin(xhr)) {
                return;
            }

            notify(
                'Không thể kết nối đến máy chủ.',
                true
            );
        })
        .always(function () {
            requestInProgress = false;
        });
    }

    function scheduleUpdate() {
        window.clearTimeout(updateTimer);

        updateTimer =
            window.setTimeout(updateCart, 280);
    }

    $(document).on(
        'click',
        '.plus-btn, .minus-btn',
        function () {
            var input =
                $(this)
                    .closest('.input-counter')
                    .find('.productQuantity');

            var value =
                parseInt(input.val(), 10) || 1;

            var max =
                parseInt(input.attr('max'), 10);

            if ($(this).hasClass('plus-btn')) {
                value += 1;

                if (Number.isFinite(max)) {
                    value = Math.min(max, value);
                }
            } else {
                value = Math.max(1, value - 1);
            }

            input.val(value);
            scheduleUpdate();
        }
    );

    $(document).on(
        'change input',
        '.productQuantity',
        function () {
            var value =
                parseInt($(this).val(), 10) || 1;

            var max =
                parseInt($(this).attr('max'), 10);

            value = Math.max(1, value);

            if (Number.isFinite(max)) {
                value = Math.min(max, value);
            }

            $(this).val(value);
            scheduleUpdate();
        }
    );

    $(document).on(
        'click',
        '.btnDelete',
        function () {
            var id = $(this).data('id');

            $.ajax({
                url: window.appUrl('/Cart/Delete'),
                type: 'POST',
                dataType: 'json',
                data: {
                    __RequestVerificationToken: token(),
                    id: id
                }
            })
            .done(function (response) {
                if (response.status) {
                    window.location.reload();
                }
            })
            .fail(function (xhr) {
                if (redirectToLogin(xhr)) {
                    return;
                }

                notify(
                    'Không thể xóa sản phẩm.',
                    true
                );
            });
        }
    );

    $('#btnDeleteCart').on('click', function () {
        if (!window.confirm(
            'Bạn có chắc muốn xóa toàn bộ giỏ hàng?'
        )) {
            return;
        }

        $.ajax({
            url: window.appUrl('/Cart/DeleteAllCart'),
            type: 'POST',
            dataType: 'json',
            data: {
                __RequestVerificationToken: token()
            }
        })
        .done(function (response) {
            if (response.status) {
                window.location.reload();
            }
        })
        .fail(function (xhr) {
            if (redirectToLogin(xhr)) {
                return;
            }

            notify(
                'Không thể xóa giỏ hàng.',
                true
            );
        });
    });

})(jQuery);
