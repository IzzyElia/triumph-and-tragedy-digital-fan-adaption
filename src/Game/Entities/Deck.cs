using System;
using System.Collections.Generic;
using System.Linq;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

public class Deck : GameEntity
{
    private Random _random = new();
    public SyncedIntList AllDeckCards;
    public SyncedIntList DiscardPile;

    public Deck()
    {
        AllDeckCards = new SyncedIntList(this, nameof(AllDeckCards));
    }

    public Card TryDrawCard(ICardHolder cardHolder)
    {
        var drawPile = GetCardsInDrawPile().ToArray();
        if (drawPile.Length > 0)
        {
            Card card = drawPile[_random.Next(drawPile.Length)];
            cardHolder.CardsHeld.List.Add(card.ID);
            cardHolder.CommitState();
            return card;
        }
        else return null;
    }

    public void ReshuffleDiscard()
    {
        DiscardPile.List.Clear();
        CommitState();
    }
    
    public void ReturnCardToDrawPile(int cardId)
    {
        if (DiscardPile.List.Contains(cardId))
        {
            DiscardPile.List.Remove(cardId);
            CommitState();
        }
        
        foreach (var cardHolder in GameState.GetEntitiesWithType<ICardHolder>())
        {
            if (cardHolder.CardsHeld.List.Contains(cardId))
            {
                cardHolder.CardsHeld.List.Remove(cardId);
                cardHolder.CommitState();
            }
        }
    }

    public void MoveCardToDiscardPile(int cardId)
    {
        ReturnCardToDrawPile(cardId);
        DiscardPile.List.Add(cardId);
    }
    
    public bool IsCardInDrawPile(int cardId)
    {
        bool cardFoundElsewhere = false;
        
        // Check the discard
        foreach (var discardedId in DiscardPile.List)
        {
            if (discardedId == cardId)
            {
                return false;
            }
        }
        
        // Check other player hands/other entities that might be holding the card
        foreach (var cardHolder in GameState.GetEntitiesWithType<ICardHolder>())
        {
            if (cardHolder.CardsHeld.List.Any(x => x == cardId))
            {
                return false;
            }
        } 
        
        return true;
    }

    public IEnumerable<Card> GetCardsInDrawPile()
    {
        foreach (var cardId in AllDeckCards.List)
        {
            if (IsCardInDrawPile(cardId)) yield return GameState.GetEntity<Card>(cardId);
        }
    }
}

public interface ICardHolder : IGameEntity
{
    public SyncedIntList CardsHeld { get; }

    public IEnumerable<Card> GetHeldCards()
    {
        foreach (var cardId in CardsHeld.List) 
            yield return GameState.GetEntity<Card>(cardId);
    }
}