using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class DeckCreationEngine
{
    public static Stack<CardData> LastGeneratedDeck { get; set; }

    public static IEnumerator GetDeckFromWeb(GameRules rules, int seed)
    {
        // Unity will mangle the Json somewhat if we don't start it as a PUT and change it to a POST
        UnityWebRequest request = UnityWebRequest.Put($"https://wispymouse.net/HappyGraphs/GenerateDeck/{seed}", JsonConvert.SerializeObject(rules));
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
            Debug.Log(request.downloadHandler.text);
            LastGeneratedDeck = JsonConvert.DeserializeObject<Stack<CardData>>(request.downloadHandler.text);
            // Frustratingly gives us the deck up side down, so flip it around
            LastGeneratedDeck = new Stack<CardData>(LastGeneratedDeck);

            System.Text.StringBuilder deckString = new System.Text.StringBuilder();
            Debug.Log("The deck is:");

            foreach (CardData cards in LastGeneratedDeck.ToList())
            {
                deckString.Append(cards.FaceValue);
                deckString.Append(", ");
            }

            Debug.Log(deckString.ToString().TrimEnd(' ').TrimEnd(','));
        }
    }

    static void PrintBoardState(PlayFieldData activePlayField)
    {
        foreach (Coordinate coordinate in activePlayField.PlayedCards.Keys)
        {
            Debug.Log($"{coordinate} - {activePlayField.PlayedCards[coordinate].FaceValue}");
        }
    }
}
