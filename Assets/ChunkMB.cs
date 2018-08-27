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

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
