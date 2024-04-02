using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.SceneManagement;

public class AuthManager2 : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;
    public TMP_Text confirmRegisterText; // เพิ่มตัวแปรนี้

    DatabaseReference databaseReference;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;

        // เชื่อมต่อ Firebase Realtime Database
        FirebaseApp app = FirebaseApp.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    //Function for the login button
    public void LoginButton()
    {
        // ตรวจสอบว่าทั้ง email และ password ถูกกรอกครบหรือไม่
        if (string.IsNullOrEmpty(emailLoginField.text) || string.IsNullOrEmpty(passwordLoginField.text))
        {
            warningLoginText.text = "Please enter email and password.";
            return; // ออกจากฟังก์ชันโดยไม่ดำเนินการต่อ
        }

        // เรียกใช้งาน Coroutine สำหรับการ Login โดยใช้ email และ password ที่ผู้ใช้กรอก
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
        SceneManager.LoadScene("Card sample");
    }
    //Function for the register button
    public void RegisterButton()
    {
        if (string.IsNullOrEmpty(usernameRegisterField.text) || string.IsNullOrEmpty(emailRegisterField.text) || string.IsNullOrEmpty(passwordRegisterField.text) || string.IsNullOrEmpty(passwordRegisterVerifyField.text))
        {
            warningRegisterText.text = "Please enter information.";
            return; // ออกจากฟังก์ชันโดยไม่ดำเนินการต่อ
        }
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
        UIManager.instance.LoginScreen();
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            // เข้าสู่หน้า Card Sample เมื่อ Login สำเร็จ
            SceneManager.LoadScene("Card Sample");
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            // ถ้าไม่ได้ป้อน username ให้แสดงข้อความเตือน
            warningRegisterText.text = "Missing Username";
            yield break; // ออกจากฟังก์ชัน Register โดยไม่ทำงานต่อ
        }

        if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            // ถ้า password ไม่ตรงกัน ให้แสดงข้อความเตือน
            warningRegisterText.text = "Password Does Not Match!";
            yield break; // ออกจากฟังก์ชัน Register โดยไม่ทำงานต่อ
        }

        // สร้างบัญชีผู้ใช้ใน Firebase Authentication
        Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

        // รอให้การสร้างบัญชีผู้ใช้เสร็จสมบูรณ์
        yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

        if (RegisterTask.Exception != null)
        {
            // แสดงข้อความเตือนถ้าเกิดข้อผิดพลาดในการสร้างบัญชีผู้ใช้
            Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
            FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Register Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WeakPassword:
                    message = "Weak Password";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "Email Already In Use";
                    break;
            }
            warningRegisterText.text = message;
        }
        else
        {
            // ตรวจสอบ User ว่าไม่ใช่ null ก่อนที่จะทำการบันทึก username
            User = RegisterTask.Result.User;
            if (User != null)
            {
                // บันทึก username ไปยัง Realtime Database
                DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(User.UserId).Child("username");
                reference.SetValueAsync(_username).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Username saved to Realtime Database");
                        UIManager.instance.LoginScreen();
                    }
                    else
                    {
                        Debug.LogError("Failed to save username to Realtime Database");
                    }
                });

                // แสดงข้อความยืนยันการลงทะเบียนสำเร็จ
                warningRegisterText.text = "";
                confirmRegisterText.text = "Registered Successfully!";
            }
        }
    }

    public void CreateAccountTextClicked()
    {
        UIManager.instance.RegisterScreen(); // เปิดหน้า register โดยเรียกใช้งานฟังก์ชัน RegisterScreen() ใน UIManager
    }
}
