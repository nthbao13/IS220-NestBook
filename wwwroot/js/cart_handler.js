// Shopping Cart JavaScript - Improved Version
// Utility functions
function formatCurrency(amount) {
    return new Intl.NumberFormat("vi-VN").format(amount) + " đ";
}

// Loading state management
let isLoading = false;

function showLoadingCustom() {
    if (isLoading) return;
    isLoading = true;

    // Create loading overlay
    const overlay = document.createElement('div');
    overlay.id = 'customLoadingOverlay';
    overlay.innerHTML = `
        <div style="
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 9999;
        ">
            <div style="
                background: white;
                padding: 20px;
                border-radius: 8px;
                text-align: center;
                box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            ">
                <div class="spinner-border text-primary mb-3" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mb-0">Đang cập nhật...</p>
            </div>
        </div>
    `;

    document.body.appendChild(overlay);
}

function hideLoadingCustom() {
    isLoading = false;
    const overlay = document.getElementById('customLoadingOverlay');
    if (overlay) {
        overlay.remove();
    }
}

// Toast notification system
function showToast(message, type = "success") {
    const toastId = 'toast_' + Date.now();
    const toast = document.createElement("div");
    toast.id = toastId;
    toast.className = `alert alert-${type} alert-dismissible position-fixed top-0 end-0 m-3`;
    toast.style.zIndex = "10000";
    toast.innerHTML = `
        <i class="fas fa-${type === "success" ? "check-circle" :
            type === "danger" ? "exclamation-triangle" :
                type === "warning" ? "exclamation-circle" : "info-circle"} me-2"></i>
        ${message}
        <button type="button" class="btn-close" onclick="removeToast('${toastId}')"></button>
    `;

    document.body.appendChild(toast);

    // Auto remove after 3 seconds
    setTimeout(() => removeToast(toastId), 3000);
}

function removeToast(toastId) {
    const toast = document.getElementById(toastId);
    if (toast && toast.parentElement) {
        toast.remove();
    }
}

// Cart quantity management
function increaseQuantity(itemId) {
    const input = document.getElementById("quantity_" + itemId);
    if (!input) return;

    const newQuantity = parseInt(input.value) + 1;
    input.value = newQuantity;
    updateCartItem(itemId, newQuantity);
}

function decreaseQuantity(itemId) {
    const input = document.getElementById("quantity_" + itemId);
    if (!input) return;

    const currentQuantity = parseInt(input.value);
    if (currentQuantity > 1) {
        const newQuantity = currentQuantity - 1;
        input.value = newQuantity;
        updateCartItem(itemId, newQuantity);
    }
}

function updateQuantityManual(itemId, quantity) {
    const qty = parseInt(quantity);
    if (isNaN(qty) || qty < 1) {
        const input = document.getElementById("quantity_" + itemId);
        if (input) {
            input.value = 1;
            updateCartItem(itemId, 1);
        }
    } else {
        updateCartItem(itemId, qty);
    }
}

// Main cart update function with proper error handling
function updateCartItem(itemId, quantity) {
    // Prevent multiple simultaneous requests
    if (isLoading) return;

    showLoadingCustom();

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const updateUrl = window.cartConfig?.updateUrl || '/Cart/UpdateCartQuantity';

    fetch(updateUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token || ""
        },
        body: JSON.stringify({
            itemId: itemId,
            quantity: quantity
        })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                showToast("Cập nhật số lượng thành công!");
                updateItemSubtotal(itemId, quantity);
                updateCartTotals();

                // Update total amounts if provided by server
                if (data.newTotal !== undefined) {
                    updateTotalDisplay(data.newTotal);
                }
            } else {
                showToast(data.message || "Có lỗi xảy ra khi cập nhật", "danger");
                // Revert quantity on failure
                revertQuantity(itemId);
            }
        })
        .catch(error => {
            console.error("Cart update error:", error);
            showToast("Có lỗi xảy ra khi cập nhật: " + error.message, "danger");
            // Revert quantity on error
            revertQuantity(itemId);
        })
        .finally(() => {
            hideLoadingCustom();
        });
}

// Helper function to revert quantity on error
function revertQuantity(itemId) {
    const input = document.getElementById("quantity_" + itemId);
    if (input && input.dataset.originalValue) {
        input.value = input.dataset.originalValue;
    }
}

// Update item subtotal in UI
function updateItemSubtotal(itemId, quantity) {
    const row = document.getElementById("row_" + itemId);
    if (!row) return;

    const unitPriceElement = row.querySelector(".unit-price");
    const subtotalElement = document.getElementById("subtotal_" + itemId);

    if (!unitPriceElement || !subtotalElement) return;

    const unitPrice = parseFloat(unitPriceElement.getAttribute("data-price"));
    if (isNaN(unitPrice)) return;

    const subtotal = unitPrice * quantity;
    subtotalElement.textContent = formatCurrency(subtotal);
}

// Update total display elements
function updateTotalDisplay(newTotal) {
    const totalElements = ["totalAmount", "finalTotal"];
    totalElements.forEach(elementId => {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = formatCurrency(newTotal);
        }
    });
}

// Update cart totals based on selected items
function updateCartTotals() {
    let total = 0;
    const checkedItems = document.querySelectorAll(".item-checkbox:checked");

    checkedItems.forEach((checkbox) => {
        const itemId = checkbox.value;
        const row = document.getElementById("row_" + itemId);
        if (!row) return;

        const quantityInput = document.getElementById("quantity_" + itemId);
        const unitPriceElement = row.querySelector(".unit-price");

        if (!quantityInput || !unitPriceElement) return;

        const quantity = parseInt(quantityInput.value);
        const unitPrice = parseFloat(unitPriceElement.getAttribute("data-price"));

        if (!isNaN(quantity) && !isNaN(unitPrice)) {
            total += unitPrice * quantity;
        }
    });

    updateTotalDisplay(total);
}

// Remove item from cart
function removeItem(itemId) {
    if (!confirm("Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?")) {
        return;
    }

    if (isLoading) return;

    showLoadingCustom();

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const removeUrl = window.cartConfig?.removeUrl || '/Cart/RemoveFromCart';

    fetch(removeUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token || ""
        },
        body: JSON.stringify({ itemId: itemId })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Remove row from table
                const row = document.getElementById("row_" + itemId);
                if (row) {
                    row.remove();
                }

                // Update item counts
                updateItemCounts();

                // Update totals
                updateCartTotals();

                showToast("Đã xóa sản phẩm khỏi giỏ hàng");

                // Reload page if no items left
                const remainingItems = document.querySelectorAll(".item-checkbox").length;
                if (remainingItems === 0) {
                    setTimeout(() => location.reload(), 1000);
                }
            } else {
                showToast(data.message || "Có lỗi xảy ra khi xóa sản phẩm", "danger");
            }
        })
        .catch(error => {
            console.error("Remove item error:", error);
            showToast("Có lỗi xảy ra khi xóa sản phẩm", "danger");
        })
        .finally(() => {
            hideLoadingCustom();
        });
}

// Update item count displays
function updateItemCounts() {
    const remainingItems = document.querySelectorAll(".item-checkbox").length;

    const totalItemsElement = document.getElementById("totalItemsCount");
    const selectAllCountElement = document.getElementById("selectAllCount");

    if (totalItemsElement) {
        totalItemsElement.textContent = remainingItems;
    }
    if (selectAllCountElement) {
        selectAllCountElement.textContent = remainingItems;
    }
}

// Apply promo code
function applyPromoCode() {
    const promoCodeInput = document.getElementById("promoCode");
    if (!promoCodeInput) return;

    const promoCode = promoCodeInput.value.trim();
    if (promoCode === "") {
        showToast("Vui lòng nhập mã khuyến mãi", "warning");
        promoCodeInput.focus();
        return;
    }

    if (isLoading) return;

    showLoadingCustom();

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const applyUrl = window.cartConfig?.promoUrl || '/Cart/ApplyPromoCode';

    fetch(applyUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token || ""
        },
        body: JSON.stringify({ promoCode: promoCode })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                showToast("Áp dụng mã khuyến mãi thành công!");

                // Update total with discount
                if (data.newTotal !== undefined) {
                    updateTotalDisplay(data.newTotal);
                }

                // Clear the promo code input
                promoCodeInput.value = "";

                // Show discount information if provided
                if (data.discountAmount) {
                    showDiscountInfo(data.discountAmount);
                }
            } else {
                showToast(data.message || "Mã khuyến mãi không hợp lệ", "danger");
            }
        })
        .catch(error => {
            console.error("Promo code error:", error);
            showToast("Có lỗi xảy ra khi áp dụng mã khuyến mãi", "danger");
        })
        .finally(() => {
            hideLoadingCustom();
        });
}

// Show discount information
function showDiscountInfo(discountAmount) {
    const discountElement = document.getElementById("discountInfo");
    if (discountElement) {
        discountElement.innerHTML = `
            <div class="text-success">
                <i class="fas fa-tag me-1"></i>
                Giảm giá: ${formatCurrency(discountAmount)}
            </div>
        `;
        discountElement.style.display = "block";
    }
}

// Select all functionality and event handlers
function initializeCartEventHandlers() {
    const selectAllCheckbox = document.getElementById("selectAll");

    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener("change", function () {
            const checkboxes = document.querySelectorAll(".item-checkbox");
            checkboxes.forEach((checkbox) => {
                checkbox.checked = this.checked;
            });
            updateCartTotals();
        });
    }

    // Individual checkbox change events
    document.querySelectorAll(".item-checkbox").forEach((checkbox) => {
        checkbox.addEventListener("change", function () {
            updateCartTotals();
            updateSelectAllState();
        });
    });

    // Store original quantities for error recovery
    document.querySelectorAll('input[id^="quantity_"]').forEach(input => {
        input.dataset.originalValue = input.value;

        // Update original value when input changes successfully
        input.addEventListener('input', function () {
            // Debounce the input to avoid too many API calls
            clearTimeout(this.updateTimeout);
            this.updateTimeout = setTimeout(() => {
                const itemId = this.id.replace('quantity_', '');
                updateQuantityManual(itemId, this.value);
            }, 500);
        });
    });

    // Add keyboard support for promo code
    const promoCodeInput = document.getElementById("promoCode");
    if (promoCodeInput) {
        promoCodeInput.addEventListener("keypress", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                applyPromoCode();
            }
        });
    }
}

// Update select all checkbox state
function updateSelectAllState() {
    const selectAllCheckbox = document.getElementById("selectAll");
    if (!selectAllCheckbox) return;

    const allCheckboxes = document.querySelectorAll(".item-checkbox");
    const checkedCheckboxes = document.querySelectorAll(".item-checkbox:checked");

    selectAllCheckbox.checked = allCheckboxes.length > 0 && allCheckboxes.length === checkedCheckboxes.length;
    selectAllCheckbox.indeterminate = checkedCheckboxes.length > 0 && checkedCheckboxes.length < allCheckboxes.length;
}

// Initialize when DOM is ready
document.addEventListener("DOMContentLoaded", function () {
    initializeCartEventHandlers();
    updateCartTotals(); // Calculate initial totals
    updateSelectAllState(); // Set initial select all state

    // Global error handler for unhandled promise rejections
    window.addEventListener('unhandledrejection', function (event) {
        console.error('Unhandled promise rejection:', event.reason);
        showToast("Có lỗi không mong muốn xảy ra", "danger");
    });
});

// Export functions for testing (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        formatCurrency,
        updateCartItem,
        removeItem,
        applyPromoCode
    };
}