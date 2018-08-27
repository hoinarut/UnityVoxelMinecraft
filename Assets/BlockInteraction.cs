using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInteraction : MonoBehaviour
{

    public GameObject cam;
    private Block.BlockType selectedBlockType;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            selectedBlockType = Block.BlockType.SAND;            
        }
        if (Input.GetKeyDown("2"))
        {
            selectedBlockType = Block.BlockType.STONE;            
        }
		if (Input.GetKeyDown("3"))
        {
            selectedBlockType = Block.BlockType.DIAMOND;            
        }
		if (Input.GetKeyDown("4"))
        {
            selectedBlockType = Block.BlockType.REDSTONE;            
        }
		if (Input.GetKeyDown("5"))
        {
            selectedBlockType = Block.BlockType.GOLD;            
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;

            //for mouse clicking
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            //if ( Physics.Raycast (ray,out hit,10)) 
            //{

            //for cross hairs
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 10))
            {
                Chunk hitc;
                if (!World.chunks.TryGetValue(hit.collider.gameObject.name, out hitc))
                {
                    return;
                }

                Vector3 hitBlock;

                if (Input.GetMouseButtonDown(0))
                {
                    hitBlock = hit.point - hit.normal / 2.0f;
                }
                else
                {
                    hitBlock = hit.point + hit.normal / 2.0f;
                }

                var b = World.GetWorldBlock(hitBlock);
                hitc = b.owner;

                var update = false;
                if (Input.GetMouseButtonDown(0))
                {
                    update = b.HitBlock();
                }
                else
                {
                    update = b.BuildBlock(selectedBlockType);
                }

                if (update)
                {
                    hitc.changed = true;
                    List<string> updates = new List<string>();
                    float thisChunkx = hitc.chunk.transform.position.x;
                    float thisChunky = hitc.chunk.transform.position.y;
                    float thisChunkz = hitc.chunk.transform.position.z;

                    //updates.Add(hit.collider.gameObject.name);

                    //update neighbours?
                    if (b.position.x == 0)
                        updates.Add(World.BuildChunkName(new Vector3(thisChunkx - World.chunkSize, thisChunky, thisChunkz)));
                    if (b.position.x == World.chunkSize - 1)
                        updates.Add(World.BuildChunkName(new Vector3(thisChunkx + World.chunkSize, thisChunky, thisChunkz)));
                    if (b.position.y == 0)
                        updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky - World.chunkSize, thisChunkz)));
                    if (b.position.y == World.chunkSize - 1)
                        updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky + World.chunkSize, thisChunkz)));
                    if (b.position.z == 0)
                        updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky, thisChunkz - World.chunkSize)));
                    if (b.position.z == World.chunkSize - 1)
                        updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky, thisChunkz + World.chunkSize)));

                    foreach (string cname in updates)
                    {
                        Chunk c;
                        if (World.chunks.TryGetValue(cname, out c))
                        {
                            c.Redraw();
                        }
                    }
                }

            }
        }
    }
}

