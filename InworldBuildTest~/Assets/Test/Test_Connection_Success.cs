using System.Collections;
using InworldUtils;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;



public class Test_Connection_Success
{
    bool sceneLoaded;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Sample2D", LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator Test_Connection_SuccessWithEnumeratorPasses()
    {
        yield return new WaitWhile(() => sceneLoaded == false);
        var buttonConnectGameObject = GameObject.Find("BtnConnect");
        var connectButton = buttonConnectGameObject.GetComponent<Button>();
        connectButton.onClick.Invoke();

        var statusTextGameObject = GameObject.Find("TxtStatus");
        var statusText = statusTextGameObject.GetComponent<TMP_Text>();

        yield return Wait.Until(() => statusText.text == "Initialized", timeout: 10f);

        //Invoking the Connect Button which has become Load Scene
        connectButton.onClick.Invoke();

        yield return Wait.Until(() => statusText.text == "Connected", timeout: 10f);
        Assert.AreEqual("Connected", statusText.text);


    }
}
