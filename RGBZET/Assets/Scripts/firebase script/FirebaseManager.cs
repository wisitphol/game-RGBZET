using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using UnityEngine;


public class FirebaseManager : MonoBehaviour
{
    DatabaseReference reference;

    void Start()
    {
        // Initialize Firebase SDK
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Exception);
                return;
            }

            // Get the root reference location of the database
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            // Set Firebase Realtime Database URL
             FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(true);
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            // Read data from Firebase
            ReadDataFromFirebase();
        });
    }

    void ReadDataFromFirebase()
    {
        // Listen for changes in the data at this location
        reference.Child("your_child_node").ValueChanged += HandleValueChanged;
    }

    void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        // Data snapshot containing the new data
        DataSnapshot snapshot = args.Snapshot;

        // Access data from the snapshot
        // For example, to access a string value:
        string value = snapshot.Child("your_key").GetValue(true).ToString();
        Debug.Log("Value: " + value);
    }
}