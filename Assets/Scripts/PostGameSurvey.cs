using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostGameSurvey : MonoBehaviour
{
    [Header("Answers from 1 to 5")]
    [SerializeField] private TMP_Dropdown challengeDropdown;
    [SerializeField] private TMP_Dropdown errorResponsibilityDropdown;
    [SerializeField] private TMP_Dropdown masteryDropdown;
    [SerializeField] private TMP_Dropdown shieldImpactDropdown;
    [SerializeField] private TMP_InputField hardestPartInput;
    [SerializeField] private Button openSurveyButton;
    [SerializeField] private Button exitSurveyButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text validationMessage;
    [SerializeField] private GameObject surveyPanel;

    private bool submitted;

    private void Awake()
    {
        if (surveyPanel != null && surveyPanel != gameObject)
        {
            surveyPanel.SetActive(false);
        }

        if (openSurveyButton != null)
        {
            openSurveyButton.onClick.AddListener(OpenSurvey);
        }

        if (exitSurveyButton != null)
        {
            exitSurveyButton.onClick.AddListener(CloseSurvey);
        }

        ConfigureRatingDropdown(challengeDropdown, "Muy facil", "Muy dificil");
        ConfigureRatingDropdown(errorResponsibilityDropdown, "Nada", "Mucho");
        ConfigureRatingDropdown(masteryDropdown, "Nada", "Mucho");
        ConfigureRatingDropdown(shieldImpactDropdown, "Nada", "Mucho");

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitSurvey);
        }
    }

    public void OpenSurvey()
    {
        if (surveyPanel != null)
        {
            surveyPanel.SetActive(true);
        }

        if (openSurveyButton != null)
        {
            openSurveyButton.gameObject.SetActive(false);
        }
    }

    public void CloseSurvey()
    {
        if (surveyPanel != null)
        {
            surveyPanel.SetActive(false);
        }

        if (openSurveyButton != null)
        {
            openSurveyButton.gameObject.SetActive(true);
        }
    }

    public void SubmitSurvey()
    {
        if (submitted || !HasRequiredReferences())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(hardestPartInput.text))
        {
            ShowValidation("Escribe que parte del nivel fue mas dificil.");
            return;
        }

        GameplayLogger logger = GameplayLogger.Instance;
        if (logger == null)
        {
            ShowValidation("No se encontro el registrador de la partida.");
            return;
        }

        logger.SetSurveyResponses(
            challengeDropdown.value + 1,
            errorResponsibilityDropdown.value + 1,
            masteryDropdown.value + 1,
            shieldImpactDropdown.value + 1,
            hardestPartInput.text.Trim());

        submitted = true;
        ShowValidation("Respuesta guardada. Gracias.");

        if (submitButton != null)
        {
            submitButton.interactable = false;
        }
    }

    private void ConfigureRatingDropdown(TMP_Dropdown dropdown, string firstLabel, string lastLabel)
    {
        if (dropdown == null)
        {
            return;
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "1 - " + firstLabel,
            "2",
            "3",
            "4",
            "5 - " + lastLabel
        });
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private bool HasRequiredReferences()
    {
        bool valid = challengeDropdown != null &&
                     errorResponsibilityDropdown != null &&
                     masteryDropdown != null &&
                     shieldImpactDropdown != null &&
                     hardestPartInput != null;

        if (!valid)
        {
            ShowValidation("Faltan referencias de la encuesta en el Inspector.");
        }

        return valid;
    }

    private void ShowValidation(string message)
    {
        if (validationMessage != null)
        {
            validationMessage.text = message;
        }
    }
}
