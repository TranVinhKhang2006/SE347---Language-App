document.addEventListener('DOMContentLoaded', () => {
    document.body.classList.add('page-ready');

    const form = document.querySelector('.account-form');
    const message = document.createElement('p');
    message.className = 'form-message';
    message.setAttribute('role', 'alert');

    if (!form) return;

    const mode = new URLSearchParams(window.location.search).get('mode') || 'signup';
    form.action = `/Account/Authentication?mode=${encodeURIComponent(mode)}`;

    const verify = () => {
        const code = form.querySelector('#verificationCode').value.trim();
        const pendingSignup = sessionStorage.getItem('pendingSignup');
        const pendingLogin = sessionStorage.getItem('pendingLogin');
        const pendingPasswordReset = sessionStorage.getItem('pendingPasswordReset');

        if (code !== '123456' || (!pendingSignup && !pendingLogin && !pendingPasswordReset)) {
            message.textContent = 'Mã xác thực không đúng hoặc phiên đăng ký đã hết hạn.';
            form.after(message);
            return false;
        }

        if (pendingSignup) {
            sessionStorage.setItem('registeredUser', pendingSignup);
            sessionStorage.removeItem('pendingSignup');
            window.location.assign('/Account/Login');
            return true;
        }

        if (pendingPasswordReset) {
            sessionStorage.removeItem('pendingPasswordReset');
            window.location.assign('/Account/Login');
            return true;
        }

        sessionStorage.setItem('userToken', 'authenticated');
        sessionStorage.removeItem('pendingLogin');
        document.cookie = 'userToken=authenticated; path=/; SameSite=Lax';
        window.location.assign('/Index');
        return true;
    };

    form.addEventListener('submit', (event) => {
        event.preventDefault();
        verify();
    });

    document.querySelector('#verificationSubmit').addEventListener('click', (event) => {
        event.preventDefault();
        verify();
    });

    form.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            form.requestSubmit();
        }
    });
});
