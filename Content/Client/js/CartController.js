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
            url: '/Cart/Update',
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
        .fail(function () {
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

            if ($(this).hasClass('plus-btn')) {
                value += 1;
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

            $(this).val(Math.max(1, value));
            scheduleUpdate();
        }
    );

    $(document).on(
        'click',
        '.btnDelete',
        function () {
            var id = $(this).data('id');

            $.ajax({
                url: '/Cart/Delete',
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
            .fail(function () {
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
            url: '/Cart/DeleteAllCart',
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
        .fail(function () {
            notify(
                'Không thể xóa giỏ hàng.',
                true
            );
        });
    });

    $(document).on(
        'click',
        '.js-quick-add',
        function () {
            var button = $(this);
            var productId =
                button.data('product-id');

            if (!productId || button.prop('disabled')) {
                return;
            }

            button.prop('disabled', true);

            $.ajax({
                url: '/Cart/AddItem',
                type: 'POST',
                dataType: 'json',
                data: {
                    __RequestVerificationToken: token(),
                    productID: productId,
                    quantity: 1
                },
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
            .done(function (response) {
                if (!response.status) {
                    notify(
                        response.message ||
                        'Không thể thêm sản phẩm.',
                        true
                    );
                    return;
                }

                $('.cart-count')
                    .text(response.cartCount || 0)
                    .toggleClass(
                        'has-items',
                        (response.cartCount || 0) > 0
                    );

                notify(
                    response.message ||
                    'Đã thêm vào giỏ hàng.',
                    false
                );
            })
            .fail(function (xhr) {
                if (xhr.status === 302
                    || xhr.status === 401) {
                    var returnUrl =
                        window.location.pathname
                        + window.location.search
                        + window.location.hash;

                    window.location.href =
                        '/dang-nhap?returnUrl='
                        + encodeURIComponent(returnUrl);
                    return;
                }

                notify(
                    'Không thể thêm sản phẩm vào giỏ.',
                    true
                );
            })
            .always(function () {
                button.prop('disabled', false);
            });
        }
    );
})(jQuery);
