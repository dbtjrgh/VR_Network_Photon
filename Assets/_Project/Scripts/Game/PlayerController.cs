using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun ,IPunObservable       //phtonview를 상속받아서 사용    // iponobservable을 사용해서 hp와 shotCount를 동기화
{
    


    private Rigidbody rb;
    private Animator anim;


    public Transform pointer;   // 캐릭터가 바라볼 방향
    public Bomb bombPrefab;     // 폭탄 투사체 프리펩
    public Transform shotPoint;     // 폭탄  투사체 생성 위치
    public float moveSpeed = 5f;    // 이동 속도
    public float shotPower = 15f;   // 투사체 던지는 힘
    public float hp = 100f;         // 체력
    public int shotCount = 0;     // 폭탄 발사 횟수
    public Text hpText;
    public Text shotText;

    public GameObject[] eyes;


    // private PhotonView photonView;



    private void Awake()
    {
        rb= GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        pointer.gameObject.SetActive(photonView.IsMine);        // 내가 조종하는 캐릭터의 pointer만 활성화함
        //photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (false==photonView.IsMine) return;

        Move();

        if(Input.GetButtonDown("Fire1"))
        {
            shotCount++;
            shotText.text = shotCount.ToString();

            // 로컬에서만 호출될겁니다.
            //Fire();

            // PhotonNetwork의 RPC를 호출.
            photonView.RPC("Fire", RpcTarget.All, shotPoint.position, shotPoint.forward);  //  모든 클라이언트에게 Fire를 호출하도록 함.
            
        }
        
        
    
    }

    private void FixedUpdate()
    {
        if(false == photonView.IsMine) return;

        Rotate();
    }

    private void Move()
    {
        float x=Input.GetAxis("Horizontal");
        float z=Input.GetAxis("Vertical");

        rb.velocity=new Vector3(x, 0, z) * moveSpeed;


    }

    private void Rotate()
    {
        var pos = rb.position;                  // 내 rb의 위치
        pos.y = 0;                              // 고저차가 있을 수 있으므로 Y축 좌표를 0으로.


        var forward = pointer.position - pos;   

        rb.rotation=Quaternion.LookRotation(forward,Vector3.up);    // 내 위치에서 pointer의 위치를 바라보도록 함
    }


    // Bomb에 Photonview가 붙을 경우, 불필요한 패킷이 교환되는 비효율이 발생하므로,
    // 특정 클라이언트가 Fire를 호출할 경우 다른 클라이언트에게 RPC를 통해 똑같이 Fire를 
    // 호출하도록 하고 싶음.
    // 추측 항법(Dead Reckoning) 알고리즘을 활용하기 위해 투사체는 각 클라이언트에서 생성하도록
    // Remote Produre Call을 사용함.
    [PunRPC]
    private void Fire(Vector3 shotPoint, Vector3 shotDirection ,PhotonMessageInfo info)     //지연보상을 위한 info
    {
        // 지연을 보상해서, 서버 시간과 내 클라이언트의 시간 차이만큼 값을 보정.

        print($"Fire Procedure Called by{info.Sender.NickName}");
        print($"local time :{PhotonNetwork.Time}");
        print($"server time :{info.SentServerTime}");

                            // 1 : 35 : 20.5     1 : 35 : 20.3            0.2초의 지연이 있을 경우      로컬타임이 무조건 더느리다.      
        float lag=(float)(PhotonNetwork.Time - info.SentServerTime);        // 서버 시간과 클라이언트 시간의 차이

        var bomb=Instantiate(bombPrefab,shotPoint, Quaternion.identity);
        bomb.rb.AddForce(shotDirection * shotPower, ForceMode.Impulse);
        bomb.Owner = photonView.Owner;


        // 지연보상 데드레커닝과는 다른 방식.
        bomb.rb.position+=bomb.rb.velocity * lag;        // 폭탄이 생성된 시점에서의 위치를 계산
        // 폭탄의 위치에서 폭탄의 운동량만큼 지연시간 동안 진행된 위치로 보정.


    }

    public void Hit(float damage)
    {
        hp -= damage;
        /*if(hp>0)        //죽음
        {

        }*/
        hpText.text = hp.ToString();


    }

    public void Heal(float amount)
    {
        hp += amount;

        if (hp > 100) hp = 100;     // 최대 체력을 100으로 설정했을 경우.

        hpText.text = hp.ToString();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // stream을 통해서 HP와 shotCount만 동기화
        // stream은 queue의 형태.
        if(stream.IsWriting)    // 쓸때
        { 
            stream.SendNext(hp);
            stream.SendNext(shotCount);
        }
        else
        {
            // 읽을때
            hp=(float)stream.ReceiveNext();
            shotCount=(int)stream.ReceiveNext();
            hpText.text = hp.ToString();
            shotText.text = shotCount.ToString();
           
        }
    }
}
