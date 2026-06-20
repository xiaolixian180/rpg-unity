using UnityEngine;

namespace HeroQuest.Systems.World
{
    public sealed class TopDownPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private Transform visualRoot;

        private Vector2 moveInput;
        private bool inputEnabled;
        private Vector3? clickMoveTarget;

        public Vector2 MoveInput => moveInput;
        public bool IsMoving => moveInput.sqrMagnitude > 0.0001f;

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!inputEnabled)
            {
                moveInput = Vector2.zero;
                clickMoveTarget = null;
            }
        }

        public void SetClickMoveTarget(Vector3 worldPos)
        {
            clickMoveTarget = worldPos;
        }

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform.Find("VisualRoot");
            }
        }

        private void Start()
        {
            PrototypeGameplayFlow.Ensure(this);
        }

        private void Update()
        {
            if (!inputEnabled)
            {
                moveInput = Vector2.zero;
                return;
            }

            // WASD 移动（WoW 风格）
            float mx = 0f;
            if (Input.GetKey(KeyCode.D)) mx += 1f;
            if (Input.GetKey(KeyCode.A)) mx -= 1f;
            float my = 0f;
            if (Input.GetKey(KeyCode.W)) my += 1f;
            if (Input.GetKey(KeyCode.S)) my -= 1f;
            moveInput = new Vector2(mx, my);

            // 方向键有输入时取消点击移动
            if (moveInput.sqrMagnitude > 0.001f)
            {
                clickMoveTarget = null;
            }

            // 鼠标左键点击移动（未点击怪物时走地面）
            if (Input.GetMouseButtonDown(0) && clickMoveTarget.HasValue)
            {
                // clickMoveTarget 已由 PrototypeGameplayFlow 设置
            }

            // 右键按住时朝鼠标方向转身
            if (Input.GetMouseButton(1))
            {
                var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var dir = mouseWorld - transform.position;
                if (visualRoot != null && Mathf.Abs(dir.x) > 0.01f)
                {
                    var scale = visualRoot.localScale;
                    scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dir.x);
                    visualRoot.localScale = scale;
                }
            }

            // 点击移动：向目标位置移动
            if (clickMoveTarget.HasValue)
            {
                var diff = clickMoveTarget.Value - transform.position;
                diff.z = 0f;
                if (diff.magnitude < 0.1f)
                {
                    clickMoveTarget = null;
                    moveInput = Vector2.zero;
                }
                else
                {
                    moveInput = diff.normalized;
                }
            }

            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            if (visualRoot != null && Mathf.Abs(moveInput.x) > 0.01f)
            {
                var scale = visualRoot.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput.x);
                visualRoot.localScale = scale;
            }
        }

        private void FixedUpdate()
        {
            transform.position += (Vector3)(moveInput * (moveSpeed * Time.fixedDeltaTime));
        }
    }
}
