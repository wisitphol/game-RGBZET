using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : MonoBehaviour
{
    
  public static List<Card> cardList = new List<Card>();
  

  void Awake()
  {
      cardList.Add(new Card(0 ,"R" ,"Red"   ,"Normal" ,"Thru" ,Resources.Load<Sprite>("n1") ));
      cardList.Add(new Card(1 ,"R" ,"Red"   ,"Normal" ,"Thin" ,Resources.Load<Sprite>("n2") ));
      cardList.Add(new Card(2 ,"R" ,"Red"   ,"Normal" ,"Thick",Resources.Load<Sprite>("n3") ));
      cardList.Add(new Card(3 ,"R" ,"Green" ,"Normal" ,"Thru" ,Resources.Load<Sprite>("n4") ));
      cardList.Add(new Card(4 ,"R" ,"Green" ,"Normal" ,"Thin" ,Resources.Load<Sprite>("n5") ));
      cardList.Add(new Card(5 ,"R" ,"Green" ,"Normal" ,"Thick",Resources.Load<Sprite>("n6") ));
    

  }
    


}
