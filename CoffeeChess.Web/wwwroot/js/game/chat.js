document.addEventListener('DOMContentLoaded', () => {
    function addMessageToChat(sender, text) {
        const messageDiv = document.createElement('div');
        messageDiv.classList.add('chat-message');

        const senderSpan = document.createElement('span');
        senderSpan.classList.add('chat-message-sender');
        senderSpan.textContent = sender + ':';

        const textSpan = document.createElement('span');
        textSpan.classList.add('chat-message-text');
        textSpan.textContent = ' ' + text;

        messageDiv.appendChild(senderSpan);
        messageDiv.appendChild(textSpan);

        chatMessagesContainerElement.appendChild(messageDiv);
        chatMessagesContainerElement.scrollTop = chatMessagesContainerElement.scrollHeight;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    const pathParts = window.location.pathname.split('/');
    const gameId = pathParts[pathParts.length - 1];

    connection.on("ReceiveChatMessage", (user, message) => {
        addMessageToChat(user, message);
    });

    async function startGamePageSignalR() {
        try {
            await connection.start();
        } catch (err) {
            console.error(err.toString());
        }
    }

    startGamePageSignalR();
    
    const chatInputElement = document.getElementById('chatInput');
    const chatMessagesContainerElement = document.getElementById('chatMessages');

    const currentUserDataElement = document.getElementById('currentUserData');
    const currentUsername = currentUserDataElement ? currentUserDataElement.dataset.username : 'You';

    if (chatInputElement && chatMessagesContainerElement) {
        chatInputElement.addEventListener('keypress', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                const messageText = chatInputElement.value.trim();
                if (messageText) {
                    connection.invoke("SendChatMessage", gameId, messageText);
                    chatInputElement.value = '';
                }
            }
        });
    }
});