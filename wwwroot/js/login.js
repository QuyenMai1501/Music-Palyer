// Tùy chỉnh thông báo lỗi của jQuery Validation bằng tiếng Việt
jQuery.extend(jQuery.validator.messages, {
    required: "Trường này không được để trống."
});

document.addEventListener('DOMContentLoaded', function () {
    console.log('Script đã tải'); // Ghi log để kiểm tra script chạy

    // Kiểm tra client-side cho các input
    const usernameInput = document.querySelector('input[asp-for="Username"]');
    const passwordInput = document.querySelector('input[asp-for="Password"]');
    const usernameError = usernameInput.nextElementSibling.nextElementSibling;
    const passwordError = passwordInput.nextElementSibling.nextElementSibling;

    // Kiểm tra tên người dùng
    function validateUsername() {
        if (!usernameInput.value.trim()) {
            usernameInput.classList.add('invalid');
            usernameError.textContent = 'Tên đăng nhập không được để trống.';
        } else {
            usernameInput.classList.remove('invalid');
            usernameError.textContent = '';
        }
    }

    // Kiểm tra mật khẩu
    function validatePassword() {
        if (!passwordInput.value.trim()) {
            passwordInput.classList.add('invalid');
            passwordError.textContent = 'Mật khẩu không được để trống.';
        } else {
            passwordInput.classList.remove('invalid');
            passwordError.textContent = '';
        }
    }

    // Gắn sự kiện input để kiểm tra ngay lập tức
    usernameInput.addEventListener('input', validateUsername);
    passwordInput.addEventListener('input', validatePassword);

    // Tùy chỉnh jQuery Validation
    $("#loginForm").validate({
        errorClass: "error-message",
        errorElement: "span",
        errorPlacement: function (error, element) {
            // Đặt thông báo lỗi dưới input
            error.insertAfter(element.next('i'));
        },
        rules: {
            Username: {
                required: true
            },
            Password: {
                required: true
            }
        },
        messages: {
            Username: {
                required: "Tên đăng nhập không được để trống."
            },
            Password: {
                required: "Mật khẩu không được để trống."
            }
        }
    });

    // Kiểm tra khi submit form
    const form = document.querySelector('#loginForm');
    form.addEventListener('submit', function (e) {
        console.log('Đã cố gắng submit form'); // Debug
        validateUsername();
        validatePassword();

        // Ngăn submit nếu có lỗi
        if (usernameInput.classList.contains('invalid') ||
            passwordInput.classList.contains('invalid')) {
            e.preventDefault();
        }
    });
});