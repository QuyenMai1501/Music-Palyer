document.addEventListener('DOMContentLoaded', function () {
    console.log('Script đã tải'); // Debug

    document.querySelectorAll('.toggle-password').forEach(toggle => {
        toggle.addEventListener('click', function () {
            const passwordInput = this.previousElementSibling;
            passwordInput.type = passwordInput.type === 'password' ? 'text' : 'password';
            this.classList.toggle('bx-show');
            this.classList.toggle('bx-hide');
        });
    });
});
