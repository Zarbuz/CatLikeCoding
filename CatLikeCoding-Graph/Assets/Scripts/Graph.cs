using UnityEngine;

public class Graph : MonoBehaviour
{
    public Transform PointPrefab;

    [Range(10, 100)] public int Resolution = 10;
    public GraphFunctionName Function;

    private Transform[] _points;

    static GraphFunction[] functions = {
        SineFunction, Sine2DFunction, MultiSineFunction
    };

    private void Awake()
    {
        float step = 2f / Resolution;
        Vector3 scale = Vector3.one * step;
        Vector3 position = Vector3.zero;
        _points = new Transform[Resolution * Resolution];
        for (int i = 0, z = 0; z < Resolution; z++)
        {
            position.z = (z + 0.5f) * step - 1f;
            for (int x = 0; x < Resolution; x++, i++)
            {
                Transform point = Instantiate(PointPrefab);
                position.x = (x + 0.5f) * step - 1f;
                point.localPosition = position;
                point.localScale = scale;
                point.SetParent(transform);
                _points[i] = point;
            }
        }
    }

    private void Update()
    {
        float t = Time.time;
        GraphFunction f = functions[(int)Function];
        for (int i = 0; i < _points.Length; i++)
        {
            Transform point = _points[i];
            Vector3 position = point.localPosition;
            position.y = f(position.x, position.z, t);
            point.localPosition = position;
        }
    }

    const float pi = Mathf.PI;

    public static float SineFunction(float x, float z, float t)
    {
        return Mathf.Sin(pi * (x + t));
    }

    public static float MultiSineFunction(float x, float z, float t)
    {
        float y = Mathf.Sin(pi * (x + t));
        y += Mathf.Sin(2f * pi * (x + 2f * t)) / 2f;
        y *= 2f / 3f;
        return y;
    }

    public static float Sine2DFunction(float x, float z, float t)
    {
        float y = Mathf.Sin(pi * (x + t));
        y += Mathf.Sin(pi * (z + t));
        y *= 0.5f;
        return y;
    }
}
