document.addEventListener('DOMContentLoaded', () => {
    document.body.classList.add('page-ready');

    const form = document.querySelector('.account-form');
    if (!form) return;

    form.addEventListener('submit', (event) => {
        event.preventDefault();
        const formData = new FormData(form);
        sessionStorage.setItem('pendingSignup', JSON.stringify({
            fullName: formData.get('fullName'),
            email: formData.get('email'),
            password: formData.get('password')
        }));
        window.location.assign('/Account/Authentication');
    });

    form.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            form.requestSubmit();
        }
    });
});
