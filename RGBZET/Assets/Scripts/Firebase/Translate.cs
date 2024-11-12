using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
public class Translate : MonoBehaviour
{
    public Text uiText;  // UI Text ที่แสดงข้อความ
    //private bool isEnglish = true;  // ตรวจสอบว่าใช้ภาษาอังกฤษหรือไม่

    // พจนานุกรมเก็บคำแปล
    private Dictionary<string, string> englishToThai = new Dictionary<string, string>()
    {
       
        { "The object of the game is to identify a SET of 3 cards from 12 cards place face up on the board.", "เป้าหมายของเกมนี้คือการจำแนกการ์ด 3 ใบจาก 12 ใบบนกระดาน ให้ตรง SET"},
        { "Each card has four features","โดยแต่ละการ์ดจะมี 4 คุณลักษณะดังนี้" },
        { "Color", "สี" },
        { "Letter", "ตัวอักษร" },
        { "Amount", "จำนวน" },
        { "Font", "รูปแบบตัวอักษร" },
        { "A SET consists of 3 cards in which each of the cards features looked ata one-by-one, are the same on each card or, are different on each card"," ในหนึ่ง SET จะประกอบด้วยการ์ด 3 ใบ ที่มีคุณลักษณะทั้ง 4 เหมือนกันทั้งหมดหรือต่างกันทั้งหมด "}
        
    };

    private Dictionary<string, string> thaiToEnglish = new Dictionary<string, string>()
    {
        { "เป้าหมายของเกมนี้คือการจำแนกการ์ด 3 ใบจาก 12 ใบบนกระดาน ให้ตรง SET","The object of the game is to identify a SET of 3 cards from 12 cards place face up on the board."},
        { "โดยแต่ละการ์ดจะมี 4 คุณลักษณะดังนี้","Each card has four features" },
        { "สี","Color" },
        { "ตัวอักษร","Letter" },
        { "จำนวน","Amount" },
        { "รูปแบบตัวอักษร","Font" },
        { "ในหนึ่ง SET จะประกอบด้วยการ์ด 3 ใบ ที่มีคุณลักษณะทั้ง 4 เหมือนกันทั้งหมดหรือต่างกันทั้งหมด ","A SET consists of 3 cards in which each of the cards features looked ata one-by-one, are the same on each card or, are different on each card"}
        
    };
}
