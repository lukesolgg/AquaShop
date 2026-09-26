using UnityEngine;

namespace AquariumShop
{
    public class Interactable : MonoBehaviour
    {
        public enum Kind { Tank, Counter }

        public Kind kind = Kind.Tank;
        public string prompt = "Tank";
    }
}
