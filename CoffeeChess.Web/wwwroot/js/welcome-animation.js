const trueString = "true"
const welcomeAnimationAlreadyPlayed = localStorage.getItem("welcomeAnimationAlreadyPlayed") === trueString;
if (!welcomeAnimationAlreadyPlayed) {
    setUpWelcomeAnimation();
    playWelcomeAnimation();
    localStorage.setItem("welcomeAnimationAlreadyPlayed", trueString);
}

function setUpWelcomeAnimation() {
    const toUnhide = [
        $('#welcomePageOverlay'), 
        $('#welcomeTitleForAnimation'), 
        $('#welcomeTitleOverlay'), 
        $('#welcomePageOverlayTexture')
    ];
    toUnhide.forEach($element => {
        $element.css('transition', 'none');
        void $element[0].offsetHeight;
        $element.removeClass('hide');
        requestAnimationFrame(() => $element.css('transition', ''));
    });
    $('.welcome-text').addClass('hide');
    $('.music-player-panel').addClass('hide').css({
        'transition-duration': '1s',
        'transition-timing-function': 'ease'
    });
    $('#welcomeTitle').addClass('hide');
    $('.welcome-button').addClass('hide');
}

function playWelcomeAnimation() {
    const $welcomeTitleForAnimation = $('#welcomeTitleForAnimation');
    const $originalWelcomeTitle = $('#welcomeTitle');
    const $welcomeTitleOverlay = $('#welcomeTitleOverlay');
    const $welcomePageOverlay = $('#welcomePageOverlay');
    const $welcomePageOverlayTexture = $('#welcomePageOverlayTexture');
    const delay = 500;

    const animation = [
        () => hideWelcomeTitleOverlay($welcomeTitleOverlay),
        () => moveWelcomeTitleForwardAndScale($welcomeTitleForAnimation),
        () => {
            hideWelcomePageOverlay($welcomePageOverlay, $welcomePageOverlayTexture);
            moveWelcomeTitleToOriginalPlace($originalWelcomeTitle, $welcomeTitleForAnimation);
            changeWelcomeTitleColorToOriginal($welcomeTitleForAnimation);
        },
        () => unhideWelcomeTextAndButtons(),
        () => unhideMusicPlayer()
    ]

    animation.forEach((animation, i) => {
        setTimeout(animation, delay * (i + 3));
    })
}

function hideWelcomeTitleOverlay($welcomeTitleOverlay) {
    $welcomeTitleOverlay.css({
        'left': `calc(${$welcomeTitleOverlay.width() / 2}px + 50%)`,
        'transform': 'translate(0, -50%)',
    }).one('transitionend', () => {
        $welcomeTitleOverlay.addClass('hide');
    });
}

function moveWelcomeTitleForwardAndScale($welcomeTitleForAnimation) {
    $welcomeTitleForAnimation.css('transform', 'translate(-50%, -30%) scale(1.3)');
}

function hideWelcomePageOverlay($welcomePageOverlay, $welcomePageOverlayTexture) {
    $welcomePageOverlay.addClass('hide');
    $welcomePageOverlayTexture.addClass('hide');
}

function moveWelcomeTitleToOriginalPlace($originalWelcomeTitle, $welcomeTitleForAnimation) {
    const offset = $originalWelcomeTitle.offset();
    $welcomeTitleForAnimation.css({
        'top': `${offset.top + $originalWelcomeTitle.height() / 2}px`,
        'left': `${offset.left + $originalWelcomeTitle.width() / 2}px`,
        'transform': 'translate(-50%, -50%) scale(1)'
    }).one('transitionend', () => {
        $originalWelcomeTitle.removeClass('hide');
        $welcomeTitleForAnimation.addClass('hide');
    });
}

function changeWelcomeTitleColorToOriginal($welcomeTitleForAnimation) {
    $welcomeTitleForAnimation.removeClass('milk').addClass('dark');
}

function unhideWelcomeTextAndButtons() {
    const welcomeAnimationTransitionStyle = "transform 1s ease, opacity 1s ease";
    const delay = 100;
    const toUnhide = [];
    $('.welcome-button').each((i, el) => toUnhide.push($(el)));
    toUnhide.push($('.welcome-text'));
    toUnhide.forEach(($el, i) => {
        $el.css("transition", welcomeAnimationTransitionStyle);
        setTimeout(() => {
            $el.removeClass('hide').one('transitionend', () => $el.css("transition", ""))
        }, delay * i)
    })
}

function unhideMusicPlayer() {
    const $musicPlayerPanel = $('.music-player-panel');
    $musicPlayerPanel.removeClass('hide').one('transitionend', () => {
        $musicPlayerPanel.css({
            'transition-duration': '',
            'transition-timing-function': ''
        });
    });
}
