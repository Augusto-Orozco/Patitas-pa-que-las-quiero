using UnityEngine;
using System.IO;

public class GameplayLogger : MonoBehaviour
{
    public static GameplayLogger Instance;

    public int jumps = 0;
    public int damageReceived = 0;
    public int damageBlocked = 0;

    private int challengeRating;
    private int errorResponsibilityRating;
    private int masteryRating;
    private int shieldImpactRating;
    private string hardestPart = "";
    private bool surveyCompleted;
    private bool exportInProgress;
    private bool hasExported;

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

    public void SetSurveyResponses(
        int challenge,
        int errorResponsibility,
        int mastery,
        int shieldImpact,
        string hardestLevelPart)
    {
        challengeRating = Mathf.Clamp(challenge, 1, 5);
        errorResponsibilityRating = Mathf.Clamp(errorResponsibility, 1, 5);
        masteryRating = Mathf.Clamp(mastery, 1, 5);
        shieldImpactRating = Mathf.Clamp(shieldImpact, 1, 5);
        hardestPart = hardestLevelPart ?? "";
        surveyCompleted = true;
    }

    public void ExportCSV()
    {
        if (exportInProgress || hasExported)
        {
            return;
        }

        exportInProgress = true;

        string logsFolder = Path.Combine(Application.dataPath, "Logs");
        Directory.CreateDirectory(logsFolder);

        string path = Path.Combine(
            logsFolder,
            "dyn_log.csv"
        );

        try
        {
            // Crear encabezados si el archivo todavía no existe
            if (!File.Exists(path))
            {
                File.WriteAllText(
                    path,
                    "TiempoSegundos,Saltos,DanioRecibido,DanioBloqueado," +
                    "\"Que tan desafiante te parecio el juego?\"," +
                    "\"Sentiste que los errores fueron causados por tus decisiones?\"," +
                    "\"Sentiste que mejorabas mientras avanzabas?\"," +
                    "\"El escudo fue util?\"," +
                    "\"Comentarios para el desarrollador\"\n"
                );
            }

            // Agregar los datos de esta sesión y cerrar el archivo inmediatamente.
            string data =
                sessionTime.ToString("F2") + "," +
                jumps + "," +
                damageReceived + "," +
                damageBlocked + "," +
                challengeRating + "," +
                errorResponsibilityRating + "," +
                masteryRating + "," +
                shieldImpactRating + "," +
                EscapeCsv(hardestPart) + "\n";

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.Write(data);
            }

            hasExported = true;

            Debug.Log(
                "Resultados de la sesión:\n" +
                "Tiempo: " + sessionTime.ToString("F2") + " segundos\n" +
                "Saltos: " + jumps + "\n" +
                "Daño recibido: " + damageReceived + "\n" +
                "Daño bloqueado: " + damageBlocked + "\n" +
                "Encuesta completada: " + (surveyCompleted ? "Sí" : "No") + "\n" +
                "Datos exportados a: " + path
            );
        }
        catch (IOException exception)
        {
            Debug.LogError(
                "No se pudo escribir el CSV porque está siendo usado por otro programa. " +
                "Cierra Excel u otro editor y vuelve a ejecutar la partida. Detalle: " +
                exception.Message
            );
        }
        finally
        {
            exportInProgress = false;
        }
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private void OnApplicationQuit()
    {
        ExportCSV();
    }
}