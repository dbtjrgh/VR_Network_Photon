using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(PhotonView))]

//phtonview를 상속받아서 사용
public class PlayerController : MonoBehaviourPun,IPunObservable
{
    #region 변수

    private Rigidbody rb;
    private Animator anim;

    // 캐릭터가 바라볼 방향
    public Transform pointer;
    // 폭탄 투사체 프리팹
    public Bomb bombPrefab;
    // 투사체 생성 위치
    public Transform shotPoint;

    // private PhotonView photonView;

    // 이동속도
    public float moveSpeed = 5f;
    // 투사체 던지는 힘
    public float shotPower = 15f;
    // 체력
    public float hp = 50f;
    // 투사체 발사 횟수
    public int shotCount = 0;
    public Text hpText;
    public Text shotText;


    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        // 내가 조종하는 캐릭터의 Pointer만 발사함
        pointer.gameObject.SetActive(photonView.IsMine);
        hpText.text = hp.ToString();
        //photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if(false == photonView.IsMine)
        {
            return;
        }
        Move();

        if (Input.GetButtonDown("Fire1"))
        {
            shotCount++;
            shotText.text = shotCount.ToString();
            // 로컬에서만 호출되도록
            // Fire();

            // PhotonNetwork의 RPC를 호출.
            photonView.RPC("Fire", RpcTarget.All, shotPoint.position, shotPoint.forward);
        }
    }

    private void FixedUpdate()
    {
        if(false == photonView.IsMine)
        {
            return;
        }
        Rotate();
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        rb.velocity = new Vector3(x, 0, z) * moveSpeed;
    }

    private void Rotate()
    {
        // 내 rb의 위치
        var pos = rb.position;
        // 고저차가 있을 수 있으므로 y축 좌표를 0으로
        pos.y = 0;
        var forward = pointer.position - pos;

        // 내 위치에서 pointer쪽으로 바라보도록 함
        rb.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }


    // Bomb에 PhotonView가 붙을 경우 불필요한 패킷이 교환되는 비효율이 발생하므로,
    // 특정 클라이언트가 Fire를 호출할 경우 다른 클라이언트에게
    // RPC를 통해 똑같이 Fire를 호출하도록 하고싶음.
    [PunRPC]
    private void Fire(Vector3 shotPoint, Vector3 shotDirection, PhotonMessageInfo info)
    {
        // 지연을 보상해서, 서버 시간과 내 클라이언트의 시간 차이만큼 값을 보정.

        print($"Fire Procedure Called by{info.Sender.NickName}");
        print($"local time : {PhotonNetwork.Time}");
        print($"server time : {info.SentServerTime}");

        //                       1:35:20.5          1:35:20.3       0.2초의 지연이 있을 경우
        float lag = (float)(PhotonNetwork.Time - info.SentServerTime);

        var bomb = Instantiate(bombPrefab, shotPoint, Quaternion.identity);
        bomb.rb.AddForce(shotDirection * shotPower, ForceMode.Impulse);
        bomb.Owner = photonView.Owner;

        // 폭탄의 위치에서 폭탄의 운동량 만큼 지연시간동안 진행한 위치로 보정
        bomb.rb.position += bomb.rb.velocity * lag;
    }

    public void Hit(float damage)
    {
        hp -= damage;
        // 죽음
        //if (hp < 0)
        //{

        //}
        hpText.text = hp.ToString();
        Debug.Log($"Player가 {damage}를 입음.");
    }

    public void Heal(float amount)
    {
        hp += amount;

        if(hp > 100)
        {
            // 최대 체력을 100으로 설정했을 경우
            hp = 100;
        }
        hpText.text = hp.ToString();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // stream을 통해서 hp와 shotCount만 동기화
        // 쓸 때
        if (stream.IsWriting)
        {
            stream.SendNext(hp);
            stream.SendNext(shotCount);
        }
        // 읽을 때
        else if (stream.IsReading)
        {
            hp = (float)stream.ReceiveNext();
            shotCount = (int)stream.ReceiveNext();
            hpText.text = hp.ToString();
            shotText.text = shotCount.ToString();
        }
    }
}
