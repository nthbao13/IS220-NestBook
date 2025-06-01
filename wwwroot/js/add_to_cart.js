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

// Biến để ngăn request trùng lặp
let isAddingToCart = false;

function addToCart(productId, quantity = 1) {
    // Ngăn request trùng lặp
    if (isAddingToCart) {
        console.log("Already adding to cart, please wait...");
        return;
    }

    isAddingToCart = true;

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    fetch('/Customer/Cart/AddToCart', {
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
            console.error("Add to cart error:", error);
            showToast("Có lỗi xảy ra khi thêm sản phẩm", "danger");
        })
        .finally(() => {
            // Reset flag sau khi hoàn thành
            isAddingToCart = false;
        });
}

// CHỈ MỘT EVENT LISTENER DUY NHẤT
document.addEventListener("DOMContentLoaded", function () {
    // Sử dụng event delegation để xử lý tất cả các nút add to cart
    document.addEventListener('click', function (e) {
        // Tìm button add to cart được click
        const button = e.target.closest('.cart-button, .add-to-cart');

        if (button) {
            e.preventDefault(); // Ngăn hành vi mặc định
            e.stopPropagation(); // Ngăn event bubbling

            // Lấy product ID
            const productId = button.dataset.productId ||
                button.closest('[data-product-id]')?.dataset.productId;

            if (productId) {
                console.log("Adding product to cart:", productId);
                addToCart(productId);
            } else {
                showToast("Không tìm thấy thông tin sản phẩm", "danger");
                console.error("Product ID not found for button:", button);
            }
        }
    });

    console.log("Cart functionality initialized");
});