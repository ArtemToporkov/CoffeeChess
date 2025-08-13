$(document).ready(function () {
    const $mainContainer = $('main[role="main"]');
    
    function loadContent(url, shouldPushToHistory = true) {
        $.get(url, data => {
            $mainContainer.html(data);
            if (shouldPushToHistory)
                history.pushState({path: url}, '', url);
        }).fail(() => {
           throw new Error('Could not load content.'); 
        });
    }
    
    $(document).on('click', '.ajax-nav-link', e => {
        e.preventDefault();
        const url = $(e.currentTarget).data('url');
        loadContent(url);
    });

    $(window).on('popstate', (event) => {
        if (event.originalEvent.state && event.originalEvent.state.path) {
            loadContent(event.originalEvent.state.path, false);
        }
    });

    console.log(history);
    history.replaceState({ path: window.location.pathname }, '', window.location.pathname);
})