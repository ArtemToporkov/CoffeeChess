$(document).ready(function () {
    const initialPageModuleSrc = $('main[role="main"]').children().first().data('page-module-src');
    if (initialPageModuleSrc) {
        import(initialPageModuleSrc).then(module => {
            if (!module)
                return;
            currentPageModule = module.default;
            if (currentPageModule && currentPageModule.init)
                currentPageModule.init();
        });
    }
    
    $(document).on('click', '.ajax-nav-link', async e => {
        e.preventDefault();
        const url = $(e.currentTarget).prop('href');
        
        const parsedUrl = new URL(url, window.location.origin);
        if (parsedUrl.pathname === window.location.pathname)
            return;
        
        await loadContent(url);
    });

    $(window).on('popstate', async e => {
        if (e.originalEvent.state && e.originalEvent.state.path) {
            await loadContent(e.originalEvent.state.path, false);
        }
    });

    history.replaceState({ path: window.location.pathname }, '', window.location.pathname);
});

let currentPageModule;

async function loadContent(url, shouldPushToHistory = true, delay = 100) {
    const $mainContainer = $('main[role="main"]');
    try {
        const data = await $.get(url);
        await hideEverythingHideable(delay);
        
        if (currentPageModule && currentPageModule.destroy) {
            currentPageModule.destroy();
            currentPageModule = null;
        }
        
        const $parsed = $(data);
        $parsed.filter('.hideable').add($parsed.find('.hideable')).each((i, el) => $(el).addClass('hide'));
        $mainContainer.html($parsed);
        requestAnimationFrame(async () => await unhideEverythingHideable(delay));
        
        if (shouldPushToHistory)
            history.pushState({path: url}, '', url);

        const moduleSrc = $mainContainer.children().first().data('page-module-src');
        if (moduleSrc) {
            currentPageModule = (await import(moduleSrc)).default;
            if (currentPageModule && currentPageModule.init) {
                currentPageModule.init();
            }
        }
    } catch (e) {
        throw new Error(`Could not load content: ${e}`);
    }
}

async function hideEverythingHideable(delay) {
    let transitionDuration = 700;
    let toWaitOverall = 0;
    $('.hideable').each((i, el) => {
        const $el = $(el);
        const order = parseInt($el.data('animation-order')) || 1;
        const toWaitToBeginTransition = (order - 1) * delay;
        setTimeout(() => {
            $el.addClass('hide');
        }, toWaitToBeginTransition);
        if (toWaitToBeginTransition > toWaitOverall)
            toWaitOverall = toWaitToBeginTransition;
    });
    await new Promise(resolve => setTimeout(resolve, toWaitOverall + transitionDuration));
}

async function unhideEverythingHideable(delay) {
    let transitionDuration = 1000;
    let toWaitOverall = 0;
    $('.hideable').each((i, el) => {
        const $el = $(el);
        const order = parseInt($el.data('animation-order')) || 1;
        const toWaitToBeginTransition = (order - 1) * delay;
        setTimeout(() => {
            $el.removeClass('hide');
        }, toWaitToBeginTransition);
        if (toWaitToBeginTransition > toWaitOverall)
            toWaitOverall = toWaitToBeginTransition;
    });
    await new Promise(resolve => setTimeout(resolve, toWaitOverall + transitionDuration));
}

function loadScript(src) {
    return new Promise((resolve, reject) => {
        if (document.querySelector(`script[src="${src}"]`)) {
            resolve();
            return;
        }
        const script = document.createElement('script');
        script.src = src;
        script.onload = () => resolve();
        script.onerror = () => reject(new Error(`Script load error for ${src}`));
        document.head.appendChild(script);
    });
}