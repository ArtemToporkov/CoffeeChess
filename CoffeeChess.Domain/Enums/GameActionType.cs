namespace CoffeeChess.Domain.Enums;

public enum GameActionType
{
    SendDrawOffer,
    AcceptDrawOffer,
    DeclineDrawOffer,
    ReceiveDrawOffer,
    GetDrawOfferDeclination,
    Resign
}