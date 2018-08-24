using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
class BlockData
{
    public Block.BlockType[,,] matrix;
    public BlockData() { }
    public BlockData(Block[,,] b)
    {
        matrix = new Block.BlockType[World.chunkSize, World.chunkSize, World.chunkSize];
        for (var z = 0; z < World.chunkSize; z++)
        {
            for (var y = 0; y < World.chunkSize; y++)
            {
                for (var x = 0; x < World.chunkSize; x++)
                {
                    matrix[x, y, z] = b[x, y, z].bType;
                }
            }
        }
    }
}

public class Chunk
{

    public Material cubeMaterial;
    public Block[,,] chunkData;
    public GameObject chunk;
    public enum ChunkStatus { DRAW, DONE, KEEP };
    public ChunkStatus status;
    public float touchedTime;
    BlockData bd;

    string BuildChunkFileName(Vector3 v)
    {
        return string.Format("{0}/savedata/Chunk_{1}_{2}_{3}_{4}_{5}.dat",
            Application.persistentDataPath,
            (int)v.x,
            (int)v.y,
            (int)v.z,
            World.chunkSize,
            World.radius);
    }

    bool Load()
    {
        var chunkFile = BuildChunkFileName(chunk.transform.position);
        if (File.Exists(chunkFile))
        {
            var bf = new BinaryFormatter();
            var file = File.Open(chunkFile, FileMode.Open);
            bd = new BlockData();
            bd = (BlockData)bf.Deserialize(file);
            file.Close();
            //Debug.Log("Loading chunk from file: " + chunkFile);
            return true;
        }
        return false;
    }

    public void Save()
    {
        var chunkFile = BuildChunkFileName(chunk.transform.position);
        if(!File.Exists(chunkFile))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(chunkFile));
        }
        var bf = new BinaryFormatter();
        var file = File.Open(chunkFile, FileMode.OpenOrCreate);
        bd = new BlockData(chunkData);
        bf.Serialize(file, bd);
        file.Close();
        //Debug.Log("Saving chunk to file: " + chunkFile);
    }

    void BuildChunk()
    {
        var dataFromFile = false;
        dataFromFile = Load();

        touchedTime = Time.time;
        chunkData = new Block[World.chunkSize, World.chunkSize, World.chunkSize];
        for (int z = 0; z < World.chunkSize; z++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int x = 0; x < World.chunkSize; x++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    int worldX = (int)(x + chunk.transform.position.x);
                    int worldY = (int)(y + chunk.transform.position.y);
                    int worldZ = (int)(z + chunk.transform.position.z);

                    if(dataFromFile)
                    {
                        chunkData[x, y, z] = new Block(bd.matrix[x, y, z], pos, chunk.gameObject, this);
                        continue;
                    }

                    int surfaceHeight = Utils.GenerateHeight(worldX, worldZ);

                    if (Utils.fBM3D(worldX, worldY, worldZ, 0.1f, 3) < 0.42f)
                        chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos,
                                        chunk.gameObject, this);
                    else if (worldY == 0)
                        chunkData[x, y, z] = new Block(Block.BlockType.BEDROCK, pos,
                                        chunk.gameObject, this);
                    else if (worldY <= Utils.GenerateStoneHeight(worldX, worldZ))
                    {
                        if (Utils.fBM3D(worldX, worldY, worldZ, 0.01f, 2) < 0.4f && worldY < 40)
                            chunkData[x, y, z] = new Block(Block.BlockType.DIAMOND, pos,
                                        chunk.gameObject, this);
                        else if (Utils.fBM3D(worldX, worldY, worldZ, 0.03f, 3) < 0.41f && worldY < 20)
                            chunkData[x, y, z] = new Block(Block.BlockType.REDSTONE, pos,
                                        chunk.gameObject, this);
                        else
                            chunkData[x, y, z] = new Block(Block.BlockType.STONE, pos,
                                        chunk.gameObject, this);
                    }
                    else if (worldY == surfaceHeight)
                    {
                        chunkData[x, y, z] = new Block(Block.BlockType.GRASS, pos,
                                        chunk.gameObject, this);
                    }
                    else if (worldY < surfaceHeight)
                        chunkData[x, y, z] = new Block(Block.BlockType.DIRT, pos,
                                        chunk.gameObject, this);
                    else
                    {
                        chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos,
                                        chunk.gameObject, this);
                    }

                    status = ChunkStatus.DRAW;

                }

    }

    public void DrawChunk()
    {
        for (int z = 0; z < World.chunkSize; z++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int x = 0; x < World.chunkSize; x++)
                {
                    chunkData[x, y, z].Draw();
                }

        CombineQuads();
        MeshCollider collider = chunk.gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        collider.sharedMesh = chunk.transform.GetComponent<MeshFilter>().mesh;
        status = ChunkStatus.DONE;
    }

    public Chunk() { }
    // Use this for initialization
    public Chunk(Vector3 position, Material c)
    {

        chunk = new GameObject(World.BuildChunkName(position));
        chunk.transform.position = position;
        cubeMaterial = c;
        BuildChunk();
    }

    public void CombineQuads()
    {
        //1. Combine all children meshes
        MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        //2. Create a new mesh on the parent object
        MeshFilter mf = (MeshFilter)chunk.gameObject.AddComponent(typeof(MeshFilter));
        mf.mesh = new Mesh();

        //3. Add combined meshes on children as the parent's mesh
        mf.mesh.CombineMeshes(combine);

        //4. Create a renderer for the parent
        MeshRenderer renderer = chunk.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = cubeMaterial;

        //5. Delete all uncombined children
        foreach (Transform quad in chunk.transform)
        {
            GameObject.Destroy(quad.gameObject);
        }

    }

}
