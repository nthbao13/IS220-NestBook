function showToast(message, type = "success") {
    const toast = document.createElement("div");
    toast.className = `alert alert-${type} position-fixed top-0 end-0 m-3`;
    toast.style.zIndex = "9999";
    toast.innerHTML = `
        <i class="fas fa-${type === "success" ? "check-circle" : "exclamation-triangle"} me-2"></i>
        ${message}
        <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
    `;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

function addToCart(productId, quantity = 1) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    fetch('/Cart/AddToCart', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token || ""
        },
        body: JSON.stringify({
            productId: productId,
            quantity: quantity
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast("Đã thêm sản phẩm vào giỏ hàng!");
                const cartCount = document.querySelector('.cart-count');
                if (cartCount) cartCount.textContent = data.cartCount || 0;
            } else {
                showToast(data.message || "Có lỗi xảy ra", "danger");
            }
        })
        .catch(error => {
            showToast("Có lỗi xảy ra khi thêm sản phẩm", "danger");
        });
}

document.addEventListener("DOMContentLoaded", function () {
    document.addEventListener('click', function (e) {
        if (e.target.closest('.cart-button, .add-to-cart')) {
            const button = e.target.closest('.cart-button, .add-to-cart');
            const productId = button.dataset.productId ||
                button.closest('[data-product-id]')?.dataset.productId;

            if (productId) {
                addToCart(productId);
            } else {
                showToast("Không tìm thấy thông tin sản phẩm", "danger");
            }
        }
    });
});

document.addEventListener("DOMContentLoaded", function () {
    // Xử lý nút .btn.cart-button
    document.querySelectorAll(".btn.cart-button.require-login, .btn.add-to-cart.require-login").forEach(button => {
        button.addEventListener("click", function (e) {
            e.preventDefault(); // Ngăn hành vi mặc định của button
            e.stopPropagation(); // Ngăn sự kiện click lan truyền lên thẻ <a>

            const productId = button.dataset.productId || button.closest('[data-product-id]')?.dataset.productId;
            if (!productId) {
                console.error("Product ID not found!");
                return;
            }
            addToCart(productId);
        });
    });
});