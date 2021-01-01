using UnityEngine;

public class CameraController : MonoBehaviour
{

    /*Use WASD + Mouse to move around*/
    float mouseSpeed = 0.25f;
    private Vector3 currMousePos = new Vector3(255, 255, 255);

    // Update is called once per frame
    void Update()
    {
        currMousePos = Input.mousePosition - currMousePos;
        currMousePos = new Vector3(-currMousePos.y * mouseSpeed, currMousePos.x * mouseSpeed, 0);
        transform.eulerAngles = currMousePos = new Vector3(transform.eulerAngles.x + currMousePos.x, transform.eulerAngles.y + currMousePos.y, 0);
        currMousePos = Input.mousePosition;

        Vector3 newPos = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            newPos += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            newPos += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            newPos += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            newPos += new Vector3(1, 0, 0);
        }
        transform.Translate(newPos * 100.0f * Time.deltaTime); //100 is the movement speed
    }
}
