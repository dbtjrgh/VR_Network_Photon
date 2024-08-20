using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(PhotonView))]

//phtonview�� ��ӹ޾Ƽ� ���
public class PlayerController : MonoBehaviourPun,IPunObservable
{
    #region ����

    private Rigidbody rb;
    private Animator anim;

    // ĳ���Ͱ� �ٶ� ����
    public Transform pointer;
    // ��ź ����ü ������
    public Bomb bombPrefab;
    // ����ü ���� ��ġ
    public Transform shotPoint;

    // private PhotonView photonView;

    // �̵��ӵ�
    public float moveSpeed = 5f;
    // ����ü ������ ��
    public float shotPower = 15f;
    // ü��
    public float hp = 50f;
    // ����ü �߻� Ƚ��
    public int shotCount = 0;
    public Text hpText;
    public Text shotText;


    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        // ���� �����ϴ� ĳ������ Pointer�� �߻���
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
            // ���ÿ����� ȣ��ǵ���
            // Fire();

            // PhotonNetwork�� RPC�� ȣ��.
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
        // �� rb�� ��ġ
        var pos = rb.position;
        // �������� ���� �� �����Ƿ� y�� ��ǥ�� 0����
        pos.y = 0;
        var forward = pointer.position - pos;

        // �� ��ġ���� pointer������ �ٶ󺸵��� ��
        rb.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }


    // Bomb�� PhotonView�� ���� ��� ���ʿ��� ��Ŷ�� ��ȯ�Ǵ� ��ȿ���� �߻��ϹǷ�,
    // Ư�� Ŭ���̾�Ʈ�� Fire�� ȣ���� ��� �ٸ� Ŭ���̾�Ʈ����
    // RPC�� ���� �Ȱ��� Fire�� ȣ���ϵ��� �ϰ����.
    [PunRPC]
    private void Fire(Vector3 shotPoint, Vector3 shotDirection, PhotonMessageInfo info)
    {
        // ������ �����ؼ�, ���� �ð��� �� Ŭ���̾�Ʈ�� �ð� ���̸�ŭ ���� ����.

        print($"Fire Procedure Called by{info.Sender.NickName}");
        print($"local time : {PhotonNetwork.Time}");
        print($"server time : {info.SentServerTime}");

        //                       1:35:20.5          1:35:20.3       0.2���� ������ ���� ���
        float lag = (float)(PhotonNetwork.Time - info.SentServerTime);

        var bomb = Instantiate(bombPrefab, shotPoint, Quaternion.identity);
        bomb.rb.AddForce(shotDirection * shotPower, ForceMode.Impulse);
        bomb.Owner = photonView.Owner;

        // ��ź�� ��ġ���� ��ź�� ��� ��ŭ �����ð����� ������ ��ġ�� ����
        bomb.rb.position += bomb.rb.velocity * lag;
    }

    public void Hit(float damage)
    {
        hp -= damage;
        // ����
        //if (hp < 0)
        //{

        //}
        hpText.text = hp.ToString();
        Debug.Log($"Player�� {damage}�� ����.");
    }

    public void Heal(float amount)
    {
        hp += amount;

        if(hp > 100)
        {
            // �ִ� ü���� 100���� �������� ���
            hp = 100;
        }
        hpText.text = hp.ToString();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // stream�� ���ؼ� hp�� shotCount�� ����ȭ
        // �� ��
        if (stream.IsWriting)
        {
            stream.SendNext(hp);
            stream.SendNext(shotCount);
        }
        // ���� ��
        else if (stream.IsReading)
        {
            hp = (float)stream.ReceiveNext();
            shotCount = (int)stream.ReceiveNext();
            hpText.text = hp.ToString();
            shotText.text = shotCount.ToString();
        }
    }
}
