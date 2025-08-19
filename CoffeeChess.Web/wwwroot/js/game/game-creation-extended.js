import { ColorPreference } from './enums/ColorPreference.js';
import { ajaxNavigator } from "../site.js";

const init = async () => {
    $('#whiteColorPreference').val(ColorPreference.White);
    $('#blackColorPreference').val(ColorPreference.Black);
    $('#anyColorPreference').val(ColorPreference.Any);
    $('.time-input').on('change.validateTime', e => validateTime(e.currentTarget));
    $('.rating-input').on('change.validateRating', e => validateRating(e.currentTarget));
    $('#playButton').on('click.play', async e => {
        e.preventDefault();
        const serializedForm = $('#gameCreationExtendedForm').serialize();
        const url = `/Game/CreateGame/?${serializedForm}`;
        await ajaxNavigator.loadContent(url);
    });
};

const destroy = async () => {
    $('.time-input').off('.validateTime');
    $('.rating-input').off('.validateRating');
    $('#playButton').off('.play');
};

function validateTime(input) {
    const min = parseInt(input.min);
    const max = parseInt(input.max);

    if (input.value < min) {
        input.value = min;
    } else if (input.value > max) {
        input.value = max;
    }
}
function validateRating(input) {
    const min = parseInt(input.min);
    const max = parseInt(input.max);
    const minRatingInput = $('#minRating')[0];
    const maxRatingInput = $('#maxRating')[0];

    if (input.value < min) {
        input.value = min;
    } else if (input.value > max) {
        input.value = max;
    }

    const minValue = parseInt(minRatingInput.value) || 0;
    const maxValue = parseInt(maxRatingInput.value) || max;

    if (minValue > maxValue) {
        maxRatingInput.value = minValue;
    }
}

export default { init, destroy };