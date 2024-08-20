using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public ParticleSystem particlePrefab;

    //[HideInInspector]
    //public Rigidbody rb;
    //[HideInInspector]
    //public Player owner;

    public Rigidbody rb { get; private set; }
    public Player Owner { get; set; }

    // explosion radius => ���� ����
    public float expRad = 1.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        var particle = Instantiate(particlePrefab, transform.position, particlePrefab.transform.rotation);
        particle.Play();

        // ������Ʈ Ǯ�� ��õ
        Destroy(particle, 3f);

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 0.1f);

        // ���� ���� ���� ������ �����ؼ� �ش� ���� �ȿ� ���� �ݶ��̴��� ���
        var contactedColliders = Physics.OverlapSphere(transform.position, expRad);
        
        foreach(var collider in contactedColliders)
        {
            // 1��° ���
            //if(collider.tag.Equals("Player"))
            //{
            //    // �÷��̾�� Ÿ�� �Լ� ȣ��
            //    collider.SendMessage("Hit",1,SendMessageOptions.RequireReceiver);
               
            //}

            // 2��° ���
            if(collider.TryGetComponent<PlayerController>(out var player))
            {
                // �´»�� ����
                // local�÷��̾��� 
                bool isMine = PhotonNetwork.LocalPlayer.ActorNumber == player.photonView.Owner.ActorNumber;

                if(isMine)
                {
                    player.Hit(1);
                    print($"{Owner.NickName}�� ���� ��ź�� {player.photonView.Owner.NickName} ���� ����");
                }
            }
        }

    }


}
