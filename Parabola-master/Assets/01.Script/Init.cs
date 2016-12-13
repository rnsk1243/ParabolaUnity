using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Init : MonoBehaviour {

    public GameObject ball;
    public GameObject myBall;
    public GameObject simullBall;
    public float height;
    public float power;
    public float angle;
    public float mass;
    public bool isGravity;
    bool isStart;


    void Start()
    {

    }

    public void isStartButton()
    {
        GameObject.Find("BestHeight").GetComponent<Text>().text = "?";
        GameObject.Find("Time").GetComponent<Text>().text = "?";
        GameObject.Find("Dis").GetComponent<Text>().text = "?";

        height = float.Parse(GameObject.Find("HeightInput").GetComponent<InputField>().text);
        power = float.Parse(GameObject.Find("PowerInput").GetComponent<InputField>().text);
        angle = float.Parse(GameObject.Find("AngleInput").GetComponent<InputField>().text);
        mass = float.Parse(GameObject.Find("MessInput").GetComponent<InputField>().text);
        isGravity = GameObject.Find("GravityToggle").GetComponent<Toggle>().isOn;
        isStart = true;

        //Debug.Log("height = " + height);
        //Debug.Log("power = " + power);
        //Debug.Log("angle = " + angle);
        //Debug.Log("mass = " + m_mass);
        //Debug.Log("isGravity = " + isGravity);

       Instantiate(ball,new Vector3(0.0f,0.0f,0.0f),Quaternion.identity);
       simullBall = (GameObject)Instantiate(myBall, new Vector3(0.0f,0.0f,0.0f), Quaternion.identity);
    }

}
