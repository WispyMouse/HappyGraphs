using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class DeckCreationEngine
{
    public static Deck LastGeneratedDeck { get; set; }

    public static IEnumerator GetDeckFromWeb(GameRules rules, int seed)
    {
        // Unity will mangle the Json somewhat if we don't start it as a PUT and change it to a POST
        UnityWebRequest request = UnityWebRequest.Put($"https://wispymouse.net/HappyGraphs/GenerateDeckPile/{seed}", JsonConvert.SerializeObject(rules));
        request.method = "POST";
        request.uploadHandler.contentType = "application/json";
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            try
            {
                CardData[] deckPile = JsonConvert.DeserializeObject<CardData[]>(request.downloadHandler.text);
                Deck generatedDeck = new Deck();
                generatedDeck.DeckStack = new Stack<CardData>(new Stack<CardData>(deckPile));
                LastGeneratedDeck = generatedDeck;
            }
            catch (System.Exception e)
            {
                Debug.Log("Deck failure.");
                Debug.Log(e);
            }
        }
    }

    static void PrintBoardState(PlayFieldData activePlayField)
    {
        foreach (Coordinate coordinate in activePlayField.PlayedCards.Keys)
        {
            Debug.Log($"{coordinate} - {activePlayField.PlayedCards[coordinate].FaceValue}");
        }
    }

    public class ForceDeck
    {
        public CardData[] DeckStack;

        public ForceDeck()
        {

        }

        public Deck ConvertDeck()
        {
            Deck newDeck = new Deck();
            newDeck.DeckStack = new Stack<CardData>(new Stack<CardData>(DeckStack));
            return newDeck;
        }
    }
}
