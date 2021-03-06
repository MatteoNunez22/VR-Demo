using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Voronoi;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class FractureChunk : MonoBehaviour 
{
	public Material material;

	private Mesh meshSunnyside, meshDouble;

	public Cell cell;

	public bool separated = false;

	float forceAccumulation = 0.0f;

	public void ApplyForce(Vector3 impactPoint)
	{
		GetComponent<MeshFilter>().sharedMesh = meshDouble;
        GetComponent<MeshCollider>().sharedMesh = meshDouble;

        forceAccumulation += 1;
		float d = 0.5f;
		transform.Rotate (new Vector3(Random.Range(-d, d), Random.Range(-d, d), Random.Range(-d, d)));

        Debug.Log("forceAccumulation: ");
        Debug.Log(forceAccumulation);

		if(forceAccumulation >= 5) {
			separated = true;

 			if(!GetComponent<Rigidbody>()) {
				Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
				GetComponent<Rigidbody>().useGravity = false;
				rigidBody.mass = 1;
				GetComponent<Rigidbody>().drag = 0.1f;
				GetComponent<Rigidbody>().angularDrag = 0.01f;
				GetComponent<Rigidbody>().AddForce(new Vector3(0,-100,0));
				//float d = 1.0f;
				GetComponent<Rigidbody>().AddTorque(new Vector3(-cell.site.y, 0, cell.site.x).normalized * Random.Range(1.0f, 10.0f) * 10 + new Vector3(Random.Range(-d,d), 0, Random.Range(-d,d)) * 3);
                GetComponent<Rigidbody>().isKinematic = true; // Edited by Peter 2017/01/20
            }

			List<float> args = new List<float>();
			args.Add(Mathf.Lerp (0, 6, (transform.position.x+2.4f)/4.8f));
			args.Add(Mathf.Lerp (0, 6, (transform.position.z+2.4f)/4.8f));
			//OSCHandler.Instance.SendMessageToClient("IceServer", "/niw/ice/shatter", args);
		}
	}

	void Update()
	{
        //Debug.Log("In the FractureChunk.cs file.");
		if(separated) {
            transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
            GetComponent<Renderer>().material.SetColor("_Color", GetComponent<Renderer>().material.GetColor("_Color") - new Color(0.01f, 0.01f, 0.01f, 0.01f));
            if (transform.localScale.x <= 0.0f)
            {
                GetComponent<Renderer>().enabled = false;
            }
        }
	}

	public void CreateFanMesh()
	{
		if (cell.halfEdges.Count > 0)
		{
			meshSunnyside = new Mesh();
			meshDouble = new Mesh();
			meshSunnyside.name = "Chunk " + cell.site.id;
			meshDouble.name = "Chunk " + cell.site.id;

			int numSideVerts = cell.halfEdges.Count + 1;
			int numSideIndices = cell.halfEdges.Count * 3;

			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			List<int> triangles = new List<int>();
			
			Vector3 vThickness = new Vector3(0, -0.02f, 0);

            float uvf = 1.0f / 20.0f;
			vertices.Add(cell.site.ToVector3() - transform.position);
			normals.Add (new Vector3(0,1,0));
			uvs.Add(new Vector2(vertices[0].x, vertices[0].z));

			for (int v = 1; v < numSideVerts; v++)
			{
				vertices.Add(cell.halfEdges[v-1].GetStartPoint().ToVector3() - transform.position);// * 0.98f;
				normals.Add (new Vector3(0,1,0));
				uvs.Add (new Vector2(vertices[v].x, vertices[v].z));
				triangles.Add (0);
				triangles.Add (v);
				triangles.Add (v % (numSideVerts-1) + 1);
			}

			vertices.Add (cell.site.ToVector3() - transform.position + vThickness);
			normals.Add (new Vector3(0,-1,0));
			uvs.Add (new Vector2(vertices[numSideVerts].x, vertices[numSideVerts].z));

			for (int v = 1, t = 1; v < numSideVerts; v++, t += 3)
			{
				vertices.Add (vertices[v] + vThickness);
				normals.Add (new Vector3(0,-1,0));
				uvs.Add (new Vector2(vertices[v + numSideVerts].x, vertices[v + numSideVerts].z));
				triangles.Add (v % (numSideVerts-1) + numSideVerts + 1);
				triangles.Add (v + numSideVerts);
				triangles.Add (numSideVerts);

				triangles.Add (v % (numSideVerts-1) + 1);
				triangles.Add (v);
				triangles.Add (v + numSideVerts);
				triangles.Add (v + numSideVerts);
				triangles.Add (v % (numSideVerts-1) + 1 + numSideVerts);
				triangles.Add (v % (numSideVerts-1) + 1);
			}

			meshSunnyside.vertices = vertices.GetRange(0, numSideVerts).ToArray();
			meshSunnyside.normals = normals.GetRange(0, numSideVerts).ToArray();
			meshSunnyside.uv = uvs.GetRange(0, numSideVerts).ToArray();
			meshSunnyside.triangles = triangles.GetRange(0, numSideIndices).ToArray();
			meshSunnyside.RecalculateBounds();

			meshDouble.vertices = vertices.ToArray();
			meshDouble.normals = normals.ToArray();
			meshDouble.uv = uvs.ToArray();
			meshDouble.triangles = triangles.ToArray();
			meshDouble.RecalculateBounds();

            // not separated at first
            GetComponent<MeshFilter>().sharedMesh = meshSunnyside;
            GetComponent<MeshCollider>().sharedMesh = meshSunnyside;

            GetComponent<Renderer>().sharedMaterial = material;
		}
	}
}
