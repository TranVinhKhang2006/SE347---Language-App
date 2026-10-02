document.addEventListener('DOMContentLoaded', () => {
    if (new URLSearchParams(window.location.search).get('logout') === '1') {
        sessionStorage.removeItem('userToken');
        document.cookie = 'userToken=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
    }

    const form = document.querySelector('.login-card form');
    const message = document.createElement('p');
    message.className = 'login-message';
    message.setAttribute('role', 'alert');

    if (!form) return;

    form.addEventListener('submit', (event) => {
        const username = document.querySelector('#username');
        const password = document.querySelector('#password');
        const registeredUser = JSON.parse(sessionStorage.getItem('registeredUser') || 'null');

        if (!username.value.trim() || !password.value) {
            event.preventDefault();
            message.textContent = 'Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.';
            form.after(message);
            return;
        }

        if (!registeredUser || username.value.trim() !== registeredUser.email || password.value !== registeredUser.password) {
            event.preventDefault();
            message.textContent = 'Email hoặc mật khẩu không chính xác.';
            form.after(message);
            return;
        }

        sessionStorage.setItem('pendingLogin', '1');
        window.location.assign('/Account/Authentication?mode=login');
    });

    form.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            form.requestSubmit();
        }
    });
});
