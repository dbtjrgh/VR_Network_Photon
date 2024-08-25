using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public ParticleSystem particlePrefab;
    /*[HideInInspector] public Rigidbody rb;      // ���� : debug������ �Ⱥ��δ�. �׷��Ƿ� hideininspector���� internal�� ����Ѵ�.
    [HideInInspector] public Player owner;*/

    /*private Rigidbody rb;
    public Rigidbody RB { get => rb; }      // ĸ��ȭ*/

    public Rigidbody rb { get; private set; }

    public Player Owner { get; set; }

    public float expRad=1.5f;        // explosion radius         : ���� ����


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

    }

    private void OnTriggerEnter(Collider other)
    {
        var particle = Instantiate(particlePrefab, transform.position, particlePrefab.transform.rotation);

        particle.Play();

        Destroy(particle.gameObject, 3f);      // ��� ������Ʈ Ǯ�� ���°� ����

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 0.1f);        // 0.1�� �ڿ� ��ź�� �����

        // ���� ���� ���� ������ �����ؼ� �ش� ���� �ȿ� ���� �ݶ��̴��� ���.
        var contactedColliders = Physics.OverlapSphere(transform.position, expRad);

        foreach(var coll in contactedColliders)
        {
            /*if(coll.tag.Equals("Player"))
            {
                // �÷��̾�� Ÿ�� �Լ� ȣ��
                coll.SendMessage("Hit",1,SendMessageOptions.RequireReceiver);        // 1�� ������

            }*/


            if(coll.TryGetComponent<PlayerController>(out var player))
            {

                // local player�� ���� ��ź�� ���� �÷��̾�� �������� ��� true
                bool isMine= PhotonNetwork.LocalPlayer.ActorNumber==player.photonView.Owner.ActorNumber;                //userid�� actorNumber�� ���ص� ��
                // �´� �������  �������� ��
                if(isMine)
                {
                    player.Hit(1);                    
                }
                print($"{Owner.NickName}�� ���� ��ź�� {player.photonView.Owner.NickName}���� ����");     // ��ְ� ���ϰ��
            }
        }

    }

}


// photon view  :   ��Ŷ�� �����ϰ� ����
// RPC : Remote Procedure Call  : ��û�� ���� ���� ����
// Custom Property : �÷��̾��� ���¸� �����ϴµ� ��� :RPC�� PhotonView�� �߰��ܰ� rpc���� ��� photonview���� ��
