let reviewsData = [];
let currentUserInfo = {};

// Đợi DOM load xong
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing profile page...');

    // Debug: Kiểm tra các elements có tồn tại không
    const sidebarLinks = document.querySelectorAll('.sidebar ul li a');
    const tabs = document.querySelectorAll('.tab');

    console.log('Found sidebar links:', sidebarLinks.length);
    console.log('Found tabs:', tabs.length);

    // Sidebar navigation - FIX: Sử dụng selector chính xác
    sidebarLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            console.log('Sidebar clicked:', this.getAttribute('data-section'));

            // Update active sidebar item
            sidebarLinks.forEach(a => a.classList.remove('active'));
            this.classList.add('active');

            // Show/hide sections
            const section = this.getAttribute('data-section');
            const ordersSection = document.getElementById('orders-section');
            const reviewsSection = document.getElementById('reviews-section');
            const accountSection = document.getElementById('account-section');

            // Hide all sections
            if (ordersSection) ordersSection.style.display = 'none';
            if (reviewsSection) reviewsSection.style.display = 'none';
            if (accountSection) accountSection.style.display = 'none';

            // Show selected section
            if (section === 'orders' && ordersSection) {
                ordersSection.style.display = 'block';
            } else if (section === 'reviews' && reviewsSection) {
                reviewsSection.style.display = 'block';
                loadReviewsSection();
            } else if (section === 'account' && accountSection) {
                accountSection.style.display = 'block';
                loadAccountSection();
            }
        });
    });

    // Tab switching for orders - FIX: Thêm kiểm tra tồn tại
    tabs.forEach(tab => {
        tab.addEventListener('click', function () {
            console.log('Tab clicked:', this.getAttribute('data-tab'));

            tabs.forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('active'));

            this.classList.add('active');
            const tabId = this.getAttribute('data-tab');
            const targetPane = document.getElementById(tabId);

            if (targetPane) {
                targetPane.classList.add('active');
            } else {
                console.error('Tab pane not found:', tabId);
            }
        });
    });
});

// Load reviews section
function loadReviewsSection() {
    const reviewsContent = document.getElementById('reviews-content');
    if (!reviewsContent) {
        console.error('Reviews content element not found');
        return;
    }

    reviewsContent.innerHTML = '<div style="text-align: center; padding: 20px;">Đang tải...</div>';

    fetch('/Customer/Account/GetPendingReviews')
        .then(response => response.json())
        .then(data => {
            renderReviewsContent(data);
            reviewsData = data;
        })
        .catch(error => {
            console.error('Error:', error);
            reviewsContent.innerHTML =
                '<div class="empty-state"><h3>Có lỗi xảy ra</h3><p>Không thể tải dữ liệu đánh giá</p></div>';
        });
}

function renderReviewsContent(data) {
    let html = '';

    if (data.length > 0) {
        // Group by order
        const orderGroups = {};
        data.forEach(item => {
            if (!orderGroups[item.orderId]) {
                orderGroups[item.orderId] = {
                    orderId: item.orderId,
                    orderDate: item.orderDate,
                    books: []
                };
            }
            orderGroups[item.orderId].books.push(item);
        });

        Object.values(orderGroups).forEach(group => {
            html += `
                <div class="review-item" data-order-id="${group.orderId}">
                    <div class="review-header">
                        <div class="order-info">
                            <span class="order-id">Đơn hàng #${group.orderId}</span>
                            <span class="order-status status-completed">Chờ đánh giá</span>
                        </div>
                        <div style="font-size: 14px; color: #666; margin-top: 5px;">
                            Ngày đặt: ${new Date(group.orderDate).toLocaleDateString('vi-VN')}
                        </div>
                    </div>
                    <div class="review-body">
            `;

            group.books.forEach(book => {
                html += `
                    <div class="review-book-item" data-book-id="${book.bookId}">
                        <img src="${book.bookImage || '/images/default-book.jpg'}" alt="${book.bookTitle}" class="book-image">
                        <div class="book-info">
                            <div class="book-title">${book.bookTitle}</div>
                            <div class="book-author">Tác giả: ${book.bookAuthor}</div>
                            <div class="book-price">${book.bookPrice.toLocaleString()} đ</div>
                        </div>
                        <button class="btn btn-warning" onclick="showReviewForm(${book.orderId}, ${book.bookId}, '${book.bookTitle}', '${book.bookImage}', this)">
                            Viết đánh giá
                        </button>
                    </div>
                `;
            });

            html += `
                    </div>
                </div>
            `;
        });
    } else {
        html = '<div class="empty-state"><h3>Không có sản phẩm nào cần đánh giá</h3><p>Tất cả sản phẩm đã được đánh giá hoặc chưa có đơn hàng hoàn thành</p></div>';
    }

    document.getElementById('reviews-content').innerHTML = html;
}

function showReviewForm(orderId, bookId, bookTitle, bookImage, buttonElement) {
    // Hide the button and show review form
    buttonElement.style.display = 'none';

    const bookItem = buttonElement.closest('.review-book-item');
    const reviewForm = document.createElement('div');
    reviewForm.className = 'review-form';
    reviewForm.innerHTML = `
        <h4 style="color: #8B4513; margin-bottom: 10px;">Đánh giá: ${bookTitle}</h4>
        <div>
            <label style="display: block; margin-bottom: 5px; font-weight: bold;">Số sao:</label>
            <div class="stars" id="stars-${bookId}" data-current-rating="0">
                <span class="star" data-rating="1">★</span>
                <span class="star" data-rating="2">★</span>
                <span class="star" data-rating="3">★</span>
                <span class="star" data-rating="4">★</span>
                <span class="star" data-rating="5">★</span>
            </div>
        </div>
        <div>
            <label style="display: block; margin-bottom: 5px; font-weight: bold;">Nhận xét:</label>
            <textarea class="review-textarea" id="comment-${bookId}" placeholder="Chia sẻ trải nghiệm của bạn về sản phẩm này..."></textarea>
        </div>
        <div style="display: flex; gap: 10px; margin-top: 15px;">
            <button class="btn btn-primary" onclick="submitReview(${orderId}, ${bookId}, this)">Gửi đánh giá</button>
            <button class="btn btn-outline" onclick="cancelReview(${bookId}, this)">Hủy</button>
        </div>
    `;

    bookItem.appendChild(reviewForm);

    // Initialize star rating for this form
    initStarRating(bookId);
}

function initStarRating(bookId) {
    const starsContainer = document.getElementById(`stars-${bookId}`);
    const stars = starsContainer.querySelectorAll('.star');
    let currentRating = 0;

    stars.forEach(star => {
        star.addEventListener('click', function () {
            currentRating = parseInt(this.getAttribute('data-rating'));
            starsContainer.setAttribute('data-current-rating', currentRating);
            updateStars(bookId, currentRating);
        });

        star.addEventListener('mouseover', function () {
            const hoverRating = parseInt(this.getAttribute('data-rating'));
            updateStars(bookId, hoverRating);
        });
    });

    starsContainer.addEventListener('mouseleave', function () {
        updateStars(bookId, currentRating);
    });
}

function updateStars(bookId, rating) {
    const stars = document.querySelectorAll(`#stars-${bookId} .star`);
    stars.forEach((star, index) => {
        if (index < rating) {
            star.classList.add('active');
        } else {
            star.classList.remove('active');
        }
    });
}

function submitReview(orderId, bookId, buttonElement) {
    const starsContainer = document.getElementById(`stars-${bookId}`);
    const rating = parseInt(starsContainer.getAttribute('data-current-rating')) || 0;
    const comment = document.getElementById(`comment-${bookId}`).value.trim();

    // Validation
    if (rating === 0) {
        alert('Vui lòng chọn số sao đánh giá');
        return;
    }

    // Disable button để tránh double submit
    buttonElement.disabled = true;
    buttonElement.textContent = 'Đang gửi...';

    // Lấy anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    // Tạo FormData
    const formData = new FormData();
    formData.append('orderId', orderId);
    formData.append('bookId', bookId);
    formData.append('rating', rating);
    formData.append('content', comment);

    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    fetch('/Customer/Account/AddReview', {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                alert('Đánh giá đã được gửi thành công!');

                try {
                    const bookItem = buttonElement.closest('.review-book-item');
                    if (!bookItem) {
                        console.error('Không tìm thấy .review-book-item');
                        location.reload();
                        return;
                    }

                    bookItem.remove();

                    const reviewItem = document.querySelector(`[data-order-id="${orderId}"]`);
                    if (reviewItem) {
                        const remainingBooks = reviewItem.querySelectorAll('.review-book-item');
                        if (remainingBooks.length === 0) {
                            reviewItem.remove();
                        }
                    }

                    const reviewsContent = document.getElementById('reviews-content');
                    if (reviewsContent) {
                        const remainingReviews = reviewsContent.querySelectorAll('.review-item');
                        if (remainingReviews.length === 0) {
                            reviewsContent.innerHTML = '<div class="empty-state"><h3>Không có sản phẩm nào cần đánh giá</h3><p>Tất cả sản phẩm đã được đánh giá</p></div>';
                        }
                    }
                } catch (domError) {
                    console.error('DOM manipulation error:', domError);
                    alert('Đánh giá thành công! Trang sẽ được tải lại.');
                    location.reload();
                }
            } else {
                alert(data.message || 'Có lỗi xảy ra');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Có lỗi xảy ra khi gửi đánh giá: ' + error.message);
        })
        .finally(() => {
            if (buttonElement && !buttonElement.closest('.review-book-item')?.hasAttribute('data-removed')) {
                buttonElement.disabled = false;
                buttonElement.textContent = 'Gửi đánh giá';
            }
        });
}

function cancelReview(bookId, buttonElement) {
    const reviewForm = buttonElement.closest('.review-form');
    const bookItem = reviewForm.closest('.review-book-item');
    const originalButton = bookItem.querySelector('.btn-warning');

    reviewForm.remove();
    originalButton.style.display = 'inline-block';
}

// Load account information section
function loadAccountSection() {
    const accountContent = document.getElementById('account-content');
    if (!accountContent) {
        console.error('Account content element not found');
        return;
    }

    accountContent.innerHTML = `
        <div class="loading-container" style="text-align: center; padding: 50px;">
            <div class="loading-spinner" style="
                width: 50px; 
                height: 50px; 
                border: 4px solid #f3f3f3; 
                border-top: 4px solid #D2B48C; 
                border-radius: 50%; 
                animation: spin 1s linear infinite; 
                margin: 0 auto 20px;
            "></div>
            <p style="color: #666;">Đang tải thông tin tài khoản...</p>
        </div>
    `;

    fetch('/Customer/Account/GetAccountInfo')
        .then(response => response.json())
        .then(data => {
            currentUserInfo = data;
            renderAccountContent(data);
        })
        .catch(error => {
            console.error('Error:', error);
            showErrorState();
        });
}

function renderAccountContent(userInfo) {
    const html = `
        <div class="account-info-container">
            <div class="account-form">
                <div class="avatar-section">
                    <div class="avatar-container">
                        <img id="user-avatar" src="${userInfo.image || '/images/default-avatar.jpg'}" alt="Avatar" class="user-avatar">
                        <div class="avatar-overlay">
                            <input type="file" id="avatar-input" accept="image/*" style="display: none;" onchange="handleAvatarChange(this)">
                            <button type="button" class="btn-change-avatar" onclick="document.getElementById('avatar-input').click()">
                                📷 Đổi ảnh
                            </button>
                        </div>
                    </div>
                    <h3 style="color: #333; margin: 0; font-size: 1.4rem;">
                        ${userInfo.firstName && userInfo.lastName ? `${userInfo.firstName} ${userInfo.lastName}` : 'Chưa cập nhật tên'}
                    </h3>
                </div>
                
                <div class="form-group">
                    <label for="user-email">📧 Email:</label>
                    <input type="email" id="user-email" value="${userInfo.email || ''}" disabled class="form-control disabled">
                    <small class="text-muted">Email không thể thay đổi</small>
                </div>
                
                <div class="form-group">
                    <label for="user-firstname">👤 Họ:</label>
                    <input type="text" id="user-firstname" value="${userInfo.firstName || ''}" class="form-control" placeholder="Nhập họ của bạn">
                </div>
                
                <div class="form-group">
                    <label for="user-lastname">👤 Tên:</label>
                    <input type="text" id="user-lastname" value="${userInfo.lastName || ''}" class="form-control" placeholder="Nhập tên của bạn">
                </div>
                
                <div class="form-group">
                    <label for="user-phone">📱 Số điện thoại:</label>
                    <input type="tel" id="user-phone" value="${userInfo.phone || ''}" class="form-control" placeholder="Nhập số điện thoại">
                </div>
                
                <div class="form-actions">
                    <button type="button" class="btn btn-primary" onclick="updateAccountInfo()">
                        💾 Cập nhật thông tin
                    </button>
                    <button type="button" class="btn btn-outline" onclick="resetAccountForm()">
                        🔄 Khôi phục
                    </button>
                </div>
            </div>
        </div>
    `;

    document.getElementById('account-content').innerHTML = html;
    addRealTimeValidation();
}

function addRealTimeValidation() {
    const inputs = document.querySelectorAll('#account-content .form-control:not(.disabled)');

    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            validateField(this);
        });

        input.addEventListener('input', function () {
            this.classList.remove('error');
        });
    });
}

function validateField(field) {
    const value = field.value.trim();
    let isValid = true;

    switch (field.id) {
        case 'user-firstname':
        case 'user-lastname':
            isValid = value.length >= 1;
            break;
        case 'user-phone':
            if (value) {
                isValid = /^[0-9+\-\s()]+$/.test(value) && value.length >= 10;
            }
            break;
    }

    if (isValid) {
        field.classList.add('success');
        field.classList.remove('error');
    } else {
        field.classList.add('error');
        field.classList.remove('success');
    }

    return isValid;
}

function handleAvatarChange(input) {
    if (input.files && input.files[0]) {
        const file = input.files[0];

        if (!file.type.startsWith('image/')) {
            showNotification('Vui lòng chọn file hình ảnh', 'error');
            return;
        }

        if (file.size > 5 * 1024 * 1024) {
            showNotification('Kích thước file không được vượt quá 5MB', 'error');
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            const avatar = document.getElementById('user-avatar');
            avatar.src = e.target.result;

            avatar.style.transform = 'scale(0.8)';
            avatar.style.opacity = '0.5';
            setTimeout(() => {
                avatar.style.transform = 'scale(1)';
                avatar.style.opacity = '1';
            }, 300);
        };
        reader.readAsDataURL(file);

        showNotification('Ảnh đã được chọn. Nhấn "Cập nhật thông tin" để lưu.', 'info');
    }
}

function updateAccountInfo() {
    const firstName = document.getElementById('user-firstname').value.trim();
    const lastName = document.getElementById('user-lastname').value.trim();
    const phone = document.getElementById('user-phone').value.trim();
    const avatarInput = document.getElementById('avatar-input');
    const updateBtn = document.querySelector('.btn-primary');

    let isValid = true;

    if (!firstName || !lastName) {
        showNotification('Vui lòng nhập đầy đủ họ và tên', 'error');
        isValid = false;
    }

    if (phone && !/^[0-9+\-\s()]+$/.test(phone)) {
        showNotification('Số điện thoại không hợp lệ', 'error');
        isValid = false;
    }

    if (!isValid) return;

    updateBtn.classList.add('loading');
    updateBtn.textContent = 'Đang cập nhật...';

    const formData = new FormData();
    formData.append('firstName', firstName);
    formData.append('lastName', lastName);
    formData.append('phone', phone);

    if (avatarInput.files && avatarInput.files[0]) {
        formData.append('image', avatarInput.files[0]);
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    fetch('/Customer/Account/UpdateAccountInfo', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('Cập nhật thông tin thành công!', 'success');
                currentUserInfo = { ...currentUserInfo, firstName, lastName, phone };
                if (data.imageUrl) {
                    currentUserInfo.image = data.imageUrl;
                }

                const displayName = document.querySelector('.avatar-section h3');
                if (displayName) {
                    displayName.textContent = `${firstName} ${lastName}`;
                }
            } else {
                showNotification(data.message || 'Có lỗi xảy ra khi cập nhật thông tin', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('Có lỗi xảy ra khi cập nhật thông tin', 'error');
        })
        .finally(() => {
            updateBtn.classList.remove('loading');
            updateBtn.textContent = '💾 Cập nhật thông tin';
        });
}

function resetAccountForm() {
    if (confirm('Bạn có chắc chắn muốn khôi phục thông tin về trạng thái ban đầu?')) {
        document.getElementById('user-firstname').value = currentUserInfo.firstName || '';
        document.getElementById('user-lastname').value = currentUserInfo.lastName || '';
        document.getElementById('user-phone').value = currentUserInfo.phone || '';
        document.getElementById('user-avatar').src = currentUserInfo.image || '/images/default-avatar.jpg';
        document.getElementById('avatar-input').value = '';

        const inputs = document.querySelectorAll('#account-content .form-control');
        inputs.forEach(input => {
            input.classList.remove('success', 'error');
        });

        showNotification('Đã khôi phục thông tin ban đầu', 'info');
    }
}

function showErrorState() {
    document.getElementById('account-content').innerHTML = `
        <div class="error-state" style="text-align: center; padding: 50px; color: #dc3545;">
            <div style="font-size: 4rem; margin-bottom: 20px;">😞</div>
            <h3>Có lỗi xảy ra</h3>
            <p>Không thể tải thông tin tài khoản. Vui lòng thử lại sau.</p>
            <button class="btn btn-primary" onclick="loadAccountSection()" style="margin-top: 20px;">
                🔄 Thử lại
            </button>
        </div>
    `;
}

function showNotification(message, type = 'info') {
    const existingNotification = document.querySelector('.notification');
    if (existingNotification) {
        existingNotification.remove();
    }

    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 20px;
        border-radius: 10px;
        color: white;
        font-weight: 600;
        z-index: 9999;
        transform: translateX(100%);
        transition: all 0.3s ease;
        max-width: 350px;
        box-shadow: 0 4px 15px rgba(0,0,0,0.2);
    `;

    const colors = {
        success: '#28a745',
        error: '#dc3545',
        info: '#17a2b8',
        warning: '#ffc107'
    };

    notification.style.background = colors[type] || colors.info;
    notification.textContent = message;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);

    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 300);
    }, 4000);
}

// CSS cho spin animation
const style = document.createElement('style');
style.textContent = `
    @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
    }
`;
document.head.appendChild(style);

// Confirm received order
function confirmReceived(orderId) {
    if (confirm('Xác nhận bạn đã nhận được hàng?')) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

        fetch('/Customer/Account/ConfirmReceived', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: `orderId=${orderId}`
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Xác nhận thành công!');
                    location.reload();
                } else {
                    alert(data.message || 'Có lỗi xảy ra');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Có lỗi xảy ra');
            });
    }
}