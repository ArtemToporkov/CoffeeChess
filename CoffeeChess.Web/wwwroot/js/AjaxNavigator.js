export class AjaxNavigator {
    #currentPageModule;
    
    constructor() {
        const initialPageModuleSrc = $('main[role="main"]').children().first().data('page-module-src');
        if (initialPageModuleSrc) {
            import(initialPageModuleSrc).then(async module => {
                if (!module)
                    return;
                this.#currentPageModule = module.default;
                if (this.#currentPageModule && this.#currentPageModule.init)
                    await this.#currentPageModule.init();
            });
        }

        $(document).on('click', '.ajax-nav-link', async e => {
            e.preventDefault();
            const url = $(e.currentTarget).prop('href');

            const parsedUrl = new URL(url, window.location.origin);
            if (parsedUrl.pathname === window.location.pathname)
                return;

            await this.loadContent(url);
        });

        $(window).on('popstate', async e => {
            if (e.originalEvent.state && e.originalEvent.state.path) {
                await this.loadContent(e.originalEvent.state.path, false);
            }
        });

        history.replaceState({ path: window.location.pathname }, '', window.location.pathname);
    }

    async loadContent(url, shouldPushToHistory = true, delay = 100) {
        const $mainContainer = $('main[role="main"]');
        const data = await $.get(url);
        await this.hideEverythingHideable(delay);

        if (this.#currentPageModule && this.#currentPageModule.destroy) {
            await this.#currentPageModule.destroy();
            this.#currentPageModule = null;
        }

        const $parsed = $(data);
        $parsed.filter('.hideable').add($parsed.find('.hideable')).each((i, el) => $(el).addClass('hide'));
        $mainContainer.html($parsed);
        requestAnimationFrame(async () => await this.unhideEverythingHideable(delay));

        if (shouldPushToHistory)
            history.pushState({path: url}, '', url);

        const moduleSrc = $mainContainer.children().first().data('page-module-src');
        if (moduleSrc) {
            this.#currentPageModule = (await import(moduleSrc)).default;
            if (this.#currentPageModule && this.#currentPageModule.init) {
                await this.#currentPageModule.init();
            }
        }
    }

    loadScript(src) {
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

    async hideEverythingHideable(delay) {
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

    async unhideEverythingHideable(delay) {
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
}