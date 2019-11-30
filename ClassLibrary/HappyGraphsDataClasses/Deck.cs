using System.Collections.Generic;
using System.Linq;

public class Deck
{
    public Stack<CardData> DeckStack = new Stack<CardData>();

    public Deck()
    {

    }

    public Deck(Deck copiedDeck)
    {
        DeckStack = new Stack<CardData>(new Stack<CardData>(copiedDeck.DeckStack));
    }

    public Deck(Queue<CardData> cardsInDeck)
    {
        foreach (CardData curCard in cardsInDeck)
        {
            PushCard(curCard);
        }
    }

    public void PushCard(CardData card)
    {
        DeckStack.Push(card);
    }

    public List<CardData> GetAllCards()
    {
        return DeckStack.ToList();
    }

    public CardData PopCard()
    {
        return DeckStack.Pop();
    }

    public int DeckSize
    {
        get
        {
            return DeckStack.Count;
        }
    }
}
