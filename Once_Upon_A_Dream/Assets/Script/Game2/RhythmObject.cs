using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RhythmObject : MonoBehaviour
{
    //[SerializeField] float Speed = 3;

    [SerializeField] bool isRhythmTile;


    public float PressTime;

    public List<GameObject> excute;

    bool success;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isRhythmTile)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (GameManager_game2.instance.TileSpeed * Time.deltaTime), 0);

            if (transform.position.y <= -6f)
            {
                Destroy(gameObject);
            }
        }


        if (!GameManager_game2.instance.isGameStart) return;

        if (!isRhythmTile)
        {
            // 아무것도 닿지 않았는데 버튼을 누르면
            if (Input.GetKeyDown(GameManager_game2.instance.InteractionKey) && excute.Count <= 0 && !success)
            {
                GameManager_game2.instance.PlayerPoints[int.Parse(transform.parent.GetComponent<PlayerController>().role) - 1]--;
            }

            if (Input.GetKey(GameManager_game2.instance.InteractionKey))
            {
                PressTime += Time.deltaTime * 100;
            }
            else if (Input.GetKeyUp(GameManager_game2.instance.InteractionKey))
            {
                PressTime = 0;
            }
        }

    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (Input.GetKey(GameManager_game2.instance.InteractionKey))
        {

            if (collision != null)
            {
                if (!isRhythmTile)
                {
                    if (collision.tag == "RhythmTile")
                    {
                        
                        if (PressTime <= 3)
                        {
                            StartCoroutine(SuccessSet());
                            GameManager_game2.instance.PlayerPoints[int.Parse(transform.parent.GetComponent<PlayerController>().role) - 1]++;
                            Destroy(collision.gameObject);
                        }

                    }
                }

            }

        }

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (!isRhythmTile)
            {
                if (collision.tag == "RhythmTile")
                {
                    excute.Add(collision.gameObject);
                }
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (!isRhythmTile)
            {
                if (collision.tag == "RhythmTile")
                {
                    excute.Remove(collision.gameObject);
                }
            }
        }
    }




    IEnumerator SuccessSet()
    {
        success = true;
        yield return new WaitForSeconds(0.1f);
        success = false;
    }
}
