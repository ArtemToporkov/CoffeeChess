namespace CoffeeChess.Domain.Games.Enums;

public enum GameActionType
{
    SendDrawOffer,
    AcceptDrawOffer,
    DeclineDrawOffer,
    ReceiveDrawOffer,
    GetDrawOfferDeclination,
    Resign
}