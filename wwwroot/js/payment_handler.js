// If user confirms payment, don't cancel order
document.querySelector('#confirm-payment')?.addEventListener('click', function () {
    shouldWarnOnLeave = false;
});

function applyVoucher() {
    const voucherCode = document.getElementById('voucherCode').value.trim();
    const messageDiv = document.getElementById('voucherMessage');
    const voucherBtn = document.getElementById('voucherBtn');
    const btnText = voucherBtn.querySelector('.btn-text');
    const spinner = voucherBtn.querySelector('.spinner-border');

    if (!voucherCode) {
        showMessage(messageDiv, 'Vui lòng nhập mã voucher', 'danger');
        return;
    }

    // Show loading
    btnText.textContent = 'Đang kiểm tra...';
    spinner.classList.remove('d-none');
    voucherBtn.disabled = true;

    // Get CSRF token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    fetch('/Order/CheckVoucher', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ voucherCode: voucherCode })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                currentDiscount = data.discountAmount;
                appliedVoucherCode = voucherCode;
                showMessage(messageDiv, `Áp dụng voucher thành công! Giảm ${data.discountAmount.toLocaleString()}đ`, 'success');
                updateOrderSummary();
            } else {
                showMessage(messageDiv, data.message || 'Mã voucher không hợp lệ', 'danger');
                currentDiscount = 0;
                appliedVoucherCode = '';
                updateOrderSummary();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showMessage(messageDiv, 'Có lỗi xảy ra khi kiểm tra voucher', 'danger');
            currentDiscount = 0;
            appliedVoucherCode = '';
            updateOrderSummary();
        })
        .finally(() => {
            // Hide loading
            btnText.textContent = 'Áp dụng';
            spinner.classList.add('d-none');
            voucherBtn.disabled = false;
        });
}

function showMessage(element, message, type) {
    element.innerHTML = `<div class="alert alert-${type} alert-sm mb-0">${message}</div>`;
    setTimeout(() => {
        element.innerHTML = '';
    }, 5000);
}

function updateOrderSummary() {
    const discountRow = document.getElementById('discountRow');
    const discountAmount = document.getElementById('discountAmount');
    const totalAmount = document.getElementById('totalAmount');
    const savingsText = document.getElementById('savingsText');

    console.log('Updating summary:', { originalSubtotal, shippingFee, currentDiscount }); // Debug log

    if (currentDiscount > 0) {
        discountRow.style.display = 'flex';
        discountRow.style.removeProperty('display'); // Remove inline style
        discountAmount.textContent = `-${currentDiscount.toLocaleString()}đ`;
        savingsText.textContent = `Tiết kiệm ${currentDiscount.toLocaleString()}đ`;
    } else {
        discountRow.style.display = 'none';
        savingsText.textContent = 'Tiết kiệm 0đ';
    }

    const newTotal = originalSubtotal + shippingFee - currentDiscount;
    totalAmount.textContent = `${newTotal.toLocaleString()}đ`;

    console.log('New total calculated:', newTotal); // Debug log
}

function validateForm() {
    const form = document.getElementById('paymentForm');
    const inputs = form.querySelectorAll('input[required]');
    let isValid = true;

    inputs.forEach(input => {
        const value = input.value.trim();
        const feedback = input.nextElementSibling;

        if (!value) {
            input.classList.add('is-invalid');
            if (feedback && feedback.classList.contains('invalid-feedback')) {
                feedback.textContent = 'Trường này không được để trống';
            }
            isValid = false;
        } else {
            input.classList.remove('is-invalid');
            if (feedback && feedback.classList.contains('invalid-feedback')) {
                feedback.textContent = '';
            }
        }
    });

    // Validate phone number
    const phoneInput = form.querySelector('input[name="recipientPhone"]');
    const phoneValue = phoneInput.value.trim();
    const phoneRegex = /^[0-9]{10,11}$/;

    if (phoneValue && !phoneRegex.test(phoneValue)) {
        phoneInput.classList.add('is-invalid');
        const feedback = phoneInput.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = 'Số điện thoại không hợp lệ';
        }
        isValid = false;
    }

    return isValid;
}

// Add real-time validation
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('paymentForm');
    const inputs = form.querySelectorAll('input[required]');

    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            validateField(this);
        });

        input.addEventListener('input', function () {
            if (this.classList.contains('is-invalid')) {
                validateField(this);
            }
        });
    });
});

function validateField(input) {
    const value = input.value.trim();
    const feedback = input.nextElementSibling;

    if (!value) {
        input.classList.add('is-invalid');
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = 'Trường này không được để trống';
        }
        return false;
    }

    // Special validation for phone
    if (input.name === 'recipientPhone') {
        const phoneRegex = /^[0-9]{10,11}$/;
        if (!phoneRegex.test(value)) {
            input.classList.add('is-invalid');
            if (feedback && feedback.classList.contains('invalid-feedback')) {
                feedback.textContent = 'Số điện thoại không hợp lệ (10-11 chữ số)';
            }
            return false;
        }
    }

    input.classList.remove('is-invalid');
    if (feedback && feedback.classList.contains('invalid-feedback')) {
        feedback.textContent = '';
    }
    return true;
}

// Allow Enter key to apply voucher
document.getElementById('voucherCode').addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        applyVoucher();
    }
});

// Main order submission function
function submitOrder() {
    const confirmBtn = document.getElementById('confirm-payment');
    const btnText = confirmBtn.querySelector('.btn-text');
    const spinner = confirmBtn.querySelector('.spinner-border');

    // Validate form first
    if (!validateForm()) {
        alert('Vui lòng kiểm tra và điền đầy đủ thông tin');
        return;
    }

    // Show loading
    btnText.textContent = 'Đang xử lý...';
    spinner.classList.remove('d-none');
    confirmBtn.disabled = true;

    // Get form data
    const form = document.getElementById('paymentForm');
    const formData = new FormData(form);
    const paymentMethod = document.querySelector('input[name="paymentMethod"]:checked').value;

    // Calculate final amount
    const finalAmount = originalSubtotal + shippingFee - currentDiscount;

    // Prepare order data
    const orderData = {
        orderId: orderId,
        name: formData.get('recipientName'),
        phone: formData.get('recipientPhone'),
        address: formData.get('recipientAddress'),
        paymentMethod: paymentMethod,
        voucherCode: appliedVoucherCode,
        totalAmount: finalAmount
    };

    // Get CSRF token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    // Submit order
    fetch('/Order/ProcessPayment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify(orderData)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                orderCompleted = true;
                shouldWarnOnLeave = false;

                if (data.paymentMethod === 'vnpay' && data.paymentUrl) {
                    // Redirect to VNPay
                    window.location.href = data.paymentUrl;
                } else {
                    // COD payment - show success message and redirect
                    alert('Đặt hàng thành công! Cảm ơn bạn đã mua hàng.');
                    window.location.href = '/Order/Success/' + orderId;
                }
            } else {
                throw new Error(data.message || 'Có lỗi xảy ra khi đặt hàng');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Có lỗi xảy ra: ' + error.message);
        })
        .finally(() => {
            // Hide loading
            btnText.textContent = 'Đặt hàng';
            spinner.classList.add('d-none');
            confirmBtn.disabled = false;
        });
}