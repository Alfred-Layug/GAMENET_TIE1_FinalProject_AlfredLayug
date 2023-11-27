using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera _camera;
    public GameObject _bulletEffectPrefab;

    [Header("Player Name")]
    public TextMeshProUGUI _playerNameText;

    private Animator _animator;
    private int _score;

    private void Start()
    {
        _playerNameText.text = photonView.Owner.NickName;
        _animator = this.GetComponent<Animator>();
        _score = 0;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && photonView.IsMine)
        {
            Fire();
        }
    }

    public void Fire()
    {
        if (!_animator.GetBool("isDead"))
        {
            RaycastHit hit;
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

            if (Physics.Raycast(ray, out hit, 200))
            {
                Debug.Log(hit.collider.gameObject.name);

                photonView.RPC("CreateBulletEffects", RpcTarget.All, hit.point);
            }
        }
    }

    [PunRPC]
    public void CreateBulletEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(_bulletEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }

    public void Die()
    {
        if (photonView.IsMine)
        {
            _animator.SetBool("isDead", true);
        }
    }

    [PunRPC]
    public void ShowKillFeed(string feedText)
    {
        GameObject killFeedText = GameObject.Find("Kill Feed Text");
        killFeedText.GetComponent<TextMeshProUGUI>().text = feedText;
    }

    [PunRPC]
    public void HideKillFeed()
    {
        GameObject killFeedText = GameObject.Find("Kill Feed Text");
        killFeedText.GetComponent<TextMeshProUGUI>().text = "";
    }

    private void UpdateKillAmount()
    {
        if (photonView.IsMine)
        {
            _score++;
            GameObject killsText = GameObject.Find("Kills Text");
            killsText.GetComponent<TextMeshProUGUI>().text = "Kills: " + _score;

            if (_score >= 10)
            {
                photonView.RPC("ShowWinner", RpcTarget.All, photonView.Owner.NickName);
            }
        }
    }

    [PunRPC]
    public void ShowWinner(string winnerName)
    {
        GameObject winnerText = GameObject.Find("Winner Text");
        
        if (winnerText.GetComponent<TextMeshProUGUI>().text == "")
        {
            winnerText.GetComponent<TextMeshProUGUI>().text = winnerName + " is the winner!";
        }

        StartCoroutine(ReturnToLobbyTimer());
    }

    [PunRPC]
    public void ReturnToLobby()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    private IEnumerator ReturnToLobbyTimer()
    {
        yield return new WaitForSeconds(4);
        photonView.RPC("ReturnToLobby", RpcTarget.All);
    }
}
