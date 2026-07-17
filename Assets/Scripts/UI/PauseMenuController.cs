using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*
Controla o menu de pausa.

- Carrega no ESC para abrir/fechar a pausa.
- Se o painel de opcoes estiver aberto, o ESC fecha primeiro as opcoes.
- Congela o jogo (Time.timeScale = 0) e liberta o rato enquanto esta em pausa.

Liga os botoes que ja tens a estes metodos (no OnClick do Button):
  Resume()      -> botao "Continuar"
  OpenOptions() -> botao "Opcoes"
  CloseOptions()-> botao "Voltar" dentro das opcoes
  BackToMenu()  -> botao "Voltar ao menu"
  QuitGame()    -> botao "Sair"
*/
public class PauseMenuController : MonoBehaviour
{
    [Header("Paineis")]
    [Tooltip("Painel raiz do menu de pausa (o que aparece/desaparece).")]
    [SerializeField] private GameObject pausePanel;
    [Tooltip("Painel de opcoes (opcional). Fica escondido ate abrir as opcoes.")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Cenas")]
    [Tooltip("Nome exato da cena do menu principal (no Build Settings).")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Rato")]
    [Tooltip("Libertar e mostrar o rato enquanto esta em pausa.")]
    [SerializeField] private bool freeCursorWhilePaused = true;

    public static bool IsPaused { get; private set; }

    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private void Start()
    {
        // Garante que comeca escondido e o jogo a correr normalmente.
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        IsPaused = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Le o ESC diretamente do teclado (novo Input System).
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscape();
        }
    }

    private void HandleEscape()
    {
        // Se as opcoes estiverem abertas, o ESC fecha-as primeiro.
        if (IsPaused && optionsPanel != null && optionsPanel.activeSelf)
        {
            CloseOptions();
            return;
        }

        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        if (freeCursorWhilePaused)
        {
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        AudioManager.Instance?.PlayClick();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        if (freeCursorWhilePaused)
        {
            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;
        }

        AudioManager.Instance?.PlayClick();
    }

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        AudioManager.Instance?.PlayClick();
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        AudioManager.Instance?.PlayClick();
    }

    public void BackToMenu()
    {
        // Repoe o tempo antes de trocar de cena, senao a proxima cena fica congelada.
        Time.timeScale = 1f;
        IsPaused = false;
        AudioManager.Instance?.PlayClick();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlayClick();
        Debug.Log("A sair do jogo...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDisable()
    {
        // Seguranca: se este objeto for destruido a meio da pausa, nao deixa o jogo congelado.
        if (IsPaused)
        {
            Time.timeScale = 1f;
            IsPaused = false;
        }
    }
}
