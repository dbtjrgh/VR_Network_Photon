using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun , IPunObservable {

	private Rigidbody rb;
	private Animator anim;

	public Transform pointer;//캐릭터가 바라볼 방향
	public Bomb bombPrefab;//폭탄 투사체 프리팹
	public Transform shotPoint; //투사체 생성위치
	public float moveSpeed = 5f; //이동속도
	public float shotPower = 15f; //투사체 던지는 힘
	public float hp = 100f; //체력
	public int shotCount = 0;//투사체 발사 횟수
	public Text hpText;
	public Text shotText;

	public GameObject[] eyes;

	private void Awake() {
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
		pointer.gameObject.SetActive(photonView.IsMine); //내가 조종하는 캐릭터의 pointer만 활성화함.
		hpText.text = hp.ToString();
	}
	private void Update() {
		if (false == photonView.IsMine) return;
		Move();

		if (Input.GetButtonDown("Fire1")) {
			shotCount++;
			shotText.text = shotCount.ToString();

			//로컬에서만 호출될겁니다.
			//Fire();


			//PhotonNetwork의 RPC를 호출.
			photonView.RPC("Fire", RpcTarget.All, shotPoint.position, shotPoint.forward);

		}

	}

	private void FixedUpdate() {
		if (false == photonView.IsMine) return;
		Rotate();
	}

	private void Move() {
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		rb.velocity = new Vector3(x, 0, z) * moveSpeed;
	}

	private void Rotate() {
		var pos = rb.position; //내 rb의 위치
		pos.y = 0; //고저차가 있을 수 있으므로 y축 좌표를 0으로.
		var forward = pointer.position - pos;

		//내 위치에서 pointer쪽으로 바라보도록 함
		rb.rotation = Quaternion.LookRotation(forward, Vector3.up); 
	}

	//Bomb에 PhotonView가 붙을 경우 불필요한 패킷이 교환되는 비효율이 발생 하므로,
	//특정 클라이언트가 Fire를 호출할 경우 다른 클라이언트에게
	//RPC를 통해 똑같이 Fire를 호출하도록 하고싶음.
	//추측 항법(Dead Reckoning) 알고리즘을 활용하기 위해 투사체는 각 클라이언트에서 생성하도록
	//Remote Procedure Call을 함.
	[PunRPC]
	private void Fire(Vector3 shotPoint, Vector3 shotDirection, PhotonMessageInfo info) {
		//지연을 보상해서, 서버 시간과 내 클라이언트의 시간 차이만큼 값을 보정.

		print($"Fire Procedure Called by {info.Sender.NickName}");
		print($"local time : {PhotonNetwork.Time}");
		print($"server time : {info.SentServerTime}");

		//					1:35:20.5				1:35:20.3   0.2초의 지연이 있을 경우
		float lag = (float)(PhotonNetwork.Time - info.SentServerTime);

		var bomb = Instantiate(bombPrefab, shotPoint, Quaternion.identity);
		bomb.rb.AddForce(shotDirection * shotPower, ForceMode.Impulse);
		bomb.Owner = photonView.Owner;

		//폭탄의 위치에서 폭탄의 운동량 만큼 지연시간동안 진행한 위치로 보정
		//지연 보상
		bomb.rb.position += bomb.rb.velocity * lag;

	}

	public void Hit(float damage) {
		hp -= damage;
		//if (hp < 0) ;//죽음

		hpText.text = hp.ToString();

	}

	public void Heal(float amount) {
		hp += amount;

		if(hp > 100f) hp = 100f; //최대 체력을 100으로 설정했을 경우.

		hpText.text = hp.ToString();
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		//stream을 통해서 hp와 shotCount만 동기화
		//stream은 queue의 형태.
		if (stream.IsWriting) { //쓸때
			stream.SendNext(hp);
			stream.SendNext(shotCount);
		} else { //읽을때
			hp = (float)stream.ReceiveNext();
			shotCount = (int)stream.ReceiveNext();
			hpText.text = hp.ToString();
			shotText.text = shotCount.ToString();
		}
	}
}
