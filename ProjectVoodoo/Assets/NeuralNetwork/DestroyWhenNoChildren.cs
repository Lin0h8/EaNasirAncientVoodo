using UnityEngine;

namespace NeuralNetwork_IHNMAIMS
{
    public class DestroyWhenNoChildren : MonoBehaviour
    {
        private void Update()
        {
            if (transform.childCount == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}