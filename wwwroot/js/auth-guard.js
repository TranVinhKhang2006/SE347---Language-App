function validateAdmin(token) {
    // Add validating logic here.
    return token === 'admin-123';
}

function hasUserToken() {
    return Boolean(sessionStorage.getItem('userToken') || document.cookie.split('; ').some(cookie => cookie === 'userToken=authenticated'));
}

if (!hasUserToken()) {
    window.location.replace('/Account/Login');
}