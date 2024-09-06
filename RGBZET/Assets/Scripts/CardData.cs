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
    //Debug.Log("CardData: Awake called.");
    InitializeCardList();

  }

  void InitializeCardList()
  {
    if (cardList.Count > 0)
    {
      //Debug.LogWarning("CardList has already been initialized. Skipping adding cards.");
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

    cardList.Add(new Card(9,  "R", "Green", "One", "Merk", 1, Resources.Load<Sprite>("st-10")));
    cardList.Add(new Card(10, "R", "Green", "One", "Game", 1, Resources.Load<Sprite>("st-11")));
    cardList.Add(new Card(11, "R", "Green", "One", "Tic",  1, Resources.Load<Sprite>("st-12")));
    cardList.Add(new Card(12, "G", "Green", "One", "Merk", 1, Resources.Load<Sprite>("st-13")));
    cardList.Add(new Card(13, "G", "Green", "One", "Game", 1, Resources.Load<Sprite>("st-14")));
    cardList.Add(new Card(14, "G", "Green", "One", "Tic",  1, Resources.Load<Sprite>("st-15")));
    cardList.Add(new Card(15, "B", "Green", "One", "Merk", 1, Resources.Load<Sprite>("st-16")));
    cardList.Add(new Card(16, "B", "Green", "One", "Game", 1, Resources.Load<Sprite>("st-17")));
    cardList.Add(new Card(17, "B", "Green", "One", "Tic",  1, Resources.Load<Sprite>("st-18")));

/*    cardList.Add(new Card(18, "R", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("st-19")));
    cardList.Add(new Card(19, "R", "Blue", "One", "Game", 1, Resources.Load<Sprite>("st-20")));
    cardList.Add(new Card(20, "R", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("st-21")));
    cardList.Add(new Card(21, "G", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("st-22")));
    cardList.Add(new Card(22, "G", "Blue", "One", "Game", 1, Resources.Load<Sprite>("st-23")));
    cardList.Add(new Card(23, "G", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("st-24")));
    cardList.Add(new Card(24, "B", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("st-25")));
    cardList.Add(new Card(25, "B", "Blue", "One", "Game", 1, Resources.Load<Sprite>("st-26")));
    cardList.Add(new Card(26, "B", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("st-27")));*/

/*    cardList.Add(new Card(27, "RR", "Red", "One", "Merk", 1, Resources.Load<Sprite>("nd-1")));
    cardList.Add(new Card(28, "RR", "Red", "One", "Game", 1, Resources.Load<Sprite>("nd-2")));
    cardList.Add(new Card(29, "RR", "Red", "One", "Tic",  1, Resources.Load<Sprite>("nd-3")));
    cardList.Add(new Card(30, "GG", "Red", "One", "Merk", 1, Resources.Load<Sprite>("nd-4")));
    cardList.Add(new Card(31, "GG", "Red", "One", "Game", 1, Resources.Load<Sprite>("nd-5")));
    cardList.Add(new Card(32, "GG", "Red", "One", "Tic",  1, Resources.Load<Sprite>("nd-6")));
    cardList.Add(new Card(33, "BB", "Red", "One", "Merk", 1, Resources.Load<Sprite>("nd-7")));
    cardList.Add(new Card(34, "BB", "Red", "One", "Game", 1, Resources.Load<Sprite>("nd-8")));
    cardList.Add(new Card(35, "BB", "Red", "One", "Tic",  1, Resources.Load<Sprite>("nd-9")));

    cardList.Add(new Card(36, "RR", "Green", "One", "Merk", 1, Resources.Load<Sprite>("nd-10")));
    cardList.Add(new Card(37, "RR", "Green", "One", "Game", 1, Resources.Load<Sprite>("nd-11")));
    cardList.Add(new Card(38, "RR", "Green", "One", "Tic",  1, Resources.Load<Sprite>("nd-12")));
    cardList.Add(new Card(39, "GG", "Green", "One", "Merk", 1, Resources.Load<Sprite>("nd-13")));
    cardList.Add(new Card(40, "GG", "Green", "One", "Game", 1, Resources.Load<Sprite>("nd-14")));
    cardList.Add(new Card(41, "GG", "Green", "One", "Tic",  1, Resources.Load<Sprite>("nd-15")));
    cardList.Add(new Card(42, "BB", "Green", "One", "Merk", 1, Resources.Load<Sprite>("nd-16")));
    cardList.Add(new Card(43, "BB", "Green", "One", "Game", 1, Resources.Load<Sprite>("nd-17")));
    cardList.Add(new Card(44, "BB", "Green", "One", "Tic",  1, Resources.Load<Sprite>("nd-18")));

    cardList.Add(new Card(45, "RR", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("nd-19")));
    cardList.Add(new Card(46, "RR", "Blue", "One", "Game", 1, Resources.Load<Sprite>("nd-20")));
    cardList.Add(new Card(47, "RR", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("nd-21")));
    cardList.Add(new Card(48, "GG", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("nd-22")));
    cardList.Add(new Card(49, "GG", "Blue", "One", "Game", 1, Resources.Load<Sprite>("nd-23")));
    cardList.Add(new Card(50, "GG", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("nd-24")));
    cardList.Add(new Card(51, "BB", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("nd-25")));
    cardList.Add(new Card(52, "BB", "Blue", "One", "Game", 1, Resources.Load<Sprite>("nd-26")));
    cardList.Add(new Card(53, "BB", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("nd-27")));*/

  /*  cardList.Add(new Card(54, "RRR", "Red", "One", "Merk", 1, Resources.Load<Sprite>("rd-1")));
    cardList.Add(new Card(55, "RRR", "Red", "One", "Game", 1, Resources.Load<Sprite>("rd-2")));
    cardList.Add(new Card(56, "RRR", "Red", "One", "Tic",  1, Resources.Load<Sprite>("rd-3")));
    cardList.Add(new Card(57, "GGG", "Red", "One", "Merk", 1, Resources.Load<Sprite>("rd-4")));
    cardList.Add(new Card(58, "GGG", "Red", "One", "Game", 1, Resources.Load<Sprite>("rd-5")));
    cardList.Add(new Card(59, "GGG", "Red", "One", "Tic",  1, Resources.Load<Sprite>("rd-6")));
    cardList.Add(new Card(60, "BBB", "Red", "One", "Merk", 1, Resources.Load<Sprite>("rd-7")));
    cardList.Add(new Card(61, "BBB", "Red", "One", "Game", 1, Resources.Load<Sprite>("rd-8")));
    cardList.Add(new Card(62, "BBB", "Red", "One", "Tic",  1, Resources.Load<Sprite>("rd-9")));

    cardList.Add(new Card(63, "RRR", "Green", "One", "Merk", 1, Resources.Load<Sprite>("rd-10")));
    cardList.Add(new Card(64, "RRR", "Green", "One", "Game", 1, Resources.Load<Sprite>("rd-11")));
    cardList.Add(new Card(65, "RRR", "Green", "One", "Tic",  1, Resources.Load<Sprite>("rd-12")));
    cardList.Add(new Card(66, "GGG", "Green", "One", "Merk", 1, Resources.Load<Sprite>("rd-13")));
    cardList.Add(new Card(67, "GGG", "Green", "One", "Game", 1, Resources.Load<Sprite>("rd-14")));
    cardList.Add(new Card(68, "GGG", "Green", "One", "Tic",  1, Resources.Load<Sprite>("rd-15")));
    cardList.Add(new Card(69, "BBB", "Green", "One", "Merk", 1, Resources.Load<Sprite>("rd-16")));
    cardList.Add(new Card(70, "BBB", "Green", "One", "Game", 1, Resources.Load<Sprite>("rd-17")));
    cardList.Add(new Card(71, "BBB", "Green", "One", "Tic",  1, Resources.Load<Sprite>("rd-18")));

    cardList.Add(new Card(72, "RRR", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("rd-19")));
    cardList.Add(new Card(73, "RRR", "Blue", "One", "Game", 1, Resources.Load<Sprite>("rd-20")));
    cardList.Add(new Card(74, "RRR", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("rd-21")));
    cardList.Add(new Card(75, "GGG", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("rd-22")));
    cardList.Add(new Card(76, "GGG", "Blue", "One", "Game", 1, Resources.Load<Sprite>("rd-23")));
    cardList.Add(new Card(77, "GGG", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("rd-24")));
    cardList.Add(new Card(78, "BBB", "Blue", "One", "Merk", 1, Resources.Load<Sprite>("rd-25")));
    cardList.Add(new Card(79, "BBB", "Blue", "One", "Game", 1, Resources.Load<Sprite>("rd-26")));
    cardList.Add(new Card(80, "BBB", "Blue", "One", "Tic",  1, Resources.Load<Sprite>("rd-27")));*/


    Debug.Log("CardList Count after adding: " + cardList.Count);
  }



}
