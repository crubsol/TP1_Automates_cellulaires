using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;
    [SerializeField] private Tile aliveTile;
    [SerializeField] private Tile deadTile;
    [SerializeField] private float updateInterval = 0.05f;

    // optimizacion
    private HashSet<Vector3Int> aliveCells;
    private HashSet<Vector3Int> cellsToCheck;

    // para controlar simulacion
    private bool isSimulating = false;
    private Coroutine simulationCoroutine;

    public int population { get; private set; }
    public int iteration { get; private set; }
    public float time { get; private set; }

    private void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();
    }

    private void Update()
    {
        // deteccion de clics del ratón para dibujar el patrón
        if (!isSimulating && Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = currentState.WorldToCell(mouseWorldPos);

            // alternar entre celda viva y muerta al hacer clic
            if (currentState.GetTile(cellPosition) == aliveTile)
            {
                currentState.SetTile(cellPosition, deadTile);
                aliveCells.Remove(cellPosition);
            }
            else
            {
                currentState.SetTile(cellPosition, aliveTile);
                aliveCells.Add(cellPosition);
            }
        }

        // alternar entre iniciar y pausar la simulación con la tecla espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isSimulating)
            {
                StopSimulation();
            }
            else
            {
                StartSimulation();
            }
        }

        // Limpiar la pantalla al presionar la tecla "C"
        if (Input.GetKeyDown(KeyCode.C))
        {
            StopSimulation(); // Detener la simulación si está en curso
            Clear(); // Limpiar la pantalla
        }
    }

    private void StartSimulation()
    {
        // Iniciar la simulación si no está ya en marcha
        isSimulating = true;
        simulationCoroutine = StartCoroutine(Simulate());
    }

    private void StopSimulation()
    {
        // Detener la simulación
        if (simulationCoroutine != null)
        {
            StopCoroutine(simulationCoroutine);
            simulationCoroutine = null;
        }
        isSimulating = false;
    }

    private void Clear()
    {
        // Limpiar todos los tiles y reiniciar las variables
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        aliveCells.Clear();
        cellsToCheck.Clear();
        population = 0;
        iteration = 0;
        time = 0;
    }

    private IEnumerator Simulate()
    {
        var interval = new WaitForSeconds(updateInterval);

        while (isSimulating) // Mientras esté activa la simulación
        {
            UpdateState(); // Cambia el estado
            population = aliveCells.Count;
            iteration++;
            time += updateInterval;
            yield return interval; // Espera un segundo
        }
    }

    private void UpdateState()
    {
        // Recolectar las celdas a revisar
        cellsToCheck.Clear();

        foreach (Vector3Int cell in aliveCells)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
                }
            }
        }

        // Transición de las celdas al siguiente estado
        foreach (Vector3Int cell in cellsToCheck)
        {
            int neighbors = CountNeighbors(cell);
            bool alive = IsAlive(cell);

            if (!alive && neighbors == 3)
            {
                nextState.SetTile(cell, aliveTile); // Se convierte en viva
                aliveCells.Add(cell);
            }
            else if (alive && (neighbors < 2 || neighbors > 3))
            {
                nextState.SetTile(cell, deadTile); // Se convierte en muerta
                aliveCells.Remove(cell);
            }
            else
            {
                nextState.SetTile(cell, currentState.GetTile(cell)); // Permanece igual
            }
        }

        // Intercambiar el estado actual con el siguiente
        Tilemap temp = currentState;
        currentState = nextState;
        nextState = temp;
        nextState.ClearAllTiles();
    }

    private int CountNeighbors(Vector3Int cell)
    {
        int count = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighbor = cell + new Vector3Int(x, y, 0);

                if (x == 0 && y == 0) continue; // Ignorar la celda central
                if (IsAlive(neighbor)) count++;
            }
        }
        return count;
    }

    private bool IsAlive(Vector3Int cell)
    {
        return currentState.GetTile(cell) == aliveTile;
    }
}
