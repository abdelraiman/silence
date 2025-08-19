using Unity.VisualScripting;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    Astar astar;
    bool followpath = false;
    public float speed;
    Vector2 direction;
    int curentIndex = 0;
    float DistanceToCell = 0;
    void Start()
    {
        astar = FindAnyObjectByType<Astar>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("W");
            print(astar.Path.Count);

            if (astar.Path.Count > 0)
            {
                followpath = true;
            }
        }

        if (!followpath)
        {
            return;
        }
        Vector3 Astr = new Vector3(astar.Path[curentIndex].x, astar.Path[curentIndex].y, 0);
        direction = (Astr - transform.position).normalized;
        Vector3 moveby = direction * speed * Time.deltaTime;
        DistanceToCell = Vector3.Distance(transform.position, Astr);
        if (moveby.magnitude >= DistanceToCell)
        {
            curentIndex++;
            transform.position = Astr;
            if (curentIndex > astar.Path.Count - 1)
            {
                curentIndex = 0;
                followpath = false;
                return;
            }
        }
        transform.position += moveby;
    }
}
