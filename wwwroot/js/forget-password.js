document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('.account-form');
    if (!form) return;

    form.addEventListener('submit', (event) => {
        event.preventDefault();
        const email = form.querySelector('#resetEmail').value.trim();
        sessionStorage.setItem('pendingPasswordReset', email);
        window.location.assign('/Account/Authentication?mode=forgot');
    });

    form.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            form.requestSubmit();
        }
    });
});