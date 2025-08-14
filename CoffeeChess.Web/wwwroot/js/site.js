$(document).ready(function () {
    const initialPageModule = $('main[role="main"]').children().first().data('page-module');
    if (initialPageModule) {
        pageModules[initialPageModule]().then(module => {
            currentPageModule = module.default;
            if (currentPageModule && currentPageModule.init)
                currentPageModule.init();
        });
    }
    
    $(document).on('click', '.ajax-nav-link', async e => {
        e.preventDefault();
        const url = $(e.currentTarget).prop('href');
        await loadContent(url);
    });

    $(window).on('popstate', async e => {
        if (e.originalEvent.state && e.originalEvent.state.path) {
            await loadContent(e.originalEvent.state.path, false);
        }
    });

    history.replaceState({ path: window.location.pathname }, '', window.location.pathname);
});

const pageModules = {
    "game-waiting": () => import('/js/game/waiting.js'),
    "game-play": () => import('/js/game/connection.js'),
    "game-review": () => import('/js/game/read/review.js'),
    "games-history": () => import('/js/game/read/games-history.js')
}

let currentPageModule;

async function loadContent(url, shouldPushToHistory = true) {
    const $mainContainer = $('main[role="main"]');
    try {
        const data = await $.get(url);
        if (currentPageModule && currentPageModule.destroy) {
            currentPageModule.destroy();
            currentPageModule = null;
        }

        $mainContainer.html(data);
        if (shouldPushToHistory)
            history.pushState({path: url}, '', url);

        const moduleName = $mainContainer.children().first().data('page-module');
        if (moduleName) {
            currentPageModule = (await pageModules[moduleName]()).default;
            if (currentPageModule && currentPageModule.init) {
                currentPageModule.init();
            }
        }
    } catch (e) {
        throw new Error(`Could not load content: ${e}`);
    }
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