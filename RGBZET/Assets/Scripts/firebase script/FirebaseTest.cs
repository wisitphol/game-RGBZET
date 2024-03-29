using UnityEngine;
using Firebase;
using Firebase.Database;

public class FirebaseTest : MonoBehaviour
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

            // ทดสอบการเขียนข้อมูลใหม่ไปยังฐานข้อมูล
            WriteDataToFirebase();
        });
    }

    void WriteDataToFirebase()
    {
        // สร้างข้อมูลที่จะเขียนลงในฐานข้อมูล
        string data = "Hello, Firebase!";

        // เขียนข้อมูลลงในฐานข้อมูลที่เราต้องการ โดยการกำหนดค่าที่ใน Child หรือโหนดของฐานข้อมูล
        reference.Child("testData").SetValueAsync(data).ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to write data to Firebase: " + task.Exception);
                return;
            }

            Debug.Log("Data was written successfully!");
        });
    }
}
