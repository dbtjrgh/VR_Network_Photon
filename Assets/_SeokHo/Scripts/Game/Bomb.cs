using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Bomb : MonoBehaviour {
	public ParticleSystem particlePrefab;
	//[HideInInspector] public Rigidbody rb;
	//[HideInInspector] public Player owner;
	public Rigidbody rb { get; private set; }
	public Player Owner { get; set; }
	public float expRad = 1.5f; //explosion radius => 폭발 범위

	private void Awake() {
		rb = GetComponent<Rigidbody>();
	}

	private void OnTriggerEnter(Collider other) {
		var particle = Instantiate(particlePrefab, transform.position, particlePrefab.transform.rotation);
		particle.Play();
		
		Destroy(particle, 3f); //사실 오브젝트 풀링 쓰는게 좋음

		GetComponent<Renderer>().enabled = false;
		GetComponent<Collider>().enabled = false;

		Destroy(gameObject, 0.1f);

		//게임 씬에 구형 영역을 전개해서 해당 영역 안에 들어온 콜라이더를 골라냄
		var contactedColliders = Physics.OverlapSphere(transform.position, expRad);

		foreach(var coll in contactedColliders) {
			//if (coll.tag.Equals("Player")) {
			//	//플레이어에게 타격 함수 호출
			//	coll.SendMessage("Hit", 1, SendMessageOptions.RequireReceiver);
			//}

			if(coll.TryGetComponent<PlayerController>(out var player)) {

				//local플레이어인 내가 폭탄에 맞은 플레이어와 동일인일 경우 true
				bool isMine = PhotonNetwork.LocalPlayer.ActorNumber == player.photonView.Owner.ActorNumber;

				if (isMine) {
					player.Hit(1);
				}

				print($"{Owner.NickName}이 던진 폭탄이 {player.photonView.Owner.NickName}에게 맞음");
			}
		}
	}

}
