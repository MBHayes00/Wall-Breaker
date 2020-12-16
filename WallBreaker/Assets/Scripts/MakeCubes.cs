using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeCubes : MonoBehaviour
{
    // the cube prefab
    [SerializeField]
    private GameObject cube;

    // the wall array
    private int[,] cubemap;

    // the actual wall
    private CubeScript[,] wall;

    [SerializeField]
    private Material dissolveMaterial;

    public bool wallSolved;
    [SerializeField]
    private GameObject dissolveWall;
    public bool lastWall;

    // Start is called before the first frame update
    void Start()
    {
        wallSolved = false;

        int initial_size = 3;
        // start of the game is 3x3
        wall = new CubeScript[initial_size, initial_size];
        cubemap = new int[initial_size, initial_size];

        int size = wall.GetLength(0);
        float cubeDimension = cube.GetComponent<MeshRenderer>().bounds.size.x;
        float x = size % 2 == 0 ? (cubeDimension / 2f) - (cubeDimension * size / 2) : -(cubeDimension * (size - 1) / 2);
        float y = cubeDimension / 2f;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject temp = Instantiate(cube, transform.position + new Vector3(x + cubeDimension * j, y + cubeDimension * i, 0), Quaternion.identity);
                temp.transform.parent = transform;
                temp.name = "haha hey guys whats up";
                wall[i, j] = temp.GetComponent<CubeScript>();
            }
        }

        RandomWall();
        DisplayPattern();
    }

    // Update is called once per frame
    void Update()
    {
        bool solved = true;
        int size = wall.GetLength(0);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // if it is supposed to be activated and is not,
                // or if is supposed to not be activated and is
                if ((cubemap[i, j] == 1 && !wall[i, j].activated) ||
                    (cubemap[i, j] == 0 && wall[i, j].activated))
                {
                    // the puzzle was not solved
                    solved = false;
                }
            }
        }

        if (solved && !wallSolved)
        {
            wallSolved = true;
            // wall dissolves here
            List<GameObject> g = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                g.Add(transform.GetChild(i).gameObject);
                g[i].GetComponent<Renderer>().material = dissolveMaterial;
            }
            dissolveMaterial.SetFloat("Dissolvedness", 0);
            int midIndex = size % 2 == 0 ? size / 2 - 1 : size / 2;
            GameObject _g = wall[midIndex, midIndex].gameObject;
            Vector3 midWall = size % 2 == 0 ? _g.transform.position + new Vector3((_g.GetComponent<MeshRenderer>().bounds.size.x / 2f), (_g.GetComponent<MeshRenderer>().bounds.size.x / 2f), 0) : _g.transform.position;
            GameObject dissolveWallInstance = Instantiate(dissolveWall, midWall, Quaternion.identity);
            float desiredGirth = size * _g.GetComponent<MeshRenderer>().bounds.size.x;
            dissolveWallInstance.transform.localScale = new Vector3(desiredGirth, desiredGirth, 1);
            if (lastWall)
                dissolveWallInstance.GetComponent<DissolveWallScript>().decoyWall = GameObject.Find("dw");
            for (int i = transform.childCount - 1; i > -1; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            } 
        }
    }

    private void RandomWall()
    {
        int size = cubemap.GetLength(0);
        bool anything = false;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // is this a zero?
                float rng = Random.Range(0, 1f);
                if (rng > 0.75f)
                {
                    cubemap[i, j] = 1;
                    anything = true;
                }
            }
        }

        float rng1 = Random.Range(0, 1f);
        float rng2 = Random.Range(0, 1f);
        if (!anything)
        {
            cubemap[(int)(rng1 * size), (int)(rng2 * size)] = 1;
        }
    }

    public void ResetWall(int increment = 0)
    {
        wallSolved = false;
        int size = cubemap.GetLength(0) + increment;
        cubemap = new int[size, size];
        wall = new CubeScript[size, size];

        RandomWall();

        size = cubemap.GetLength(0);
        float cubeDimension = cube.GetComponent<MeshRenderer>().bounds.size.x;
        float x = size % 2 == 0 ? (cubeDimension / 2f) - (cubeDimension * size / 2) : -(cubeDimension * (size - 1) / 2);
        float y = cubeDimension / 2f;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject temp = Instantiate(cube, transform.position + new Vector3(x + cubeDimension * j, y + cubeDimension * i, 0), Quaternion.identity);
                temp.transform.parent = transform;
                wall[i, j] = temp.GetComponent<CubeScript>();
            }
        }
    }

    IEnumerator DisplayForSeconds(float seconds)
    {
        // highlight pattern
        int size = cubemap.GetLength(0);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                wall[i, j].displaying = true;
                if (cubemap[i, j] == 1)
                {
                    wall[i, j].gameObject.GetComponent<Renderer>().material = wall[i, j].clicked;
                }
            }
        }

        yield return new WaitForSeconds(seconds);

        // erase pattern
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                wall[i, j].gameObject.GetComponent<Renderer>().material = wall[i, j].notClicked;
                wall[i, j].displaying = false;
            }
        }
    }

    public void DisplayPattern()
    {
        StartCoroutine(DisplayForSeconds(1f));
    }

    public void Solve()
    {
        // solve the puzzle
        int size = cubemap.GetLength(0);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (cubemap[i, j] == 1)
                {
                    wall[i, j].activated = true;
                }
                else if (cubemap[i, j] == 0)
                {
                    wall[i, j].activated = false;
                }
            }
        }
    }

    public void DoLastWall(GameObject decoyWall)
    {
        decoyWall.SetActive(true);
        float desiredZ = decoyWall.transform.position.z;
        int size = cubemap.GetLength(0);
        dissolveMaterial.SetFloat("Dissolvedness", 0);
        int midIndex = size % 2 == 0 ? size / 2 - 1 : size / 2;
        GameObject _g = wall[midIndex, midIndex].gameObject;
        Vector3 midWall = size % 2 == 0 ? _g.transform.position + new Vector3((_g.GetComponent<MeshRenderer>().bounds.size.x / 2f), (_g.GetComponent<MeshRenderer>().bounds.size.x / 2f), 0) : _g.transform.position;
        midWall.z = desiredZ;
        decoyWall.transform.position = midWall;
        float desiredGirth = size * _g.GetComponent<MeshRenderer>().bounds.size.x;
        decoyWall.transform.localScale = new Vector3(desiredGirth, desiredGirth, 1);
    }

    public bool Destroyed()
    {
        return transform.childCount == 0;
    }
}
