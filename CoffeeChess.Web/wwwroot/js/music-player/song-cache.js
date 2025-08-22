import { SongCache } from "./SongCache.js";

if (!window._songCache)
    window._songCache = new SongCache();

export const songCache = window._songCache;