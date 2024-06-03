using System;
using Cysharp.Threading.Tasks;
using FullMoon.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FullMoon.Scene
{
    public class LoadScene : ComponentSingleton<LoadScene>
    {
        public string loadingSceneName = "LoadingScene";

        private string sceneToLoad;

        public void StartLoading(string sceneName)
        {
            sceneToLoad = sceneName;
            LoadLoadingScene().Forget();
        }

        private async UniTaskVoid LoadLoadingScene()
        {
            await SceneManager.LoadSceneAsync(loadingSceneName);
            await LoadSceneAsync();
        }

        private async UniTask LoadSceneAsync()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                ConsoleProDebug.Watch("Loading Progress", progress.ToString("P0"));
                await UniTask.Yield();
            }

            ConsoleProDebug.Watch("Loading Progress", "100%");
            await UniTask.Delay(TimeSpan.FromSeconds(3f));

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                await UniTask.Yield();
            }
        }
    }
}