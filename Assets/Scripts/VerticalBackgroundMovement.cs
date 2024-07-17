using UnityEngine;

public class VerticalBackgroundMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;      
    [SerializeField] private float upperBound = 14f;    
    [SerializeField] private float lowerBound = -14f;

    void Start()
    {
        transform.position = new Vector2(transform.position.x, upperBound);
    }

    void Update()
    {
        transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);
        if (transform.position.y <= lowerBound)
        {
            transform.position = new Vector2(transform.position.x, upperBound);
        }
    }

}
