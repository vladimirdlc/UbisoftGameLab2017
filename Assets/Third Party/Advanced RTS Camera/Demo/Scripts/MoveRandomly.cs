using UnityEngine;
using System.Collections;

public class MoveRandomly : MonoBehaviour
{

    CharacterController cont;
    Vector3 moveLocation;
    float lastTime;

    float moveTime = 4.5f;
    float moveDistance = 100f;
    public float speed = 10f;

    // Use this for initialization
    void Start()
    {
        cont = GetComponent<CharacterController>();
        lastTime = Time.time + moveTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastTime > moveTime)
        {
            FindNewLocation();
            lastTime = Time.time;
        }
        Move();
    }

    void Move()
    {
        cont.Move( (((moveLocation - transform.position).normalized * speed) + new Vector3(0, -10, 0)) * Time.deltaTime);
    }

    void FindNewLocation()
    {
        RaycastHit hit;
        while (true)
        {
            if (Physics.Raycast(new Ray(new Vector3(transform.position.x + Random.Range(-moveDistance, moveDistance), 650, transform.position.z + Random.Range(-moveDistance, moveDistance)), -Vector3.up), out hit, Mathf.Infinity))
            {
                moveLocation = hit.point;
                return;
            }
        }

    }
}
