using UnityEngine;
using UnityEngine.InputSystem;

namespace AquariumShop
{
    public class InteractRaycast : MonoBehaviour
    {
        [SerializeField] float interactDistance = 2.5f;
        [SerializeField] LayerMask interactMask = ~0;

        Interactable _current;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.MenuOpen)
            {
                ClearPrompt();
                return;
            }

            _current = null;
            var ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out var hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
                _current = hit.collider.GetComponentInParent<Interactable>();

            gm.hud?.SetPrompt(_current != null ? $"E — {_current.prompt}" : string.Empty);

            if (_current != null && WasPressed(Key.E))
                gm.OpenInteract(_current.kind);

            if (WasPressed(Key.Tab))
                gm.ToggleStoreMenu();
        }

        static bool WasPressed(Key key)
        {
            var kb = Keyboard.current;
            return kb != null && kb[key].wasPressedThisFrame;
        }

        void ClearPrompt()
        {
            GameManager.Instance?.hud?.SetPrompt(string.Empty);
        }
    }
}
