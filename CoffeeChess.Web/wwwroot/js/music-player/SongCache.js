export class SongCache {
    #dbName;
    #dbVersion;
    #playlistStore;
    #filesStore;

    #db = null;
    #dbPromise = null;

    constructor() {
        this.#dbName = 'CoffeeChessDB';
        this.#dbVersion = 1;
        this.#playlistStore = 'playlist';
        this.#filesStore = 'songFiles';
    }
    #openDb() {
        if (this.#dbPromise) {
            return this.#dbPromise;
        }

        this.#dbPromise = new Promise((resolve, reject) => {
            const request = indexedDB.open(this.#dbName, this.#dbVersion);

            request.onupgradeneeded = event => {
                const dbInstance = event.target.result;
                if (!dbInstance.objectStoreNames.contains(this.#playlistStore)) {
                    dbInstance.createObjectStore(this.#playlistStore, { keyPath: 'songId' });
                }
                if (!dbInstance.objectStoreNames.contains(this.#filesStore)) {
                    dbInstance.createObjectStore(this.#filesStore, { keyPath: 'songId' });
                }
            };

            request.onsuccess = event => {
                this.#db = event.target.result;
                resolve(this.#db);
            };

            request.onerror = event => {
                console.error('IndexedDB error:', event.target.error);
                reject(event.target.error);
            };
        });

        return this.#dbPromise;
    }

    async getPlaylist() {
        const db = await this.#openDb();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(this.#playlistStore, 'readonly');
            const store = transaction.objectStore(this.#playlistStore);
            const request = store.getAll();

            request.onsuccess = () => resolve(request.result || []);
            request.onerror = () => reject(request.error);
        });
    }
    
    async cachePlaylist(playlist) {
        const db = await this.#openDb();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(this.#playlistStore, 'readwrite');
            const store = transaction.objectStore(this.#playlistStore);
            store.clear();
            playlist.forEach(song => store.put(song));

            transaction.oncomplete = () => resolve();
            transaction.onerror = () => reject(transaction.error);
        });
    }

    async getSongFiles(songId) {
        const db = await this.#openDb();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(this.#filesStore, 'readonly');
            const store = transaction.objectStore(this.#filesStore);
            const request = store.get(songId);

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async cacheSongFiles(songId, audioBlob, coverBlob) {
        const db = await this.#openDb();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(this.#filesStore, 'readwrite');
            const store = transaction.objectStore(this.#filesStore);
            store.put({ songId, audio: audioBlob, cover: coverBlob });

            transaction.oncomplete = () => resolve();
            transaction.onerror = () => reject(transaction.error);
        });
    }
}