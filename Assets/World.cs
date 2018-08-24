using Realtime.Messaging.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class World : MonoBehaviour
{

    public GameObject player;
    public Material textureAtlas;
    public static int columnHeight = 16;
    public static int chunkSize = 16;
    public static int worldSize = 1;
    public static int radius = 4;
    public static ConcurrentDictionary<string, Chunk> chunks;
    public static bool firstbuild = true;
    public static List<string> toRemove = new List<string>();

    CoroutineQueue queue;
    public static uint maxCoroutines = 1000;

    public Vector3 lastBuildPos;

    public static string BuildChunkName(Vector3 v)
    {
        return (int)v.x + "_" +
                     (int)v.y + "_" +
                     (int)v.z;
    }

    void BuildChunkAt(int x, int y, int z)
    {
        Vector3 chunkPosition = new Vector3(x * chunkSize,
                                            y * chunkSize,
                                            z * chunkSize);

        string n = BuildChunkName(chunkPosition);
        Chunk c;

        if (!chunks.TryGetValue(n, out c))
        {
            c = new Chunk(chunkPosition, textureAtlas);
            c.chunk.transform.parent = this.transform;
            chunks.TryAdd(c.chunk.name, c);
        }

    }

    IEnumerator BuildRecursiveWorld(int x, int y, int z, int rad)
    {
        rad--;

        if (rad == 0)
        {
            yield break;
        }

        BuildChunkAt(x, y, z + 1);
        queue.Run(BuildRecursiveWorld(x, y, z + 1, rad));
        yield return null;

        BuildChunkAt(x, y, z - 1);
        queue.Run(BuildRecursiveWorld(x, y, z - 1, rad));
        yield return null;

        BuildChunkAt(x - 1, y, z);
        queue.Run(BuildRecursiveWorld(x - 1, y, z, rad));
        yield return null;

        BuildChunkAt(x + 1, y, z);
        queue.Run(BuildRecursiveWorld(x + 1, y, z, rad));
        yield return null;

        BuildChunkAt(x, y - 1, z);
        queue.Run(BuildRecursiveWorld(x, y - 1, z, rad));
        yield return null;

        BuildChunkAt(x, y + 1, z);
        queue.Run(BuildRecursiveWorld(x, y + 1, z, rad));
        yield return null;
    }

    IEnumerator DrawChunks()
    {
        foreach (KeyValuePair<string, Chunk> c in chunks)
        {
            if (c.Value.status == Chunk.ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
            }
            if (c.Value.chunk && Vector3.Distance(player.transform.position,
                c.Value.chunk.transform.position) > radius * chunkSize)
            {
                toRemove.Add(c.Key);
            }
            yield return null;
        }
    }

    IEnumerator RemoveOldChunks()
    {
        for (var i = 0; i < toRemove.Count; i++)
        {
            var n = toRemove[i];
            Chunk c;
            if (chunks.TryGetValue(n, out c))
            {
                Destroy(c.chunk);
                c.Save();
                chunks.TryRemove(n, out c);
                yield return null;
            }
        }
    }

    public void BuildNearPlayer()
    {
        StopCoroutine("BuildRecursiveWorld");
        queue.Run(BuildRecursiveWorld((int)player.transform.position.x / chunkSize,
            (int)player.transform.position.y / chunkSize,
            (int)player.transform.position.z / chunkSize,
            radius));
    }

    // Use this for initialization
    void Start()
    {
        Vector3 ppos = player.transform.position;
        player.transform.position = new Vector3(ppos.x,
                                            Utils.GenerateHeight(ppos.x, ppos.z) + 1,
                                            ppos.z);
        lastBuildPos = player.transform.position;
        player.SetActive(false);

        firstbuild = true;
        chunks = new ConcurrentDictionary<string, Chunk>();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        queue = new CoroutineQueue(maxCoroutines, StartCoroutine);

        //build starting chunk
        BuildChunkAt((int)(player.transform.position.x / chunkSize),
                                            (int)(player.transform.position.y / chunkSize),
                                            (int)(player.transform.position.z / chunkSize));
        //draw it
        queue.Run(DrawChunks());

        //create a bigger world
        queue.Run(BuildRecursiveWorld((int)(player.transform.position.x / chunkSize),
                                            (int)(player.transform.position.y / chunkSize),
                                            (int)(player.transform.position.z / chunkSize), radius));
    }

    // Update is called once per frame
    void Update()
    {
        var movement = lastBuildPos - player.transform.position;
        if (movement.magnitude > chunkSize)
        {
            lastBuildPos = player.transform.position;
            BuildNearPlayer();
        }

        if (!player.activeSelf)
        {
            player.SetActive(true);
            firstbuild = false;
        }
        queue.Run(DrawChunks());
        queue.Run(RemoveOldChunks());
    }
}
