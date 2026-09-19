document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('.login-card form');
    const message = document.createElement('p');
    message.className = 'login-message';
    message.setAttribute('role', 'alert');

    if (!form) return;

    form.addEventListener('submit', (event) => {
        const username = document.querySelector('#username');
        const password = document.querySelector('#password');

        if (!username.value.trim() || !password.value) {
            event.preventDefault();
            message.textContent = 'Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.';
            form.after(message);
        }
    });
});
