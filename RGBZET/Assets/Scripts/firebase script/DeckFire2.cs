using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using TMPro;

public class DeckFire2 : MonoBehaviour
{
    public List<Card> container = new List<Card>();
    public List<Card> deck = new List<Card>();
    public int x;
    public static int deckSize;
    public static List<Card> staticDeck = new List<Card>();

    public GameObject CardInDeck;

    public GameObject CardPrefab;
    public GameObject[] Clones;
    public GameObject Board;
    private BoardCheck3 boardCheckScript;

    DatabaseReference reference;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(InitializeFirebase());

        deck = new List<Card>(CardData.cardList);
        deckSize = deck.Count;
        Shuffle(deck);

        // Now deck is shuffled and ready to use
        StartCoroutine(StartGame());

        boardCheckScript = FindObjectOfType<BoardCheck3>();

    }
    public IEnumerator InitializeFirebase()
    {
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("Failed to initialize Firebase: " + task.Exception);
            yield break;
        }

        FirebaseApp app = FirebaseApp.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("Firebase Realtime Database connected successfully!");

        deck = new List<Card>(CardData.cardList);
        deckSize = deck.Count;
        Shuffle(deck);

        StartCoroutine(StartGame());

        boardCheckScript = FindObjectOfType<BoardCheck3>();



        SaveDeckToDatabase(deck);
    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;

        if (deckSize <= 0)
        {
            CardInDeck.SetActive(false);
        }
    }

    IEnumerator StartGame()
    {
        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(0.5f);
            GameObject newCard = Instantiate(CardPrefab, transform.position, transform.rotation);
            newCard.transform.SetParent(Board.transform, false);
            newCard.SetActive(true);
        }
    }

    private void Shuffle(List<Card> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Card temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public IEnumerator Draw(int x)
    {
        for (int i = 0; i < x; i++)
        {
            yield return new WaitForSeconds(1);

            if (deckSize > 0)
            {
                GameObject newCard = Instantiate(CardPrefab, transform.position, transform.rotation);
                newCard.transform.SetParent(Board.transform, false);
                newCard.SetActive(true);

                Debug.Log("Number of cards in deck: " + RemainingCardsCount());
            }
            else
            {
                break;
            }
        }

        if (deckSize <= 0)
        {
            boardCheckScript.CheckBoardEnd();
        }
    }

    public void DrawCards(int numberOfCards)
    {
        StartCoroutine(Draw(numberOfCards));
    }

    public int RemainingCardsCount()
    {
        return deckSize;
    }

    public void SaveDeckToDatabase(List<Card> deck)
    {
        if (reference == null)
        {
            Debug.LogError("Firebase Realtime Database reference is not set!");
            return;
        }

        reference.Child("decks").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve deck count: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            int deckCount = (int)snapshot.ChildrenCount;

            Debug.Log("Current deck count: " + deckCount);

            string deckName = "deck" + (deckCount + 1);

            Debug.Log("New deck name: " + deckName);

            for (int i = 0; i < deck.Count; i++)
            {
                int cardIndex = i;  // Use a local variable to capture the index

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    string json = JsonUtility.ToJson(deck[cardIndex]);

                    var dbTask = reference.Child("decks").Child(deckName).Child(cardIndex.ToString()).SetRawJsonValueAsync(json);

                    dbTask.ContinueWith(innerTask =>
                    {
                        if (innerTask.IsFaulted || innerTask.IsCanceled)
                        {
                            Debug.LogError("SaveDeckToDatabase failed: " + innerTask.Exception);
                        }
                        else
                        {
                            // Debug.Log("Card saved successfully: " + deck[cardIndex].Id);
                        }
                    });
                });
            }
        });
    }


    public void DeleteDeckFromDatabase()
    {
        DatabaseReference deckRef = reference.Child("decks/deck1");

        deckRef.RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to delete deck data: " + task.Exception);
            }
            else
            {
                Debug.Log("Deck data deleted successfully");
            }
        });
    }
}
