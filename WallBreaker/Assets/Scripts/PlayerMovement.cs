using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    //Variables
    public float speed;
    public float teleportDis;

    [SerializeField]
    private int lives;

    private Vector3 position;
    private Vector3 teleportPos;
    private Vector3 initialPos;

    private List<MakeCubes> walls;
    private int currentWall;
    private bool changeWall;

    private bool collidingWall;
    private bool previousCollide;

    private int score;

    [SerializeField]
    private GameObject[] scenery;
    private GameObject currentScenery;
    private GameObject currentDecor;
    private int sceneryIndex;
    private int prevSceneryIndex;

    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Text liveText;

    [SerializeField]
    private GameObject fakeWall;
    private bool wallDestroyed;

    private UIManager manager;
    private float colorLerpSeconds = .5f;
    [SerializeField]
    private Material shaderMaterial;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UIManager>();

        wallDestroyed = false;
        collidingWall = false;

        initialPos = transform.position;

        position = gameObject.transform.position;
        teleportPos = new Vector3(transform.position.x, transform.position.y, teleportDis);

        // Store all the walls found in the scene
        walls = new List<MakeCubes>();
        for (int i = 0; i < 10; i++)
        {
            GameObject wall = GameObject.Find("Wall" + i.ToString());
            if (wall != null)
            {
                walls.Add(wall.GetComponent<MakeCubes>());
            }
            if (wall == null)
            {
                walls[i - 1].lastWall = true;
                break;
            }
        }
        currentWall = -1;
        changeWall = true;
        // run this once to display the first wall's pattern
        if (changeWall && currentWall != walls.Count - 1)
        {
            changeWall = false;
            currentWall++;
            walls[currentWall].DisplayPattern();
        }

        score = 0;
        sceneryIndex = 0;
        if (scenery != null)
        {
            currentDecor = Instantiate(scenery[sceneryIndex], Vector3.zero - new Vector3(0, 0, 128), Quaternion.identity);
            currentScenery = Instantiate(scenery[sceneryIndex], Vector3.zero, Quaternion.identity);
            currentScenery.transform.GetChild(0).gameObject.GetComponent<Light>().color = currentDecor.transform.GetChild(0).gameObject.GetComponent<Light>().color;
            prevSceneryIndex = sceneryIndex;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Constantly adjusts position
        position.z += (speed * Time.deltaTime);
        gameObject.transform.position = position;

        if (walls.Count > 0)
        {
            wallDestroyed = walls[currentWall].Destroyed();
            if (wallDestroyed && collidingWall)
            {
                StartCoroutine(SlowPlayerForSeconds(1));
                collidingWall = false;
            }

            if (changeWall && currentWall != walls.Count - 1)
            {
                changeWall = false;
                currentWall++;
                walls[currentWall].DisplayPattern();
            }

            if (walls[currentWall].wallSolved && currentWall != walls.Count - 1)
            {
                changeWall = true;
            }

            if (walls[walls.Count - 1].wallSolved)
                ResetScene();

            if (walls[0].wallSolved)
                fakeWall.SetActive(false);

            ChangeLives();
            previousCollide = collidingWall;
        }

        if (scoreText != null && liveText != null)
        {
            scoreText.text = "Score: " + score;
            liveText.text = "Lives: " + lives;
        }

        shaderMaterial.SetColor("_EdgeColor", currentScenery.transform.GetChild(0).gameObject.GetComponent<Light>().color);
        if(teleportDis == 384)
        {
            Vector3 distance = transform.position - teleportPos;
            if (distance.magnitude < 0.5f && distance.magnitude > -0.5f)
            {
                Teleport();
            }
        }

        //manager.score = score;
        
    }

    /// <summary>
    /// Teleports the player to the beginning of the chunk
    /// </summary>
    void Teleport()
    {
        if(teleportDis == 384)
        {
            position.z -= 514;
            gameObject.transform.position = position;
        }
        else
        {
            position.z -= teleportDis - (speed * Time.deltaTime);
            gameObject.transform.position = position;
            collidingWall = false;
        }
        
        
    }

    void ResetScene()
    {
        walls[walls.Count - 1].DoLastWall(fakeWall);
        score++;
        if (score % 2 == 0 && scenery != null)
        {
            // if all the scenery has been cycled 
            // through, loop back to the beginning
            prevSceneryIndex = sceneryIndex;
            if (sceneryIndex + 1 >= scenery.Length)
            {
                sceneryIndex = -1;
            }
            sceneryIndex++;
            Destroy(currentScenery);
            currentScenery = Instantiate(scenery[sceneryIndex], Vector3.zero, Quaternion.identity);
            currentScenery.transform.GetChild(0).gameObject.GetComponent<Light>().color = currentDecor.transform.GetChild(0).gameObject.GetComponent<Light>().color;
        }
        if (score % 2 == 1)
        {
            prevSceneryIndex = sceneryIndex;
        }
        Destroy(currentDecor);
        Debug.Log(score % 2 == 0 ? "yes" : "no");
        currentDecor = Instantiate(scenery[prevSceneryIndex], Vector3.zero - new Vector3(0, 0, 128), Quaternion.identity);
        if (score % 2 == 0)
        {
            //StartCoroutine(LerpScenes(currentDecor, currentScenery));
        }

        Teleport();
        for (int i = 0; i < walls.Count; i++)
        {
            walls[i].ResetWall((score + 1) % 2);
        }
        changeWall = true;
        currentWall = -1;
        // run this once to display the first wall's pattern
        if (changeWall && currentWall != walls.Count - 1)
        {
            changeWall = false;
            currentWall++;
            walls[currentWall].DisplayPattern();
        }
    }

    //Collision Detection
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("sceneChange"))
        {
            if (score % 2 == 0)
                StartCoroutine(LerpScenes(currentDecor, currentScenery));
            return;
        }
        collidingWall = true;
    }

    private void OnTriggerExit(Collider other)
    {
        collidingWall = false;
    }

    void ChangeLives()
    {
        if (collidingWall && !previousCollide)
        {
            if (lives <= 1)
            {
                manager.gameOver = true;
                manager.GameOver();
            }            
            else
                lives--;

            walls[currentWall].Solve();
        }
    }

    IEnumerator LerpScenes(GameObject prevscene, GameObject nextscene, float time = 0)
    {
        if (prevscene == null || nextscene == null)
            yield break;

        ChangeScenery nex = nextscene.transform.GetChild(1).gameObject.GetComponent<ChangeScenery>();
        ChangeScenery pre = prevscene.transform.GetChild(1).gameObject.GetComponent<ChangeScenery>();

        Color prev = pre.fogColor;
        Color next = nex.fogColor;
        Color prevl = pre.lightColor;
        Color nextl = nex.lightColor;
        Light prevLight = pre.light;
        Light nextLight = nex.light;

        nextLight.color = Color.Lerp(prevl, nextl, time);
        prevLight.color = Color.Lerp(prevl, nextl, time);

        RenderSettings.fogColor = Color.Lerp(prev, next, time);
        float oneSixtieth = Time.deltaTime;
        yield return new WaitForSeconds(oneSixtieth);
        time += oneSixtieth / colorLerpSeconds;
        if (time <= 1)
        {
            StartCoroutine(LerpScenes(prevscene, nextscene, time));
        }
        else
        {
            nextLight.color = Color.Lerp(prevl, nextl, 1);
            prevLight.color = Color.Lerp(prevl, nextl, 1);
        }
    }

    IEnumerator SlowPlayerForSeconds(float seconds)
    {
        float pastSpeed = speed;
        speed = 4;
        yield return new WaitForSeconds(seconds);
        Debug.Log(pastSpeed);
        speed = pastSpeed;
    }
}
