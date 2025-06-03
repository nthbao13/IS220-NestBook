// requireLogin.js

// Tạo popup login yêu cầu, chỉ tạo 1 lần
function createLoginPopup() {
    if (document.getElementById('login-required-popup')) return;

    const popup = document.createElement('div');
    popup.id = 'login-required-popup';
    popup.innerHTML = `
    <div class="popup-overlay"></div>
    <div class="popup-content">
      <h3>Bạn cần đăng nhập</h3>
      <p>Bạn cần đăng nhập để thực hiện thao tác này.</p>
      <div class="popup-buttons">
        <button id="login-popup-login-btn">Đăng nhập</button>
        <button id="login-popup-cancel-btn">Hủy</button>
      </div>
    </div>
  `;
    document.body.appendChild(popup);

    // CSS popup (bạn có thể đưa vào CSS riêng nếu muốn)
    const style = document.createElement('style');
    style.textContent = `
    #login-required-popup {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
      visibility: hidden;
      opacity: 0;
      transition: opacity 0.3s ease;
    }
    #login-required-popup.show {
      visibility: visible;
      opacity: 1;
    }
    #login-required-popup .popup-overlay {
      position: absolute;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5);
      cursor: pointer;
    }
    #login-required-popup .popup-content {
      position: relative;
      background: white;
      padding: 20px 30px;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.3);
      max-width: 300px;
      text-align: center;
      z-index: 10;
    }
    #login-required-popup h3 {
      margin-top: 0;
      margin-bottom: 10px;
    }
    #login-required-popup .popup-buttons {
      margin-top: 20px;
      display: flex;
      justify-content: space-around;
    }
    #login-required-popup button {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: bold;
    }
    #login-required-popup #login-popup-login-btn {
      background-color: #007bff;
      color: white;
    }
    #login-required-popup #login-popup-cancel-btn {
      background-color: #aaa;
      color: white;
    }
    #login-required-popup button:hover {
      opacity: 0.85;
    }
  `;
    document.head.appendChild(style);

    // Sự kiện nút
    document.getElementById('login-popup-login-btn').addEventListener('click', () => {
        window.location.href = '/Customer/Account/Login'; // Đường dẫn login sửa theo app bạn
    });
    document.getElementById('login-popup-cancel-btn').addEventListener('click', () => {
        hideLoginPopup();
    });
    popup.querySelector('.popup-overlay').addEventListener('click', () => {
        hideLoginPopup();
    });
}

function showLoginPopup() {
    createLoginPopup();
    const popup = document.getElementById('login-required-popup');
    if (popup) {
        popup.classList.add('show');
    }
}

function hideLoginPopup() {
    const popup = document.getElementById('login-required-popup');
    if (popup) {
        popup.classList.remove('show');
    }
}

function initRequireLoginHandlers() {
    const isLoggedIn = window.appUser && window.appUser.isLoggedIn;
    document.querySelectorAll('.require-login').forEach(el => {
        el.addEventListener('click', function (e) {
            if (!isLoggedIn) {
                e.preventDefault();
                showLoginPopup();
            }
        });
    });
}

document.addEventListener("DOMContentLoaded", initRequireLoginHandlers);
