using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class Drop2 : MonoBehaviourPunCallbacks, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    //public TMP_Text scoreText; // อ้างอิงไปยัง Text UI สำหรับแสดงคะแนน

    private List<Card> droppedCards = new List<Card>(); // เก็บคุณสมบัติของการ์ดที่ลากมาวางลงในพื้นที่ drop
    private Transform parentToReturnTo;
    private Deck2 deck;
    private int currentScore;
    private MutiManage2 muti2;
    public GameObject iszet;
    public GameObject isnotzet;


    public void Start()
    {
        deck = FindObjectOfType<Deck2>(); // หรือใช้วิธีการค้นหาที่สอดคล้องกับโครงสร้างของโปรเจคของคุณ
        currentScore = 0; // เพิ่มบรรทัดนี้เพื่อกำหนดค่าเริ่มต้นของ currentScore
        muti2 = FindAnyObjectByType<MutiManage2>();

        iszet.SetActive(false);
        isnotzet.SetActive(false);
    }

    public void Update()
    {
        // เมื่อปุ่ม ZET ไม่ได้ถูกกด
        if (!MutiManage2.isZETActive)
        {
            // ตรวจสอบจำนวนการ์ดใน Checkzone
            int numberOfCardsInCheckZone = transform.childCount;

            // ถ้ามีการ์ดใน Checkzone และไม่ได้ถูกกด ZET
            if (numberOfCardsInCheckZone > 0 && numberOfCardsInCheckZone < 3)
            {
                // วนลูปเพื่อนำการ์ดทั้งหมดใน Checkzone กลับไปยัง Boardzone
                for (int i = numberOfCardsInCheckZone - 1; i >= 0; i--) // เริ่มจากตัวสุดท้าย
                {
                    Transform cardTransform = transform.GetChild(i);
                    if (cardTransform != null)
                    {
                        cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                        cardTransform.localPosition = Vector3.zero;

                        DisplayCard2 displayCardComponent = cardTransform.GetComponent<DisplayCard2>();
                        if (displayCardComponent != null)
                        {
                            displayCardComponent.SetBlocksRaycasts(true);
                        }
                    }
                }

                // ล้างรายการการ์ดใน Checkzone
                droppedCards.Clear();

                Debug.Log("Returned cards to Boardzone because ZET button was not pressed.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // เมื่อมี pointer เข้ามาในระยะ
        if (eventData.pointerDrag == null)
            return;

    }

    public void OnDrop(PointerEventData eventData)
    {
        // ตรวจสอบก่อนว่าปุ่ม ZET ถูกกดแล้วหรือยัง
        if (!MutiManage2.isZETActive)
        {
            //Debug.Log("Cannot drop. ZET button has not been pressed.");
            return; // ยกเลิกการ drop ถ้าปุ่ม ZET ยังไม่ถูกกด
        }

        parentToReturnTo = transform.parent;

        int numberOfCardsOnDropZone = CheckNumberOfCardsOnDropZone();

        if (numberOfCardsOnDropZone == 3)
        {
            Debug.Log("Cannot drop. Maximum number of cards reached.");
            return;
        }

        // ตรวจสอบว่ามี object ที่ลากมาวางลงหรือไม่
        if (eventData.pointerDrag != null)
        {
            // เรียกใช้สคริปต์ Drag ของ object ที่ลากมา
            Drag2 draggable = eventData.pointerDrag.GetComponent<Drag2>();
            if (draggable != null)
            {
                // กำหนด parent ใหม่ให้กับ object ที่ลากมา เพื่อให้วางลงใน panel นี้
                draggable.parentToReturnTo = this.transform;
                photonView.RPC("UpdateCardParent", RpcTarget.AllBuffered, draggable.GetComponent<PhotonView>().ViewID, photonView.ViewID);
            }

            DisplayCard2 displayCardComponent = eventData.pointerDrag.GetComponent<DisplayCard2>();
            if (displayCardComponent != null)
            {

                droppedCards.Add(displayCardComponent.displayCard[0]);
            }
        }
        StartCoroutine(CheckSetWithDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // เมื่อ pointer ออกจากระยะ
        if (eventData.pointerDrag == null)
        {
            return;
        }
    }

    public int CheckNumberOfCardsOnDropZone()
    {
        int numberOfCards = this.transform.childCount;
        return numberOfCards;
    }

    public void CheckSetConditions()
    {
        // เรียกเมธอดหรือทำงานเพิ่มเติมที่ต้องการเมื่อมีการ์ดทั้ง 3 ใบในพื้นที่ drop
        // ตรวจสอบว่าการ์ดทั้งสามใบตรงเงื่อนไขหรือไม่ ตามกฎของเกม Set
        //Debug.Log("number of cards: " + droppedCards.Count);
        if (droppedCards.Count == 3)
        {

            bool isSet = CheckCardsAreSet(droppedCards[0], droppedCards[1], droppedCards[2]);

            if (isSet)
            {
                iszet.SetActive(true);
                // คำนวณคะแนนรวมของการ์ด 3 ใบ
                int TotalScore = CalculateTotalScore(droppedCards[0], droppedCards[1], droppedCards[2]);

                UpdateScore(TotalScore);

                //scoreText.text =  totalScore.ToString();
                RemoveCardsFromGameRPC();
                // เรียกใช้งาน RPC เพื่อให้ MasterClient ดึงการ์ด
                if (PhotonNetwork.IsMasterClient)
                {

                    deck.DrawCards(3);
                }
                else
                {
                    // photonView.RPC("RemoveCardByMasterClient", RpcTarget.MasterClient);
                    deck.photonView.RPC("RequestDrawCardsFromMaster", RpcTarget.MasterClient, 3);
                }

            }
            else
            {
                isnotzet.SetActive(true);
                //Debug.Log("Not a valid Set. Return the cards to their original position.");
                // นำการ์ดที่ไม่ตรงเงื่อนไขกลับไปที่ตำแหน่งเดิม

                photonView.RPC("ReturnCardsToOriginalPositionRPC", RpcTarget.AllBuffered);

                UpdateScore(-1);

            }
             StartCoroutine(HideSetIndicators());
        }
        else
        {
            //Debug.Log("Unexpected number of cards: " + droppedCards.Count); // Debug Log เมื่อมีจำนวนการ์ดไม่ถูกต้อง
        }

    }

    private IEnumerator HideSetIndicators()
    {
        yield return new WaitForSeconds(2f); // ปรับเวลาได้ตามต้องการ
        iszet.SetActive(false);
        isnotzet.SetActive(false);
    }

    public bool CheckCardsAreSet(Card card1, Card card2, Card card3)
    {
        // ตรวจสอบคุณสมบัติของการ์ดทั้งสามใบ
        bool letterSet = ArePropertiesEqual(card1.LetterType, card2.LetterType, card3.LetterType);
        bool colorSet = ArePropertiesEqual(card1.ColorType, card2.ColorType, card3.ColorType);
        bool amountSet = ArePropertiesEqual(card1.AmountType, card2.AmountType, card3.AmountType);
        bool fontSet = ArePropertiesEqual(card1.FontType, card2.FontType, card3.FontType);

        // ตรวจสอบว่ามีการ์ดทั้งสามใบมีคุณสมบัติเหมือนหรือต่างกันทั้งหมดหรือไม่
        return (letterSet && colorSet && amountSet && fontSet);
    }

    // ตรวจสอบว่าคุณสมบัติของการ์ดทั้งสามใบเหมือนหรือต่างกันทั้งหมดหรือไม่
    public bool ArePropertiesEqual(string property1, string property2, string property3)
    {
        return (property1 == property2 && property2 == property3) || (property1 != property2 && property2 != property3 && property1 != property3);
    }

    public int CalculateTotalScore(Card card1, Card card2, Card card3)
    {
        // คำนวณคะแนนรวมของการ์ดทั้ง 3 ใบ
        int totalScore = card1.Point + card2.Point + card3.Point;
        return totalScore;
    }

    public IEnumerator CheckSetWithDelay()
    {
        // หน่วงเวลา 3 วินาที
        yield return new WaitForSeconds(4f);

        // ตรวจสอบเงื่อนไขการ์ดหลังจากหน่วงเวลา 3 วินาที
        CheckSetConditions();
    }

    public void UpdateScore(int addScore)
    {
        // อัปเดตคะแนน
        currentScore += addScore;

        // ตรวจสอบไม่ให้คะแนนต่ำกว่า 0
        if (currentScore < 0)
        {
            currentScore = 0;
        }

        // รับ actorNumber จาก PlayerCon2 หรือใช้วิธีอื่นเพื่อดึง actorNumber
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber; // หรือดึงจาก component ที่เกี่ยวข้อง

        // เรียก RPC เพื่ออัปเดตคะแนน
        muti2.photonView.RPC("UpdatePlayerScore", RpcTarget.All, actorNumber, currentScore);

    }

    public void CheckAndReturnCardsToBoardZone()
    {
        // ตรวจสอบว่าปุ่ม ZET ไม่ได้ถูกกด
        if (!MutiManage2.isZETActive)
        {
            // ตรวจสอบจำนวนการ์ดใน Checkzone
            int numberOfCardsInCheckZone = transform.childCount;

            // ถ้ามีการ์ดใน Checkzone
            if (numberOfCardsInCheckZone > 0 && numberOfCardsInCheckZone < 3)
            {
                // วนลูปเพื่อนำการ์ดทั้งหมดใน Checkzone กลับไปยัง Boardzone
                for (int i = numberOfCardsInCheckZone - 1; i >= 0; i--) // เริ่มจากตัวสุดท้าย
                {
                    Transform cardTransform = transform.GetChild(i);
                    if (cardTransform != null)
                    {
                        cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                        cardTransform.localPosition = Vector3.zero;

                        DisplayCard2 displayCardComponent = cardTransform.GetComponent<DisplayCard2>();
                        if (displayCardComponent != null)
                        {
                            displayCardComponent.SetBlocksRaycasts(true);
                        }
                    }
                }

                // ล้างรายการการ์ดใน Checkzone
                droppedCards.Clear();

                Debug.Log("Returned cards to Boardzone because ZET button was not pressed.");
            }
        }
    }

    [PunRPC]
    public void ReturnCardsToOriginalPositionRPC()
    {
        int numberOfCards = transform.childCount;

        for (int i = numberOfCards - 1; i >= 0; i--)
        {
            Transform cardTransform = transform.GetChild(i);
            if (cardTransform != null)
            {
                cardTransform.SetParent(GameObject.Find("Boardzone").transform);
                cardTransform.localPosition = Vector3.zero;

                DisplayCard2 displayCardComponent = cardTransform.GetComponent<DisplayCard2>();
                if (displayCardComponent != null)
                {
                    displayCardComponent.SetBlocksRaycasts(true);
                }

                Card card = displayCardComponent.displayCard[0];
                if (droppedCards.Contains(card))
                {
                    droppedCards.Remove(card);
                }
            }
        }

        photonView.RPC("ClearDroppedCards", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RemoveCardsFromGameRPC()
    {
        int numberOfCards = transform.childCount;

        for (int i = numberOfCards - 1; i >= 0; i--)
        {
            Transform cardTransform = transform.GetChild(i);
            if (cardTransform != null)
            {
                DisplayCard2 displayCard = cardTransform.GetComponent<DisplayCard2>();
                if (displayCard != null && droppedCards.Contains(displayCard.displayCard[0]))
                {
                    if (PhotonNetwork.IsMasterClient || photonView.Owner == PhotonNetwork.LocalPlayer)
                    {
                        PhotonNetwork.Destroy(cardTransform.gameObject); // ให้ MasterClient หรือเจ้าของการ์ดเป็นคนลบ
                        droppedCards.Remove(displayCard.displayCard[0]);
                    }
                    else
                    {
                        photonView.RPC("RemoveCardByMasterClient", RpcTarget.MasterClient, cardTransform.gameObject.GetPhotonView().ViewID);
                    }
                }
            }
        }

        photonView.RPC("ClearDroppedCards", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RemoveCardByMasterClient(int viewID)
    {
        // Debug.Log("RemoveCardByMasterClient RPC called");
        // หา PhotonView จาก viewID ที่ได้รับมา
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView targetView = PhotonView.Find(viewID);
            if (targetView != null)
            {
                PhotonNetwork.Destroy(targetView.gameObject);
            }
        }
    }

    [PunRPC]
    public void ClearDroppedCards()
    {
        droppedCards.Clear();
    }

    [PunRPC]
    public void UpdateCardParent(int cardViewID, int parentViewID)
    {
        PhotonView cardPhotonView = PhotonView.Find(cardViewID);
        PhotonView parentPhotonView = PhotonView.Find(parentViewID);

        if (cardPhotonView != null && parentPhotonView != null)
        {
            cardPhotonView.transform.SetParent(parentPhotonView.transform);
            cardPhotonView.transform.localPosition = Vector3.zero;
        }
    }



}
