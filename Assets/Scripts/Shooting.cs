using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera _camera;
    public GameObject _hitEffectPrefab;

    [Header("HP Related Stuff")]
    public float _startHealth = 100;
    private float _health;
    public Image _healthBar;

    [Header("Player Name Stuff")]
    public TextMeshProUGUI _playerNameText;

    private Animator _animator;
    private int _score;

    private void Start()
    {
        _health = _startHealth;
        _healthBar.fillAmount = _health / _startHealth;
        _playerNameText.text = photonView.Owner.NickName;
        _animator = this.GetComponent<Animator>();
        _score = 0;
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

                photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

                if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine
                        && hit.collider.gameObject.GetComponent<Shooting>()._healthBar.fillAmount > 0)
                {
                    hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 25);
                    
                    if (hit.collider.gameObject.GetComponent<Shooting>()._healthBar.fillAmount <= 0)
                    {
                        UpdateKillAmount();
                    }
                }
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        this._health -= damage;
        this._healthBar.fillAmount = _health / _startHealth;

        if (_health <= 0)
        {
            Die();
            photonView.RPC("ShowKillFeed", RpcTarget.All, info.Sender.NickName + " killed " + info.photonView.Owner.NickName);
            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);
        }
    }

    [PunRPC]
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(_hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }

    public void Die()
    {
        if (photonView.IsMine)
        {
            _animator.SetBool("isDead", true);
            StartCoroutine(RespawnCountdown());
        }
    }

    private IEnumerator RespawnCountdown()
    {
        GameObject respawnText = GameObject.Find("Respawn Text");
        float respawnTime = 5.0f;

        while (respawnTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            transform.GetComponent<PlayerMovementController>().enabled = false;
            respawnText.GetComponent<TextMeshProUGUI>().text = "You are killed. Respawning in " + respawnTime.ToString(".00");
        }

        _animator.SetBool("isDead", false);
        respawnText.GetComponent<TextMeshProUGUI>().text = "";

        this.transform.position = SpawnManager.Instance.SetSpawnLocation();
        transform.GetComponent<PlayerMovementController>().enabled = true;

        photonView.RPC("HideKillFeed", RpcTarget.All);
        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
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

    [PunRPC]
    public void RegainHealth()
    {
        _health = 100;
        _healthBar.fillAmount = _health / _startHealth;
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
