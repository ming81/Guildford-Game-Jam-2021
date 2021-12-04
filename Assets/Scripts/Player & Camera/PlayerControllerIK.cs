using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine;

namespace Bleak.Controller
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerControllerIK : MonoBehaviour
    {
        // REFERENCES
        public Animator _anim;
        public CharacterController _characterController;

        // VARIABLES
        //  public GameObject equippedWeapon;
        // public GameObject displayWeapon;
        public GameObject playerGfx;
        // public string characterRecipe;
        //public DynamicCharacterAvatar avatar;
        public bool isAttacking;
        public Interactable focus;

        // CAMERA
        public Camera playerCamera;
        public bool cameraEnabled = true;
        public bool initCameraOnSpawn = true;
        public string cameraName = "Main Camera";
        public Vector3 cameraPositionOffset = new Vector3(0, 10, 0);
        public Vector3 cameraRotationOffset = new Vector3(45, 0, 0);
        public float cameraDampTime = 0.1f;
        public float offsetDampTime = 0.25f;
        public float maxOffset = 0.3f;
        public bool isDraggable = true;
        public float minCameraHeight = 3,
            maxCameraHeight = 15,
            minCameraVertical = 0.5f,
            maxCameraVertical = 0.5f,
            cameraZoomSpeed = 15,
            cameraZoomPower = 5;
        private float _currentCameraHeight, _cameraHeightTarget, _currentCameraVertical, _cameraVerticalTarget;
        private Vector3 _forward;
        private Vector3 _cameraVelocity;
        private Vector3 _dampedCameraPosition;
        private Vector2 _dampedOffsetVector;
        private Vector2 _currentDistanceVectorVelocity;
        private float _dragAngle;
        private bool _isDragging;

        private Vector2 currentInputVector;
        private Vector2 smoothInputVelocity;
        private float smoothInputSpeed = 0.2f;
        private Vector3 lastPos;

        // NAVIGATION
        public bool movementEnabled = true;
        public bool interactEnabled = true;
        public bool combatEnabled = true;
        public float jumpHeight = 4;
        public float gravity = 10;

        // SPEED
        private float speed;
        public float moveSpeed = 4.5f;
        private float walkSpeed = 1.5f;
        private float sneakSpeed = 1.5f;
        private float animSpeedMod = 1;

        // INPUT
        public KeyCode moveUpKey = KeyCode.W,
            moveDownKey = KeyCode.S,
            moveLeftKey = KeyCode.A,
            moveRightKey = KeyCode.D,
            jumpKey = KeyCode.Space,
            walkKey = KeyCode.LeftShift,
            sneakKey = KeyCode.LeftControl,
            interactKey = KeyCode.F;

        // Attack Variables
        bool comboPossible;
        int comboStep;
        bool isBlocking = false;
        public bool weaponDrawn = false;

        public enum MovementInputType
        {
            Keyboard = 0
        }
        public MovementInputType movementInputType;

        // IK
        public float bodyWeightIK = 0.5f;
        public float headWeightIK = 1.0f;
        public float ikWeight = 1.0f;
        public float eyeWeight = 1.0f;
        public float dampSmoothTimeIK = 0.4f;
        public float dampSmoothTimeRotation = 0.25f;
        
        // ANIMATOR 
        static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
        static readonly int VerticalHash = Animator.StringToHash("Vertical");
        static readonly int FallingHash = Animator.StringToHash("Falling");
        static readonly int JumpHash = Animator.StringToHash("Jump");
        public float animatorSmoothTime = 0.1f;

        // OTHER
        private float _deltaAngle;
        private Vector3 _displacement;
        private bool _isJumping;
        private float _deltaAngleVelocity;
        private float _angularVelocity;
        private float _verticalSpeed;
        private float _forwardF = 180.0f;
        private float _lookAngle = 180.0f;
        private float _actualMoveSpeed;
        private float _targetAngle = 180.0f;
        private float _rotationAngle = 180.0f;
        private float _targetRotationAngle = 180.0f;
        private Vector2? _targetPosition;

        private void Awake()
        {
            _actualMoveSpeed = moveSpeed;
            InitReferences();
            playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            // avatar = GetComponentInChildren<DynamicCharacterAvatar>();

            if (cameraEnabled)
            {
                InitCameraValues();
                InitCamera();
            }
        }

        void Update()
        {
            if (interactEnabled)
                Interact();

            if (movementEnabled)
                HandleMovement();

            //  if (cameraEnabled) CameraLogic();
        }

        void Interact()
        {
            bool sleeping = _anim.GetBool("Sleeping");

            if (Input.GetKeyDown(interactKey))
            {
                Ray cameraRay = playerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(cameraRay, out hit, 100))
                {
                    Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
                    if (interactable != null)
                    {
                        SetFocus(interactable);
                    }
                }
            }
        }
        void SetFocus(Interactable newFocus)
        {
            if (newFocus != focus)
            {
                if (focus != null)
                    focus.OnDefocused();
                focus = newFocus;
            }

            newFocus.OnFocused(transform);
        }

        void RemoveFocus()
        {
            if (focus != null)
                focus.OnDefocused();
            focus = null;
        }

        private void LateUpdate()
        {
            if (cameraEnabled) LateCameraUpdate();
        }

        private void InitReferences()
        {
            _anim = GetComponent<Animator>();
            _characterController = GetComponent<CharacterController>();
        }

        private void LateCameraUpdate()
        {
            if (isDraggable)
            {
                if (Input.GetMouseButtonDown(2))
                {
                    Vector3 point = GetPoint();
                    if (Vector3.Distance(point, transform.position) > 3f)
                    {
                        Vector3 targetCameraPosition = transform.position - _forward * _currentCameraHeight;

                        _isDragging = true;

                        _dragAngle =
                            Mathf.Atan2(transform.position.z - point.z, point.x - transform.position.x) *
                            Mathf.Rad2Deg + 90;

                        _dampedOffsetVector =
                            new Vector2(_dampedCameraPosition.x - targetCameraPosition.x + _dampedOffsetVector.x,
                                _dampedCameraPosition.z - targetCameraPosition.z + _dampedOffsetVector.y);
                    }
                }
                else if (Input.GetMouseButton(2))
                {
                    Vector3 point = GetPoint();
                    if (_isDragging && Vector3.Distance(point, transform.position) > 0.25f)
                    {
                        float dragAngle =
                            Mathf.Atan2(transform.position.z - point.z, point.x - transform.position.x) *
                            Mathf.Rad2Deg + 90;

                        float deltaAngle = _dragAngle - dragAngle;
                        Vector3 eulerAngles = playerCamera.transform.rotation.eulerAngles;
                        _deltaAngle = deltaAngle;
                        float targetAngle = eulerAngles.y + deltaAngle;
                        targetAngle = Mathf.MoveTowardsAngle(eulerAngles.y, targetAngle, Time.deltaTime * 360f);

                        playerCamera.transform.rotation =
                            Quaternion.Euler(eulerAngles.x, targetAngle, eulerAngles.z);

                        _forward = playerCamera.transform.forward;
                    }
                }
            }

            if (Input.GetMouseButtonUp(2))
            {
                _isDragging = false;
            }

            if (!_isDragging)
            {
                Vector3 point = GetPoint();
                Vector2 distanceToPlayer = new Vector2(point.x, point.z) -
                                           new Vector2(transform.position.x, transform.position.z);

                distanceToPlayer = Vector2.ClampMagnitude(distanceToPlayer, maxOffset);
                _dampedOffsetVector = Vector2.SmoothDamp(_dampedOffsetVector, distanceToPlayer,
                    ref _currentDistanceVectorVelocity, offsetDampTime);

                _dampedCameraPosition = Vector3.SmoothDamp(_dampedCameraPosition,
                    new Vector3(transform.position.x + cameraPositionOffset.x, transform.position.y, transform.position.z + _currentCameraVertical) - _forward * _currentCameraHeight, ref _cameraVelocity, cameraDampTime);
            }
            else
            {
                _dampedCameraPosition = transform.position - _forward * _currentCameraHeight;
            }

            playerCamera.transform.position = _dampedCameraPosition + new Vector3(_dampedOffsetVector.x, 0, _dampedOffsetVector.y);
        }

        private void CameraInputs()
        {
            HandleCameraZoom();
        }
        
        private void CameraLogic()
        {
            if (!cameraEnabled) return;
            CameraInputs();
            LerpCameraHeight();
        }
        
        private void LerpCameraHeight()
        {
            _currentCameraHeight = Mathf.Lerp(_currentCameraHeight, _cameraHeightTarget, Time.deltaTime * cameraZoomSpeed);
            _currentCameraVertical = Mathf.Lerp(_currentCameraVertical, _cameraVerticalTarget, Time.deltaTime * cameraZoomSpeed);
        }
        
        private void HandleCameraZoom()
        {
            if (Input.mouseScrollDelta.y == 0) return;
            float heightDifference = Input.mouseScrollDelta.y < 0f ? cameraZoomPower : -cameraZoomPower;
            _cameraHeightTarget = _currentCameraHeight + heightDifference;
            _cameraVerticalTarget = _currentCameraVertical + heightDifference;
            if (_cameraHeightTarget > maxCameraHeight) _cameraHeightTarget = maxCameraHeight;
            else if (_cameraHeightTarget < minCameraHeight) _cameraHeightTarget = minCameraHeight;
            if (_cameraVerticalTarget > maxCameraVertical) _cameraVerticalTarget = maxCameraVertical;
            else if (_cameraVerticalTarget < minCameraVertical) _cameraVerticalTarget = minCameraVertical;
        }
        
        private void InitCamera()
        {
            if (!initCameraOnSpawn && playerCamera != null) return;
            Camera cam = GameObject.Find(cameraName).GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError(
                    "TOPDOWN_WASD_CONTROLLER: NO CAMERA FOUND! MAKE SURE TO EITHER DRAG AND DROP ONE, OR ENABLE INIT CAMERA AND TYPE A VALID CAMERA NAME");
            }
            else
            {
                playerCamera = cam;
            }

            if (playerCamera == null) return;
            playerCamera.transform.eulerAngles = cameraRotationOffset;
            _forward = playerCamera.transform.forward;
            InstantCameraUpdate();
        }
        
        private void InitCameraValues()
        {
            _currentCameraHeight = cameraPositionOffset.y;
            _cameraHeightTarget = _currentCameraHeight;
            _currentCameraVertical = cameraPositionOffset.z;
            _cameraVerticalTarget = _currentCameraVertical;
        }
        
        void InstantCameraUpdate()
        {
            Vector3 targetPos = transform.position - (playerCamera.transform.forward * _currentCameraHeight);
            targetPos.z -= _currentCameraVertical;
            _dampedCameraPosition = targetPos;
            playerCamera.transform.position = targetPos;
        }

        private void HandleMovement()
        {
            _displacement.y = 0;

            if (_characterController.isGrounded)
            {
                _verticalSpeed = 0f;
                _isJumping = false;
                Vector2 input = GetMovementInput();
                currentInputVector = Vector2.SmoothDamp(currentInputVector, input, ref smoothInputVelocity, smoothInputSpeed);

                if (Input.GetKeyDown(jumpKey))
                {
                    input = Vector2.zero;
                    _targetPosition = null;
                    _verticalSpeed = jumpHeight;
                    _anim.SetTrigger(JumpHash);
                    _isJumping = true;
                }

                _anim.SetBool(FallingHash, false);
                
                if (Input.GetKey(walkKey) || isBlocking == true)
                {
                    _actualMoveSpeed = walkSpeed;
                    animSpeedMod = walkSpeed / 3;
                }
                else
                {
                    _actualMoveSpeed = moveSpeed;
                    animSpeedMod = 1;
                }
                
                input.Normalize();
                input = Rotate(currentInputVector, Direction(playerCamera.transform.rotation.eulerAngles.y * Mathf.Deg2Rad)) * (_actualMoveSpeed * Time.deltaTime);

                Vector3 point = GetPoint();
                if (_targetPosition is Vector2 value)
                {
                    Vector2 displacementToTarget = value - new Vector2(transform.position.x, transform.position.z);
                    input = Vector2.ClampMagnitude(displacementToTarget, _actualMoveSpeed * Time.deltaTime);
                }

                if (currentInputVector.magnitude > 0.3f)
                {
                    _forwardF = Mathf.Atan2(-input.y, input.x) * Mathf.Rad2Deg + 90;

                    _displacement.x = input.x;
                    _displacement.z = input.y;

                    _anim.SetLayerWeight(1, 1);
                    _anim.SetLayerWeight(2, 0);
                }
                else
                {
                    if (!_isJumping)
                    {
                        _displacement.x = 0.0f;
                        _displacement.z = 0.0f;
                    }

                    _anim.SetLayerWeight(1, 0);
                    _anim.SetLayerWeight(2, 1);
                }

                _lookAngle =
                    Mathf.Atan2(transform.position.z - point.z, point.x - transform.position.x) * Mathf.Rad2Deg + 90;

                if (currentInputVector.magnitude > 0.3f)
                {
                    float deltaAngle = Mathf.DeltaAngle(_lookAngle, _forwardF);
                    float differenceAngle = Mathf.Round(deltaAngle / 45) * 45;

                    _targetRotationAngle = _forwardF - differenceAngle;

                    float horizontal = Mathf.Round(Mathf.Sin(differenceAngle * Mathf.Deg2Rad));
                    float vertical = Mathf.Round(Mathf.Cos(differenceAngle * Mathf.Deg2Rad));

                    _anim.SetFloat(HorizontalHash, horizontal * animSpeedMod, animatorSmoothTime, Time.deltaTime);
                    _anim.SetFloat(VerticalHash, vertical * animSpeedMod, animatorSmoothTime, Time.deltaTime);
                    
                    // Debug.Log(input);
                }
                else
                {
                    float deltaAngle = Mathf.DeltaAngle(_forwardF, _lookAngle);

                    if (deltaAngle < -90)
                    {
                        _targetRotationAngle = _forwardF - 90f;
                        _forwardF = _forwardF - 90f;
                    }

                    if (deltaAngle > 90)
                    {
                        _targetRotationAngle = _forwardF + 90f;
                        _forwardF = _forwardF + 90f;
                    }

                    _anim.SetFloat(HorizontalHash, 0, animatorSmoothTime, Time.deltaTime);
                    _anim.SetFloat(VerticalHash, 0, animatorSmoothTime, Time.deltaTime);
                }

                if (!Mathf.Approximately(_rotationAngle, _targetRotationAngle))
                {
                    _rotationAngle = Mathf.SmoothDampAngle(_rotationAngle, _targetRotationAngle,
                        ref _angularVelocity,
                        dampSmoothTimeRotation);

                    transform.rotation = Quaternion.Euler(0, _rotationAngle, 0);
                }
            }
            else
            {
                _targetPosition = null;

                Vector3 point = GetPoint();
                _lookAngle = Mathf.Atan2(transform.position.z - point.z, point.x - transform.position.x) *
                    Mathf.Rad2Deg + 90;

                _isJumping = true;
                if (GetGroundDistance() > 0.2f) _anim.SetBool(FallingHash, true);
                else _anim.SetBool(FallingHash, false);
            }

            if (_characterController.isGrounded && !_isJumping)
            {
                _displacement.y = -gravity * Time.deltaTime;
            }
            else
            {
                _displacement.y = _verticalSpeed * Time.deltaTime;
            }

            _verticalSpeed -= gravity * Time.deltaTime;

            _characterController.Move(_displacement);

            lastPos = transform.position;
        }

        private float GetGroundDistance()
        {
            if (Physics.Raycast (transform.position, -Vector3.up, out var hit)) {
                return hit.distance;
            }
            return 0;
        }

        private Vector2 GetMovementInput()
        {
            Vector2 v2Input = new Vector2();
            if (movementInputType == MovementInputType.Keyboard)
            {
                if (Input.GetKey(moveUpKey))
                {
                    v2Input.y = 1;
                }

                if (Input.GetKey(moveDownKey))
                {
                    v2Input.y -= 1;
                }

                if (Input.GetKey(moveLeftKey))
                {
                    v2Input.x -= 1;
                }

                if (Input.GetKey(moveRightKey))
                {
                    v2Input.x = 1;
                }

                _targetPosition = null;
            }

            return v2Input;
        }

        static Vector2 Rotate(Vector2 self, Vector2 other)
        {
            return new Vector2(self.x * other.x - self.y * other.y, self.x * other.y + other.x * self.y);
        }

        static Vector2 Direction(float angle)
        {
            return new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle));
        }

        private Vector3 GetPoint()
        {
            var playerPlane = new Plane(Vector3.up, transform.position);
            var ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            return playerPlane.Raycast(ray, out var hitDist) ? ray.GetPoint(hitDist) : Vector3.zero;
        }

        void OnAnimatorIK()
        {
            _anim.SetLookAtWeight(ikWeight, bodyWeightIK, headWeightIK, eyeWeight);

            float targetDeltaAngle = Mathf.Clamp(Mathf.DeltaAngle(_rotationAngle, _lookAngle), -60, 60);

            _deltaAngle =
                Mathf.SmoothDampAngle(_deltaAngle, targetDeltaAngle, ref _deltaAngleVelocity, dampSmoothTimeIK);

            _targetAngle = (_rotationAngle + _deltaAngle - 90) * Mathf.Deg2Rad;

            Vector3 targetLookAt = transform.position +
                                   new Vector3(Mathf.Cos(_targetAngle) * 10, 0, Mathf.Sin(_targetAngle) * -10);

            Transform headTransform = playerGfx.transform.Find("metarig/spine/spine.001/spine.002/spine.003/spine.004");

            targetLookAt.y = headTransform.position.y;

            _anim.SetLookAtPosition(targetLookAt);
        }
    }
}
