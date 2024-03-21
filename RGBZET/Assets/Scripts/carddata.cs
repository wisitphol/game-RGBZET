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
      cardList.Add(new Card(0  ,"R" ,"Red"   ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n1") ));
      cardList.Add(new Card(1  ,"R" ,"Red"   ,"One" ,"Game",1 ,Resources.Load<Sprite>("n2") ));
      cardList.Add(new Card(2  ,"R" ,"Red"   ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n3") ));
      cardList.Add(new Card(3  ,"R" ,"Green" ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n4") ));
      cardList.Add(new Card(4  ,"R" ,"Green" ,"One" ,"Game",1 ,Resources.Load<Sprite>("n5") ));
      cardList.Add(new Card(5  ,"R" ,"Green" ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n6") ));
      cardList.Add(new Card(6  ,"R" ,"Blue"  ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n7") ));
      cardList.Add(new Card(7  ,"R" ,"Blue"  ,"One" ,"Game",1 ,Resources.Load<Sprite>("n8") ));
      cardList.Add(new Card(8  ,"R" ,"Blue"  ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n9") ));
      cardList.Add(new Card(9  ,"G" ,"Red"   ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n10") ));
      cardList.Add(new Card(10 ,"G" ,"Red"   ,"One" ,"Game",1 ,Resources.Load<Sprite>("n11") ));
      cardList.Add(new Card(11 ,"G" ,"Red"   ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n12") ));
      cardList.Add(new Card(12 ,"G" ,"Green" ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n13") ));
      cardList.Add(new Card(13 ,"G" ,"Green" ,"One" ,"Game",1 ,Resources.Load<Sprite>("n14") ));
      cardList.Add(new Card(14 ,"G" ,"Green" ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n15") ));
      cardList.Add(new Card(15 ,"G" ,"Blue"  ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n16") ));
      cardList.Add(new Card(16 ,"G" ,"Blue"  ,"One" ,"Game",1 ,Resources.Load<Sprite>("n17") ));
      cardList.Add(new Card(17 ,"G" ,"Blue"  ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n18") ));
      cardList.Add(new Card(18 ,"B" ,"Red"   ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n19") ));
      cardList.Add(new Card(19 ,"B" ,"Red"   ,"One" ,"Game",1 ,Resources.Load<Sprite>("n20") ));
      cardList.Add(new Card(20 ,"B" ,"Red"   ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n21") ));
      cardList.Add(new Card(21 ,"B" ,"Green" ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n22") ));
      cardList.Add(new Card(22 ,"B" ,"Green" ,"One" ,"Game",1 ,Resources.Load<Sprite>("n23") ));
      cardList.Add(new Card(23 ,"B" ,"Green" ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n24") ));
      cardList.Add(new Card(24 ,"B" ,"Blue"  ,"One" ,"Merk",1 ,Resources.Load<Sprite>("n25") ));
      cardList.Add(new Card(25 ,"B" ,"Blue"  ,"One" ,"Game",1 ,Resources.Load<Sprite>("n26") ));
      cardList.Add(new Card(26 ,"B" ,"Blue"  ,"One" ,"Tic" ,1 ,Resources.Load<Sprite>("n27") ));
    
    //Debug.Log("CardList Count after adding: " + cardList.Count);

    
  }
    


}
