using UnityEngine;
using System.IO;

public class GameplayLogger : MonoBehaviour
{
    public static GameplayLogger Instance;

    public int jumps = 0;
    public int damageReceived = 0;
    public int damageBlocked = 0;

    private float sessionTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateLogger()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject loggerObject = new GameObject("GameplayLogger");
        loggerObject.AddComponent<GameplayLogger>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        sessionTime += Time.deltaTime;
    }

    public void LogJump()
    {
        jumps++;
    }

    public void LogDamage()
    {
        damageReceived++;
    }

    public void LogBlockedDamage()
    {
        damageBlocked++;
    }

    public void ExportCSV()
    {
        string logsFolder = Path.Combine(Application.dataPath, "Logs");
        Directory.CreateDirectory(logsFolder);

        string path = Path.Combine(
            logsFolder,
            "dyn_log.csv"
        );

        // Crear encabezados si el archivo todavía no existe
        if (!File.Exists(path))
        {
            File.WriteAllText(
                path,
                "TiempoSegundos,Saltos,DanioRecibido,DanioBloqueado\n"
            );
        }

        // Agregar los datos de esta sesión
        string data =
            sessionTime.ToString("F2") + "," +
            jumps + "," +
            damageReceived + "," +
            damageBlocked + "\n";

        File.AppendAllText(path, data);

        Debug.Log(
            "Resultados de la sesión:\n" +
            "Tiempo: " + sessionTime.ToString("F2") + " segundos\n" +
            "Saltos: " + jumps + "\n" +
            "Daño recibido: " + damageReceived + "\n" +
            "Daño bloqueado: " + damageBlocked + "\n" +
            "Datos exportados a: " + path
        );
    }

    private void OnApplicationQuit()
    {
        ExportCSV();
    }
}