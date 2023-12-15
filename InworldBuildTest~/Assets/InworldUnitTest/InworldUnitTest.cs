using Inworld;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void CreateInworldController()
    {
        Object.Instantiate(InworldAI.ControllerPrefab2D);
        Assert.AreEqual(true, InworldController.Instance != null);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator CreateInworldControllerAsync()
    {
        Object.Instantiate(InworldAI.ControllerPrefab2D);
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
        Assert.AreEqual(true, InworldController.Instance != null);
    }
}
