using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Grid")]
    public int width = 16;

    public int height = 12;
    public float cellSize = 4f;
    public float wallHeight = 3f;
    public float wallThickness = 0.25f;
    public float floorThickness = 0.2f;
    public float ceilingThickness = 0.2f;
    public int seed = 0;
    public bool randomizeSeed = true;
    public bool generateOnStart = true;
    public bool addCeiling = false;

    public bool singleCeiling = false;

    [Header("Materials")]
    public Material floorMaterial;

    public Material wallMaterial;
    public Material ceilingMaterial;

    [Header("Lighting")]
    public bool addLights = true;

    public LightShadows shadowType = LightShadows.None;
    public float lightIntensity = 1.2f;
    public float lightRange = 12f;
    public float lightSpacing = 8f;
    public Color lightColor = Color.white;
    public bool randomizeLightColor = false;
    public bool useLightPalette = false;
    public Color[] lightPalette;
    public bool paletteCycle = false;
    public float lightHeight = 2.4f;

    private Cell[,] _grid;
    private readonly List<Vector3> _path = new List<Vector3>();

    private Transform _floorRoot;
    private Transform _wallRoot;
    private Transform _lightRoot;
    private Transform _ceilingRoot;

    private System.Random _rng;
    private int _paletteIndex;

    private struct Cell
    {
        public bool visited;
        public bool northWall;
        public bool southWall;
        public bool eastWall;
        public bool westWall;
    }

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        _path.Clear();
    }

    public void Generate()
    {
        Clear();

        if (randomizeSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);
        _rng = new System.Random(seed);
        _paletteIndex = 0;

        _grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid[x, y] = new Cell
                {
                    visited = false,
                    northWall = true,
                    southWall = true,
                    eastWall = true,
                    westWall = true
                };
            }
        }

        var stack = new Stack<Vector2Int>();
        var start = new Vector2Int(_rng.Next(0, width), _rng.Next(0, height));
        _grid[start.x, start.y].visited = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var neighbors = GetUnvisitedNeighbors(current);
            if (neighbors.Count == 0)
            {
                stack.Pop();
                continue;
            }
            var next = neighbors[_rng.Next(neighbors.Count)];
            RemoveWall(current, next);
            _grid[next.x, next.y].visited = true;
            stack.Push(next);
        }

        _floorRoot = new GameObject("Floors").transform;
        _floorRoot.SetParent(transform, false);
        _wallRoot = new GameObject("Walls").transform;
        _wallRoot.SetParent(transform, false);
        _lightRoot = new GameObject("Lights").transform;
        _lightRoot.SetParent(transform, false);

        if (addCeiling)
        {
            _ceilingRoot = new GameObject("Ceilings").transform;
            _ceilingRoot.SetParent(transform, false);
        }

        BuildGeometry();
        BuildLights();
        BuildPath();
    }

    private void BuildGeometry()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var center = CellToWorldCenter(new Vector2Int(x, y));
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = $"Floor_{x}_{y}";
                floor.transform.SetParent(_floorRoot, false);
                floor.transform.position = center + new Vector3(0, -floorThickness * 0.5f, 0);
                floor.transform.localScale = new Vector3(cellSize, floorThickness, cellSize);
                var fr = floor.GetComponent<MeshRenderer>();
                if (floorMaterial != null) fr.sharedMaterial = floorMaterial;
                floor.isStatic = true;

                var c = _grid[x, y];

                if (c.northWall) SpawnWall(center + new Vector3(0, wallHeight * 0.5f, cellSize * 0.5f), new Vector3(cellSize + wallThickness, wallHeight, wallThickness));
                if (c.southWall) SpawnWall(center + new Vector3(0, wallHeight * 0.5f, -cellSize * 0.5f), new Vector3(cellSize + wallThickness, wallHeight, wallThickness));
                if (c.eastWall) SpawnWall(center + new Vector3(cellSize * 0.5f, wallHeight * 0.5f, 0), new Vector3(wallThickness, wallHeight, cellSize + wallThickness));
                if (c.westWall) SpawnWall(center + new Vector3(-cellSize * 0.5f, wallHeight * 0.5f, 0), new Vector3(wallThickness, wallHeight, cellSize + wallThickness));

                if (addCeiling && _ceilingRoot != null && !singleCeiling)
                {
                    var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ceiling.name = $"Ceiling_{x}_{y}";
                    ceiling.transform.SetParent(_ceilingRoot, false);
                    ceiling.transform.position = center + new Vector3(0, wallHeight - (ceilingThickness * 0.5f), 0);
                    ceiling.transform.localScale = new Vector3(cellSize, ceilingThickness, cellSize);
                    var cm = ceiling.GetComponent<MeshRenderer>();
                    if (ceilingMaterial != null) cm.sharedMaterial = ceilingMaterial;
                    else if (wallMaterial != null) cm.sharedMaterial = wallMaterial;
                    ceiling.isStatic = true;
                }
            }
        }

        if (addCeiling && _ceilingRoot != null && singleCeiling)
        {
            var totalWidth = width * cellSize;
            var totalHeight = height * cellSize;
            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(_ceilingRoot, false);
            var center = new Vector3(0f, wallHeight - (ceilingThickness * 0.5f), 0f);
            ceiling.transform.position = center;
            ceiling.transform.localScale = new Vector3(totalWidth, ceilingThickness, totalHeight);
            var cm = ceiling.GetComponent<MeshRenderer>();
            if (ceilingMaterial != null) cm.sharedMaterial = ceilingMaterial;
            else if (wallMaterial != null) cm.sharedMaterial = wallMaterial;
            ceiling.isStatic = true;
        }
    }

    private void SpawnWall(Vector3 position, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(_wallRoot, false);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        var mr = wall.GetComponent<MeshRenderer>();
        if (wallMaterial != null) mr.sharedMaterial = wallMaterial;
        wall.isStatic = true;
    }

    private void BuildLights()
    {
        if (!addLights) return;

        float spacing = Mathf.Max(1f, lightSpacing);
        for (float x = 0; x < width * cellSize; x += spacing)
        {
            for (float y = 0; y < height * cellSize; y += spacing)
            {
                var world = new Vector3(x - (width * cellSize) * 0.5f + cellSize * 0.5f, lightHeight, y - (height * cellSize) * 0.5f + cellSize * 0.5f);
                var lgo = new GameObject("Light");
                lgo.transform.SetParent(_lightRoot, false);
                lgo.transform.position = world;
                var light = lgo.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = lightRange;
                light.intensity = lightIntensity;
                light.shadows = shadowType;

                Color chosen;
                if (useLightPalette && lightPalette != null && lightPalette.Length > 0)
                {
                    if (paletteCycle)
                    {
                        chosen = lightPalette[_paletteIndex % lightPalette.Length];
                        _paletteIndex++;
                    }
                    else if (randomizeLightColor)
                    {
                        chosen = lightPalette[_rng.Next(lightPalette.Length)];
                    }
                    else
                    {
                        chosen = lightPalette[0];
                    }
                }
                else if (randomizeLightColor)
                {
                    chosen = RandomLightColor();
                }
                else
                {
                    chosen = lightColor;
                }

                light.color = chosen;
            }
        }
    }

    private Color RandomLightColor()
    {
        if (_rng == null) _rng = new System.Random();
        float h = (float)_rng.NextDouble();
        float s = 0.7f + (float)_rng.NextDouble() * 0.3f;
        float v = 0.8f + (float)_rng.NextDouble() * 0.2f;
        return Color.HSVToRGB(h, s, v);
    }

    private void BuildPath()
    {
        _path.Clear();
        var start = new Vector2Int(0, 0);
        var a = FarthestCellFrom(start);
        var b = FarthestCellFrom(a);
        var pathCells = ReconstructPath(a, b);
        foreach (var cell in pathCells)
        {
            _path.Add(CellToWorldCenter(cell) + Vector3.up * 0.5f);
        }
    }

    public IReadOnlyList<Vector3> GetPathPoints()
    {
        return _path;
    }

    private Vector3 CellToWorldCenter(Vector2Int cell)
    {
        float worldX = (cell.x * cellSize) - (width * cellSize) * 0.5f + cellSize * 0.5f;
        float worldZ = (cell.y * cellSize) - (height * cellSize) * 0.5f + cellSize * 0.5f;
        return new Vector3(worldX, 0f, worldZ);
    }

    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        var list = new List<Vector2Int>();
        if (cell.y + 1 < height && !_grid[cell.x, cell.y + 1].visited) list.Add(new Vector2Int(cell.x, cell.y + 1));
        if (cell.y - 1 >= 0 && !_grid[cell.x, cell.y - 1].visited) list.Add(new Vector2Int(cell.x, cell.y - 1));
        if (cell.x + 1 < width && !_grid[cell.x + 1, cell.y].visited) list.Add(new Vector2Int(cell.x + 1, cell.y));
        if (cell.x - 1 >= 0 && !_grid[cell.x - 1, cell.y].visited) list.Add(new Vector2Int(cell.x - 1, cell.y));
        return list;
    }

    private void RemoveWall(Vector2Int a, Vector2Int b)
    {
        if (a.x == b.x)
        {
            if (a.y < b.y)
            {
                _grid[a.x, a.y].northWall = false;
                _grid[b.x, b.y].southWall = false;
            }
            else
            {
                _grid[a.x, a.y].southWall = false;
                _grid[b.x, b.y].northWall = false;
            }
        }
        else if (a.y == b.y)
        {
            if (a.x < b.x)
            {
                _grid[a.x, a.y].eastWall = false;
                _grid[b.x, b.y].westWall = false;
            }
            else
            {
                _grid[a.x, a.y].westWall = false;
                _grid[b.x, b.y].eastWall = false;
            }
        }
    }

    private Vector2Int FarthestCellFrom(Vector2Int start)
    {
        var q = new Queue<Vector2Int>();
        var dist = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                dist[x, y] = -1;

        q.Enqueue(start);
        dist[start.x, start.y] = 0;
        Vector2Int far = start;

        while (q.Count > 0)
        {
            var c = q.Dequeue();
            if (dist[c.x, c.y] > dist[far.x, far.y]) far = c;
            foreach (var n in GetOpenNeighbors(c))
            {
                if (dist[n.x, n.y] == -1)
                {
                    dist[n.x, n.y] = dist[c.x, c.y] + 1;
                    q.Enqueue(n);
                }
            }
        }

        return far;
    }

    private List<Vector2Int> ReconstructPath(Vector2Int a, Vector2Int b)
    {
        var q = new Queue<Vector2Int>();
        var prev = new Vector2Int[width, height];
        var seen = new bool[width, height];
        q.Enqueue(a);
        seen[a.x, a.y] = true;

        while (q.Count > 0)
        {
            var c = q.Dequeue();
            if (c == b) break;
            foreach (var n in GetOpenNeighbors(c))
            {
                if (seen[n.x, n.y]) continue;
                seen[n.x, n.y] = true;
                prev[n.x, n.y] = c;
                q.Enqueue(n);
            }
        }

        var path = new List<Vector2Int>();
        var cur = b;
        path.Add(cur);
        while (cur != a)
        {
            cur = prev[cur.x, cur.y];
            path.Add(cur);
        }
        path.Reverse();
        return path;
    }

    private IEnumerable<Vector2Int> GetOpenNeighbors(Vector2Int c)
    {
        if (!_grid[c.x, c.y].northWall && c.y + 1 < height) yield return new Vector2Int(c.x, c.y + 1);
        if (!_grid[c.x, c.y].southWall && c.y - 1 >= 0) yield return new Vector2Int(c.x, c.y - 1);
        if (!_grid[c.x, c.y].eastWall && c.x + 1 < width) yield return new Vector2Int(c.x + 1, c.y);
        if (!_grid[c.x, c.y].westWall && c.x - 1 >= 0) yield return new Vector2Int(c.x - 1, c.y);
    }
}