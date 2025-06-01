async function createOrderAndRedirect(products, options = {}) {
    if (!products || products.length === 0) {
        alert('Vui lòng chọn sản phẩm để đặt hàng.');
        return;
    }

    // Hiển thị loading nếu có
    if (options.showLoading && typeof showLoadingModal === 'function') {
        showLoadingModal();
    }

    try {
        // Chuẩn bị dữ liệu request
        const requestData = {
            From: options.from || 0,
            Products: products.map(item => ({
                productId: item.productId || item.BookId,
                quantity: item.quantity || 1
            }))
        };

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
            window.cartConfig?.antiForgeryToken;

        const response = await fetch('/Customer/Order/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(requestData)
        });

        const result = await response.json();

        if (options.showLoading && typeof hideLoadingModal === 'function') {
            hideLoadingModal();
        }

        if (result.success) {
            if (options.successCallback) {
                options.successCallback(result);
            }

            window.location.href = '/Customer/Order/Payment';
        } else {
            const errorMessage = result.message || 'Có lỗi xảy ra khi tạo đơn hàng';

            if (options.errorCallback) {
                options.errorCallback(errorMessage);
            } else {
                alert(errorMessage);
            }
        }

    } catch (error) {
        if (options.showLoading && typeof hideLoadingModal === 'function') {
            hideLoadingModal();
        }

        console.error('Error creating order:', error);

        const errorMessage = 'Không thể kết nối đến server. Vui lòng thử lại.';
        if (options.errorCallback) {
            options.errorCallback(errorMessage);
        } else {
            alert(errorMessage);
        }
    }
}

    
function buyNowFromDetails() {
    const bookId = document.querySelector('.add-to-cart')?.getAttribute('data-product-id');
    const quantity = parseInt(document.getElementById('quantity')?.value || 1);

    if (!bookId) {
        alert('Không tìm thấy thông tin sản phẩm');
        return;
    }

    const products = [{
        productId: parseInt(bookId),
        quantity: quantity
    }];

    createOrderAndRedirect(products, {
        from : 0,
        showLoading: true,
        successCallback: (result) => {
            console.log('Đặt hàng thành công:', result.message);
        },
        errorCallback: (error) => {
            alert('Lỗi đặt hàng: ' + error);
        }
    });
}

// Hàm dành riêng cho trang giỏ hàng (cart/index)
function proceedToCheckout() {
    // Lấy danh sách sản phẩm được chọn trong giỏ hàng
    const selectedItems = [];
    const checkboxes = document.querySelectorAll('.item-checkbox:checked');

    if (checkboxes.length === 0) {
        alert('Vui lòng chọn ít nhất một sản phẩm để thanh toán.');
        return;
    }

    checkboxes.forEach(checkbox => {
        const cartId = checkbox.value;
        const row = document.getElementById(`row_${cartId}`);

        if (row) {
            const quantityInput = row.querySelector('.quantity-input');
            const quantity = parseInt(quantityInput?.value || 1);

            // Lấy bookId từ data attribute hoặc từ DOM
            const bookId = row.getAttribute('data-book-id') ||
                row.querySelector('[data-book-id]')?.getAttribute('data-book-id');

            if (bookId) {
                selectedItems.push({
                    productId: parseInt(bookId),
                    quantity: quantity
                });
            }
        }
    });

    if (selectedItems.length === 0) {
        alert('Không thể lấy thông tin sản phẩm. Vui lòng thử lại.');
        return;
    }

    createOrderAndRedirect(selectedItems, {
        from: 1,
        showLoading: true,
        successCallback: (result) => {
            console.log('Tạo đơn hàng thành công:', result.message);
            // Có thể xóa các item đã đặt hàng khỏi giỏ hàng ở đây nếu cần
        },
        errorCallback: (error) => {
            alert('Lỗi tạo đơn hàng: ' + error);
        }
    });
}

// Hàm hỗ trợ hiển thị/ẩn loading modal
function showLoadingModal() {
    const loadingModal = document.getElementById('loadingModal');
    if (loadingModal) {
        const modal = new bootstrap.Modal(loadingModal);
        modal.show();
    }
}

function hideLoadingModal() {
    const loadingModal = document.getElementById('loadingModal');
    if (loadingModal) {
        const modal = bootstrap.Modal.getInstance(loadingModal);
        if (modal) {
            modal.hide();
        }
    }
}

// Hàm kiểm tra đăng nhập (có thể sử dụng chung)
function checkLoginStatus() {
    // Kiểm tra xem người dùng đã đăng nhập chưa
    // Có thể check từ server-side hoặc từ session/cookie
    return true; // Tạm thời return true, bạn có thể customize logic này
}

// Event listeners cho các button
//document.addEventListener('DOMContentLoaded', function () {
//    const buyNowBtn = document.querySelector('.buy-now');
//    if (buyNowBtn) {
//        buyNowBtn.addEventListener('click', function () {
//            buyNowFromDetails();
//        });
//    }
//});