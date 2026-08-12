using UnityEngine;

namespace CIS2991Project.Player
{
    // Placeholder for skills, progression, and quest state; expand as those systems land.
    public class CharacterSheet : MonoBehaviour
    {
        public string PreviousScene { get; set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
