import { GameHubEvents } from "../enums/GameHubEvents.js";
import { GameHubMethods } from "../enums/GameHubMethods.js";

export class ChatManager {
    #connection;
    #gameId;
    #chatInput;
    #chatMessages;
    eventNamespace;
    
    constructor(connection, gameId) {
        this.#connection = connection;
        this.#gameId = gameId;
        this.#chatInput = $('#chatInput');
        this.#chatMessages = $('#chatMessages');
        this.eventNamespace = `.chatInput_${gameId}`;

        this.#chatInput.on(`keypress${this.eventNamespace}`, e => {
            if (e.key === 'Enter') {
                e.preventDefault();
                const messageText = this.#chatInput.val().trim();
                if (messageText) {
                    this.#connection.invoke(GameHubMethods.SendChatMessage, this.#gameId, messageText);
                    this.#chatInput.val('');
                }
            }
        });
    }
    
    async destroy() {
        this.#chatInput.off(this.eventNamespace);
    }

    addMessageToChat(sender, text) {
        const messageDiv = $('<div>', {
            class: 'chat-message',
        }).append(
            $('<span>', {
                class: 'chat-message-sender',
                text: `${sender}:`
            }),
            $('<span>', {
                class: 'chat-message-text',
                text: ` ${text}`
            })
        )

        this.#chatMessages.append(messageDiv);
        this.#chatMessages.scrollTop(this.#chatMessages[0].scrollHeight);
    }
}