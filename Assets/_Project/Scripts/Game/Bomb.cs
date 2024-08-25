using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public ParticleSystem particlePrefab;
    /*[HideInInspector] public Rigidbody rb;      // 맹점 : debug에서도 안보인다. 그러므로 hideininspector보다 internal을 사용한다.
    [HideInInspector] public Player owner;*/

    /*private Rigidbody rb;
    public Rigidbody RB { get => rb; }      // 캡슐화*/

    public Rigidbody rb { get; private set; }

    public Player Owner { get; set; }

    public float expRad=1.5f;        // explosion radius         : 폭발 범위


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

    }

    private void OnTriggerEnter(Collider other)
    {
        var particle = Instantiate(particlePrefab, transform.position, particlePrefab.transform.rotation);

        particle.Play();

        Destroy(particle.gameObject, 3f);      // 사실 오브젝트 풀링 쓰는게 좋음

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 0.1f);        // 0.1초 뒤에 폭탄이 사라짐

        // 게임 씬에 구형 영역을 전개해서 해당 영역 안에 들어온 콜라이더를 골라냄.
        var contactedColliders = Physics.OverlapSphere(transform.position, expRad);

        foreach(var coll in contactedColliders)
        {
            /*if(coll.tag.Equals("Player"))
            {
                // 플레이어에게 타격 함수 호출
                coll.SendMessage("Hit",1,SendMessageOptions.RequireReceiver);        // 1은 데미지

            }*/


            if(coll.TryGetComponent<PlayerController>(out var player))
            {

                // local player인 내가 폭탄에 맞은 플레이어와 동일인인 경우 true
                bool isMine= PhotonNetwork.LocalPlayer.ActorNumber==player.photonView.Owner.ActorNumber;                //userid나 actorNumber로 비교해도 됨
                // 맞는 사람기준  데미지를 줌
                if(isMine)
                {
                    player.Hit(1);                    
                }
                print($"{Owner.NickName}이 던진 폭탄이 {player.photonView.Owner.NickName}에게 맞음");     // 쏜애가 나일경우
            }
        }

    }

}


// photon view  :   패킷을 무수하게 보냄
// RPC : Remote Procedure Call  : 요청이 있을 때만 보냄
// Custom Property : 플레이어의 상태를 저장하는데 사용 :RPC와 PhotonView의 중간단계 rpc보단 잦게 photonview보단 덜
