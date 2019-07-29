using UnityEngine;
using System.Collections.Generic;

[SelectionBase]
public class VoxelGrid : MonoBehaviour
{
    public int Resolution;
    public GameObject VoxelPrefab;
    public VoxelGrid XNeighbor, yNeighbor, xyNeighbor;

    private Voxel[] _voxels;
    private float _voxelSize, _gridSize;

    private Material[] _voxelMaterials;

    private Mesh _mesh;

    private List<Vector3> _vertices;
    private List<int> _triangles;

    private Voxel _dummyX, _dummyY, _dummyT;

    public void Initialize(int resolution, float size)
    {
        Resolution = resolution;
        _gridSize = size;
        _voxelSize = size / resolution;
        _voxels = new Voxel[resolution * resolution];
        _voxelMaterials = new Material[_voxels.Length];

        _dummyX = new Voxel();
        _dummyY = new Voxel();
        _dummyT = new Voxel();

        for (int i = 0, y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                CreateVoxel(i, x, y);
            }
        }

        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "VoxelGrid Mesh";
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        Refresh();
    }

    private void CreateVoxel(int i, int x, int y)
    {
        GameObject o = Instantiate(VoxelPrefab) as GameObject;
        o.transform.parent = transform;
        o.transform.localPosition = new Vector3((x + 0.5f) * _voxelSize, (y + 0.5f) * _voxelSize, -0.01f);
        o.transform.localScale = Vector3.one * _voxelSize * 0.1f;
        _voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
        _voxels[i] = new Voxel(x, y, _voxelSize);
    }

    private void Refresh()
    {
        SetVoxelColors();
        Triangulate();
    }

    private void Triangulate()
    {
        _vertices.Clear();
        _triangles.Clear();
        _mesh.Clear();

        if (XNeighbor != null)
        {
            _dummyX.BecomeXDummyOf(XNeighbor._voxels[0], _gridSize);
        }
        TriangulateCellRows();
        if (yNeighbor != null)
        {
            TriangulateGapRow();
        }

        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
    }

    private void TriangulateCellRows()
    {
        int cells = Resolution - 1;
        for (int i = 0, y = 0; y < cells; y++, i++)
        {
            for (int x = 0; x < cells; x++, i++)
            {
                TriangulateCell(
                    _voxels[i],
                    _voxels[i + 1],
                    _voxels[i + Resolution],
                    _voxels[i + Resolution + 1]);
            }
            if (XNeighbor != null)
            {
                TriangulateGapCell(i);
            }
        }
    }

    private void TriangulateGapCell(int i)
    {
        Voxel dummySwap = _dummyT;
        dummySwap.BecomeXDummyOf(XNeighbor._voxels[i + 1], _gridSize);
        _dummyT = _dummyX;
        _dummyX = dummySwap;
        TriangulateCell(_voxels[i], _dummyT, _voxels[i + Resolution], _dummyX);
    }

    private void TriangulateGapRow()
    {
        _dummyY.BecomeYDummyOf(yNeighbor._voxels[0], _gridSize);
        int cells = Resolution - 1;
        int offset = cells * Resolution;

        for (int x = 0; x < cells; x++)
        {
            Voxel dummySwap = _dummyT;
            dummySwap.BecomeYDummyOf(yNeighbor._voxels[x + 1], _gridSize);
            _dummyT = _dummyY;
            _dummyY = dummySwap;
            TriangulateCell(_voxels[x + offset], _voxels[x + offset + 1], _dummyT, _dummyY);
        }

        if (XNeighbor != null)
        {
            _dummyT.BecomeXYDummyOf(xyNeighbor._voxels[0], _gridSize);
            TriangulateCell(_voxels[_voxels.Length - 1], _dummyX, _dummyY, _dummyT);
        }
    }

    private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        int cellType = 0;
        if (a.State)
        {
            cellType |= 1;
        }
        if (b.State)
        {
            cellType |= 2;
        }
        if (c.State)
        {
            cellType |= 4;
        }
        if (d.State)
        {
            cellType |= 8;
        }
        switch (cellType)
        {
            case 0:
                return;
            case 1:
                AddTriangle(a.Position, a.YEdgePosition, a.XEdgePosition);
                break;
            case 2:
                AddTriangle(b.Position, a.XEdgePosition, b.YEdgePosition);
                break;
            case 3:
                AddQuad(a.Position, a.YEdgePosition, b.YEdgePosition, b.Position);
                break;
            case 4:
                AddTriangle(c.Position, c.XEdgePosition, a.YEdgePosition);
                break;
            case 5:
                AddQuad(a.Position, c.Position, c.XEdgePosition, a.XEdgePosition);
                break;
            case 6:
                AddTriangle(b.Position, a.XEdgePosition, b.YEdgePosition);
                AddTriangle(c.Position, c.XEdgePosition, a.YEdgePosition);
                break;
            case 7:
                AddPentagon(a.Position, c.Position, c.XEdgePosition, b.YEdgePosition, b.Position);
                break;
            case 8:
                AddTriangle(d.Position, b.YEdgePosition, c.XEdgePosition);
                break;
            case 9:
                AddTriangle(a.Position, a.YEdgePosition, a.XEdgePosition);
                AddTriangle(d.Position, b.YEdgePosition, c.XEdgePosition);
                break;
            case 10:
                AddQuad(a.XEdgePosition, c.XEdgePosition, d.Position, b.Position);
                break;
            case 11:
                AddPentagon(b.Position, a.Position, a.YEdgePosition, c.XEdgePosition, d.Position);
                break;
            case 12:
                AddQuad(a.YEdgePosition, c.Position, d.Position, b.YEdgePosition);
                break;
            case 13:
                AddPentagon(c.Position, d.Position, b.YEdgePosition, a.XEdgePosition, a.Position);
                break;
            case 14:
                AddPentagon(d.Position, b.Position, a.XEdgePosition, a.YEdgePosition, c.Position);
                break;
            case 15:
                AddQuad(a.Position, c.Position, d.Position, b.Position);
                break;
        }
    }

    private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        int vertexIndex = _vertices.Count;
        _vertices.Add(a);
        _vertices.Add(b);
        _vertices.Add(c);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
    }

    private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int vertexIndex = _vertices.Count;
        _vertices.Add(a);
        _vertices.Add(b);
        _vertices.Add(c);
        _vertices.Add(d);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 3);
    }

    private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        int vertexIndex = _vertices.Count;
        _vertices.Add(a);
        _vertices.Add(b);
        _vertices.Add(c);
        _vertices.Add(d);
        _vertices.Add(e);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 3);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 3);
        _triangles.Add(vertexIndex + 4);
    }

    private void SetVoxelColors()
    {
        for (int i = 0; i < _voxels.Length; i++)
        {
            _voxelMaterials[i].color = _voxels[i].State ? Color.black : Color.white;
        }
    }

    public void Apply(VoxelStencil stencil)
    {
        int xStart = stencil.XStart;
        if (xStart < 0)
        {
            xStart = 0;
        }
        int xEnd = stencil.XEnd;
        if (xEnd >= Resolution)
        {
            xEnd = Resolution - 1;
        }
        int yStart = stencil.YStart;
        if (yStart < 0)
        {
            yStart = 0;
        }
        int yEnd = stencil.YEnd;
        if (yEnd >= Resolution)
        {
            yEnd = Resolution - 1;
        }

        for (int y = yStart; y <= yEnd; y++)
        {
            int i = y * Resolution + xStart;
            for (int x = xStart; x <= xEnd; x++, i++)
            {
                _voxels[i].State = stencil.Apply(x, y, _voxels[i].State);
            }
        }
        Refresh();
    }
}