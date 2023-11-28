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

    private void Start()
    {
        _playerNameText.text = photonView.Owner.NickName;
        _animator = this.GetComponent<Animator>();
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

                if (hit.collider.GetComponent<Target>() != null)
                {
                    hit.collider.gameObject.GetComponent<Target>().EliminateTarget();
                }
            }
        }
    }

    [PunRPC]
    public void CreateBulletEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(_bulletEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }
}
