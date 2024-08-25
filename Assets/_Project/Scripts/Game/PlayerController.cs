using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun ,IPunObservable       //phtonview�� ��ӹ޾Ƽ� ���    // iponobservable�� ����ؼ� hp�� shotCount�� ����ȭ
{
    


    private Rigidbody rb;
    private Animator anim;


    public Transform pointer;   // ĳ���Ͱ� �ٶ� ����
    public Bomb bombPrefab;     // ��ź ����ü ������
    public Transform shotPoint;     // ��ź  ����ü ���� ��ġ
    public float moveSpeed = 5f;    // �̵� �ӵ�
    public float shotPower = 15f;   // ����ü ������ ��
    public float hp = 100f;         // ü��
    public int shotCount = 0;     // ��ź �߻� Ƚ��
    public Text hpText;
    public Text shotText;

    public GameObject[] eyes;


    // private PhotonView photonView;



    private void Awake()
    {
        rb= GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        pointer.gameObject.SetActive(photonView.IsMine);        // ���� �����ϴ� ĳ������ pointer�� Ȱ��ȭ��
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

            // ���ÿ����� ȣ��ɰ̴ϴ�.
            //Fire();

            // PhotonNetwork�� RPC�� ȣ��.
            photonView.RPC("Fire", RpcTarget.All, shotPoint.position, shotPoint.forward);  //  ��� Ŭ���̾�Ʈ���� Fire�� ȣ���ϵ��� ��.
            
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
        var pos = rb.position;                  // �� rb�� ��ġ
        pos.y = 0;                              // �������� ���� �� �����Ƿ� Y�� ��ǥ�� 0����.


        var forward = pointer.position - pos;   

        rb.rotation=Quaternion.LookRotation(forward,Vector3.up);    // �� ��ġ���� pointer�� ��ġ�� �ٶ󺸵��� ��
    }


    // Bomb�� Photonview�� ���� ���, ���ʿ��� ��Ŷ�� ��ȯ�Ǵ� ��ȿ���� �߻��ϹǷ�,
    // Ư�� Ŭ���̾�Ʈ�� Fire�� ȣ���� ��� �ٸ� Ŭ���̾�Ʈ���� RPC�� ���� �Ȱ��� Fire�� 
    // ȣ���ϵ��� �ϰ� ����.
    // ���� �׹�(Dead Reckoning) �˰����� Ȱ���ϱ� ���� ����ü�� �� Ŭ���̾�Ʈ���� �����ϵ���
    // Remote Produre Call�� �����.
    [PunRPC]
    private void Fire(Vector3 shotPoint, Vector3 shotDirection ,PhotonMessageInfo info)     //���������� ���� info
    {
        // ������ �����ؼ�, ���� �ð��� �� Ŭ���̾�Ʈ�� �ð� ���̸�ŭ ���� ����.

        print($"Fire Procedure Called by{info.Sender.NickName}");
        print($"local time :{PhotonNetwork.Time}");
        print($"server time :{info.SentServerTime}");

                            // 1 : 35 : 20.5     1 : 35 : 20.3            0.2���� ������ ���� ���      ����Ÿ���� ������ ��������.      
        float lag=(float)(PhotonNetwork.Time - info.SentServerTime);        // ���� �ð��� Ŭ���̾�Ʈ �ð��� ����

        var bomb=Instantiate(bombPrefab,shotPoint, Quaternion.identity);
        bomb.rb.AddForce(shotDirection * shotPower, ForceMode.Impulse);
        bomb.Owner = photonView.Owner;


        // �������� ���巹Ŀ�װ��� �ٸ� ���.
        bomb.rb.position+=bomb.rb.velocity * lag;        // ��ź�� ������ ���������� ��ġ�� ���
        // ��ź�� ��ġ���� ��ź�� �����ŭ �����ð� ���� ����� ��ġ�� ����.


    }

    public void Hit(float damage)
    {
        hp -= damage;
        /*if(hp>0)        //����
        {

        }*/
        hpText.text = hp.ToString();


    }

    public void Heal(float amount)
    {
        hp += amount;

        if (hp > 100) hp = 100;     // �ִ� ü���� 100���� �������� ���.

        hpText.text = hp.ToString();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // stream�� ���ؼ� HP�� shotCount�� ����ȭ
        // stream�� queue�� ����.
        if(stream.IsWriting)    // ����
        { 
            stream.SendNext(hp);
            stream.SendNext(shotCount);
        }
        else
        {
            // ������
            hp=(float)stream.ReceiveNext();
            shotCount=(int)stream.ReceiveNext();
            hpText.text = hp.ToString();
            shotText.text = shotCount.ToString();
           
        }
    }
}
