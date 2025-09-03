import { animateSearching } from "./ui.js";

const init = async () => {
    await document.fonts.ready;
    animateSearching();
    
    const challengeSettingsParams = new URLSearchParams(window.location.search);
    const keys = ['minutes', 'increment', 'colorPreference', 'minRating', 'maxRating'];

    const challengeSettings = keys.reduce((obj, key) => {
        const value = parseInt(challengeSettingsParams.get(key));
        if (!isNaN(value)) 
            obj[key] = value;
        return obj;
    }, {});

    await $.ajax({
        url: `/Game/GameCreation/QueueOrFindChallenge`,
        method: 'POST',
        data: JSON.stringify(challengeSettings),
        contentType: 'application/json',
    });
};

const destroy = async () => {
};

export default { init, destroy };

