using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardData : MonoBehaviour
{
    
  public static List<Card> cardList = new List<Card>();
  

  void Awake()
  {
      cardList.Add(new Card(0  ,"R" ,"Red"   ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o1") ));
      cardList.Add(new Card(1  ,"R" ,"Red"   ,"One" ,"Game",1 ,Resources.Load<Sprite>("o2") ));
      cardList.Add(new Card(2  ,"R" ,"Red"   ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o3") ));
      cardList.Add(new Card(3  ,"G" ,"Red"   ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o4") ));
      cardList.Add(new Card(4  ,"G" ,"Red"   ,"One" ,"Game",1 ,Resources.Load<Sprite>("o5") ));
      cardList.Add(new Card(5  ,"G" ,"Red"   ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o6") ));
      cardList.Add(new Card(6  ,"B" ,"Red"   ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o7") ));
      cardList.Add(new Card(7  ,"B" ,"Red"   ,"One" ,"Game",1 ,Resources.Load<Sprite>("o8") ));
      cardList.Add(new Card(8  ,"B" ,"Red"   ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o9") ));

      cardList.Add(new Card(9  ,"R" ,"Green" ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o10") ));
      cardList.Add(new Card(10 ,"R" ,"Green" ,"One" ,"Game",1 ,Resources.Load<Sprite>("o11") ));
      cardList.Add(new Card(11 ,"R" ,"Green" ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o12") ));
      cardList.Add(new Card(12 ,"G" ,"Green" ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o13") ));
      cardList.Add(new Card(13 ,"G" ,"Green" ,"One" ,"Game",1 ,Resources.Load<Sprite>("o14") ));
      cardList.Add(new Card(14 ,"G" ,"Green" ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o15") ));
      cardList.Add(new Card(15 ,"B" ,"Green" ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o16") ));
      cardList.Add(new Card(16 ,"B" ,"Green" ,"One" ,"Game",1 ,Resources.Load<Sprite>("o17") ));
      cardList.Add(new Card(17 ,"B" ,"Green" ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o18") ));

      cardList.Add(new Card(18 ,"R" ,"Blue"  ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o19") ));
      cardList.Add(new Card(19 ,"R" ,"Blue"  ,"One" ,"Game",1 ,Resources.Load<Sprite>("o20") ));
      cardList.Add(new Card(20 ,"R" ,"Blue"  ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o21") ));
      cardList.Add(new Card(21 ,"G" ,"Blue"  ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o22") ));
      cardList.Add(new Card(22 ,"G" ,"Blue"  ,"One" ,"Game",1 ,Resources.Load<Sprite>("o23") ));
      cardList.Add(new Card(23 ,"G" ,"Blue"  ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o24") ));
      cardList.Add(new Card(24 ,"B" ,"Blue"  ,"One" ,"Merk",1 ,Resources.Load<Sprite>("o25") ));
      cardList.Add(new Card(25 ,"B" ,"Blue"  ,"One" ,"Game",1 ,Resources.Load<Sprite>("o26") ));
      cardList.Add(new Card(26 ,"B" ,"Blue"  ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("o27") ));
    
    Debug.Log("CardList Count after adding: " + cardList.Count);

    
  }
    


}
