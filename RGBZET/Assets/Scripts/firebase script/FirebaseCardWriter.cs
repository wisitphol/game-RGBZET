using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;

public class FirebaseCardWriter : MonoBehaviour
{
    DatabaseReference reference;

    void Start()
    {
        // เชื่อมต่อ Firebase Realtime Database
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Exception);
                return;
            }

            // หากเชื่อมต่อสำเร็จ จะได้รับอินสแตนซ์ FirebaseApp และเปิดใช้งาน Realtime Database
            FirebaseApp app = FirebaseApp.DefaultInstance;
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            Debug.Log("Firebase Realtime Database connected successfully!");
            
            // ตรวจสอบความยาวของข้อมูลการ์ด
            if (CardData.cardList.Count > 0)
            {
                StartCoroutine(WriteCardsToFirebaseCoroutine());
            }
            else
            {
                Debug.LogError("Card data is empty. No data to write.");
            }
        });
    }

    IEnumerator WriteCardsToFirebaseCoroutine()
    {
        // รอจนกว่าเฟรมถัดไปจะเริ่ม
        yield return new WaitForEndOfFrame();

        // สร้างข้อมูลการ์ดและบันทึกลงใน Realtime Database
        foreach (Card card in CardData.cardList)
        {
            string cardJson = JsonUtility.ToJson(card); // แปลงข้อมูลการ์ดเป็น JSON
            reference.Child("cards").Child(card.Id.ToString()).SetRawJsonValueAsync(cardJson)
                .ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        Debug.LogError("Failed to write card data to Firebase: " + task.Exception);
                        return;
                    }

                    Debug.Log("Card data was written to Firebase successfully!");
                });

            // รอจนกว่าข้อมูลการ์ดจะถูกเขียนลงใน Firebase Realtime Database เสร็จ
            yield return new WaitForSeconds(0.1f);
        }
    }



}
