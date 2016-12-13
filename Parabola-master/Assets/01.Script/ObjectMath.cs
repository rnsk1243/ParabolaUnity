using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ObjectMath : MonoBehaviour
{

    // 시작 높이
    public float height = 0.0f;
    // 힘의 크기
    public float power = 0.0f;
    // 각도
    public float angle = 0.0f;
    // 질량 (클수록 적게 이동)
    public float m_mass = 0.0f;

    // 중력 사용할래 말래?
    public bool isGravity = true;

    Rigidbody rig;
    Transform tr;
    float maxY;

    // 바닥에 충돌했을때 위치
    Vector3 creshPosition;
    // 시작 위치
    Vector3 testPosition;
    // 실제 시간 기록을위한 변수
    float realityTime;
    // 힘의 x성분
    float vectorX;
    float vectorY;

    TrailRenderer trail;

    bool isStart = false;
    bool isCrash = false;
    Init BallInit;

    InputField angleInputField;

    Vector3 highestXY = Vector3.zero;

    FollowCam cam;

    public Vector3 GetParabolaVertex()
    {
        if(highestXY.x == 0 && highestXY.y == 0)
        {
            Debug.Log("초기화 아직임");
            return new Vector3(height, height, 0.0f);
        }
        return highestXY;
    }

    // Use this for initialization
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowCam>();
        BallInit = GameObject.Find("GameObject").GetComponent<Init>();
        trail = GetComponent<TrailRenderer>();
        trail.time = Mathf.Infinity;
        tr = GetComponent<Transform>();
        rig = GetComponent<Rigidbody>();
        angleInputField = GameObject.Find("AngleInput").GetComponent<InputField>();
        m_mass = BallInit.mass;
        isGravity = BallInit.isGravity;

        rig.mass = m_mass;
        rig.useGravity = isGravity;
        angle = BallInit.angle;
        height = BallInit.height;
        power = BallInit.power;


        if (isGravity)
        {
            // 포물선만 가능하도록 각도 조절
            if (angle > 180)
            {
                angle = 180;
            }
            else if (angle < 0)
            {
                angle = 0;
            }
        }
        else
        {
            // 자유낙하만 가능하도록 각도 조절
            if (angle <= 180)
            {
                angle = 181;
            }
            else if (angle >= 360)
            {
                angle = 359;
            }
        }
        // Debug.Log("power ==== " + power);
        // Debug.Log("angle ==== " + angle);

        angleInputField.text = angle.ToString();

        vectorX = power * Mathf.Cos((angle * Mathf.PI) / 180.0f);
        vectorY = power * Mathf.Sin((angle * Mathf.PI) / 180.0f);
        //Debug.Log("Mathf.Sin((angle * Mathf.PI) / 180.0f) = " + Mathf.Sin((angle * Mathf.PI) / 180.0f));
        testPosition = new Vector3(0.0f, height, 0.0f);

        Debug.Log("출발 위치 = " + testPosition);
        // 테스트 위치로 이동
        tr.Translate(testPosition);
        BallInit.simullBall.transform.position = testPosition;

        // Debug.Log("cos = " + Mathf.Cos((angle * Mathf.PI) / 180.0f));
        // Debug.Log("sin = " + Mathf.Sin((angle * Mathf.PI) / 180.0f));
        // 출발
        rig.AddForce(new Vector3(vectorX, vectorY));

        
        //
        // 출발 시간 기록
        realityTime = Time.realtimeSinceStartup;
        //Debug.Log("Time.deltaTime = " + Time.deltaTime);
        //rig.vel

        // 예상 수치 기록 코루틴 실행
        if (isGravity)
        {
            StartCoroutine(useGravity());
        }
        else
        {
            StartCoroutine(zeroGravity());
        }
        StartCoroutine(MyParabola());
    }

    void Update()
    {
        
        // 현재높이 나중높이 비교 하여 큰값 저장
        maxY = Mathf.Max(tr.position.y, maxY);

    }

    // 중력사용 포물선
    IEnumerator useGravity()
    {
        // 이전 위치
        Vector3 oldPosition = tr.position;
        yield return null; // 한 프레임
        // 한 프레임 후 위치
        Vector3 newPosition = tr.position;
        // 위치변화량
        Vector3 delPosition = newPosition - oldPosition;
        if (delPosition.y < 0)
        {
            Debug.Log("delPosition.y = " + delPosition.y);
            delPosition.y = -delPosition.y;
            Debug.Log("변환 후 delPosition.y = " + delPosition.y);
        }
        if (delPosition.x < 0)
        {
            Debug.Log("delPosition.x = " + delPosition.x);
            delPosition.x = -delPosition.x;
            Debug.Log("변환 후 delPosition.x = " + delPosition.x);
        }
        // 속도 
        Vector3 velocity = (delPosition / Time.deltaTime) + new Vector3(0.0f, 0.1f);
        // 최고점은 나중속도가 0이므로 다음 공식이 성립함 -> 처음속도^2 = -(2 * 중력 * 거리)
        // 따라서 높이 = -(처음속도^2 / (2 * 중력))
        float powerH = (-Mathf.Pow(velocity.y, 2.0f) / (2.0f * (-9.81f)));
        // 최고 높이 = 기본시작높이 + 힘으로만 올라간 높이
        float maxY = height + powerH;
        // 예상 최고높이까지 걸린 시간 // 1/2 * 중력 * 시간^2 = 거리 이므로 시간을 구한다.
        float time = Mathf.Sqrt((2 * powerH) / -(-9.81f));
        // 최고높이 일때 x좌표 (시간*속도)
        float maxX = time * velocity.x;
        // 예상되는 포물선의 꼭지점 좌표
        highestXY = new Vector3(maxX, maxY);

        if(cam.CameraSetUp != null)
        {
            cam.CameraSetUp(highestXY, power);
        }

        // 예상 최고높이에서 땅에 닿기까지 걸린 시간
        float time2 = Mathf.Sqrt((2 * maxY) / -(-9.81f));

        if (time < 0)
        {
            time = -time;
        }

        if (time2 < 0)
        {
            time2 = -time2;
        }

        // 최고높이까지 올라갈때 걸린 시간과 최고높이에서 땅에 닿기까지 걸린시간을 더하여 전체 걸린시간을 구함.
        float resultTime = time + time2;


        float exBH = Mathf.Round(maxY * 100.0f) / 100.0f;
        float d = resultTime * velocity.x;
        float exTime = Mathf.Round(resultTime * 100.0f) / 100.0f;
        float exDis = Mathf.Round(d * 100.0f) / 100.0f;
        Debug.Log("예상 시작부터 땅에 닿기까지 걸린 시간 = " + resultTime + "초");

        Debug.Log("예상 이동 거리 = " + d);
        isStart = true;
        GameObject.Find("exBestHeight").GetComponent<Text>().text = exBH.ToString();
        GameObject.Find("exTime").GetComponent<Text>().text = exTime.ToString();
        GameObject.Find("exDis").GetComponent<Text>().text = exDis.ToString();
    }
    // 무중력 자유낙하
    IEnumerator zeroGravity()
    {
        // 이전 위치
        Vector3 oldPosition = tr.position;
        yield return null; // 한 프레임
        // 한 프레임 후 위치
        Vector3 newPosition = tr.position;
        // 위치변화량
        Vector3 delPosition = newPosition - oldPosition;

        if(delPosition.y < 0)
        {
            Debug.Log("delPosition.y = " + delPosition.y);
            delPosition.y = -delPosition.y;
            Debug.Log("변환 후 delPosition.y = " + delPosition.y);
        }
        if (delPosition.x < 0)
        {
            Debug.Log("delPosition.x = " + delPosition.x);
            delPosition.x = -delPosition.x;
            Debug.Log("변환 후 delPosition.x = " + delPosition.x);
        }

        Debug.Log("속도 = " + delPosition / Time.deltaTime);
        // 순간 속도 
        Vector3 velocity = (delPosition / Time.deltaTime);

        Debug.Log("y성분 속도 = " + velocity.y);
        // 예상 시간 // 중력이 없으므로 -> 처음속도 * 시간 = 시작높이 // 따라서 시간 = 시작높이 / 처음속도 이다.
        // 이때 속도 성분은 -을 곱하여 양수로 만든다. 
        float resultTime = height / -velocity.y;
        print("=========예상============");

        if(resultTime < 0)
        {
            resultTime = -resultTime;
        }

        Debug.Log("예상 시작부터 땅에 닿기까지 걸린 시간 = " + resultTime + "초");
        // 거리 = 시간 * 속도
        Debug.Log("예상 이동 거리 = " + resultTime * velocity.x);


        float d = resultTime * velocity.x;
        float exTime = Mathf.Round(resultTime * 100.0f) / 100.0f;
        float exDis = Mathf.Round(d * 100.0f) / 100.0f;
        Debug.Log("예상 시작부터 땅에 닿기까지 걸린 시간 = " + resultTime + "초");

        Debug.Log("예상 이동 거리 = " + d);
        
       

        GameObject.Find("exBestHeight").GetComponent<Text>().text = height.ToString();
        GameObject.Find("exTime").GetComponent<Text>().text = exTime.ToString();
        GameObject.Find("exDis").GetComponent<Text>().text = exDis.ToString();

    }

    void OnTriggerEnter(Collider coll)
    {
        
            if (coll.gameObject.tag == "Floor")
            {
                if (!isCrash)
                {
                    creshPosition = tr.position;
                    print("=========결과============");
                    if (isGravity)
                    {
                        Debug.Log("실제 최고높이 = " + maxY);
                    }
                    //gameObject.GetComponent<SphereCollider>().isTrigger = true;
                    Destroy(gameObject, 5.0f);

                    ////////
                    float BestHeight = Mathf.Round(maxY * 100.0f) / 100.0f;
                    float t = Time.realtimeSinceStartup - realityTime;
                    float resultTime = Mathf.Round(t * 100.0f) / 100.0f;
                    float Dis = Mathf.Round((creshPosition.x - testPosition.x) * 100.0f) / 100.0f;


                    GameObject.Find("BestHeight").GetComponent<Text>().text = BestHeight.ToString();
                    GameObject.Find("Time").GetComponent<Text>().text = resultTime.ToString();
                    GameObject.Find("Dis").GetComponent<Text>().text = Dis.ToString();

                    print("실제 걸린 시간 = " + resultTime);
                    print("실제 이동 거리 = " + Dis);
                    isCrash = true;
                      Destroy(BallInit.simullBall, 1.0f);
                }

               
            }
        
    }

    IEnumerator MyParabola()
    {
        if(!isStart)
        {
            yield return null;
        }

        // y = a(x-p)^2 + q
        // a = (y - q) / (x - p)^2
        // 그래프는 시작점 (0, height)과 꼭지점 (highestXY.x , highestXY.y) 지남.
        float x = 0.0f;
        float y = 0.0f;
        float a = (height - highestXY.y) / Mathf.Pow((x - highestXY.x), 2.0f);
        while (true)
        {
            // x좌표를 1씩 증가 할때 
            x += 1.0f;
            // y좌표를 구해준다. y = a(x-p)^2 + q
            y = a * Mathf.Pow((x - highestXY.x), 2.0f) + highestXY.y;
            yield return null;
            // (x,y)좌표로 포물선 이동
            BallInit.simullBall.transform.position = new Vector3(x, y);
        }
    }

}
