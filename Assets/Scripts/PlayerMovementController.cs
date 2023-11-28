using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementController : MonoBehaviour
{
    private RigidbodyFPSController _rigidbodyFPSController;
    private Animator _animator;
    
    public bool _isMovementEnabled;

    private void Start()
    {
        _rigidbodyFPSController = this.GetComponent<RigidbodyFPSController>();
        _animator = this.GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (_isMovementEnabled)
        {
            _rigidbodyFPSController.mouseLook._lookInputAxis.x = Input.GetAxis("Mouse X");
            _rigidbodyFPSController.mouseLook._lookInputAxis.y = Input.GetAxis("Mouse Y");

            _animator.SetFloat("horizontal", Input.GetAxis("Horizontal"));
            _animator.SetFloat("vertical", Input.GetAxis("Vertical"));

            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.9 || Mathf.Abs(Input.GetAxis("Vertical")) > 0.9)
            {
                _animator.SetBool("isRunning", true);
                _rigidbodyFPSController.movementSettings.ForwardSpeed = 10;
            }
            else
            {
                _animator.SetBool("isRunning", false);
                _rigidbodyFPSController.movementSettings.ForwardSpeed = 5;
            }
        }
    }
}
