using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;



[RequireComponent(typeof(PathCreator))]
public class Generate_Track : MonoBehaviour
{
    public GameObject trackArea;

    public BezierPath trackPath;
    private Bounds bounds;
    public int Points_min = 3;
    public int Points_max = 10;
    
    private void generatePath()
    {
        bounds = trackArea.GetComponent<MeshRenderer>().bounds;
        List<Vector3> points = new List<Vector3>();
        int num_points = Random.Range(Points_min, Points_max);

        RoadMeshCreator pst = (RoadMeshCreator)GetComponent<PathSceneTool>();
        PathCreator pc = GetComponent<PathCreator>();

        for (int i = 0; i < num_points; i++)
        {
            float size_x = bounds.size.x;
            float size_z = bounds.size.z;
            float size_y = bounds.size.y;

            //print(string.Format("Size X: {0}, Y: {1} Z: {2}", size_x, size_y, size_z));

            float x = Random.Range(bounds.center.x - size_x / 2, bounds.center.x + size_x / 2);
            float z = Random.Range(bounds.center.z - size_z / 2, bounds.center.z + size_z / 2);
            float y = 1;
            // I want to use some dumb raycasting so that the Y level always is on top of the terrain

            RaycastHit hit;
            if(Physics.Raycast(new Vector3(x, 10000, z), Vector3.down, out hit))
            {
                print(hit.point.y);
                y = hit.point.y + pst.thickness;
            }   

            //print(string.Format("Point X: {0}, Y: {1} Z: {2}", x, y, z));

            Vector3 point = new Vector3(x,y,z);
            points.Add(point);
        }

        trackPath = new BezierPath(points, true, PathSpace.xyz);
        trackPath.AutoControlLength = Random.Range(0.1f, 1.0f);
        trackPath.IsClosed = true;

        for(int i = 0; i < trackPath.NumPoints; i++)
        {
            Vector3 point = trackPath.GetPoint(i);
            float y = Random.Range(0.0f, 10.0f);
            point.y = y;
        }
        trackPath.NotifyPathModified();

        pc.bezierPath = trackPath;

        pst.TriggerUpdate();
    }

    private void Awake()
    {
        generatePath();
    }

// Start is called before the first frame update
    void Start()    
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        // generatePath();
    }
}
