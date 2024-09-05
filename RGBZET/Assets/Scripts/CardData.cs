using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;


public class CardData : MonoBehaviour
{

  public static List<Card> cardList = new List<Card>();

  void Awake()
  {
    Debug.Log("CardData: Awake called.");
    InitializeCardList();

  }

  void InitializeCardList()
  {
    if (cardList.Count > 0)
    {
      Debug.LogWarning("CardList has already been initialized. Skipping adding cards.");
      return; // ออกก่อนถ้ามีการ์ดอยู่แล้วใน list
    }

    cardList.Add(new Card(0, "R", "Red", "One", "Merk", 1, Resources.Load<Sprite>("st-1")));
    cardList.Add(new Card(1, "R", "Red", "One", "Game", 1, Resources.Load<Sprite>("st-2")));
    cardList.Add(new Card(2, "R", "Red", "One", "Tic",  1, Resources.Load<Sprite>("st-3")));
    cardList.Add(new Card(3, "G", "Red", "One", "Merk", 1, Resources.Load<Sprite>("st-4")));
    cardList.Add(new Card(4, "G", "Red", "One", "Game", 1, Resources.Load<Sprite>("st-5")));
    cardList.Add(new Card(5, "G", "Red", "One", "Tic",  1, Resources.Load<Sprite>("st-6")));
    cardList.Add(new Card(6, "B", "Red", "One", "Merk", 1, Resources.Load<Sprite>("st-7")));
    cardList.Add(new Card(7, "B", "Red", "One", "Game", 1, Resources.Load<Sprite>("st-8")));
    cardList.Add(new Card(8, "B", "Red", "One", "Tic",  1, Resources.Load<Sprite>("st-9")));

    /*   cardList.Add(new Card(9, "R", "Green", "One", "Merk", 1, Resources.Load<Sprite>("st-10")));
       cardList.Add(new Card(10, "R", "Green", "One", "Game", 1, Resources.Load<Sprite>("st-11")));
       cardList.Add(new Card(11, "R", "Green", "One", "Tic", 1, Resources.Load<Sprite>("st-12")));
       cardList.Add(new Card(12, "G", "Green", "One", "Merk", 1, Resources.Load<Sprite>("st-13")));
       cardList.Add(new Card(13, "G", "Green", "One", "Game", 1, Resources.Load<Sprite>("st-14")));
       cardList.Add(new Card(14, "G", "Green", "One", "Tic", 1, Resources.Load<Sprite>("st-15")));
       cardList.Add(new Card(15, "B", "Green", "One", "Merk", 1, Resources.Load<Sprite>("st-16")));
       cardList.Add(new Card(16, "B", "Green", "One", "Game", 1, Resources.Load<Sprite>("st-17")));
       cardList.Add(new Card(17, "B", "Green", "One", "Tic", 1, Resources.Load<Sprite>("st-18")));*/

    /*   cardList.Add(new Card(18, "R", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("st-19")));
       cardList.Add(new Card(19, "R", "Blue", "One", "Game", 1, Resources.Load<Sprite>("st-20")));
       cardList.Add(new Card(20, "R", "Blue", "One", "Tic", 1, Resources.Load<Sprite>("st-21")));
       cardList.Add(new Card(21, "G", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("st-22")));
       cardList.Add(new Card(22, "G", "Blue", "One", "Game", 1, Resources.Load<Sprite>("st-23")));
       cardList.Add(new Card(23, "G", "Blue", "One", "Tic", 1, Resources.Load<Sprite>("st-24")));
       cardList.Add(new Card(24, "B", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("st-25")));
       cardList.Add(new Card(25, "B", "Blue", "One", "Game", 1, Resources.Load<Sprite>("st-26")));
       cardList.Add(new Card(26, "B", "Blue", "One", "Tic", 1, Resources.Load<Sprite>("st-27")));*/

    Debug.Log("CardList Count after adding: " + cardList.Count);
  }



}
