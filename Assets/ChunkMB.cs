using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMB : MonoBehaviour
{
    Chunk owner;

    public ChunkMB() { }

    public void SetOwner(Chunk o)
    {
        owner = o;
        InvokeRepeating("SaveProgress", 10, 1000);
    }

    public IEnumerator HealBlock(Vector3 bpos)
    {
        yield return new WaitForSeconds(3);
        var x = (int)bpos.x;
        var y = (int)bpos.y;
        var z = (int)bpos.z;

        if (owner.chunkData[x, y, z].bType != Block.BlockType.AIR)
        {
            owner.chunkData[x, y, z].Reset();
        }
    }

    public IEnumerator Flow(Block b, Block.BlockType bt, int strength, int maxsize)
    {
        if (maxsize <= 0) yield break;
        if (b == null) yield break;
        if (strength <= 0) yield break;
        if (b.bType != Block.BlockType.AIR) yield break;
        b.SetType(bt);
        b.currentHealth = strength;
        b.owner.Redraw();
        yield return new WaitForSeconds(1);

        var x = (int)b.position.x;
        var y = (int)b.position.y;
        var z = (int)b.position.z;

        var below = b.GetBlock(x, y - 1, z);
        if (below != null && below.bType == Block.BlockType.AIR)
        {
            StartCoroutine(Flow(b.GetBlock(x, y - 1, z), bt, strength, --maxsize));
            yield break;
        }
        else
        {
            --strength;
            --maxsize;
            World.queue.Run(Flow(b.GetBlock(x - 1, y, z), bt, strength, maxsize));
            yield return new WaitForSeconds(1);
            World.queue.Run(Flow(b.GetBlock(x + 1, y, z), bt, strength, maxsize));
            yield return new WaitForSeconds(1);
            World.queue.Run(Flow(b.GetBlock(x, y, z + 1), bt, strength, maxsize));
            yield return new WaitForSeconds(1);
            World.queue.Run(Flow(b.GetBlock(x, y, z - 1), bt, strength, maxsize));
            yield return new WaitForSeconds(1);
        }
    }

    void SaveProgress()
    {
        if (owner.changed)
        {
            owner.Save();
            owner.changed = false;
        }
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
