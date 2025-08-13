$(document).ready(function () {
    const $mainContainer = $('main[role="main"]');
    
    function loadContent(url) {
        $.get(url, data => {
            $mainContainer.html(data);
        }).fail(() => {
           throw new Error('Could not load content.'); 
        });
    }
    
    $(document).on('click', '.ajax-nav-link', e => {
        e.preventDefault();
        const url = $(e.currentTarget).data('url');
        loadContent(url);
    });
})