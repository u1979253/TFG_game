using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ggj25
{
    public class GameManager : Singleton<GameManager>
    {
        [field: SerializeField]
        public GameConfig GameConfig { get; private set; }

        public bool IsGameOver { get; private set; }
        public bool IsInGame { get; private set; }
        public bool IsPlaying => Time.timeScale > 0;
        
        public LevelManager LevelManager { get;  set; }

        private Tween _mainMusicTimer;
        private InputData _input;

        private void Start()
        {
            SoundManager.Instance.StopLoop();
            SoundManager.Instance.PlayLoop(AudioType.Loop.MainTheme);

            _input = new InputData();
            _input.Enable();
        }

        public void ToMainMenu()
        {
            Time.timeScale = 1;
            _mainMusicTimer?.Kill();
            SceneManager.LoadScene(0);
            IsInGame = false;
            
            SoundManager.Instance.StopAllSounds();
            SoundManager.Instance.StopLoop();
            SoundManager.Instance.PlayLoop(AudioType.Loop.MainTheme);
        }

        public void ToGame()
        {
            _mainMusicTimer?.Kill();
            SoundManager.Instance.StopAllSounds();
            SoundManager.Instance.StopLoop();
            SoundManager.Instance.PlaySFX(AudioType.SFX.GameTheme, OnPlayMainTheme);
            
            SceneManager.LoadScene(1);
            SceneManager.sceneLoaded += OnGameSceneLoaded;

            Time.timeScale = 1;
            IsInGame = true;
            IsGameOver = false;
        }

        public void ToProcedural()
        {
            _mainMusicTimer?.Kill();
            SoundManager.Instance.StopAllSounds();
            SoundManager.Instance.StopLoop();
            SoundManager.Instance.PlaySFX(AudioType.SFX.GameTheme, OnPlayMainTheme);

            SceneManager.LoadScene(3);
            SceneManager.sceneLoaded += OnGameSceneLoaded;

            Time.timeScale = 1;
            IsInGame = true;
            IsGameOver = false;
        }

        private void OnPlayMainTheme(AudioSFX sfx, AudioClip clip)
        {
            _mainMusicTimer = DOVirtual.DelayedCall(clip.length, OnFinishLoop);
            //_mainMusicTimer = DOVirtual.DelayedCall(1, OnFinishLoop);
        }

        private void OnFinishLoop()
        {
            SoundManager.Instance.PlaySFX(AudioType.SFX.GameTheme, OnPlayMainTheme);
        }

        private void OnGameSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            LevelManager = new GameObject(nameof(ggj25.LevelManager)).AddComponent<LevelManager>();
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
        }

        public void PauseGame()
        {
            Time.timeScale = 0;
            FindObjectOfType<UIPauseView>(true).Open();
        }

        public void ContinueGame()
        {
            Time.timeScale = 1;
            FindObjectOfType<UIPauseView>().Close();
        }

        public void GameOver(bool isWin)
        {
            Time.timeScale = 0;
            
            DOVirtual.DelayedCall(GameConfig.GameOverDelay, () => CheckGameOver(isWin));
        }

        private void CheckGameOver(bool isWin)
        {
            if (isWin)
            {
                ShowGameOver(true);
                return;
            }
            
            var heroController = GameObject.FindObjectOfType<HeroController>();
            heroController.DeductLives();
            if (heroController.CurrentLives <= 0)
            {
                ShowGameOver(false);
            }
            else
            {
                Respawn(heroController);
            }
        }

        private void Respawn(HeroController heroController)
        {
            Time.timeScale = 1;
            //LevelManager.Rewpawn();
        }

        private void ShowGameOver(bool isWin)
        {
            if (isWin)
            {
                SoundManager.Instance.PlaySFX(AudioType.SFX.GameWon);
            }
            else
            {
                SoundManager.Instance.PlaySFX(AudioType.SFX.GameOver);
            }
            
            IsGameOver = true;
            FindObjectOfType<UIGameOverView>(true).Open(isWin);
            _mainMusicTimer?.Kill();
            SoundManager.Instance.StopLoop();
        }


        public void ExitGame()
        {
            _mainMusicTimer?.Kill();
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void Update()
        {
            if (!IsInGame)
            {
                UpdateInMenu();
                return;
            }

            if (IsPlaying)
            {
                UpdateInGame();
            }
            else
            {
                UpdateInPause();
            }
        }

        private void UpdateInPause()
        {
            if (IsGameOver)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || _input.UI.Exit.inProgress)
                {
                    ToMainMenu();
                }
                else if (_input.UI.Enter.inProgress || _input.UI.Pause.inProgress)
                {
                    ToGame();
                }

                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) || _input.UI.Exit.inProgress)
            {
                ToMainMenu();
            }
            else if (_input.UI.Enter.inProgress)
            {
                ContinueGame();
            }
        }

        private void UpdateInMenu()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitGame();
            }

            if (_input.UI.Pause.inProgress || _input.UI.Enter.inProgress)
            {
                ToGame();
            }
        }

        private void UpdateInGame()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || _input.UI.Pause.inProgress)
            {
                PauseGame();
            }
        }
    }
}