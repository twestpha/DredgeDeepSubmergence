using UnityEngine;

namespace DeepSubmergence {
    public class Loader {

        // This method is run by Winch to initialize your mod
        public static void Initialize()
        {
            var gameObject = new GameObject(nameof(DeepSubmergence));
            gameObject.AddComponent<DeepSubmergence>();
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }
}