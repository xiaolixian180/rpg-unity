using UnityEngine;

namespace HeroQuest.Systems.World
{
    public sealed class TopDownPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private Transform visualRoot;

        private Vector2 moveInput;
        private bool inputEnabled;
        public Vector2 MoveInput => moveInput;
        public bool IsMoving => moveInput.sqrMagnitude > 0.0001f;

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!inputEnabled)
            {
                moveInput = Vector2.zero;
            }
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

            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
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
