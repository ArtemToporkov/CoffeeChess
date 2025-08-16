import { AjaxNavigator } from "./AjaxNavigator.js";

if (!window._ajaxNavigator)
    window._ajaxNavigator = new AjaxNavigator();

export const ajaxNavigator = window._ajaxNavigator;