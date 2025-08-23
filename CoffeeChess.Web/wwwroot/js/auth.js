import { ajaxNavigator } from "./site.js";

const init = async () => {
    const $loginForm = $('#loginForm');
    const $registerForm = $('#registerForm');
    onSubmitForm($registerForm);
    onSubmitForm($loginForm);
}

function onSubmitForm($form) {
    $form.on('submit.auth', async e => {
        e.preventDefault();
        const data = $form.serialize();
        await $.ajax({
            url: $form.attr('action'),
            type: $form.attr('method'),
            data: data,
            success: async response => {
                await ajaxNavigator.loadContent(response.url);
                ajaxNavigator.toggleAuthenticatedHeader(true, response.username);
            },
            error: error => {
                
            }
        })
    })
}

const destroy = async () => {
    $('#loginForm').off('.auth');
    $('#registerForm').off('.auth');
}

export default { init, destroy }